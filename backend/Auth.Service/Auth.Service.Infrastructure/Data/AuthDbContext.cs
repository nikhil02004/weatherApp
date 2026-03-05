using Auth.Service.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Service.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id)
                  .HasMaxLength(450);

            entity.Property(u => u.Username)
                  .HasMaxLength(256)
                  .IsRequired();
            entity.HasIndex(u => u.Username)
                  .IsUnique()
                  .HasDatabaseName("UQ_Users_Username");

            entity.Property(u => u.PasswordHash)
                  .IsRequired();

            entity.Property(u => u.Email)
                  .HasMaxLength(256);
            entity.HasIndex(u => u.Email)
                  .IsUnique()
                  .HasFilter("[Email] IS NOT NULL")
                  .HasDatabaseName("UQ_Users_Email");

            entity.Property(u => u.ExternalProvider)
                  .HasMaxLength(50);

            entity.Property(u => u.ExternalId)
                  .HasMaxLength(256);

            entity.Property(u => u.CreatedAt)
                  .HasDefaultValueSql("SYSUTCDATETIME()")
                  .ValueGeneratedOnAdd();
        });
    }
}
