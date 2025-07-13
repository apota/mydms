using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public enum PriceSource
    {
        Manufacturer,
        Manual,
        Formula
    }

    public class PriceHistoryEntry
    {
        public DateTime EffectiveDate { get; set; }
        public decimal Cost { get; set; }
        public decimal RetailPrice { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class PartPricing
    {
        public Guid Id { get; set; }
        public Guid PartId { get; set; }
        public decimal Cost { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal? SpecialPrice { get; set; }
        public DateTime? SpecialPriceStartDate { get; set; }
        public DateTime? SpecialPriceEndDate { get; set; }
        public decimal Markup { get; set; }
        public decimal Margin { get; set; }
        public PriceSource PriceSource { get; set; }
        public List<PriceHistoryEntry> PriceHistory { get; set; } = new List<PriceHistoryEntry>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Part? Part { get; set; }
    }
}
