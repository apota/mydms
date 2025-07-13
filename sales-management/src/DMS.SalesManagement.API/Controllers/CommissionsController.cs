using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Core.Repositories;

namespace DMS.SalesManagement.API.Controllers
{
    [ApiController]
    [Route("api/sales/[controller]")]
    public class CommissionsController : ControllerBase
    {
        private readonly ICommissionRepository _commissionRepository;

        public CommissionsController(ICommissionRepository commissionRepository)
        {
            _commissionRepository = commissionRepository;
        }

        /// <summary>
        /// Gets all commissions for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of commissions</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CommissionDto>>> GetUserCommissions(string userId)
        {
            var commissions = await _commissionRepository.GetByUserIdAsync(userId);
            
            var commissionDtos = new List<CommissionDto>();
            foreach (var commission in commissions)
            {
                commissionDtos.Add(MapToDto(commission));
            }

            return Ok(commissionDtos);
        }

        /// <summary>
        /// Gets all commissions for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <returns>List of commissions</returns>
        [HttpGet("deal/{dealId}")]
        public async Task<ActionResult<IEnumerable<CommissionDto>>> GetDealCommissions(Guid dealId)
        {
            var commissions = await _commissionRepository.GetByDealIdAsync(dealId);
            
            var commissionDtos = new List<CommissionDto>();
            foreach (var commission in commissions)
            {
                commissionDtos.Add(MapToDto(commission));
            }

            return Ok(commissionDtos);
        }

        /// <summary>
        /// Calculates commissions for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <returns>List of calculated commissions</returns>
        [HttpPost("calculate")]
        public async Task<ActionResult<IEnumerable<CommissionDto>>> CalculateCommissions(Guid dealId)
        {
            var commissions = await _commissionRepository.CalculateCommissionsForDealAsync(dealId);
            
            var commissionDtos = new List<CommissionDto>();
            foreach (var commission in commissions)
            {
                commissionDtos.Add(MapToDto(commission));
            }

            return Ok(commissionDtos);
        }

        /// <summary>
        /// Updates a commission status
        /// </summary>
        /// <param name="id">Commission ID</param>
        /// <param name="updateDto">Status update details</param>
        /// <returns>Updated commission</returns>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<CommissionDto>> UpdateCommissionStatus(
            Guid id, CommissionStatusUpdateDto updateDto)
        {
            var commission = await _commissionRepository.GetByIdAsync(id);
            if (commission == null)
                return NotFound();

            commission.Status = Enum.Parse<CommissionStatus>(updateDto.Status);
            
            if (updateDto.Status == CommissionStatus.Paid.ToString())
            {
                commission.PayoutDate = DateTime.UtcNow;
            }
            
            commission.UpdatedAt = DateTime.UtcNow;

            await _commissionRepository.UpdateAsync(commission);

            return Ok(MapToDto(commission));
        }

        private static CommissionDto MapToDto(Commission commission)
        {
            return new CommissionDto
            {
                Id = commission.Id,
                DealId = commission.DealId,
                UserId = commission.UserId,
                Role = commission.Role.ToString(),
                BaseAmount = commission.BaseAmount,
                BonusAmount = commission.BonusAmount,
                TotalAmount = commission.TotalAmount,
                CalculationMethod = commission.CalculationMethod,
                Status = commission.Status.ToString(),
                PayoutDate = commission.PayoutDate,
                CreatedAt = commission.CreatedAt,
                UpdatedAt = commission.UpdatedAt
            };
        }
    }
}
