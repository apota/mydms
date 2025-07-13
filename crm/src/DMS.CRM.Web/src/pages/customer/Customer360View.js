import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Box, 
  Typography, 
  Paper, 
  Grid, 
  Tabs, 
  Tab, 
  Button, 
  CircularProgress, 
  Alert, 
  Divider, 
  Chip, 
  IconButton,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Avatar,
  Badge
} from '@mui/material';
import { 
  Person, 
  DirectionsCar, 
  Assignment, 
  Build, 
  AccountBalance, 
  Stars, 
  Event, 
  Sync, 
  Phone, 
  Email,
  Message,
  Timeline,
  AttachMoney,
  LocalOffer,
  Notifications
} from '@mui/icons-material';
import { format } from 'date-fns';
import { IntegrationService } from '../../services/api-services';

function Customer360View() {
  const { customerId } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [customer360, setCustomer360] = useState(null);
  const [activeTab, setActiveTab] = useState(0);
  const [syncStatus, setSyncStatus] = useState(null);

  // Fetch customer 360 data
  useEffect(() => {
    const fetchCustomer360Data = async () => {
      try {
        setLoading(true);
        const response = await IntegrationService.getCustomer360(customerId);
        setCustomer360(response);
        setError(null);
      } catch (err) {
        console.error('Error fetching customer 360 data:', err);
        setError('Failed to load customer information. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    if (customerId) {
      fetchCustomer360Data();
    }
  }, [customerId]);

  const handleTabChange = (event, newValue) => {
    setActiveTab(newValue);
  };

  const handleSynchronize = async () => {
    try {
      setSyncStatus({ loading: true });
      const result = await IntegrationService.synchronizeCustomer(customerId);
      
      if (result.success) {
        setSyncStatus({ success: true, message: 'Customer data synchronized successfully' });
        // Refresh data after sync
        const updatedData = await IntegrationService.getCustomer360(customerId);
        setCustomer360(updatedData);
      } else {
        setSyncStatus({ 
          error: true, 
          message: `Synchronization failed: ${result.moduleMessages ? 
            Object.entries(result.moduleMessages)
              .filter(([_, msg]) => msg.includes('Failed'))
              .map(([module, msg]) => `${module}: ${msg}`)
              .join('; ') : 
            'Unknown error'}`
        });
      }
    } catch (err) {
      console.error('Error synchronizing customer data:', err);
      setSyncStatus({ error: true, message: 'Failed to synchronize customer data. Please try again later.' });
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  if (!customer360) {
    return (
      <Box sx={{ p: 3 }}>
        <Alert severity="warning">Customer information not found.</Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Customer 360° View
        </Typography>
        <Button 
          variant="contained" 
          startIcon={<Sync />} 
          onClick={handleSynchronize}
          disabled={syncStatus?.loading}
        >
          {syncStatus?.loading ? 'Syncing...' : 'Sync Customer Data'}
        </Button>
      </Box>

      {syncStatus && (
        <Alert 
          severity={syncStatus.error ? 'error' : 'success'} 
          sx={{ mb: 2 }}
          onClose={() => setSyncStatus(null)}
        >
          {syncStatus.message}
        </Alert>
      )}

      {/* Customer Overview Card */}
      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Avatar sx={{ width: 80, height: 80, mr: 2, bgcolor: 'primary.main' }}>
                {customer360.customer.firstName?.charAt(0)}{customer360.customer.lastName?.charAt(0)}
              </Avatar>
              <Box>
                <Typography variant="h5">
                  {customer360.customer.firstName} {customer360.customer.lastName}
                </Typography>
                <Typography variant="body1" color="textSecondary">
                  Customer ID: {customerId}
                </Typography>
                <Box sx={{ mt: 1 }}>
                  <Chip 
                    label={customer360.customer.customerType || 'Regular Customer'} 
                    size="small" 
                    color="primary" 
                    sx={{ mr: 1 }} 
                  />
                  <Chip 
                    label={customer360.loyaltyStatus?.tier || 'Bronze'} 
                    size="small" 
                    color="secondary"
                    icon={<Stars fontSize="small" />}
                  />
                </Box>
              </Box>
            </Box>
          </Grid>
          <Grid item xs={12} md={6}>
            <Grid container spacing={1}>
              <Grid item xs={6}>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <IconButton size="small" sx={{ mr: 1 }}>
                    <Email fontSize="small" />
                  </IconButton>
                  <Typography variant="body2">{customer360.customer.email || 'No email'}</Typography>
                </Box>
              </Grid>
              <Grid item xs={6}>
                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                  <IconButton size="small" sx={{ mr: 1 }}>
                    <Phone fontSize="small" />
                  </IconButton>
                  <Typography variant="body2">{customer360.customer.phoneNumber || 'No phone'}</Typography>
                </Box>
              </Grid>
              <Grid item xs={12}>
                <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                  <IconButton size="small" sx={{ mr: 1 }}>
                    <LocalOffer fontSize="small" />
                  </IconButton>
                  <Typography variant="body2">
                    Lead Source: {customer360.customer.leadSource || 'Unknown'}
                  </Typography>
                </Box>
              </Grid>
              <Grid item xs={12}>
                <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                  <Badge 
                    color="error" 
                    badgeContent={customer360.serviceReminders?.length || 0}
                    max={99}
                    sx={{ mr: 2 }}
                  >
                    <Notifications />
                  </Badge>
                  <Typography variant="body2">
                    {customer360.serviceReminders?.length || 0} Active Reminders
                  </Typography>
                </Box>
              </Grid>
            </Grid>
          </Grid>
        </Grid>
      </Paper>

      {/* Key Metrics */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Lifetime Value
              </Typography>
              <Typography variant="h5" component="div">
                ${customer360.metrics?.lifetimeValue?.toLocaleString() || '0'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Loyalty Points
              </Typography>
              <Typography variant="h5" component="div">
                {customer360.loyaltyStatus?.currentPoints || 0} pts
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Vehicles Owned
              </Typography>
              <Typography variant="h5" component="div">
                {customer360.vehicles?.length || 0}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Outstanding Balance
              </Typography>
              <Typography variant="h5" component="div" color={customer360.financialStatus?.totalOutstanding > 0 ? 'error' : 'initial'}>
                ${customer360.financialStatus?.totalOutstanding?.toLocaleString() || '0'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Tabs for detailed information */}
      <Paper sx={{ width: '100%', mb: 4 }}>
        <Tabs value={activeTab} onChange={handleTabChange} aria-label="customer 360 tabs">
          <Tab icon={<Person />} iconPosition="start" label="Profile" />
          <Tab icon={<DirectionsCar />} iconPosition="start" label="Vehicles" />
          <Tab icon={<Assignment />} iconPosition="start" label="Purchases" />
          <Tab icon={<Build />} iconPosition="start" label="Services" />
          <Tab icon={<AccountBalance />} iconPosition="start" label="Financial" />
          <Tab icon={<Stars />} iconPosition="start" label="Loyalty" />
          <Tab icon={<Timeline />} iconPosition="start" label="Journey" />
          <Tab icon={<Message />} iconPosition="start" label="Communications" />
        </Tabs>

        <Box sx={{ p: 3 }}>
          {/* Profile Tab */}
          {activeTab === 0 && (
            <Grid container spacing={3}>
              {/* Profile details would go here */}
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>Personal Information</Typography>
                <List>
                  <ListItem>
                    <ListItemText primary="Name" secondary={`${customer360.customer.firstName} ${customer360.customer.lastName}`} />
                  </ListItem>
                  <ListItem>
                    <ListItemText primary="Email" secondary={customer360.customer.email} />
                  </ListItem>
                  <ListItem>
                    <ListItemText primary="Phone" secondary={customer360.customer.phoneNumber} />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Address" 
                      secondary={customer360.customer.address ? 
                        `${customer360.customer.address.street1}${customer360.customer.address.street2 ? ', ' + customer360.customer.address.street2 : ''}, 
                        ${customer360.customer.address.city}, ${customer360.customer.address.state} ${customer360.customer.address.zipCode}` : 
                        'No address on file'} 
                    />
                  </ListItem>
                </List>
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>Preferences & Settings</Typography>
                <List>
                  <ListItem>
                    <ListItemText 
                      primary="Contact Preference" 
                      secondary={customer360.customer.preferredContactMethod || 'Not specified'} 
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText primary="Account Created" secondary={
                      customer360.customer.createdAt ? 
                      format(new Date(customer360.customer.createdAt), 'PPP') : 
                      'Unknown'} 
                    />
                  </ListItem>
                  <ListItem>
                    <ListItemText 
                      primary="Marketing Preferences" 
                      secondary={
                        customer360.preferences?.marketingPreferences ? 
                        Object.entries(customer360.preferences.marketingPreferences)
                          .filter(([_, value]) => value)
                          .map(([key]) => key)
                          .join(', ') : 
                        'None specified'
                      } 
                    />
                  </ListItem>
                </List>
              </Grid>

              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>Recommended Actions</Typography>
                <List>
                  {customer360.recommendedActions && customer360.recommendedActions.length > 0 ? (
                    customer360.recommendedActions.map((action, index) => (
                      <ListItem key={index} divider={index < customer360.recommendedActions.length - 1}>
                        <ListItemAvatar>
                          <Avatar sx={{ bgcolor: action.priority === 'High' ? 'error.main' : action.priority === 'Medium' ? 'warning.main' : 'success.main' }}>
                            {action.priority === 'High' ? '!' : action.priority === 'Medium' ? '!!' : '✓'}
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText 
                          primary={action.description} 
                          secondary={`Priority: ${action.priority} | Due: ${format(new Date(action.dueDate), 'PPP')}`} 
                        />
                        <Button variant="outlined" size="small">
                          Take Action
                        </Button>
                      </ListItem>
                    ))
                  ) : (
                    <ListItem>
                      <ListItemText primary="No recommended actions at this time." />
                    </ListItem>
                  )}
                </List>
              </Grid>
            </Grid>
          )}

          {/* Vehicles Tab */}
          {activeTab === 1 && (
            <div>
              <Typography variant="h6" gutterBottom>Customer Vehicles</Typography>
              {customer360.vehicles?.length > 0 ? (
                <Grid container spacing={2}>
                  {customer360.vehicles.map((vehicle) => (
                    <Grid item xs={12} md={6} key={vehicle.id || vehicle.vin}>
                      <Card variant="outlined">
                        <CardContent>
                          <Typography variant="h6">{vehicle.year} {vehicle.make} {vehicle.model}</Typography>
                          <Typography color="textSecondary">VIN: {vehicle.vin}</Typography>
                          <Typography>Color: {vehicle.color}</Typography>
                          <Typography>License: {vehicle.licensePlate}</Typography>
                          <Typography>Last Service: {vehicle.lastServiceDate ? 
                            format(new Date(vehicle.lastServiceDate), 'PPP') : 'Never'}</Typography>
                        </CardContent>
                      </Card>
                    </Grid>
                  ))}
                </Grid>
              ) : (
                <Alert severity="info">No vehicles registered to this customer.</Alert>
              )}

              <Box sx={{ mt: 4 }}>
                <Typography variant="h6" gutterBottom>Recommended Vehicles</Typography>
                {customer360.recommendedVehicles?.length > 0 ? (
                  <Grid container spacing={2}>
                    {customer360.recommendedVehicles.map((vehicle) => (
                      <Grid item xs={12} sm={6} md={4} key={vehicle.id}>
                        <Card variant="outlined">
                          <CardContent>
                            <Typography variant="h6">{vehicle.year} {vehicle.make} {vehicle.model}</Typography>
                            <Typography color="textSecondary">{vehicle.trim}</Typography>
                            <Typography>${vehicle.price?.toLocaleString()}</Typography>
                            <Typography>Match Score: {vehicle.matchScore}%</Typography>
                            <Box sx={{ mt: 2 }}>
                              <Button variant="contained" size="small" sx={{ mr: 1 }}>View Details</Button>
                              <Button variant="outlined" size="small">Contact Sales</Button>
                            </Box>
                          </CardContent>
                        </Card>
                      </Grid>
                    ))}
                  </Grid>
                ) : (
                  <Alert severity="info">No vehicle recommendations available.</Alert>
                )}
              </Box>
            </div>
          )}

          {/* Purchase History Tab */}
          {activeTab === 2 && (
            <div>
              <Typography variant="h6" gutterBottom>Purchase History</Typography>
              {customer360.purchaseHistory?.length > 0 ? (
                <List>
                  {customer360.purchaseHistory.map((purchase) => (
                    <ListItem key={purchase.id} divider>
                      <ListItemAvatar>
                        <Avatar>
                          <DirectionsCar />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText 
                        primary={purchase.vehicleDetails} 
                        secondary={`Date: ${format(new Date(purchase.purchaseDate), 'PPP')} | Amount: $${purchase.amount?.toLocaleString()} | Sales Rep: ${purchase.salesPerson}`} 
                      />
                      <Button variant="outlined" size="small">
                        View Details
                      </Button>
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Alert severity="info">No purchase history available.</Alert>
              )}
            </div>
          )}

          {/* Service History Tab */}
          {activeTab === 3 && (
            <div>
              <Typography variant="h6" gutterBottom>Service History</Typography>
              {customer360.serviceHistory?.length > 0 ? (
                <List>
                  {customer360.serviceHistory.map((service) => (
                    <ListItem key={service.id} divider>
                      <ListItemAvatar>
                        <Avatar>
                          <Build />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText 
                        primary={service.serviceType} 
                        secondary={`Date: ${format(new Date(service.serviceDate), 'PPP')} | Vehicle: ${service.vehicleDetails} | Status: ${service.status}`} 
                      />
                      <Chip 
                        label={service.status} 
                        color={service.status === 'Completed' ? 'success' : service.status === 'Scheduled' ? 'info' : 'warning'} 
                        size="small" 
                        sx={{ mr: 2 }}
                      />
                      <Button variant="outlined" size="small">
                        View Details
                      </Button>
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Alert severity="info">No service history available.</Alert>
              )}

              <Box sx={{ mt: 4 }}>
                <Typography variant="h6" gutterBottom>Upcoming Appointments</Typography>
                {customer360.upcomingAppointments?.length > 0 ? (
                  <List>
                    {customer360.upcomingAppointments.map((appointment) => (
                      <ListItem key={appointment.id} divider>
                        <ListItemAvatar>
                          <Avatar>
                            <Event />
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText 
                          primary={appointment.serviceType} 
                          secondary={`Date: ${format(new Date(appointment.appointmentDate), 'PPP p')} | Vehicle: ${appointment.vehicleDetails}`} 
                        />
                        <Box>
                          <Button variant="contained" size="small" sx={{ mr: 1 }}>Reschedule</Button>
                          <Button variant="outlined" size="small" color="error">Cancel</Button>
                        </Box>
                      </ListItem>
                    ))}
                  </List>
                ) : (
                  <Alert severity="info">No upcoming appointments.</Alert>
                )}
              </Box>
            </div>
          )}

          {/* Financial Tab */}
          {activeTab === 4 && (
            <div>
              <Typography variant="h6" gutterBottom>Financial Overview</Typography>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Card variant="outlined" sx={{ mb: 2 }}>
                    <CardContent>
                      <Typography variant="subtitle1" gutterBottom>Outstanding Balance</Typography>
                      <Typography variant="h4" color={customer360.financialStatus?.totalOutstanding > 0 ? 'error' : 'success'}>
                        ${customer360.financialStatus?.totalOutstanding?.toLocaleString() || '0'}
                      </Typography>
                      {customer360.financialStatus?.pastDueAmount > 0 && (
                        <Typography color="error" variant="body2">
                          Past Due: ${customer360.financialStatus.pastDueAmount.toLocaleString()}
                        </Typography>
                      )}
                      {customer360.financialStatus?.nextPaymentDue && (
                        <Box sx={{ mt: 1 }}>
                          <Typography variant="body2">
                            Next Payment Due: {format(new Date(customer360.financialStatus.nextPaymentDue), 'PPP')}
                          </Typography>
                          <Typography variant="body2">
                            Amount: ${customer360.financialStatus.nextPaymentAmount?.toLocaleString() || '0'}
                          </Typography>
                        </Box>
                      )}
                    </CardContent>
                  </Card>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="subtitle1" gutterBottom>Credit Profile</Typography>
                      <Box sx={{ display: 'flex', alignItems: 'center' }}>
                        <Typography variant="h6" sx={{ mr: 2 }}>Score: {customer360.financialStatus?.creditScore || 'N/A'}</Typography>
                        <Chip 
                          label={customer360.financialStatus?.creditScore >= 700 ? 'Excellent' : customer360.financialStatus?.creditScore >= 650 ? 'Good' : customer360.financialStatus?.creditScore >= 600 ? 'Fair' : 'Needs Improvement'} 
                          color={customer360.financialStatus?.creditScore >= 700 ? 'success' : customer360.financialStatus?.creditScore >= 650 ? 'primary' : customer360.financialStatus?.creditScore >= 600 ? 'warning' : 'error'} 
                        />
                      </Box>
                      <Typography variant="body2" sx={{ mt: 1 }}>
                        Last Updated: {customer360.financialStatus?.lastCreditCheck ? format(new Date(customer360.financialStatus.lastCreditCheck), 'PPP') : 'Never'}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <Typography variant="h6" sx={{ mt: 3, mb: 2 }}>Financing Details</Typography>
              {customer360.financingDetails?.length > 0 ? (
                <List sx={{ bgcolor: 'background.paper' }}>
                  {customer360.financingDetails.map((financing) => (
                    <ListItem key={financing.id} divider>
                      <ListItemAvatar>
                        <Avatar>
                          <AttachMoney />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText 
                        primary={`${financing.financingType} - ${financing.vehicleDetails || 'No vehicle details'}`} 
                        secondary={
                          <>
                            <Typography component="span" variant="body2">
                              Amount: ${financing.amount?.toLocaleString() || '0'} | Term: {financing.termMonths || 0} months | Rate: {financing.interestRate || 0}%
                            </Typography>
                            <br />
                            <Typography component="span" variant="body2">
                              Start Date: {financing.startDate ? format(new Date(financing.startDate), 'PPP') : 'N/A'} | 
                              Status: {financing.status || 'Unknown'}
                            </Typography>
                          </>
                        } 
                      />
                      <Button variant="outlined" size="small">
                        View Details
                      </Button>
                    </ListItem>
                  ))}
                </List>
              ) : (
                <Alert severity="info">No financing information available.</Alert>
              )}
            </div>
          )}

          {/* Loyalty Tab */}
          {activeTab === 5 && (
            <div>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Card variant="outlined" sx={{ mb: 2 }}>
                    <CardContent>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                        <Typography variant="h6">Loyalty Status</Typography>
                        <Chip 
                          label={customer360.loyaltyStatus?.tier || 'Bronze'} 
                          color="secondary"
                          icon={<Stars fontSize="small" />}
                        />
                      </Box>
                      <Typography variant="body1" gutterBottom>
                        Current Points: <strong>{customer360.loyaltyStatus?.currentPoints || 0}</strong>
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        Points to Next Tier: {customer360.loyaltyStatus?.pointsToNextTier || 0}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        Next Tier: {customer360.loyaltyStatus?.nextTier || 'Max tier reached'}
                      </Typography>
                      <Typography variant="body2" color="textSecondary">
                        Member Since: {customer360.loyaltyStatus?.enrollmentDate ? 
                          format(new Date(customer360.loyaltyStatus.enrollmentDate), 'PPP') : 'Unknown'}
                      </Typography>
                      <Button variant="contained" color="primary" sx={{ mt: 2 }}>
                        View Tier Benefits
                      </Button>
                    </CardContent>
                  </Card>
                </Grid>
                
                <Grid item xs={12} md={6}>
                  <Card variant="outlined">
                    <CardContent>
                      <Typography variant="h6" gutterBottom>
                        Available Rewards
                      </Typography>
                      {customer360.loyaltyStatus?.availableRewards?.length > 0 ? (
                        <List dense>
                          {customer360.loyaltyStatus.availableRewards.map((reward, index) => (
                            <ListItem key={index} divider>
                              <ListItemText 
                                primary={reward.name} 
                                secondary={`${reward.pointsCost} points required`} 
                              />
                              <Button 
                                variant="outlined" 
                                size="small"
                                disabled={customer360.loyaltyStatus.currentPoints < reward.pointsCost}
                              >
                                Redeem
                              </Button>
                            </ListItem>
                          ))}
                        </List>
                      ) : (
                        <Typography>No rewards currently available</Typography>
                      )}
                    </CardContent>
                  </Card>
                </Grid>
              </Grid>

              <Typography variant="h6" sx={{ mt: 3, mb: 1 }}>Recent Point Activity</Typography>
              <Paper variant="outlined">
                <List dense>
                  {customer360.loyaltyStatus?.pointHistory?.length > 0 ? (
                    customer360.loyaltyStatus.pointHistory.map((activity, index) => (
                      <ListItem key={index} divider>
                        <ListItemText 
                          primary={activity.description} 
                          secondary={`Date: ${activity.date ? format(new Date(activity.date), 'PPP') : 'Unknown'}`} 
                        />
                        <Typography variant="body1" color={activity.points > 0 ? 'success.main' : 'error.main'}>
                          {activity.points > 0 ? '+' : ''}{activity.points} points
                        </Typography>
                      </ListItem>
                    ))
                  ) : (
                    <ListItem>
                      <ListItemText primary="No recent loyalty activity" />
                    </ListItem>
                  )}
                </List>
              </Paper>
            </div>
          )}

          {/* Customer Journey Tab */}
          {activeTab === 6 && (
            <div>
              <Typography variant="h6" gutterBottom>Customer Journey</Typography>
              <Alert severity="info" sx={{ mb: 2 }}>
                This feature shows the customer's progression through different stages of their relationship with your dealership.
              </Alert>
              {customer360.customerJourney ? (
                <Box sx={{ p: 2 }}>
                  {/* Journey visualization would go here */}
                  <Typography>Current Stage: {customer360.customerJourney.currentStage || 'Prospect'}</Typography>
                  <Typography variant="body2">Days in Current Stage: {customer360.customerJourney.daysInCurrentStage || 0}</Typography>
                  <Box sx={{ my: 3 }}>
                    <Typography variant="subtitle1" gutterBottom>Journey Timeline</Typography>
                    {/* Timeline visualization placeholder */}
                    <Box sx={{ display: 'flex', alignItems: 'center', overflow: 'auto', py: 2 }}>
                      {['Prospect', 'Lead', 'Opportunity', 'Customer', 'Repeat Customer', 'Advocate'].map((stage, index) => (
                        <Box key={index} sx={{ 
                          display: 'flex', 
                          flexDirection: 'column', 
                          alignItems: 'center',
                          minWidth: '150px'
                        }}>
                          <Avatar 
                            sx={{ 
                              bgcolor: customer360.customerJourney.currentStage === stage ? 'success.main' : 
                                index < ['Prospect', 'Lead', 'Opportunity', 'Customer', 'Repeat Customer', 'Advocate']
                                  .indexOf(customer360.customerJourney.currentStage) ? 'primary.main' : 'grey.400',
                              mb: 1
                            }}
                          >
                            {index + 1}
                          </Avatar>
                          <Typography variant="body2">{stage}</Typography>
                          {customer360.customerJourney.stageTransitions && 
                           customer360.customerJourney.stageTransitions[stage] && (
                            <Typography variant="caption">
                              {format(new Date(customer360.customerJourney.stageTransitions[stage]), 'PP')}
                            </Typography>
                          )}
                          {index < 5 && (
                            <Box sx={{ width: '50px', height: '2px', bgcolor: 'divider', my: 2 }} />
                          )}
                        </Box>
                      ))}
                    </Box>
                  </Box>
                </Box>
              ) : (
                <Alert severity="warning">Journey data not available for this customer.</Alert>
              )}
            </div>
          )}

          {/* Communications Tab */}
          {activeTab === 7 && (
            <div>
              <Typography variant="h6" gutterBottom>Customer Communications</Typography>
              <Box sx={{ display: 'flex', mb: 2 }}>
                <Button variant="contained" startIcon={<Email />} sx={{ mr: 1 }}>Compose Email</Button>
                <Button variant="outlined" startIcon={<Message />} sx={{ mr: 1 }}>Send SMS</Button>
                <Button variant="outlined" startIcon={<Phone />}>Log Phone Call</Button>
              </Box>
              
              <Typography variant="subtitle1" sx={{ mt: 3, mb: 1 }}>Recent Communications</Typography>
              <List>
                {customer360.interactions?.filter(i => i.interactionType === 'Email' || i.interactionType === 'SMS' || i.interactionType === 'Call')
                  .slice(0, 5)
                  .map((comm, index) => (
                    <ListItem key={index} divider>
                      <ListItemAvatar>
                        <Avatar>
                          {comm.interactionType === 'Email' ? <Email /> : 
                           comm.interactionType === 'SMS' ? <Message /> : <Phone />}
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText 
                        primary={comm.description} 
                        secondary={`Date: ${comm.interactionDate ? format(new Date(comm.interactionDate), 'PPP p') : 'Unknown'} | By: ${comm.staffMember || 'System'}`} 
                      />
                      <Button variant="text" size="small">
                        View
                      </Button>
                    </ListItem>
                  ))}
              </List>

              {(!customer360.interactions || 
                customer360.interactions.filter(i => i.interactionType === 'Email' || i.interactionType === 'SMS' || i.interactionType === 'Call').length === 0) && 
                <Alert severity="info">No recent communications found.</Alert>
              }
            </div>
          )}
        </Box>
      </Paper>
    </Box>
  );
}

export default Customer360View;
