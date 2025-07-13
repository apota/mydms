using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Services;
using DMS.Shared.Core.Data;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.InventoryManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository for vehicle entities
    /// </summary>
    public class VehicleRepository : EfRepository<Vehicle>, IVehicleRepository
    {
        private readonly InventoryDbContext _context;

        public VehicleRepository(InventoryDbContext context) : base(context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Gets a vehicle by its VIN
        /// </summary>
        /// <param name="vin">The vehicle identification number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The vehicle if found, otherwise null</returns>
        public async Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default)
        {
            return await _context.Vehicles
                .Include(v => v.Features)
                .Include(v => v.Images)
                .Include(v => v.ReconditioningRecords)
                .FirstOrDefaultAsync(v => v.VIN == vin, cancellationToken);
        }
        
        /// <summary>
        /// Gets a vehicle by its stock number
        /// </summary>
        /// <param name="stockNumber">The stock number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The vehicle if found, otherwise null</returns>
        public async Task<Vehicle?> GetByStockNumberAsync(string stockNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Vehicles
                .Include(v => v.Features)
                .Include(v => v.Images)
                .Include(v => v.ReconditioningRecords)
                .FirstOrDefaultAsync(v => v.StockNumber == stockNumber, cancellationToken);
        }
        
        /// <summary>
        /// Gets vehicles by their status
        /// </summary>
        /// <param name="status">The vehicle status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of vehicles with the specified status</returns>
        public async Task<IEnumerable<Vehicle>> GetByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Vehicles
                .Where(v => v.Status == status)
                .Include(v => v.Images.Where(i => i.IsPrimary))
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets vehicles that have been in inventory longer than the specified number of days
        /// </summary>
        /// <param name="days">The number of days</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of vehicles that have been in inventory longer than the specified number of days</returns>
        public async Task<IEnumerable<Vehicle>> GetAgingInventoryAsync(int days, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            return await _context.Vehicles
                .Where(v => v.AcquisitionDate <= cutoffDate && v.Status != VehicleStatus.Sold && v.Status != VehicleStatus.Delivered)
                .Include(v => v.Images.Where(i => i.IsPrimary))
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets vehicles by their type
        /// </summary>
        /// <param name="vehicleType">The vehicle type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of vehicles with the specified type</returns>
        public async Task<IEnumerable<Vehicle>> GetByTypeAsync(VehicleType vehicleType, CancellationToken cancellationToken = default)
        {
            return await _context.Vehicles
                .Where(v => v.VehicleType == vehicleType)
                .Include(v => v.Images.Where(i => i.IsPrimary))
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Gets a vehicle inspection by vehicle ID
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The vehicle inspection if found, otherwise null</returns>
        public async Task<VehicleInspection?> GetVehicleInspection(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            return await _context.VehicleInspections
                .Include(i => i.Vehicle)
                .FirstOrDefaultAsync(i => i.VehicleId == vehicleId, cancellationToken);
        }
        
        /// <summary>
        /// Adds a new vehicle inspection
        /// </summary>
        /// <param name="inspection">The inspection to add</param>
        public void AddInspection(VehicleInspection inspection)
        {
            _context.VehicleInspections.Add(inspection);
        }
        
        /// <summary>
        /// Gets vehicle documents by ID
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="documentIds">List of document IDs to retrieve</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A list of vehicle documents</returns>
        public async Task<List<VehicleDocument>> GetVehicleDocuments(Guid vehicleId, List<Guid> documentIds, CancellationToken cancellationToken = default)
        {
            return await _context.VehicleDocuments
                .Where(d => d.VehicleId == vehicleId && documentIds.Contains(d.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
