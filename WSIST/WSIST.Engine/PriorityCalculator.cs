namespace WSIST.Engine;

public class PriorityCalculator
{
    public int CalculateUrgencyScore(DateOnly dueDate)
    {
        var daysUntil = dueDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

        return daysUntil switch
        {
            <= 1 => 10,
            <= 2 => 8,
            <= 7 => 6,
            <= 14 => 4,
            <= 30 => 2,
            _ => 0
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
            _ => 0
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
            _ => 0
        };
    }
    
    public int CalculateGradeScore(Test.Subjects subject, List<Test> allTests)
    {
        var gradedTests = allTests
            .Where(t => t.Subject == subject && t.Grade != null)
            .ToList();

        if (gradedTests.Count == 0)
            return 0;

        var average = gradedTests.Average(t => t.Grade!.Value);

        return average switch
        {
            >= 5 => 2,
            >= 4 => 4,
            >= 3 => 6,
            _ => 0
        };
    }

    public int CalculateTotalScore(Test test, List<Test> allTests)
    {
        var sum = 0;
        sum += CalculateUrgencyScore(test.DueDate);
        sum += CalculateVolumeScore(test.Volume);
        sum += CalculateUnderstandingScore(test.Understanding);
        sum += CalculateGradeScore(test.Subject, allTests);
        return sum;
    }
    
    public List<Test> GetStudyRecommendations(List<Test> allTests, double hoursAvailable)
    {
        var topCount = hoursAvailable switch
        {
            < 1 => 1,
            < 2 => 1,
            < 3 => 3,
            < 4 => 3,
            _ => 5
        };

        return allTests
            .Where(t => t.DueDate >= DateOnly.FromDateTime(DateTime.Today)) // exclude past tests
            .OrderByDescending(t => CalculateTotalScore(t, allTests))
            .ThenBy(t => t.DueDate)
            .Take(topCount)
            .ToList();
    }
}