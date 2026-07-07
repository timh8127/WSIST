using NUnit.Framework;
using WSIST.Engine;
using static WSIST.UnitTests.TestHelpers;

namespace WSIST.UnitTests;

public class FeedbackManagementTests
{
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

    [Test]
    public void UpdateStatus_ChangesStatusAndPersists()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var feedback = new FeedbackManagement(context);
        var saved = feedback.Submit(user.Id, "A bug", Feedback.FeedbackCategory.Bug);
        Assert.That(saved.Status, Is.EqualTo(Feedback.FeedbackStatus.Open));

        //act
        var ok = feedback.UpdateStatus(saved.Id, Feedback.FeedbackStatus.Reviewed);

        //assert
        Assert.That(ok, Is.True);
        Assert.That(
            context.Feedbacks.Single().Status,
            Is.EqualTo(Feedback.FeedbackStatus.Reviewed)
        );
    }

    [Test]
    public void UpdateStatus_ReturnsFalseForMissingRow()
    {
        using var context = CreateContext();
        var feedback = new FeedbackManagement(context);
        Assert.That(feedback.UpdateStatus(999, Feedback.FeedbackStatus.Closed), Is.False);
    }

    [Test]
    public void GetForUser_ReturnsOnlyTheUsersOwnFeedback()
    {
        //arrange
        using var context = CreateContext();
        var user = SeedUser(context);
        var other = new User
        {
            Email = "other@example.com",
            DisplayName = "Other",
            GoogleId = "google-456",
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(other);
        context.SaveChanges();
        var feedback = new FeedbackManagement(context);
        feedback.Submit(user.Id, "Mine", Feedback.FeedbackCategory.Bug);
        feedback.Submit(other.Id, "Theirs", Feedback.FeedbackCategory.Other);

        //act
        var mine = feedback.GetForUser(user.Id);

        //assert — strictly scoped to the requesting user
        Assert.That(mine, Has.Count.EqualTo(1));
        Assert.That(mine[0].Message, Is.EqualTo("Mine"));
    }
}
