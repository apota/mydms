import React from 'react';
import { Box, Typography, Card, CardContent, Grid, Button } from '@mui/material';
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
                User Management Application
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                The user management functionality has been moved to a dedicated microservice. 
                Click the button above to access the full user management application.
              </Typography>
              <Button 
                variant="outlined" 
                size="large"
                startIcon={<Launch />}
                onClick={handleOpenUserManagement}
              >
                Launch User Management
              </Button>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
