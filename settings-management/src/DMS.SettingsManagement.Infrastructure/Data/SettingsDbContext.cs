using DMS.SettingsManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DMS.SettingsManagement.Infrastructure.Data
{
    public class SettingsDbContext : DbContext
    {
        public SettingsDbContext(DbContextOptions<SettingsDbContext> options) : base(options) { }

        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Key);
                entity.Property(e => e.Key).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DataType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.UpdatedBy).HasMaxLength(255);
                
                entity.HasIndex(e => e.Category);
            });

            // Seed default settings
            modelBuilder.Entity<Setting>().HasData(
                new Setting
                {
                    Key = "theme",
                    Value = "light",
                    Description = "Application theme (light/dark)",
                    Category = "Appearance",
                    DataType = "string",
                    IsUserEditable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = "notifications.email",
                    Value = "true",
                    Description = "Enable email notifications",
                    Category = "Notifications",
                    DataType = "boolean",
                    IsUserEditable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = "notifications.push",
                    Value = "true",
                    Description = "Enable push notifications",
                    Category = "Notifications",
                    DataType = "boolean",
                    IsUserEditable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = "system.maintenance",
                    Value = "false",
                    Description = "System maintenance mode",
                    Category = "System",
                    DataType = "boolean",
                    IsUserEditable = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = "security.session_timeout",
                    Value = "30",
                    Description = "Session timeout in minutes",
                    Category = "Security",
                    DataType = "number",
                    IsUserEditable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
