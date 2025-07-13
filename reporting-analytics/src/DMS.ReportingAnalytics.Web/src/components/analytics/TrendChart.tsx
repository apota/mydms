import React, { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import CircularProgress from '@mui/material/CircularProgress';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Typography from '@mui/material/Typography';
import Alert from '@mui/material/Alert';
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
import type { TrendResult } from '../../types/analyticsTypes';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface TrendChartProps {
  metricId: string;
  title?: string;
  height?: number;
}

const TrendChart = ({ metricId, title, height = 400 }: TrendChartProps) => {
  const [trend, setTrend] = useState<TrendResult | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [timeFrame, setTimeFrame] = useState<string>('month');
  const [compareWith, setCompareWith] = useState<string>('');

  useEffect(() => {
    const fetchTrend = async () => {
      try {
        setLoading(true);
        const analyticsService = new AnalyticsService();
        const data = await analyticsService.getTrend(metricId, timeFrame, compareWith);
        setTrend(data);
        setError(null);
      } catch (err) {
        setError('Failed to fetch trend data');
        console.error('Error fetching trend:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTrend();
  }, [metricId, timeFrame, compareWith]);

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    const formatter = new Intl.DateTimeFormat('en-US', { 
      month: 'short', 
      day: 'numeric',
      ...(timeFrame === 'year' ? { year: 'numeric' } : {})
    });
    return formatter.format(date);
  };

  const getChartData = () => {
    if (!trend) return {
      labels: [],
      datasets: []
    };

    return {
      labels: trend.points.map(p => formatDate(p.date)),
      datasets: [
        {
          label: trend.metricName,
          data: trend.points.map(p => p.value),
          borderColor: 'rgb(53, 162, 235)',
          backgroundColor: 'rgba(53, 162, 235, 0.5)',
          tension: 0.4,
          fill: true,
        },
        ...(trend.comparisonPoints ? [{
          label: 'Comparison',
          data: trend.comparisonPoints.map(p => p.value),
          borderColor: 'rgb(255, 99, 132)',
          backgroundColor: 'rgba(255, 99, 132, 0.5)',
          borderDash: [5, 5],
          tension: 0.4,
        }] : [])
      ]
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
              label += new Intl.NumberFormat('en-US').format(context.parsed.y);
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
            return value.toLocaleString();
          }
        }
      }
    }
  };

  return (
    <Card sx={{ width: '100%', mb: 3 }}>
      <CardHeader 
        title={title || `${trend?.metricName || metricId} Trend`}
        action={
          <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 2 }}>
            <ToggleButtonGroup
              value={timeFrame}
              exclusive
              onChange={(e, newValue) => newValue && setTimeFrame(newValue)}
              size="small"
            >
              <ToggleButton value="day">Daily</ToggleButton>
              <ToggleButton value="week">Weekly</ToggleButton>
              <ToggleButton value="month">Monthly</ToggleButton>
              <ToggleButton value="quarter">Quarterly</ToggleButton>
              <ToggleButton value="year">Yearly</ToggleButton>
            </ToggleButtonGroup>
            
            <ToggleButtonGroup
              value={compareWith}
              exclusive
              onChange={(e, newValue) => setCompareWith(newValue)}
              size="small"
            >
              <ToggleButton value="">No Comparison</ToggleButton>
              <ToggleButton value="previous-period">Previous Period</ToggleButton>
              <ToggleButton value="previous-year">Previous Year</ToggleButton>
            </ToggleButtonGroup>
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
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box sx={{ height }}>
            <Line data={getChartData()} options={chartOptions} />
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default TrendChart;
