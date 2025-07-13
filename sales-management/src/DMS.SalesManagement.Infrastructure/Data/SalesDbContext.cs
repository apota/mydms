using System;
using DMS.SalesManagement.Core.Models;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.SalesManagement.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework DbContext for Sales Management
    /// </summary>
    public class SalesDbContext : BaseDbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
        {
        }        public DbSet<Lead> Leads { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<DealAddOn> DealAddOns { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<SalesDocument> Documents { get; set; }

        /// <summary>
        /// Configures model for Sales Management entities
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Lead entity configuration
            modelBuilder.Entity<Lead>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.Property(e => e.SourceId).HasMaxLength(100);
                entity.Property(e => e.Comments).HasMaxLength(4000);
                
                entity.OwnsOne(e => e.Address, a =>
                {
                    a.Property(p => p.Street).HasMaxLength(255);
                    a.Property(p => p.City).HasMaxLength(100);
                    a.Property(p => p.State).HasMaxLength(50);
                    a.Property(p => p.Zip).HasMaxLength(20);
                });
                
                entity.OwnsMany(e => e.Activities, a =>
                {
                    a.WithOwner().HasForeignKey("LeadId");
                    a.HasKey(a => a.Id);
                    a.Property(a => a.Id).ValueGeneratedOnAdd();
                    a.Property(a => a.Notes).HasMaxLength(4000);
                });
            });

            // Deal entity configuration
            modelBuilder.Entity<Deal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TradeInValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DownPayment).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FinancingRate).HasColumnType("decimal(8,4)");
                entity.Property(e => e.MonthlyPayment).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(8,4)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                
                entity.OwnsMany(e => e.Fees, a =>
                {
                    a.WithOwner().HasForeignKey("DealId");
                    a.HasKey(a => a.Id);
                    a.Property(a => a.Id).ValueGeneratedOnAdd();
                    a.Property(a => a.Type).HasMaxLength(50);
                    a.Property(a => a.Amount).HasColumnType("decimal(18,2)");
                    a.Property(a => a.Description).HasMaxLength(255);
                });
                
                entity.OwnsMany(e => e.StatusHistory, a =>
                {
                    a.WithOwner().HasForeignKey("DealId");
                    a.HasKey(a => a.Id);
                    a.Property(a => a.Id).ValueGeneratedOnAdd();
                    a.Property(a => a.Notes).HasMaxLength(4000);
                });
            });

            // DealAddOn entity configuration
            modelBuilder.Entity<DealAddOn>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Term).HasMaxLength(50);
                
                entity.HasOne(d => d.Deal)
                    .WithMany(p => p.AddOns)
                    .HasForeignKey(d => d.DealId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Commission entity configuration
            modelBuilder.Entity<Commission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.BaseAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.BonusAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CalculationMethod).HasMaxLength(100);
                
                entity.HasOne(d => d.Deal)
                    .WithMany(p => p.Commissions)
                    .HasForeignKey(d => d.DealId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SalesDocument entity configuration
            modelBuilder.Entity<SalesDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Filename).HasMaxLength(255);
                entity.Property(e => e.Location).HasMaxLength(1000);
                
                entity.OwnsMany(e => e.RequiredSignatures, a =>
                {
                    a.WithOwner().HasForeignKey("SalesDocumentId");
                    a.HasKey(a => a.Id);
                    a.Property(a => a.Id).ValueGeneratedOnAdd();
                    a.Property(a => a.Role).HasMaxLength(50);
                    a.Property(a => a.Name).HasMaxLength(100);
                });
                
                entity.HasOne(d => d.Deal)
                    .WithMany(p => p.Documents)
                    .HasForeignKey(d => d.DealId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
