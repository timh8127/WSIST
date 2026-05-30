using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Home(TestManagement management, AuthenticationStateProvider authStateProvider, NavigationManager navigation)
    : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private List<Test> tests = [];
    private Test? temporaryTest;
    private Modes Mode { get; set; }
    private bool showPastCompleted;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    // Upcoming: due today or later.
    private IEnumerable<Test> UpcomingTests => tests.Where(t => t.DueDate >= Today);

    // Past but missing a grade — needs the user's attention to be completed.
    private IEnumerable<Test> PastUngradedTests => tests.Where(t => t.DueDate < Today && t.Grade is null);

    // Past and graded — hidden by default, shown when the user wants to review performance.
    private IEnumerable<Test> PastCompletedTests => tests.Where(t => t.DueDate < Today && t.Grade is not null);

    protected override Task OnAuthenticatedAsync()
    {
        LoadTests();
        return Task.CompletedTask;
    }

    private void LoadTests()
    {
        tests = management.LoadAllTests(CurrentUserId);
    }

    private void TogglePastCompleted()
    {
        showPastCompleted = !showPastCompleted;
    }

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
        LoadTests();
        StateHasChanged();
    }

    private void DeleteTest(Guid id)
    {
        management.TestRemover(id);
        Refresh();
    }
}
