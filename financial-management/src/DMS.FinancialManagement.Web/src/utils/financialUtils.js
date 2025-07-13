/**
 * Format a date to display in the UI
 * @param {Date|string} date - Date to format
 * @param {boolean} includeTime - Whether to include the time in the formatted date
 * @returns {string} - Formatted date string
 */
export const formatDate = (date, includeTime = false) => {
  if (!date) return '';
  
  const dateObj = typeof date === 'string' ? new Date(date) : date;
  
  if (isNaN(dateObj.getTime())) {
    return '';
  }
  
  const options = {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  };
  
  if (includeTime) {
    options.hour = '2-digit';
    options.minute = '2-digit';
    options.hour12 = true;
  }
  
  return dateObj.toLocaleDateString('en-US', options);
};

/**
 * Format a currency value for display
 * @param {number} amount - Amount to format
 * @param {string} currencyCode - Currency code (default: USD)
 * @returns {string} - Formatted currency string
 */
export const formatCurrency = (amount, currencyCode = 'USD') => {
  if (amount === null || amount === undefined) {
    return '';
  }
  
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currencyCode
  }).format(amount);
};

/**
 * Get the first day of the current month
 * @returns {Date} - First day of the current month
 */
export const getFirstDayOfMonth = () => {
  const date = new Date();
  return new Date(date.getFullYear(), date.getMonth(), 1);
};

/**
 * Get the last day of the current month
 * @returns {Date} - Last day of the current month
 */
export const getLastDayOfMonth = () => {
  const date = new Date();
  return new Date(date.getFullYear(), date.getMonth() + 1, 0);
};

/**
 * Get the first day of the current year
 * @returns {Date} - First day of the current year
 */
export const getFirstDayOfYear = () => {
  const date = new Date();
  return new Date(date.getFullYear(), 0, 1);
};

/**
 * Get the last day of the current year
 * @returns {Date} - Last day of the current year
 */
export const getLastDayOfYear = () => {
  const date = new Date();
  return new Date(date.getFullYear(), 11, 31);
};

/**
 * Get the first and last day of a fiscal period
 * @param {string} periodCode - Fiscal period code (e.g., 'FY2023-01')
 * @param {string} fiscalYearStartMonth - Month when fiscal year starts (1-12)
 * @returns {Object} - Object with startDate and endDate properties
 */
export const getFiscalPeriodDates = (periodCode, fiscalYearStartMonth = 1) => {
  // Extract year and period from the code (format: 'FY2023-01')
  const match = periodCode.match(/FY(\d{4})-(\d{2})/);
  
  if (!match) {
    return { startDate: null, endDate: null };
  }
  
  const fiscalYear = parseInt(match[1], 10);
  const periodMonth = parseInt(match[2], 10) - 1; // 0-indexed
  
  // Calculate the actual calendar month based on fiscal year start
  const calendarMonth = (periodMonth + (fiscalYearStartMonth - 1)) % 12;
  const calendarYear = fiscalYear + Math.floor((periodMonth + (fiscalYearStartMonth - 1)) / 12);
  
  // Create start and end dates
  const startDate = new Date(calendarYear, calendarMonth, 1);
  const endDate = new Date(calendarYear, calendarMonth + 1, 0); // Last day of month
  
  return { startDate, endDate };
};

/**
 * Calculate date range based on a predefined period
 * @param {string} period - Predefined period ('current-month', 'previous-month', 'ytd', 'last-30-days', 'last-90-days', 'custom')
 * @param {Date} customStartDate - Custom start date (only used if period is 'custom')
 * @param {Date} customEndDate - Custom end date (only used if period is 'custom')
 * @returns {Object} - Object with startDate and endDate properties
 */
export const calculateDateRange = (period, customStartDate = null, customEndDate = null) => {
  const today = new Date();
  
  switch (period) {
    case 'current-month':
      return {
        startDate: getFirstDayOfMonth(),
        endDate: today
      };
      
    case 'previous-month':
      const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, 1);
      return {
        startDate: lastMonth,
        endDate: new Date(today.getFullYear(), today.getMonth(), 0)
      };
      
    case 'ytd':
      return {
        startDate: getFirstDayOfYear(),
        endDate: today
      };
      
    case 'last-30-days':
      const thirtyDaysAgo = new Date();
      thirtyDaysAgo.setDate(today.getDate() - 30);
      return {
        startDate: thirtyDaysAgo,
        endDate: today
      };
      
    case 'last-90-days':
      const ninetyDaysAgo = new Date();
      ninetyDaysAgo.setDate(today.getDate() - 90);
      return {
        startDate: ninetyDaysAgo,
        endDate: today
      };
      
    case 'custom':
      return {
        startDate: customStartDate,
        endDate: customEndDate
      };
      
    default:
      return {
        startDate: getFirstDayOfMonth(),
        endDate: today
      };
  }
};

/**
 * Validate if a journal entry is balanced (debits = credits)
 * @param {Object} journalEntry - Journal entry object
 * @returns {boolean} - True if the entry is balanced, false otherwise
 */
export const isJournalEntryBalanced = (journalEntry) => {
  if (!journalEntry || !journalEntry.lineItems || !journalEntry.lineItems.length) {
    return false;
  }
  
  let totalDebits = 0;
  let totalCredits = 0;
  
  journalEntry.lineItems.forEach(item => {
    totalDebits += parseFloat(item.debitAmount || 0);
    totalCredits += parseFloat(item.creditAmount || 0);
  });
  
  // Allow for small rounding differences (e.g., 0.01)
  return Math.abs(totalDebits - totalCredits) < 0.01;
};

/**
 * Calculate totals for a journal entry
 * @param {Object} journalEntry - Journal entry object
 * @returns {Object} - Object with totalDebits and totalCredits properties
 */
export const calculateJournalEntryTotals = (journalEntry) => {
  if (!journalEntry || !journalEntry.lineItems) {
    return { totalDebits: 0, totalCredits: 0 };
  }
  
  let totalDebits = 0;
  let totalCredits = 0;
  
  journalEntry.lineItems.forEach(item => {
    totalDebits += parseFloat(item.debitAmount || 0);
    totalCredits += parseFloat(item.creditAmount || 0);
  });
  
  return { totalDebits, totalCredits };
};
