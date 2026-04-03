using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Study(TestManagement management, AuthenticationStateProvider authStateProvider, NavigationManager navigation)
{
    private List<Test> allTests = [];
    private List<Test> recommendations = [];
    private double hoursAvailable = 1;
    private bool calculated = false;
    private int currentUserId;
    private PriorityCalculator calculator = new();

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
        allTests = management.LoadAllTests(currentUserId);
    }

    private void Calculate()
    {
        recommendations = calculator.GetStudyRecommendations(allTests, hoursAvailable);
        calculated = true;
    }

    private string GetBecauseText(Test test)
    {
        var days = test.DueDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
        var avgGrade = allTests
            .Where(t => t.Subject == test.Subject && t.Grade != null)
            .Select(t => t.Grade!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var parts = new List<string>
        {
            $"you have {Test.UnderstandingHelper(test.Understanding).ToLower()} understanding",
            $"the test has {Test.VolumeHelper(test.Volume).ToLower()} volume",
            $"it's in {days} day{(days == 1 ? "" : "s")}",
        };

        if (avgGrade > 0)
            parts.Add($"your avg grade in {test.Subject} is {avgGrade:F1}");

        return string.Join(", ", parts);
    }
}