using DMS.ServiceManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DMS.ServiceManagement.Infrastructure.Data
{
    public class ServiceManagementDbContext : DbContext
    {
        public ServiceManagementDbContext(DbContextOptions<ServiceManagementDbContext> options) : base(options)
        {
        }

        public DbSet<ServiceAppointment> ServiceAppointments { get; set; }
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<ServiceJob> ServiceJobs { get; set; }
        public DbSet<ServicePart> ServiceParts { get; set; }
        public DbSet<ServiceBay> ServiceBays { get; set; }
        public DbSet<ServiceInspection> ServiceInspections { get; set; }
        public DbSet<LoanerVehicle> LoanerVehicles { get; set; }
        public DbSet<InspectionResult> InspectionResults { get; set; }
        public DbSet<RecommendedService> RecommendedServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure service appointment
            modelBuilder.Entity<ServiceAppointment>()
                .HasOne(a => a.Bay)
                .WithMany(b => b.Appointments)
                .HasForeignKey(a => a.BayId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<ServiceAppointment>()
                .HasOne(a => a.RepairOrder)
                .WithOne(r => r.Appointment)
                .HasForeignKey<RepairOrder>(r => r.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure repair order
            modelBuilder.Entity<RepairOrder>()
                .HasMany(r => r.ServiceJobs)
                .WithOne(j => j.RepairOrder)
                .HasForeignKey(j => j.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RepairOrder>()
                .HasMany(r => r.Inspections)
                .WithOne(i => i.RepairOrder)
                .HasForeignKey(i => i.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure service job
            modelBuilder.Entity<ServiceJob>()
                .HasMany(j => j.Parts)
                .WithOne(p => p.ServiceJob)
                .HasForeignKey(p => p.ServiceJobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceJob>()
                .HasMany(j => j.InspectionResults)
                .WithOne(i => i.ServiceJob)
                .HasForeignKey(i => i.ServiceJobId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure service inspection
            modelBuilder.Entity<ServiceInspection>()
                .HasMany(i => i.RecommendedServices)
                .WithOne(r => r.ServiceInspection)
                .HasForeignKey(r => r.ServiceInspectionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure loaner vehicle
            modelBuilder.Entity<LoanerVehicle>()
                .HasOne(l => l.CurrentRepairOrder)
                .WithMany()
                .HasForeignKey(l => l.CurrentRepairOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure enums to store as strings
            modelBuilder.Entity<ServiceAppointment>()
                .Property(e => e.AppointmentType)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceAppointment>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceAppointment>()
                .Property(e => e.TransportationType)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceAppointment>()
                .Property(e => e.ConfirmationStatus)
                .HasConversion<string>();
                
            modelBuilder.Entity<RepairOrder>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceJob>()
                .Property(e => e.JobType)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceJob>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceJob>()
                .Property(e => e.WarrantyPayType)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServicePart>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceBay>()
                .Property(e => e.Type)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceBay>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceInspection>()
                .Property(e => e.Type)
                .HasConversion<string>();
                
            modelBuilder.Entity<ServiceInspection>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<RecommendedService>()
                .Property(e => e.Urgency)
                .HasConversion<string>();
                
            modelBuilder.Entity<LoanerVehicle>()
                .Property(e => e.Status)
                .HasConversion<string>();
                
            modelBuilder.Entity<InspectionResult>()
                .Property(e => e.Result)
                .HasConversion<string>();

            // JSON conversion for lists
            modelBuilder.Entity<ServiceBay>()
                .Property(e => e.Equipment)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                );

            modelBuilder.Entity<ServiceInspection>()
                .Property(e => e.InspectionImages)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                );

            modelBuilder.Entity<InspectionResult>()
                .Property(e => e.Images)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null)
                );
        }
    }
}
