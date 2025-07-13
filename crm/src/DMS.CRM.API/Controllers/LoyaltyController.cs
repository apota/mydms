using Microsoft.AspNetCore.Mvc;
using DMS.CRM.Core.Services;
using DMS.CRM.Core.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.CRM.API.Controllers
{
    /// <summary>
    /// Controller for managing customer loyalty program
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ILogger<LoyaltyController> _logger;

        public LoyaltyController(ILoyaltyService loyaltyService, ILogger<LoyaltyController> logger)
        {
            _loyaltyService = loyaltyService;
            _logger = logger;
        }

        /// <summary>
        /// Gets customer's loyalty status and dashboard information
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer loyalty status</returns>
        [HttpGet("customers/{customerId:guid}/status")]
        public async Task<ActionResult<CustomerLoyaltyStatusDTO>> GetCustomerLoyaltyStatus(
            Guid customerId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var status = await _loyaltyService.GetCustomerLoyaltyStatusAsync(customerId, cancellationToken);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyalty status for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Adds loyalty points to a customer's account
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="request">The add points request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated points balance</returns>
        [HttpPost("customers/{customerId:guid}/points")]
        public async Task<ActionResult<int>> AddLoyaltyPoints(
            Guid customerId, 
            [FromBody] AddPointsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request.Points <= 0)
                    return BadRequest("Points must be greater than zero");

                var newBalance = await _loyaltyService.AddLoyaltyPointsAsync(
                    customerId, 
                    request.Points, 
                    request.Source, 
                    request.ReferenceId, 
                    cancellationToken);

                return Ok(new { pointsAdded = request.Points, newBalance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loyalty points for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Redeems loyalty points for a reward
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="request">The redemption request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Redemption result</returns>
        [HttpPost("customers/{customerId:guid}/redeem")]
        public async Task<ActionResult<LoyaltyRedemptionResultDTO>> RedeemPoints(
            Guid customerId, 
            [FromBody] RedeemPointsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _loyaltyService.RedeemPointsAsync(customerId, request.RewardId, cancellationToken);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming points for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets loyalty point transaction history for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of loyalty point transactions</returns>
        [HttpGet("customers/{customerId:guid}/transactions")]
        public async Task<ActionResult<IEnumerable<LoyaltyTransactionDTO>>> GetLoyaltyPointHistory(
            Guid customerId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var transactions = await _loyaltyService.GetLoyaltyPointHistoryAsync(customerId, cancellationToken);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyalty transaction history for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets available loyalty rewards
        /// </summary>
        /// <param name="tierLevel">Optional tier level filter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available rewards</returns>
        [HttpGet("rewards")]
        public async Task<ActionResult<IEnumerable<LoyaltyRewardDTO>>> GetAvailableRewards(
            [FromQuery] string? tierLevel = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var rewards = await _loyaltyService.GetAvailableRewardsAsync(tierLevel ?? string.Empty, cancellationToken);
                return Ok(rewards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rewards");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets rewards redeemed by a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of redeemed rewards</returns>
        [HttpGet("customers/{customerId:guid}/redemptions")]
        public async Task<ActionResult<IEnumerable<RedeemedRewardDTO>>> GetRedeemedRewards(
            Guid customerId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var redemptions = await _loyaltyService.GetRedeemedRewardsAsync(customerId, cancellationToken);
                return Ok(redemptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting redeemed rewards for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates customer's loyalty tier (admin function)
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="request">The tier update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated loyalty status</returns>
        [HttpPut("customers/{customerId:guid}/tier")]
        public async Task<ActionResult<CustomerLoyaltyStatusDTO>> UpdateLoyaltyTier(
            Guid customerId, 
            [FromBody] UpdateTierRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var status = await _loyaltyService.UpdateLoyaltyTierAsync(
                    customerId, 
                    request.NewTier, 
                    request.Reason, 
                    cancellationToken);

                return Ok(status);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loyalty tier for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Calculates points that would be awarded for a transaction
        /// </summary>
        /// <param name="request">The calculation request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Points that would be awarded</returns>
        [HttpPost("calculate-points")]
        public async Task<ActionResult<int>> CalculateEarnablePoints(
            [FromBody] CalculatePointsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request.TransactionAmount <= 0)
                    return BadRequest("Transaction amount must be greater than zero");

                var points = await _loyaltyService.CalculateEarnablePointsAsync(
                    request.TransactionType, 
                    request.TransactionAmount, 
                    request.CustomerTier, 
                    cancellationToken);

                return Ok(new { earnablePoints = points });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating earnable points");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets loyalty program dashboard data
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dashboard data</returns>
        [HttpGet("dashboard")]
        public Task<ActionResult<LoyaltyDashboardDTO>> GetLoyaltyDashboard(CancellationToken cancellationToken = default)
        {
            try
            {
                // This would typically aggregate data from multiple services
                // For now, return basic structure that frontend can expand on
                var dashboard = new LoyaltyDashboardDTO
                {
                    TotalActiveMembers = 0, // TODO: Implement these metrics
                    TotalPointsIssued = 0,
                    TotalPointsRedeemed = 0,
                    ActiveRewardsCount = 0,
                    PendingRedemptionsCount = 0
                };

                return Task.FromResult<ActionResult<LoyaltyDashboardDTO>>(Ok(dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loyalty dashboard data");
                return Task.FromResult<ActionResult<LoyaltyDashboardDTO>>(StatusCode(500, "Internal server error"));
            }
        }
    }

    // Request/Response DTOs
    public class AddPointsRequest
    {
        public int Points { get; set; }
        public string Source { get; set; } = string.Empty;
        public string ReferenceId { get; set; } = string.Empty;
    }

    public class RedeemPointsRequest
    {
        public Guid RewardId { get; set; }
    }

    public class UpdateTierRequest
    {
        public string NewTier { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class CalculatePointsRequest
    {
        public string TransactionType { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
        public string CustomerTier { get; set; } = string.Empty;
    }

    public class LoyaltyDashboardDTO
    {
        public int TotalActiveMembers { get; set; }
        public int TotalPointsIssued { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int ActiveRewardsCount { get; set; }
        public int PendingRedemptionsCount { get; set; }
    }
}
