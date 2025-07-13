import React, { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import CircularProgress from '@mui/material/CircularProgress';
import Alert from '@mui/material/Alert';
import Avatar from '@mui/material/Avatar';
import Typography from '@mui/material/Typography';
import Tooltip from '@mui/material/Tooltip';
import Chip from '@mui/material/Chip';
import { DataGrid } from '@mui/x-data-grid';
import type { GridColDef, GridRenderCellParams, GridValueFormatterParams } from '../../types/datagrid-types';
import PersonIcon from '@mui/icons-material/Person';
import InfoIcon from '@mui/icons-material/Info';
import AnalyticsService from '../../services/AnalyticsService';
import type { CustomerChurnPrediction } from '../../types/analyticsTypes';

const CustomerChurnPredictions: React.FC = () => {
  const [predictions, setPredictions] = useState<CustomerChurnPrediction[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchPredictions = async () => {
      try {
        setLoading(true);
        const analyticsService = new AnalyticsService();
        const data = await analyticsService.getCustomerChurnPredictions(0.3); // Include medium risk customers
        setPredictions(data);
        setError(null);
      } catch (err) {
        setError('Failed to fetch customer churn predictions');
        console.error('Error fetching predictions:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchPredictions();
  }, []);

  const getRiskChip = (riskCategory: string) => {
    switch (riskCategory.toLowerCase()) {
      case 'high':
        return <Chip label="High Risk" color="error" size="small" />;
      case 'medium':
        return <Chip label="Medium Risk" color="warning" size="small" />;
      case 'low':
        return <Chip label="Low Risk" color="success" size="small" />;
      default:
        return <Chip label="Unknown" color="default" size="small" />;
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  };

  const columns: GridColDef[] = [
    {
      field: 'customerName',
      headerName: 'Customer',
      flex: 1,
      minWidth: 200,
      renderCell: (params: GridRenderCellParams<CustomerChurnPrediction>) => (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Avatar sx={{ mr: 2, bgcolor: 'primary.main' }}>
            <PersonIcon />
          </Avatar>
          <Typography variant="body2">{params.value}</Typography>
        </Box>
      )
    },
    {
      field: 'churnRiskScore',
      headerName: 'Risk Score',
      type: 'number',
      width: 120,
      align: 'center',
      headerAlign: 'center',
      renderCell: (params: GridRenderCellParams<CustomerChurnPrediction, number>) => {
        const score = params.value || 0;
        return (
          <Box
            sx={{
              width: 60,
              height: 60,
              position: 'relative',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <CircularProgress
              variant="determinate"
              value={score * 100}
              size={60}
              thickness={4}
              sx={{
                color: score > 0.7 ? 'error.main' : score > 0.4 ? 'warning.main' : 'success.main',
              }}
            />
            <Typography
              variant="caption"
              component="div"
              sx={{
                position: 'absolute',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              {`${(score * 100).toFixed(0)}%`}
            </Typography>
          </Box>
        );
      }
    },
    {
      field: 'riskCategory',
      headerName: 'Risk Level',
      width: 140,
      renderCell: (params: GridRenderCellParams<CustomerChurnPrediction, string>) => (
        getRiskChip(params.value || '')
      )
    },
    {
      field: 'lifetimeValue',
      headerName: 'Lifetime Value',
      type: 'number',
      width: 140,
      align: 'right',
      headerAlign: 'right',
      valueFormatter: (params: GridValueFormatterParams) => formatCurrency(params.value as number)
    },
    {
      field: 'daysSinceLastPurchase',
      headerName: 'Days Since Purchase',
      type: 'number',
      width: 160,
      align: 'right',
      headerAlign: 'right',
      renderHeader: () => (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Typography variant="body2">Days Since Purchase</Typography>
          <Tooltip title="Number of days since customer's last purchase">
            <InfoIcon fontSize="small" sx={{ ml: 0.5 }} />
          </Tooltip>
        </Box>
      )
    },
    {
      field: 'churnFactors',
      headerName: 'Churn Factors',
      width: 250,
      renderCell: (params: GridRenderCellParams<CustomerChurnPrediction, string[]>) => {
        const factors = params.value || [];
        return (
          <Box>
            {factors.map((factor: string, idx: number) => (
              <Chip key={idx} label={factor} size="small" sx={{ mr: 0.5, mb: 0.5 }} />
            ))}
          </Box>
        );
      }
    }
  ];

  const rows = predictions.map((pred: CustomerChurnPrediction) => ({
    id: pred.customerId,
    ...pred,
  }));

  return (
    <Card sx={{ width: '100%', mb: 3 }}>
      <CardHeader 
        title="Customer Churn Risk Predictions"
      />
      
      <CardContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Box sx={{ height: 500, width: '100%' }}>
            <DataGrid
              rows={rows}
              columns={columns}
              initialState={{
                pagination: {
                  paginationModel: { pageSize: 10 }
                },
              }}
              pageSizeOptions={[10, 25, 50]}
              disableRowSelectionOnClick
            />
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default CustomerChurnPredictions;
