using DMS.FinancialManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.FinancialManagement.Core.Services
{
    /// <summary>
    /// Service interface for managing accounts payable operations
    /// </summary>
    public interface IAccountsPayableService
    {
        /// <summary>
        /// Get all bills with optional filtering
        /// </summary>
        /// <param name="status">Optional status filter</param>
        /// <param name="vendorId">Optional vendor ID filter</param>
        /// <param name="fromDate">Optional from date filter</param>
        /// <param name="toDate">Optional to date filter</param>
        /// <returns>Collection of bills</returns>
        Task<IEnumerable<Bill>> GetBillsAsync(string status = null, Guid? vendorId = null, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Get bill by ID
        /// </summary>
        /// <param name="id">Bill ID</param>
        /// <returns>The bill or null if not found</returns>
        Task<Bill> GetBillByIdAsync(Guid id);

        /// <summary>
        /// Create a new bill
        /// </summary>
        /// <param name="bill">Bill to create</param>
        /// <returns>Created bill with assigned ID</returns>
        Task<Bill> CreateBillAsync(Bill bill);

        /// <summary>
        /// Update an existing bill
        /// </summary>
        /// <param name="bill">Bill with updated values</param>
        /// <returns>Updated bill</returns>
        Task<Bill> UpdateBillAsync(Bill bill);

        /// <summary>
        /// Pay a bill
        /// </summary>
        /// <param name="billId">ID of bill to pay</param>
        /// <param name="payment">Payment information</param>
        /// <returns>Updated bill</returns>
        Task<Bill> PayBillAsync(Guid billId, Payment payment);

        /// <summary>
        /// Generate vendor aging report
        /// </summary>
        /// <param name="asOfDate">Report as of date</param>
        /// <returns>Aging report data</returns>
        Task<VendorAgingReport> GetVendorAgingReportAsync(DateTime? asOfDate = null);
        
        /// <summary>
        /// Get purchase orders that can be billed
        /// </summary>
        /// <param name="vendorId">Optional vendor ID filter</param>
        /// <returns>Collection of billable purchase orders</returns>
        Task<IEnumerable<PurchaseOrder>> GetBillablePurchaseOrdersAsync(Guid? vendorId = null);
        
        /// <summary>
        /// Create bill from purchase order
        /// </summary>
        /// <param name="purchaseOrderId">ID of purchase order to bill</param>
        /// <returns>Created bill</returns>
        Task<Bill> CreateBillFromPurchaseOrderAsync(Guid purchaseOrderId);
    }

    /// <summary>
    /// Bill model
    /// </summary>
    public class Bill
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the bill number
        /// </summary>
        public string BillNumber { get; set; }

        /// <summary>
        /// Gets or sets the vendor's invoice number
        /// </summary>
        public string VendorInvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets the bill date
        /// </summary>
        public DateTime BillDate { get; set; }

        /// <summary>
        /// Gets or sets the due date
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the vendor ID
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets the purchase order ID
        /// </summary>
        public Guid? PurchaseOrderId { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the paid amount
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the payment terms
        /// </summary>
        public string PaymentTerms { get; set; }

        /// <summary>
        /// Gets or sets the notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the created by
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the modified date
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the modified by
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the list of line items
        /// </summary>
        public List<BillLineItem> LineItems { get; set; } = new List<BillLineItem>();
    }

    /// <summary>
    /// Bill line item
    /// </summary>
    public class BillLineItem
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the bill ID
        /// </summary>
        public Guid BillId { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the account ID
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax code ID
        /// </summary>
        public Guid? TaxCodeId { get; set; }
    }

    /// <summary>
    /// Purchase order model
    /// </summary>
    public class PurchaseOrder
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the purchase order number
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Gets or sets the order date
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the expected delivery date
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Gets or sets the vendor ID
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the notes
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Vendor aging report
    /// </summary>
    public class VendorAgingReport
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
        /// Gets or sets the vendor aging details
        /// </summary>
        public List<VendorAgingDetail> VendorDetails { get; set; } = new List<VendorAgingDetail>();
    }

    /// <summary>
    /// Vendor aging detail
    /// </summary>
    public class VendorAgingDetail
    {
        /// <summary>
        /// Gets or sets the vendor ID
        /// </summary>
        public Guid VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor name
        /// </summary>
        public string VendorName { get; set; }

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
}
