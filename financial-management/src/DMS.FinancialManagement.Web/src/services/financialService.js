import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'api';

const financialService = {
  // Dashboard related endpoints
  getCashPosition: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/dashboard/cash-position`);
    return response.data;
  },

  getAccountsReceivableAging: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/accounts-receivable/aging-summary`);
    return response.data;
  },

  getAccountsPayableAging: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/bills/aging-summary`);
    return response.data;
  },

  getDailySalesMetrics: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/dashboard/daily-sales`);
    return response.data;
  },

  getMonthlyProfitLoss: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/dashboard/profit-loss`);
    return response.data;
  },

  getBudgetComparison: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/dashboard/budget-comparison`);
    return response.data;
  },

  getRecentTransactions: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/dashboard/recent-transactions`);
    return response.data;
  },

  // Accounts Receivable endpoints
  getInvoices: async (params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/accounts-receivable/invoices`, { params });
    return response.data;
  },

  getInvoiceById: async (id) => {
    const response = await axios.get(`${API_BASE_URL}/finance/accounts-receivable/invoices/${id}`);
    return response.data;
  },

  createInvoice: async (invoice) => {
    const response = await axios.post(`${API_BASE_URL}/finance/accounts-receivable/invoices`, invoice);
    return response.data;
  },

  updateInvoice: async (id, invoice) => {
    const response = await axios.put(`${API_BASE_URL}/finance/accounts-receivable/invoices/${id}`, invoice);
    return response.data;
  },

  deleteInvoice: async (id) => {
    const response = await axios.delete(`${API_BASE_URL}/finance/accounts-receivable/invoices/${id}`);
    return response.data;
  },

  getCreditMemos: async (params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/accounts-receivable/credit-memos`, { params });
    return response.data;
  },

  createCreditMemo: async (creditMemo) => {
    const response = await axios.post(`${API_BASE_URL}/finance/accounts-receivable/credit-memos`, creditMemo);
    return response.data;
  },

  applyPaymentToInvoice: async (invoiceId, payment) => {
    const response = await axios.post(`${API_BASE_URL}/finance/accounts-receivable/invoices/${invoiceId}/payments`, payment);
    return response.data;
  },

  generateCustomerStatement: async (customerId, params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/accounts-receivable/customers/${customerId}/statement`, { params });
    return response.data;
  },

  // Accounts Payable endpoints
  getBills: async (params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/bills`, { params });
    return response.data;
  },

  getBillById: async (id) => {
    const response = await axios.get(`${API_BASE_URL}/finance/bills/${id}`);
    return response.data;
  },

  createBill: async (bill) => {
    const response = await axios.post(`${API_BASE_URL}/finance/bills`, bill);
    return response.data;
  },

  updateBill: async (id, bill) => {
    const response = await axios.put(`${API_BASE_URL}/finance/bills/${id}`, bill);
    return response.data;
  },

  deleteBill: async (id) => {
    const response = await axios.delete(`${API_BASE_URL}/finance/bills/${id}`);
    return response.data;
  },

  getVendorBills: async (vendorId) => {
    const response = await axios.get(`${API_BASE_URL}/finance/vendors/${vendorId}/bills`);
    return response.data;
  },

  // Payments endpoints
  getPayments: async (params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/payments`, { params });
    return response.data;
  },

  getPaymentById: async (id) => {
    const response = await axios.get(`${API_BASE_URL}/finance/payments/${id}`);
    return response.data;
  },

  createPayment: async (payment) => {
    const response = await axios.post(`${API_BASE_URL}/finance/payments`, payment);
    return response.data;
  },

  updatePayment: async (id, payment) => {
    const response = await axios.put(`${API_BASE_URL}/finance/payments/${id}`, payment);
    return response.data;
  },

  voidPayment: async (id) => {
    const response = await axios.post(`${API_BASE_URL}/finance/payments/${id}/void`);
    return response.data;
  },

  // Bank Reconciliation endpoints
  getBankAccounts: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/bank-accounts`);
    return response.data;
  },

  getBankReconciliations: async (bankAccountId) => {
    const response = await axios.get(`${API_BASE_URL}/finance/bank-accounts/${bankAccountId}/reconciliations`);
    return response.data;
  },

  createBankReconciliation: async (bankAccountId, reconciliation) => {
    const response = await axios.post(`${API_BASE_URL}/finance/bank-accounts/${bankAccountId}/reconciliations`, reconciliation);
    return response.data;
  },

  updateBankReconciliation: async (bankAccountId, reconciliationId, reconciliation) => {
    const response = await axios.put(`${API_BASE_URL}/finance/bank-accounts/${bankAccountId}/reconciliations/${reconciliationId}`, reconciliation);
    return response.data;
  },

  // General Ledger endpoints
  getChartOfAccounts: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/general-ledger/chart-of-accounts`);
    return response.data;
  },

  getJournalEntries: async (params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/general-ledger/journal-entries`, { params });
    return response.data;
  },
  createJournalEntry: async (journalEntry) => {
    const response = await axios.post(`${API_BASE_URL}/finance/general-ledger/journal-entries`, journalEntry);
    return response.data;
  },

  getJournalEntryById: async (id) => {
    const response = await axios.get(`${API_BASE_URL}/finance/general-ledger/journal-entries/${id}`);
    return response.data;
  },

  postJournalEntry: async (id, postData) => {
    const response = await axios.post(`${API_BASE_URL}/finance/general-ledger/journal-entries/${id}/post`, postData);
    return response.data;
  },

  reverseJournalEntry: async (id, reverseData) => {
    const response = await axios.post(`${API_BASE_URL}/finance/general-ledger/journal-entries/${id}/reverse`, reverseData);
    return response.data;
  },

  getAccountDetails: async (id) => {
    const response = await axios.get(`${API_BASE_URL}/finance/general-ledger/chart-of-accounts/${id}`);
    return response.data;
  },

  createAccount: async (accountData) => {
    const response = await axios.post(`${API_BASE_URL}/finance/general-ledger/chart-of-accounts`, accountData);
    return response.data;
  },

  updateAccount: async (id, accountData) => {
    const response = await axios.put(`${API_BASE_URL}/finance/general-ledger/chart-of-accounts/${id}`, accountData);
    return response.data;
  },

  deactivateAccount: async (id) => {
    const response = await axios.post(`${API_BASE_URL}/finance/general-ledger/chart-of-accounts/${id}/deactivate`);
    return response.data;
  },

  activateAccount: async (id) => {
    const response = await axios.post(`${API_BASE_URL}/finance/general-ledger/chart-of-accounts/${id}/activate`);
    return response.data;
  },
  
  getGeneralLedgerReport: async (params) => {
    const response = await axios.get(`${API_BASE_URL}/finance/reports/general-ledger`, { params });
    return response.data;
  },

  getTrialBalanceReport: async (params) => {
    const response = await axios.get(`${API_BASE_URL}/finance/reports/trial-balance`, { params });
    return response.data;
  },

  // Tax Management endpoints
  getTaxRates: async () => {
    const response = await axios.get(`${API_BASE_URL}/finance/taxes/rates`);
    return response.data;
  },

  createTaxRate: async (taxRate) => {
    const response = await axios.post(`${API_BASE_URL}/finance/taxes/rates`, taxRate);
    return response.data;
  },

  getTaxReports: async (params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/taxes/reports`, { params });
    return response.data;
  },

  // Financial Reporting endpoints
  getFinancialReports: async (reportType, params = {}) => {
    const response = await axios.get(`${API_BASE_URL}/finance/reports/${reportType}`, { params });
    return response.data;
  }
};

export { financialService };
