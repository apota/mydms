using System;

namespace DMS.PartsManagement.Core.Models
{
    public enum CoreStatus
    {
        Sold,
        Returned,
        Credited
    }

    public class PartCoreTracking
    {
        public Guid Id { get; set; }
        public Guid PartId { get; set; }
        public string CorePartNumber { get; set; } = string.Empty;
        public decimal CoreValue { get; set; }
        public CoreStatus Status { get; set; }
        public DateTime? SoldDate { get; set; }
        public Guid? SoldReferenceId { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public Guid? ReturnReferenceId { get; set; }
        public DateTime? CreditedDate { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Part? Part { get; set; }
    }
}
