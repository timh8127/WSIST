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

    private static int SeedSystemSubject(WsistContext context, string name = "Math")
    {
        // HasData seeding doesn't run for the in-memory provider, and the
        // engine now validates that a test's subject exists and is accessible,
        // so tests must seed a subject explicitly. System subjects live on
        // negative ids (see WsistContext).
        var subject = new Subject
        {
            Id = -6,
            Name = name,
            IsSystem = true,
        };
        context.Subjects.Add(subject);
        context.SaveChanges();
        return subject.Id;
    }

    [Test]
    public void TestIfNewTestGetsMade()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var subjectId = SeedSystemSubject(context);
        var manager = new TestManagement(context);

        //act
        manager.NewTestMaker(
            "Math Test",
            subjectId,
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
        var subjectId = SeedSystemSubject(context, "German");
        var manager = new TestManagement(context);

        manager.NewTestMaker(
            "Test To Delete",
            subjectId,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Low,
            Test.PersonalUnderstanding.High,
            null,
            user.Id
        );

        //act
        var test = manager.LoadAllTests(user.Id).First();
        manager.TestRemover(test.Id, user.Id);

        //assert
        Assert.That(manager.LoadAllTests(user.Id).Any(t => t.Id == test.Id), Is.False);
    }

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

        // HasData seeding doesn't run for in-memory DB, so seed manually.
        // System subjects live on negative ids (see WsistContext) so they can
        // never collide with the provider-generated ids of custom subjects.
        context.Subjects.AddRange(
            new Subject
            {
                Id = -6,
                Name = "Math",
                IsSystem = true,
            },
            new Subject
            {
                Id = -5,
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
    public void TestRemover_RefusesToDeleteAnotherUsersTest()
    {
        //arrange
        using var context = CreateContext();
        var owner = SeedUser(context);
        var attacker = new User
        {
            Email = "attacker@example.com",
            DisplayName = "Attacker",
            GoogleId = "google-999",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(attacker);
        context.SaveChanges();
        var subjectId = SeedSystemSubject(context);

        var manager = new TestManagement(context);
        manager.NewTestMaker(
            "Owner's Test",
            subjectId,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Medium,
            Test.PersonalUnderstanding.Medium,
            null,
            owner.Id
        );
        var test = manager.LoadAllTests(owner.Id).First();

        //act — attacker tries to delete the owner's test
        manager.TestRemover(test.Id, attacker.Id);

        //assert
        Assert.That(manager.LoadAllTests(owner.Id).Any(t => t.Id == test.Id));
    }

    [Test]
    public void TestEditor_RefusesToEditAnotherUsersTest()
    {
        //arrange
        using var context = CreateContext();
        var owner = SeedUser(context);
        var attacker = new User
        {
            Email = "attacker@example.com",
            DisplayName = "Attacker",
            GoogleId = "google-999",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(attacker);
        context.SaveChanges();
        var subjectId = SeedSystemSubject(context);

        var manager = new TestManagement(context);
        manager.NewTestMaker(
            "Original Title",
            subjectId,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Medium,
            Test.PersonalUnderstanding.Medium,
            null,
            owner.Id
        );
        var test = manager.LoadAllTests(owner.Id).First();

        //act — attacker tries to edit the owner's test
        manager.TestEditor(
            test.Id,
            "Hijacked Title",
            test.Subject,
            test.DueDate,
            test.Volume,
            test.Understanding,
            test.Grade,
            attacker.Id
        );

        //assert
        Assert.That(manager.LoadAllTests(owner.Id).First().Title, Is.EqualTo("Original Title"));
    }

    [Test]
    public void NewTestMaker_RejectsEmptyTitle()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act + assert
        Assert.Throws<ArgumentException>(() =>
            manager.NewTestMaker(
                "   ",
                0,
                new DateOnly(2026, 12, 01),
                Test.TestVolume.Medium,
                Test.PersonalUnderstanding.Medium,
                null,
                user.Id
            )
        );
    }

    [Test]
    public void NewTestMaker_RejectsAnotherUsersSubject()
    {
        //arrange
        using var context = CreateContext();
        var owner = SeedUser(context);
        var attacker = new User
        {
            Email = "attacker@example.com",
            DisplayName = "Attacker",
            GoogleId = "google-999",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(attacker);
        context.SaveChanges();

        var manager = new TestManagement(context);
        manager.AddCustomSubject("Owner's Subject", owner.Id);
        var subjectId = manager.GetSubjectsForUser(owner.Id).First(s => !s.IsSystem).Id;

        //act + assert — attacker may not file tests under the owner's subject
        Assert.Throws<ArgumentException>(() =>
            manager.NewTestMaker(
                "Sneaky Test",
                subjectId,
                new DateOnly(2026, 12, 01),
                Test.TestVolume.Medium,
                Test.PersonalUnderstanding.Medium,
                null,
                attacker.Id
            )
        );
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

    [Test]
    public void AddCustomSubject_ReturnsCreatedSubject()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act
        var created = manager.AddCustomSubject("Biology", user.Id);

        //assert — the caller (test modal) needs the new id to auto-select it
        Assert.That(created.Name, Is.EqualTo("Biology"));
        Assert.That(created.IsSystem, Is.False);
        Assert.That(created.UserId, Is.EqualTo(user.Id));
        Assert.That(created.Id, Is.GreaterThan(0));
    }

    [Test]
    public void AddCustomSubject_TrimsWhitespace()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act
        var created = manager.AddCustomSubject("  Biology  ", user.Id);

        //assert
        Assert.That(created.Name, Is.EqualTo("Biology"));
    }

    [Test]
    public void AddCustomSubject_RejectsWhitespaceOnlyName()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);

        //act + assert
        Assert.Throws<ArgumentException>(() => manager.AddCustomSubject("   ", user.Id));
    }

    [Test]
    public void AddCustomSubject_RejectsDuplicateNameCaseInsensitive()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var manager = new TestManagement(context);
        manager.AddCustomSubject("Biology", user.Id);

        //act + assert — the duplicate rule lives in the engine, so it holds
        //regardless of which UI path (Settings or test modal) calls it
        Assert.Throws<SubjectAlreadyExistsException>(() =>
            manager.AddCustomSubject("biology", user.Id)
        );
    }

    [Test]
    public void AddCustomSubject_RejectsDuplicateOfSystemSubject()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        SeedSystemSubject(context, "Math");
        var manager = new TestManagement(context);

        //act + assert
        Assert.Throws<SubjectAlreadyExistsException>(() =>
            manager.AddCustomSubject("math", user.Id)
        );
    }

    [Test]
    public void AddCustomSubject_AllowsSameNameForDifferentUsers()
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
        var manager = new TestManagement(context);
        manager.AddCustomSubject("Biology", user.Id);

        //act — a different user's identical name is not a duplicate
        var created = manager.AddCustomSubject("Biology", otherUser.Id);

        //assert
        Assert.That(created.UserId, Is.EqualTo(otherUser.Id));
    }

    [Test]
    public void GetTestExport_OnlyIncludesRequestingUsersTests()
    {
        //arrange
        using var context = CreateContext();
        var owner = SeedUser(context);
        var other = new User
        {
            Email = "other@example.com",
            DisplayName = "Other",
            GoogleId = "google-456",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(other);
        context.SaveChanges();
        var subjectId = SeedSystemSubject(context);
        var manager = new TestManagement(context);
        manager.NewTestMaker(
            "Owner Test",
            subjectId,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Medium,
            Test.PersonalUnderstanding.Medium,
            null,
            owner.Id
        );
        manager.NewTestMaker(
            "Other Test",
            subjectId,
            new DateOnly(2026, 12, 01),
            Test.TestVolume.Medium,
            Test.PersonalUnderstanding.Medium,
            null,
            other.Id
        );

        //act
        var export = manager.GetTestExport(owner.Id);

        //assert — the export is strictly scoped to the requesting user
        Assert.That(export.Any(r => r.Title == "Owner Test"));
        Assert.That(export.Any(r => r.Title == "Other Test"), Is.False);
    }

    [Test]
    public void GetTestExport_IncludesPastGradedTestsWithResolvedSubjectName()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var subjectId = SeedSystemSubject(context, "Math");
        var manager = new TestManagement(context);
        manager.NewTestMaker(
            "Past Exam",
            subjectId,
            new DateOnly(2025, 01, 01),
            Test.TestVolume.High,
            Test.PersonalUnderstanding.Low,
            5.5,
            user.Id
        );

        //act
        var export = manager.GetTestExport(user.Id);

        //assert
        var row = export.Single(r => r.Title == "Past Exam");
        Assert.That(row.Subject, Is.EqualTo("Math"));
        Assert.That(row.Grade, Is.EqualTo(5.5));
        Assert.That(row.Volume, Is.EqualTo("High"));
    }

    [Test]
    public void ToCsv_WritesHeaderAndFormatsValues()
    {
        var rows = new List<TestExportRow>
        {
            new("Exam", "Math", new DateOnly(2026, 03, 01), "High", "Low", 5.5),
        };

        var csv = TestExporter.ToCsv(rows);

        Assert.That(csv, Does.StartWith("Title,Subject,DueDate,Volume,Understanding,Grade"));
        Assert.That(csv, Does.Contain("2026-03-01"));
        Assert.That(csv, Does.Contain("5.5"));
    }

    [Test]
    public void ToCsv_EscapesDelimitersAndQuotes()
    {
        var rows = new List<TestExportRow>
        {
            new("Title, comma", "Sub \"quote\"", new DateOnly(2026, 03, 01), "High", "Low", null),
        };

        var csv = TestExporter.ToCsv(rows);

        Assert.That(csv, Does.Contain("\"Title, comma\""));
        Assert.That(csv, Does.Contain("\"Sub \"\"quote\"\"\""));
    }

    [Test]
    public void ToCsv_MitigatesFormulaInjection()
    {
        var rows = new List<TestExportRow>
        {
            new("=SUM(A1:A2)", "Math", new DateOnly(2026, 03, 01), "High", "Low", null),
        };

        var csv = TestExporter.ToCsv(rows);

        //a leading '=' must be neutralised so spreadsheets treat it as text
        Assert.That(csv, Does.Contain("'=SUM(A1:A2)"));
    }

    [Test]
    public void SubmitFeedback_PersistsTrimmedOpenRow()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var feedback = new FeedbackManagement(context);

        //act
        var saved = feedback.Submit(
            user.Id,
            "  Please add dark mode  ",
            Feedback.FeedbackCategory.Feature
        );

        //assert
        Assert.That(saved.Id, Is.GreaterThan(0));
        var row = context.Feedbacks.Single();
        Assert.That(row.Message, Is.EqualTo("Please add dark mode"));
        Assert.That(row.Category, Is.EqualTo(Feedback.FeedbackCategory.Feature));
        Assert.That(row.Status, Is.EqualTo(Feedback.FeedbackStatus.Open));
        Assert.That(row.UserId, Is.EqualTo(user.Id));
    }

    [Test]
    public void SubmitFeedback_EmptyMessage_Throws()
    {
        using var context = CreateContext();
        var user = SeedUser(context);
        var feedback = new FeedbackManagement(context);

        Assert.Throws<ArgumentException>(() =>
            feedback.Submit(user.Id, "   ", Feedback.FeedbackCategory.Bug)
        );
        Assert.That(context.Feedbacks.Any(), Is.False);
    }

    [Test]
    public void SubmitFeedback_TooLongMessage_Throws()
    {
        using var context = CreateContext();
        var user = SeedUser(context);
        var feedback = new FeedbackManagement(context);

        var tooLong = new string('x', 4001);
        Assert.Throws<ArgumentException>(() =>
            feedback.Submit(user.Id, tooLong, Feedback.FeedbackCategory.Bug)
        );
    }

    [Test]
    public void SubmitFeedback_UndefinedCategory_Throws()
    {
        using var context = CreateContext();
        var user = SeedUser(context);
        var feedback = new FeedbackManagement(context);

        Assert.Throws<ArgumentException>(() =>
            feedback.Submit(user.Id, "Valid message", (Feedback.FeedbackCategory)99)
        );
    }

    [Test]
    public void GetAllFeedback_ReturnsNewestFirstWithSubmitterInfo()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        // Insert directly with explicit timestamps so ordering is deterministic
        // (Submit stamps DateTime.UtcNow, which two quick calls could tie on).
        context.Feedbacks.Add(
            new Feedback
            {
                UserId = user.Id,
                Message = "older",
                Category = Feedback.FeedbackCategory.Bug,
                CreatedAt = new DateTime(2026, 01, 01, 0, 0, 0, DateTimeKind.Utc),
            }
        );
        context.Feedbacks.Add(
            new Feedback
            {
                UserId = user.Id,
                Message = "newer",
                Category = Feedback.FeedbackCategory.Feature,
                CreatedAt = new DateTime(2026, 02, 01, 0, 0, 0, DateTimeKind.Utc),
            }
        );
        context.SaveChanges();
        var feedback = new FeedbackManagement(context);

        //act
        var all = feedback.GetAll();

        //assert — newest first, submitter name/email resolved
        Assert.That(all, Has.Count.EqualTo(2));
        Assert.That(all[0].Message, Is.EqualTo("newer"));
        Assert.That(all[1].Message, Is.EqualTo("older"));
        Assert.That(all[0].SubmittedByName, Is.EqualTo("Test User"));
        Assert.That(all[0].SubmittedByEmail, Is.EqualTo("test@example.com"));
    }
}
