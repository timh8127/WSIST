using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class FeedbackPage(
    TestManagement management,
    FeedbackManagement feedbackManagement,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation,
    IConfiguration configuration,
    IStringLocalizer<SharedResource> localizer
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
        catch (ArgumentException)
        {
            // An empty message is the common case (length is capped client-side
            // and the category comes from a fixed dropdown); map it to a clear
            // localized message and fall back to a generic one for any other
            // engine validation failure rather than surfacing English text.
            submitError = string.IsNullOrWhiteSpace(message)
                ? localizer["Feedback_EmptyError"]
                : localizer["Feedback_SubmitValidationError"];
            return;
        }
        finally
        {
            isSubmitting = false;
        }

        message = string.Empty;
        category = Feedback.FeedbackCategory.Bug;
        submitMessage = localizer["Feedback_Thanks"];

        // Reflect the new row immediately for the admin's own listing.
        if (isAdmin)
            allFeedback = feedbackManagement.GetAll();

        StateHasChanged();
    }

    // Admin-only: change a submission's status from the listing.
    private void ChangeStatus(int feedbackId, ChangeEventArgs e)
    {
        if (!isAdmin)
            return;
        if (Enum.TryParse<Feedback.FeedbackStatus>(e.Value?.ToString(), out var status))
        {
            feedbackManagement.UpdateStatus(feedbackId, status);
            allFeedback = feedbackManagement.GetAll();
            StateHasChanged();
        }
    }
}
