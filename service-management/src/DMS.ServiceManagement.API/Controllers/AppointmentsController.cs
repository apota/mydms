using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.ServiceManagement.API.Controllers
{
    [ApiController]
    [Route("api/service/appointments")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceAppointment>>> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all appointments");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving appointments");
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceAppointment>> GetAppointmentById(Guid id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving appointment with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving appointment");
            }
        }

        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServiceAppointment>>> GetAppointmentsByCustomerId(Guid customerId)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customerId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving appointments for customer ID: {customerId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving appointments");
            }
        }

        [HttpGet("availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServiceAppointment>>> GetAvailableSlots([FromQuery] DateTime date, [FromQuery] int duration = 60)
        {
            try
            {
                var availableSlots = await _appointmentService.GetAvailableSlotsAsync(date, duration);
                return Ok(availableSlots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving available slots for date: {date}, duration: {duration}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving available slots");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceAppointment>> CreateAppointment([FromBody] ServiceAppointment appointment)
        {
            try
            {
                if (appointment == null)
                {
                    return BadRequest("Appointment data is null");
                }

                var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
                
                return CreatedAtAction(nameof(GetAppointmentById), 
                    new { id = createdAppointment.Id }, 
                    createdAppointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating appointment");
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceAppointment>> UpdateAppointment(Guid id, [FromBody] ServiceAppointment appointment)
        {
            try
            {
                if (appointment == null)
                {
                    return BadRequest("Appointment data is null");
                }

                if (id != appointment.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(appointment);
                return Ok(updatedAppointment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Appointment with ID {id} not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating appointment");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(id);
                if (!result)
                {
                    return NotFound($"Appointment with ID {id} not found");
                }
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Appointment with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error canceling appointment with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error canceling appointment");
            }
        }
        
        [HttpPost("{id:guid}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmAppointment(Guid id)
        {
            try
            {
                var result = await _appointmentService.ConfirmAppointmentAsync(id);
                if (!result)
                {
                    return NotFound($"Appointment with ID {id} not found");
                }
                return Ok(new { message = "Appointment confirmed successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Appointment with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming appointment with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error confirming appointment");
            }
        }
    }
}
