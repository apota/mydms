import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Paper,
  Grid,
  Button,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Tabs,
  Tab,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  IconButton,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Avatar,
  Divider,
  Stack,
  Tooltip,
  LinearProgress,
  Badge
} from '@mui/material';
import {
  Stars,
  CardGiftcard,
  Redeem,
  AddCircle,
  Assignment,
  TrendingUp,
  People,
  EmojiEvents,
  Refresh,
  Edit,
  Delete,
  Search
} from '@mui/icons-material';
import { format } from 'date-fns';
import { LoyaltyService } from '../../services/api-services';

function LoyaltyDashboard() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState(0);
  const [dashboardData, setDashboardData] = useState({
    totalActiveMembers: 0,
    totalPointsIssued: 0,
    totalPointsRedeemed: 0,
    activeRewardsCount: 0,
    pendingRedemptionsCount: 0
  });
  const [rewards, setRewards] = useState([]);
  const [recentActivity, setRecentActivity] = useState([]);
  const [customers, setCustomers] = useState([]);
  
  // Dialog states
  const [pointsDialogOpen, setPointsDialogOpen] = useState(false);
  const [rewardDialogOpen, setRewardDialogOpen] = useState(false);
  const [tierDialogOpen, setTierDialogOpen] = useState(false);
  
  // Form states
  const [pointsForm, setPointsForm] = useState({
    customerId: '',
    points: 0,
    source: '',
    referenceId: ''
  });
  const [redeemForm, setRedeemForm] = useState({
    customerId: '',
    rewardId: ''
  });
  const [tierForm, setTierForm] = useState({
    customerId: '',
    newTier: '',
    reason: ''
  });

  // Pagination states
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  // Sample customers for demonstration (fallback data)
  const demoCustomers = [
    {
      id: '1',
      name: 'John Smith',
      email: 'john.smith@example.com',
      tier: 'Gold',
      currentPoints: 7500,
      totalPointsEarned: 10000,
      enrollmentDate: '2023-06-15',
      lastActivity: '2025-06-20'
    },
    {
      id: '2',
      name: 'Sarah Johnson',
      email: 'sarah.j@example.com',
      tier: 'Silver',
      currentPoints: 2200,
      totalPointsEarned: 3000,
      enrollmentDate: '2024-03-10',
      lastActivity: '2025-06-18'
    },
    {
      id: '3',
      name: 'Michael Brown',
      email: 'mbrown@example.com',
      tier: 'Bronze',
      currentPoints: 450,
      totalPointsEarned: 800,
      enrollmentDate: '2025-02-05',
      lastActivity: '2025-06-15'
    },
    {
      id: '4',
      name: 'Emily Davis',
      email: 'emily.d@example.com',
      tier: 'Platinum',
      currentPoints: 15300,
      totalPointsEarned: 18000,
      enrollmentDate: '2022-10-20',
      lastActivity: '2025-06-22'
    }
  ];

  // Load initial data
  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Try to load real data from API
      const [dashboardResponse, rewardsResponse] = await Promise.all([
        LoyaltyService.getDashboardData(),
        LoyaltyService.getAvailableRewards()
      ]);
      
      setDashboardData(dashboardResponse);
      setRewards(rewardsResponse);
      setCustomers(demoCustomers); // Use demo customers until we have a customer API endpoint
      
    } catch (err) {
      console.error('Error loading loyalty data:', err);
      setError('Using demo data - API connection failed');
      
      // Use fallback demo data
      setDashboardData({
        totalActiveMembers: demoCustomers.length,
        totalPointsIssued: 35800,
        totalPointsRedeemed: 6350,
        activeRewardsCount: 8,
        pendingRedemptionsCount: 2
      });
      
      setRewards([
        {
          id: 'r1',
          name: 'Free Oil Change',
          description: 'Complimentary standard oil change service',
          pointsCost: 500,
          category: 'Service',
          isActive: true
        },
        {
          id: 'r2',
          name: 'Premium Car Wash',
          description: 'Full-service car wash with interior cleaning',
          pointsCost: 300,
          category: 'Service',
          isActive: true
        },
        {
          id: 'r3',
          name: '$100 Service Credit',
          description: '$100 credit towards any service or repair',
          pointsCost: 2000,
          category: 'Service',
          isActive: true
        }
      ]);
      
      setRecentActivity([
        {
          id: 'a1',
          customerName: 'John Smith',
          type: 'Points Earned',
          description: 'Vehicle service: Major maintenance',
          date: '2025-06-20',
          points: 250
        },
        {
          id: 'a2',
          customerName: 'Emily Davis',
          type: 'Reward Redemption',
          description: 'Redeemed: VIP Lounge Access (7500 points)',
          date: '2025-06-19',
          points: -7500
        }
      ]);
      
      setCustomers(demoCustomers);
    } finally {
      setLoading(false);
    }
  };

  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };

  const handleRefreshData = () => {
    loadDashboardData();
  };

  // Points Dialog Handlers
  const handleAddPointsOpen = (customer = null) => {
    setPointsForm({
      customerId: customer?.id || '',
      points: 0,
      source: '',
      referenceId: ''
    });
    setPointsDialogOpen(true);
  };

  const handleAddPointsClose = () => {
    setPointsDialogOpen(false);
  };

  const handlePointsSubmit = async () => {
    try {
      await LoyaltyService.addLoyaltyPoints(pointsForm.customerId, {
        points: pointsForm.points,
        source: pointsForm.source,
        referenceId: pointsForm.referenceId
      });
      
      handleAddPointsClose();
      loadDashboardData(); // Refresh data
    } catch (error) {
      console.error('Error adding points:', error);
      setError('Failed to add points');
    }
  };

  // Reward Dialog Handlers
  const handleRewardRedemptionOpen = (customer = null) => {
    setRedeemForm({
      customerId: customer?.id || '',
      rewardId: ''
    });
    setRewardDialogOpen(true);
  };

  const handleRewardRedemptionClose = () => {
    setRewardDialogOpen(false);
  };

  const handleRewardRedemptionSubmit = async () => {
    try {
      await LoyaltyService.redeemReward(redeemForm.customerId, redeemForm.rewardId);
      
      handleRewardRedemptionClose();
      loadDashboardData(); // Refresh data
    } catch (error) {
      console.error('Error redeeming reward:', error);
      setError('Failed to redeem reward');
    }
  };

  // Tier Dialog Handlers
  const handleTierUpdateOpen = (customer = null) => {
    setTierForm({
      customerId: customer?.id || '',
      newTier: customer?.tier || '',
      reason: ''
    });
    setTierDialogOpen(true);
  };

  const handleTierUpdateClose = () => {
    setTierDialogOpen(false);
  };

  const handleTierUpdateSubmit = async () => {
    try {
      await LoyaltyService.updateCustomerTier(tierForm.customerId, {
        newTier: tierForm.newTier,
        reason: tierForm.reason
      });
      
      handleTierUpdateClose();
      loadDashboardData(); // Refresh data
    } catch (error) {
      console.error('Error updating tier:', error);
      setError('Failed to update tier');
    }
  };

  // Pagination handlers
  const handleChangePage = (event, newPage) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const getTierColor = (tier) => {
    switch (tier?.toLowerCase()) {
      case 'platinum': return '#E5E7EB';
      case 'gold': return '#FCD34D';
      case 'silver': return '#D1D5DB';
      case 'bronze': return '#A3A3A3';
      default: return '#6B7280';
    }
  };

  const renderOverviewTab = () => (
    <Grid container spacing={3}>
      {/* Key Metrics Cards */}
      <Grid item xs={12} md={3}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center">
              <People color="primary" sx={{ mr: 2 }} />
              <Box>
                <Typography variant="h4" color="primary">
                  {dashboardData.totalActiveMembers}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Active Members
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Grid>
      
      <Grid item xs={12} md={3}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center">
              <Stars color="secondary" sx={{ mr: 2 }} />
              <Box>
                <Typography variant="h4" color="secondary">
                  {dashboardData.totalPointsIssued?.toLocaleString() || 0}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Points Issued
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Grid>
      
      <Grid item xs={12} md={3}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center">
              <Redeem color="success" sx={{ mr: 2 }} />
              <Box>
                <Typography variant="h4" color="success.main">
                  {dashboardData.totalPointsRedeemed?.toLocaleString() || 0}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Points Redeemed
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Grid>
      
      <Grid item xs={12} md={3}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center">
              <CardGiftcard color="warning" sx={{ mr: 2 }} />
              <Box>
                <Typography variant="h4" color="warning.main">
                  {dashboardData.activeRewardsCount}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Active Rewards
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Grid>

      {/* Recent Activity */}
      <Grid item xs={12} md={8}>
        <Paper sx={{ p: 3 }}>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">Recent Activity</Typography>
            <Button
              startIcon={<Refresh />}
              onClick={handleRefreshData}
              size="small"
            >
              Refresh
            </Button>
          </Box>
          <List>
            {recentActivity.map((activity) => (
              <ListItem key={activity.id} divider>
                <ListItemAvatar>
                  <Avatar>
                    {activity.type === 'Points Earned' ? <Stars /> : <Redeem />}
                  </Avatar>
                </ListItemAvatar>
                <ListItemText
                  primary={`${activity.customerName} - ${activity.type}`}
                  secondary={
                    <Box>
                      <Typography variant="body2">{activity.description}</Typography>
                      <Typography variant="caption" color="textSecondary">
                        {format(new Date(activity.date), 'MMM dd, yyyy')}
                      </Typography>
                    </Box>
                  }
                />
                <Chip
                  label={`${activity.points > 0 ? '+' : ''}${activity.points} pts`}
                  color={activity.points > 0 ? 'success' : 'warning'}
                  size="small"
                />
              </ListItem>
            ))}
          </List>
        </Paper>
      </Grid>

      {/* Quick Actions */}
      <Grid item xs={12} md={4}>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>Quick Actions</Typography>
          <Stack spacing={2}>
            <Button
              variant="outlined"
              startIcon={<AddCircle />}
              onClick={() => handleAddPointsOpen()}
              fullWidth
            >
              Add Points
            </Button>
            <Button
              variant="outlined"
              startIcon={<CardGiftcard />}
              onClick={() => handleRewardRedemptionOpen()}
              fullWidth
            >
              Redeem Reward
            </Button>
            <Button
              variant="outlined"
              startIcon={<EmojiEvents />}
              onClick={() => handleTierUpdateOpen()}
              fullWidth
            >
              Update Tier
            </Button>
          </Stack>
        </Paper>
      </Grid>
    </Grid>
  );

  const renderCustomersTab = () => (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      <Box p={2} display="flex" justifyContent="space-between" alignItems="center">
        <Typography variant="h6">Loyalty Program Members</Typography>
        <Button
          variant="contained"
          startIcon={<AddCircle />}
          onClick={() => handleAddPointsOpen()}
        >
          Add Points
        </Button>
      </Box>
      <TableContainer>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>Customer</TableCell>
              <TableCell>Tier</TableCell>
              <TableCell align="right">Current Points</TableCell>
              <TableCell align="right">Total Earned</TableCell>
              <TableCell>Enrollment Date</TableCell>
              <TableCell>Last Activity</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {customers
              .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
              .map((customer) => (
                <TableRow key={customer.id} hover>
                  <TableCell>
                    <Box display="flex" alignItems="center">
                      <Avatar sx={{ mr: 2, bgcolor: getTierColor(customer.tier) }}>
                        {customer.name.charAt(0)}
                      </Avatar>
                      <Box>
                        <Typography variant="body2" fontWeight="medium">
                          {customer.name}
                        </Typography>
                        <Typography variant="caption" color="textSecondary">
                          {customer.email}
                        </Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={customer.tier}
                      color="primary"
                      variant="outlined"
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <Typography variant="body2" fontWeight="medium">
                      {customer.currentPoints?.toLocaleString()}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Typography variant="body2">
                      {customer.totalPointsEarned?.toLocaleString()}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {format(new Date(customer.enrollmentDate), 'MMM dd, yyyy')}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Typography variant="body2">
                      {format(new Date(customer.lastActivity), 'MMM dd, yyyy')}
                    </Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Stack direction="row" spacing={1} justifyContent="center">
                      <Tooltip title="Add Points">
                        <IconButton
                          size="small"
                          onClick={() => handleAddPointsOpen(customer)}
                        >
                          <AddCircle />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Redeem Reward">
                        <IconButton
                          size="small"
                          onClick={() => handleRewardRedemptionOpen(customer)}
                        >
                          <Redeem />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Update Tier">
                        <IconButton
                          size="small"
                          onClick={() => handleTierUpdateOpen(customer)}
                        >
                          <EmojiEvents />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        rowsPerPageOptions={[5, 10, 25]}
        component="div"
        count={customers.length}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
      />
    </Paper>
  );

  const renderRewardsTab = () => (
    <Grid container spacing={3}>
      <Grid item xs={12}>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
          <Typography variant="h6">Available Rewards</Typography>
          <Button variant="contained" startIcon={<AddCircle />}>
            Add New Reward
          </Button>
        </Box>
      </Grid>
      {rewards.map((reward) => (
        <Grid item xs={12} md={6} lg={4} key={reward.id}>
          <Card>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="start" mb={2}>
                <Typography variant="h6" component="div">
                  {reward.name}
                </Typography>
                <Chip
                  label={reward.isActive ? 'Active' : 'Inactive'}
                  color={reward.isActive ? 'success' : 'default'}
                  size="small"
                />
              </Box>
              <Typography variant="body2" color="text.secondary" paragraph>
                {reward.description}
              </Typography>
              <Box display="flex" justifyContent="space-between" alignItems="center">
                <Chip
                  icon={<Stars />}
                  label={`${reward.pointsCost} points`}
                  color="primary"
                />
                <Chip
                  label={reward.category}
                  variant="outlined"
                  size="small"
                />
              </Box>
              <Box mt={2} display="flex" gap={1}>
                <Button size="small" startIcon={<Edit />}>
                  Edit
                </Button>
                <Button size="small" startIcon={<Delete />} color="error">
                  Delete
                </Button>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          Loyalty Program Management
        </Typography>
        <Button
          variant="outlined"
          startIcon={<Refresh />}
          onClick={handleRefreshData}
        >
          Refresh
        </Button>
      </Box>

      {error && (
        <Alert severity="warning" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ width: '100%', mb: 3 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          aria-label="loyalty dashboard tabs"
        >
          <Tab
            icon={<TrendingUp />}
            label="Overview"
            {...{ id: 'tab-0', 'aria-controls': 'tabpanel-0' }}
          />
          <Tab
            icon={<People />}
            label="Customers"
            {...{ id: 'tab-1', 'aria-controls': 'tabpanel-1' }}
          />
          <Tab
            icon={<CardGiftcard />}
            label="Rewards"
            {...{ id: 'tab-2', 'aria-controls': 'tabpanel-2' }}
          />
        </Tabs>
      </Paper>

      <Box role="tabpanel" hidden={activeTab !== 0} id="tabpanel-0">
        {activeTab === 0 && renderOverviewTab()}
      </Box>
      <Box role="tabpanel" hidden={activeTab !== 1} id="tabpanel-1">
        {activeTab === 1 && renderCustomersTab()}
      </Box>
      <Box role="tabpanel" hidden={activeTab !== 2} id="tabpanel-2">
        {activeTab === 2 && renderRewardsTab()}
      </Box>

      {/* Add Points Dialog */}
      <Dialog open={pointsDialogOpen} onClose={handleAddPointsClose} maxWidth="sm" fullWidth>
        <DialogTitle>Add Loyalty Points</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Customer</InputLabel>
                <Select
                  value={pointsForm.customerId}
                  label="Customer"
                  onChange={(e) => setPointsForm(prev => ({ ...prev, customerId: e.target.value }))}
                >
                  {customers.map((customer) => (
                    <MenuItem key={customer.id} value={customer.id}>
                      {customer.name} ({customer.email})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Points"
                type="number"
                value={pointsForm.points}
                onChange={(e) => setPointsForm(prev => ({ ...prev, points: parseInt(e.target.value) || 0 }))}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Source"
                value={pointsForm.source}
                onChange={(e) => setPointsForm(prev => ({ ...prev, source: e.target.value }))}
                placeholder="e.g., Purchase, Service, Promotion"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Reference ID"
                value={pointsForm.referenceId}
                onChange={(e) => setPointsForm(prev => ({ ...prev, referenceId: e.target.value }))}
                placeholder="Order ID, Service ID, etc."
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleAddPointsClose}>Cancel</Button>
          <Button onClick={handlePointsSubmit} variant="contained">
            Add Points
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reward Redemption Dialog */}
      <Dialog open={rewardDialogOpen} onClose={handleRewardRedemptionClose} maxWidth="sm" fullWidth>
        <DialogTitle>Redeem Reward</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Customer</InputLabel>
                <Select
                  value={redeemForm.customerId}
                  label="Customer"
                  onChange={(e) => setRedeemForm(prev => ({ ...prev, customerId: e.target.value }))}
                >
                  {customers.map((customer) => (
                    <MenuItem key={customer.id} value={customer.id}>
                      {customer.name} ({customer.currentPoints} points)
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Reward</InputLabel>
                <Select
                  value={redeemForm.rewardId}
                  label="Reward"
                  onChange={(e) => setRedeemForm(prev => ({ ...prev, rewardId: e.target.value }))}
                >
                  {rewards.filter(r => r.isActive).map((reward) => (
                    <MenuItem key={reward.id} value={reward.id}>
                      {reward.name} ({reward.pointsCost} points)
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleRewardRedemptionClose}>Cancel</Button>
          <Button onClick={handleRewardRedemptionSubmit} variant="contained">
            Redeem
          </Button>
        </DialogActions>
      </Dialog>

      {/* Tier Update Dialog */}
      <Dialog open={tierDialogOpen} onClose={handleTierUpdateClose} maxWidth="sm" fullWidth>
        <DialogTitle>Update Customer Tier</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Customer</InputLabel>
                <Select
                  value={tierForm.customerId}
                  label="Customer"
                  onChange={(e) => setTierForm(prev => ({ ...prev, customerId: e.target.value }))}
                >
                  {customers.map((customer) => (
                    <MenuItem key={customer.id} value={customer.id}>
                      {customer.name} (Current: {customer.tier})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>New Tier</InputLabel>
                <Select
                  value={tierForm.newTier}
                  label="New Tier"
                  onChange={(e) => setTierForm(prev => ({ ...prev, newTier: e.target.value }))}
                >
                  <MenuItem value="Bronze">Bronze</MenuItem>
                  <MenuItem value="Silver">Silver</MenuItem>
                  <MenuItem value="Gold">Gold</MenuItem>
                  <MenuItem value="Platinum">Platinum</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Reason"
                multiline
                rows={3}
                value={tierForm.reason}
                onChange={(e) => setTierForm(prev => ({ ...prev, reason: e.target.value }))}
                placeholder="Reason for tier change"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleTierUpdateClose}>Cancel</Button>
          <Button onClick={handleTierUpdateSubmit} variant="contained">
            Update Tier
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default LoyaltyDashboard;
