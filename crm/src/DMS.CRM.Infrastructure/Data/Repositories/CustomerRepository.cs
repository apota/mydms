using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Repositories;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.CRM.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Customer entities
    /// </summary>
    public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
    {
        private readonly CrmDbContext _dbContext;

        public CustomerRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Customers
                .Where(c => c.Status == status)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Customers
                .Where(c => 
                    c.FirstName != null && c.FirstName.Contains(searchTerm) || 
                    c.LastName != null && c.LastName.Contains(searchTerm) || 
                    c.Email != null && c.Email.Contains(searchTerm) ||
                    c.BusinessName != null && c.BusinessName.Contains(searchTerm))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> GetByLoyaltyTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Customers
                .Where(c => c.LoyaltyTier == tier)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> GetByVehicleModelAsync(Guid modelId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerVehicles
                .Where(v => v.VehicleId == modelId)
                .Select(v => v.Customer!)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Customer?> GetCompleteProfileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Customers
                .Include(c => c.Vehicles)
                .Include(c => c.Interactions)
                .Include(c => c.Journey)
                .Include(c => c.Surveys)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
    }
}
