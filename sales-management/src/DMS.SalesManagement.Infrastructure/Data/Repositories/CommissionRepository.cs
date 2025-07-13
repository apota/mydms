using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Core.Repositories;
using DMS.SalesManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMS.SalesManagement.Infrastructure.Data.Repositories
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly SalesDbContext _context;
        private readonly IDealRepository _dealRepository;
        
        public CommissionRepository(SalesDbContext context, IDealRepository dealRepository)
        {
            _context = context;
            _dealRepository = dealRepository;
        }

        public async Task<Commission> GetByIdAsync(Guid id)
        {
            return await _context.Commissions
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Commission>> GetByDealIdAsync(Guid dealId)
        {
            return await _context.Commissions
                .Where(c => c.DealId == dealId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Commission>> GetByUserIdAsync(string userId)
        {
            return await _context.Commissions
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Commission>> GetByStatusAsync(CommissionStatus status)
        {
            return await _context.Commissions
                .Where(c => c.Status == status)
                .ToListAsync();
        }

        public async Task AddAsync(Commission commission)
        {
            await _context.Commissions.AddAsync(commission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Commission commission)
        {
            _context.Entry(commission).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var commission = await _context.Commissions.FindAsync(id);
            if (commission != null)
            {
                _context.Commissions.Remove(commission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Commission>> CalculateCommissionsForDealAsync(Guid dealId)
        {
            var deal = await _dealRepository.GetByIdWithDetailsAsync(dealId);
            if (deal == null)
                return new List<Commission>();

            var commissions = new List<Commission>();

            // Calculate Sales Rep Commission
            if (!string.IsNullOrEmpty(deal.SalesRepId))
            {
                var salesRepCommission = CalculateSalesRepCommission(deal);
                commissions.Add(salesRepCommission);
            }

            // Calculate Finance Manager Commission (if financed)
            if (deal.DealType == DealType.Finance || deal.DealType == DealType.Lease)
            {
                // This would typically come from the deal processing workflow
                // which would record who the finance manager was
                var financeManagerId = await GetFinanceManagerForDealAsync(deal);
                if (!string.IsNullOrEmpty(financeManagerId))
                {
                    var financeCommission = CalculateFinanceManagerCommission(deal, financeManagerId);
                    commissions.Add(financeCommission);
                }
            }

            // Calculate Sales Manager Commission
            var salesManagerId = await GetSalesManagerForDealAsync(deal);
            if (!string.IsNullOrEmpty(salesManagerId))
            {
                var managerCommission = CalculateSalesManagerCommission(deal, salesManagerId);
                commissions.Add(managerCommission);
            }

            // Save all calculated commissions
            foreach (var commission in commissions)
            {
                // Check if commission already exists
                var existingCommission = await _context.Commissions
                    .FirstOrDefaultAsync(c => c.DealId == dealId && c.UserId == commission.UserId);
                
                if (existingCommission != null)
                {
                    existingCommission.BaseAmount = commission.BaseAmount;
                    existingCommission.BonusAmount = commission.BonusAmount;
                    existingCommission.TotalAmount = commission.TotalAmount;
                    existingCommission.UpdatedAt = DateTime.UtcNow;
                    await UpdateAsync(existingCommission);
                }
                else
                {
                    await AddAsync(commission);
                }
            }

            return commissions;
        }

        private Commission CalculateSalesRepCommission(Deal deal)
        {
            // Simple calculation - in a real system this would be much more complex
            // based on dealership commission structures, vehicle type, etc.
            decimal baseRate = 0.10m; // 10% of profit
            decimal baseAmount = deal.TotalPrice * baseRate;
            
            // Add bonuses for F&I products
            decimal bonusAmount = 0;
            if (deal.AddOns != null && deal.AddOns.Any())
            {
                bonusAmount = deal.AddOns.Sum(a => a.Price * 0.15m); // 15% of add-on price
            }
            
            return new Commission
            {
                Id = Guid.NewGuid(),
                DealId = deal.Id,
                UserId = deal.SalesRepId,
                Role = CommissionRole.SalesRep,
                BaseAmount = baseAmount,
                BonusAmount = bonusAmount,
                TotalAmount = baseAmount + bonusAmount,
                CalculationMethod = "Standard Sales Commission",
                Status = CommissionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        private Commission CalculateFinanceManagerCommission(Deal deal, string financeManagerId)
        {
            // Finance managers typically get commission on financing and add-on products
            decimal financeAmount = 0;
            
            // Commission on financing - typically a flat fee plus percentage
            if (deal.DealType == DealType.Finance || deal.DealType == DealType.Lease)
            {
                financeAmount = 200m + (deal.FinancingRate * 100m); // $200 plus $100 per % of rate
            }
            
            // Commission on F&I products 
            decimal productAmount = 0;
            if (deal.AddOns != null && deal.AddOns.Any())
            {
                productAmount = deal.AddOns.Sum(a => (a.Price - a.Cost) * 0.20m); // 20% of add-on profit
            }
            
            return new Commission
            {
                Id = Guid.NewGuid(),
                DealId = deal.Id,
                UserId = financeManagerId,
                Role = CommissionRole.Finance,
                BaseAmount = financeAmount,
                BonusAmount = productAmount,
                TotalAmount = financeAmount + productAmount,
                CalculationMethod = "F&I Manager Commission",
                Status = CommissionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        private Commission CalculateSalesManagerCommission(Deal deal, string salesManagerId)
        {
            // Sales managers typically get a smaller percentage of all deals
            decimal baseRate = 0.03m; // 3% of deal value
            decimal baseAmount = deal.TotalPrice * baseRate;
            
            return new Commission
            {
                Id = Guid.NewGuid(),
                DealId = deal.Id,
                UserId = salesManagerId,
                Role = CommissionRole.Manager,
                BaseAmount = baseAmount,
                BonusAmount = 0m,
                TotalAmount = baseAmount,
                CalculationMethod = "Sales Manager Override",
                Status = CommissionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        
        // In a real system, these would be retrieved from a workflow or management system
        private Task<string> GetFinanceManagerForDealAsync(Deal deal)
        {
            // Simplified implementation - would normally come from a user service or workflow
            return Task.FromResult("finance-manager-1"); 
        }
        
        private Task<string> GetSalesManagerForDealAsync(Deal deal)
        {
            // Simplified implementation - would normally come from a user service or workflow
            return Task.FromResult("sales-manager-1"); 
        }
    }
}
