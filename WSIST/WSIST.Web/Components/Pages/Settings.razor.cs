using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Settings(
    TestManagement management,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation
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
        saveMessage = "Saved.";
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
            subjectError = "Subject name cannot be empty.";
            return;
        }
        catch (SubjectAlreadyExistsException)
        {
            subjectError = "A subject with that name already exists.";
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
            subjectError =
                "Cannot delete a subject that still has tests. Delete or reassign those tests first.";
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
