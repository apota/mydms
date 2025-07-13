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
    public class LeadsController : ControllerBase
    {
        private readonly ILeadRepository _leadRepository;

        public LeadsController(ILeadRepository leadRepository)
        {
            _leadRepository = leadRepository;
        }

        /// <summary>
        /// Gets all leads with optional filtering
        /// </summary>
        /// <param name="status">Optional status filter</param>
        /// <param name="salesRepId">Optional sales rep filter</param>
        /// <returns>A collection of leads</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeadDto>>> GetLeads(
            [FromQuery] LeadStatus? status = null,
            [FromQuery] string? salesRepId = null)
        {
            IEnumerable<Lead> leads;

            if (status.HasValue)
            {
                leads = await _leadRepository.GetByStatusAsync(status.Value);
            }
            else if (!string.IsNullOrEmpty(salesRepId))
            {
                leads = await _leadRepository.GetBySalesRepIdAsync(salesRepId);
            }
            else
            {
                leads = await _leadRepository.GetAllAsync();
            }

            var leadDtos = new List<LeadDto>();
            
            foreach (var lead in leads)
            {
                leadDtos.Add(MapToLeadDto(lead));
            }

            return Ok(leadDtos);
        }

        /// <summary>
        /// Gets a specific lead by ID
        /// </summary>
        /// <param name="id">The lead ID</param>
        /// <returns>The lead if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<LeadDto>> GetLead(Guid id)
        {
            var lead = await _leadRepository.GetByIdAsync(id);

            if (lead == null)
            {
                return NotFound();
            }

            return Ok(MapToLeadDto(lead));
        }

        /// <summary>
        /// Creates a new lead
        /// </summary>
        /// <param name="dto">The lead data</param>
        /// <returns>The created lead</returns>
        [HttpPost]
        public async Task<ActionResult<LeadDto>> CreateLead(CreateLeadDto dto)
        {
            var lead = new Lead
            {
                Source = dto.Source,
                SourceId = dto.SourceId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address != null
                    ? new Address
                    {
                        Street = dto.Address.Street,
                        City = dto.Address.City,
                        State = dto.Address.State,
                        Zip = dto.Address.Zip
                    }
                    : null,
                Status = dto.Status,
                InterestType = dto.InterestType,
                InterestVehicleId = dto.InterestVehicleId,
                AssignedSalesRepId = dto.AssignedSalesRepId,
                Comments = dto.Comments,
                FollowupDate = dto.FollowupDate,
                Activities = new List<LeadActivity>()
            };

            await _leadRepository.AddAsync(lead);
            
            return CreatedAtAction(
                nameof(GetLead),
                new { id = lead.Id },
                MapToLeadDto(lead));
        }

        /// <summary>
        /// Updates an existing lead
        /// </summary>
        /// <param name="id">The lead ID</param>
        /// <param name="dto">The updated lead data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLead(Guid id, UpdateLeadDto dto)
        {
            var lead = await _leadRepository.GetByIdAsync(id);

            if (lead == null)
            {
                return NotFound();
            }

            // Update lead properties
            if (dto.FirstName != null) lead.FirstName = dto.FirstName;
            if (dto.LastName != null) lead.LastName = dto.LastName;
            if (dto.Email != null) lead.Email = dto.Email;
            if (dto.Phone != null) lead.Phone = dto.Phone;
            
            if (dto.Address != null)
            {
                lead.Address ??= new Address();
                if (dto.Address.Street != null) lead.Address.Street = dto.Address.Street;
                if (dto.Address.City != null) lead.Address.City = dto.Address.City;
                if (dto.Address.State != null) lead.Address.State = dto.Address.State;
                if (dto.Address.Zip != null) lead.Address.Zip = dto.Address.Zip;
            }
            
            if (dto.Status.HasValue) lead.Status = dto.Status.Value;
            if (dto.InterestType.HasValue) lead.InterestType = dto.InterestType.Value;
            if (dto.InterestVehicleId.HasValue) lead.InterestVehicleId = dto.InterestVehicleId.Value;
            if (dto.AssignedSalesRepId != null) lead.AssignedSalesRepId = dto.AssignedSalesRepId;
            if (dto.Comments != null) lead.Comments = dto.Comments;
            if (dto.FollowupDate.HasValue) lead.FollowupDate = dto.FollowupDate.Value;

            await _leadRepository.UpdateAsync(lead);

            return NoContent();
        }

        /// <summary>
        /// Adds an activity to a lead
        /// </summary>
        /// <param name="id">The lead ID</param>
        /// <param name="dto">The activity data</param>
        /// <returns>The updated lead</returns>
        [HttpPost("{id}/activities")]
        public async Task<ActionResult<LeadDto>> AddActivity(Guid id, AddLeadActivityDto dto)
        {
            var activity = new LeadActivity
            {
                Type = dto.Type,
                Date = dto.Date,
                UserId = User.Identity?.Name, // Get from authenticated user
                Notes = dto.Notes
            };

            try
            {
                var updatedLead = await _leadRepository.AddActivityAsync(id, activity);
                return Ok(MapToLeadDto(updatedLead));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Maps a Lead entity to a LeadDto
        /// </summary>
        private static LeadDto MapToLeadDto(Lead lead)
        {
            var dto = new LeadDto
            {
                Id = lead.Id,
                Source = lead.Source,
                SourceId = lead.SourceId,
                FirstName = lead.FirstName,
                LastName = lead.LastName,
                Email = lead.Email,
                Phone = lead.Phone,
                Address = lead.Address != null
                    ? new AddressDto
                    {
                        Street = lead.Address.Street,
                        City = lead.Address.City,
                        State = lead.Address.State,
                        Zip = lead.Address.Zip
                    }
                    : null,
                Status = lead.Status,
                InterestType = lead.InterestType,
                InterestVehicleId = lead.InterestVehicleId,
                AssignedSalesRepId = lead.AssignedSalesRepId,
                Comments = lead.Comments,
                CreatedAt = lead.CreatedAt,
                LastActivityDate = lead.LastActivityDate,
                FollowupDate = lead.FollowupDate,
                Activities = new List<LeadActivityDto>()
            };

            foreach (var activity in lead.Activities)
            {
                dto.Activities.Add(new LeadActivityDto
                {
                    Id = activity.Id,
                    Type = activity.Type,
                    Date = activity.Date,
                    UserId = activity.UserId,
                    Notes = activity.Notes
                });
            }

            return dto;
        }
    }
}
