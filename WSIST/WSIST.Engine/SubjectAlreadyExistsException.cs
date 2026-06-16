namespace WSIST.Engine;

/// <summary>
/// Thrown when a custom subject cannot be created because a subject with the
/// same name (case-insensitive) is already accessible to the user — either a
/// system subject or one of their own. Keeps the duplicate rule in one place
/// so every caller (Settings page, test modal) enforces it identically.
/// </summary>
public class SubjectAlreadyExistsException : Exception
{
    public SubjectAlreadyExistsException(string name)
        : base($"A subject named \"{name}\" already exists.") { }
}
