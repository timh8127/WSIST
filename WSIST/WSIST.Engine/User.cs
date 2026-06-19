namespace WSIST.Engine;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GoogleId { get; set; } = string.Empty;

    // Two-letter culture code ("en"/"de") for the UI language. Null until the
    // user explicitly picks one — while null the language is derived from the
    // browser's Accept-Language header on each request.
    public string? PreferredLanguage { get; set; }

    // Default so a User created without an explicit timestamp never persists
    // DateTime.MinValue (0001-01-01).
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Test> Tests { get; set; } = [];
}
