using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Settings(
    TestManagement management,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation,
    IStringLocalizer<SharedResource> localizer
) : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private User? currentUser;
    private List<Subject> subjects = [];
    private string editedDisplayName = string.Empty;
    private string newSubjectName = string.Empty;
    private string? saveMessage;
    private string? subjectError;
    private List<Test> allTests = [];
    private bool showDeleteConfirm = false;
    private bool isDeleting = false;

    protected override Task OnAuthenticatedAsync()
    {
        currentUser = management.GetUser(CurrentUserId);
        subjects = management.GetSubjectsForUser(CurrentUserId);
        allTests = management.LoadAllTests(CurrentUserId);
        editedDisplayName = currentUser?.DisplayName ?? string.Empty;
        return Task.CompletedTask;
    }

    private void SaveDisplayName()
    {
        if (string.IsNullOrWhiteSpace(editedDisplayName))
            return;
        management.UpdateDisplayName(CurrentUserId, editedDisplayName);
        currentUser = management.GetUser(CurrentUserId);
        saveMessage = localizer["Settings_Saved"];
        StateHasChanged();
    }

    private void AddSubject()
    {
        subjectError = null;

        // The engine owns the empty/duplicate rules so they stay identical to
        // the inline creation flow in the test modal; just map them to messages.
        try
        {
            management.AddCustomSubject(newSubjectName, CurrentUserId);
        }
        catch (ArgumentException)
        {
            subjectError = localizer["Subject_EmptyError"];
            return;
        }
        catch (SubjectAlreadyExistsException)
        {
            subjectError = localizer["Subject_DuplicateError"];
            return;
        }

        newSubjectName = string.Empty;
        subjects = management.GetSubjectsForUser(CurrentUserId);
        StateHasChanged();
    }

    private void DeleteSubject(int subjectId)
    {
        subjectError = null;
        if (!management.RemoveCustomSubject(subjectId, CurrentUserId))
        {
            subjectError = localizer["Settings_SubjectInUseError"];
            StateHasChanged();
            return;
        }
        subjects = management.GetSubjectsForUser(CurrentUserId);
        StateHasChanged();
    }

    private void ShowDeleteConfirm() => showDeleteConfirm = true;

    private void CancelDelete() => showDeleteConfirm = false;

    private void ConfirmDeleteAccount()
    {
        isDeleting = true;
        management.DeleteUser(CurrentUserId);
        navigation.NavigateTo("/logout", forceLoad: true);
    }
}
