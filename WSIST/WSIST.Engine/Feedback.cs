namespace WSIST.Engine;

public class Feedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public required string Message { get; set; }
    public FeedbackCategory Category { get; set; }
    public FeedbackStatus Status { get; set; } = FeedbackStatus.Open;

    // Default so a Feedback created without an explicit timestamp never persists
    // DateTime.MinValue (0001-01-01) — mirrors User.CreatedAt.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public enum FeedbackCategory
    {
        Bug = 0,
        Feature = 1,
        Other = 2,
    }

    public enum FeedbackStatus
    {
        Open = 0,
        Reviewed = 1,
        Closed = 2,
    }
}
