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
  Add as AddIcon,
  AttachMoney as MoneyIcon
} from '@mui/icons-material';
import { format } from 'date-fns';

import { CoreTrackingService } from '../../services/api';

// Core status options
const coreStatuses = [
  { value: '', label: 'All Statuses' },
  { value: 'PENDING_RETURN', label: 'Pending Return' },
  { value: 'RETURNED', label: 'Returned' },
  { value: 'CREDITED', label: 'Credited' },
  { value: 'REJECTED', label: 'Rejected' },
  { value: 'EXPIRED', label: 'Expired' }
];

// Get color for core status
const getStatusColor = (status) => {
  switch (status) {
    case 'PENDING_RETURN': return 'warning';
    case 'RETURNED': return 'info';
    case 'CREDITED': return 'success';
    case 'REJECTED': return 'error';
    case 'EXPIRED': return 'error';
    default: return 'default';
  }
};

const CoreTrackingList = () => {
  // State for cores data
  const [cores, setCores] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [totalValue, setTotalValue] = useState(0);
  
  // State for filters and pagination
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(25);
  const [filterStatus, setFilterStatus] = useState('');
  const [partNumber, setPartNumber] = useState('');

  // Load cores with filtering
  const loadCores = async () => {
    setLoading(true);
    try {
      let data;
      
      // If filtering by part number
      if (partNumber) {
        // First get the part ID by part number
        try {
          // Note: This assumes your API has a way to lookup part by part number
          const partResponse = await fetch(`/api/parts/number/${encodeURIComponent(partNumber)}`);
          const partData = await partResponse.json();
          
          if (partData && partData.id) {
            // Then get cores for that part
            data = await CoreTrackingService.getCoresByPart(
              partData.id,
              page * rowsPerPage,
              rowsPerPage
            );
          } else {
            setCores([]);
            setError('Part number not found');
            setLoading(false);
            return;
          }
        } catch (err) {
          setCores([]);
          setError('Error finding part by part number');
          setLoading(false);
          return;
        }
      }
      // If filtering by status
      else if (filterStatus) {
        data = await CoreTrackingService.getCoresByStatus(
          filterStatus,
          page * rowsPerPage,
          rowsPerPage
        );
      } 
      // Standard load with no filters
      else {
        data = await CoreTrackingService.getAllCores(
          page * rowsPerPage,
          rowsPerPage
        );
      }
      
      setCores(data.items || []);

      // Get outstanding value
      const valueData = await CoreTrackingService.getOutstandingCoreValue();
      setTotalValue(valueData.totalValue || 0);
      
      setError(null);
    } catch (err) {
      console.error('Failed to fetch core tracking data:', err);
      setError('Failed to load core tracking data. Please try again.');
      setCores([]);
    } finally {
      setLoading(false);
    }
  };

  // Load data on initial render or when filters change
  useEffect(() => {
    loadCores();
  }, [page, rowsPerPage, filterStatus]);

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
    setFilterStatus('');
    setPartNumber('');
    setPage(0);
  };

  // Handle search by part number
  const handlePartNumberSearch = () => {
    setPage(0);
    loadCores();
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3, alignItems: 'center' }}>
        <Typography variant="h4" component="h1">
          Core Tracking
        </Typography>
        <Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            component={Link}
            to="/core-tracking/track"
          >
            Track New Core
          </Button>
        </Box>
      </Box>

      {/* Summary Card */}
      <Paper sx={{ p: 2, mb: 3, backgroundColor: 'primary.light', color: 'primary.contrastText' }}>
        <Grid container alignItems="center">
          <Grid item xs={12} sm={6}>
            <Typography variant="h6">
              Outstanding Core Value
            </Typography>
          </Grid>
          <Grid item xs={12} sm={6} sx={{ textAlign: { xs: 'left', sm: 'right' } }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: { xs: 'flex-start', sm: 'flex-end' } }}>
              <MoneyIcon sx={{ mr: 1, fontSize: '2rem' }} />
              <Typography variant="h4">
                ${totalValue.toFixed(2)}
              </Typography>
            </Box>
          </Grid>
        </Grid>
      </Paper>

      {/* Filters */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>
          <FilterIcon sx={{ verticalAlign: 'middle', mr: 1 }} />
          Filters
        </Typography>
        
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={6} md={4}>
            <TextField
              select
              fullWidth
              label="Status"
              value={filterStatus}
              onChange={(e) => {
                setFilterStatus(e.target.value);
                setPage(0);
              }}
              variant="outlined"
            >
              {coreStatuses.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>
          </Grid>
          
          <Grid item xs={12} sm={6} md={4}>
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
          
          <Grid item xs={12} sm={6} md={4}>
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

      {/* Cores table */}
      <Paper sx={{ width: '100%', overflow: 'hidden' }}>
        {loading ? (
          <Box sx={{ p: 3, textAlign: 'center' }}>
            <Typography>Loading core records...</Typography>
          </Box>
        ) : error ? (
          <Box sx={{ p: 3, textAlign: 'center' }}>
            <Typography color="error">{error}</Typography>
            <Button 
              sx={{ mt: 2 }} 
              variant="outlined" 
              onClick={loadCores}
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
                    <TableCell>Core ID</TableCell>
                    <TableCell>Part Number</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Customer</TableCell>
                    <TableCell>Issue Date</TableCell>
                    <TableCell>Due Date</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Core Value</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {cores.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={9} align="center">
                        No core records found matching your criteria
                      </TableCell>
                    </TableRow>
                  ) : (
                    cores.map((core) => (
                      <TableRow key={core.id} hover>
                        <TableCell>
                          <Link to={`/core-tracking/${core.id}`}>
                            #{core.id.substring(0, 8)}
                          </Link>
                        </TableCell>
                        <TableCell>
                          <Link to={`/parts/${core.partId}`}>
                            {core.partNumber}
                          </Link>
                        </TableCell>
                        <TableCell>{core.partDescription}</TableCell>
                        <TableCell>
                          {core.customerId && (
                            <Tooltip title="View in CRM">
                              <span>{core.customerName}</span>
                            </Tooltip>
                          )}
                          {!core.customerId && core.customerName}
                        </TableCell>
                        <TableCell>
                          {format(new Date(core.issueDate), 'MMM d, yyyy')}
                        </TableCell>
                        <TableCell>
                          {format(new Date(core.dueDate), 'MMM d, yyyy')}
                        </TableCell>
                        <TableCell>
                          <Chip 
                            label={core.status}
                            color={getStatusColor(core.status)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell align="right">
                          ${core.coreValue.toFixed(2)}
                        </TableCell>
                        <TableCell>
                          <Box>
                            <Tooltip title="View Details">
                              <IconButton
                                component={Link}
                                to={`/core-tracking/${core.id}`}
                                size="small"
                              >
                                <SearchIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            
                            {core.status === 'PENDING_RETURN' && (
                              <Tooltip title="Process Return">
                                <IconButton
                                  component={Link}
                                  to={`/core-tracking/${core.id}/return`}
                                  color="primary"
                                  size="small"
                                >
                                  <MoneyIcon fontSize="small" />
                                </IconButton>
                              </Tooltip>
                            )}
                          </Box>
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

export default CoreTrackingList;
