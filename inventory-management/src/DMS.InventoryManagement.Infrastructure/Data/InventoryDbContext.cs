using DMS.InventoryManagement.Core.Models;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Data
{
    /// <summary>
    /// Database context for the inventory management module
    /// </summary>
    public class InventoryDbContext : BaseDbContext
    {
        private readonly ILogger<InventoryDbContext> _logger;

        public InventoryDbContext(DbContextOptions<InventoryDbContext> options, ILogger<InventoryDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the vehicles in inventory
        /// </summary>
        public DbSet<Vehicle> Vehicles { get; set; } = null!;

        /// <summary>
        /// Gets or sets the vehicle features
        /// </summary>
        public DbSet<VehicleFeature> VehicleFeatures { get; set; } = null!;

        /// <summary>
        /// Gets or sets the vehicle images
        /// </summary>
        public DbSet<VehicleImage> VehicleImages { get; set; } = null!;        

        /// <summary>
        /// Gets or sets the reconditioning records
        /// </summary>
        public DbSet<ReconditioningRecord> ReconditioningRecords { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the vehicle documents
        /// </summary>
        public DbSet<VehicleDocument> VehicleDocuments { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the vehicle locations
        /// </summary>
        public DbSet<VehicleLocation> VehicleLocations { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the location zones
        /// </summary>
        public DbSet<LocationZone> LocationZones { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the vehicle costs
        /// </summary>
        public DbSet<VehicleCost> VehicleCosts { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the additional costs
        /// </summary>
        public DbSet<AdditionalCost> AdditionalCosts { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the vehicle pricing
        /// </summary>
        public DbSet<VehiclePricing> VehiclePricings { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the price history entries
        /// </summary>
        public DbSet<PriceHistoryEntry> PriceHistoryEntries { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the vehicle aging information
        /// </summary>
        public DbSet<VehicleAging> VehicleAgings { get; set; } = null!;

        /// <summary>
        /// Gets or sets the workflow definitions
        /// </summary>
        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the workflow steps
        /// </summary>
        public DbSet<WorkflowStep> WorkflowSteps { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the workflow instances
        /// </summary>
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the workflow step instances
        /// </summary>
        public DbSet<WorkflowStepInstance> WorkflowStepInstances { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the vehicle inspections
        /// </summary>
        public DbSet<VehicleInspection> VehicleInspections { get; set; } = null!;
        
        /// <summary>
        /// Configure the model
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Vehicle entity
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicles", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VIN).IsUnique();
                entity.HasIndex(e => e.StockNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.VehicleType);
                
                entity.Property(e => e.VIN).IsRequired().HasMaxLength(17);
                entity.Property(e => e.StockNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Make).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.Trim).HasMaxLength(50);
                entity.Property(e => e.ExteriorColor).HasMaxLength(50);
                entity.Property(e => e.InteriorColor).HasMaxLength(50);
                entity.Property(e => e.Mileage).IsRequired();
                entity.Property(e => e.VehicleType).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.AcquisitionCost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.ListPrice).HasColumnType("decimal(19,4)");
                entity.Property(e => e.InvoicePrice).HasColumnType("decimal(19,4)");
                entity.Property(e => e.MSRP).HasColumnType("decimal(19,4)");
                entity.Property(e => e.AcquisitionDate).IsRequired();
                entity.Property(e => e.AcquisitionSource).HasMaxLength(100);
                entity.Property(e => e.LotLocation).HasMaxLength(100);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            });

            // Configure VehicleFeature entity
            modelBuilder.Entity<VehicleFeature>(entity =>
            {
                entity.ToTable("VehicleFeatures", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.VehicleId, e.Name }).IsUnique();
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            // Configure VehicleImage entity
            modelBuilder.Entity<VehicleImage>(entity =>
            {
                entity.ToTable("VehicleImages", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleId);
                
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Caption).HasMaxLength(200);
                entity.Property(e => e.SequenceNumber).IsRequired();
                entity.Property(e => e.IsPrimary).HasDefaultValue(false);
                entity.Property(e => e.UploadDate).IsRequired();
                entity.Property(e => e.ImageType).IsRequired();
            });

            // Configure ReconditioningRecord entity
            modelBuilder.Entity<ReconditioningRecord>(entity =>
            {
                entity.ToTable("ReconditioningRecords", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleId);
                
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Cost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.Vendor).HasMaxLength(100);
                entity.Property(e => e.WorkDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });            // Configure VehicleLocation entity
            modelBuilder.Entity<VehicleLocation>(entity =>
            {
                entity.ToTable("VehicleLocations", "inventory");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired();
                
                entity.OwnsOne(e => e.Address, address =>
                {
                    address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
                    address.Property(a => a.City).HasColumnName("City").HasMaxLength(100).IsRequired();
                    address.Property(a => a.State).HasColumnName("State").HasMaxLength(50).IsRequired();
                    address.Property(a => a.ZipCode).HasColumnName("ZipCode").HasMaxLength(20).IsRequired();
                    address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(50).IsRequired();
                });
                
                entity.OwnsOne(e => e.Coordinates, coords =>
                {
                    coords.Property(c => c.Latitude).HasColumnName("Latitude");
                    coords.Property(c => c.Longitude).HasColumnName("Longitude");
                });
            });
            
            // Configure LocationZone entity
            modelBuilder.Entity<LocationZone>(entity =>
            {
                entity.ToTable("LocationZones", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleLocationId);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Capacity).IsRequired();
            });
            
            // Configure VehicleCost entity
            modelBuilder.Entity<VehicleCost>(entity =>
            {
                entity.ToTable("VehicleCosts", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleId).IsUnique();
                
                entity.Property(e => e.AcquisitionCost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.TransportCost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.ReconditioningCost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.CertificationCost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(19,4)");
                entity.Property(e => e.TargetGrossProfit).HasColumnType("decimal(19,4)");
            });
            
            // Configure AdditionalCost entity
            modelBuilder.Entity<AdditionalCost>(entity =>
            {
                entity.ToTable("AdditionalCosts", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleCostId);
                
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(19,4)");
                entity.Property(e => e.Date).IsRequired();
            });
            
            // Configure VehiclePricing entity
            modelBuilder.Entity<VehiclePricing>(entity =>
            {
                entity.ToTable("VehiclePricings", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleId).IsUnique();
                
                entity.Property(e => e.MSRP).HasColumnType("decimal(19,4)");
                entity.Property(e => e.InternetPrice).HasColumnType("decimal(19,4)");
                entity.Property(e => e.StickingPrice).HasColumnType("decimal(19,4)");
                entity.Property(e => e.FloorPrice).HasColumnType("decimal(19,4)");
                entity.Property(e => e.SpecialPrice).HasColumnType("decimal(19,4)");
            });
            
            // Configure PriceHistoryEntry entity
            modelBuilder.Entity<PriceHistoryEntry>(entity =>
            {
                entity.ToTable("PriceHistoryEntries", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehiclePricingId);
                
                entity.Property(e => e.Price).HasColumnType("decimal(19,4)");
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Reason).HasMaxLength(200);
                entity.Property(e => e.UserId).HasMaxLength(100);
            });
            
            // Configure VehicleAging entity
            modelBuilder.Entity<VehicleAging>(entity =>
            {
                entity.ToTable("VehicleAgings", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleId).IsUnique();
                
                entity.Property(e => e.DaysInInventory).IsRequired();
                entity.Property(e => e.AgeThreshold).IsRequired();
                entity.Property(e => e.AgingAlertLevel).IsRequired();
                entity.Property(e => e.RecommendedAction).HasMaxLength(500);
            });

            // Configure VehicleDocument entity
            modelBuilder.Entity<VehicleDocument>(entity =>
            {
                entity.ToTable("VehicleDocuments", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.VehicleId);
                
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DocumentType).IsRequired();
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UploadDate).IsRequired();
            });
            
            // Configure WorkflowDefinition entity
            modelBuilder.Entity<WorkflowDefinition>(entity =>
            {
                entity.ToTable("WorkflowDefinitions", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.WorkflowType);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.WorkflowType).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
            });
            
            // Configure WorkflowStep entity
            modelBuilder.Entity<WorkflowStep>(entity =>
            {
                entity.ToTable("WorkflowSteps", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowDefinitionId);
                entity.HasIndex(e => new { e.WorkflowDefinitionId, e.SequenceNumber }).IsUnique();
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.SequenceNumber).IsRequired();
                entity.Property(e => e.ExpectedDurationHours).IsRequired();
                entity.Property(e => e.ResponsibleParty).HasMaxLength(100);
                entity.Property(e => e.RequiresApproval).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.RequiredDocuments).HasColumnType("jsonb");
            });
            
            // Configure WorkflowInstance entity
            modelBuilder.Entity<WorkflowInstance>(entity =>
            {
                entity.ToTable("WorkflowInstances", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowDefinitionId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.Status);
                
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.Priority).IsRequired().HasDefaultValue(3);
            });
            
            // Configure WorkflowStepInstance entity
            modelBuilder.Entity<WorkflowStepInstance>(entity =>
            {
                entity.ToTable("WorkflowStepInstances", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowInstanceId);
                entity.HasIndex(e => e.WorkflowStepId);
                entity.HasIndex(e => e.Status);
                
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            });
            
            // Configure relationships
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Features)
                .WithOne()
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Images)
                .WithOne()
                .HasForeignKey(i => i.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.ReconditioningRecords)
                .WithOne()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Documents)
                .WithOne(d => d.Vehicle)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Location)
                .WithMany(l => l.Vehicles)
                .HasForeignKey(v => v.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.CostDetails)
                .WithOne(c => c.Vehicle)
                .HasForeignKey<VehicleCost>(c => c.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.PricingDetails)
                .WithOne(p => p.Vehicle)
                .HasForeignKey<VehiclePricing>(p => p.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.AgingInfo)
                .WithOne(a => a.Vehicle)
                .HasForeignKey<VehicleAging>(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<VehicleLocation>()
                .HasMany(l => l.Zones)
                .WithOne()
                .HasForeignKey(z => z.VehicleLocationId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<VehicleCost>()
                .HasMany(c => c.AdditionalCosts)
                .WithOne()
                .HasForeignKey(a => a.VehicleCostId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<VehiclePricing>()
                .HasMany(p => p.PriceHistory)
                .WithOne()
                .HasForeignKey(ph => ph.VehiclePricingId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure WorkflowDefinition entity
            modelBuilder.Entity<WorkflowDefinition>(entity =>
            {
                entity.ToTable("WorkflowDefinitions", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.WorkflowType);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.WorkflowType).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.IsDefault).IsRequired().HasDefaultValue(false);
            });
            
            // Configure WorkflowStep entity
            modelBuilder.Entity<WorkflowStep>(entity =>
            {
                entity.ToTable("WorkflowSteps", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowDefinitionId);
                entity.HasIndex(e => new { e.WorkflowDefinitionId, e.SequenceNumber }).IsUnique();
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.SequenceNumber).IsRequired();
                entity.Property(e => e.ExpectedDurationHours).IsRequired();
                entity.Property(e => e.ResponsibleParty).HasMaxLength(100);
                entity.Property(e => e.RequiresApproval).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.RequiredDocuments).HasColumnType("jsonb");
            });
            
            // Configure WorkflowInstance entity
            modelBuilder.Entity<WorkflowInstance>(entity =>
            {
                entity.ToTable("WorkflowInstances", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowDefinitionId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.Status);
                
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.Priority).IsRequired().HasDefaultValue(3);
            });
            
            // Configure WorkflowStepInstance entity
            modelBuilder.Entity<WorkflowStepInstance>(entity =>
            {
                entity.ToTable("WorkflowStepInstances", "inventory");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowInstanceId);
                entity.HasIndex(e => e.WorkflowStepId);
                entity.HasIndex(e => e.Status);
                
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.AssignedTo).HasMaxLength(100);
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            });
            
            // Configure workflow relationships
            modelBuilder.Entity<WorkflowDefinition>()
                .HasMany(w => w.Steps)
                .WithOne(s => s.WorkflowDefinition)
                .HasForeignKey(s => s.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<WorkflowDefinition>()
                .HasMany(w => w.Instances)
                .WithOne(i => i.WorkflowDefinition)
                .HasForeignKey(i => i.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<WorkflowInstance>()
                .HasMany(w => w.StepInstances)
                .WithOne(s => s.WorkflowInstance)
                .HasForeignKey(s => s.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<WorkflowStepInstance>()
                .HasOne(s => s.WorkflowStep)
                .WithMany()
                .HasForeignKey(s => s.WorkflowStepId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.WorkflowInstances)
                .WithOne(w => w.Vehicle)
                .HasForeignKey(w => w.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
