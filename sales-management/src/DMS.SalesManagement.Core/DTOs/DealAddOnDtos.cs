using System;
using DMS.SalesManagement.Core.Models;

namespace DMS.SalesManagement.Core.DTOs
{
    /// <summary>
    /// DTO for DealAddOn entity
    /// </summary>
    public class DealAddOnDto
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public AddOnType Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Term { get; set; }
    }

    /// <summary>
    /// DTO for creating a new deal add-on
    /// </summary>
    public class CreateDealAddOnDto
    {
        public AddOnType Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public string? Term { get; set; }
        public Guid? ProviderId { get; set; }
    }
}
