import React from 'react';
import { Box, Typography, Card, CardContent, Grid, Button } from '@mui/material';
import { Build, Add } from '@mui/icons-material';

export default function Service() {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Service Management
        </Typography>
        <Button variant="contained" startIcon={<Add />}>
          Schedule Service
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Service Appointments
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Schedule and manage service appointments with automated reminders.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Work Orders
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Create and track work orders with technician assignment and progress tracking.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
