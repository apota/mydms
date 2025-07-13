// @ts-nocheck
import React from 'react';
import {
  Box,
  Grid,
  Paper,
  Typography,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Avatar,
  Button,
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  Person as PersonIcon,
  DirectionsCar as CarIcon,
  AttachMoney as MoneyIcon,
} from '@mui/icons-material';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const Dashboard = () => {
  // Mock data for charts
  const salesData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    datasets: [
      {
        label: 'Sales ($)',
        data: [12000, 19000, 15000, 28000, 22000, 35000],
        fill: true,
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        borderColor: 'rgba(75, 192, 192, 1)',
        tension: 0.4,
      },
    ],
  };

  const leadsData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    datasets: [
      {
        label: 'New Leads',
        data: [45, 62, 58, 80, 95, 112],
        fill: true,
        backgroundColor: 'rgba(54, 162, 235, 0.2)',
        borderColor: 'rgba(54, 162, 235, 1)',
        tension: 0.4,
      },
    ],
  };

  // Mock data for lists
  const upcomingDeals = [
    { id: 1, customer: 'John Smith', vehicle: '2023 Ford Mustang', amount: '$45,000', date: 'Today' },
    { id: 2, customer: 'Mary Johnson', vehicle: '2022 Toyota Camry', amount: '$28,500', date: 'Tomorrow' },
    { id: 3, customer: 'Robert Brown', vehicle: '2023 Honda Accord', amount: '$32,000', date: 'June 23' },
  ];

  const leadFollowUps = [
    { id: 1, customer: 'Emma Wilson', status: 'Contacted', appointmentDate: 'Today, 2:00 PM' },
    { id: 2, customer: 'James Miller', status: 'New', appointmentDate: 'Tomorrow, 10:30 AM' },
    { id: 3, customer: 'Sara Davis', status: 'Qualified', appointmentDate: 'June 23, 3:15 PM' },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Sales Dashboard
      </Typography>

      {/* Summary Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 3,
              display: 'flex',
              flexDirection: 'column',
              bgcolor: 'primary.light',
              color: 'white',
            }}
          >
            <Box display="flex" justifyContent="space-between">
              <Box>
                <Typography variant="h6">Total Sales</Typography>
                <Typography variant="h4">$157,000</Typography>
              </Box>
              <TrendingUpIcon fontSize="large" />
            </Box>
            <Typography variant="body2" sx={{ mt: 1 }}>
              This Month
            </Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 3,
              display: 'flex',
              flexDirection: 'column',
              bgcolor: 'success.light',
              color: 'white',
            }}
          >
            <Box display="flex" justifyContent="space-between">
              <Box>
                <Typography variant="h6">Leads</Typography>
                <Typography variant="h4">47</Typography>
              </Box>
              <PersonIcon fontSize="large" />
            </Box>
            <Typography variant="body2" sx={{ mt: 1 }}>
              Active Leads
            </Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 3,
              display: 'flex',
              flexDirection: 'column',
              bgcolor: 'warning.light',
              color: 'white',
            }}
          >
            <Box display="flex" justifyContent="space-between">
              <Box>
                <Typography variant="h6">Vehicles</Typography>
                <Typography variant="h4">15</Typography>
              </Box>
              <CarIcon fontSize="large" />
            </Box>
            <Typography variant="body2" sx={{ mt: 1 }}>
              Sold This Month
            </Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper
            sx={{
              p: 3,
              display: 'flex',
              flexDirection: 'column',
              bgcolor: 'secondary.light',
              color: 'white',
            }}
          >
            <Box display="flex" justifyContent="space-between">
              <Box>
                <Typography variant="h6">Commission</Typography>
                <Typography variant="h4">$12,450</Typography>
              </Box>
              <MoneyIcon fontSize="large" />
            </Box>
            <Typography variant="body2" sx={{ mt: 1 }}>
              Pending Payout
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      {/* Charts */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Monthly Sales
            </Typography>
            <Line data={salesData} options={{ responsive: true }} />
          </Paper>
        </Grid>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Lead Acquisition
            </Typography>
            <Line data={leadsData} options={{ responsive: true }} />
          </Paper>
        </Grid>
      </Grid>

      {/* Lists */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h6">Upcoming Deals</Typography>
              <Button variant="contained" color="primary" size="small">
                View All
              </Button>
            </Box>
            <List>
              {upcomingDeals.map((deal) => (
                <ListItem key={deal.id} divider>
                  <ListItemAvatar>
                    <Avatar>
                      <CarIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={deal.customer}
                    secondary={
                      <React.Fragment>
                        <Typography component="span" variant="body2" color="text.primary">
                          {deal.vehicle}
                        </Typography>
                        {` — ${deal.amount} • ${deal.date}`}
                      </React.Fragment>
                    }
                  />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>
        <Grid item xs={12} md={6}>
          <Paper sx={{ p: 2 }}>
            <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
              <Typography variant="h6">Lead Follow Ups</Typography>
              <Button variant="contained" color="primary" size="small">
                View All
              </Button>
            </Box>
            <List>
              {leadFollowUps.map((lead) => (
                <ListItem key={lead.id} divider>
                  <ListItemAvatar>
                    <Avatar>
                      <PersonIcon />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText
                    primary={lead.customer}
                    secondary={
                      <React.Fragment>
                        <Typography component="span" variant="body2" color="text.primary">
                          {lead.status}
                        </Typography>
                        {` — ${lead.appointmentDate}`}
                      </React.Fragment>
                    }
                  />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
