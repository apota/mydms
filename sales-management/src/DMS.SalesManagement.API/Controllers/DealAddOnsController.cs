using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Infrastructure.Data;

namespace DMS.SalesManagement.API.Controllers
{
    [ApiController]
    [Route("api/sales/deals/{dealId}/addons")]
    public class DealAddOnsController : ControllerBase
    {
        private readonly SalesDbContext _context;

        public DealAddOnsController(SalesDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all add-ons for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <returns>A collection of add-ons</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DealAddOnDto>>> GetDealAddOns(Guid dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.AddOns)
                .FirstOrDefaultAsync(d => d.Id == dealId);

            if (deal == null)
            {
                return NotFound("Deal not found");
            }

            var addOnDtos = deal.AddOns.Select(addOn => new DealAddOnDto
            {
                Id = addOn.Id,
                DealId = addOn.DealId,
                Type = addOn.Type,
                Name = addOn.Name,
                Description = addOn.Description,
                Price = addOn.Price,
                Term = addOn.Term
            }).ToList();

            return Ok(addOnDtos);
        }

        /// <summary>
        /// Gets a specific add-on by ID
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <param name="id">The add-on ID</param>
        /// <returns>The add-on if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DealAddOnDto>> GetDealAddOn(Guid dealId, Guid id)
        {
            var addOn = await _context.DealAddOns
                .FirstOrDefaultAsync(a => a.DealId == dealId && a.Id == id);

            if (addOn == null)
            {
                return NotFound("Add-on not found");
            }

            var dto = new DealAddOnDto
            {
                Id = addOn.Id,
                DealId = addOn.DealId,
                Type = addOn.Type,
                Name = addOn.Name,
                Description = addOn.Description,
                Price = addOn.Price,
                Term = addOn.Term
            };

            return Ok(dto);
        }

        /// <summary>
        /// Adds an add-on to a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <param name="dto">The add-on data</param>
        /// <returns>The created add-on</returns>
        [HttpPost]
        public async Task<ActionResult<DealAddOnDto>> CreateDealAddOn(Guid dealId, CreateDealAddOnDto dto)
        {
            var deal = await _context.Deals.FindAsync(dealId);

            if (deal == null)
            {
                return NotFound("Deal not found");
            }

            // Only allow adding add-ons if deal is in draft or pending status
            if (deal.Status != DealStatus.Draft && deal.Status != DealStatus.Pending)
            {
                return BadRequest("Cannot add add-ons to a deal that is not in draft or pending status");
            }

            var addOn = new DealAddOn
            {
                DealId = dealId,
                Type = dto.Type,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Cost = dto.Cost,
                Term = dto.Term,
                ProviderId = dto.ProviderId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name
            };

            _context.DealAddOns.Add(addOn);
            await _context.SaveChangesAsync();

            // Update the deal's total price
            deal.TotalPrice += addOn.Price;
            await _context.SaveChangesAsync();

            var responseDto = new DealAddOnDto
            {
                Id = addOn.Id,
                DealId = addOn.DealId,
                Type = addOn.Type,
                Name = addOn.Name,
                Description = addOn.Description,
                Price = addOn.Price,
                Term = addOn.Term
            };

            return CreatedAtAction(
                nameof(GetDealAddOn),
                new { dealId = dealId, id = addOn.Id },
                responseDto);
        }

        /// <summary>
        /// Removes an add-on from a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <param name="id">The add-on ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDealAddOn(Guid dealId, Guid id)
        {
            var deal = await _context.Deals.FindAsync(dealId);

            if (deal == null)
            {
                return NotFound("Deal not found");
            }

            // Only allow removing add-ons if deal is in draft or pending status
            if (deal.Status != DealStatus.Draft && deal.Status != DealStatus.Pending)
            {
                return BadRequest("Cannot remove add-ons from a deal that is not in draft or pending status");
            }

            var addOn = await _context.DealAddOns
                .FirstOrDefaultAsync(a => a.DealId == dealId && a.Id == id);

            if (addOn == null)
            {
                return NotFound("Add-on not found");
            }

            _context.DealAddOns.Remove(addOn);
            
            // Update the deal's total price
            deal.TotalPrice -= addOn.Price;
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
