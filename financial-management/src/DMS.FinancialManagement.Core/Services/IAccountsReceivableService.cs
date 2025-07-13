using DMS.FinancialManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing accounts receivable operations
    /// </summary>
    public interface IAccountsReceivableService
    {
        /// <summary>
        /// Get all invoices with optional filtering
        /// </summary>
        /// <param name="status">Optional status filter</param>
        /// <param name="customerId">Optional customer ID filter</param>
        /// <param name="fromDate">Optional from date filter</param>
        /// <param name="toDate">Optional to date filter</param>
        /// <returns>Collection of invoices</returns>
        Task<IEnumerable<Invoice>> GetInvoicesAsync(string status = null, Guid? customerId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>The invoice or null if not found</returns>
        Task<Invoice> GetInvoiceByIdAsync(Guid id);

        /// <summary>
        /// Create a new invoice
        /// </summary>
        /// <param name="invoice">Invoice to create</param>
        /// <returns>Created invoice with assigned ID</returns>
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);

        /// <summary>
        /// Update an existing invoice
        /// </summary>
        /// <param name="invoice">Invoice with updated values</param>
        /// <returns>Updated invoice</returns>
        Task<Invoice> UpdateInvoiceAsync(Invoice invoice);

        /// <summary>
        /// Send invoice to customer
        /// </summary>
        /// <param name="invoiceId">ID of invoice to send</param>
        /// <returns>True if sent successfully</returns>
        Task<bool> SendInvoiceAsync(Guid invoiceId);

        /// <summary>
        /// Record payment against invoice(s)
        /// </summary>
        /// <param name="payment">Payment information</param>
        /// <param name="invoiceIds">List of invoice IDs to apply payment to</param>
        /// <returns>Updated payment record</returns>
        Task<Payment> RecordPaymentAsync(Payment payment, IEnumerable<Guid> invoiceIds);

        /// <summary>
        /// Generate customer statements
        /// </summary>
        /// <param name="customerId">Optional customer ID to filter by</param>
        /// <param name="asOfDate">Statement as of date</param>
        /// <returns>Collection of customer statements</returns>
        Task<IEnumerable<CustomerStatement>> GenerateCustomerStatementsAsync(Guid? customerId = null, DateTime? asOfDate = null);

        /// <summary>
        /// Get aging report for accounts receivable
        /// </summary>
        /// <param name="asOfDate">Report as of date</param>
        /// <returns>Aging report data</returns>
        Task<ARAgingReport> GetAgingReportAsync(DateTime? asOfDate = null);

        /// <summary>
        /// Create a credit memo
        /// </summary>
        /// <param name="creditMemo">Credit memo to create</param>
        /// <returns>Created credit memo</returns>
        Task<CreditMemo> CreateCreditMemoAsync(CreditMemo creditMemo);

        /// <summary>
        /// Apply credit memo to invoice(s)
        /// </summary>
        /// <param name="creditMemoId">ID of credit memo to apply</param>
        /// <param name="invoiceIds">List of invoice IDs to apply credit to</param>
        /// <returns>True if applied successfully</returns>
        Task<bool> ApplyCreditMemoAsync(Guid creditMemoId, IEnumerable<Guid> invoiceIds);
    }

    /// <summary>
    /// Customer statement model
    /// </summary>
    public class CustomerStatement
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the statement date
        /// </summary>
        public DateTime StatementDate { get; set; }

        /// <summary>
        /// Gets or sets the beginning balance
        /// </summary>
        public decimal BeginningBalance { get; set; }

        /// <summary>
        /// Gets or sets the ending balance
        /// </summary>
        public decimal EndingBalance { get; set; }

        /// <summary>
        /// Gets or sets the statement line items
        /// </summary>
        public List<StatementLineItem> LineItems { get; set; } = new List<StatementLineItem>();
    }

    /// <summary>
    /// Statement line item
    /// </summary>
    public class StatementLineItem
    {
        /// <summary>
        /// Gets or sets the date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the reference number
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the charges amount
        /// </summary>
        public decimal Charges { get; set; }

        /// <summary>
        /// Gets or sets the payments amount
        /// </summary>
        public decimal Payments { get; set; }

        /// <summary>
        /// Gets or sets the balance
        /// </summary>
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// Accounts receivable aging report
    /// </summary>
    public class ARAgingReport
    {
        /// <summary>
        /// Gets or sets the as of date
        /// </summary>
        public DateTime AsOfDate { get; set; }

        /// <summary>
        /// Gets or sets the aging buckets
        /// </summary>
        public List<AgingBucket> AgingBuckets { get; set; } = new List<AgingBucket>();

        /// <summary>
        /// Gets or sets the customer aging details
        /// </summary>
        public List<CustomerAgingDetail> CustomerDetails { get; set; } = new List<CustomerAgingDetail>();
    }

    /// <summary>
    /// Aging bucket
    /// </summary>
    public class AgingBucket
    {
        /// <summary>
        /// Gets or sets the bucket name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start days
        /// </summary>
        public int StartDays { get; set; }

        /// <summary>
        /// Gets or sets the end days
        /// </summary>
        public int EndDays { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Customer aging detail
    /// </summary>
    public class CustomerAgingDetail
    {
        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the current amount
        /// </summary>
        public decimal Current { get; set; }

        /// <summary>
        /// Gets or sets the 1-30 days amount
        /// </summary>
        public decimal Days1To30 { get; set; }

        /// <summary>
        /// Gets or sets the 31-60 days amount
        /// </summary>
        public decimal Days31To60 { get; set; }

        /// <summary>
        /// Gets or sets the 61-90 days amount
        /// </summary>
        public decimal Days61To90 { get; set; }

        /// <summary>
        /// Gets or sets the over 90 days amount
        /// </summary>
        public decimal DaysOver90 { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Credit memo model
    /// </summary>
    public class CreditMemo
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the credit memo number
        /// </summary>
        public string CreditMemoNumber { get; set; }

        /// <summary>
        /// Gets or sets the issue date
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Gets or sets the customer ID
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the remaining amount
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Gets or sets the original invoice ID (if applicable)
        /// </summary>
        public Guid? OriginalInvoiceId { get; set; }
    }
}
