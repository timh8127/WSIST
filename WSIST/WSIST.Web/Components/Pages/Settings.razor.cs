using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Settings(
    TestManagement management,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation)
    : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private User? currentUser;
    private List<Subject> subjects = [];
    private string editedDisplayName = string.Empty;
    private string newSubjectName = string.Empty;
    private string? saveMessage;
    private string? subjectError;

    protected override Task OnAuthenticatedAsync()
    {
        currentUser = management.GetUser(CurrentUserId);
        subjects = management.GetSubjectsForUser(CurrentUserId);
        editedDisplayName = currentUser?.DisplayName ?? string.Empty;
        return Task.CompletedTask;
    }

    private void SaveDisplayName()
    {
        if (string.IsNullOrWhiteSpace(editedDisplayName)) return;
        management.UpdateDisplayName(CurrentUserId, editedDisplayName);
        currentUser = management.GetUser(CurrentUserId);
        saveMessage = "Saved.";
        StateHasChanged();
    }

    private void AddSubject()
    {
        subjectError = null;
        var trimmed = newSubjectName.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            subjectError = "Subject name cannot be empty.";
            return;
        }

        if (subjects.Any(s => s.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            subjectError = "A subject with that name already exists.";
            return;
        }

        management.AddCustomSubject(trimmed, CurrentUserId);
        newSubjectName = string.Empty;
        subjects = management.GetSubjectsForUser(CurrentUserId);
        StateHasChanged();
    }

    private void DeleteSubject(int subjectId)
    {
        management.RemoveCustomSubject(subjectId, CurrentUserId);
        subjects = management.GetSubjectsForUser(CurrentUserId);
        StateHasChanged();
    }
}
