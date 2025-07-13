using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Repositories;
using DMS.CRM.Core.Services;
using Microsoft.Extensions.Logging;

namespace DMS.CRM.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for managing customer loyalty program
    /// </summary>
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ICustomerLoyaltyRepository _customerLoyaltyRepository;
        private readonly ILoyaltyTransactionRepository _loyaltyTransactionRepository;
        private readonly ILoyaltyRewardRepository _loyaltyRewardRepository;
        private readonly ILoyaltyRedemptionRepository _loyaltyRedemptionRepository;
        private readonly ILoyaltyTierConfigRepository _loyaltyTierConfigRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<LoyaltyService> _logger;

        public LoyaltyService(
            ICustomerLoyaltyRepository customerLoyaltyRepository,
            ILoyaltyTransactionRepository loyaltyTransactionRepository,
            ILoyaltyRewardRepository loyaltyRewardRepository,
            ILoyaltyRedemptionRepository loyaltyRedemptionRepository,
            ILoyaltyTierConfigRepository loyaltyTierConfigRepository,
            ICustomerRepository customerRepository,
            ILogger<LoyaltyService> logger)
        {
            _customerLoyaltyRepository = customerLoyaltyRepository;
            _loyaltyTransactionRepository = loyaltyTransactionRepository;
            _loyaltyRewardRepository = loyaltyRewardRepository;
            _loyaltyRedemptionRepository = loyaltyRedemptionRepository;
            _loyaltyTierConfigRepository = loyaltyTierConfigRepository;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CustomerLoyaltyStatusDTO> GetCustomerLoyaltyStatusAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            var customerLoyalty = await _customerLoyaltyRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            
            if (customerLoyalty == null)
            {
                // Initialize loyalty for new customer
                customerLoyalty = await InitializeCustomerLoyaltyAsync(customerId, cancellationToken);
            }

            var nextTierConfig = await _loyaltyTierConfigRepository.GetNextTierAsync(customerLoyalty.CurrentTier, cancellationToken);
            var currentTierConfig = await _loyaltyTierConfigRepository.GetByTierAsync(customerLoyalty.CurrentTier, cancellationToken);
            
            var expiringPoints = await _loyaltyTransactionRepository.GetExpiringPointsAsync(
                customerId, DateTime.UtcNow.AddDays(30), cancellationToken);
            
            var availableRewards = await _loyaltyRewardRepository.GetAffordableRewardsAsync(
                customerLoyalty.CurrentPoints, customerLoyalty.CurrentTier, cancellationToken);

            var benefits = currentTierConfig != null ? 
                System.Text.Json.JsonSerializer.Deserialize<List<LoyaltyBenefitDTO>>(currentTierConfig.Benefits) ?? new List<LoyaltyBenefitDTO>() :
                new List<LoyaltyBenefitDTO>();

            return new CustomerLoyaltyStatusDTO
            {
                CustomerId = customerId,
                LoyaltyTier = customerLoyalty.CurrentTier.ToString(),
                CurrentPoints = customerLoyalty.CurrentPoints,
                LifetimePoints = customerLoyalty.LifetimePointsEarned,
                MemberSince = customerLoyalty.EnrollmentDate,
                TierExpirationDate = customerLoyalty.TierExpirationDate,
                PointsToNextTier = nextTierConfig?.MinimumPoints - customerLoyalty.CurrentPoints ?? 0,
                NextTier = nextTierConfig?.Name ?? "Max Tier Reached",
                PointsExpiringSoon = expiringPoints.Sum(ep => ep.Points),
                PointsExpirationDate = expiringPoints.OrderBy(ep => ep.ExpirationDate).FirstOrDefault()?.ExpirationDate,
                CurrentBenefits = benefits,
                AvailableRewards = availableRewards.Select(MapToLoyaltyRewardDTO).ToList()
            };
        }

        /// <inheritdoc />
        public async Task<int> AddLoyaltyPointsAsync(Guid customerId, int points, string source, string referenceId, CancellationToken cancellationToken = default)
        {
            try
            {
                var customerLoyalty = await _customerLoyaltyRepository.GetByCustomerIdAsync(customerId, cancellationToken);
                
                if (customerLoyalty == null)
                {
                    customerLoyalty = await InitializeCustomerLoyaltyAsync(customerId, cancellationToken);
                }

                // Apply tier multiplier
                var tierConfig = await _loyaltyTierConfigRepository.GetByTierAsync(customerLoyalty.CurrentTier, cancellationToken);
                var adjustedPoints = (int)(points * (tierConfig?.PointsMultiplier ?? 1.0m));

                // Create transaction
                var transaction = new LoyaltyTransaction
                {
                    Id = Guid.NewGuid(),
                    CustomerLoyaltyId = customerLoyalty.Id,
                    CustomerId = customerId,
                    TransactionType = LoyaltyTransactionType.Earned,
                    Points = adjustedPoints,
                    PointsBalance = customerLoyalty.CurrentPoints + adjustedPoints,
                    Source = source,
                    ReferenceId = referenceId,
                    Description = $"Points earned from {source}",
                    TransactionDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddYears(1), // Points expire in 1 year
                    CreatedAt = DateTime.UtcNow
                };

                await _loyaltyTransactionRepository.AddAsync(transaction, cancellationToken);

                // Update customer loyalty
                customerLoyalty.CurrentPoints += adjustedPoints;
                customerLoyalty.LifetimePointsEarned += adjustedPoints;
                customerLoyalty.LastActivityDate = DateTime.UtcNow;
                customerLoyalty.UpdatedAt = DateTime.UtcNow;

                // Check for tier upgrade
                await CheckAndUpdateTierAsync(customerLoyalty, cancellationToken);

                await _customerLoyaltyRepository.UpdateAsync(customerLoyalty, cancellationToken);

                _logger.LogInformation("Added {Points} loyalty points to customer {CustomerId}", adjustedPoints, customerId);

                return customerLoyalty.CurrentPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loyalty points for customer {CustomerId}", customerId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<LoyaltyRedemptionResultDTO> RedeemPointsAsync(Guid customerId, Guid rewardId, CancellationToken cancellationToken = default)
        {
            try
            {
                var customerLoyalty = await _customerLoyaltyRepository.GetByCustomerIdAsync(customerId, cancellationToken);
                var reward = await _loyaltyRewardRepository.GetByIdAsync(rewardId, cancellationToken);

                if (customerLoyalty == null)
                    throw new InvalidOperationException("Customer loyalty record not found");

                if (reward == null)
                    throw new InvalidOperationException("Reward not found");

                if (reward.Status != LoyaltyRewardStatus.Active)
                    throw new InvalidOperationException("Reward is not active");

                if (customerLoyalty.CurrentPoints < reward.PointsCost)
                    throw new InvalidOperationException("Insufficient points for redemption");

                // Check tier eligibility
                var tierName = customerLoyalty.CurrentTier.ToString();
                if (!reward.EligibleTiers.Contains("All") && !reward.EligibleTiers.Contains($"\"{tierName}\""))
                    throw new InvalidOperationException("Customer tier not eligible for this reward");

                // Check quantity availability
                if (reward.QuantityAvailable.HasValue && reward.QuantityRedeemed >= reward.QuantityAvailable)
                    throw new InvalidOperationException("Reward is no longer available");

                // Generate redemption code
                var redemptionCode = GenerateRedemptionCode();

                // Create redemption
                var redemption = new LoyaltyRedemption
                {
                    Id = Guid.NewGuid(),
                    CustomerLoyaltyId = customerLoyalty.Id,
                    CustomerId = customerId,
                    RewardId = rewardId,
                    PointsRedeemed = reward.PointsCost,
                    RedemptionDate = DateTime.UtcNow,
                    RedemptionCode = redemptionCode,
                    ExpirationDate = DateTime.UtcNow.AddDays(reward.ExpirationDays),
                    Status = reward.RequiresApproval ? RedeemedRewardStatus.Pending : RedeemedRewardStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                await _loyaltyRedemptionRepository.AddAsync(redemption, cancellationToken);

                // Create transaction
                var transaction = new LoyaltyTransaction
                {
                    Id = Guid.NewGuid(),
                    CustomerLoyaltyId = customerLoyalty.Id,
                    CustomerId = customerId,
                    TransactionType = LoyaltyTransactionType.Redeemed,
                    Points = -reward.PointsCost,
                    PointsBalance = customerLoyalty.CurrentPoints - reward.PointsCost,
                    Source = "Reward Redemption",
                    ReferenceId = redemption.Id.ToString(),
                    Description = $"Redeemed: {reward.Name}",
                    TransactionDate = DateTime.UtcNow,
                    RewardId = rewardId,
                    CreatedAt = DateTime.UtcNow
                };

                await _loyaltyTransactionRepository.AddAsync(transaction, cancellationToken);

                // Update customer loyalty
                customerLoyalty.CurrentPoints -= reward.PointsCost;
                customerLoyalty.LifetimePointsRedeemed += reward.PointsCost;
                customerLoyalty.LastActivityDate = DateTime.UtcNow;
                customerLoyalty.UpdatedAt = DateTime.UtcNow;

                await _customerLoyaltyRepository.UpdateAsync(customerLoyalty, cancellationToken);

                // Update reward quantity
                await _loyaltyRewardRepository.UpdateQuantityRedeemedAsync(rewardId, 1, cancellationToken);

                _logger.LogInformation("Customer {CustomerId} redeemed reward {RewardId} for {Points} points", 
                    customerId, rewardId, reward.PointsCost);

                return new LoyaltyRedemptionResultDTO
                {
                    RedemptionId = redemption.Id,
                    CustomerId = customerId,
                    RewardId = rewardId,
                    RewardName = reward.Name,
                    RedemptionDate = redemption.RedemptionDate,
                    PointsRedeemed = reward.PointsCost,
                    RemainingPoints = customerLoyalty.CurrentPoints,
                    Success = true,
                    RedemptionCode = redemptionCode,
                    ExpirationDate = redemption.ExpirationDate,
                    RedemptionInstructions = reward.Terms ?? "Please present this code at time of use",
                    Status = redemption.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming points for customer {CustomerId}", customerId);
                
                return new LoyaltyRedemptionResultDTO
                {
                    CustomerId = customerId,
                    RewardId = rewardId,
                    Success = false,
                    RedemptionInstructions = ex.Message
                };
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTransactionDTO>> GetLoyaltyPointHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            var transactions = await _loyaltyTransactionRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            
            return transactions.Select(t => new LoyaltyTransactionDTO
            {
                Id = t.Id,
                CustomerId = t.CustomerId,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType.ToString(),
                Points = t.Points,
                Source = t.Source,
                ReferenceId = t.ReferenceId ?? string.Empty,
                Description = t.Description ?? string.Empty,
                RewardId = t.RewardId,
                PointsBalance = t.PointsBalance
            });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRewardDTO>> GetAvailableRewardsAsync(string? tierLevel = null, CancellationToken cancellationToken = default)
        {
            var rewards = await _loyaltyRewardRepository.GetActiveRewardsAsync(cancellationToken);
            
            if (!string.IsNullOrEmpty(tierLevel) && Enum.TryParse<LoyaltyTier>(tierLevel, out var tier))
            {
                rewards = await _loyaltyRewardRepository.GetEligibleForTierAsync(tier, cancellationToken);
            }

            return rewards.Select(MapToLoyaltyRewardDTO);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RedeemedRewardDTO>> GetRedeemedRewardsAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            var redemptions = await _loyaltyRedemptionRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            
            return redemptions.Select(r => new RedeemedRewardDTO
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                RewardId = r.RewardId,
                RewardName = r.Reward?.Name ?? "Unknown Reward",
                RedemptionDate = r.RedemptionDate,
                PointsRedeemed = r.PointsRedeemed,
                RedemptionCode = r.RedemptionCode,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status.ToString(),
                UsedDate = r.UsedDate
            });
        }

        /// <inheritdoc />
        public async Task<CustomerLoyaltyStatusDTO> UpdateLoyaltyTierAsync(Guid customerId, string newTier, string reason, CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<LoyaltyTier>(newTier, out var tier))
                throw new ArgumentException("Invalid tier level", nameof(newTier));

            var customerLoyalty = await _customerLoyaltyRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            
            if (customerLoyalty == null)
                throw new InvalidOperationException("Customer loyalty record not found");

            var oldTier = customerLoyalty.CurrentTier;
            customerLoyalty.CurrentTier = tier;
            customerLoyalty.TierAchievedDate = DateTime.UtcNow;
            customerLoyalty.UpdatedAt = DateTime.UtcNow;

            // Create adjustment transaction
            var transaction = new LoyaltyTransaction
            {
                Id = Guid.NewGuid(),
                CustomerLoyaltyId = customerLoyalty.Id,
                CustomerId = customerId,
                TransactionType = LoyaltyTransactionType.Adjusted,
                Points = 0,
                PointsBalance = customerLoyalty.CurrentPoints,
                Source = "Tier Update",
                Description = $"Tier changed from {oldTier} to {tier}: {reason}",
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _loyaltyTransactionRepository.AddAsync(transaction, cancellationToken);
            await _customerLoyaltyRepository.UpdateAsync(customerLoyalty, cancellationToken);

            _logger.LogInformation("Updated customer {CustomerId} tier from {OldTier} to {NewTier}: {Reason}", 
                customerId, oldTier, tier, reason);

            return await GetCustomerLoyaltyStatusAsync(customerId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CalculateEarnablePointsAsync(string transactionType, decimal transactionAmount, string customerTier, CancellationToken cancellationToken = default)
        {
            // Base calculation: 1 point per dollar spent
            var basePoints = (int)Math.Floor(transactionAmount);
            
            // Apply tier multiplier
            if (Enum.TryParse<LoyaltyTier>(customerTier, out var tier))
            {
                var tierConfig = await _loyaltyTierConfigRepository.GetByTierAsync(tier, cancellationToken);
                if (tierConfig != null)
                {
                    basePoints = (int)(basePoints * tierConfig.PointsMultiplier);
                }
            }

            // Apply transaction type bonus
            var bonus = transactionType.ToLower() switch
            {
                "service" => 1.5m,
                "parts" => 1.2m,
                "referral" => 2.0m,
                _ => 1.0m
            };

            return (int)(basePoints * bonus);
        }

        private async Task<CustomerLoyalty> InitializeCustomerLoyaltyAsync(Guid customerId, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            var customerLoyalty = new CustomerLoyalty
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                CurrentTier = LoyaltyTier.Bronze,
                CurrentPoints = 0,
                LifetimePointsEarned = 0,
                LifetimePointsRedeemed = 0,
                EnrollmentDate = DateTime.UtcNow,
                TierAchievedDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return await _customerLoyaltyRepository.AddAsync(customerLoyalty, cancellationToken);
        }

        private async Task CheckAndUpdateTierAsync(CustomerLoyalty customerLoyalty, CancellationToken cancellationToken)
        {
            var newTierConfig = await _loyaltyTierConfigRepository.GetTierForPointsAsync(customerLoyalty.CurrentPoints, cancellationToken);
            
            if (newTierConfig != null && newTierConfig.Tier != customerLoyalty.CurrentTier)
            {
                var oldTier = customerLoyalty.CurrentTier;
                customerLoyalty.CurrentTier = newTierConfig.Tier;
                customerLoyalty.TierAchievedDate = DateTime.UtcNow;
                
                _logger.LogInformation("Customer {CustomerId} upgraded from {OldTier} to {NewTier}", 
                    customerLoyalty.CustomerId, oldTier, newTierConfig.Tier);
            }
        }

        private string GenerateRedemptionCode()
        {
            return $"RDM{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
        }

        private LoyaltyRewardDTO MapToLoyaltyRewardDTO(LoyaltyReward reward)
        {
            var eligibleTiers = new List<string>();
            try
            {
                if (reward.EligibleTiers.Contains("All"))
                {
                    eligibleTiers.Add("All");
                }
                else
                {
                    var tiers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reward.EligibleTiers);
                    eligibleTiers = tiers ?? new List<string>();
                }
            }
            catch
            {
                eligibleTiers = new List<string> { "Bronze" }; // Default fallback
            }

            return new LoyaltyRewardDTO
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description,
                PointsCost = reward.PointsCost,
                Category = reward.Category.ToString(),
                RewardType = reward.RewardType,
                MonetaryValue = reward.MonetaryValue,
                IsActive = reward.Status == LoyaltyRewardStatus.Active,
                EligibleTiers = eligibleTiers,
                StartDate = reward.StartDate,
                EndDate = reward.EndDate,
                QuantityAvailable = reward.QuantityAvailable ?? 0,
                QuantityRedeemed = reward.QuantityRedeemed,
                ImageUrl = reward.ImageUrl ?? string.Empty
            };
        }
    }
}
