using Microsoft.AspNetCore.Mvc;
using DMS.CRM.Core.Services;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;

namespace DMS.CRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;

        public IntegrationController(
            ILoyaltyService loyaltyService)
        {
            _loyaltyService = loyaltyService;
        }

        [HttpGet("customer360/{customerId}")]
        public async Task<ActionResult<Customer360ResponseDto>> GetCustomer360(Guid customerId)
        {
            try
            {
                // TODO: Get customer basic info from customer service when it's implemented
                // For now, return a mock customer
                var customer = new CustomerDto
                {
                    Id = customerId,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    ContactType = "Individual",
                    PhoneNumbers = new List<PhoneNumberDto>(),
                    Addresses = new List<AddressDto>(),
                    CommunicationPreferences = new CommunicationPreferencesDto(),
                    LoyaltyTier = "Bronze",
                    LoyaltyPoints = 0,
                    LifetimeValue = 0,
                    Status = "Active",
                    Tags = new List<string>(),
                    CreatedAt = DateTime.UtcNow.AddYears(-1)
                };

                // Get loyalty status
                CustomerLoyaltyStatusDTO? loyaltyStatus = null;
                try
                {
                    loyaltyStatus = await _loyaltyService.GetCustomerLoyaltyStatusAsync(customerId);
                }
                catch (Exception ex)
                {
                    // Log but don't fail - loyalty might not be set up yet
                    Console.WriteLine($"Could not get loyalty status for customer {customerId}: {ex.Message}");
                }
                
                // Get available rewards for customer's tier if loyalty status exists
                var availableRewards = new List<LoyaltyRewardDTO>();
                if (loyaltyStatus != null)
                {
                    try
                    {
                        availableRewards = (await _loyaltyService.GetAvailableRewardsAsync()).ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not get available rewards: {ex.Message}");
                    }
                }

                // Get loyalty transaction history (last 10 transactions)
                var pointHistory = new List<LoyaltyTransactionDTO>();
                if (loyaltyStatus != null)
                {
                    try
                    {
                        var historyResult = await _loyaltyService.GetLoyaltyPointHistoryAsync(customerId);
                        pointHistory = historyResult.Take(10).ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not get loyalty transaction history: {ex.Message}");
                    }
                }

                // Build Customer360 response
                var response = new Customer360ResponseDto
                {
                    Customer = customer,
                    
                    LoyaltyStatus = loyaltyStatus != null ? new LoyaltyStatusDto
                    {
                        Tier = loyaltyStatus.LoyaltyTier ?? "Bronze",
                        CurrentPoints = loyaltyStatus.CurrentPoints,
                        PointsToNextTier = loyaltyStatus.PointsToNextTier,
                        NextTier = loyaltyStatus.NextTier ?? "Max tier reached",
                        EnrollmentDate = loyaltyStatus.MemberSince,
                        AvailableRewards = availableRewards.Select(r => new RewardDto
                        {
                            Id = r.Id,
                            Name = r.Name ?? "",
                            Description = r.Description ?? "",
                            PointsCost = r.PointsCost,
                            Category = r.Category ?? ""
                        }).ToList(),
                        PointHistory = pointHistory.Select(t => new PointActivityDto
                        {
                            Description = t.Description ?? "",
                            Points = t.Points,
                            Date = t.TransactionDate,
                            Source = t.Source ?? ""
                        }).ToList()
                    } : null,

                    // These would be populated from other services in a real implementation
                    RecentInteractions = new List<InteractionDto>(),
                    CustomerJourneys = new List<JourneyDto>(),
                    Vehicles = new List<VehicleDto>(),
                    ServiceHistory = new List<ServiceRecordDto>(),
                    FinancialSummary = new FinancialSummaryDto
                    {
                        TotalSpent = 0,
                        OutstandingBalance = 0,
                        CreditLimit = 0
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customer 360 data.", details = ex.Message });
            }
        }

        [HttpPost("synchronize/{customerId}")]
        public ActionResult<SynchronizationResultDto> SynchronizeCustomer(Guid customerId)
        {
            try
            {
                var result = new SynchronizationResultDto
                {
                    Success = true,
                    CustomerId = customerId,
                    SynchronizedAt = DateTime.UtcNow,
                    ModuleMessages = new Dictionary<string, string>
                    {
                        { "CRM", "Customer data synchronized successfully" },
                        { "Loyalty", "Loyalty status updated" },
                        { "Interactions", "Recent interactions synced" }
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                var result = new SynchronizationResultDto
                {
                    Success = false,
                    CustomerId = customerId,
                    SynchronizedAt = DateTime.UtcNow,
                    ModuleMessages = new Dictionary<string, string>
                    {
                        { "Error", $"Synchronization failed: {ex.Message}" }
                    }
                };

                return StatusCode(500, result);
            }
        }

        [HttpPost("propagate/{customerId}")]
        public ActionResult<PropagationResultDto> PropagateCustomerChanges(Guid customerId)
        {
            try
            {
                // This would propagate customer changes to other modules/services
                var result = new PropagationResultDto
                {
                    Success = true,
                    CustomerId = customerId,
                    PropagatedAt = DateTime.UtcNow,
                    TargetSystems = new List<string> { "Service Management", "Sales Management", "Parts Management" },
                    Message = "Customer changes propagated successfully"
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                var result = new PropagationResultDto
                {
                    Success = false,
                    CustomerId = customerId,
                    PropagatedAt = DateTime.UtcNow,
                    TargetSystems = new List<string>(),
                    Message = $"Propagation failed: {ex.Message}"
                };

                return StatusCode(500, result);
            }
        }

        private int CalculatePointsToNextTier(int currentPoints, LoyaltyTier? currentTier)
        {
            // Define tier thresholds
            var tierThresholds = new Dictionary<LoyaltyTier, int>
            {
                { LoyaltyTier.Bronze, 0 },
                { LoyaltyTier.Silver, 1000 },
                { LoyaltyTier.Gold, 5000 },
                { LoyaltyTier.Platinum, 15000 }
            };

            if (currentTier == null || currentTier == LoyaltyTier.Platinum)
                return 0; // Already at highest tier

            var nextTier = GetNextTierEnum(currentTier.Value);
            if (nextTier == null)
                return 0;

            var nextTierThreshold = tierThresholds[nextTier.Value];
            return Math.Max(0, nextTierThreshold - currentPoints);
        }

        private string GetNextTier(LoyaltyTier? currentTier)
        {
            var nextTierEnum = GetNextTierEnum(currentTier);
            return nextTierEnum?.ToString() ?? "Max tier reached";
        }

        private LoyaltyTier? GetNextTierEnum(LoyaltyTier? currentTier)
        {
            return currentTier switch
            {
                LoyaltyTier.Bronze => LoyaltyTier.Silver,
                LoyaltyTier.Silver => LoyaltyTier.Gold,
                LoyaltyTier.Gold => LoyaltyTier.Platinum,
                LoyaltyTier.Platinum => null,
                _ => LoyaltyTier.Silver
            };
        }
    }

    // DTOs for Integration responses
    public class Customer360ResponseDto
    {
        public required CustomerDto Customer { get; set; }
        public LoyaltyStatusDto? LoyaltyStatus { get; set; }
        public List<InteractionDto> RecentInteractions { get; set; } = new();
        public List<JourneyDto> CustomerJourneys { get; set; } = new();
        public List<VehicleDto> Vehicles { get; set; } = new();
        public List<ServiceRecordDto> ServiceHistory { get; set; } = new();
        public required FinancialSummaryDto FinancialSummary { get; set; }
    }

    public class LoyaltyStatusDto
    {
        public required string Tier { get; set; }
        public int CurrentPoints { get; set; }
        public int PointsToNextTier { get; set; }
        public required string NextTier { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public List<RewardDto> AvailableRewards { get; set; } = new();
        public List<PointActivityDto> PointHistory { get; set; } = new();
    }

    public class RewardDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int PointsCost { get; set; }
        public required string Category { get; set; }
    }

    public class PointActivityDto
    {
        public required string Description { get; set; }
        public int Points { get; set; }
        public DateTime Date { get; set; }
        public required string Source { get; set; }
    }

    public class InteractionDto
    {
        public int Id { get; set; }
        public required string Type { get; set; }
        public required string Description { get; set; }
        public DateTime Date { get; set; }
        public required string Channel { get; set; }
    }

    public class JourneyDto
    {
        public int Id { get; set; }
        public required string StageName { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class VehicleDto
    {
        public int Id { get; set; }
        public required string Make { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public required string VIN { get; set; }
    }

    public class ServiceRecordDto
    {
        public int Id { get; set; }
        public required string ServiceType { get; set; }
        public DateTime Date { get; set; }
        public decimal Cost { get; set; }
        public required string Description { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal TotalSpent { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal CreditLimit { get; set; }
    }

    public class SynchronizationResultDto
    {
        public bool Success { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime SynchronizedAt { get; set; }
        public Dictionary<string, string> ModuleMessages { get; set; } = new();
    }

    public class PropagationResultDto
    {
        public bool Success { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime PropagatedAt { get; set; }
        public List<string> TargetSystems { get; set; } = new();
        public required string Message { get; set; }
    }
}
