namespace WSIST.Engine;

public class TestManagement
{
    private readonly WsistContext context;

    public TestManagement(WsistContext context)
    {
        this.context = context;
    }

    public List<Test> LoadAllTests(int userId) =>
        context.Tests.Where(t => t.UserId == userId).ToList();

    public void NewTestMaker(
        string title,
        int subjectId,
        DateOnly dueDate,
        Test.TestVolume volume,
        Test.PersonalUnderstanding understanding,
        double? grade,
        int userId
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        EnsureSubjectAccessible(subjectId, userId);

        var test = new Test
        {
            Id = Guid.NewGuid(),
            Title = title,
            Subject = subjectId,
            DueDate = dueDate,
            Volume = volume,
            Understanding = understanding,
            Grade = TestAssistants.GradeVerifier(dueDate, grade),
            UserId = userId,
        };
        context.Tests.Add(test);
        context.SaveChanges();
    }

    public void TestEditor(
        Guid id,
        string title,
        int subjectId,
        DateOnly dueDate,
        Test.TestVolume volume,
        Test.PersonalUnderstanding understanding,
        double? grade,
        int userId
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        EnsureSubjectAccessible(subjectId, userId);

        var test = context.Tests.Find(id);
        // Only the owner may edit a test.
        if (test is null || test.UserId != userId)
            return;

        test.Title = title;
        test.Subject = subjectId;
        test.DueDate = dueDate;
        test.Volume = volume;
        test.Understanding = understanding;
        test.Grade = TestAssistants.GradeVerifier(dueDate, grade);
        context.SaveChanges();
    }

    public void TestRemover(Guid id, int userId)
    {
        var test = context.Tests.Find(id);
        // Only the owner may delete a test.
        if (test is null || test.UserId != userId)
            return;
        context.Tests.Remove(test);
        context.SaveChanges();
    }

    private void EnsureSubjectAccessible(int subjectId, int userId)
    {
        // A test may only reference a system subject or one of the user's own
        // custom subjects — never another user's subject.
        var accessible = context.Subjects.Any(s =>
            s.Id == subjectId && (s.IsSystem || s.UserId == userId)
        );
        if (!accessible)
            throw new ArgumentException(
                "Subject does not exist or is not accessible.",
                nameof(subjectId)
            );
    }

    public List<Subject> GetSubjectsForUser(int userId)
    {
        return context
            .Subjects.Where(s => s.IsSystem || s.UserId == userId)
            .OrderBy(s => s.IsSystem ? 0 : 1)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public void AddCustomSubject(string name, int userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Subject name cannot be empty.", nameof(name));

        // Id is database-generated (auto-increment) — see WsistContext.
        var subject = new Subject
        {
            Name = name,
            IsSystem = false,
            UserId = userId,
        };
        context.Subjects.Add(subject);
        context.SaveChanges();
    }

    public bool RemoveCustomSubject(int subjectId, int userId)
    {
        var subject = context.Subjects.FirstOrDefault(s =>
            s.Id == subjectId && s.UserId == userId && !s.IsSystem
        );
        if (subject is null)
            return false;
        // The Tests.Subject FK is Restrict — deleting a subject still in use would throw.
        if (context.Tests.Any(t => t.Subject == subjectId))
            return false;
        context.Subjects.Remove(subject);
        context.SaveChanges();
        return true;
    }

    public User? GetUser(int userId)
    {
        return context.Users.Find(userId);
    }

    public void UpdateDisplayName(int userId, string displayName)
    {
        var trimmed = displayName.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        var user = context.Users.Find(userId);
        if (user is null)
            return;
        user.DisplayName = trimmed;
        context.SaveChanges();
    }

    public User GetOrCreateUser(string email, string displayName, string googleId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var user = context.Users.FirstOrDefault(u => u.Email == email);
        if (user is not null)
            return user;

        try
        {
            user = new User
            {
                Email = email,
                DisplayName = displayName,
                GoogleId = googleId,
                CreatedAt = DateTime.UtcNow,
            };
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // A concurrent request created the same user between our check and
            // the insert (unique index on Email) — fetch the winner instead.
            // If no row exists, the failure had another cause; surface it.
            context.ChangeTracker.Clear();
            var existing = context.Users.FirstOrDefault(u => u.Email == email);
            if (existing is null)
                throw;
            return existing;
        }
    }

    public void DeleteUser(int userId)
    {
        var user = context.Users.Find(userId);
        if (user is null)
            return;
        context.Users.Remove(user);
        context.SaveChanges();
    }

    public record SubjectGradeAverage(
        int SubjectId,
        string SubjectName,
        double AverageGrade,
        int GradedTestCount
    );

    public List<SubjectGradeAverage> GetGradeAverages(int userId)
    {
        var subjects = context.Subjects.Where(s => s.IsSystem || s.UserId == userId).ToList();

        return context
            .Tests.Where(t => t.UserId == userId && t.Grade != null)
            .AsEnumerable()
            .GroupBy(t => t.Subject)
            .Select(g =>
            {
                var subject = subjects.FirstOrDefault(s => s.Id == g.Key);
                return new SubjectGradeAverage(
                    g.Key,
                    subject?.Name ?? g.Key.ToString(),
                    Math.Round(g.Average(t => t.Grade!.Value), 2),
                    g.Count()
                );
            })
            .OrderBy(x => x.SubjectName)
            .ToList();
    }
}
