using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Core.Services;

namespace DMS.ServiceManagement.Core.Services.Implementations
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly IServiceJobRepository _serviceJobRepository;

        public RepairOrderService(
            IRepairOrderRepository repairOrderRepository,
            IServiceJobRepository serviceJobRepository)
        {
            _repairOrderRepository = repairOrderRepository;
            _serviceJobRepository = serviceJobRepository;
        }

        public async Task<IEnumerable<RepairOrder>> GetAllRepairOrdersAsync()
        {
            return await _repairOrderRepository.GetAllAsync();
        }

        public async Task<RepairOrder> GetRepairOrderByIdAsync(Guid id)
        {
            return await _repairOrderRepository.GetByIdAsync(id);
        }

        public async Task<RepairOrder> GetRepairOrderByNumberAsync(string number)
        {
            return await _repairOrderRepository.GetByNumberAsync(number);
        }

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByCustomerIdAsync(Guid customerId)
        {
            return await _repairOrderRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<IEnumerable<RepairOrder>> GetRepairOrdersByVehicleIdAsync(Guid vehicleId)
        {
            return await _repairOrderRepository.GetByVehicleIdAsync(vehicleId);
        }

        public async Task<RepairOrder> CreateRepairOrderAsync(RepairOrder repairOrder)
        {
            if (repairOrder == null)
                throw new ArgumentNullException(nameof(repairOrder));
            
            // Generate a unique RO number if not provided
            if (string.IsNullOrEmpty(repairOrder.Number))
            {
                repairOrder.Number = GenerateRepairOrderNumber();
            }
            
            // Set dates
            repairOrder.CreatedAt = DateTime.UtcNow;
            repairOrder.UpdatedAt = DateTime.UtcNow;
            repairOrder.OpenDate = DateTime.UtcNow;
            
            // Set initial status
            repairOrder.Status = RepairOrderStatus.Open;
            
            return await _repairOrderRepository.CreateAsync(repairOrder);
        }

        public async Task<RepairOrder> UpdateRepairOrderAsync(RepairOrder repairOrder)
        {
            if (repairOrder == null)
                throw new ArgumentNullException(nameof(repairOrder));
            
            var existingRepairOrder = await _repairOrderRepository.GetByIdAsync(repairOrder.Id);
            if (existingRepairOrder == null)
                throw new KeyNotFoundException($"Repair Order with ID {repairOrder.Id} not found");
            
            // Update timestamp
            repairOrder.UpdatedAt = DateTime.UtcNow;
            
            return await _repairOrderRepository.UpdateAsync(repairOrder);
        }

        public async Task<RepairOrder> UpdateRepairOrderStatusAsync(Guid id, RepairOrderStatus status)
        {
            var repairOrder = await _repairOrderRepository.GetByIdAsync(id);
            if (repairOrder == null)
                throw new KeyNotFoundException($"Repair Order with ID {id} not found");
            
            repairOrder.Status = status;
            repairOrder.UpdatedAt = DateTime.UtcNow;
            
            // If completed, set the completion date
            if (status == RepairOrderStatus.Completed)
            {
                repairOrder.CompletionDate = DateTime.UtcNow;
            }
            
            return await _repairOrderRepository.UpdateAsync(repairOrder);
        }

        public async Task<RepairOrder> AddServiceJobAsync(Guid repairOrderId, ServiceJob serviceJob)
        {
            var repairOrder = await _repairOrderRepository.GetByIdAsync(repairOrderId);
            if (repairOrder == null)
                throw new KeyNotFoundException($"Repair Order with ID {repairOrderId} not found");
            
            serviceJob.RepairOrderId = repairOrderId;
            serviceJob.CreatedAt = DateTime.UtcNow;
            serviceJob.UpdatedAt = DateTime.UtcNow;
            
            await _serviceJobRepository.CreateAsync(serviceJob);
            
            // Recalculate totals
            await CalculateTotalAmountAsync(repairOrderId);
            
            return await _repairOrderRepository.GetByIdAsync(repairOrderId);
        }

        public async Task<RepairOrder> CloseRepairOrderAsync(Guid id)
        {
            var repairOrder = await _repairOrderRepository.GetByIdAsync(id);
            if (repairOrder == null)
                throw new KeyNotFoundException($"Repair Order with ID {id} not found");
            
            // Check if all jobs are completed
            var jobs = await _serviceJobRepository.GetByRepairOrderIdAsync(id);
            bool allJobsCompleted = true;
            foreach (var job in jobs)
            {
                if (job.Status != JobStatus.Completed)
                {
                    allJobsCompleted = false;
                    break;
                }
            }
            
            if (!allJobsCompleted)
            {
                throw new InvalidOperationException("Cannot close repair order because not all jobs are completed");
            }
            
            // Update status and dates
            repairOrder.Status = RepairOrderStatus.Closed;
            repairOrder.UpdatedAt = DateTime.UtcNow;
            if (!repairOrder.CompletionDate.HasValue)
            {
                repairOrder.CompletionDate = DateTime.UtcNow;
            }
            
            return await _repairOrderRepository.UpdateAsync(repairOrder);
        }

        public async Task<decimal> CalculateTotalAmountAsync(Guid id)
        {
            var repairOrder = await _repairOrderRepository.GetByIdAsync(id);
            if (repairOrder == null)
                throw new KeyNotFoundException($"Repair Order with ID {id} not found");
            
            // Get all jobs related to this repair order
            var jobs = await _serviceJobRepository.GetByRepairOrderIdAsync(id);
            
            decimal laborTotal = 0;
            decimal partsTotal = 0;
            
            foreach (var job in jobs)
            {
                laborTotal += job.LaborCharge;
                partsTotal += job.PartsCharge;
            }
            
            // Calculate tax on parts (typically labor is not taxed)
            decimal taxTotal = partsTotal * (repairOrder.TaxRate / 100);
            
            // Calculate totals
            decimal totalActual = laborTotal + partsTotal + taxTotal - repairOrder.DiscountTotal;
            
            // Update repair order with new totals
            repairOrder.LaborTotal = laborTotal;
            repairOrder.PartsTotal = partsTotal;
            repairOrder.TaxTotal = taxTotal;
            repairOrder.TotalActualAmount = totalActual;
            repairOrder.UpdatedAt = DateTime.UtcNow;
            
            await _repairOrderRepository.UpdateAsync(repairOrder);
            
            return totalActual;
        }
        
        // Helper methods
        private string GenerateRepairOrderNumber()
        {
            // Generate a unique repair order number
            // Format: RO-YYYYMMDD-XXXX (where XXXX is a random number)
            string dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            string randomStr = new Random().Next(1000, 9999).ToString();
            return $"RO-{dateStr}-{randomStr}";
        }
    }
}
