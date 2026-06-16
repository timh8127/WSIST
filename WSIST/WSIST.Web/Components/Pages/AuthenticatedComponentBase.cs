using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public abstract class AuthenticatedComponentBase(
    TestManagement management,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation
) : ComponentBase
{
    protected int CurrentUserId { get; private set; }

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

        if (email is null)
        {
            // Without an email we can't resolve a user; treat the session as
            // unauthenticated instead of leaving CurrentUserId at 0.
            navigation.NavigateTo("/login-page", forceLoad: true);
            return;
        }

        try
        {
            var dbUser = management.GetOrCreateUser(email, name, googleId);
            CurrentUserId = dbUser.Id;
        }
        catch (Exception)
        {
            // Database unavailable or user resolution failed — send the user
            // to the error page instead of crashing the circuit.
            navigation.NavigateTo("/Error", forceLoad: true);
            return;
        }

        await OnAuthenticatedAsync();
    }

    protected virtual Task OnAuthenticatedAsync() => Task.CompletedTask;

    // Shared grade-to-CSS mapping used by the dashboard and study pages.
    protected static string GetGradeClass(double avg) =>
        avg switch
        {
            >= 5 => "grade-good",
            >= 4 => "grade-ok",
            _ => "grade-poor",
        };
}
