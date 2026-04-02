using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Home(TestManagement management, AuthenticationStateProvider authStateProvider, NavigationManager navigation)
{
    private List<Test> tests = [];
    private Test? temporaryTest;
    private Modes Mode { get; set; }
    private int currentUserId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            navigation.NavigateTo("/login-page", forceLoad: true);
            return;
        }

        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var name = user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        var googleId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        if (email is null) return;

        var dbUser = management.GetOrCreateUser(email, name, googleId);
        currentUserId = dbUser.Id;
        tests = management.LoadAllTests(currentUserId);
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
                    currentUserId
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
        tests = management.LoadAllTests(currentUserId);
        StateHasChanged();
    }

    private void DeleteTest(Guid id)
    {
        management.TestRemover(id);
        Refresh();
    }
}
