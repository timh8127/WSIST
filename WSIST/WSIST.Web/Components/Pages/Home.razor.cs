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
    private List<Subject> subjects = [];
    private Test? temporaryTest;
    private Test? topRecommendation;
    private Modes Mode { get; set; }
    private bool showPastTests;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    // Past tests that still have no grade entered, oldest first.
    private List<Test> MissingGrades =>
        allTests
            .Where(t => t.DueDate < DateOnly.FromDateTime(DateTime.Today) && t.Grade == null)
            .OrderBy(t => t.DueDate)
            .ToList();

    protected override Task OnAuthenticatedAsync()
    {
        Refresh();
        return Task.CompletedTask;
    }

    // Show or hide past tests in the main table.
    private void TogglePastTests()
    {
        showPastTests = !showPastTests;
        Refresh();
    }

    // Average grade per subject across every graded test (only past tests can be graded).
    private Dictionary<int, double> GetSubjectAverages()
    {
        return allTests
            .Where(t => t.Grade.HasValue)
            .GroupBy(t => t.Subject)
            .ToDictionary(
                g => g.Key,
                g => g.Average(t => t.Grade!.Value)
            );
    }

    // Chronological grade history per subject, only for subjects with 2+ graded tests.
    private Dictionary<int, List<(DateOnly Date, double Grade)>> GetGradeHistory()
    {
        return allTests
            .Where(t => t.Grade.HasValue)
            .GroupBy(t => t.Subject)
            .Where(g => g.Count() >= 2)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(t => t.DueDate)
                      .Select(t => (t.DueDate, t.Grade!.Value))
                      .ToList()
            );
    }

    // Map a grade (1–6) to an SVG Y coordinate — higher grade = lower Y (top of SVG).
    private static string MapGradeToY(double grade, int height, int padding) =>
        ((height - padding) - ((grade - 1) / 5.0 * (height - padding * 2))).ToString("F1");

    // Map a point index to an SVG X coordinate, spreading points across the width.
    private static string MapIndexToX(int index, int totalPoints, int width, int padding) =>
        (padding + index * (double)(width - padding * 2) / (totalPoints - 1)).ToString("F1");

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
                    temporaryTest.Grade,
                    CurrentUserId
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
        tests = (showPastTests
                ? allTests
                : allTests.Where(t => t.DueDate >= Today))
            .OrderBy(t => t.DueDate)
            .ToList();
        topRecommendation = calculator.GetStudyRecommendations(allTests, 2).FirstOrDefault();
        subjects = management.GetSubjectsForUser(CurrentUserId);
        StateHasChanged();
    }

    private void DeleteTest(Guid id)
    {
        management.TestRemover(id, CurrentUserId);
        Refresh();
    }
}
