namespace WSIST.Engine;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GoogleId { get; set; } = string.Empty;
    // Default so a User created without an explicit timestamp never persists
    // DateTime.MinValue (0001-01-01).
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Test> Tests { get; set; } = [];
}