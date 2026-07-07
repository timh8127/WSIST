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
}
