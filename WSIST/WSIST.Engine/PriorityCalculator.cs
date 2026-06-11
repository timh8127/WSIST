namespace WSIST.Engine;

public class PriorityCalculator
{
    public int CalculateUrgencyScore(DateOnly dueDate, DateOnly? asOfDate = null)
    {
        var reference = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var daysUntil = dueDate.DayNumber - reference.DayNumber;

        return daysUntil switch
        {
            <= 1 => 10,
            <= 2 => 8,
            <= 7 => 6,
            <= 14 => 4,
            <= 30 => 2,
            _ => 0,
        };
    }

    public int CalculateVolumeScore(Test.TestVolume volume)
    {
        return volume switch
        {
            Test.TestVolume.VeryLow => 2,
            Test.TestVolume.Low => 4,
            Test.TestVolume.Medium => 6,
            Test.TestVolume.Average => 8,
            Test.TestVolume.High => 10,
            Test.TestVolume.VeryHigh => 12,
            _ => 0,
        };
    }

    public int CalculateUnderstandingScore(Test.PersonalUnderstanding understanding)
    {
        return understanding switch
        {
            Test.PersonalUnderstanding.VeryLow => 12,
            Test.PersonalUnderstanding.Low => 10,
            Test.PersonalUnderstanding.Medium => 8,
            Test.PersonalUnderstanding.Average => 6,
            Test.PersonalUnderstanding.High => 4,
            Test.PersonalUnderstanding.VeryHigh => 2,
            _ => 0,
        };
    }

    public int CalculateGradeScore(int subjectId, List<Test> allTests)
    {
        var gradedTests = allTests.Where(t => t.Subject == subjectId && t.Grade != null).ToList();

        if (gradedTests.Count == 0)
            return 0;

        var average = gradedTests.Average(t => t.Grade!.Value);

        return average switch
        {
            >= 5 => 2,
            >= 4 => 4,
            _ => 6,
        };
    }

    public int CalculateTotalScore(Test test, List<Test> allTests, DateOnly? asOfDate = null)
    {
        var sum = 0;
        sum += CalculateUrgencyScore(test.DueDate, asOfDate);
        sum += CalculateVolumeScore(test.Volume);
        sum += CalculateUnderstandingScore(test.Understanding);
        sum += CalculateGradeScore(test.Subject, allTests);
        return sum;
    }

    public List<Test> GetStudyRecommendations(List<Test> allTests, double hoursAvailable, DateOnly? asOfDate = null)
    {
        var reference = asOfDate ?? DateOnly.FromDateTime(DateTime.Today);
        var topCount = hoursAvailable switch
        {
            < 1 => 1,
            < 2 => 1,
            < 3 => 3,
            < 4 => 3,
            _ => 5,
        };

        return allTests
            .Where(t => t.DueDate >= reference) // exclude tests already due by the reference date
            .OrderByDescending(t => CalculateTotalScore(t, allTests, reference))
            .ThenBy(t => t.DueDate)
            .Take(topCount)
            .ToList();
    }

    // Plan the coming 7 days. A test repeating on several days is intentional —
    // it has consistently high priority throughout the week and deserves repeated attention.
    public Dictionary<DateOnly, List<Test>> GetWeeklyPlan(
        List<Test> allTests,
        Dictionary<DayOfWeek, double> hoursPerDay)
    {
        var plan = new Dictionary<DateOnly, List<Test>>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(i);
            var hours = hoursPerDay.GetValueOrDefault(date.DayOfWeek, 0);

            plan[date] = hours > 0
                ? GetStudyRecommendations(allTests, hours, date)
                : [];
        }

        return plan;
    }
}
