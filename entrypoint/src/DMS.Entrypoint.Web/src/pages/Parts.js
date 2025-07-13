import React from 'react';
import { Box, Typography, Card, CardContent, Grid, Button } from '@mui/material';
import { Extension, Add } from '@mui/icons-material';

export default function Parts() {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Parts Management
        </Typography>
        <Button variant="contained" startIcon={<Add />}>
          Add Part
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Parts Inventory
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Track parts inventory with automated reordering and supplier management.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Parts Catalog
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Browse and search comprehensive parts catalog with pricing and availability.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
