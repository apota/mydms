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
    /// Implementation of the accounts receivable service
    /// </summary>
    public class AccountsReceivableService : IAccountsReceivableService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICreditMemoRepository _creditMemoRepository;
        private readonly ICustomerRepository _customerRepository;

        public AccountsReceivableService(
            IInvoiceRepository invoiceRepository,
            IPaymentRepository paymentRepository,
            ICreditMemoRepository creditMemoRepository,
            ICustomerRepository customerRepository)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _creditMemoRepository = creditMemoRepository ?? throw new ArgumentNullException(nameof(creditMemoRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesAsync(string status = null, Guid? customerId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Convert string status to enum if provided
            InvoiceStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }

            return await _invoiceRepository.GetInvoicesAsync(statusEnum, customerId, fromDate, toDate);
        }

        public async Task<Invoice> GetInvoiceByIdAsync(Guid id)
        {
            return await _invoiceRepository.GetByIdAsync(id);
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            // Set default values
            invoice.Id = Guid.NewGuid();
            invoice.InvoiceDate = invoice.InvoiceDate == default ? DateTime.UtcNow : invoice.InvoiceDate;
            invoice.Status = InvoiceStatus.Draft;
            invoice.PaidAmount = 0;
            invoice.CreatedAt = DateTime.UtcNow;

            // Generate invoice number if not provided
            if (string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                // Get the next sequential invoice number - in a real implementation, this would be more robust
                var lastInvoice = await _invoiceRepository.GetLastInvoiceNumberAsync();
                var nextNumber = 1;
                
                if (!string.IsNullOrEmpty(lastInvoice) && int.TryParse(lastInvoice.Replace("INV-", ""), out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                
                invoice.InvoiceNumber = $"INV-{nextNumber:D6}";
            }

            return await _invoiceRepository.AddAsync(invoice);
        }

        public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null)
            {
                throw new ArgumentNullException(nameof(invoice));
            }

            var existingInvoice = await _invoiceRepository.GetByIdAsync(invoice.Id);
            if (existingInvoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {invoice.Id} not found");
            }

            // Don't allow changing paid amount directly - it's calculated from payments
            invoice.PaidAmount = existingInvoice.PaidAmount;
            
            // Don't allow changing invoice number or creation date
            invoice.InvoiceNumber = existingInvoice.InvoiceNumber;
            invoice.CreatedAt = existingInvoice.CreatedAt;
            
            // Update modified timestamp
            invoice.UpdatedAt = DateTime.UtcNow;

            return await _invoiceRepository.UpdateAsync(invoice);
        }

        public async Task<bool> SendInvoiceAsync(Guid invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
            {
                return false;
            }

            // In a real implementation, this would send an email or generate a PDF
            // For now, we'll just update the status
            invoice.Status = InvoiceStatus.Sent;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _invoiceRepository.UpdateAsync(invoice);
            return true;
        }

        public async Task<Payment> RecordPaymentAsync(Payment payment, IEnumerable<Guid> invoiceIds)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            if (invoiceIds == null || !invoiceIds.Any())
            {
                throw new ArgumentException("At least one invoice ID must be provided", nameof(invoiceIds));
            }

            // Set default values
            payment.Id = Guid.NewGuid();
            payment.PaymentDate = payment.PaymentDate == default ? DateTime.UtcNow : payment.PaymentDate;
            payment.Status = PaymentStatus.Cleared;
            payment.EntityType = EntityType.Customer;
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
            using var transaction = await _paymentRepository.BeginTransactionAsync();

            try
            {
                // Add the payment
                var createdPayment = await _paymentRepository.AddAsync(payment);
                
                // Apply payment to invoices
                decimal remainingAmount = payment.Amount;
                foreach (var invoiceId in invoiceIds)
                {
                    var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                    if (invoice == null)
                    {
                        throw new InvalidOperationException($"Invoice with ID {invoiceId} not found");
                    }

                    // Calculate amount to apply to this invoice
                    decimal amountToApply = Math.Min(remainingAmount, invoice.TotalAmount - invoice.PaidAmount);
                    if (amountToApply <= 0)
                    {
                        continue; // Skip if no amount to apply
                    }

                    // Apply payment to invoice
                    invoice.PaidAmount += amountToApply;
                    remainingAmount -= amountToApply;

                    // Update invoice status
                    if (Math.Round(invoice.PaidAmount, 2) >= Math.Round(invoice.TotalAmount, 2))
                    {
                        invoice.Status = InvoiceStatus.Paid;
                    }
                    
                    invoice.UpdatedAt = DateTime.UtcNow;
                    await _invoiceRepository.UpdateAsync(invoice);

                    // Create payment-invoice relationship
                    await _paymentRepository.AddInvoicePaymentAsync(payment.Id, invoice.Id, amountToApply);

                    if (remainingAmount <= 0)
                    {
                        break; // No more payment amount to apply
                    }
                }

                // Commit the transaction
                await transaction.CommitAsync();
                
                return createdPayment;
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<CustomerStatement>> GenerateCustomerStatementsAsync(Guid? customerId = null, DateTime? asOfDate = null)
        {
            // Default to current date if not specified
            var statementDate = asOfDate ?? DateTime.UtcNow.Date;
            
            // Get all relevant customers
            var customers = customerId.HasValue
                ? new[] { await _customerRepository.GetByIdAsync(customerId.Value) }
                : await _customerRepository.GetAllAsync();
            
            // Filter out any null customers (e.g., if a specific customer ID wasn't found)
            customers = customers.Where(c => c != null).ToList();
            
            var statements = new List<CustomerStatement>();
            
            foreach (var customer in customers)
            {
                // Get all transactions for this customer up to the statement date
                var invoices = await _invoiceRepository.GetInvoicesAsync(
                    customerId: customer.Id, 
                    toDate: statementDate);
                
                var payments = await _paymentRepository.GetPaymentsByEntityAsync(
                    entityId: customer.Id, 
                    entityType: EntityType.Customer,
                    toDate: statementDate);
                
                var creditMemos = await _creditMemoRepository.GetCreditMemosByCustomerAsync(
                    customerId: customer.Id,
                    toDate: statementDate);
                
                // Create statement
                var statement = new CustomerStatement
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    CustomerName = customer.Name,
                    StatementDate = statementDate,
                    BeginningBalance = 0, // Will be calculated
                    EndingBalance = 0     // Will be calculated
                };

                // Get beginning balance (30 days before statement date)
                var beginningBalanceDate = statementDate.AddDays(-30);
                decimal beginningBalance = await CalculateCustomerBalanceAsync(customer.Id, beginningBalanceDate);
                statement.BeginningBalance = beginningBalance;
                
                // Create line items for the statement
                decimal runningBalance = beginningBalance;
                var lineItems = new List<StatementLineItem>();
                
                // Add invoice line items
                foreach (var invoice in invoices.Where(i => i.InvoiceDate > beginningBalanceDate))
                {
                    runningBalance += invoice.TotalAmount;
                    lineItems.Add(new StatementLineItem
                    {
                        Date = invoice.InvoiceDate,
                        Description = $"Invoice {invoice.InvoiceNumber}",
                        ReferenceNumber = invoice.InvoiceNumber,
                        Charges = invoice.TotalAmount,
                        Payments = 0,
                        Balance = runningBalance
                    });
                }
                
                // Add payment line items
                foreach (var payment in payments.Where(p => p.PaymentDate > beginningBalanceDate))
                {
                    runningBalance -= payment.Amount;
                    lineItems.Add(new StatementLineItem
                    {
                        Date = payment.PaymentDate,
                        Description = $"Payment {payment.PaymentNumber}",
                        ReferenceNumber = payment.PaymentNumber,
                        Charges = 0,
                        Payments = payment.Amount,
                        Balance = runningBalance
                    });
                }
                
                // Add credit memo line items
                foreach (var creditMemo in creditMemos.Where(cm => cm.IssueDate > beginningBalanceDate))
                {
                    runningBalance -= creditMemo.Amount;
                    lineItems.Add(new StatementLineItem
                    {
                        Date = creditMemo.IssueDate,
                        Description = $"Credit Memo {creditMemo.CreditMemoNumber}",
                        ReferenceNumber = creditMemo.CreditMemoNumber,
                        Charges = 0,
                        Payments = creditMemo.Amount,
                        Balance = runningBalance
                    });
                }
                
                // Sort line items by date
                statement.LineItems = lineItems.OrderBy(li => li.Date).ToList();
                statement.EndingBalance = runningBalance;
                
                statements.Add(statement);
            }
            
            return statements;
        }

        public async Task<ARAgingReport> GetAgingReportAsync(DateTime? asOfDate = null)
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
            
            // Get all outstanding invoices
            var invoices = await _invoiceRepository.GetInvoicesAsync(
                status: InvoiceStatus.Sent.ToString(),
                toDate: reportDate);
            
            // Add overdue invoices
            var overdueInvoices = await _invoiceRepository.GetInvoicesAsync(
                status: InvoiceStatus.Overdue.ToString(),
                toDate: reportDate);
            
            invoices = invoices.Concat(overdueInvoices).ToList();
            
            // Group by customer
            var customerGroups = invoices.GroupBy(i => i.CustomerId);
            
            var report = new ARAgingReport
            {
                AsOfDate = reportDate,
                AgingBuckets = agingBuckets,
                CustomerDetails = new List<CustomerAgingDetail>()
            };
            
            // Calculate aging for each customer
            foreach (var group in customerGroups)
            {
                var customerId = group.Key;
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null) continue;
                
                var customerDetail = new CustomerAgingDetail
                {
                    CustomerId = customerId,
                    CustomerName = customer.Name,
                    Current = 0,
                    Days1To30 = 0,
                    Days31To60 = 0,
                    Days61To90 = 0,
                    DaysOver90 = 0,
                    TotalAmount = 0
                };
                
                // Calculate aging for each invoice
                foreach (var invoice in group)
                {
                    // Calculate remaining amount to be paid
                    decimal remainingAmount = invoice.TotalAmount - invoice.PaidAmount;
                    if (remainingAmount <= 0) continue;
                    
                    // Calculate days outstanding
                    int daysOutstanding = (reportDate - invoice.DueDate).Days;
                    
                    // Assign to appropriate aging bucket
                    if (daysOutstanding <= 0)
                    {
                        customerDetail.Current += remainingAmount;
                    }
                    else if (daysOutstanding <= 30)
                    {
                        customerDetail.Days1To30 += remainingAmount;
                    }
                    else if (daysOutstanding <= 60)
                    {
                        customerDetail.Days31To60 += remainingAmount;
                    }
                    else if (daysOutstanding <= 90)
                    {
                        customerDetail.Days61To90 += remainingAmount;
                    }
                    else
                    {
                        customerDetail.DaysOver90 += remainingAmount;
                    }
                    
                    customerDetail.TotalAmount += remainingAmount;
                }
                
                report.CustomerDetails.Add(customerDetail);
                
                // Update bucket totals
                agingBuckets[0].TotalAmount += customerDetail.Current;
                agingBuckets[1].TotalAmount += customerDetail.Days1To30;
                agingBuckets[2].TotalAmount += customerDetail.Days31To60;
                agingBuckets[3].TotalAmount += customerDetail.Days61To90;
                agingBuckets[4].TotalAmount += customerDetail.DaysOver90;
            }
            
            return report;
        }

        public async Task<CreditMemo> CreateCreditMemoAsync(CreditMemo creditMemo)
        {
            if (creditMemo == null)
            {
                throw new ArgumentNullException(nameof(creditMemo));
            }
            
            // Set default values
            creditMemo.Id = Guid.NewGuid();
            creditMemo.IssueDate = creditMemo.IssueDate == default ? DateTime.UtcNow : creditMemo.IssueDate;
            creditMemo.Status = "Open";
            creditMemo.RemainingAmount = creditMemo.Amount;
            
            // Generate credit memo number if not provided
            if (string.IsNullOrEmpty(creditMemo.CreditMemoNumber))
            {
                // Get the next sequential credit memo number
                var lastCreditMemo = await _creditMemoRepository.GetLastCreditMemoNumberAsync();
                var nextNumber = 1;
                
                if (!string.IsNullOrEmpty(lastCreditMemo) && int.TryParse(lastCreditMemo.Replace("CM-", ""), out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
                
                creditMemo.CreditMemoNumber = $"CM-{nextNumber:D6}";
            }
            
            return await _creditMemoRepository.AddAsync(creditMemo);
        }

        public async Task<bool> ApplyCreditMemoAsync(Guid creditMemoId, IEnumerable<Guid> invoiceIds)
        {
            var creditMemo = await _creditMemoRepository.GetByIdAsync(creditMemoId);
            if (creditMemo == null)
            {
                throw new InvalidOperationException($"Credit memo with ID {creditMemoId} not found");
            }
            
            if (creditMemo.RemainingAmount <= 0)
            {
                throw new InvalidOperationException($"Credit memo {creditMemo.CreditMemoNumber} has no remaining amount to apply");
            }
            
            if (invoiceIds == null || !invoiceIds.Any())
            {
                throw new ArgumentException("At least one invoice ID must be provided", nameof(invoiceIds));
            }
            
            // Start a transaction
            using var transaction = await _creditMemoRepository.BeginTransactionAsync();
            
            try
            {
                decimal remainingAmount = creditMemo.RemainingAmount;
                
                foreach (var invoiceId in invoiceIds)
                {
                    var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                    if (invoice == null)
                    {
                        throw new InvalidOperationException($"Invoice with ID {invoiceId} not found");
                    }
                    
                    // Verify invoice belongs to same customer as credit memo
                    if (invoice.CustomerId != creditMemo.CustomerId)
                    {
                        throw new InvalidOperationException($"Invoice {invoice.InvoiceNumber} does not belong to the same customer as the credit memo");
                    }
                    
                    // Calculate amount to apply to this invoice
                    decimal amountToApply = Math.Min(remainingAmount, invoice.TotalAmount - invoice.PaidAmount);
                    if (amountToApply <= 0)
                    {
                        continue; // Skip if no amount to apply
                    }
                    
                    // Apply credit to invoice
                    invoice.PaidAmount += amountToApply;
                    remainingAmount -= amountToApply;
                    
                    // Update invoice status
                    if (Math.Round(invoice.PaidAmount, 2) >= Math.Round(invoice.TotalAmount, 2))
                    {
                        invoice.Status = InvoiceStatus.Paid;
                    }
                    
                    invoice.UpdatedAt = DateTime.UtcNow;
                    await _invoiceRepository.UpdateAsync(invoice);
                    
                    // Create credit memo application record
                    await _creditMemoRepository.AddCreditMemoApplicationAsync(creditMemo.Id, invoice.Id, amountToApply);
                    
                    if (remainingAmount <= 0)
                    {
                        break; // No more credit to apply
                    }
                }
                
                // Update credit memo
                creditMemo.RemainingAmount = remainingAmount;
                if (remainingAmount <= 0)
                {
                    creditMemo.Status = "Applied";
                }
                
                await _creditMemoRepository.UpdateAsync(creditMemo);
                
                // Commit the transaction
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        private async Task<decimal> CalculateCustomerBalanceAsync(Guid customerId, DateTime asOfDate)
        {
            decimal balance = 0;
            
            // Get all invoices up to the specified date
            var invoices = await _invoiceRepository.GetInvoicesAsync(
                customerId: customerId,
                toDate: asOfDate);
            
            // Get all payments up to the specified date
            var payments = await _paymentRepository.GetPaymentsByEntityAsync(
                entityId: customerId,
                entityType: EntityType.Customer,
                toDate: asOfDate);
            
            // Get all credit memos up to the specified date
            var creditMemos = await _creditMemoRepository.GetCreditMemosByCustomerAsync(
                customerId: customerId,
                toDate: asOfDate);
            
            // Calculate balance
            balance += invoices.Sum(i => i.TotalAmount);
            balance -= payments.Sum(p => p.Amount);
            balance -= creditMemos.Sum(cm => cm.Amount);
            
            return balance;
        }
    }
}
