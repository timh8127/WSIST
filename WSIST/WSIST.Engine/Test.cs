namespace WSIST.Engine;

public class Test
{
    public Guid Id { get; init; }
    public required string Title { get; set; }
    public Subjects Subject { get; set; }
    public DateOnly DueDate { get; set; }
    public TestVolume Volume { get; set; }
    public PersonalUnderstanding Understanding { get; set; }
    public double? Grade { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public enum Subjects
    {
        Math,
        English,
        French,
        German,
        Chemistry,
        Other,
    }

    public enum TestVolume
    {
        VeryLow,
        Low,
        Medium,
        Average,
        High,
        VeryHigh,
    }

    public enum PersonalUnderstanding
    {
        VeryLow = 0,
        Low = 1,
        Medium = 2,
        Average = 3,
        High = 4,
        VeryHigh = 5,
    }

    public static string VolumeHelper(TestVolume volume)
    {
        switch (volume)
        {
            case TestVolume.VeryLow:
            {
                return "Very Low";
            }
            case TestVolume.Low:
            {
                return "Low";
            }
            case TestVolume.Medium:
            {
                return "Medium";
            }
            case TestVolume.Average:
            {
                return "Average";
            }
            case TestVolume.High:
            {
                return "High";
            }
            case TestVolume.VeryHigh:
            {
                return "Very High";
            }
        }

        return "Please Choose a Setting";
    }

    public static string UnderstandingHelper(PersonalUnderstanding volume)
    {
        switch (volume)
        {
            case PersonalUnderstanding.VeryLow:
            {
                return "Very Low";
            }
            case PersonalUnderstanding.Low:
            {
                return "Low";
            }
            case PersonalUnderstanding.Medium:
            {
                return "Medium";
            }
            case PersonalUnderstanding.Average:
            {
                return "Average";
            }
            case PersonalUnderstanding.High:
            {
                return "High";
            }
            case PersonalUnderstanding.VeryHigh:
            {
                return "Very High";
            }
        }

        return "Please Choose a Setting";
    }
}
