using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Study(TestManagement management, AuthenticationStateProvider authStateProvider, NavigationManager navigation, PriorityCalculator calculator)
    : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private List<Test> allTests = [];
    private List<Test> recommendations = [];
    private double hoursAvailable = 1;
    private bool calculated = false;

    protected override Task OnAuthenticatedAsync()
    {
        allTests = management.LoadAllTests(CurrentUserId);
        return Task.CompletedTask;
    }

    private void Calculate()
    {
        recommendations = calculator.GetStudyRecommendations(allTests, hoursAvailable);
        calculated = true;
    }

    private string GetScoreBreakdown(Test test)
    {
        var urgency = calculator.CalculateUrgencyScore(test.DueDate);
        var volume = calculator.CalculateVolumeScore(test.Volume);
        var understanding = calculator.CalculateUnderstandingScore(test.Understanding);
        var grade = calculator.CalculateGradeScore(test.Subject, allTests);

        return $"Urgency: {urgency}/10 · Volume: {volume}/12 · Understanding: {understanding}/12 · Grade: {grade}/6";
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
            parts.Add($"your average grade in {test.Subject} is {avgGrade:F1}");

        return string.Join(", ", parts);
    }
}