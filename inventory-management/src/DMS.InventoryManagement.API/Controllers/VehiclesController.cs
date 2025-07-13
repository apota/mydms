using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(
            IVehicleRepository vehicleRepository,
            ILogger<VehiclesController> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all vehicles
        /// </summary>
        /// <returns>A list of vehicles</returns>
        [HttpGet]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return Ok(vehicles.Select(MapToDto));
        }

        /// <summary>
        /// Gets a vehicle by ID
        /// </summary>
        /// <param name="id">The vehicle ID</param>
        /// <returns>The vehicle</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehicleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleDto>> GetById(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(vehicle));
        }

        /// <summary>
        /// Gets a vehicle by VIN
        /// </summary>
        /// <param name="vin">The vehicle identification number</param>
        /// <returns>The vehicle</returns>
        [HttpGet("vin/{vin}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehicleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleDto>> GetByVin(string vin)
        {
            var vehicle = await _vehicleRepository.GetByVinAsync(vin);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(vehicle));
        }

        /// <summary>
        /// Gets a vehicle by stock number
        /// </summary>
        /// <param name="stockNumber">The stock number</param>
        /// <returns>The vehicle</returns>
        [HttpGet("stockNumber/{stockNumber}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(VehicleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VehicleDto>> GetByStockNumber(string stockNumber)
        {
            var vehicle = await _vehicleRepository.GetByStockNumberAsync(stockNumber);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(vehicle));
        }

        /// <summary>
        /// Gets vehicles by status
        /// </summary>
        /// <param name="status">The vehicle status</param>
        /// <returns>A list of vehicles with the specified status</returns>
        [HttpGet("status/{status}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetByStatus(VehicleStatus status)
        {
            var vehicles = await _vehicleRepository.GetByStatusAsync(status);
            return Ok(vehicles.Select(MapToDto));
        }

        /// <summary>
        /// Gets aging inventory
        /// </summary>
        /// <param name="days">The number of days</param>
        /// <returns>A list of vehicles that have been in inventory longer than the specified number of days</returns>
        [HttpGet("aging/{days:int}")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAgingInventory(int days)
        {
            var vehicles = await _vehicleRepository.GetAgingInventoryAsync(days);
            return Ok(vehicles.Select(MapToDto));
        }

        /// <summary>
        /// Creates a new vehicle
        /// </summary>
        /// <param name="vehicleDto">The vehicle data</param>
        /// <returns>The created vehicle</returns>
        [HttpPost]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(typeof(VehicleDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<VehicleDto>> Create(CreateVehicleDto vehicleDto)
        {
            // Check if VIN already exists
            var existingVehicle = await _vehicleRepository.GetByVinAsync(vehicleDto.VIN);
            if (existingVehicle != null)
            {
                return BadRequest($"Vehicle with VIN {vehicleDto.VIN} already exists");
            }

            // Check if stock number already exists
            existingVehicle = await _vehicleRepository.GetByStockNumberAsync(vehicleDto.StockNumber);
            if (existingVehicle != null)
            {
                return BadRequest($"Vehicle with stock number {vehicleDto.StockNumber} already exists");
            }

            var vehicle = MapFromCreateDto(vehicleDto);
            await _vehicleRepository.AddAsync(vehicle);
            await (_vehicleRepository as IUnitOfWork).SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, MapToDto(vehicle));
        }

        /// <summary>
        /// Updates a vehicle
        /// </summary>
        /// <param name="id">The vehicle ID</param>
        /// <param name="vehicleDto">The updated vehicle data</param>
        /// <returns>No content</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid id, UpdateVehicleDto vehicleDto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            // Check if VIN is being changed and if it's already in use
            if (vehicleDto.VIN != vehicle.VIN)
            {
                var existingVehicle = await _vehicleRepository.GetByVinAsync(vehicleDto.VIN);
                if (existingVehicle != null && existingVehicle.Id != id)
                {
                    return BadRequest($"Vehicle with VIN {vehicleDto.VIN} already exists");
                }
            }

            // Check if stock number is being changed and if it's already in use
            if (vehicleDto.StockNumber != vehicle.StockNumber)
            {
                var existingVehicle = await _vehicleRepository.GetByStockNumberAsync(vehicleDto.StockNumber);
                if (existingVehicle != null && existingVehicle.Id != id)
                {
                    return BadRequest($"Vehicle with stock number {vehicleDto.StockNumber} already exists");
                }
            }

            // Update vehicle properties
            vehicle.VIN = vehicleDto.VIN;
            vehicle.StockNumber = vehicleDto.StockNumber;
            vehicle.Make = vehicleDto.Make;
            vehicle.Model = vehicleDto.Model;
            vehicle.Year = vehicleDto.Year;
            vehicle.Trim = vehicleDto.Trim;
            vehicle.ExteriorColor = vehicleDto.ExteriorColor;
            vehicle.InteriorColor = vehicleDto.InteriorColor;
            vehicle.Mileage = vehicleDto.Mileage;
            vehicle.VehicleType = vehicleDto.VehicleType;
            vehicle.Status = vehicleDto.Status;
            vehicle.ListPrice = vehicleDto.ListPrice;
            vehicle.InvoicePrice = vehicleDto.InvoicePrice;
            vehicle.MSRP = vehicleDto.MSRP;
            vehicle.LotLocation = vehicleDto.LotLocation;

            await _vehicleRepository.UpdateAsync(vehicle);
            await (_vehicleRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes a vehicle
        /// </summary>
        /// <param name="id">The vehicle ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "InventoryWrite")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            await _vehicleRepository.DeleteAsync(vehicle);
            await (_vehicleRepository as IUnitOfWork).SaveChangesAsync();

            return NoContent();
        }

        #region Helper Methods

        private static VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                VIN = vehicle.VIN,
                StockNumber = vehicle.StockNumber,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Trim = vehicle.Trim,
                ExteriorColor = vehicle.ExteriorColor,
                InteriorColor = vehicle.InteriorColor,
                Mileage = vehicle.Mileage,
                VehicleType = vehicle.VehicleType,
                Status = vehicle.Status,
                AcquisitionCost = vehicle.AcquisitionCost,
                ListPrice = vehicle.ListPrice,
                InvoicePrice = vehicle.InvoicePrice,
                MSRP = vehicle.MSRP,
                AcquisitionDate = vehicle.AcquisitionDate,
                AcquisitionSource = vehicle.AcquisitionSource,
                LotLocation = vehicle.LotLocation,
                Features = vehicle.Features?.Select(f => new VehicleFeatureDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category
                }).ToList() ?? new List<VehicleFeatureDto>(),
                Images = vehicle.Images?.Select(i => new VehicleImageDto
                {
                    Id = i.Id,
                    FilePath = i.FilePath,
                    Caption = i.Caption,
                    SequenceNumber = i.SequenceNumber,
                    IsPrimary = i.IsPrimary,
                    UploadDate = i.UploadDate,
                    ImageType = i.ImageType
                }).ToList() ?? new List<VehicleImageDto>(),
                ReconditioningRecords = vehicle.ReconditioningRecords?.Select(r => new ReconditioningRecordDto
                {
                    Id = r.Id,
                    Description = r.Description,
                    Cost = r.Cost,
                    Vendor = r.Vendor,
                    WorkDate = r.WorkDate,
                    Status = r.Status
                }).ToList() ?? new List<ReconditioningRecordDto>(),
                CreatedAt = vehicle.CreatedAt,
                CreatedBy = vehicle.CreatedBy,
                UpdatedAt = vehicle.UpdatedAt,
                UpdatedBy = vehicle.UpdatedBy
            };
        }

        private static Vehicle MapFromCreateDto(CreateVehicleDto dto)
        {
            return new Vehicle
            {
                VIN = dto.VIN,
                StockNumber = dto.StockNumber,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Trim = dto.Trim,
                ExteriorColor = dto.ExteriorColor,
                InteriorColor = dto.InteriorColor,
                Mileage = dto.Mileage,
                VehicleType = dto.VehicleType,
                Status = dto.Status,
                AcquisitionCost = dto.AcquisitionCost,
                ListPrice = dto.ListPrice,
                InvoicePrice = dto.InvoicePrice,
                MSRP = dto.MSRP,
                AcquisitionDate = dto.AcquisitionDate,
                AcquisitionSource = dto.AcquisitionSource,
                LotLocation = dto.LotLocation
            };
        }

        #endregion
    }
}
