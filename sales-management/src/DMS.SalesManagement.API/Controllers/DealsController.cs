using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Core.Repositories;

namespace DMS.SalesManagement.API.Controllers
{
    [ApiController]
    [Route("api/sales/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly IDealRepository _dealRepository;

        public DealsController(IDealRepository dealRepository)
        {
            _dealRepository = dealRepository;
        }

        /// <summary>
        /// Gets all deals with optional filtering
        /// </summary>
        /// <param name="status">Optional status filter</param>
        /// <param name="salesRepId">Optional sales rep filter</param>
        /// <param name="customerId">Optional customer filter</param>
        /// <returns>A collection of deals</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DealDto>>> GetDeals(
            [FromQuery] DealStatus? status = null,
            [FromQuery] string? salesRepId = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] Guid? vehicleId = null)
        {
            IEnumerable<Deal> deals;

            if (status.HasValue)
            {
                deals = await _dealRepository.GetByStatusAsync(status.Value);
            }
            else if (!string.IsNullOrEmpty(salesRepId))
            {
                deals = await _dealRepository.GetBySalesRepIdAsync(salesRepId);
            }
            else if (customerId.HasValue)
            {
                deals = await _dealRepository.GetByCustomerIdAsync(customerId.Value);
            }
            else if (vehicleId.HasValue)
            {
                deals = await _dealRepository.GetByVehicleIdAsync(vehicleId.Value);
            }
            else
            {
                deals = await _dealRepository.GetAllAsync();
            }

            var dealDtos = new List<DealDto>();
            
            foreach (var deal in deals)
            {
                dealDtos.Add(MapToDealDto(deal));
            }

            return Ok(dealDtos);
        }

        /// <summary>
        /// Gets a specific deal by ID
        /// </summary>
        /// <param name="id">The deal ID</param>
        /// <param name="includeAllData">Whether to include all related data</param>
        /// <returns>The deal if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DealDto>> GetDeal(Guid id, [FromQuery] bool includeAllData = false)
        {
            Deal? deal;

            if (includeAllData)
            {
                deal = await _dealRepository.GetWithAllRelatedDataAsync(id);
            }
            else
            {
                deal = await _dealRepository.GetByIdAsync(id);
            }

            if (deal == null)
            {
                return NotFound();
            }

            return Ok(MapToDealDto(deal));
        }

        /// <summary>
        /// Creates a new deal
        /// </summary>
        /// <param name="dto">The deal data</param>
        /// <returns>The created deal</returns>
        [HttpPost]
        public async Task<ActionResult<DealDto>> CreateDeal(CreateDealDto dto)
        {
            var deal = new Deal
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                SalesRepId = dto.SalesRepId ?? User.Identity?.Name, // Get from authenticated user if not specified
                Status = DealStatus.Draft,
                DealType = dto.DealType,
                PurchasePrice = dto.PurchasePrice,
                TradeInVehicleId = dto.TradeInVehicleId,
                TradeInValue = dto.TradeInValue,
                DownPayment = dto.DownPayment,
                FinancingTermMonths = dto.FinancingTermMonths,
                FinancingRate = dto.FinancingRate,
                TaxRate = dto.TaxRate,
                Fees = new List<Fee>(),
                StatusHistory = new List<DealStatusHistory>
                {
                    new DealStatusHistory
                    {
                        Status = DealStatus.Draft,
                        Date = DateTime.UtcNow,
                        UserId = User.Identity?.Name,
                        Notes = "Deal created"
                    }
                }
            };

            if (dto.Fees != null)
            {
                foreach (var feeDto in dto.Fees)
                {
                    deal.Fees.Add(new Fee
                    {
                        Type = feeDto.Type,
                        Amount = feeDto.Amount,
                        Description = feeDto.Description
                    });
                }
            }

            // Calculate tax amount based on purchase price and tax rate
            deal.TaxAmount = dto.PurchasePrice * dto.TaxRate;
            
            // Calculate total price
            deal.TotalPrice = dto.PurchasePrice + deal.TaxAmount + deal.Fees.Sum(f => f.Amount) - dto.TradeInValue;
            
            // Calculate monthly payment if financing
            if (dto.DealType == DealType.Finance && dto.FinancingTermMonths > 0 && dto.FinancingRate > 0)
            {
                decimal amountToFinance = deal.TotalPrice - dto.DownPayment;
                decimal monthlyRate = dto.FinancingRate.Value / 12 / 100;
                int months = dto.FinancingTermMonths.Value;
                
                deal.MonthlyPayment = amountToFinance * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, months)) /
                                     ((decimal)Math.Pow(1 + (double)monthlyRate, months) - 1);
            }

            await _dealRepository.AddAsync(deal);
            
            return CreatedAtAction(
                nameof(GetDeal),
                new { id = deal.Id },
                MapToDealDto(deal));
        }

        /// <summary>
        /// Updates an existing deal
        /// </summary>
        /// <param name="id">The deal ID</param>
        /// <param name="dto">The updated deal data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeal(Guid id, UpdateDealDto dto)
        {
            var deal = await _dealRepository.GetByIdAsync(id);

            if (deal == null)
            {
                return NotFound();
            }

            // Only allow updates if deal is still in draft status
            if (deal.Status != DealStatus.Draft)
            {
                return BadRequest("Cannot update a deal that is not in draft status");
            }

            // Update deal properties
            if (dto.SalesRepId != null) deal.SalesRepId = dto.SalesRepId;
            if (dto.DealType.HasValue) deal.DealType = dto.DealType.Value;
            if (dto.PurchasePrice.HasValue) deal.PurchasePrice = dto.PurchasePrice.Value;
            if (dto.TradeInVehicleId.HasValue) deal.TradeInVehicleId = dto.TradeInVehicleId;
            if (dto.TradeInValue.HasValue) deal.TradeInValue = dto.TradeInValue.Value;
            if (dto.DownPayment.HasValue) deal.DownPayment = dto.DownPayment.Value;
            if (dto.FinancingTermMonths.HasValue) deal.FinancingTermMonths = dto.FinancingTermMonths;
            if (dto.FinancingRate.HasValue) deal.FinancingRate = dto.FinancingRate;
            if (dto.TaxRate.HasValue) deal.TaxRate = dto.TaxRate.Value;
            
            if (dto.Fees != null)
            {
                deal.Fees.Clear();
                foreach (var feeDto in dto.Fees)
                {
                    deal.Fees.Add(new Fee
                    {
                        Type = feeDto.Type,
                        Amount = feeDto.Amount,
                        Description = feeDto.Description
                    });
                }
            }

            // Recalculate tax amount, total price, and monthly payment
            deal.TaxAmount = deal.PurchasePrice * deal.TaxRate;
            deal.TotalPrice = deal.PurchasePrice + deal.TaxAmount + deal.Fees.Sum(f => f.Amount) - deal.TradeInValue;
            
            if (deal.DealType == DealType.Finance && deal.FinancingTermMonths > 0 && deal.FinancingRate > 0)
            {
                decimal amountToFinance = deal.TotalPrice - deal.DownPayment;
                decimal monthlyRate = deal.FinancingRate.Value / 12 / 100;
                int months = deal.FinancingTermMonths.Value;
                
                deal.MonthlyPayment = amountToFinance * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, months)) /
                                     ((decimal)Math.Pow(1 + (double)monthlyRate, months) - 1);
            }
            else
            {
                deal.MonthlyPayment = null;
            }

            await _dealRepository.UpdateAsync(deal);

            return NoContent();
        }

        /// <summary>
        /// Calculates deal financial details
        /// </summary>
        /// <param name="id">The deal ID</param>
        /// <param name="dto">The calculation parameters</param>
        /// <returns>The calculation results</returns>
        [HttpPost("{id}/calculate")]
        public ActionResult<DealCalculationResultDto> CalculateDeal(Guid id, CalculateDealDto dto)
        {
            decimal taxAmount = dto.PurchasePrice * dto.TaxRate;
            decimal totalFees = dto.Fees.Sum(f => f.Amount);
            decimal totalPrice = dto.PurchasePrice + taxAmount + totalFees - dto.TradeInValue;
            decimal amountToFinance = totalPrice - dto.DownPayment;
            
            decimal? monthlyPayment = null;
            
            if (dto.FinancingTermMonths.HasValue && dto.FinancingRate.HasValue && 
                dto.FinancingTermMonths.Value > 0 && dto.FinancingRate.Value > 0)
            {
                decimal monthlyRate = dto.FinancingRate.Value / 12 / 100;
                int months = dto.FinancingTermMonths.Value;
                
                monthlyPayment = amountToFinance * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, months)) /
                               ((decimal)Math.Pow(1 + (double)monthlyRate, months) - 1);
            }
            
            return Ok(new DealCalculationResultDto
            {
                TaxAmount = taxAmount,
                TotalFees = totalFees,
                TotalPrice = totalPrice,
                MonthlyPayment = monthlyPayment,
                AmountToFinance = amountToFinance
            });
        }

        /// <summary>
        /// Updates the status of a deal
        /// </summary>
        /// <param name="id">The deal ID</param>
        /// <param name="dto">The status update data</param>
        /// <returns>The updated deal</returns>
        [HttpPost("{id}/status")]
        public async Task<ActionResult<DealDto>> UpdateDealStatus(Guid id, DealStatusUpdateDto dto)
        {
            var statusHistory = new DealStatusHistory
            {
                Status = dto.Status,
                Date = DateTime.UtcNow,
                UserId = User.Identity?.Name,
                Notes = dto.Notes
            };

            try
            {
                var updatedDeal = await _dealRepository.AddStatusHistoryAsync(id, statusHistory);
                return Ok(MapToDealDto(updatedDeal));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Maps a Deal entity to a DealDto
        /// </summary>
        private static DealDto MapToDealDto(Deal deal)
        {
            var dto = new DealDto
            {
                Id = deal.Id,
                CustomerId = deal.CustomerId,
                VehicleId = deal.VehicleId,
                SalesRepId = deal.SalesRepId,
                Status = deal.Status,
                DealType = deal.DealType,
                PurchasePrice = deal.PurchasePrice,
                TradeInVehicleId = deal.TradeInVehicleId,
                TradeInValue = deal.TradeInValue,
                DownPayment = deal.DownPayment,
                FinancingTermMonths = deal.FinancingTermMonths,
                FinancingRate = deal.FinancingRate,
                MonthlyPayment = deal.MonthlyPayment,
                TaxRate = deal.TaxRate,
                TaxAmount = deal.TaxAmount,
                TotalPrice = deal.TotalPrice,
                CreatedAt = deal.CreatedAt,
                Fees = new List<FeeDto>(),
                StatusHistory = new List<DealStatusHistoryDto>(),
                AddOns = new List<DealAddOnDto>()
            };

            foreach (var fee in deal.Fees)
            {
                dto.Fees.Add(new FeeDto
                {
                    Id = fee.Id,
                    Type = fee.Type,
                    Amount = fee.Amount,
                    Description = fee.Description
                });
            }

            foreach (var history in deal.StatusHistory)
            {
                dto.StatusHistory.Add(new DealStatusHistoryDto
                {
                    Id = history.Id,
                    Status = history.Status,
                    Date = history.Date,
                    UserId = history.UserId,
                    Notes = history.Notes
                });
            }

            // Include add-ons if they're loaded
            if (deal.AddOns != null)
            {
                foreach (var addOn in deal.AddOns)
                {
                    dto.AddOns.Add(new DealAddOnDto
                    {
                        Id = addOn.Id,
                        DealId = addOn.DealId,
                        Type = addOn.Type,
                        Name = addOn.Name,
                        Description = addOn.Description,
                        Price = addOn.Price,
                        Term = addOn.Term
                    });
                }
            }

            return dto;
        }
    }
}
