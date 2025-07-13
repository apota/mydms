using DMS.FinancialManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing payment operations
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Get all payments with optional filtering
        /// </summary>
        /// <param name="status">Optional status filter</param>
        /// <param name="entityId">Optional entity ID filter (customer or vendor)</param>
        /// <param name="entityType">Optional entity type filter</param>
        /// <param name="fromDate">Optional from date filter</param>
        /// <param name="toDate">Optional to date filter</param>
        /// <returns>Collection of payments</returns>
        Task<IEnumerable<Payment>> GetPaymentsAsync(string status = null, Guid? entityId = null, string entityType = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get payment by ID
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>The payment or null if not found</returns>
        Task<Payment> GetPaymentByIdAsync(Guid id);

        /// <summary>
        /// Create a new payment
        /// </summary>
        /// <param name="payment">Payment to create</param>
        /// <returns>Created payment with assigned ID</returns>
        Task<Payment> CreatePaymentAsync(Payment payment);

        /// <summary>
        /// Update an existing payment
        /// </summary>
        /// <param name="payment">Payment with updated values</param>
        /// <returns>Updated payment</returns>
        Task<Payment> UpdatePaymentAsync(Payment payment);

        /// <summary>
        /// Void a payment
        /// </summary>
        /// <param name="paymentId">ID of payment to void</param>
        /// <param name="reason">Reason for voiding the payment</param>
        /// <returns>True if voided successfully</returns>
        Task<bool> VoidPaymentAsync(Guid paymentId, string reason);

        /// <summary>
        /// Get bank reconciliation data
        /// </summary>
        /// <param name="bankAccountId">Bank account ID</param>
        /// <param name="statementDate">Statement date</param>
        /// <returns>Bank reconciliation data</returns>
        Task<BankReconciliation> GetBankReconciliationDataAsync(Guid bankAccountId, DateTime statementDate);

        /// <summary>
        /// Process bank reconciliation
        /// </summary>
        /// <param name="reconciliation">Reconciliation data with matched transactions</param>
        /// <returns>Updated reconciliation information</returns>
        Task<BankReconciliation> ProcessBankReconciliationAsync(BankReconciliation reconciliation);
    }

    /// <summary>
    /// Bank reconciliation model
    /// </summary>
    public class BankReconciliation
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the bank account ID
        /// </summary>
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Gets or sets the statement date
        /// </summary>
        public DateTime StatementDate { get; set; }

        /// <summary>
        /// Gets or sets the statement balance
        /// </summary>
        public decimal StatementBalance { get; set; }

        /// <summary>
        /// Gets or sets the book balance
        /// </summary>
        public decimal BookBalance { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the list of unreconciled payments
        /// </summary>
        public List<Payment> UnreconciledPayments { get; set; } = new List<Payment>();

        /// <summary>
        /// Gets or sets the list of unreconciled receipts
        /// </summary>
        public List<Payment> UnreconciledReceipts { get; set; } = new List<Payment>();

        /// <summary>
        /// Gets or sets the list of reconciled transactions
        /// </summary>
        public List<ReconciliationItem> ReconciledItems { get; set; } = new List<ReconciliationItem>();
    }

    /// <summary>
    /// Reconciliation item model
    /// </summary>
    public class ReconciliationItem
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the payment ID
        /// </summary>
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the payment date
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the payment number
        /// </summary>
        public string PaymentNumber { get; set; }

        /// <summary>
        /// Gets or sets the payment description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the payment amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets whether the item is reconciled
        /// </summary>
        public bool IsReconciled { get; set; }

        /// <summary>
        /// Gets or sets the reconciled date
        /// </summary>
        public DateTime? ReconciledDate { get; set; }
    }
}
