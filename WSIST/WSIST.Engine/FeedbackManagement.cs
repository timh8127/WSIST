using Microsoft.EntityFrameworkCore;

namespace WSIST.Engine;

public class FeedbackManagement
{
    private readonly WsistContext context;

    public FeedbackManagement(WsistContext context)
    {
        this.context = context;
    }

    public Feedback Submit(int userId, string message, Feedback.FeedbackCategory category)
    {
        var trimmed = message?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new ArgumentException("Feedback message cannot be empty.", nameof(message));

        // Defensively cap to the column length so an over-long message surfaces
        // as a validation error rather than a DbUpdateException at SaveChanges.
        if (trimmed.Length > 4000)
            throw new ArgumentException(
                "Feedback message is too long (4000 characters max).",
                nameof(message)
            );

        if (!Enum.IsDefined(category))
            throw new ArgumentException("Unknown feedback category.", nameof(category));

        var feedback = new Feedback
        {
            UserId = userId,
            Message = trimmed,
            Category = category,
            Status = Feedback.FeedbackStatus.Open,
            CreatedAt = DateTime.UtcNow,
        };
        context.Feedbacks.Add(feedback);
        context.SaveChanges();
        return feedback;
    }

    // Admin-only listing. Authorization (is the caller the owner?) is enforced
    // by the page that calls this — the engine intentionally has no notion of
    // who the admin is, so it stays config-driven in the web layer.
    public List<FeedbackView> GetAll()
    {
        return context
            .Feedbacks.AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FeedbackView(
                f.Id,
                f.Message,
                f.Category,
                f.Status,
                f.CreatedAt,
                f.User != null ? f.User.DisplayName : "(unknown)",
                f.User != null ? f.User.Email : ""
            ))
            .ToList();
    }

    public record FeedbackView(
        int Id,
        string Message,
        Feedback.FeedbackCategory Category,
        Feedback.FeedbackStatus Status,
        DateTime CreatedAt,
        string SubmittedByName,
        string SubmittedByEmail
    );
}
