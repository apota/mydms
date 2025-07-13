using System;
using System.Text.Json;
using DMS.CRM.Core.Models;
using DMS.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DMS.CRM.Infrastructure.Data.Seeds
{
    /// <summary>
    /// Seed data for loyalty program components
    /// </summary>
    public static class LoyaltySeedData
    {
        /// <summary>
        /// Seeds loyalty tier configurations and sample rewards
        /// </summary>
        public static async Task SeedLoyaltyDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CrmDbContext>>();

            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed Loyalty Tier Configurations
                await SeedLoyaltyTierConfigsAsync(context, logger);

                // Seed Sample Loyalty Rewards
                await SeedLoyaltyRewardsAsync(context, logger);

                await context.SaveChangesAsync();
                logger.LogInformation("Loyalty seed data created successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error seeding loyalty data");
                throw;
            }
        }

        private static async Task SeedLoyaltyTierConfigsAsync(CrmDbContext context, ILogger logger)
        {
            // Check if tier configs already exist
            if (await context.LoyaltyTierConfigs.AnyAsync())
            {
                logger.LogInformation("Loyalty tier configurations already exist, skipping seed");
                return;
            }

            var tierConfigs = new[]
            {
                new LoyaltyTierConfig
                {
                    Id = Guid.NewGuid(),
                    Tier = LoyaltyTier.Bronze,
                    Name = "Bronze",
                    MinimumPoints = 0,
                    PointsMultiplier = 1.0m,
                    Benefits = JsonSerializer.Serialize(new[]
                    {
                        new { BenefitName = "Basic Support", BenefitDescription = "Standard customer support", Category = "Support", TierLevel = "Bronze", IsActive = true },
                        new { BenefitName = "Monthly Newsletter", BenefitDescription = "Monthly automotive tips and news", Category = "Communication", TierLevel = "Bronze", IsActive = true },
                        new { BenefitName = "Birthday Reward", BenefitDescription = "Special offer on your birthday", Category = "Rewards", TierLevel = "Bronze", IsActive = true }
                    }),
                    Color = "#cd7f32",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyTierConfig
                {
                    Id = Guid.NewGuid(),
                    Tier = LoyaltyTier.Silver,
                    Name = "Silver",
                    MinimumPoints = 1000,
                    PointsMultiplier = 1.25m,
                    Benefits = JsonSerializer.Serialize(new[]
                    {
                        new { BenefitName = "Priority Support", BenefitDescription = "Priority customer support queue", Category = "Support", TierLevel = "Silver", IsActive = true },
                        new { BenefitName = "Free Basic Service", BenefitDescription = "One free basic service per year", Category = "Service", TierLevel = "Silver", IsActive = true },
                        new { BenefitName = "Exclusive Offers", BenefitDescription = "Members-only discounts and promotions", Category = "Rewards", TierLevel = "Silver", IsActive = true },
                        new { BenefitName = "Extended Warranty", BenefitDescription = "Extended warranty on parts and service", Category = "Warranty", TierLevel = "Silver", IsActive = true }
                    }),
                    Color = "#c0c0c0",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyTierConfig
                {
                    Id = Guid.NewGuid(),
                    Tier = LoyaltyTier.Gold,
                    Name = "Gold",
                    MinimumPoints = 2500,
                    PointsMultiplier = 1.5m,
                    Benefits = JsonSerializer.Serialize(new[]
                    {
                        new { BenefitName = "VIP Support", BenefitDescription = "Dedicated VIP support representative", Category = "Support", TierLevel = "Gold", IsActive = true },
                        new { BenefitName = "Free Premium Service", BenefitDescription = "One free premium service per year", Category = "Service", TierLevel = "Gold", IsActive = true },
                        new { BenefitName = "Loaner Vehicle", BenefitDescription = "Free loaner vehicle during service", Category = "Service", TierLevel = "Gold", IsActive = true },
                        new { BenefitName = "Early Access", BenefitDescription = "Early access to new vehicles and services", Category = "Access", TierLevel = "Gold", IsActive = true },
                        new { BenefitName = "Concierge Service", BenefitDescription = "Personal automotive concierge", Category = "Service", TierLevel = "Gold", IsActive = true }
                    }),
                    Color = "#ffd700",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyTierConfig
                {
                    Id = Guid.NewGuid(),
                    Tier = LoyaltyTier.Platinum,
                    Name = "Platinum",
                    MinimumPoints = 5000,
                    PointsMultiplier = 2.0m,
                    Benefits = JsonSerializer.Serialize(new[]
                    {
                        new { BenefitName = "White Glove Service", BenefitDescription = "Premium white glove service experience", Category = "Service", TierLevel = "Platinum", IsActive = true },
                        new { BenefitName = "Unlimited Premium Services", BenefitDescription = "Unlimited premium services", Category = "Service", TierLevel = "Platinum", IsActive = true },
                        new { BenefitName = "Luxury Loaner Fleet", BenefitDescription = "Access to luxury loaner vehicle fleet", Category = "Service", TierLevel = "Platinum", IsActive = true },
                        new { BenefitName = "Personal Account Manager", BenefitDescription = "Dedicated personal account manager", Category = "Support", TierLevel = "Platinum", IsActive = true },
                        new { BenefitName = "Exclusive Events", BenefitDescription = "Invitations to exclusive automotive events", Category = "Experience", TierLevel = "Platinum", IsActive = true },
                        new { BenefitName = "VIP Pricing", BenefitDescription = "VIP pricing on all products and services", Category = "Rewards", TierLevel = "Platinum", IsActive = true }
                    }),
                    Color = "#e5e4e2",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyTierConfig
                {
                    Id = Guid.NewGuid(),
                    Tier = LoyaltyTier.Diamond,
                    Name = "Diamond",
                    MinimumPoints = 10000,
                    PointsMultiplier = 3.0m,
                    Benefits = JsonSerializer.Serialize(new[]
                    {
                        new { BenefitName = "Ultimate Service Experience", BenefitDescription = "The ultimate in automotive service experience", Category = "Service", TierLevel = "Diamond", IsActive = true },
                        new { BenefitName = "Concierge Vehicle Management", BenefitDescription = "Complete vehicle lifecycle management", Category = "Service", TierLevel = "Diamond", IsActive = true },
                        new { BenefitName = "Exclusive Fleet Access", BenefitDescription = "Access to exclusive high-end vehicle fleet", Category = "Access", TierLevel = "Diamond", IsActive = true },
                        new { BenefitName = "Executive Account Team", BenefitDescription = "Dedicated executive account management team", Category = "Support", TierLevel = "Diamond", IsActive = true },
                        new { BenefitName = "VIP Event Access", BenefitDescription = "Access to VIP automotive events and launches", Category = "Experience", TierLevel = "Diamond", IsActive = true },
                        new { BenefitName = "Priority Everything", BenefitDescription = "Priority access to all services and products", Category = "Access", TierLevel = "Diamond", IsActive = true },
                        new { BenefitName = "Lifetime Benefits", BenefitDescription = "Guaranteed lifetime tier benefits", Category = "Rewards", TierLevel = "Diamond", IsActive = true }
                    }),
                    Color = "#b9f2ff",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.LoyaltyTierConfigs.AddRangeAsync(tierConfigs);
            logger.LogInformation("Added {Count} loyalty tier configurations", tierConfigs.Length);
        }

        private static async Task SeedLoyaltyRewardsAsync(CrmDbContext context, ILogger logger)
        {
            // Check if rewards already exist
            if (await context.LoyaltyRewards.AnyAsync())
            {
                logger.LogInformation("Loyalty rewards already exist, skipping seed");
                return;
            }

            var rewards = new[]
            {
                // Service Rewards
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "Free Oil Change",
                    Description = "Complimentary oil change service including oil filter replacement",
                    PointsCost = 500,
                    Category = LoyaltyRewardCategory.Service,
                    RewardType = "Free Service",
                    MonetaryValue = 89.99m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Bronze", "Silver", "Gold", "Platinum", "Diamond" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = 1000,
                    QuantityRedeemed = 0,
                    ExpirationDays = 90,
                    RequiresApproval = false,
                    DisplayOrder = 1,
                    Terms = "Valid for standard oil change service. Synthetic oil upgrade available for additional cost.",
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "Free Vehicle Inspection",
                    Description = "Comprehensive 30-point vehicle inspection",
                    PointsCost = 300,
                    Category = LoyaltyRewardCategory.Service,
                    RewardType = "Free Service",
                    MonetaryValue = 79.99m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Bronze", "Silver", "Gold", "Platinum", "Diamond" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = 500,
                    QuantityRedeemed = 0,
                    ExpirationDays = 60,
                    RequiresApproval = false,
                    DisplayOrder = 2,
                    Terms = "Includes comprehensive inspection report. Additional repairs not included.",
                    CreatedAt = DateTime.UtcNow
                },

                // Parts Rewards
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "Free Air Filter",
                    Description = "Premium air filter replacement",
                    PointsCost = 200,
                    Category = LoyaltyRewardCategory.Parts,
                    RewardType = "Free Part",
                    MonetaryValue = 49.99m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Bronze", "Silver", "Gold", "Platinum", "Diamond" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = 300,
                    QuantityRedeemed = 0,
                    ExpirationDays = 120,
                    RequiresApproval = false,
                    DisplayOrder = 3,
                    Terms = "Standard air filter only. Installation included.",
                    CreatedAt = DateTime.UtcNow
                },

                // Discount Rewards
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "10% Off Service",
                    Description = "10% discount on any service appointment",
                    PointsCost = 750,
                    Category = LoyaltyRewardCategory.Discount,
                    RewardType = "Percentage Discount",
                    MonetaryValue = 100.00m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Silver", "Gold", "Platinum" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = null, // Unlimited
                    QuantityRedeemed = 0,
                    ExpirationDays = 30,
                    RequiresApproval = false,
                    DisplayOrder = 4,
                    Terms = "Cannot be combined with other offers. Maximum discount $200.",
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "20% Off Parts",
                    Description = "20% discount on parts purchase",
                    PointsCost = 1000,
                    Category = LoyaltyRewardCategory.Discount,
                    RewardType = "Percentage Discount",
                    MonetaryValue = 150.00m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Gold", "Platinum" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = null, // Unlimited
                    QuantityRedeemed = 0,
                    ExpirationDays = 45,
                    RequiresApproval = false,
                    DisplayOrder = 5,
                    Terms = "Valid on genuine parts only. Installation labor not included.",
                    CreatedAt = DateTime.UtcNow
                },

                // Experience Rewards
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "VIP Service Experience",
                    Description = "Premium service experience with dedicated technician",
                    PointsCost = 2000,
                    Category = LoyaltyRewardCategory.Experience,
                    RewardType = "Premium Experience",
                    MonetaryValue = 299.99m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Gold", "Platinum" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = 100,
                    QuantityRedeemed = 0,
                    ExpirationDays = 60,
                    RequiresApproval = true,
                    DisplayOrder = 6,
                    Terms = "Subject to availability. Must be scheduled in advance.",
                    CreatedAt = DateTime.UtcNow
                },

                // Gift Card Rewards
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "$25 Service Credit",
                    Description = "$25 credit toward any service",
                    PointsCost = 2500,
                    Category = LoyaltyRewardCategory.GiftCard,
                    RewardType = "Service Credit",
                    MonetaryValue = 25.00m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Bronze", "Silver", "Gold", "Platinum", "Diamond" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = null, // Unlimited
                    QuantityRedeemed = 0,
                    ExpirationDays = 365,
                    RequiresApproval = false,
                    DisplayOrder = 7,
                    Terms = "Cannot be exchanged for cash. Valid for one year from redemption.",
                    CreatedAt = DateTime.UtcNow
                },
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "$50 Service Credit",
                    Description = "$50 credit toward any service",
                    PointsCost = 5000,
                    Category = LoyaltyRewardCategory.GiftCard,
                    RewardType = "Service Credit",
                    MonetaryValue = 50.00m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Silver", "Gold", "Platinum" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = null, // Unlimited
                    QuantityRedeemed = 0,
                    ExpirationDays = 365,
                    RequiresApproval = false,
                    DisplayOrder = 8,
                    Terms = "Cannot be exchanged for cash. Valid for one year from redemption.",
                    CreatedAt = DateTime.UtcNow
                },

                // Platinum Exclusive Rewards
                new LoyaltyReward
                {
                    Id = Guid.NewGuid(),
                    Name = "Private Track Day Experience",
                    Description = "Exclusive track day experience at premium racing facility",
                    PointsCost = 10000,
                    Category = LoyaltyRewardCategory.Experience,
                    RewardType = "Exclusive Experience",
                    MonetaryValue = 999.99m,
                    Status = LoyaltyRewardStatus.Active,
                    EligibleTiers = JsonSerializer.Serialize(new[] { "Platinum" }),
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    QuantityAvailable = 20,
                    QuantityRedeemed = 0,
                    ExpirationDays = 90,
                    RequiresApproval = true,
                    DisplayOrder = 9,
                    Terms = "Platinum members only. Subject to availability and weather conditions. Safety briefing required.",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.LoyaltyRewards.AddRangeAsync(rewards);
            logger.LogInformation("Added {Count} loyalty rewards", rewards.Length);
        }
    }
}
