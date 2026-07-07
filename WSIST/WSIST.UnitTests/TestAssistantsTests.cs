using NUnit.Framework;
using WSIST.Engine;

namespace WSIST.UnitTests;

public class TestAssistantsTests
{
    [Test]
    public void CheckIfGradeIsNotNullIfInThePast()
    {
        //arrange
        DateOnly dueDate = new DateOnly(2025, 06, 07);
        var grade = 4.6;

        //act
        var result = TestAssistants.GradeVerifier(dueDate, grade);

        //assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void CheckIfGradeIsNullIfInTheFuture()
    {
        //arrange
        DateOnly dueDate = new DateOnly(2030, 06, 07);
        var grade = 4.6;

        //act
        var result = TestAssistants.GradeVerifier(dueDate, grade);

        //assert
        Assert.That(result, Is.Null);
    }
}
