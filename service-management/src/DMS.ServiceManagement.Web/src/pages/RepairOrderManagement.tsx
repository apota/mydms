// @ts-nocheck
import React, { useState, useEffect } from 'react';
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
  Chip,
  Button,
  CircularProgress,
  TextField,
  InputAdornment,
  IconButton,
  Menu,
  MenuItem,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle
} from '@mui/material';
import { 
  Search as SearchIcon,
  FilterList as FilterIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material';
import { format } from 'date-fns';

// Mock data for repair orders
const mockRepairOrders = [
  {
    id: 'ro-12345',
    number: 'RO-12345',
    customerId: 'cust-12345',
    customerName: 'John Doe',
    vehicleId: 'veh-12345',
    vehicleInfo: '2020 Toyota Camry - ABC123',
    advisorId: 'adv-12345',
    advisorName: 'Mike Smith',
    status: 'Open',
    mileage: 35000,
    openDate: '2025-06-20T09:30:00Z',
    promiseDate: '2025-06-20T16:00:00Z',
    completionDate: null,
    totalEstimatedAmount: 450.75,
    totalActualAmount: 0,
    laborTotal: 350.00,
    partsTotal: 85.50,
    discountTotal: 0,
    taxTotal: 15.25,
    createdAt: '2025-06-20T09:30:00Z',
    updatedAt: '2025-06-20T09:30:00Z'
  },
  {
    id: 'ro-12346',
    number: 'RO-12346',
    customerId: 'cust-67890',
    customerName: 'Jane Smith',
    vehicleId: 'veh-67890',
    vehicleInfo: '2019 Honda Accord - DEF456',
    advisorId: 'adv-12345',
    advisorName: 'Mike Smith',
    status: 'InProgress',
    mileage: 42000,
    openDate: '2025-06-19T14:15:00Z',
    promiseDate: '2025-06-20T12:00:00Z',
    completionDate: null,
    totalEstimatedAmount: 875.25,
    totalActualAmount: 0,
    laborTotal: 600.00,
    partsTotal: 239.95,
    discountTotal: 0,
    taxTotal: 35.30,
    createdAt: '2025-06-19T14:15:00Z',
    updatedAt: '2025-06-19T14:15:00Z'
  },
  {
    id: 'ro-12347',
    number: 'RO-12347',
    customerId: 'cust-24680',
    customerName: 'Robert Johnson',
    vehicleId: 'veh-24680',
    vehicleInfo: '2021 Ford F-150 - GHI789',
    advisorId: 'adv-67890',
    advisorName: 'Sarah Davis',
    status: 'Completed',
    mileage: 18500,
    openDate: '2025-06-18T10:00:00Z',
    promiseDate: '2025-06-18T15:00:00Z',
    completionDate: '2025-06-18T14:30:00Z',
    totalEstimatedAmount: 325.50,
    totalActualAmount: 310.25,
    laborTotal: 240.00,
    partsTotal: 58.75,
    discountTotal: 0,
    taxTotal: 11.50,
    createdAt: '2025-06-18T10:00:00Z',
    updatedAt: '2025-06-18T14:30:00Z'
  },
  {
    id: 'ro-12348',
    number: 'RO-12348',
    customerId: 'cust-13579',
    customerName: 'Emily Wilson',
    vehicleId: 'veh-13579',
    vehicleInfo: '2022 Nissan Altima - JKL012',
    advisorId: 'adv-67890',
    advisorName: 'Sarah Davis',
    status: 'OnHold',
    mileage: 8200,
    openDate: '2025-06-19T08:45:00Z',
    promiseDate: '2025-06-19T17:00:00Z',
    completionDate: null,
    totalEstimatedAmount: 1250.00,
    totalActualAmount: 0,
    laborTotal: 800.00,
    partsTotal: 395.25,
    discountTotal: 0,
    taxTotal: 54.75,
    createdAt: '2025-06-19T08:45:00Z',
    updatedAt: '2025-06-19T10:15:00Z'
  },
  {
    id: 'ro-12349',
    number: 'RO-12349',
    customerId: 'cust-97531',
    customerName: 'David Brown',
    vehicleId: 'veh-97531',
    vehicleInfo: '2018 Chevrolet Malibu - MNO345',
    advisorId: 'adv-12345',
    advisorName: 'Mike Smith',
    status: 'Invoiced',
    mileage: 67500,
    openDate: '2025-06-17T11:30:00Z',
    promiseDate: '2025-06-17T16:00:00Z',
    completionDate: '2025-06-17T15:45:00Z',
    totalEstimatedAmount: 175.50,
    totalActualAmount: 175.50,
    laborTotal: 150.00,
    partsTotal: 15.95,
    discountTotal: 0,
    taxTotal: 9.55,
    createdAt: '2025-06-17T11:30:00Z',
    updatedAt: '2025-06-17T15:45:00Z'
  }
];

const STATUS_COLORS = {
  Open: 'primary',
  InProgress: 'warning',
  OnHold: 'error',
  Completed: 'success',
  Invoiced: 'info',
  Closed: 'default'
};

const RepairOrderStatusChip = ({ status }) => (
  <Chip 
    label={status} 
    color={STATUS_COLORS[status] || 'default'} 
    size="small" 
    sx={{ fontWeight: 'bold' }} 
  />
);

const RepairOrderManagement = () => {
  const [repairOrders, setRepairOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterAnchorEl, setFilterAnchorEl] = useState(null);
  const [statusFilter, setStatusFilter] = useState('All');
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [statusDialogOpen, setStatusDialogOpen] = useState(false);
  const [newStatus, setNewStatus] = useState('');
  
  const filterOpen = Boolean(filterAnchorEl);
  
  // Fetch repair orders (mock)
  useEffect(() => {
    const fetchRepairOrders = async () => {
      try {
        setLoading(true);
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));
        setRepairOrders(mockRepairOrders);
      } catch (err) {
        setError('Failed to load repair orders. Please try again.');
        console.error('Error fetching repair orders:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchRepairOrders();
  }, []);
  
  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
  };
  
  const handleFilterClick = (e) => {
    setFilterAnchorEl(e.currentTarget);
  };
  
  const handleFilterClose = () => {
    setFilterAnchorEl(null);
  };
  
  const handleStatusFilterChange = (status) => {
    setStatusFilter(status);
    handleFilterClose();
  };
  
  const handleRefresh = () => {
    setLoading(true);
    // Simulate API call
    setTimeout(() => {
      setRepairOrders(mockRepairOrders);
      setLoading(false);
    }, 1000);
  };
  
  const handleStatusChangeClick = (order) => {
    setSelectedOrder(order);
    setNewStatus(order.status);
    setStatusDialogOpen(true);
  };
  
  const handleStatusDialogClose = () => {
    setStatusDialogOpen(false);
  };
  
  const handleStatusChange = () => {
    if (selectedOrder && newStatus) {
      // Update repair order status (mock)
      const updatedOrders = repairOrders.map(order => 
        order.id === selectedOrder.id 
          ? { ...order, status: newStatus, updatedAt: new Date().toISOString() }
          : order
      );
      
      setRepairOrders(updatedOrders);
      setStatusDialogOpen(false);
    }
  };
  
  // Filter repair orders
  const filteredRepairOrders = repairOrders.filter(order => {
    // Apply search term filter
    const searchTermMatch = 
      order.number.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.vehicleInfo.toLowerCase().includes(searchTerm.toLowerCase());
    
    // Apply status filter
    const statusMatch = statusFilter === 'All' || order.status === statusFilter;
    
    return searchTermMatch && statusMatch;
  });
  
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <CircularProgress />
      </Box>
    );
  }
  
  if (error) {
    return (
      <Box sx={{ mt: 4, textAlign: 'center' }}>
        <Typography color="error">{error}</Typography>
        <Button 
          variant="contained" 
          onClick={handleRefresh} 
          sx={{ mt: 2 }}
        >
          Retry
        </Button>
      </Box>
    );
  }
  
  return (
    <Box sx={{ flexGrow: 1, p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Repair Order Management
      </Typography>
      
      <Box sx={{ mb: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <TextField
          placeholder="Search by RO#, customer, or vehicle"
          variant="outlined"
          size="small"
          value={searchTerm}
          onChange={handleSearch}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
          sx={{ width: 300 }}
        />
        
        <Box>
          <Button 
            variant="outlined" 
            startIcon={<FilterIcon />}
            onClick={handleFilterClick}
            sx={{ mr: 1 }}
          >
            Filter: {statusFilter}
          </Button>
          
          <Menu
            anchorEl={filterAnchorEl}
            open={filterOpen}
            onClose={handleFilterClose}
          >
            <MenuItem onClick={() => handleStatusFilterChange('All')}>All</MenuItem>
            <MenuItem onClick={() => handleStatusFilterChange('Open')}>Open</MenuItem>
            <MenuItem onClick={() => handleStatusFilterChange('InProgress')}>In Progress</MenuItem>
            <MenuItem onClick={() => handleStatusFilterChange('OnHold')}>On Hold</MenuItem>
            <MenuItem onClick={() => handleStatusFilterChange('Completed')}>Completed</MenuItem>
            <MenuItem onClick={() => handleStatusFilterChange('Invoiced')}>Invoiced</MenuItem>
            <MenuItem onClick={() => handleStatusFilterChange('Closed')}>Closed</MenuItem>
          </Menu>
          
          <Button 
            variant="outlined" 
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
          >
            Refresh
          </Button>
        </Box>
      </Box>
      
      <TableContainer component={Paper}>
        <Table sx={{ minWidth: 650 }}>
          <TableHead>
            <TableRow>
              <TableCell>RO #</TableCell>
              <TableCell>Customer</TableCell>
              <TableCell>Vehicle</TableCell>
              <TableCell>Open Date</TableCell>
              <TableCell>Promise Date</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Estimated Amount</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredRepairOrders.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  No repair orders found.
                </TableCell>
              </TableRow>
            ) : (
              filteredRepairOrders.map((order) => (
                <TableRow key={order.id}>
                  <TableCell>{order.number}</TableCell>
                  <TableCell>{order.customerName}</TableCell>
                  <TableCell>{order.vehicleInfo}</TableCell>
                  <TableCell>{format(new Date(order.openDate), 'MM/dd/yyyy h:mm a')}</TableCell>
                  <TableCell>{format(new Date(order.promiseDate), 'MM/dd/yyyy h:mm a')}</TableCell>
                  <TableCell>
                    <RepairOrderStatusChip status={order.status} />
                  </TableCell>
                  <TableCell align="right">${order.totalEstimatedAmount.toFixed(2)}</TableCell>
                  <TableCell align="right">
                    <Button 
                      size="small" 
                      onClick={() => handleStatusChangeClick(order)}
                    >
                      Change Status
                    </Button>
                    <Button 
                      size="small" 
                      variant="contained" 
                      sx={{ ml: 1 }}
                    >
                      View
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      
      <Dialog open={statusDialogOpen} onClose={handleStatusDialogClose}>
        <DialogTitle>Update Repair Order Status</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Change the status for repair order {selectedOrder?.number}:
          </DialogContentText>
          <TextField
            select
            fullWidth
            value={newStatus}
            onChange={(e) => setNewStatus(e.target.value)}
            margin="normal"
          >
            <MenuItem value="Open">Open</MenuItem>
            <MenuItem value="InProgress">In Progress</MenuItem>
            <MenuItem value="OnHold">On Hold</MenuItem>
            <MenuItem value="Completed">Completed</MenuItem>
            <MenuItem value="Invoiced">Invoiced</MenuItem>
            <MenuItem value="Closed">Closed</MenuItem>
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleStatusDialogClose}>Cancel</Button>
          <Button onClick={handleStatusChange} variant="contained">Update</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default RepairOrderManagement;
