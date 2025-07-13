using System;

namespace DMS.SalesManagement.Core.DTOs
{
    public class CommissionDto
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CalculationMethod { get; set; }
        public string Status { get; set; }
        public DateTime? PayoutDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    public class CommissionStatusUpdateDto
    {
        public string Status { get; set; }
    }
    
    public class CommissionCalculateDto
    {
        public Guid DealId { get; set; }
    }
}
