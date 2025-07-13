using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents types of loyalty transactions
    /// </summary>
    public enum LoyaltyTransactionType
    {
        Earned,
        Redeemed,
        Expired,
        Adjusted,
        Reversed
    }

    /// <summary>
    /// Represents the status of a loyalty reward
    /// </summary>
    public enum LoyaltyRewardStatus
    {
        Active,
        Inactive,
        Discontinued,
        ComingSoon
    }

    /// <summary>
    /// Represents the category of loyalty rewards
    /// </summary>
    public enum LoyaltyRewardCategory
    {
        Service,
        Parts,
        Experience,
        Discount,
        GiftCard,
        Merchandise,
        Access
    }

    /// <summary>
    /// Represents the status of a redeemed reward
    /// </summary>
    public enum RedeemedRewardStatus
    {
        Active,
        Used,
        Expired,
        Cancelled,
        Pending
    }

    /// <summary>
    /// Represents a customer's loyalty program membership
    /// </summary>
    public class CustomerLoyalty : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public LoyaltyTier CurrentTier { get; set; } = LoyaltyTier.Bronze;
        public int CurrentPoints { get; set; }
        public int LifetimePointsEarned { get; set; }
        public int LifetimePointsRedeemed { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? TierAchievedDate { get; set; }
        public DateTime? TierExpirationDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
        public virtual ICollection<LoyaltyRedemption> Redemptions { get; set; } = new List<LoyaltyRedemption>();
    }

    /// <summary>
    /// Represents a loyalty transaction (earning or spending points)
    /// </summary>
    public class LoyaltyTransaction : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerLoyaltyId { get; set; }
        public Guid CustomerId { get; set; }
        public LoyaltyTransactionType TransactionType { get; set; }
        public int Points { get; set; }
        public int PointsBalance { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? ReferenceId { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Guid? RewardId { get; set; }
        public string? ProcessedBy { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual CustomerLoyalty? CustomerLoyalty { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual LoyaltyReward? Reward { get; set; }
    }

    /// <summary>
    /// Represents a loyalty reward that customers can redeem
    /// </summary>
    public class LoyaltyReward : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PointsCost { get; set; }
        public LoyaltyRewardCategory Category { get; set; }
        public string RewardType { get; set; } = string.Empty; // Free Service, Discount, Gift Card, etc.
        public decimal MonetaryValue { get; set; }
        public LoyaltyRewardStatus Status { get; set; } = LoyaltyRewardStatus.Active;
        public string EligibleTiers { get; set; } = string.Empty; // JSON array of eligible tiers
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? QuantityAvailable { get; set; }
        public int QuantityRedeemed { get; set; }
        public string? ImageUrl { get; set; }
        public string? Terms { get; set; }
        public int ExpirationDays { get; set; } = 365; // Days until redeemed reward expires
        public bool RequiresApproval { get; set; }
        public int DisplayOrder { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
        public virtual ICollection<LoyaltyRedemption> Redemptions { get; set; } = new List<LoyaltyRedemption>();
    }

    /// <summary>
    /// Represents a redeemed loyalty reward
    /// </summary>
    public class LoyaltyRedemption : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerLoyaltyId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RewardId { get; set; }
        public int PointsRedeemed { get; set; }
        public DateTime RedemptionDate { get; set; }
        public string RedemptionCode { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
        public RedeemedRewardStatus Status { get; set; } = RedeemedRewardStatus.Active;
        public DateTime? UsedDate { get; set; }
        public string? UsedBy { get; set; }
        public string? Notes { get; set; }
        public string? ApprovalStatus { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual CustomerLoyalty? CustomerLoyalty { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual LoyaltyReward? Reward { get; set; }
    }

    /// <summary>
    /// Represents loyalty tier configuration and benefits
    /// </summary>
    public class LoyaltyTierConfig : IAuditableEntity
    {
        public Guid Id { get; set; }
        public LoyaltyTier Tier { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MinimumPoints { get; set; }
        public decimal PointsMultiplier { get; set; } = 1.0m;
        public string Benefits { get; set; } = string.Empty; // JSON array of benefits
        public string Color { get; set; } = "#cd7f32"; // Display color
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
