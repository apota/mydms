// @ts-nocheck
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Button,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  SelectChangeEvent,
  LinearProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  FilterList as FilterIcon,
  PersonOutline as PersonIcon,
  DirectionsCar as CarIcon,
} from '@mui/icons-material';
import { DataGrid, GridColDef, GridRenderCellParams, GridValueFormatterParams } from '@mui/x-data-grid';
import { fetchDeals } from '../../services/dealService';
import { DealStatus } from '../../types/deal';

const getStatusColor = (status: DealStatus) => {
  switch (status) {
    case DealStatus.Pending:
      return 'warning';
    case DealStatus.Approved:
    case DealStatus.FinancingApproved:
    case DealStatus.Completed:
      return 'success';
    case DealStatus.FinancingRequired:
    case DealStatus.DepositPaid:
      return 'info';
    case DealStatus.FinancingRejected:
    case DealStatus.Cancelled:
      return 'error';
    default:
      return 'default';
  }
};

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

const Deals: React.FC = () => {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<DealStatus | ''>('');

  const { data: deals, isLoading, error } = useQuery(['deals', statusFilter], () =>
    fetchDeals(statusFilter || undefined)
  );

  const handleStatusFilterChange = (event: SelectChangeEvent) => {
    setStatusFilter(event.target.value as DealStatus | '');
  };

  const handleCreateDeal = () => {
    navigate('/deals/create');
  };

  const handleDealClick = (id: string) => {
    navigate(`/deals/${id}`);
  };

  const columns: GridColDef[] = [
    { field: 'dealNumber', headerName: 'Deal #', width: 120 },
    {
      field: 'customer',
      headerName: 'Customer',
      flex: 1,
      renderCell: (params: GridRenderCellParams) => (
        <Box display="flex" alignItems="center">
          <PersonIcon fontSize="small" sx={{ mr: 1 }} />
          <span>{`${params.row.customer.firstName} ${params.row.customer.lastName}`}</span>
        </Box>
      ),
    },
    {
      field: 'vehicle',
      headerName: 'Vehicle',
      flex: 1,
      renderCell: (params: GridRenderCellParams) => (
        <Box display="flex" alignItems="center">
          <CarIcon fontSize="small" sx={{ mr: 1 }} />
          <span>{`${params.row.vehicle.year} ${params.row.vehicle.make} ${params.row.vehicle.model}`}</span>
        </Box>
      ),
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 180,
      renderCell: (params: GridRenderCellParams) => (
        <Chip
          label={params.value}
          color={getStatusColor(params.value as DealStatus)}
          size="small"
        />
      ),
    },
    {
      field: 'total',
      headerName: 'Total',
      width: 130,
      valueFormatter: (params: GridValueFormatterParams) => {
        if (params.value == null) return '';
        return formatCurrency(params.value as number);
      },
    },
    { 
      field: 'type', 
      headerName: 'Type', 
      width: 100 
    },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 110,
      valueFormatter: (params: GridValueFormatterParams) => {
        if (!params.value) return '';
        return new Date(params.value as string).toLocaleDateString();
      },
    },
  ];

  if (error) {
    return (
      <Box p={3}>
        <Typography color="error">Error loading deals: {(error as Error).message}</Typography>
      </Box>
    );
  }

  return (
    <Box p={3}>
      <Grid container spacing={2} alignItems="center" sx={{ mb: 3 }}>
        <Grid item xs>
          <Typography variant="h4" component="h1">
            Deals
          </Typography>
        </Grid>
        <Grid item>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateDeal}
          >
            Create Deal
          </Button>
        </Grid>
      </Grid>

      <Paper sx={{ p: 2, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid item>
            <FilterIcon color="action" />
          </Grid>
          <Grid item xs={12} sm={3}>
            <FormControl fullWidth size="small">
              <InputLabel>Filter by Status</InputLabel>
              <Select
                value={statusFilter}
                label="Filter by Status"
                onChange={handleStatusFilterChange}
              >
                <MenuItem value="">All Statuses</MenuItem>
                {Object.values(DealStatus).map((status) => (
                  <MenuItem key={status} value={status}>
                    {status}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12} sm={3}>
            <TextField
              size="small"
              label="Search Deals"
              variant="outlined"
              fullWidth
            />
          </Grid>
        </Grid>
      </Paper>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={deals || []}
          columns={columns}
          loading={isLoading}
          components={{
            LoadingOverlay: LinearProgress,
          }}
          onRowClick={(params) => handleDealClick(params.id.toString())}
          getRowId={(row) => row.id}
          pageSize={10}
          rowsPerPageOptions={[10, 25, 50]}
          disableSelectionOnClick
        />
      </Paper>
    </Box>
  );
};

export default Deals;
