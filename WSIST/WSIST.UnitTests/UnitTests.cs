using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WSIST.Engine;

namespace WSIST.UnitTests;

public class UnitTests
{
    private static WsistContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WsistContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WsistContext(options);
    }

    private static User SeedUser(WsistContext context)
    {
        var user = new User
        {
            Email = "test@example.com",
            DisplayName = "Test User",
            GoogleId = "google-123",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    [Test]
    public void TestIfNewTestGetsMade()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act
        manager.NewTestMaker(
            "Math Test",
            Test.Subjects.Math,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.VeryHigh,
            Test.PersonalUnderstanding.VeryLow,
            null,
            user.Id
        );

        //assert
        Assert.That(manager.LoadAllTests(user.Id).Any(t => t.Title == "Math Test"));
    }

    [Test]
    public void CheckIfTestWasDeleted()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        manager.NewTestMaker(
            "Test To Delete",
            Test.Subjects.German,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Low,
            Test.PersonalUnderstanding.High,
            null,
            user.Id
        );

        //act
        var test = manager.LoadAllTests(user.Id).First();
        manager.TestRemover(test.Id);

        //assert
        Assert.That(manager.LoadAllTests(user.Id).Any(t => t.Id == test.Id), Is.False);
    }

    [Test]
    public static void CheckIfGradeIsNotNullIfInThePast()
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
    public static void CheckIfGradeIsNullIfInTheFuture()
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
