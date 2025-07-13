using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DMS.CRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerService customerService,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all customers", Description = "Retrieves a paginated list of all customers")]
        [SwaggerResponse(200, "List of customers retrieved successfully", typeof(IEnumerable<CustomerDto>))]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync(skip, take);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customers");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get customer by ID", Description = "Retrieves a specific customer by their ID")]
        [SwaggerResponse(200, "Customer retrieved successfully", typeof(CustomerDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound($"Customer with ID {id} not found");
                }
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer");
            }
        }

        [HttpGet("email/{email}")]
        [SwaggerOperation(Summary = "Get customer by email", Description = "Retrieves a specific customer by their email address")]
        [SwaggerResponse(200, "Customer retrieved successfully", typeof(CustomerDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(string email)
        {
            try
            {
                var customer = await _customerService.GetCustomerByEmailAsync(email);
                if (customer == null)
                {
                    return NotFound($"Customer with email {email} not found");
                }
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with email: {Email}", email);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer");
            }
        }

        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search customers", Description = "Searches customers based on the provided search term")]
        [SwaggerResponse(200, "Search results retrieved successfully", typeof(IEnumerable<CustomerDto>))]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers(
            [FromQuery] string searchTerm,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var customers = await _customerService.SearchCustomersAsync(searchTerm, skip, take);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error searching customers");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create customer", Description = "Creates a new customer")]
        [SwaggerResponse(201, "Customer created successfully", typeof(CustomerDto))]
        [SwaggerResponse(400, "Invalid customer data")]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateDto customerDto)
        {
            try
            {
                if (customerDto == null)
                {
                    return BadRequest("Customer data is null");
                }

                var createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating customer");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update customer", Description = "Updates an existing customer")]
        [SwaggerResponse(200, "Customer updated successfully", typeof(CustomerDto))]
        [SwaggerResponse(404, "Customer not found")]
        [SwaggerResponse(400, "Invalid customer data")]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] CustomerUpdateDto customerDto)
        {
            try
            {
                if (customerDto == null)
                {
                    return BadRequest("Customer data is null");
                }

                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerDto);
                if (updatedCustomer == null)
                {
                    return NotFound($"Customer with ID {id} not found");
                }

                return Ok(updatedCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating customer");
            }
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete customer", Description = "Deletes an existing customer")]
        [SwaggerResponse(204, "Customer deleted successfully")]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                {
                    return NotFound($"Customer with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting customer");
            }
        }

        [HttpGet("{id:guid}/stats")]
        [SwaggerOperation(Summary = "Get customer statistics", Description = "Retrieves statistics for a specific customer")]
        [SwaggerResponse(200, "Customer statistics retrieved successfully", typeof(CustomerStatsDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<CustomerStatsDto>> GetCustomerStats(Guid id)
        {
            try
            {
                var stats = await _customerService.GetCustomerStatsAsync(id);
                if (stats == null)
                {
                    return NotFound($"Customer with ID {id} not found");
                }
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer statistics");
            }
        }

        [HttpGet("{customerId:guid}/vehicles")]
        [SwaggerOperation(Summary = "Get customer vehicles", Description = "Retrieves all vehicles associated with a specific customer")]
        [SwaggerResponse(200, "Customer vehicles retrieved successfully", typeof(IEnumerable<CustomerVehicleDto>))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<IEnumerable<CustomerVehicleDto>>> GetCustomerVehicles(Guid customerId)
        {
            try
            {
                var vehicles = await _customerService.GetCustomerVehiclesAsync(customerId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer vehicles");
            }
        }

        [HttpPost("{customerId:guid}/vehicles")]
        [SwaggerOperation(Summary = "Add customer vehicle", Description = "Adds a new vehicle to a specific customer")]
        [SwaggerResponse(201, "Vehicle added successfully", typeof(CustomerVehicleDto))]
        [SwaggerResponse(404, "Customer not found")]
        [SwaggerResponse(400, "Invalid vehicle data")]
        public async Task<ActionResult<CustomerVehicleDto>> AddCustomerVehicle(
            Guid customerId,
            [FromBody] CustomerVehicleCreateDto vehicleDto)
        {
            try
            {
                if (vehicleDto == null)
                {
                    return BadRequest("Vehicle data is null");
                }

                var vehicle = await _customerService.AddCustomerVehicleAsync(customerId, vehicleDto);
                if (vehicle == null)
                {
                    return NotFound($"Customer with ID {customerId} not found");
                }

                return CreatedAtAction(nameof(GetCustomerVehicles), new { customerId }, vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding customer vehicle");
            }
        }

        [HttpDelete("{customerId:guid}/vehicles/{vehicleId:guid}")]
        [SwaggerOperation(Summary = "Remove customer vehicle", Description = "Removes a vehicle from a specific customer")]
        [SwaggerResponse(204, "Vehicle removed successfully")]
        [SwaggerResponse(404, "Customer or vehicle not found")]
        public async Task<ActionResult> RemoveCustomerVehicle(Guid customerId, Guid vehicleId)
        {
            try
            {
                var result = await _customerService.RemoveCustomerVehicleAsync(customerId, vehicleId);
                if (!result)
                {
                    return NotFound($"Customer with ID {customerId} or vehicle with ID {vehicleId} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing vehicle {VehicleId} for customer with ID: {CustomerId}", vehicleId, customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error removing customer vehicle");
            }
        }
    }
}
