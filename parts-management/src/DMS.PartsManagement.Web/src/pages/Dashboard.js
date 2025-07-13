import React, { useState, useEffect } from 'react';
import {
  Grid,
  Paper,
  Typography,
  Box,
  Card,
  CardContent,
  CardHeader,
  List,
  ListItem,
  ListItemText,
  Button,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  LinearProgress,
  Alert,
  Stack
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import {
  Inventory as InventoryIcon,
  ShoppingCart as OrderIcon,
  Warning as AlertIcon,
  ArrowForward as ArrowForwardIcon,
  Refresh as RefreshIcon,
  BarChart as BarChartIcon,
  LocalShipping as ShippingIcon,
  Assessment as AssessmentIcon,
  Build as BuildIcon
} from '@mui/icons-material';
import { PartsService, InventoryService, OrdersService, TransactionsService } from '../services/api';

const Dashboard = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [dashboardData, setDashboardData] = useState({
    inventorySummary: {},
    lowStockItems: [],
    recentOrders: [],
    pendingDeliveries: [],
    notifications: []
  });
  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      
      // In a real application, you would have a dedicated endpoint for dashboard data
      // For now, we'll simulate fetching data from multiple endpoints
      
      // Mock data for demonstration
      const inventorySummary = {
        totalParts: 1254,
        totalValue: 287562.50,
        pendingOrders: 12,
        lowStockCount: 18
      };
      
      const lowStockItems = [
        { partId: 101, locationId: 1, partNumber: 'AX-1234', partName: 'Brake Pad', quantity: 3, minStockLevel: 10 },
        { partId: 205, locationId: 2, partNumber: 'FT-5678', partName: 'Oil Filter', quantity: 2, minStockLevel: 15 },
        { partId: 310, locationId: 1, partNumber: 'CN-9101', partName: 'Spark Plug', quantity: 4, minStockLevel: 20 },
        { partId: 422, locationId: 3, partNumber: 'BT-1122', partName: 'Battery Terminal', quantity: 0, minStockLevel: 5 }
      ];
      
      const recentOrders = [
        { id: 'ord-001', orderNumber: 'PO-2025-001', orderDate: '2025-06-20', supplierName: 'AutoParts Inc', status: 'Processing' },
        { id: 'ord-002', orderNumber: 'PO-2025-002', orderDate: '2025-06-18', supplierName: 'Global Motors Supply', status: 'Submitted' },
        { id: 'ord-003', orderNumber: 'PO-2025-003', orderDate: '2025-06-15', supplierName: 'EliteParts Co', status: 'PartiallyReceived' }
      ];
      
      const pendingDeliveries = [
        { orderId: 'ord-001', orderNumber: 'PO-2025-001', supplierName: 'AutoParts Inc', expectedDate: '2025-06-25' },
        { orderId: 'ord-004', orderNumber: 'PO-2025-004', supplierName: 'Premium Auto Supplies', expectedDate: '2025-06-27' }
      ];
      
      const notifications = [
        { id: 1, title: 'Low Stock Alert', message: '5 items are below critical stock levels', timestamp: '2025-06-22T08:30:00', type: 'alert' },
        { id: 2, title: 'Order Received', message: 'PO-2025-005 has been fully received', timestamp: '2025-06-21T14:45:00', type: 'success' },
        { id: 3, title: 'Price Update', message: 'Supplier price changes detected for 12 items', timestamp: '2025-06-20T11:15:00', type: 'info' }
      ];
      
      setDashboardData({
        inventorySummary,
        lowStockItems,
        recentOrders,
        pendingDeliveries,
        notifications
      });
      
      setError(null);
    } catch (err) {
      console.error('Error fetching dashboard data:', err);
      setError('Failed to load dashboard data. Please refresh the page.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusChip = (status) => {
    let color;
    switch(status) {
      case 'Draft':
        color = 'default';
        break;
      case 'Submitted':
        color = 'primary';
        break;
      case 'Processing':
        color = 'info';
        break;
      case 'PartiallyReceived':
        color = 'warning';
        break;
      case 'Received':
        color = 'success';
        break;
      case 'Cancelled':
        color = 'error';
        break;
      default:
        color = 'default';
    }
    return <Chip size="small" label={status} color={color} />;
  };
  if (loading) {
    return <LinearProgress />;
  }

  return (
    <Box sx={{ flexGrow: 1, p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Parts Management Dashboard
        </Typography>
        <Button 
          variant="outlined" 
          startIcon={<RefreshIcon />}
          onClick={fetchDashboardData}
        >
          Refresh
        </Button>
      </Box>
      
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}
      
      {/* Stats Overview */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Box sx={{ 
                bgcolor: 'primary.main', 
                color: 'white', 
                p: 1.5, 
                borderRadius: 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mr: 2
              }}>
                <InventoryIcon />
              </Box>
              <Box>
                <Typography variant="body2" color="text.secondary">
                  Total Parts
                </Typography>
                <Typography variant="h5" component="div">
                  {dashboardData.inventorySummary.totalParts?.toLocaleString()}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Across all categories
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Box sx={{ 
                bgcolor: 'success.main', 
                color: 'white', 
                p: 1.5, 
                borderRadius: 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mr: 2
              }}>
                <BarChartIcon />
              </Box>
              <Box>
                <Typography variant="body2" color="text.secondary">
                  Total Value
                </Typography>
                <Typography variant="h5" component="div">
                  ${dashboardData.inventorySummary.totalValue?.toLocaleString()}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Based on current stock
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Box sx={{ 
                bgcolor: 'info.main', 
                color: 'white', 
                p: 1.5, 
                borderRadius: 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mr: 2
              }}>
                <OrderIcon />
              </Box>
              <Box>
                <Typography variant="body2" color="text.secondary">
                  Pending Orders
                </Typography>
                <Typography variant="h5" component="div">
                  {dashboardData.inventorySummary.pendingOrders}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Awaiting delivery
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
        
        <Grid item xs={12} sm={6} md={3}>
          <Paper elevation={2} sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Box sx={{ 
                bgcolor: 'warning.main', 
                color: 'white', 
                p: 1.5, 
                borderRadius: 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mr: 2
              }}>
                <AlertIcon />
              </Box>
              <Box>
                <Typography variant="body2" color="text.secondary">
                  Low Stock Items
                </Typography>
                <Typography variant="h5" component="div">
                  {dashboardData.inventorySummary.lowStockCount}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Below minimum levels
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Grid>
      </Grid>
      
      {/* Main Content */}
      <Grid container spacing={3} sx={{ mt: 3 }}>
        {/* Low Stock Items */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader 
              title="Low Stock Items" 
              titleTypographyProps={{ variant: 'h6' }}
              action={
                <Button
                  size="small"
                  endIcon={<ArrowForwardIcon />}
                  onClick={() => navigate('/inventory?filter=lowstock')}
                >
                  View All
                </Button>
              }
            />
            <Divider />
            <CardContent sx={{ p: 0 }}>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Part #</TableCell>
                      <TableCell>Name</TableCell>
                      <TableCell align="center">Stock</TableCell>
                      <TableCell align="center">Min</TableCell>
                      <TableCell align="right">Action</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {dashboardData.lowStockItems.map((item) => (
                      <TableRow key={`${item.partId}-${item.locationId}`}>
                        <TableCell>{item.partNumber}</TableCell>
                        <TableCell>{item.partName}</TableCell>
                        <TableCell align="center">
                          <Chip 
                            size="small" 
                            color={item.quantity <= 0 ? 'error' : 'warning'} 
                            label={item.quantity} 
                          />
                        </TableCell>
                        <TableCell align="center">{item.minStockLevel}</TableCell>
                        <TableCell align="right">
                          <Button 
                            size="small" 
                            variant="outlined"
                            onClick={() => navigate(`/orders/new?partId=${item.partId}`)}
                          >
                            Order
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
        
        {/* Recent Orders */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader 
              title="Recent Orders" 
              titleTypographyProps={{ variant: 'h6' }}
              action={
                <Button
                  size="small"
                  endIcon={<ArrowForwardIcon />}
                  onClick={() => navigate('/orders')}
                >
                  View All
                </Button>
              }
            />
            <Divider />
            <CardContent sx={{ p: 0 }}>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Order #</TableCell>
                      <TableCell>Date</TableCell>
                      <TableCell>Supplier</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell align="right">Action</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {dashboardData.recentOrders.map((order) => (
                      <TableRow key={order.id}>
                        <TableCell>{order.orderNumber}</TableCell>
                        <TableCell>{new Date(order.orderDate).toLocaleDateString()}</TableCell>
                        <TableCell>{order.supplierName}</TableCell>
                        <TableCell>{getStatusChip(order.status)}</TableCell>
                        <TableCell align="right">
                          <Button 
                            size="small" 
                            variant="outlined"
                            onClick={() => navigate(`/orders/${order.id}`)}
                          >
                            View
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </CardContent>
          </Card>
        </Grid>
        
        {/* Expected Deliveries */}
        <Grid item xs={12} md={6} sx={{ mt: 3 }}>
          <Card>
            <CardHeader 
              title="Expected Deliveries" 
              titleTypographyProps={{ variant: 'h6' }}
              action={
                <Button
                  size="small"
                  endIcon={<ArrowForwardIcon />}
                  onClick={() => navigate('/orders?status=processing')}
                >
                  View All
                </Button>
              }
            />
            <List sx={{ width: '100%' }}>
              {dashboardData.pendingDeliveries.map((delivery, index) => (
                <React.Fragment key={delivery.orderId}>
                  <ListItem
                    secondaryAction={
                      <Button 
                        size="small" 
                        variant="outlined"
                        onClick={() => navigate(`/orders/${delivery.orderId}`)}
                      >
                        View
                      </Button>
                    }
                  >
                    <ListItemText
                      primary={`Order #${delivery.orderNumber}`}
                      secondary={
                        <>
                          <Typography component="span" variant="body2" color="text.primary">
                            {delivery.supplierName}
                          </Typography>
                          {' — Expected: ' + new Date(delivery.expectedDate).toLocaleDateString()}
                        </>
                      }
                    />
                  </ListItem>
                  {index < dashboardData.pendingDeliveries.length - 1 && <Divider />}
                </React.Fragment>
              ))}
              {dashboardData.pendingDeliveries.length === 0 && (
                <ListItem>
                  <ListItemText primary="No pending deliveries" />
                </ListItem>
              )}
            </List>
          </Card>
        </Grid>
        
        {/* Quick Actions */}
        <Grid item xs={12} md={6} sx={{ mt: 3 }}>
          <Card>
            <CardHeader title="Quick Actions" titleTypographyProps={{ variant: 'h6' }} />
            <CardContent>
              <Stack spacing={2}>
                <Button
                  variant="contained"
                  color="primary"
                  startIcon={<OrderIcon />}
                  onClick={() => navigate('/orders/new')}
                  fullWidth
                >
                  Create New Order
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<InventoryIcon />}
                  onClick={() => navigate('/parts')}
                  fullWidth
                >
                  Manage Parts
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<ShippingIcon />}
                  onClick={() => navigate('/inventory')}
                  fullWidth
                >
                  View Inventory
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<BuildIcon />}
                  onClick={() => navigate('/inventory/adjust')}
                  fullWidth
                >
                  Adjust Inventory
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<AssessmentIcon />}
                  onClick={() => navigate('/reports')}
                  fullWidth
                >                  Generate Reports
                </Button>
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      
      {/* Performance Metrics */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', height: 120 }}>
            <Typography component="p" variant="h6">
              Backordered Parts
            </Typography>
            <Typography component="p" variant="h4" sx={{ color: 'error.main' }}>
              {dashboardData.inventorySummary.backOrderedParts || 0}
            </Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', height: 120 }}>
            <Typography component="p" variant="h6">
              Orders to Receive
            </Typography>
            <Typography component="p" variant="h4">
              {dashboardData.inventorySummary.pendingOrders || 0}
            </Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', height: 120 }}>
            <Typography component="p" variant="h6">
              Special Orders
            </Typography>
            <Typography component="p" variant="h4">
              {dashboardData.inventorySummary.specialOrders || 0}
            </Typography>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Paper sx={{ p: 2, display: 'flex', flexDirection: 'column', height: 120 }}>
            <Typography component="p" variant="h6">
              Low Stock Items
            </Typography>
            <Typography component="p" variant="h4" sx={{ color: 'warning.main' }}>
              {dashboardData.inventorySummary.lowStockCount || 0}
            </Typography>
          </Paper>
        </Grid>
      </Grid>

      {/* Main Panels */}
      <Grid container spacing={3}>
        {/* Low Stock Items */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader 
              title="Low Stock Items" 
              action={
                <Button 
                  size="small" 
                  color="primary" 
                  onClick={() => navigate('/inventory?filter=low-stock')}
                >
                  View All
                </Button>
              }
            />
            <Divider />
            <CardContent>
              {lowStockItems.length > 0 ? (
                <List>
                  {lowStockItems.map((item) => (
                    <ListItem 
                      key={item.id} 
                      divider 
                      button 
                      onClick={() => navigate(`/parts/${item.partId}`)}
                    >
                      <ListItemText 
                        primary={item.partNumber + ' - ' + item.description} 
                        secondary={`Quantity: ${item.quantity} | Min: ${item.minQuantity}`} 
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="textSecondary" align="center" sx={{ py: 3 }}>
                  No low stock items found
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Pending Orders */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardHeader 
              title="Pending Orders" 
              action={
                <Button 
                  size="small" 
                  color="primary" 
                  onClick={() => navigate('/orders?status=Pending')}
                >
                  View All
                </Button>
              }
            />
            <Divider />
            <CardContent>              {dashboardData.pendingDeliveries.length > 0 ? (
                <List>
                  {dashboardData.pendingDeliveries.map((order) => (
                    <ListItem 
                      key={order.id} 
                      divider 
                      button 
                      onClick={() => navigate(`/orders/${order.id}`)}
                    >
                      <ListItemText 
                        primary={`Order #${order.orderNumber}`} 
                        secondary={`Supplier: ${order.supplierName} | Date: ${new Date(order.orderDate).toLocaleDateString()}`} 
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="textSecondary" align="center" sx={{ py: 3 }}>
                  No pending orders found
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Reorder Recommendations */}
        <Grid item xs={12}>
          <Card>
            <CardHeader 
              title="Reorder Recommendations" 
              action={
                <Button 
                  variant="contained" 
                  color="primary" 
                  onClick={() => navigate('/orders/create', { state: { recommendations: true } })}
                >
                  Create Order
                </Button>
              }
            />
            <Divider />
            <CardContent>              {dashboardData.lowStockItems.length > 0 ? (
                <List>
                  {dashboardData.lowStockItems.map((rec) => (
                    <ListItem 
                      key={rec.partId} 
                      divider 
                      button 
                      onClick={() => navigate(`/parts/${rec.partId}`)}
                    >
                      <ListItemText                        primary={`${rec.partNumber} - ${rec.partName}`} 
                        secondary={`Current: ${rec.quantity} | Min Level: ${rec.minStockLevel}`} 
                      />
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="textSecondary" align="center" sx={{ py: 3 }}>
                  No reorder recommendations found
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
