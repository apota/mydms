import React, { useState, useEffect } from 'react';
import { Routes, Route, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  Container,
  Tabs,
  Tab,
  Paper,
  Alert,
  Snackbar,
  Button,
  IconButton
} from '@mui/material';
import { 
  Settings as SettingsIcon,
  ArrowBack as ArrowBackIcon,
  Home as HomeIcon 
} from '@mui/icons-material';

// Import components
import UserSettings from './components/UserSettings';
import SystemSettings from './components/SystemSettings';
import { SettingsProvider } from './context/SettingsContext';

const App = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [currentTab, setCurrentTab] = useState(0);
  const [notification, setNotification] = useState({ open: false, message: '', severity: 'info' });

  // Map routes to tab indices
  const routeToTab = {
    '/': 0,
    '/user': 0,
    '/system': 1
  };

  const tabToRoute = ['/', '/system'];

  useEffect(() => {
    const tabIndex = routeToTab[location.pathname] || 0;
    setCurrentTab(tabIndex);
  }, [location.pathname]);

  const handleTabChange = (event, newValue) => {
    setCurrentTab(newValue);
    navigate(tabToRoute[newValue]);
  };

  const handleBackToDashboard = () => {
    // Navigate back to the main DMS dashboard
    window.location.href = 'http://localhost:3000';
  };

  const showNotification = (message, severity = 'info') => {
    setNotification({ open: true, message, severity });
  };

  const hideNotification = () => {
    setNotification({ ...notification, open: false });
  };

  return (
    <SettingsProvider>
      <Box sx={{ flexGrow: 1 }}>
        {/* App Bar */}
        <AppBar position="static" elevation={1}>
          <Toolbar>
            <IconButton
              edge="start"
              color="inherit"
              onClick={handleBackToDashboard}
              sx={{ mr: 2 }}
              title="Back to DMS Dashboard"
            >
              <ArrowBackIcon />
            </IconButton>
            <SettingsIcon sx={{ mr: 2 }} />
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Settings Management
            </Typography>
            <Button
              color="inherit"
              startIcon={<HomeIcon />}
              onClick={handleBackToDashboard}
              sx={{ ml: 2 }}
            >
              Dashboard
            </Button>
          </Toolbar>
        </AppBar>

        {/* Navigation Tabs */}
        <Paper square sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Container maxWidth="lg">
            <Tabs 
              value={currentTab} 
              onChange={handleTabChange}
              aria-label="settings navigation"
            >
              <Tab label="User Settings" />
              <Tab label="System Settings" />
            </Tabs>
          </Container>
        </Paper>

        {/* Main Content */}
        <Container maxWidth="lg" sx={{ mt: 3, mb: 4 }}>
          <Routes>
            <Route 
              path="/" 
              element={<UserSettings showNotification={showNotification} />} 
            />
            <Route 
              path="/user" 
              element={<UserSettings showNotification={showNotification} />} 
            />
            <Route 
              path="/system" 
              element={<SystemSettings showNotification={showNotification} />} 
            />
          </Routes>
        </Container>

        {/* Notification Snackbar */}
        <Snackbar
          open={notification.open}
          autoHideDuration={6000}
          onClose={hideNotification}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        >
          <Alert 
            onClose={hideNotification} 
            severity={notification.severity}
            sx={{ width: '100%' }}
          >
            {notification.message}
          </Alert>
        </Snackbar>
      </Box>
    </SettingsProvider>
  );
};

export default App;
