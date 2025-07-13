import React, { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CircularProgress from '@mui/material/CircularProgress';
import Grid from '@mui/material/Grid';
import Typography from '@mui/material/Typography';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import InputLabel from '@mui/material/InputLabel';
import Alert from '@mui/material/Alert';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import RemoveIcon from '@mui/icons-material/Remove';
import AnalyticsService from '../../services/AnalyticsService';
import type { KpiResult } from '../../types/analyticsTypes';

const KpiDashboard = () => {
  const [kpis, setKpis] = useState<KpiResult[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [department, setDepartment] = useState<string>('all');

  useEffect(() => {
    const fetchKpis = async () => {
      try {
        setLoading(true);
        const analyticsService = new AnalyticsService();
        const data = await analyticsService.getKpis(department);
        setKpis(data);
        setError(null);
      } catch (err) {
        setError('Failed to fetch KPIs');
        console.error('Error fetching KPIs:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchKpis();
  }, [department]);

  const getTrendIcon = (trend: string) => {
    if (trend === 'up') return <ArrowUpwardIcon sx={{ color: 'success.main' }} />;
    if (trend === 'down') return <ArrowDownwardIcon sx={{ color: 'error.main' }} />;
    return <RemoveIcon sx={{ color: 'info.main' }} />;
  };

  const formatValue = (value: number, unit: string) => {
    if (unit === 'currency') return `$${value.toLocaleString()}`;
    if (unit === 'percent') return `${value.toFixed(1)}%`;
    return value.toLocaleString();
  };

  return (
    <Box sx={{ width: '100%', mb: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h5" component="h2">Key Performance Indicators</Typography>
        <FormControl size="small" sx={{ minWidth: 180 }}>
          <InputLabel id="department-select-label">Department</InputLabel>
          <Select
            labelId="department-select-label"
            id="department-select"
            value={department}
            onChange={(e) => setDepartment(e.target.value as string)}
            label="Department"
          >
            <MenuItem value="all">All Departments</MenuItem>
            <MenuItem value="sales">Sales</MenuItem>
            <MenuItem value="service">Service</MenuItem>
            <MenuItem value="parts">Parts</MenuItem>
            <MenuItem value="inventory">Inventory</MenuItem>
            <MenuItem value="financial">Financial</MenuItem>
          </Select>
        </FormControl>
      </Box>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Grid container spacing={3}>
          {kpis.map(kpi => (
            <Grid item xs={12} sm={6} md={4} lg={3} key={kpi.kpiId}>
              <Card variant="outlined" sx={{ height: '100%' }}>
                <CardContent>
                  <Typography variant="h6" gutterBottom>{kpi.name}</Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                    {getTrendIcon(kpi.trend)}
                    <Typography variant="h4" component="div" sx={{ ml: 1 }}>
                      {formatValue(kpi.value, kpi.unit)}
                    </Typography>
                  </Box>
                  
                  {kpi.changePercent !== undefined && (
                    <Typography variant="body2" color={kpi.trend === 'up' ? 'success.main' : kpi.trend === 'down' ? 'error.main' : 'text.secondary'}>
                      {kpi.changePercent > 0 ? '+' : ''}{kpi.changePercent.toFixed(1)}% from previous period
                    </Typography>
                  )}
                  
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                    <Typography variant="caption" color="text.secondary">
                      {kpi.department.toUpperCase()}
                    </Typography>
                    {kpi.previousValue !== undefined && (
                      <Typography variant="caption" color="text.secondary">
                        Previous: {formatValue(kpi.previousValue, kpi.unit)}
                      </Typography>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
};

export default KpiDashboard;
