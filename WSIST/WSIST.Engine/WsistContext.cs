using Microsoft.EntityFrameworkCore;

namespace WSIST.Engine;

public class WsistContext : DbContext
{
    public WsistContext(DbContextOptions<WsistContext> options) : base(options) { }

    public DbSet<Test> Tests { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>(entity =>
        {
            entity.ToTable("Tests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Subject).HasConversion<int>();
            entity.Property(e => e.Volume).HasConversion<int>();
            entity.Property(e => e.Understanding).HasConversion<int>();
            entity.Property(e => e.Grade).IsRequired(false);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Tests)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.GoogleId).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(100);
        });
    }
}