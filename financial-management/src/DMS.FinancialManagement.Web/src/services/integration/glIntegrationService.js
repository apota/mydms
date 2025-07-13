import axios from 'axios';
import financialService from '../financialService';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'api';

/**
 * Integration service for handling interactions between General Ledger and other financial modules
 */
const glIntegrationService = {
  /**
   * Generate journal entries from Accounts Receivable transactions
   * @param {Object} transactionData - The accounts receivable transaction data
   * @returns {Promise<Object>} - The created journal entry
   */
  createJournalEntryFromARTransaction: async (transactionData) => {
    try {
      // Map AR transaction to journal entry structure
      const journalEntry = {
        entryDate: transactionData.transactionDate || new Date(),
        description: `AR Transaction: ${transactionData.description || transactionData.invoiceNumber}`,
        referenceNumber: transactionData.invoiceNumber,
        lineItems: []
      };

      // Add appropriate line items based on transaction type
      if (transactionData.transactionType === 'INVOICE') {
        // Debit Accounts Receivable account
        journalEntry.lineItems.push({
          accountId: transactionData.accountsReceivableAccountId,
          description: `Invoice ${transactionData.invoiceNumber}`,
          debitAmount: transactionData.totalAmount,
          creditAmount: 0
        });

        // If tax is applicable
        if (transactionData.taxAmount > 0) {
          journalEntry.lineItems.push({
            accountId: transactionData.salesTaxAccountId,
            description: `Tax for invoice ${transactionData.invoiceNumber}`,
            debitAmount: 0,
            creditAmount: transactionData.taxAmount
          });
        }

        // Credit Revenue/Sales account
        journalEntry.lineItems.push({
          accountId: transactionData.revenueAccountId,
          description: `Revenue for invoice ${transactionData.invoiceNumber}`,
          debitAmount: 0,
          creditAmount: transactionData.totalAmount - (transactionData.taxAmount || 0)
        });
      } 
      else if (transactionData.transactionType === 'PAYMENT') {
        // Debit Cash/Bank account
        journalEntry.lineItems.push({
          accountId: transactionData.cashAccountId,
          description: `Payment for invoice ${transactionData.invoiceNumber}`,
          debitAmount: transactionData.paymentAmount,
          creditAmount: 0
        });

        // Credit Accounts Receivable account
        journalEntry.lineItems.push({
          accountId: transactionData.accountsReceivableAccountId,
          description: `Payment applied to invoice ${transactionData.invoiceNumber}`,
          debitAmount: 0,
          creditAmount: transactionData.paymentAmount
        });
      }

      // Create the journal entry
      return await financialService.createJournalEntry(journalEntry);
    } catch (error) {
      console.error('Error creating AR journal entry:', error);
      throw error;
    }
  },

  /**
   * Generate journal entries from Accounts Payable transactions
   * @param {Object} transactionData - The accounts payable transaction data
   * @returns {Promise<Object>} - The created journal entry
   */
  createJournalEntryFromAPTransaction: async (transactionData) => {
    try {
      // Map AP transaction to journal entry structure
      const journalEntry = {
        entryDate: transactionData.transactionDate || new Date(),
        description: `AP Transaction: ${transactionData.description || transactionData.billNumber}`,
        referenceNumber: transactionData.billNumber,
        lineItems: []
      };

      // Add appropriate line items based on transaction type
      if (transactionData.transactionType === 'BILL') {
        // Credit Accounts Payable account
        journalEntry.lineItems.push({
          accountId: transactionData.accountsPayableAccountId,
          description: `Bill ${transactionData.billNumber}`,
          debitAmount: 0,
          creditAmount: transactionData.totalAmount
        });

        // If tax is applicable
        if (transactionData.taxAmount > 0) {
          journalEntry.lineItems.push({
            accountId: transactionData.inputTaxAccountId,
            description: `Tax for bill ${transactionData.billNumber}`,
            debitAmount: transactionData.taxAmount,
            creditAmount: 0
          });
        }

        // Debit Expense account
        journalEntry.lineItems.push({
          accountId: transactionData.expenseAccountId,
          description: `Expense for bill ${transactionData.billNumber}`,
          debitAmount: transactionData.totalAmount - (transactionData.taxAmount || 0),
          creditAmount: 0
        });
      } 
      else if (transactionData.transactionType === 'PAYMENT') {
        // Credit Cash/Bank account
        journalEntry.lineItems.push({
          accountId: transactionData.cashAccountId,
          description: `Payment for bill ${transactionData.billNumber}`,
          debitAmount: 0,
          creditAmount: transactionData.paymentAmount
        });

        // Debit Accounts Payable account
        journalEntry.lineItems.push({
          accountId: transactionData.accountsPayableAccountId,
          description: `Payment applied to bill ${transactionData.billNumber}`,
          debitAmount: transactionData.paymentAmount,
          creditAmount: 0
        });
      }

      // Create the journal entry
      return await financialService.createJournalEntry(journalEntry);
    } catch (error) {
      console.error('Error creating AP journal entry:', error);
      throw error;
    }
  },

  /**
   * Generate journal entries from Inventory transactions
   * @param {Object} transactionData - The inventory transaction data
   * @returns {Promise<Object>} - The created journal entry
   */
  createJournalEntryFromInventoryTransaction: async (transactionData) => {
    try {
      // Map Inventory transaction to journal entry structure
      const journalEntry = {
        entryDate: transactionData.transactionDate || new Date(),
        description: `Inventory Transaction: ${transactionData.description}`,
        referenceNumber: transactionData.referenceNumber,
        lineItems: []
      };

      // Add appropriate line items based on transaction type
      if (transactionData.transactionType === 'PURCHASE') {
        // Debit Inventory account
        journalEntry.lineItems.push({
          accountId: transactionData.inventoryAccountId,
          description: `Inventory purchase: ${transactionData.description}`,
          debitAmount: transactionData.amount,
          creditAmount: 0
        });

        // Credit Accounts Payable account or Cash account
        journalEntry.lineItems.push({
          accountId: transactionData.isPurchaseOnCredit ? 
            transactionData.accountsPayableAccountId : 
            transactionData.cashAccountId,
          description: `Payment for inventory purchase: ${transactionData.description}`,
          debitAmount: 0,
          creditAmount: transactionData.amount
        });
      } 
      else if (transactionData.transactionType === 'SALE') {
        // Credit Inventory account
        journalEntry.lineItems.push({
          accountId: transactionData.inventoryAccountId,
          description: `Inventory sold: ${transactionData.description}`,
          debitAmount: 0,
          creditAmount: transactionData.costAmount
        });

        // Debit Cost of Goods Sold account
        journalEntry.lineItems.push({
          accountId: transactionData.cogsAccountId,
          description: `COGS for inventory sold: ${transactionData.description}`,
          debitAmount: transactionData.costAmount,
          creditAmount: 0
        });
      }
      else if (transactionData.transactionType === 'ADJUSTMENT') {
        if (transactionData.adjustmentType === 'INCREASE') {
          // Debit Inventory account
          journalEntry.lineItems.push({
            accountId: transactionData.inventoryAccountId,
            description: `Inventory adjustment increase: ${transactionData.description}`,
            debitAmount: transactionData.amount,
            creditAmount: 0
          });

          // Credit Inventory Adjustment account
          journalEntry.lineItems.push({
            accountId: transactionData.inventoryAdjustmentAccountId,
            description: `Offset for inventory adjustment: ${transactionData.description}`,
            debitAmount: 0,
            creditAmount: transactionData.amount
          });
        } else {
          // Credit Inventory account
          journalEntry.lineItems.push({
            accountId: transactionData.inventoryAccountId,
            description: `Inventory adjustment decrease: ${transactionData.description}`,
            debitAmount: 0,
            creditAmount: transactionData.amount
          });

          // Debit Inventory Adjustment account
          journalEntry.lineItems.push({
            accountId: transactionData.inventoryAdjustmentAccountId,
            description: `Offset for inventory adjustment: ${transactionData.description}`,
            debitAmount: transactionData.amount,
            creditAmount: 0
          });
        }
      }

      // Create the journal entry
      return await financialService.createJournalEntry(journalEntry);
    } catch (error) {
      console.error('Error creating inventory journal entry:', error);
      throw error;
    }
  },

  /**
   * Generate journal entries from Payroll transactions
   * @param {Object} transactionData - The payroll transaction data
   * @returns {Promise<Object>} - The created journal entry
   */
  createJournalEntryFromPayrollTransaction: async (transactionData) => {
    try {
      // Map Payroll transaction to journal entry structure
      const journalEntry = {
        entryDate: transactionData.payDate || new Date(),
        description: `Payroll Transaction: ${transactionData.description || 'Payroll ' + new Date().toISOString().split('T')[0]}`,
        referenceNumber: transactionData.referenceNumber,
        lineItems: []
      };

      // Debit Payroll Expense account
      journalEntry.lineItems.push({
        accountId: transactionData.payrollExpenseAccountId,
        description: `Gross payroll expense`,
        debitAmount: transactionData.grossAmount,
        creditAmount: 0
      });

      // Credit various withholding accounts
      if (transactionData.incomeTaxWithholding > 0) {
        journalEntry.lineItems.push({
          accountId: transactionData.incomeTaxPayableAccountId,
          description: `Income tax withholding`,
          debitAmount: 0,
          creditAmount: transactionData.incomeTaxWithholding
        });
      }

      if (transactionData.socialSecurityWithholding > 0) {
        journalEntry.lineItems.push({
          accountId: transactionData.socialSecurityPayableAccountId,
          description: `Social security withholding`,
          debitAmount: 0,
          creditAmount: transactionData.socialSecurityWithholding
        });
      }

      if (transactionData.medicareWithholding > 0) {
        journalEntry.lineItems.push({
          accountId: transactionData.medicarePayableAccountId,
          description: `Medicare withholding`,
          debitAmount: 0,
          creditAmount: transactionData.medicareWithholding
        });
      }

      if (transactionData.otherWithholdings > 0) {
        journalEntry.lineItems.push({
          accountId: transactionData.otherWithholdingsPayableAccountId,
          description: `Other withholdings`,
          debitAmount: 0,
          creditAmount: transactionData.otherWithholdings
        });
      }

      // Credit Cash account for net pay
      journalEntry.lineItems.push({
        accountId: transactionData.cashAccountId,
        description: `Net payroll payment`,
        debitAmount: 0,
        creditAmount: transactionData.netAmount
      });

      // Create the journal entry
      return await financialService.createJournalEntry(journalEntry);
    } catch (error) {
      console.error('Error creating payroll journal entry:', error);
      throw error;
    }
  },

  /**
   * Get transactions from other modules that affect a specific General Ledger account
   * @param {string} accountId - The GL account ID
   * @param {Object} dateRange - The date range for the search
   * @returns {Promise<Array>} - Transactions from other modules affecting this account
   */
  getTransactionsForAccount: async (accountId, dateRange) => {
    try {
      const response = await axios.get(
        `${API_BASE_URL}/finance/general-ledger/accounts/${accountId}/transactions`,
        { params: dateRange }
      );
      return response.data;
    } catch (error) {
      console.error('Error fetching account transactions:', error);
      throw error;
    }
  },
};

export default glIntegrationService;
