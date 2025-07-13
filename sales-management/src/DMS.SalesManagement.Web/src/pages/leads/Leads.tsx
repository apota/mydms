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
  Person as PersonIcon,
  FilterList as FilterIcon,
} from '@mui/icons-material';
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import { fetchLeads } from '../../services/leadService';
import { LeadStatus } from '../../types/lead';

const getStatusColor = (status: LeadStatus) => {
  switch (status) {
    case LeadStatus.New:
      return 'primary';
    case LeadStatus.Contacted:
      return 'info';
    case LeadStatus.Qualified:
      return 'success';
    case LeadStatus.Appointment:
      return 'secondary';
    case LeadStatus.Sold:
      return 'success';
    case LeadStatus.Lost:
      return 'error';
    default:
      return 'default';
  }
};

const Leads: React.FC = () => {
  const navigate = useNavigate();
  const [statusFilter, setStatusFilter] = useState<LeadStatus | ''>('');

  const { data: leads, isLoading, error } = useQuery(['leads', statusFilter], () =>
    fetchLeads(statusFilter || undefined)
  );

  const handleStatusFilterChange = (event: SelectChangeEvent) => {
    setStatusFilter(event.target.value as LeadStatus | '');
  };

  const handleCreateLead = () => {
    navigate('/leads/create');
  };

  const handleLeadClick = (id: string) => {
    navigate(`/leads/${id}`);
  };

  const columns: GridColDef[] = [
    {
      field: 'fullName',
      headerName: 'Customer',
      flex: 1,
      valueGetter: (params: any) => `${params.row.firstName || ''} ${params.row.lastName || ''}`,
    },
    { field: 'email', headerName: 'Email', flex: 1 },
    { field: 'phone', headerName: 'Phone', width: 150 },
    {
      field: 'status',
      headerName: 'Status',
      width: 150,
      renderCell: (params: GridRenderCellParams) => (
        <Chip
          label={params.value}
          color={getStatusColor(params.value as LeadStatus)}
          size="small"
        />
      ),
    },
    { 
      field: 'source', 
      headerName: 'Source', 
      width: 150 
    },
    {
      field: 'createdAt',
      headerName: 'Created',
      width: 150,
      valueFormatter: (params) => {
        if (!params.value) return '';
        return new Date(params.value as string).toLocaleDateString();
      },
    },
    {
      field: 'lastActivityDate',
      headerName: 'Last Activity',
      width: 150,
      valueFormatter: (params) => {
        if (!params.value) return '';
        return new Date(params.value as string).toLocaleDateString();
      },
    },
    {
      field: 'assignedSalesRepId',
      headerName: 'Assigned To',
      width: 150,
      renderCell: (params: GridRenderCellParams) => (
        <Box display="flex" alignItems="center">
          <PersonIcon fontSize="small" sx={{ mr: 1 }} />
          <span>{params.value ? params.value : 'Unassigned'}</span>
        </Box>
      ),
    },
  ];

  if (error) {
    return (
      <Box p={3}>
        <Typography color="error">Error loading leads: {(error as Error).message}</Typography>
      </Box>
    );
  }

  return (
    <Box p={3}>
      <Grid container spacing={2} alignItems="center" sx={{ mb: 3 }}>
        <Grid item xs>
          <Typography variant="h4" component="h1">
            Leads
          </Typography>
        </Grid>
        <Grid item>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateLead}
          >
            Create Lead
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
                {Object.values(LeadStatus).map((status) => (
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
              label="Search Leads"
              variant="outlined"
              fullWidth
            />
          </Grid>
        </Grid>
      </Paper>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={leads || []}
          columns={columns}
          loading={isLoading}
          components={{
            LoadingOverlay: LinearProgress,
          }}          onRowClick={(params: any) => handleLeadClick(params.id.toString())}
          getRowId={(row: any) => row.id}
          pageSize={10}
          rowsPerPageOptions={[10, 25, 50]}
          disableSelectionOnClick
        />
      </Paper>
    </Box>
  );
};

export default Leads;
