namespace WSIST.Engine;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GoogleId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<Test> Tests { get; set; } = [];
}