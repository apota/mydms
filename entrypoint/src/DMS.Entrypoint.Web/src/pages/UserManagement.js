import React, { useEffect } from 'react';
import { Box, Typography, Card, CardContent, Grid, Button, CircularProgress } from '@mui/material';
import { ManageAccounts, Launch } from '@mui/icons-material';

export default function UserManagement() {
  const handleOpenUserManagement = () => {
    // Open the dedicated user management application
    window.open('http://localhost:3003', '_blank');
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          User Management
        </Typography>
        <Button 
          variant="contained" 
          startIcon={<Launch />}
          onClick={handleOpenUserManagement}
        >
          Open User Management
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card>
            <CardContent sx={{ textAlign: 'center', py: 6 }}>
              <ManageAccounts sx={{ fontSize: 80, color: 'primary.main', mb: 2 }} />
              <Typography variant="h5" gutterBottom>
                User Management System
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3, maxWidth: 600, mx: 'auto' }}>
                The User Management system has been migrated to a dedicated application. 
                Click the button above to access the full user management interface where you can 
                manage user accounts, roles, permissions, and security settings.
              </Typography>
              <Button 
                variant="contained" 
                size="large"
                startIcon={<Launch />}
                onClick={handleOpenUserManagement}
              >
                Open User Management Application
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
