import React from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Grid,
  Card,
  CardContent,
  Avatar,
  Chip
} from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { LogoutOutlined, PersonOutlined, EmailOutlined, CalendarTodayOutlined } from '@mui/icons-material';

const Dashboard = () => {
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  return (
    <Container component="main" maxWidth="lg">
      <Box sx={{ marginTop: 4, marginBottom: 4 }}>
        {/* Header */}
        <Paper elevation={2} sx={{ padding: 3, mb: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <Avatar
                sx={{ 
                  width: 60, 
                  height: 60, 
                  mr: 2, 
                  bgcolor: 'primary.main',
                  fontSize: '1.5rem'
                }}
              >
                {user?.firstName?.[0]?.toUpperCase() || 'U'}
              </Avatar>
              <Box>
                <Typography variant="h4" gutterBottom>
                  Welcome back, {user?.firstName || 'User'}!
                </Typography>
                <Typography variant="body1" color="text.secondary">
                  DMS Login Management Dashboard
                </Typography>
              </Box>
            </Box>
            <Button
              variant="outlined"
              startIcon={<LogoutOutlined />}
              onClick={handleLogout}
              color="inherit"
            >
              Logout
            </Button>
          </Box>
        </Paper>

        {/* User Information Cards */}
        <Grid container spacing={3}>
          <Grid item xs={12} md={6}>
            <Card elevation={2}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <PersonOutlined sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="h6">
                    Profile Information
                  </Typography>
                </Box>
                
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Full Name
                  </Typography>
                  <Typography variant="body1">
                    {user?.firstName} {user?.lastName}
                  </Typography>
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    User ID
                  </Typography>
                  <Typography variant="body1" sx={{ fontFamily: 'monospace' }}>
                    {user?.id}
                  </Typography>
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Account Status
                  </Typography>
                  <Chip 
                    label={user?.isEmailVerified ? 'Verified' : 'Pending Verification'} 
                    color={user?.isEmailVerified ? 'success' : 'warning'}
                    size="small"
                  />
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} md={6}>
            <Card elevation={2}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <EmailOutlined sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="h6">
                    Contact Information
                  </Typography>
                </Box>
                
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Email Address
                  </Typography>
                  <Typography variant="body1">
                    {user?.email}
                  </Typography>
                </Box>

                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    Email Status
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <Chip 
                      label={user?.isEmailVerified ? 'Verified' : 'Not Verified'} 
                      color={user?.isEmailVerified ? 'success' : 'error'}
                      size="small"
                    />
                    {!user?.isEmailVerified && (
                      <Button
                        variant="text"
                        size="small"
                        sx={{ ml: 1 }}
                        onClick={() => {
                          // TODO: Implement resend verification
                          console.log('Resend verification email');
                        }}
                      >
                        Resend
                      </Button>
                    )}
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12}>
            <Card elevation={2}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <CalendarTodayOutlined sx={{ mr: 1, color: 'primary.main' }} />
                  <Typography variant="h6">
                    Account Timeline
                  </Typography>
                </Box>
                
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6}>
                    <Box>
                      <Typography variant="body2" color="text.secondary">
                        Account Created
                      </Typography>
                      <Typography variant="body1">
                        {formatDate(user?.createdAt)}
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <Box>
                      <Typography variant="body2" color="text.secondary">
                        Last Updated
                      </Typography>
                      <Typography variant="body1">
                        {formatDate(user?.updatedAt)}
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Quick Actions */}
        <Paper elevation={2} sx={{ padding: 3, mt: 3 }}>
          <Typography variant="h6" gutterBottom>
            Quick Actions
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <Button variant="outlined" onClick={() => console.log('Change password')}>
              Change Password
            </Button>
            <Button variant="outlined" onClick={() => console.log('Update profile')}>
              Update Profile
            </Button>
            <Button variant="outlined" onClick={() => console.log('Download data')}>
              Download My Data
            </Button>
            <Button variant="outlined" color="error" onClick={() => console.log('Delete account')}>
              Delete Account
            </Button>
          </Box>
        </Paper>
      </Box>
    </Container>
  );
};

export default Dashboard;
