using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.Models;
using DMS.FinancialManagement.Core.Repositories;
using DMS.FinancialManagement.Core.Services;

namespace DMS.FinancialManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the accounts payable service
    /// </summary>
    public class AccountsPayableService : IAccountsPayableService
    {
        private readonly IBillRepository _billRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly IChartOfAccountRepository _accountRepository;

        public AccountsPayableService(
            IBillRepository billRepository,
            IPaymentRepository paymentRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IVendorRepository vendorRepository,
            IChartOfAccountRepository accountRepository)
        {
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
            _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<IEnumerable<Bill>> GetBillsAsync(string status = null, Guid? vendorId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _billRepository.GetBillsAsync(status, vendorId, fromDate, toDate);
        }

        public async Task<Bill> GetBillByIdAsync(Guid id)
        {
            return await _billRepository.GetByIdAsync(id);
        }

        public async Task<Bill> CreateBillAsync(Bill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException(nameof(bill));
            }

            // Validate vendor
            var vendor = await _vendorRepository.GetByIdAsync(bill.VendorId);
            if (vendor == null)
            {
                throw new InvalidOperationException($"Vendor with ID {bill.VendorId} not found");
            }

            // Set default values
            bill.Id = Guid.NewGuid();
            bill.BillDate = bill.BillDate == default ? DateTime.UtcNow : bill.BillDate;
            bill.Status = "Pending";
            bill.PaidAmount = 0;
            bill.CreatedDate = DateTime.UtcNow;
            bill.VendorName = vendor.Name;

            // Generate bill number if not provided
            if (string.IsNullOrEmpty(bill.BillNumber))
            {
                // Get the next sequential bill number
                var lastBill = await _billRepository.GetLastBillNumberAsync();
                var nextNumber = 1;
                
                if (!string.IsNullOrEmpty(lastBill) && int.TryParse(lastBill.Replace("BILL-", ""), out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                
                bill.BillNumber = $"BILL-{nextNumber:D6}";
            }

            // Calculate totals
            if (bill.LineItems != null && bill.LineItems.Any())
            {
                foreach (var lineItem in bill.LineItems)
                {
                    lineItem.Id = Guid.NewGuid();
                    lineItem.BillId = bill.Id;
                    lineItem.Amount = lineItem.Quantity * lineItem.UnitPrice;
                }

                bill.TotalAmount = bill.LineItems.Sum(li => li.Amount + li.TaxAmount);
                bill.TaxAmount = bill.LineItems.Sum(li => li.TaxAmount);
            }

            return await _billRepository.AddAsync(bill);
        }

        public async Task<Bill> UpdateBillAsync(Bill bill)
        {
            if (bill == null)
            {
                throw new ArgumentNullException(nameof(bill));
            }

            var existingBill = await _billRepository.GetByIdAsync(bill.Id);
            if (existingBill == null)
            {
                throw new InvalidOperationException($"Bill with ID {bill.Id} not found");
            }

            // Don't allow changing vendor
            bill.VendorId = existingBill.VendorId;
            bill.VendorName = existingBill.VendorName;
            
            // Don't allow changing paid amount directly - it's calculated from payments
            bill.PaidAmount = existingBill.PaidAmount;
            
            // Don't allow changing bill number or creation date
            bill.BillNumber = existingBill.BillNumber;
            bill.CreatedDate = existingBill.CreatedDate;
            bill.CreatedBy = existingBill.CreatedBy;
            
            // Update modified timestamp
            bill.ModifiedDate = DateTime.UtcNow;

            // Recalculate totals if line items exist
            if (bill.LineItems != null && bill.LineItems.Any())
            {
                foreach (var lineItem in bill.LineItems)
                {
                    if (lineItem.Id == Guid.Empty)
                    {
                        lineItem.Id = Guid.NewGuid();
                    }
                    
                    lineItem.BillId = bill.Id;
                    lineItem.Amount = lineItem.Quantity * lineItem.UnitPrice;
                }

                bill.TotalAmount = bill.LineItems.Sum(li => li.Amount + li.TaxAmount);
                bill.TaxAmount = bill.LineItems.Sum(li => li.TaxAmount);
            }

            return await _billRepository.UpdateAsync(bill);
        }

        public async Task<Bill> PayBillAsync(Guid billId, Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            var bill = await _billRepository.GetByIdAsync(billId);
            if (bill == null)
            {
                throw new InvalidOperationException($"Bill with ID {billId} not found");
            }

            // Ensure payment amount doesn't exceed remaining amount
            decimal remainingAmount = bill.TotalAmount - bill.PaidAmount;
            if (payment.Amount > remainingAmount)
            {
                throw new InvalidOperationException($"Payment amount ({payment.Amount:C}) exceeds remaining bill amount ({remainingAmount:C})");
            }

            // Set default values for payment
            payment.Id = Guid.NewGuid();
            payment.PaymentDate = payment.PaymentDate == default ? DateTime.UtcNow : payment.PaymentDate;
            payment.Status = PaymentStatus.Cleared;
            payment.EntityId = bill.VendorId;
            payment.EntityType = EntityType.Vendor;
            payment.CreatedAt = DateTime.UtcNow;
            payment.ProcessedDate = DateTime.UtcNow;

            // Generate payment number if not provided
            if (string.IsNullOrEmpty(payment.PaymentNumber))
            {
                // Get the next sequential payment number
                var lastPayment = await _paymentRepository.GetLastPaymentNumberAsync();
                var nextNumber = 1;
                
                if (!string.IsNullOrEmpty(lastPayment) && int.TryParse(lastPayment.Replace("PAY-", ""), out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                
                payment.PaymentNumber = $"PAY-{nextNumber:D6}";
            }

            // Start a transaction
            using var transaction = await _billRepository.BeginTransactionAsync();

            try
            {
                // Add the payment
                await _paymentRepository.AddAsync(payment);
                
                // Update the bill
                bill.PaidAmount += payment.Amount;
                
                // Update bill status
                if (Math.Round(bill.PaidAmount, 2) >= Math.Round(bill.TotalAmount, 2))
                {
                    bill.Status = "Paid";
                }
                else
                {
                    bill.Status = "Partial";
                }
                
                bill.ModifiedDate = DateTime.UtcNow;
                bill = await _billRepository.UpdateAsync(bill);

                // Create bill-payment relationship
                await _billRepository.AddBillPaymentAsync(bill.Id, payment.Id, payment.Amount);

                // Commit the transaction
                await transaction.CommitAsync();
                
                return bill;
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<VendorAgingReport> GetVendorAgingReportAsync(DateTime? asOfDate = null)
        {
            // Default to current date if not specified
            var reportDate = asOfDate ?? DateTime.UtcNow.Date;
            
            // Define aging buckets
            var agingBuckets = new List<AgingBucket>
            {
                new AgingBucket { Name = "Current", StartDays = 0, EndDays = 0 },
                new AgingBucket { Name = "1-30 Days", StartDays = 1, EndDays = 30 },
                new AgingBucket { Name = "31-60 Days", StartDays = 31, EndDays = 60 },
                new AgingBucket { Name = "61-90 Days", StartDays = 61, EndDays = 90 },
                new AgingBucket { Name = "Over 90 Days", StartDays = 91, EndDays = int.MaxValue }
            };
            
            // Get all outstanding bills (not fully paid)
            var bills = await _billRepository.GetBillsAsync(
                status: "Pending,Partial",
                toDate: reportDate);
            
            // Group by vendor
            var vendorGroups = bills.GroupBy(b => b.VendorId);
            
            var report = new VendorAgingReport
            {
                AsOfDate = reportDate,
                AgingBuckets = agingBuckets,
                VendorDetails = new List<VendorAgingDetail>()
            };
            
            // Calculate aging for each vendor
            foreach (var group in vendorGroups)
            {
                var vendorId = group.Key;
                var vendor = await _vendorRepository.GetByIdAsync(vendorId);
                if (vendor == null) continue;
                
                var vendorDetail = new VendorAgingDetail
                {
                    VendorId = vendorId,
                    VendorName = vendor.Name,
                    Current = 0,
                    Days1To30 = 0,
                    Days31To60 = 0,
                    Days61To90 = 0,
                    DaysOver90 = 0,
                    TotalAmount = 0
                };
                
                // Calculate aging for each bill
                foreach (var bill in group)
                {
                    // Calculate remaining amount to be paid
                    decimal remainingAmount = bill.TotalAmount - bill.PaidAmount;
                    if (remainingAmount <= 0) continue;
                    
                    // Calculate days outstanding
                    int daysOutstanding = (reportDate - bill.DueDate).Days;
                    
                    // Assign to appropriate aging bucket
                    if (daysOutstanding <= 0)
                    {
                        vendorDetail.Current += remainingAmount;
                    }
                    else if (daysOutstanding <= 30)
                    {
                        vendorDetail.Days1To30 += remainingAmount;
                    }
                    else if (daysOutstanding <= 60)
                    {
                        vendorDetail.Days31To60 += remainingAmount;
                    }
                    else if (daysOutstanding <= 90)
                    {
                        vendorDetail.Days61To90 += remainingAmount;
                    }
                    else
                    {
                        vendorDetail.DaysOver90 += remainingAmount;
                    }
                    
                    vendorDetail.TotalAmount += remainingAmount;
                }
                
                report.VendorDetails.Add(vendorDetail);
                
                // Update bucket totals
                agingBuckets[0].TotalAmount += vendorDetail.Current;
                agingBuckets[1].TotalAmount += vendorDetail.Days1To30;
                agingBuckets[2].TotalAmount += vendorDetail.Days31To60;
                agingBuckets[3].TotalAmount += vendorDetail.Days61To90;
                agingBuckets[4].TotalAmount += vendorDetail.DaysOver90;
            }
            
            return report;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetBillablePurchaseOrdersAsync(Guid? vendorId = null)
        {
            // Get all purchase orders that are received but not billed
            var purchaseOrders = await _purchaseOrderRepository.GetPurchaseOrdersByStatusAsync("Received");
            
            if (vendorId.HasValue)
            {
                purchaseOrders = purchaseOrders.Where(po => po.VendorId == vendorId.Value).ToList();
            }
            
            // Filter out any purchase orders that already have bills
            var billsWithPOs = await _billRepository.GetBillsWithPurchaseOrdersAsync();
            var billedPOIds = billsWithPOs.Select(b => b.PurchaseOrderId.Value).ToHashSet();
            
            return purchaseOrders.Where(po => !billedPOIds.Contains(po.Id)).ToList();
        }

        public async Task<Bill> CreateBillFromPurchaseOrderAsync(Guid purchaseOrderId)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId);
            if (purchaseOrder == null)
            {
                throw new InvalidOperationException($"Purchase order with ID {purchaseOrderId} not found");
            }

            if (purchaseOrder.Status != "Received")
            {
                throw new InvalidOperationException($"Purchase order {purchaseOrder.PurchaseOrderNumber} is not in 'Received' status");
            }

            // Check if purchase order is already billed
            var existingBill = await _billRepository.GetBillByPurchaseOrderIdAsync(purchaseOrderId);
            if (existingBill != null)
            {
                throw new InvalidOperationException($"Purchase order {purchaseOrder.PurchaseOrderNumber} is already billed");
            }

            // Create new bill from purchase order
            var bill = new Bill
            {
                Id = Guid.NewGuid(),
                BillDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30), // Default to 30 days
                VendorId = purchaseOrder.VendorId,
                VendorName = purchaseOrder.VendorName,
                PurchaseOrderId = purchaseOrder.Id,
                TotalAmount = purchaseOrder.TotalAmount,
                Status = "Pending",
                PaymentTerms = "Net 30", // Default
                Notes = $"Created from purchase order {purchaseOrder.PurchaseOrderNumber}",
                CreatedDate = DateTime.UtcNow
            };

            // Get the next sequential bill number
            var lastBill = await _billRepository.GetLastBillNumberAsync();
            var nextNumber = 1;
            
            if (!string.IsNullOrEmpty(lastBill) && int.TryParse(lastBill.Replace("BILL-", ""), out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
            
            bill.BillNumber = $"BILL-{nextNumber:D6}";

            // Create line items from purchase order items
            var poItems = await _purchaseOrderRepository.GetPurchaseOrderItemsAsync(purchaseOrderId);
            
            bill.LineItems = poItems.Select(poItem => new BillLineItem
            {
                Id = Guid.NewGuid(),
                BillId = bill.Id,
                Description = poItem.Description,
                AccountId = poItem.AccountId,
                Quantity = poItem.Quantity,
                UnitPrice = poItem.UnitPrice,
                Amount = poItem.Quantity * poItem.UnitPrice,
                TaxAmount = poItem.TaxAmount,
                TaxCodeId = poItem.TaxCodeId
            }).ToList();

            // Ensure the total amounts match
            bill.TotalAmount = bill.LineItems.Sum(li => li.Amount + li.TaxAmount);
            bill.TaxAmount = bill.LineItems.Sum(li => li.TaxAmount);

            // Save the bill
            var createdBill = await _billRepository.AddAsync(bill);

            // Update purchase order status to "Billed"
            purchaseOrder.Status = "Billed";
            await _purchaseOrderRepository.UpdateAsync(purchaseOrder);

            return createdBill;
        }
    }
}
