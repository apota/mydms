using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Infrastructure.Data;
using DMS.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for vehicle documents
    /// </summary>
    public class VehicleDocumentRepository : Repository<VehicleDocument, InventoryDbContext>, IVehicleDocumentRepository
    {
        public VehicleDocumentRepository(InventoryDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleDocument>> GetByVehicleIdAsync(Guid vehicleId)
        {
            return await DbContext.VehicleDocuments
                .Where(vd => vd.VehicleId == vehicleId)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleDocument>> GetByVehicleIdAndTypeAsync(Guid vehicleId, DocumentType documentType)
        {
            return await DbContext.VehicleDocuments
                .Where(vd => vd.VehicleId == vehicleId && vd.DocumentType == documentType)
                .ToListAsync();
        }
    }
}
