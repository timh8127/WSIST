namespace WSIST.Engine;

public class Subject
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsSystem { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
}
