import React from 'react';
import { Box, Typography, Card, CardContent, Grid, Button } from '@mui/material';
import { TrendingUp, Add } from '@mui/icons-material';

export default function Sales() {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Sales Management
        </Typography>
        <Button variant="contained" startIcon={<Add />}>
          New Lead
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Sales Pipeline
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Track and manage your sales leads and deals through the entire sales process.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Performance Metrics
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Monitor sales performance with real-time analytics and reporting.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
