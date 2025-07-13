import React from 'react';
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Avatar,
  LinearProgress,
  Paper,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Chip,
  Button,
  Divider,
} from '@mui/material';
import {
  TrendingUp,
  DirectionsCar,
  People,
  AttachMoney,
  Build,
  Notifications,
  Assignment,
  Schedule,
} from '@mui/icons-material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import { useAuth } from '../contexts/AuthContext';

// Sample data
const salesData = [
  { name: 'Jan', sales: 45, revenue: 180000 },
  { name: 'Feb', sales: 52, revenue: 208000 },
  { name: 'Mar', sales: 48, revenue: 192000 },
  { name: 'Apr', sales: 61, revenue: 244000 },
  { name: 'May', sales: 55, revenue: 220000 },
  { name: 'Jun', sales: 67, revenue: 268000 },
];

const inventoryData = [
  { name: 'New Cars', value: 45, color: '#1976d2' },
  { name: 'Used Cars', value: 78, color: '#dc004e' },
  { name: 'Certified Pre-Owned', value: 23, color: '#ff9800' },
];

const recentActivities = [
  { type: 'sale', customer: 'John Smith', vehicle: '2023 Honda Accord', amount: '$28,500', time: '2 hours ago' },
  { type: 'service', customer: 'Sarah Wilson', service: 'Oil Change', amount: '$89', time: '4 hours ago' },
  { type: 'lead', customer: 'Mike Johnson', interest: '2024 Toyota Camry', status: 'hot', time: '6 hours ago' },
  { type: 'parts', item: 'Brake Pads - Honda', quantity: '4 sets', amount: '$320', time: '1 day ago' },
];

const upcomingTasks = [
  { task: 'Follow up with Jane Doe', type: 'sales', priority: 'high', due: 'Today 2:00 PM' },
  { task: 'Vehicle inspection - VIN 1234', type: 'service', priority: 'medium', due: 'Tomorrow 9:00 AM' },
  { task: 'Monthly inventory report', type: 'admin', priority: 'low', due: 'Friday' },
  { task: 'Customer satisfaction survey', type: 'crm', priority: 'medium', due: 'This week' },
];

const StatCard = ({ title, value, subtitle, icon, color = 'primary', trend }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Box>
          <Typography color="text.secondary" gutterBottom variant="overline">
            {title}
          </Typography>
          <Typography variant="h4" component="div" fontWeight="bold">
            {value}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {subtitle}
          </Typography>
          {trend && (
            <Box sx={{ mt: 1, display: 'flex', alignItems: 'center' }}>
              <TrendingUp sx={{ fontSize: 16, color: 'success.main', mr: 0.5 }} />
              <Typography variant="caption" color="success.main">
                {trend}
              </Typography>
            </Box>
          )}
        </Box>
        <Avatar sx={{ bgcolor: `${color}.main`, width: 56, height: 56 }}>
          {icon}
        </Avatar>
      </Box>
    </CardContent>
  </Card>
);

export default function Dashboard() {
  const { user } = useAuth();

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" fontWeight="bold" gutterBottom>
          Welcome back, {user?.firstName}!
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Here's what's happening at your dealership today.
        </Typography>
      </Box>

      {/* Key Metrics */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Monthly Sales"
            value="67"
            subtitle="Vehicles sold this month"
            icon={<DirectionsCar />}
            color="primary"
            trend="+12% from last month"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Revenue"
            value="$268K"
            subtitle="Total revenue this month"
            icon={<AttachMoney />}
            color="success"
            trend="+8% from last month"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Active Customers"
            value="1,247"
            subtitle="Total active customers"
            icon={<People />}
            color="info"
            trend="+23 this week"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Service Orders"
            value="89"
            subtitle="Pending service orders"
            icon={<Build />}
            color="warning"
          />
        </Grid>
      </Grid>

      <Grid container spacing={3}>
        {/* Sales Performance Chart */}
        <Grid item xs={12} lg={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Sales Performance
              </Typography>
              <Box sx={{ height: 300 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={salesData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis yAxisId="left" />
                    <YAxis yAxisId="right" orientation="right" />
                    <Tooltip />
                    <Bar yAxisId="left" dataKey="sales" fill="#1976d2" name="Units Sold" />
                    <Bar yAxisId="right" dataKey="revenue" fill="#dc004e" name="Revenue ($)" />
                  </BarChart>
                </ResponsiveContainer>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Inventory Overview */}
        <Grid item xs={12} lg={4}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Inventory Overview
              </Typography>
              <Box sx={{ height: 200, display: 'flex', justifyContent: 'center' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={inventoryData}
                      cx="50%"
                      cy="50%"
                      innerRadius={40}
                      outerRadius={80}
                      dataKey="value"
                    >
                      {inventoryData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </Box>
              <Box sx={{ mt: 2 }}>
                {inventoryData.map((item, index) => (
                  <Box key={index} sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                    <Box
                      sx={{
                        width: 12,
                        height: 12,
                        borderRadius: '50%',
                        backgroundColor: item.color,
                        mr: 1,
                      }}
                    />
                    <Typography variant="body2" sx={{ flexGrow: 1 }}>
                      {item.name}
                    </Typography>
                    <Typography variant="body2" fontWeight="bold">
                      {item.value}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Recent Activity */}
        <Grid item xs={12} lg={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Recent Activity
              </Typography>
              <List>
                {recentActivities.map((activity, index) => (
                  <React.Fragment key={index}>
                    <ListItem>
                      <ListItemAvatar>
                        <Avatar sx={{ 
                          bgcolor: activity.type === 'sale' ? 'success.main' 
                                 : activity.type === 'service' ? 'warning.main'
                                 : activity.type === 'lead' ? 'info.main'
                                 : 'primary.main'
                        }}>
                          {activity.type === 'sale' && <AttachMoney />}
                          {activity.type === 'service' && <Build />}
                          {activity.type === 'lead' && <People />}
                          {activity.type === 'parts' && <DirectionsCar />}
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="subtitle2">
                              {activity.customer}
                            </Typography>
                            {activity.status && (
                              <Chip 
                                label={activity.status} 
                                size="small" 
                                color={activity.status === 'hot' ? 'error' : 'default'}
                              />
                            )}
                          </Box>
                        }
                        secondary={
                          <Box>
                            <Typography variant="body2">
                              {activity.vehicle || activity.service || activity.interest || activity.item}
                            </Typography>
                            <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 0.5 }}>
                              <Typography variant="caption" color="success.main" fontWeight="bold">
                                {activity.amount}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {activity.time}
                              </Typography>
                            </Box>
                          </Box>
                        }
                      />
                    </ListItem>
                    {index < recentActivities.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
            </CardContent>
          </Card>
        </Grid>

        {/* Upcoming Tasks */}
        <Grid item xs={12} lg={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Upcoming Tasks
              </Typography>
              <List>
                {upcomingTasks.map((task, index) => (
                  <React.Fragment key={index}>
                    <ListItem>
                      <ListItemAvatar>
                        <Avatar sx={{ 
                          bgcolor: task.priority === 'high' ? 'error.main' 
                                 : task.priority === 'medium' ? 'warning.main'
                                 : 'success.main'
                        }}>
                          <Assignment />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="subtitle2">
                              {task.task}
                            </Typography>
                            <Chip 
                              label={task.priority} 
                              size="small" 
                              color={
                                task.priority === 'high' ? 'error' 
                                : task.priority === 'medium' ? 'warning' 
                                : 'success'
                              }
                            />
                          </Box>
                        }
                        secondary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                            <Schedule sx={{ fontSize: 14 }} />
                            <Typography variant="caption">
                              {task.due}
                            </Typography>
                          </Box>
                        }
                      />
                    </ListItem>
                    {index < upcomingTasks.length - 1 && <Divider component="li" />}
                  </React.Fragment>
                ))}
              </List>
              <Box sx={{ mt: 2, textAlign: 'center' }}>
                <Button variant="outlined" size="small">
                  View All Tasks
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
