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
            // The only ArgumentException reachable from the UI is an empty
            // message (length is capped client-side); map it to a localized
            // message rather than surfacing the engine's English text.
            submitError = localizer["Feedback_EmptyError"];
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
}
