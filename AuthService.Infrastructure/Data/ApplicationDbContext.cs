using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id).HasName("PK_USERS");
            e.Property(u => u.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            e.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            e.HasIndex(u => u.Email).IsUnique();

            e.Property(u => u.PasswordHash)
             .HasMaxLength(64)        // Base64(32 bytes) 約 44字元
             .IsRequired();

            e.Property(u => u.PasswordSalt)
             .HasMaxLength(24)        // Base64(16 bytes)
             .IsRequired();

            e.Property(u => u.PasswordMac)
             .HasMaxLength(64)
             .IsRequired();

            e.Property(u => u.KmsKeyVersion)
             .HasMaxLength(20);

            e.Property(u => u.CreatedAt)
                .HasDefaultValueSql("SYSDATETIME()");
        });
    }
}