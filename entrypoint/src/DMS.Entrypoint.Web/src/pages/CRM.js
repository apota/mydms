import React, { useEffect } from 'react';
import { Box, Typography, Card, CardContent, Grid, Button, Alert, CircularProgress } from '@mui/material';
import { People, Launch, Dashboard as DashboardIcon } from '@mui/icons-material';

export default function CRM() {
  const [crmStatus, setCrmStatus] = React.useState('checking');
  const crmUrl = 'http://localhost:3004';

  useEffect(() => {
    // Check if CRM service is available and redirect immediately
    const checkAndRedirect = async () => {
      try {
        const response = await fetch('http://localhost:8093/api/dashboard');
        if (response.ok) {
          setCrmStatus('available');
          // Redirect immediately to CRM dashboard
          window.location.href = crmUrl;
        } else {
          setCrmStatus('unavailable');
        }
      } catch (error) {
        setCrmStatus('unavailable');
      }
    };

    checkAndRedirect();
  }, [crmUrl]);

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Customer Relationship Management
        </Typography>
      </Box>

      {crmStatus === 'checking' && (
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <CircularProgress size={20} sx={{ mr: 2 }} />
          <Typography>Checking CRM service and redirecting...</Typography>
        </Box>
      )}

      {crmStatus === 'unavailable' && (
        <>
          <Alert severity="error" sx={{ mb: 3 }}>
            ❌ CRM Dashboard service is currently unavailable. Please ensure the CRM service is running on port 8093.
          </Alert>
          
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>
                    Service Unavailable
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    The CRM dashboard service could not be reached. Please check that:
                  </Typography>
                  <ul style={{ marginTop: '8px' }}>
                    <li>The CRM API service is running on port 8093</li>
                    <li>Docker containers are properly started</li>
                    <li>Network connectivity is available</li>
                  </ul>
                  <Button 
                    variant="outlined" 
                    onClick={() => window.location.reload()}
                    sx={{ mt: 2 }}
                  >
                    Retry Connection
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </>
      )}
    </Box>
  );
}
