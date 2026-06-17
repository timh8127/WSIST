using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class FeedbackPage(
    TestManagement management,
    FeedbackManagement feedbackManagement,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation,
    IConfiguration configuration
) : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private Feedback.FeedbackCategory category = Feedback.FeedbackCategory.Bug;
    private string message = string.Empty;
    private bool isSubmitting;
    private string? submitError;
    private string? submitMessage;

    private bool isAdmin;
    private List<FeedbackManagement.FeedbackView> allFeedback = [];

    protected override Task OnAuthenticatedAsync()
    {
        // The feedback listing is gated to the configured admin account only.
        // An unset/blank Admin:Email means nobody is admin (safe default).
        var adminEmail = configuration["Admin:Email"];
        isAdmin =
            !string.IsNullOrWhiteSpace(adminEmail)
            && string.Equals(adminEmail, CurrentUserEmail, StringComparison.OrdinalIgnoreCase);

        if (isAdmin)
            allFeedback = feedbackManagement.GetAll();

        return Task.CompletedTask;
    }

    private void SubmitFeedback()
    {
        submitError = null;
        submitMessage = null;

        if (isSubmitting)
            return;
        isSubmitting = true;
        try
        {
            feedbackManagement.Submit(CurrentUserId, message, category);
        }
        catch (ArgumentException ex)
        {
            submitError = ex.Message;
            return;
        }
        finally
        {
            isSubmitting = false;
        }

        message = string.Empty;
        category = Feedback.FeedbackCategory.Bug;
        submitMessage = "Thanks — your feedback was submitted.";

        // Reflect the new row immediately for the admin's own listing.
        if (isAdmin)
            allFeedback = feedbackManagement.GetAll();

        StateHasChanged();
    }
}
