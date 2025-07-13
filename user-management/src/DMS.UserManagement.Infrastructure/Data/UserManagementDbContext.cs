using DMS.UserManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DMS.UserManagement.Infrastructure.Data;

public class UserManagementDbContext : DbContext
{
    public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Seed default admin user
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@demoauto.com",
                PasswordHash = adminPasswordHash,
                Role = "admin",
                Status = "active",
                Department = "IT",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@demoauto.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "sales",
                Status = "active",
                Department = "Sales",
                Phone = "(555) 123-4567",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@demoauto.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "service",
                Status = "active",
                Department = "Service",
                Phone = "(555) 234-5678",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
