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
            0, // was Test.Subjects.Math
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
            3, // was Test.Subjects.German
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

    [Test]
    public void GetUser_ReturnsCorrectUser()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act
        var result = manager.GetUser(user.Id);

        //assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void UpdateDisplayName_ChangesName()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act
        manager.UpdateDisplayName(user.Id, "  New Name  ");

        //assert
        var updated = manager.GetUser(user.Id);
        Assert.That(updated!.DisplayName, Is.EqualTo("New Name"));
    }

    [Test]
    public void AddCustomSubject_AppearsInSubjectList()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act
        manager.AddCustomSubject("Biology", user.Id);

        //assert
        var subjects = manager.GetSubjectsForUser(user.Id);
        Assert.That(subjects.Any(s => s.Name == "Biology" && !s.IsSystem && s.UserId == user.Id));
    }

    [Test]
    public void RemoveCustomSubject_RemovesFromList()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);
        manager.AddCustomSubject("Biology", user.Id);
        var subjectId = manager.GetSubjectsForUser(user.Id).First(s => s.Name == "Biology").Id;

        //act
        manager.RemoveCustomSubject(subjectId, user.Id);

        //assert
        Assert.That(manager.GetSubjectsForUser(user.Id).Any(s => s.Name == "Biology"), Is.False);
    }

    [Test]
    public void GetSubjectsForUser_ReturnsSystemAndOwnSubjectsOnly()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var otherUser = new User
        {
            Email = "other@example.com",
            DisplayName = "Other",
            GoogleId = "google-456",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(otherUser);
        context.SaveChanges();

        // HasData seeding doesn't run for in-memory DB, so seed manually
        context.Subjects.AddRange(
            new Subject
            {
                Id = 0,
                Name = "Math",
                IsSystem = true,
            },
            new Subject
            {
                Id = 1,
                Name = "English",
                IsSystem = true,
            }
        );
        context.SaveChanges();

        var manager = new TestManagement(context);
        manager.AddCustomSubject("Biology", user.Id);
        manager.AddCustomSubject("Physics", otherUser.Id);

        //act
        var subjects = manager.GetSubjectsForUser(user.Id);

        //assert
        Assert.That(subjects.Any(s => s.Name == "Math" && s.IsSystem));
        Assert.That(subjects.Any(s => s.Name == "English" && s.IsSystem));
        Assert.That(subjects.Any(s => s.Name == "Biology" && !s.IsSystem));
        Assert.That(subjects.Any(s => s.Name == "Physics"), Is.False);
    }

    [Test]
    public static void GradeScoreGivesFullPushBelowAverageOfFour()
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
            "An average below 3 must earn the full +6 push, not 0."
        );
    }

    [Test]
    public static void GradeScoreGivesSmallPushForStrongAverage()
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
    public void RemoveCustomSubject_RefusesWhenSubjectStillHasTests()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);
        manager.AddCustomSubject("Biology", user.Id);
        var subjectId = manager.GetSubjectsForUser(user.Id).First(s => s.Name == "Biology").Id;
        manager.NewTestMaker(
            "Biology Test",
            subjectId,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Medium,
            Test.PersonalUnderstanding.Medium,
            null,
            user.Id
        );

        //act
        var removed = manager.RemoveCustomSubject(subjectId, user.Id);

        //assert
        Assert.That(removed, Is.False);
        Assert.That(manager.GetSubjectsForUser(user.Id).Any(s => s.Name == "Biology"));
    }
}
