import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Box, 
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Button,
  TextField,
  MenuItem,
  Grid,
  Chip,
  IconButton,
  Tooltip,
  InputAdornment
} from '@mui/material';
import { 
  Search as SearchIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon,
  ReceiptLong as ReceiptIcon,
  CompareArrows as TransferIcon
} from '@mui/icons-material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { format } from 'date-fns';

import { TransactionsService } from '../../services/api';

// Transaction type options
const transactionTypes = [
  { value: '', label: 'All Types' },
  { value: 'RECEIPT', label: 'Receipt' },
  { value: 'ISSUE', label: 'Issue' },
  { value: 'RETURN', label: 'Return' },
  { value: 'ADJUSTMENT', label: 'Adjustment' },
  { value: 'TRANSFER', label: 'Transfer' }
];

// Get color for transaction type
const getTypeColor = (type) => {
  switch(type) {
    case 'RECEIPT': return 'success';
    case 'ISSUE': return 'error';
    case 'RETURN': return 'info';
    case 'ADJUSTMENT': return 'warning';
    case 'TRANSFER': return 'primary';
    default: return 'default';
  }
};

const TransactionsList = () => {
  // State for transactions data
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // State for filters and pagination
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [filterType, setFilterType] = useState('');
  const [startDate, setStartDate] = useState(null);
  const [endDate, setEndDate] = useState(null);
  const [partNumber, setPartNumber] = useState('');

  // Load transactions with filtering
  const loadTransactions = async () => {
    setLoading(true);
    try {
      let data;
      
      // If filtering by part number
      if (partNumber) {
        // First get the part ID by part number
        try {
          // Note: This assumes your API has a way to lookup part by part number
          // If not, you'd need to adjust this logic
          const partResponse = await fetch(`/api/parts/number/${encodeURIComponent(partNumber)}`);
          const partData = await partResponse.json();
          
          if (partData && partData.id) {
            // Then get transactions for that part
            data = await TransactionsService.getTransactionsByPart(
              partData.id,
              page * rowsPerPage,
              rowsPerPage
            );
          } else {
            setTransactions([]);
            setError('Part number not found');
            setLoading(false);
            return;
          }
        } catch (err) {
          setTransactions([]);
          setError('Error finding part by part number');
          setLoading(false);
          return;
        }
      }
      // If filtering by transaction type
      else if (filterType) {
        data = await TransactionsService.getTransactionsByType(
          filterType,
          page * rowsPerPage,
          rowsPerPage
        );
      } 
      // Standard load with possible date filters
      else {
        data = await TransactionsService.getAllTransactions(
          page * rowsPerPage,
          rowsPerPage,
          startDate,
          endDate
        );
      }
      
      setTransactions(data.items || []);
      setError(null);
    } catch (err) {
      console.error('Failed to fetch transactions:', err);
      setError('Failed to load transactions. Please try again.');
      setTransactions([]);
    } finally {
      setLoading(false);
    }
  };

  // Load data on initial render or when filters change
  useEffect(() => {
    loadTransactions();
  }, [page, rowsPerPage, filterType, startDate, endDate]);

  // Handle page change
  const handleChangePage = (event, newPage) => {
    setPage(newPage);
  };

  // Handle rows per page change
  const handleChangeRowsPerPage = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  // Handle filter reset
  const handleResetFilters = () => {
    setFilterType('');
    setStartDate(null);
    setEndDate(null);
    setPartNumber('');
    setPage(0);
  };

  // Handle search by part number
  const handlePartNumberSearch = () => {
    setPage(0);
    loadTransactions();
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3, alignItems: 'center' }}>
        <Typography variant="h4" component="h1">
          Transactions History
        </Typography>
        <Box>
          <Button 
            variant="contained" 
            color="primary"
            component={Link}
            to="/transactions/issue"
            startIcon={<ReceiptIcon />}
            sx={{ mr: 1 }}
          >
            Issue Parts
          </Button>
          <Button 
            variant="outlined" 
            component={Link}
            to="/transactions/transfer"
            startIcon={<TransferIcon />}
          >
            Transfer Parts
          </Button>
        </Box>
      </Box>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          <FilterIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
          Filters
        </Typography>
        
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={6} md={3}>
            <TextField
              select
              fullWidth
              label="Transaction Type"
              value={filterType}
              onChange={(e) => {
                setFilterType(e.target.value);
                setPage(0);
              }}
              variant="outlined"
            >
              {transactionTypes.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
          
          <Grid item xs={12} sm={6} md={2}>
            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <DatePicker
                label="From Date"
                value={startDate}
                onChange={(newValue) => {
                  setStartDate(newValue);
                  setPage(0);
                }}
                renderInput={(params) => <TextField {...params} fullWidth />}
              />
            </LocalizationProvider>
          </Grid>
          
          <Grid item xs={12} sm={6} md={2}>
            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <DatePicker
                label="To Date"
                value={endDate}
                onChange={(newValue) => {
                  setEndDate(newValue);
                  setPage(0);
                }}
                renderInput={(params) => <TextField {...params} fullWidth />}
              />
            </LocalizationProvider>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <TextField
              fullWidth
              label="Part Number"
              value={partNumber}
              onChange={(e) => setPartNumber(e.target.value)}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton 
                      onClick={handlePartNumberSearch}
                      edge="end"
                    >
                      <SearchIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              onKeyPress={(e) => {
                if (e.key === 'Enter') {
                  handlePartNumberSearch();
                }
              }}
            />
          </Grid>
          
          <Grid item xs={12} sm={6} md={2}>
            <Button
              fullWidth
              variant="outlined"
              onClick={handleResetFilters}
              startIcon={<RefreshIcon />}
            >
              Reset Filters
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {/* Transactions table */}
      <Paper sx={{ width: '100%', overflow: 'hidden' }}>
        {loading ? (
          <Box sx={{ p: 3, textAlign: 'center' }}>
            <Typography>Loading transactions...</Typography>
          </Box>
        ) : error ? (
          <Box sx={{ p: 3, textAlign: 'center' }}>
            <Typography color="error">{error}</Typography>
            <Button 
              sx={{ mt: 2 }} 
              variant="outlined" 
              onClick={loadTransactions}
            >
              Retry
            </Button>
          </Box>
        ) : (
          <>
            <TableContainer sx={{ maxHeight: 500 }}>
              <Table stickyHeader>
                <TableHead>
                  <TableRow>
                    <TableCell>Transaction ID</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell>Type</TableCell>
                    <TableCell>Part Number</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell align="right">Quantity</TableCell>
                    <TableCell>Location</TableCell>
                    <TableCell>Reference</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {transactions.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={9} align="center">
                        No transactions found matching your criteria
                      </TableCell>
                    </TableRow>
                  ) : (
                    transactions.map((transaction) => (
                      <TableRow key={transaction.id} hover>
                        <TableCell>
                          <Link to={`/transactions/${transaction.id}`}>
                            #{transaction.id.substring(0, 8)}
                          </Link>
                        </TableCell>
                        <TableCell>
                          {format(new Date(transaction.timestamp), 'MMM d, yyyy h:mm a')}
                        </TableCell>
                        <TableCell>
                          <Chip 
                            label={transaction.type}
                            color={getTypeColor(transaction.type)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Link to={`/parts/${transaction.partId}`}>
                            {transaction.partNumber}
                          </Link>
                        </TableCell>
                        <TableCell>{transaction.partDescription}</TableCell>
                        <TableCell align="right">
                          <Typography
                            sx={{
                              fontWeight: 'bold',
                              color: transaction.quantity > 0 ? 'success.main' : 'error.main'
                            }}
                          >
                            {transaction.quantity > 0 ? '+' : ''}{transaction.quantity}
                          </Typography>
                        </TableCell>
                        <TableCell>{transaction.location}</TableCell>
                        <TableCell>
                          {transaction.referenceType === 'ORDER' && (
                            <Link to={`/orders/${transaction.referenceId}`}>
                              Order #{transaction.referenceId.substring(0, 8)}
                            </Link>
                          )}
                          {transaction.referenceType === 'SERVICE_ORDER' && (
                            <Tooltip title="View in Service Management">
                              <span>Service #{transaction.referenceId.substring(0, 8)}</span>
                            </Tooltip>
                          )}
                          {transaction.referenceType === 'MANUAL' && (
                            <span>Manual - {transaction.referenceId || 'N/A'}</span>
                          )}
                        </TableCell>
                        <TableCell>
                          <Tooltip title="View Details">
                            <IconButton
                              component={Link}
                              to={`/transactions/${transaction.id}`}
                              size="small"
                            >
                              <SearchIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
            <TablePagination
              rowsPerPageOptions={[10, 25, 50, 100]}
              component="div"
              count={-1} // Server-side pagination (total unknown)
              rowsPerPage={rowsPerPage}
              page={page}
              onPageChange={handleChangePage}
              onRowsPerPageChange={handleChangeRowsPerPage}
            />
          </>
        )}
      </Paper>
    </Box>
  );
};

export default TransactionsList;
