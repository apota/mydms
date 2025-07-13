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
    /// Implementation of the payment service
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IBillRepository _billRepository;
        private readonly IJournalEntryService _journalEntryService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IBankAccountRepository bankAccountRepository,
            IInvoiceRepository invoiceRepository,
            IBillRepository billRepository,
            IJournalEntryService journalEntryService)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _bankAccountRepository = bankAccountRepository ?? throw new ArgumentNullException(nameof(bankAccountRepository));
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _billRepository = billRepository ?? throw new ArgumentNullException(nameof(billRepository));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
        }

        public async Task<IEnumerable<Payment>> GetPaymentsAsync(string status = null, Guid? entityId = null, string entityType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Convert string status to enum if provided
            PaymentStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
            {
                statusEnum = parsedStatus;
            }
            
            // Convert string entity type to enum if provided
            EntityType? entityTypeEnum = null;
            if (!string.IsNullOrEmpty(entityType) && Enum.TryParse<EntityType>(entityType, true, out var parsedEntityType))
            {
                entityTypeEnum = parsedEntityType;
            }
            
            return await _paymentRepository.GetPaymentsAsync(statusEnum, entityId, entityTypeEnum, fromDate, toDate);
        }

        public async Task<Payment> GetPaymentByIdAsync(Guid id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            // Set default values
            payment.Id = Guid.NewGuid();
            payment.PaymentDate = payment.PaymentDate == default ? DateTime.UtcNow : payment.PaymentDate;
            payment.Status = PaymentStatus.Pending;
            payment.CreatedAt = DateTime.UtcNow;

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

            var createdPayment = await _paymentRepository.AddAsync(payment);
            
            // Create journal entry for the payment if it's not pending
            if (payment.Status != PaymentStatus.Pending)
            {
                await CreateJournalEntryForPaymentAsync(payment);
            }
            
            return createdPayment;
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            var existingPayment = await _paymentRepository.GetByIdAsync(payment.Id);
            if (existingPayment == null)
            {
                throw new InvalidOperationException($"Payment with ID {payment.Id} not found");
            }

            // Don't allow changing certain fields
            payment.PaymentNumber = existingPayment.PaymentNumber;
            payment.CreatedAt = existingPayment.CreatedAt;
            payment.CreatedBy = existingPayment.CreatedBy;
            
            // Update modified timestamp
            payment.UpdatedAt = DateTime.UtcNow;

            // If status is changing to Cleared from Pending, create the journal entry
            if (existingPayment.Status == PaymentStatus.Pending && payment.Status == PaymentStatus.Cleared)
            {
                payment.ProcessedDate = DateTime.UtcNow;
                await CreateJournalEntryForPaymentAsync(payment);
            }
            
            return await _paymentRepository.UpdateAsync(payment);
        }

        public async Task<bool> VoidPaymentAsync(Guid paymentId, string reason)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return false;
            }

            // Only allow voiding if payment is not already voided
            if (payment.Status == PaymentStatus.Voided)
            {
                throw new InvalidOperationException($"Payment {payment.PaymentNumber} is already voided");
            }

            // Start a transaction
            using var transaction = await _paymentRepository.BeginTransactionAsync();

            try
            {
                // Update payment status
                payment.Status = PaymentStatus.Voided;
                payment.UpdatedAt = DateTime.UtcNow;
                payment.Reference = $"{payment.Reference} - VOIDED: {reason}".Trim();
                
                await _paymentRepository.UpdateAsync(payment);
                
                // Reverse associated transactions based on entity type
                if (payment.EntityType == EntityType.Customer)
                {
                    // Get associated invoices
                    var invoicePayments = await _paymentRepository.GetInvoicePaymentsByPaymentIdAsync(payment.Id);
                    
                    foreach (var invoicePayment in invoicePayments)
                    {
                        var invoice = await _invoiceRepository.GetByIdAsync(invoicePayment.InvoiceId);
                        if (invoice != null)
                        {
                            // Reduce the paid amount on the invoice
                            invoice.PaidAmount -= invoicePayment.Amount;
                            
                            // Update invoice status
                            if (invoice.PaidAmount <= 0)
                            {
                                invoice.PaidAmount = 0;
                                invoice.Status = InvoiceStatus.Sent;
                            }
                            else if (invoice.PaidAmount < invoice.TotalAmount)
                            {
                                invoice.Status = InvoiceStatus.Sent;
                            }
                            
                            invoice.UpdatedAt = DateTime.UtcNow;
                            await _invoiceRepository.UpdateAsync(invoice);
                        }
                    }
                }
                else if (payment.EntityType == EntityType.Vendor)
                {
                    // Get associated bills
                    var billPayments = await _billRepository.GetBillPaymentsByPaymentIdAsync(payment.Id);
                    
                    foreach (var billPayment in billPayments)
                    {
                        var bill = await _billRepository.GetByIdAsync(billPayment.BillId);
                        if (bill != null)
                        {
                            // Reduce the paid amount on the bill
                            bill.PaidAmount -= billPayment.Amount;
                            
                            // Update bill status
                            if (bill.PaidAmount <= 0)
                            {
                                bill.PaidAmount = 0;
                                bill.Status = "Pending";
                            }
                            else if (bill.PaidAmount < bill.TotalAmount)
                            {
                                bill.Status = "Partial";
                            }
                            
                            bill.ModifiedDate = DateTime.UtcNow;
                            await _billRepository.UpdateAsync(bill);
                        }
                    }
                }
                
                // Create reversing journal entry
                await CreateReversingJournalEntryForPaymentAsync(payment, reason);

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

        public async Task<BankReconciliation> GetBankReconciliationDataAsync(Guid bankAccountId, DateTime statementDate)
        {
            // Check if bank account exists
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId);
            if (bankAccount == null)
            {
                throw new InvalidOperationException($"Bank account with ID {bankAccountId} not found");
            }

            // Get book balance from the most recent reconciliation
            var lastReconciliation = await _bankAccountRepository.GetLastReconciliationAsync(bankAccountId);
            decimal bookBalance = lastReconciliation?.BookBalance ?? 0;
            
            // Add all transactions since the last reconciliation
            DateTime lastReconciliationDate = lastReconciliation?.StatementDate ?? DateTime.MinValue;
            
            // Get all cleared payments and deposits that haven't been reconciled
            var unreconciledPayments = await _paymentRepository.GetUnreconciledPaymentsByBankAccountAsync(
                bankAccountId, 
                lastReconciliationDate, 
                statementDate);
            
            // Group by direction (payments vs. receipts)
            var payments = unreconciledPayments.Where(p => p.EntityType == EntityType.Vendor).ToList();
            var receipts = unreconciledPayments.Where(p => p.EntityType == EntityType.Customer).ToList();
            
            // Create the reconciliation model
            var reconciliation = new BankReconciliation
            {
                Id = Guid.NewGuid(),
                BankAccountId = bankAccountId,
                StatementDate = statementDate,
                StatementBalance = 0, // To be filled in by the user
                BookBalance = bookBalance + receipts.Sum(r => r.Amount) - payments.Sum(p => p.Amount),
                Status = "Draft",
                UnreconciledPayments = payments,
                UnreconciledReceipts = receipts,
                ReconciledItems = new List<ReconciliationItem>()
            };
            
            return reconciliation;
        }

        public async Task<BankReconciliation> ProcessBankReconciliationAsync(BankReconciliation reconciliation)
        {
            if (reconciliation == null)
            {
                throw new ArgumentNullException(nameof(reconciliation));
            }

            // Verify bank account exists
            var bankAccount = await _bankAccountRepository.GetByIdAsync(reconciliation.BankAccountId);
            if (bankAccount == null)
            {
                throw new InvalidOperationException($"Bank account with ID {reconciliation.BankAccountId} not found");
            }

            // Start a transaction
            using var transaction = await _bankAccountRepository.BeginTransactionAsync();

            try
            {
                // Process all reconciled items
                foreach (var item in reconciliation.ReconciledItems.Where(i => i.IsReconciled))
                {
                    // Update the payment as reconciled
                    var payment = await _paymentRepository.GetByIdAsync(item.PaymentId);
                    if (payment != null)
                    {
                        payment.Reference = $"{payment.Reference} - Reconciled {reconciliation.StatementDate:MM/dd/yyyy}".Trim();
                        payment.UpdatedAt = DateTime.UtcNow;
                        await _paymentRepository.UpdateAsync(payment);
                        
                        // Link payment to this reconciliation
                        await _bankAccountRepository.AddReconciliationItemAsync(
                            reconciliation.Id, 
                            payment.Id, 
                            reconciliation.StatementDate);
                    }
                }

                // Save the reconciliation
                reconciliation.Status = "Completed";
                var savedReconciliation = await _bankAccountRepository.SaveReconciliationAsync(reconciliation);
                
                // Update the bank account with the new balance
                bankAccount.LastReconciliationDate = reconciliation.StatementDate;
                bankAccount.CurrentBalance = reconciliation.BookBalance;
                await _bankAccountRepository.UpdateAsync(bankAccount);

                // Commit the transaction
                await transaction.CommitAsync();
                
                return savedReconciliation;
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task CreateJournalEntryForPaymentAsync(Payment payment)
        {
            // Different journal entries based on payment type
            if (payment.EntityType == EntityType.Customer)
            {
                // Debit Bank Account, Credit Accounts Receivable
                await _journalEntryService.CreateCustomerPaymentJournalEntryAsync(payment);
            }
            else if (payment.EntityType == EntityType.Vendor)
            {
                // Debit Accounts Payable, Credit Bank Account
                await _journalEntryService.CreateVendorPaymentJournalEntryAsync(payment);
            }
        }

        private async Task CreateReversingJournalEntryForPaymentAsync(Payment payment, string reason)
        {
            // Create reversing journal entry
            if (payment.EntityType == EntityType.Customer)
            {
                // Credit Bank Account, Debit Accounts Receivable (reverse of normal payment)
                await _journalEntryService.CreateCustomerPaymentReversalJournalEntryAsync(payment, reason);
            }
            else if (payment.EntityType == EntityType.Vendor)
            {
                // Credit Accounts Payable, Debit Bank Account (reverse of normal payment)
                await _journalEntryService.CreateVendorPaymentReversalJournalEntryAsync(payment, reason);
            }
        }
    }
}
