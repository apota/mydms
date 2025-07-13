using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Models;

namespace DMS.ReportingAnalytics.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for the Reporting and Analytics module.
/// </summary>
public class ReportingDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportingDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Gets or sets the report definitions.
    /// </summary>
    public DbSet<ReportDefinition> Reports { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the dashboard definitions.
    /// </summary>
    public DbSet<DashboardDefinition> Dashboards { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the dashboard widgets.
    /// </summary>
    public DbSet<DashboardWidget> DashboardWidgets { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the scheduled reports.
    /// </summary>
    public DbSet<ScheduledReport> ScheduledReports { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the report execution history.
    /// </summary>
    public DbSet<ReportExecutionHistory> ReportExecutionHistory { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the data mart definitions.
    /// </summary>
    public DbSet<DataMartDefinition> DataMarts { get; set; } = null!;
    
    /// <summary>
    /// Configures the model that was discovered by convention from the entity types.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Report entity
        modelBuilder.Entity<ReportDefinition>(entity =>
        {
            entity.HasKey(e => e.ReportId);
            entity.Property(e => e.ReportId).ValueGeneratedOnAdd();
            entity.Property(e => e.ReportName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Owner).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Parameters).HasColumnType("jsonb");
            entity.Property(e => e.Permissions).HasColumnType("jsonb");
        });
        
        // Configure Dashboard entity
        modelBuilder.Entity<DashboardDefinition>(entity =>
        {
            entity.HasKey(e => e.DashboardId);
            entity.Property(e => e.DashboardId).ValueGeneratedOnAdd();
            entity.Property(e => e.DashboardName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Owner).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Layout).HasColumnType("jsonb");
            entity.Property(e => e.Permissions).HasColumnType("jsonb");
        });
        
        // Configure DashboardWidget entity
        modelBuilder.Entity<DashboardWidget>(entity =>
        {
            entity.HasKey(e => e.WidgetId);
            entity.Property(e => e.WidgetId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Position).HasColumnType("jsonb");
            entity.Property(e => e.Size).HasColumnType("jsonb");
            entity.Property(e => e.Configuration).HasColumnType("jsonb");
            
            entity.HasOne(w => w.Dashboard)
                  .WithMany(d => d.Widgets)
                  .HasForeignKey(w => w.DashboardId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure ScheduledReport entity
        modelBuilder.Entity<ScheduledReport>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);
            entity.Property(e => e.ScheduleId).ValueGeneratedOnAdd();
            entity.Property(e => e.Schedule).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Format).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Recipients).HasColumnType("jsonb");
            entity.Property(e => e.Subject).HasMaxLength(200);
            
            entity.HasOne(s => s.Report)
                  .WithMany(r => r.ScheduledReports)
                  .HasForeignKey(s => s.ReportId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure ReportExecutionHistory entity
        modelBuilder.Entity<ReportExecutionHistory>(entity =>
        {
            entity.HasKey(e => e.ExecutionId);
            entity.Property(e => e.ExecutionId).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Parameters).HasColumnType("jsonb");
            entity.Property(e => e.OutputLocation).HasMaxLength(1000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            entity.HasOne(e => e.Report)
                  .WithMany(r => r.ExecutionHistory)
                  .HasForeignKey(e => e.ReportId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Schedule)
                  .WithMany(s => s.ExecutionHistory)
                  .HasForeignKey(e => e.ScheduleId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Configure DataMartDefinition entity
        modelBuilder.Entity<DataMartDefinition>(entity =>
        {
            entity.HasKey(e => e.MartId);
            entity.Property(e => e.MartId).ValueGeneratedOnAdd();
            entity.Property(e => e.MartName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.RefreshSchedule).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Dependencies).HasColumnType("jsonb");
            entity.Property(e => e.Configuration).HasColumnType("jsonb");
        });
    }
}
