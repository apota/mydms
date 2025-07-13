import React, { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Typography from '@mui/material/Typography';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import AnalyticsService from '../../services/AnalyticsService';
import type { ForecastPoint, ForecastResult, ForecastRequest } from '../../types/analyticsTypes';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

const SalesForecast: React.FC = () => {
  const [forecast, setForecast] = useState<ForecastResult | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [metricName, setMetricName] = useState<string>('total_sales');
  const [timeGranularity, setTimeGranularity] = useState<string>('month');
  const [periods, setPeriods] = useState<number>(6);

  useEffect(() => {
    const fetchForecast = async () => {
      try {
        setLoading(true);
        const analyticsService = new AnalyticsService();
        
        const request: ForecastRequest = {
          metricName,
          timeGranularity,
          periods
        };
        
        const data = await analyticsService.generateForecast(request);
        setForecast(data);
        setError(null);
      } catch (err) {
        setError('Failed to generate sales forecast');
        console.error('Error generating forecast:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchForecast();
  }, [metricName, timeGranularity, periods]);

  const handleMetricChange = (event: SelectChangeEvent) => {
    setMetricName(event.target.value);
  };

  const handleTimeGranularityChange = (event: SelectChangeEvent) => {
    setTimeGranularity(event.target.value);
  };

  const handlePeriodsChange = (event: SelectChangeEvent) => {
    setPeriods(Number(event.target.value));
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    const formatter = new Intl.DateTimeFormat('en-US', { 
      month: 'short', 
      year: 'numeric',
      day: timeGranularity === 'day' ? 'numeric' : undefined 
    });
    return formatter.format(date);
  };

  const getChartData = () => {
    if (!forecast) return {
      labels: [],
      datasets: []
    };

    return {
      labels: forecast.points.map((p: ForecastPoint) => formatDate(p.date)),
      datasets: [
        {
          label: 'Forecast',
          data: forecast.points.map((p: ForecastPoint) => p.value),
          borderColor: 'rgb(53, 162, 235)',
          backgroundColor: 'rgba(53, 162, 235, 0.5)',
          tension: 0.4,
        },
        forecast.points[0].lowerBound !== undefined ? {
          label: 'Lower Bound',
          data: forecast.points.map((p: ForecastPoint) => p.lowerBound),
          borderColor: 'rgba(255, 99, 132, 0.7)',
          backgroundColor: 'rgba(255, 99, 132, 0.2)',
          borderDash: [5, 5],
          borderWidth: 1,
          pointRadius: 0,
          fill: false,
        } : null,
        forecast.points[0].upperBound !== undefined ? {
          label: 'Upper Bound',
          data: forecast.points.map((p: ForecastPoint) => p.upperBound),
          borderColor: 'rgba(75, 192, 192, 0.7)',
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          borderDash: [5, 5],
          borderWidth: 1,
          pointRadius: 0,
          fill: false,
        } : null,
      ].filter(Boolean)
    };
  };

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: false,
      },
      tooltip: {
        callbacks: {
          label: function(context: any) {
            let label = context.dataset.label || '';
            if (label) {
              label += ': ';
            }
            if (context.parsed.y !== null) {
              label += new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: 'USD',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
              }).format(context.parsed.y);
            }
            return label;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: false,
        ticks: {
          callback: function(value: any) {
            return new Intl.NumberFormat('en-US', {
              style: 'currency',
              currency: 'USD',
              minimumFractionDigits: 0,
              maximumFractionDigits: 0
            }).format(value);
          }
        }
      }
    }
  };

  const getMetricLabel = () => {
    switch (metricName) {
      case 'total_sales':
        return 'Total Sales';
      case 'new_vehicle_sales':
        return 'New Vehicle Sales';
      case 'used_vehicle_sales':
        return 'Used Vehicle Sales';
      case 'service_revenue':
        return 'Service Revenue';
      case 'parts_sales':
        return 'Parts Sales';
      default:
        return metricName;
    }
  };

  return (
    <Card sx={{ width: '100%', mb: 3 }}>
      <CardHeader 
        title={`Sales Forecast: ${getMetricLabel()}`}
        subheader={forecast ? `Confidence Level: ${forecast.confidenceLevel * 100}%` : ''}
        action={
          <Box sx={{ display: 'flex', gap: 2 }}>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel id="metric-select-label">Metric</InputLabel>
              <Select
                labelId="metric-select-label"
                id="metric-select"
                value={metricName}
                onChange={handleMetricChange}
                label="Metric"
              >
                <MenuItem value="total_sales">Total Sales</MenuItem>
                <MenuItem value="new_vehicle_sales">New Vehicle Sales</MenuItem>
                <MenuItem value="used_vehicle_sales">Used Vehicle Sales</MenuItem>
                <MenuItem value="service_revenue">Service Revenue</MenuItem>
                <MenuItem value="parts_sales">Parts Sales</MenuItem>
              </Select>
            </FormControl>
            
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel id="granularity-select-label">Time Granularity</InputLabel>
              <Select
                labelId="granularity-select-label"
                id="granularity-select"
                value={timeGranularity}
                onChange={handleTimeGranularityChange}
                label="Time Granularity"
              >
                <MenuItem value="day">Daily</MenuItem>
                <MenuItem value="week">Weekly</MenuItem>
                <MenuItem value="month">Monthly</MenuItem>
                <MenuItem value="quarter">Quarterly</MenuItem>
              </Select>
            </FormControl>
            
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel id="periods-select-label">Forecast Periods</InputLabel>
              <Select
                labelId="periods-select-label"
                id="periods-select"
                value={periods.toString()}
                onChange={handlePeriodsChange}
                label="Forecast Periods"
              >
                <MenuItem value="3">3</MenuItem>
                <MenuItem value="6">6</MenuItem>
                <MenuItem value="12">12</MenuItem>
                <MenuItem value="24">24</MenuItem>
              </Select>
            </FormControl>
          </Box>
        }
        sx={{
          flexDirection: { xs: 'column', md: 'row' },
          alignItems: { xs: 'flex-start', md: 'center' },
          '& .MuiCardHeader-action': {
            mt: { xs: 2, md: 0 },
            width: { xs: '100%', md: 'auto' }
          }
        }}
      />
      
      <CardContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box sx={{ height: 400 }}>
            <Line data={getChartData()} options={chartOptions} />
          </Box>
        )}
        
        {forecast && !loading && (
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" color="text.secondary">
              * Forecast is based on historical data and market trends. Actual results may vary.
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default SalesForecast;
