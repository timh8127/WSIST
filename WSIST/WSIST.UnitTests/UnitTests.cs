using NUnit.Framework;
using WSIST.Engine;

namespace WSIST.UnitTests;

public class UnitTests
{
    /*TODO: COMPLETE OVERHAUL
        - Rewrite All Test to now work with Database.
        - Clean out Tests that arent needed
        - add checks future checks
    */
    [Test]
    public void TestIfNewTestGetsMade()
    {
        //arrange
        var manager = new TestManagement();
        //act
        manager.NewTestMaker(
            "Math Tets",
            Test.Subjects.Math,
            new DateOnly(2026, 02, 04),
            Test.TestVolume.VeryHigh,
            Test.PersonalUnderstanding.VeryLow,
            4.5,
            1
        );

        //assert
        Assert.That(manager.Tests.Any(t => t.Title == "Math Tets"));
    }

    [Test]
    public void CheckIfTestWasDeleted()
    {
        //arrange
        var manager = new TestManagement();
        Guid id = new Guid("3a5cd0af-7c40-425c-8235-c47b5b9596ec");
        //act
        manager.TestRemover(id);
        Assert.That(
            manager.Tests.Any(t => t.Id == id),
            Is.False,
            "The Test with the given ID should no longer exist"
        );
    }

    [Test]
    public static void CheckIfGradeIsNotNullIfInThePast()
    {
        //arrange
        DateOnly DueDate = new DateOnly(2025, 06, 07);
        var grade = 4.6;
        //act
        var result = TestAssistants.GradeVerifier(DueDate, grade);
        //Assert
        Assert.That(
            result,
            Is.Not.Null,
            "Its Possible for a test to have a grade if the test was in the Past"
        );
    }

    [Test]
    public static void CheckIfGradeIsNullIfInTheFuture()
    {
        //arrange
        DateOnly DueDate = new DateOnly(2030, 06, 07);
        var grade = 4.6;
        //act
        var result = TestAssistants.GradeVerifier(DueDate, grade);
        //Assert
        Assert.That(result, Is.Null, "Date is in the future so Test Cant Have Grade Yet");
    }
}
