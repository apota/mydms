using System;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Repositories;
using DMS.CRM.Core.Services;
using DMS.CRM.Infrastructure.Data;
using DMS.CRM.Infrastructure.Data.Repositories;
using DMS.CRM.Infrastructure.Data.Seeds;
using DMS.CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DMS.CRM.Tests.Services
{
    /// <summary>
    /// Integration tests for LoyaltyService
    /// </summary>
    public class LoyaltyServiceIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly CrmDbContext _dbContext;
        private readonly ILoyaltyService _loyaltyService;

        public LoyaltyServiceIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            // Add DbContext with in-memory database
            services.AddDbContext<CrmDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
            // Add repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerLoyaltyRepository, CustomerLoyaltyRepository>();
            services.AddScoped<ILoyaltyTransactionRepository, LoyaltyTransactionRepository>();
            services.AddScoped<ILoyaltyRewardRepository, LoyaltyRewardRepository>();
            services.AddScoped<ILoyaltyRedemptionRepository, LoyaltyRedemptionRepository>();
            services.AddScoped<ILoyaltyTierConfigRepository, LoyaltyTierConfigRepository>();
            
            // Add service
            services.AddScoped<ILoyaltyService, LoyaltyService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<CrmDbContext>();
            _loyaltyService = _serviceProvider.GetRequiredService<ILoyaltyService>();
            
            // Initialize database
            InitializeDatabaseAsync().Wait();
        }

        private async Task InitializeDatabaseAsync()
        {
            await _dbContext.Database.EnsureCreatedAsync();
            await LoyaltySeedData.SeedLoyaltyDataAsync(_serviceProvider);
            
            // Add a test customer
            var customer = new Customer
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Status = CustomerStatus.Active,
                ContactType = CustomerType.Individual,
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetCustomerLoyaltyStatus_NewCustomer_InitializesLoyalty()
        {
            // Arrange
            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            // Act
            var status = await _loyaltyService.GetCustomerLoyaltyStatusAsync(customerId);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(customerId, status.CustomerId);
            Assert.Equal("Bronze", status.LoyaltyTier);
            Assert.Equal(0, status.CurrentPoints);
            Assert.Equal(0, status.LifetimePoints);
            Assert.True(status.MemberSince <= DateTime.UtcNow);
        }

        [Fact]
        public async Task AddLoyaltyPoints_ValidRequest_AddsPointsAndUpdatesBalance()
        {
            // Arrange
            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var pointsToAdd = 100;
            var source = "Test Purchase";
            var referenceId = "TEST-001";

            // Act
            var newBalance = await _loyaltyService.AddLoyaltyPointsAsync(customerId, pointsToAdd, source, referenceId);

            // Assert
            Assert.Equal(pointsToAdd, newBalance);
            
            // Verify transaction was created
            var status = await _loyaltyService.GetCustomerLoyaltyStatusAsync(customerId);
            Assert.Equal(pointsToAdd, status.CurrentPoints);
            Assert.Equal(pointsToAdd, status.LifetimePoints);
        }

        [Fact]
        public async Task AddLoyaltyPoints_WithTierMultiplier_AppliesMultiplier()
        {
            // Arrange
            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            
            // First, upgrade to Silver tier manually
            await _loyaltyService.UpdateLoyaltyTierAsync(customerId, "Silver", "Test upgrade");
            
            var pointsToAdd = 100;
            var source = "Test Purchase";
            var referenceId = "TEST-002";

            // Act
            var newBalance = await _loyaltyService.AddLoyaltyPointsAsync(customerId, pointsToAdd, source, referenceId);

            // Assert
            // Silver tier has 1.25x multiplier, so 100 points becomes 125 points
            Assert.Equal(125, newBalance);
        }

        [Fact]
        public async Task RedeemPoints_ValidReward_CreatesRedemption()
        {
            // Arrange
            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            
            // Add enough points
            await _loyaltyService.AddLoyaltyPointsAsync(customerId, 1000, "Setup", "SETUP-001");
            
            // Get a reward to redeem
            var rewards = await _loyaltyService.GetAvailableRewardsAsync();
            var reward = rewards.FirstOrDefault(r => r.PointsCost <= 500);
            Assert.NotNull(reward);

            // Act
            var result = await _loyaltyService.RedeemPointsAsync(customerId, reward.Id);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(reward.Id, result.RewardId);
            Assert.NotEmpty(result.RedemptionCode);
            Assert.True(result.ExpirationDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task RedeemPoints_InsufficientPoints_ReturnsFailure()
        {
            // Arrange
            var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            
            // Get an expensive reward
            var rewards = await _loyaltyService.GetAvailableRewardsAsync();
            var expensiveReward = rewards.OrderByDescending(r => r.PointsCost).First();

            // Act
            var result = await _loyaltyService.RedeemPointsAsync(customerId, expensiveReward.Id);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Insufficient points", result.RedemptionInstructions);
        }

        [Fact]
        public async Task CalculateEarnablePoints_DifferentTransactionTypes_ReturnsCorrectPoints()
        {
            // Arrange
            var transactionAmount = 100m;

            // Act & Assert
            var servicePoints = await _loyaltyService.CalculateEarnablePointsAsync("service", transactionAmount, "Bronze");
            var partsPoints = await _loyaltyService.CalculateEarnablePointsAsync("parts", transactionAmount, "Bronze");
            var regularPoints = await _loyaltyService.CalculateEarnablePointsAsync("regular", transactionAmount, "Bronze");

            // Service has 1.5x bonus, Parts has 1.2x bonus
            Assert.Equal(150, servicePoints); // 100 * 1.5
            Assert.Equal(120, partsPoints);   // 100 * 1.2
            Assert.Equal(100, regularPoints); // 100 * 1.0
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}
