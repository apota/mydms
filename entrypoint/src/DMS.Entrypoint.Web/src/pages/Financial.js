import React from 'react';
import { Box, Typography, Card, CardContent, Grid, Button } from '@mui/material';
import { AccountBalance, Add } from '@mui/icons-material';

export default function Financial() {
  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Financial Management
        </Typography>
        <Button variant="contained" startIcon={<Add />}>
          New Transaction
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Financial Reports
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Generate comprehensive financial reports including P&L, balance sheets, and cash flow.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Accounts Management
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Manage accounts receivable, payable, and general ledger entries.
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
