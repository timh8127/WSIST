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
}