using DMS.PartsManagement.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace DMS.PartsManagement.Infrastructure.Data
{
    public class PartsDbContext : DbContext
    {
        public PartsDbContext(DbContextOptions<PartsDbContext> options) : base(options)
        {
        }

        public DbSet<Part> Parts { get; set; }
        public DbSet<PartInventory> PartInventories { get; set; }
        public DbSet<PartPricing> PartPricings { get; set; }
        public DbSet<PartCategory> PartCategories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<PartOrder> PartOrders { get; set; }
        public DbSet<PartOrderLine> PartOrderLines { get; set; }
        public DbSet<PartTransaction> PartTransactions { get; set; }
        public DbSet<PartCoreTracking> PartCoreTrackings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Part
            modelBuilder.Entity<Part>(entity =>
            {
                entity.ToTable("parts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ManufacturerPartNumber).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Weight).HasColumnType("decimal(10, 3)");
                entity.Property(e => e.CoreValue).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.SupercededBy).HasMaxLength(50);
                
                entity.HasOne(e => e.Manufacturer)
                      .WithMany(m => m.Parts)
                      .HasForeignKey(e => e.ManufacturerId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Parts)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Complex type mappings
                entity.OwnsOne(e => e.Dimensions, d =>
                {
                    d.Property(p => p.Length).HasColumnType("decimal(10, 3)");
                    d.Property(p => p.Width).HasColumnType("decimal(10, 3)");
                    d.Property(p => p.Height).HasColumnType("decimal(10, 3)");
                    d.Property(p => p.UnitOfMeasure).HasMaxLength(10);
                });
                
                entity.OwnsOne(e => e.FitmentData, fd =>
                {
                    fd.ToJson();
                });
                
                entity.Property(e => e.CrossReferences).HasColumnType("jsonb");
                entity.Property(e => e.ReplacementFor).HasColumnType("jsonb");
                entity.Property(e => e.Images).HasColumnType("jsonb");
            });
            
            // PartInventory
            modelBuilder.Entity<PartInventory>(entity =>
            {
                entity.ToTable("part_inventories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.BinLocation).HasMaxLength(50);
                entity.Property(e => e.MovementClass).HasConversion<string>().HasMaxLength(20);
                
                entity.HasOne(e => e.Part)
                      .WithMany(p => p.Inventories)
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Location)
                      .WithMany(l => l.Inventories)
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasIndex(e => new { e.PartId, e.LocationId }).IsUnique();
            });
            
            // PartPricing
            modelBuilder.Entity<PartPricing>(entity =>
            {
                entity.ToTable("part_pricings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.RetailPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.WholesalePrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.SpecialPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Markup).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Margin).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PriceSource).HasConversion<string>().HasMaxLength(20);
                
                entity.HasOne(e => e.Part)
                      .WithOne(p => p.Pricing)
                      .HasForeignKey<PartPricing>(e => e.PartId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.PriceHistory).HasColumnType("jsonb");
            });
            
            // PartCategory
            modelBuilder.Entity<PartCategory>(entity =>
            {
                entity.ToTable("part_categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasOne(e => e.ParentCategory)
                      .WithMany(c => c.ChildCategories)
                      .HasForeignKey(e => e.ParentCategoryId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Manufacturer
            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.ToTable("manufacturers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.Website).HasMaxLength(255);
                entity.Property(e => e.Notes).HasMaxLength(1000);
            });
            
            // Supplier
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("suppliers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.AccountNumber).HasMaxLength(50);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Website).HasMaxLength(255);
                entity.Property(e => e.ShippingTerms).HasMaxLength(100);
                entity.Property(e => e.PaymentTerms).HasMaxLength(100);
                
                // Complex type mapping
                entity.OwnsOne(e => e.Address, a =>
                {
                    a.Property(p => p.Street).HasMaxLength(100);
                    a.Property(p => p.City).HasMaxLength(50);
                    a.Property(p => p.State).HasMaxLength(50);
                    a.Property(p => p.Zip).HasMaxLength(20);
                    a.Property(p => p.Country).HasMaxLength(50);
                });
                
                entity.Property(e => e.OrderMethods).HasColumnType("jsonb");
            });
            
            // Location
            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("locations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.Type).HasMaxLength(50);
                
                // Complex type mapping
                entity.OwnsOne(e => e.Address, a =>
                {
                    a.Property(p => p.Street).HasMaxLength(100);
                    a.Property(p => p.City).HasMaxLength(50);
                    a.Property(p => p.State).HasMaxLength(50);
                    a.Property(p => p.Zip).HasMaxLength(20);
                    a.Property(p => p.Country).HasMaxLength(50);
                });
            });
            
            // PartOrder
            modelBuilder.Entity<PartOrder>(entity =>
            {
                entity.ToTable("part_orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.OrderType).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.ShippingMethod).HasMaxLength(50);
                entity.Property(e => e.TrackingNumber).HasMaxLength(100);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ShippingCost).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                
                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Orders)
                      .HasForeignKey(e => e.SupplierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // PartOrderLine
            modelBuilder.Entity<PartOrderLine>(entity =>
            {
                entity.ToTable("part_order_lines");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ExtendedCost).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                
                entity.HasOne(e => e.PartOrder)
                      .WithMany(po => po.OrderLines)
                      .HasForeignKey(e => e.PartOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Part)
                      .WithMany()
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                // Complex type mapping
                entity.OwnsOne(e => e.Allocation, a =>
                {
                    a.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
                    a.Property(p => p.ReferenceType).HasMaxLength(50);
                });
            });
            
            // PartTransaction
            modelBuilder.Entity<PartTransaction>(entity =>
            {
                entity.ToTable("part_transactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.TransactionType).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.ReferenceType).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.UnitCost).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ExtendedCost).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ExtendedPrice).HasColumnType("decimal(10, 2)");
                
                entity.HasOne(e => e.Part)
                      .WithMany(p => p.Transactions)
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.SourceLocation)
                      .WithMany()
                      .HasForeignKey(e => e.SourceLocationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.DestinationLocation)
                      .WithMany()
                      .HasForeignKey(e => e.DestinationLocationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // PartCoreTracking
            modelBuilder.Entity<PartCoreTracking>(entity =>
            {
                entity.ToTable("part_core_trackings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CorePartNumber).HasMaxLength(50);
                entity.Property(e => e.CoreValue).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.CreditAmount).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                
                entity.HasOne(e => e.Part)
                      .WithMany(p => p.CoreTrackings)
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
