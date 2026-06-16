using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WSIST.Engine;

/// <summary>
/// Lets EF Core tooling (e.g. <c>dotnet ef migrations add</c>) construct the
/// context at design time without a running database. The production wiring in
/// Program.cs uses <c>ServerVersion.AutoDetect</c>, which opens a real
/// connection — fine at runtime, but it would block migration scaffolding when
/// no server is up. A fixed server version and a placeholder connection string
/// avoid that; neither value is ever used to talk to a real database.
/// </summary>
public class DesignTimeWsistContextFactory : IDesignTimeDbContextFactory<WsistContext>
{
    public WsistContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WsistContext>()
            .UseMySql(
                "Server=localhost;Database=wsist_design;User=root;Password=;",
                new MySqlServerVersion(new Version(8, 0, 0))
            )
            .Options;
        return new WsistContext(options);
    }
}
