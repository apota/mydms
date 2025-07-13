using Microsoft.EntityFrameworkCore;
using DMS.CRM.Core.Models;
using DMS.Shared.Data.Postgres;

namespace DMS.CRM.Infrastructure.Data
{
    /// <summary>
    /// Database context for CRM module
    /// </summary>
    public class CrmDbContext : BaseDbContext
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
        {
        }

        // Customer and related entities
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<CustomerInteraction> CustomerInteractions { get; set; } = null!;
        public DbSet<CustomerSegment> CustomerSegments { get; set; } = null!;
        public DbSet<CustomerJourney> CustomerJourneys { get; set; } = null!;
        public DbSet<CustomerVehicle> CustomerVehicles { get; set; } = null!;

        // Campaign entities
        public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<CampaignSegment> CampaignSegments { get; set; } = null!;

        // Survey entities
        public DbSet<CustomerSurvey> CustomerSurveys { get; set; } = null!;
        public DbSet<CustomerSurveyResponse> CustomerSurveyResponses { get; set; } = null!;

        // Loyalty entities
        public DbSet<CustomerLoyalty> CustomerLoyalties { get; set; } = null!;
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; } = null!;
        public DbSet<LoyaltyReward> LoyaltyRewards { get; set; } = null!;
        public DbSet<LoyaltyRedemption> LoyaltyRedemptions { get; set; } = null!;
        public DbSet<LoyaltyTierConfig> LoyaltyTierConfigs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.BusinessName).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                
                // Complex types stored as JSON
                entity.Property(e => e.PhoneNumbers).HasColumnType("jsonb");
                entity.Property(e => e.Addresses).HasColumnType("jsonb");
                entity.Property(e => e.CommunicationPreferences).HasColumnType("jsonb");
                entity.Property(e => e.DemographicInfo).HasColumnType("jsonb");
                entity.Property(e => e.Tags).HasColumnType("jsonb");
                
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.BusinessName);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ContactType);

                // Relationships
                entity.HasMany(e => e.Interactions)
                    .WithOne(i => i.Customer)
                    .HasForeignKey(i => i.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Journey)
                    .WithOne(j => j.Customer)
                    .HasForeignKey<CustomerJourney>(j => j.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Surveys)
                    .WithOne(s => s.Customer)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerInteraction entity
            modelBuilder.Entity<CustomerInteraction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subject).HasMaxLength(200);
                entity.Property(e => e.Content).HasMaxLength(2000);
                entity.Property(e => e.Outcome).HasMaxLength(500);
                entity.Property(e => e.UserId).HasMaxLength(50);
                entity.Property(e => e.ChannelId).HasMaxLength(50);
                entity.Property(e => e.RelatedToType).HasMaxLength(50);
                entity.Property(e => e.FollowUpAssignedToId).HasMaxLength(50);
                entity.Property(e => e.FollowUpNotes).HasMaxLength(1000);
                entity.Property(e => e.Tags).HasColumnType("jsonb");

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.InteractionDate);
                entity.HasIndex(e => e.Direction);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequiresFollowUp);
                entity.HasIndex(e => e.FollowUpDate);
            });

            // Configure CustomerSegment entity
            modelBuilder.Entity<CustomerSegment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Criteria).HasColumnType("jsonb");
                entity.Property(e => e.Tags).HasColumnType("jsonb");

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);

                // Relationships
                entity.HasMany(e => e.Members)
                    .WithOne(m => m.Segment)
                    .HasForeignKey(m => m.SegmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SegmentMember entity
            modelBuilder.Entity<SegmentMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.SegmentId, e.CustomerId }).IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerJourney entity
            modelBuilder.Entity<CustomerJourney>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Substage).HasMaxLength(100);
                entity.Property(e => e.CurrentMilestone).HasMaxLength(200);
                entity.Property(e => e.NextMilestone).HasMaxLength(200);
                entity.Property(e => e.AssignedToId).HasMaxLength(50);
                entity.Property(e => e.NextScheduledActivity).HasColumnType("jsonb");

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Stage);
                entity.HasIndex(e => e.JourneyStartDate);
            });

            // Configure Campaign entity
            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TargetAudience).HasColumnType("jsonb");
                entity.Property(e => e.Content).HasColumnType("jsonb");
                entity.Property(e => e.Metrics).HasColumnType("jsonb");

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartDate);

                // Relationships
                entity.HasMany(e => e.CampaignSegments)
                    .WithOne(cs => cs.Campaign)
                    .HasForeignKey(cs => cs.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CampaignSegment entity
            modelBuilder.Entity<CampaignSegment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.CampaignId);
                entity.HasIndex(e => e.SegmentId);
                entity.HasIndex(e => new { e.CampaignId, e.SegmentId }).IsUnique();

                // Relationships
                entity.HasOne(cs => cs.Segment)
                    .WithMany(s => s.CampaignSegments)
                    .HasForeignKey(cs => cs.SegmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerSurvey entity
            modelBuilder.Entity<CustomerSurvey>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RelatedToType).HasMaxLength(50);
                entity.Property(e => e.Comments).HasMaxLength(2000);
                entity.Property(e => e.FollowUpAssignedToId).HasMaxLength(50);

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.SurveyType);
                entity.HasIndex(e => e.SentDate);
                entity.HasIndex(e => e.CompletedDate);

                // Relationships
                entity.HasMany(e => e.SurveyResponses)
                    .WithOne(r => r.Survey)
                    .HasForeignKey(r => r.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerSurveyResponse entity
            modelBuilder.Entity<CustomerSurveyResponse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Question).HasMaxLength(500);
                entity.Property(e => e.Response).HasMaxLength(2000);

                entity.HasIndex(e => e.SurveyId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => e.SubmittedAt);

                // Relationships
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerVehicle entity
            modelBuilder.Entity<CustomerVehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FinanceCompany).HasMaxLength(200);
                entity.Property(e => e.PrimaryDriver).HasMaxLength(200);

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PurchaseDate);

                // Relationship with Customer
                entity.HasOne(v => v.Customer)
                    .WithMany(c => c.Vehicles)
                    .HasForeignKey(v => v.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CustomerLoyalty entity
            modelBuilder.Entity<CustomerLoyalty>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasIndex(e => e.CustomerId).IsUnique();
                entity.HasIndex(e => e.CurrentTier);
                entity.HasIndex(e => e.EnrollmentDate);
                entity.HasIndex(e => e.IsActive);

                // Relationship with Customer
                entity.HasOne(e => e.Customer)
                    .WithOne()
                    .HasForeignKey<CustomerLoyalty>(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationships with transactions and redemptions
                entity.HasMany(e => e.Transactions)
                    .WithOne(t => t.CustomerLoyalty)
                    .HasForeignKey(t => t.CustomerLoyaltyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Redemptions)
                    .WithOne(r => r.CustomerLoyalty)
                    .HasForeignKey(r => r.CustomerLoyaltyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LoyaltyTransaction entity
            modelBuilder.Entity<LoyaltyTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ReferenceId).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ProcessedBy).HasMaxLength(100);

                entity.HasIndex(e => e.CustomerLoyaltyId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.TransactionType);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.ExpirationDate);
                entity.HasIndex(e => e.RewardId);

                // Relationship with Customer
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with LoyaltyReward (optional)
                entity.HasOne(e => e.Reward)
                    .WithMany(r => r.Transactions)
                    .HasForeignKey(e => e.RewardId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure LoyaltyReward entity
            modelBuilder.Entity<LoyaltyReward>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RewardType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MonetaryValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EligibleTiers).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Terms).HasMaxLength(2000);

                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PointsCost);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.DisplayOrder);

                // Relationships
                entity.HasMany(e => e.Redemptions)
                    .WithOne(r => r.Reward)
                    .HasForeignKey(r => r.RewardId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure LoyaltyRedemption entity
            modelBuilder.Entity<LoyaltyRedemption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RedemptionCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UsedBy).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.ApprovalStatus).HasMaxLength(50);
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);

                entity.HasIndex(e => e.CustomerLoyaltyId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.RewardId);
                entity.HasIndex(e => e.RedemptionCode).IsUnique();
                entity.HasIndex(e => e.RedemptionDate);
                entity.HasIndex(e => e.ExpirationDate);
                entity.HasIndex(e => e.Status);

                // Relationship with Customer
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure LoyaltyTierConfig entity
            modelBuilder.Entity<LoyaltyTierConfig>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PointsMultiplier).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Benefits).IsRequired().HasColumnType("jsonb");
                entity.Property(e => e.Color).IsRequired().HasMaxLength(20);

                entity.HasIndex(e => e.Tier).IsUnique();
                entity.HasIndex(e => e.MinimumPoints);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });
        }
    }
}
