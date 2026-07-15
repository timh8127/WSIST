using NUnit.Framework;
using WSIST.Engine;

namespace WSIST.UnitTests;

public class PriorityCalculatorTests
{
    [Test]
    public void GradeScoreGivesFullPushBelowAverageOfFour()
    {
        var calculator = new PriorityCalculator();
        var tests = new List<Test>
        {
            new()
            {
                Title = "T",
                Subject = 0,
                Grade = 2.5,
                DueDate = new DateOnly(2026, 01, 10),
            },
        };
        Assert.That(
            calculator.CalculateGradeScore(0, tests),
            Is.EqualTo(6),
            "An average below 4 must earn the full +6 push, not 0."
        );
    }

    [Test]
    public void GradeScoreGivesSmallPushForStrongAverage()
    {
        var calculator = new PriorityCalculator();
        var tests = new List<Test>
        {
            new()
            {
                Title = "T",
                Subject = 0,
                Grade = 5.5,
                DueDate = new DateOnly(2026, 01, 10),
            },
        };
        Assert.That(calculator.CalculateGradeScore(0, tests), Is.EqualTo(2));
    }

    [Test]
    public void UrgencyScoreIsHighestForAShortHorizon()
    {
        var calculator = new PriorityCalculator();
        var asOf = new DateOnly(2026, 01, 10);
        Assert.That(
            calculator.CalculateUrgencyScore(new DateOnly(2026, 01, 11), asOf),
            Is.EqualTo(10)
        );
    }

    [Test]
    public void UrgencyScoreIsModerateForAMidHorizon()
    {
        var calculator = new PriorityCalculator();
        var asOf = new DateOnly(2026, 01, 10);
        Assert.That(
            calculator.CalculateUrgencyScore(new DateOnly(2026, 01, 17), asOf),
            Is.EqualTo(6)
        );
    }

    [Test]
    public void UrgencyScoreIsZeroBeyondThirtyDays()
    {
        var calculator = new PriorityCalculator();
        var asOf = new DateOnly(2026, 01, 10);
        Assert.That(
            calculator.CalculateUrgencyScore(new DateOnly(2026, 02, 24), asOf),
            Is.EqualTo(0)
        );
    }

    [Test]
    public void VolumeScoreIsLowestForVeryLowVolume()
    {
        var calculator = new PriorityCalculator();
        Assert.That(calculator.CalculateVolumeScore(Test.TestVolume.VeryLow), Is.EqualTo(2));
    }

    [Test]
    public void VolumeScoreIsHighestForVeryHighVolume()
    {
        var calculator = new PriorityCalculator();
        Assert.That(calculator.CalculateVolumeScore(Test.TestVolume.VeryHigh), Is.EqualTo(12));
    }

    [Test]
    public void UnderstandingScoreIsHighestWhenUnderstandingIsVeryLow()
    {
        var calculator = new PriorityCalculator();
        Assert.That(
            calculator.CalculateUnderstandingScore(Test.PersonalUnderstanding.VeryLow),
            Is.EqualTo(12),
            "Understanding is inverse: the less understood, the more it must be studied."
        );
    }

    [Test]
    public void UnderstandingScoreIsLowestWhenUnderstandingIsVeryHigh()
    {
        var calculator = new PriorityCalculator();
        Assert.That(
            calculator.CalculateUnderstandingScore(Test.PersonalUnderstanding.VeryHigh),
            Is.EqualTo(2),
            "Understanding is inverse: the better understood, the less it must be studied."
        );
    }
}
