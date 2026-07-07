using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WSIST.Engine;

namespace WSIST.UnitTests;

internal static class TestHelpers
{
    internal static WsistContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WsistContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WsistContext(options);
    }

    // TestManagement only touches the cache for language lookups, so every
    // manager gets its own fresh cache and tests stay isolated.
    internal static TestManagement CreateManager(WsistContext context) =>
        new(context, new MemoryCache(new MemoryCacheOptions()));

    internal static User SeedUser(WsistContext context)
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

    internal static int SeedSystemSubject(WsistContext context, string name = "Math")
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
}
