using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using WSIST.Engine;

namespace WSIST.Web.Components.Pages;

public partial class Study(
    TestManagement management,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigation,
    PriorityCalculator calculator,
    IStringLocalizer<SharedResource> localizer
) : AuthenticatedComponentBase(management, authStateProvider, navigation)
{
    private List<Test> allTests = [];
    private List<Test> recommendations = [];
    private List<Subject> subjects = [];
    private double hoursAvailable = 1;
    private bool calculated = false;
    private Guid? studiedTestId;
    private Test.PersonalUnderstanding updatedUnderstanding;
    private bool weeklyMode = false;
    private Dictionary<DayOfWeek, double> weeklyHours = new()
    {
        { DayOfWeek.Monday, 1 },
        { DayOfWeek.Tuesday, 1 },
        { DayOfWeek.Wednesday, 1 },
        { DayOfWeek.Thursday, 1 },
        { DayOfWeek.Friday, 1 },
        { DayOfWeek.Saturday, 0 },
        { DayOfWeek.Sunday, 0 },
    };
    private Dictionary<DateOnly, List<Test>> weeklyPlan = [];
    private bool weeklyCalculated = false;

    protected override Task OnAuthenticatedAsync()
    {
        allTests = management.LoadAllTests(CurrentUserId);
        subjects = management.GetSubjectsForUser(CurrentUserId);
        return Task.CompletedTask;
    }

    private void Calculate()
    {
        recommendations = calculator.GetStudyRecommendations(allTests, hoursAvailable);
        calculated = true;
    }

    private void CalculateWeekly()
    {
        weeklyPlan = calculator.GetWeeklyPlan(allTests, weeklyHours);
        weeklyCalculated = true;
    }

    private string DayLabel(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return date == today ? localizer["Study_Today"] : date.ToString("ddd d MMM");
    }

    private void OpenStudiedPrompt(Test test)
    {
        studiedTestId = test.Id;
        updatedUnderstanding = test.Understanding;
    }

    private void CancelStudiedPrompt()
    {
        studiedTestId = null;
    }

    private void SaveStudiedUnderstanding(Test test)
    {
        management.TestEditor(
            test.Id,
            test.Title,
            test.Subject,
            test.DueDate,
            test.Volume,
            updatedUnderstanding,
            test.Grade,
            CurrentUserId
        );

        allTests = management.LoadAllTests(CurrentUserId);
        recommendations = calculator.GetStudyRecommendations(allTests, hoursAvailable);
        if (weeklyCalculated)
            weeklyPlan = calculator.GetWeeklyPlan(allTests, weeklyHours);
        studiedTestId = null;
        StateHasChanged();
    }

    private string GetScoreBreakdown(Test test)
    {
        var urgency = calculator.CalculateUrgencyScore(test.DueDate);
        var volume = calculator.CalculateVolumeScore(test.Volume);
        var understanding = calculator.CalculateUnderstandingScore(test.Understanding);
        var grade = calculator.CalculateGradeScore(test.Subject, allTests);

        return $"{localizer["Score_Urgency"]}: {urgency}/10 · {localizer["Score_Volume"]}: {volume}/12 · {localizer["Score_Understanding"]}: {understanding}/12 · {localizer["Score_Grade"]}: {grade}/6";
    }

    private string GetBecauseText(Test test)
    {
        var days = test.DueDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
        var avgGrade = allTests
            .Where(t => t.Subject == test.Subject && t.Grade != null)
            .Select(t => t.Grade!.Value)
            .DefaultIfEmpty(0)
            .Average();

        var subjectName =
            subjects.FirstOrDefault(s => s.Id == test.Subject)?.Name ?? test.Subject.ToString();

        var understandingWord = localizer[$"Level_{test.Understanding}"].Value.ToLowerInvariant();
        var volumeWord = localizer[$"Level_{test.Volume}"].Value.ToLowerInvariant();

        var parts = new List<string>
        {
            localizer["Because_Understanding", understandingWord],
            localizer["Because_Volume", volumeWord],
            days == 1 ? localizer["Because_InOneDay"] : localizer["Because_InDaysFmt", days],
        };

        // The average grade line represents the grade pull scoring factor.
        // When avgGrade > 0, grade pull contributes +2/+4/+6 to the total score.
        if (avgGrade > 0)
            parts.Add(localizer["Because_AvgGradeFmt", subjectName, avgGrade.ToString("F1")]);

        return string.Join(", ", parts);
    }
}
