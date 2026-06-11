using Microsoft.EntityFrameworkCore;

namespace WSIST.Engine;

public class WsistContext : DbContext
{
    public WsistContext(DbContextOptions<WsistContext> options) : base(options) { }

    public DbSet<Test> Tests { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Subject> Subjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Test>(entity =>
        {
            entity.ToTable("Tests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Volume).HasConversion<int>();
            entity.Property(e => e.Understanding).HasConversion<int>();
            entity.Property(e => e.Grade).IsRequired(false);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Tests)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(t => t.Subject)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subjects");
            entity.HasKey(e => e.Id);
            // User subjects get database-generated (auto-increment) ids; the
            // seeded system subjects live on negative ids so the two ranges
            // can never collide.
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsSystem).HasDefaultValue(false);

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasData(
                new Subject { Id = -6, Name = "Math", IsSystem = true },
                new Subject { Id = -5, Name = "English", IsSystem = true },
                new Subject { Id = -4, Name = "French", IsSystem = true },
                new Subject { Id = -3, Name = "German", IsSystem = true },
                new Subject { Id = -2, Name = "Chemistry", IsSystem = true },
                new Subject { Id = -1, Name = "Other", IsSystem = true }
            );
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