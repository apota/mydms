import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Container, Box, AppBar, Toolbar, Typography, Button } from '@mui/material';
import { People as PeopleIcon, Home as HomeIcon } from '@mui/icons-material';
import UserManagement from './pages/UserManagement';

function App() {
  const handleBackToDashboard = () => {
    // Navigate back to the main DMS dashboard
    window.location.href = 'http://localhost:3000';
  };

  return (
    <Box sx={{ flexGrow: 1 }}>
      {/* App Bar */}
      <AppBar position="static" elevation={1}>
        <Toolbar>
          <PeopleIcon sx={{ mr: 2 }} />
          <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
            DMS User Management
          </Typography>
          <Button 
            color="inherit" 
            startIcon={<HomeIcon />}
            onClick={handleBackToDashboard}
          >
            Back to Dashboard
          </Button>
        </Toolbar>
      </AppBar>

      {/* Main Content */}
      <Container maxWidth="xl" sx={{ mt: 3, mb: 3 }}>
        <Routes>
          <Route path="/" element={<Navigate to="/users" replace />} />
          <Route path="/users" element={<UserManagement />} />
          <Route path="*" element={<Navigate to="/users" replace />} />
        </Routes>
      </Container>
    </Box>
  );
}

export default App;
