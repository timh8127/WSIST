using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Home(
    TestManagement management,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation,
    PriorityCalculator calculator)
    : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private List<Test> allTests = [];
    private List<Test> tests = [];
    private Test? temporaryTest;
    private Test? topRecommendation;
    private Modes Mode { get; set; }
    private bool showPastCompleted;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    // Upcoming: due today or later (tests is already filtered).
    private IEnumerable<Test> UpcomingTests => tests;

    // Past but missing a grade — needs the user's attention to be completed.
    private IEnumerable<Test> PastUngradedTests => allTests.Where(t => t.DueDate < Today && t.Grade is null);

    // Past and graded — hidden by default, shown when the user wants to review performance.
    private IEnumerable<Test> PastCompletedTests => allTests.Where(t => t.DueDate < Today && t.Grade is not null);

    protected override Task OnAuthenticatedAsync()
    {
        allTests = management.LoadAllTests(CurrentUserId);
        tests = allTests
            .Where(t => t.DueDate >= DateOnly.FromDateTime(DateTime.Today))
            .ToList();
        topRecommendation = calculator.GetStudyRecommendations(allTests, 2).FirstOrDefault();
        return Task.CompletedTask;
    }

    private void TogglePastCompleted()
    {
        showPastCompleted = !showPastCompleted;
    }

    // Average grade per subject across every graded test (only past tests can be graded).
    private Dictionary<Test.Subjects, double> GetSubjectAverages()
    {
        return allTests
            .Where(t => t.Grade.HasValue)
            .GroupBy(t => t.Subject)
            .ToDictionary(
                g => g.Key,
                g => g.Average(t => t.Grade!.Value)
            );
    }

    private string GetGradeClass(double avg) => avg switch
    {
        >= 5 => "grade-good",
        >= 4 => "grade-ok",
        _ => "grade-poor"
    };

    public enum Modes
    {
        AddTest,
        EditTest,
    }

    //Modal

    private bool showModal;

    private void OpenEditTestModal(Test test)
    {
        Mode = Modes.EditTest;

        temporaryTest = new Test
        {
            Id = test.Id,
            Title = test.Title,
            Subject = test.Subject,
            DueDate = test.DueDate,
            Volume = test.Volume,
            Understanding = test.Understanding,
            Grade = test.Grade,
        };
        showModal = true;
    }

    public void OpenAddTestModal()
    {
        temporaryTest = new Test { Title = "Some Test" };
        Mode = Modes.AddTest;
        temporaryTest.DueDate = DateOnly.FromDateTime(DateTime.Today);
        showModal = true;
    }

    private void ModalSubmit()
    {
        if (temporaryTest is null)
            return;
        switch (Mode)
        {
            case Modes.AddTest:
            {
                management.NewTestMaker(
                    temporaryTest.Title,
                    temporaryTest.Subject,
                    temporaryTest.DueDate,
                    temporaryTest.Volume,
                    temporaryTest.Understanding,
                    temporaryTest.Grade,
                    CurrentUserId
                );
                break;
            }
            case Modes.EditTest:
            {
                management.TestEditor(
                    temporaryTest.Id,
                    temporaryTest.Title,
                    temporaryTest.Subject,
                    temporaryTest.DueDate,
                    temporaryTest.Volume,
                    temporaryTest.Understanding,
                    temporaryTest.Grade
                );
                break;
            }
        }
        CloseModal();
        Refresh();
    }

    private void CloseModal()
    {
        showModal = false;
        Refresh();
    }

    private void Refresh()
    {
        allTests = management.LoadAllTests(CurrentUserId);
        tests = allTests
            .Where(t => t.DueDate >= DateOnly.FromDateTime(DateTime.Today))
            .ToList();
        topRecommendation = calculator.GetStudyRecommendations(allTests, 2).FirstOrDefault();
        StateHasChanged();
    }

    private void DeleteTest(Guid id)
    {
        management.TestRemover(id);
        Refresh();
    }
}
