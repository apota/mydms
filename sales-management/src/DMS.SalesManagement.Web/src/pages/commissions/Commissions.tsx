// @ts-nocheck 
import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Tabs,
  Tab,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Card,
  CardContent,
  LinearProgress,
  Divider,
  SelectChangeEvent,
  CircularProgress,
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import {
  FilterList as FilterIcon,
  PeopleOutline as PeopleIcon,
  AttachMoney as MoneyIcon,
} from '@mui/icons-material';
import { DataGrid, GridColDef, GridRenderCellParams, GridValueFormatterParams } from '@mui/x-data-grid';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
} from 'chart.js';
import { Bar, Doughnut } from 'react-chartjs-2';
import { fetchCommissions, fetchCommissionSummaries } from '../../services/commissionService';
import { CommissionStatus, CommissionType } from '../../types/commission';

// Register ChartJS components
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement
);

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`commission-tabpanel-${index}`}
      aria-labelledby={`commission-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
  }).format(amount);
};

const getStatusColor = (status: CommissionStatus) => {
  switch (status) {
    case CommissionStatus.Approved:
      return 'primary';
    case CommissionStatus.Paid:
      return 'success';
    case CommissionStatus.Pending:
      return 'warning';
    case CommissionStatus.Denied:
      return 'error';
    case CommissionStatus.Adjusted:
      return 'info';
    default:
      return 'default';
  }
};

const Commissions: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [statusFilter, setStatusFilter] = useState<CommissionStatus | ''>('');
  const [typeFilter, setTypeFilter] = useState<CommissionType | ''>('');
  const [startDate, setStartDate] = useState<Date | null>(
    new Date(new Date().getFullYear(), new Date().getMonth(), 1)
  );
  const [endDate, setEndDate] = useState<Date | null>(new Date());

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleStatusFilterChange = (event: SelectChangeEvent) => {
    setStatusFilter(event.target.value as CommissionStatus | '');
  };

  const handleTypeFilterChange = (event: SelectChangeEvent) => {
    setTypeFilter(event.target.value as CommissionType | '');
  };

  // Format dates for API calls
  const formattedStartDate = startDate ? startDate.toISOString().split('T')[0] : undefined;
  const formattedEndDate = endDate ? endDate.toISOString().split('T')[0] : undefined;

  // Fetch commissions list
  const { data: commissions, isLoading: isLoadingCommissions } = useQuery(
    ['commissions', statusFilter, typeFilter, formattedStartDate, formattedEndDate],
    () => fetchCommissions(undefined, undefined, statusFilter || undefined, formattedStartDate, formattedEndDate)
  );

  // Fetch commission summaries for dashboard
  const { data: summaries, isLoading: isLoadingSummaries } = useQuery(
    ['commissionSummaries', formattedStartDate, formattedEndDate],
    () => fetchCommissionSummaries(formattedStartDate, formattedEndDate)
  );

  // Columns for the commissions data grid
  const columns: GridColDef[] = [
    {
      field: 'salesPersonId',
      headerName: 'Sales Person',
      flex: 1,
      // This would normally map to actual sales person names from your backend
      valueFormatter: (params) => {
        if (!params.value) return '';
        // Mock data mapping
        const salesPeople: Record<string, string> = {
          'user1': 'John Smith',
          'user2': 'Jane Doe',
          'user3': 'Bob Johnson'
        };
        return salesPeople[params.value as string] || params.value;
      }
    },
    {
      field: 'dealId',
      headerName: 'Deal ID',
      width: 150,
      // In a real app, this would link to the deal details
    },
    {
      field: 'type',
      headerName: 'Type',
      width: 120,
    },
    {
      field: 'amount',
      headerName: 'Amount',
      width: 120,
      valueFormatter: (params: GridValueFormatterParams) => {
        if (params.value == null) return '';
        return formatCurrency(params.value as number);
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 150,
      renderCell: (params: GridRenderCellParams) => (
        <Box
          sx={{
            color: `${getStatusColor(params.value as CommissionStatus)}.main`,
            fontWeight: 'medium',
          }}
        >
          {params.value}
        </Box>
      ),
    },
    {
      field: 'createdAt',
      headerName: 'Date Created',
      width: 120,
      valueFormatter: (params: GridValueFormatterParams) => {
        if (!params.value) return '';
        return new Date(params.value as string).toLocaleDateString();
      },
    },
    {
      field: 'payDate',
      headerName: 'Pay Date',
      width: 120,
      valueFormatter: (params: GridValueFormatterParams) => {
        if (!params.value) return 'Not Paid';
        return new Date(params.value as string).toLocaleDateString();
      },
    },
  ];

  // Prepare chart data
  const prepareSummaryChartData = () => {
    if (!summaries || summaries.length === 0) {
      return {
        labels: [],
        datasets: [
          {
            label: 'Commissions',
            data: [],
            backgroundColor: 'rgba(54, 162, 235, 0.6)',
          },
        ],
      };
    }

    return {
      labels: summaries.map((summary) => summary.salesPersonName),
      datasets: [
        {
          label: 'Total Commissions',
          data: summaries.map((summary) => summary.approved + summary.paid),
          backgroundColor: 'rgba(54, 162, 235, 0.6)',
        },
      ],
    };
  };

  const prepareStatusChartData = () => {
    if (!commissions || commissions.length === 0) {
      return {
        labels: [],
        datasets: [
          {
            data: [],
            backgroundColor: [],
          },
        ],
      };
    }

    const statusCounts: Record<CommissionStatus, number> = {
      [CommissionStatus.Pending]: 0,
      [CommissionStatus.Approved]: 0,
      [CommissionStatus.Paid]: 0,
      [CommissionStatus.Adjusted]: 0,
      [CommissionStatus.Denied]: 0,
    };

    commissions.forEach((commission) => {
      statusCounts[commission.status]++;
    });

    const colors = {
      [CommissionStatus.Pending]: 'rgba(255, 206, 86, 0.8)',
      [CommissionStatus.Approved]: 'rgba(54, 162, 235, 0.8)',
      [CommissionStatus.Paid]: 'rgba(75, 192, 192, 0.8)',
      [CommissionStatus.Adjusted]: 'rgba(153, 102, 255, 0.8)',
      [CommissionStatus.Denied]: 'rgba(255, 99, 132, 0.8)',
    };

    return {
      labels: Object.keys(statusCounts),
      datasets: [
        {
          data: Object.values(statusCounts),
          backgroundColor: Object.keys(statusCounts).map(
            (status) => colors[status as CommissionStatus]
          ),
          borderWidth: 1,
        },
      ],
    };
  };

  // Calculate summary statistics
  const calculateTotalCommissions = () => {
    if (!commissions) return 0;
    return commissions.reduce((total, commission) => total + commission.amount, 0);
  };

  const calculatePaidCommissions = () => {
    if (!commissions) return 0;
    return commissions
      .filter((commission) => commission.status === CommissionStatus.Paid)
      .reduce((total, commission) => total + commission.amount, 0);
  };

  const calculatePendingCommissions = () => {
    if (!commissions) return 0;
    return commissions
      .filter(
        (commission) =>
          commission.status === CommissionStatus.Pending ||
          commission.status === CommissionStatus.Approved
      )
      .reduce((total, commission) => total + commission.amount, 0);
  };

  return (
    <Box p={3}>
      <Grid container spacing={2} alignItems="center" sx={{ mb: 3 }}>
        <Grid item xs>
          <Typography variant="h4" component="h1">
            Commissions
          </Typography>
        </Grid>
      </Grid>

      <Paper sx={{ width: '100%' }}>
        <Tabs
          value={tabValue}
          onChange={handleTabChange}
          aria-label="commission tabs"
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab icon={<MoneyIcon />} iconPosition="start" label="Overview" id="commission-tab-0" />
          <Tab icon={<PeopleIcon />} iconPosition="start" label="Commission List" id="commission-tab-1" />
        </Tabs>

        {/* Overview Tab */}
        <TabPanel value={tabValue} index={0}>
          {/* Date Range Filter */}
          <Paper sx={{ p: 2, mb: 3 }}>
            <Grid container spacing={2} alignItems="center">
              <Grid item>
                <FilterIcon color="action" />
              </Grid>
              <Grid item xs={12} sm={3}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="Start Date"
                    value={startDate}
                    onChange={(newValue) => setStartDate(newValue)}
                    slotProps={{ textField: { size: 'small', fullWidth: true } }}
                  />
                </LocalizationProvider>
              </Grid>
              <Grid item xs={12} sm={3}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="End Date"
                    value={endDate}
                    onChange={(newValue) => setEndDate(newValue)}
                    slotProps={{ textField: { size: 'small', fullWidth: true } }}
                    minDate={startDate || undefined}
                  />
                </LocalizationProvider>
              </Grid>
            </Grid>
          </Paper>

          {isLoadingCommissions || isLoadingSummaries ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <>
              {/* Summary Cards */}
              <Grid container spacing={3} sx={{ mb: 3 }}>
                <Grid item xs={12} sm={4}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Total Commissions
                      </Typography>
                      <Typography variant="h5">{formatCurrency(calculateTotalCommissions())}</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Paid Commissions
                      </Typography>
                      <Typography variant="h5">{formatCurrency(calculatePaidCommissions())}</Typography>
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <Card>
                    <CardContent>
                      <Typography color="textSecondary" gutterBottom>
                        Pending Commissions
                      </Typography>
                      <Typography variant="h5">{formatCurrency(calculatePendingCommissions())}</Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              {/* Charts */}
              <Grid container spacing={3}>
                <Grid item xs={12} md={8}>
                  <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" gutterBottom>
                      Commissions by Sales Person
                    </Typography>
                    <Box sx={{ height: 300 }}>
                      <Bar
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              display: true,
                              position: 'top' as const,
                            },
                            title: {
                              display: false,
                            },
                          },
                        }}
                        data={prepareSummaryChartData()}
                      />
                    </Box>
                  </Paper>
                </Grid>
                <Grid item xs={12} md={4}>
                  <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" gutterBottom>
                      Commission Status
                    </Typography>
                    <Box sx={{ height: 300, display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                      <Doughnut
                        options={{
                          responsive: true,
                          maintainAspectRatio: false,
                          plugins: {
                            legend: {
                              display: true,
                              position: 'bottom' as const,
                            },
                          },
                        }}
                        data={prepareStatusChartData()}
                      />
                    </Box>
                  </Paper>
                </Grid>
              </Grid>
            </>
          )}
        </TabPanel>

        {/* Commissions List Tab */}
        <TabPanel value={tabValue} index={1}>
          <Paper sx={{ p: 2, mb: 3 }}>
            <Grid container spacing={2} alignItems="center">
              <Grid item>
                <FilterIcon color="action" />
              </Grid>
              <Grid item xs={12} sm={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Status</InputLabel>
                  <Select
                    value={statusFilter}
                    label="Status"
                    onChange={handleStatusFilterChange}
                  >
                    <MenuItem value="">All Statuses</MenuItem>
                    {Object.values(CommissionStatus).map((status) => (
                      <MenuItem key={status} value={status}>
                        {status}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Type</InputLabel>
                  <Select
                    value={typeFilter}
                    label="Type"
                    onChange={handleTypeFilterChange}
                  >
                    <MenuItem value="">All Types</MenuItem>
                    {Object.values(CommissionType).map((type) => (
                      <MenuItem key={type} value={type}>
                        {type}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={2}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="Start Date"
                    value={startDate}
                    onChange={(newValue) => setStartDate(newValue)}
                    slotProps={{ textField: { size: 'small', fullWidth: true } }}
                  />
                </LocalizationProvider>
              </Grid>
              <Grid item xs={12} sm={2}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="End Date"
                    value={endDate}
                    onChange={(newValue) => setEndDate(newValue)}
                    slotProps={{ textField: { size: 'small', fullWidth: true } }}
                  />
                </LocalizationProvider>
              </Grid>
            </Grid>
          </Paper>

          <Paper sx={{ height: 600, width: '100%' }}>
            <DataGrid
              rows={commissions || []}
              columns={columns}
              loading={isLoadingCommissions}
              components={{
                LoadingOverlay: LinearProgress,
              }}
              getRowId={(row) => row.id}
              pageSize={10}
              rowsPerPageOptions={[10, 25, 50]}
              disableSelectionOnClick
            />
          </Paper>
        </TabPanel>
      </Paper>
    </Box>
  );
};

export default Commissions;
