import React, { useState, useEffect } from 'react';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import CircularProgress from '@mui/material/CircularProgress';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import { DataGrid } from '@mui/x-data-grid';
import InfoIcon from '@mui/icons-material/Info';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import RemoveIcon from '@mui/icons-material/Remove';
import FileDownloadIcon from '@mui/icons-material/FileDownload';
import AnalyticsService from '../../services/AnalyticsService';
import type { InventoryRecommendation } from '../../types/analyticsTypes';
import type { GridColDef, GridRenderCellParams, GridValueFormatterParams } from '../../types/datagrid-types';

const InventoryRecommendations: React.FC = () => {
  const [recommendations, setRecommendations] = useState<InventoryRecommendation[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchRecommendations = async () => {
      try {
        setLoading(true);
        const analyticsService = new AnalyticsService();
        const data = await analyticsService.getInventoryRecommendations();
        setRecommendations(data);
        setError(null);
      } catch (err) {
        setError('Failed to fetch inventory recommendations');
        console.error('Error fetching recommendations:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchRecommendations();
  }, []);

  const getActionIcon = (action: string) => {
    switch (action.toLowerCase()) {
      case 'increase':
        return <ArrowUpwardIcon sx={{ color: 'success.main' }} />;
      case 'decrease':
        return <ArrowDownwardIcon sx={{ color: 'error.main' }} />;
      case 'maintain':
      default:
        return <RemoveIcon sx={{ color: 'info.main' }} />;
    }
  };

  const getActionChip = (action: string) => {
    switch (action.toLowerCase()) {
      case 'increase':
        return <Chip label="Increase Inventory" color="success" size="small" />;
      case 'decrease':
        return <Chip label="Decrease Inventory" color="error" size="small" />;
      case 'maintain':
      default:
        return <Chip label="Maintain Inventory" color="primary" size="small" />;
    }
  };

  const exportToCsv = () => {
    if (!recommendations.length) return;

    // Create CSV content
    const headers = ['Make', 'Model', 'Year', 'Current Stock', 'Recommended Stock', 'Stock Delta', 'Action', 'Sales Velocity', 'Days Supply'];
    
    const csvRows = [
      headers.join(','),
      ...recommendations.map((rec: InventoryRecommendation) => [
        rec.make,
        rec.model,
        rec.year,
        rec.currentStock,
        rec.recommendedStock,
        rec.stockDelta,
        rec.action,
        rec.salesVelocity.toFixed(2),
        rec.daysSupply
      ].join(','))
    ];
    
    const csvContent = csvRows.join('\n');
    
    // Create download link
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    
    link.setAttribute('href', url);
    link.setAttribute('download', 'inventory_recommendations.csv');
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const columns: GridColDef[] = [
    {
      field: 'make',
      headerName: 'Make',
      flex: 1,
      minWidth: 100
    },
    {
      field: 'model',
      headerName: 'Model',
      flex: 1,
      minWidth: 120
    },
    {
      field: 'year',
      headerName: 'Year',
      type: 'number',
      width: 100
    },
    {
      field: 'currentStock',
      headerName: 'Current Stock',
      type: 'number',
      width: 120,
      align: 'right',
      headerAlign: 'right'
    },
    {
      field: 'recommendedStock',
      headerName: 'Recommended',
      type: 'number',
      width: 120,
      align: 'right',
      headerAlign: 'right'
    },
    {
      field: 'stockDelta',
      headerName: 'Delta',
      type: 'number',
      width: 100,
      align: 'right',
      headerAlign: 'right',
      renderCell: (params: GridRenderCellParams<InventoryRecommendation, number>) => {
        const delta = params.value || 0;
        return (
          <Typography
            variant="body2"
            sx={{
              color: delta > 0 ? 'success.main' : (delta < 0 ? 'error.main' : 'info.main')
            }}
          >
            {delta > 0 ? '+' : ''}{delta}
          </Typography>
        );
      }
    },
    {
      field: 'action',
      headerName: 'Action',
      width: 160,
      renderCell: (params: GridRenderCellParams<InventoryRecommendation, string>) => {
        return getActionChip(params.value || '');
      }
    },
    {
      field: 'salesVelocity',
      headerName: 'Sales Velocity',
      type: 'number',
      width: 120,
      align: 'right',
      headerAlign: 'right',
      renderHeader: () => (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Typography variant="body2">Sales Velocity</Typography>
          <Tooltip title="Average units sold per day">
            <InfoIcon fontSize="small" sx={{ ml: 0.5 }} />
          </Tooltip>
        </Box>
      ),
      valueFormatter: (params: GridValueFormatterParams) => {
        return (params.value as number).toFixed(2);
      }
    },
    {
      field: 'daysSupply',
      headerName: 'Days Supply',
      type: 'number',
      width: 120,
      align: 'right',
      headerAlign: 'right',
      renderHeader: () => (
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Typography variant="body2">Days Supply</Typography>
          <Tooltip title="Estimated days until stock depletes at current sales rate">
            <InfoIcon fontSize="small" sx={{ ml: 0.5 }} />
          </Tooltip>
        </Box>
      ),
      renderCell: (params: GridRenderCellParams<InventoryRecommendation, number>) => {
        const days = params.value || 0;
        let color = 'success.main';
        if (days < 15) {
          color = 'error.main';
        } else if (days < 30) {
          color = 'warning.main';
        } else if (days > 90) {
          color = 'info.main';
        }
        return <Typography variant="body2" sx={{ color }}>{days}</Typography>;
      }
    }
  ];

  const rows = recommendations.map((rec: InventoryRecommendation) => ({
    id: `${rec.make}-${rec.model}-${rec.year}`,
    ...rec
  }));
  
  const calculateSummary = () => {
    if (!recommendations.length) return null;
    
    const totalCurrent = recommendations.reduce((sum: number, item: InventoryRecommendation) => sum + item.currentStock, 0);
    const totalRecommended = recommendations.reduce((sum: number, item: InventoryRecommendation) => sum + item.recommendedStock, 0);
    const totalDelta = totalRecommended - totalCurrent;
    
    return (
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 2, borderTop: 1, borderColor: 'divider' }}>
        <Typography variant="subtitle1" sx={{ mr: 2 }}>
          Total: {totalCurrent} current / {totalRecommended} recommended / 
          <Box component="span" sx={{ 
            color: totalDelta > 0 ? 'success.main' : (totalDelta < 0 ? 'error.main' : 'info.main'),
            ml: 1
          }}>
            {totalDelta > 0 ? '+' : ''}{totalDelta} delta
          </Box>
        </Typography>
      </Box>
    );
  };
  
  return (
    <Card sx={{ width: '100%', mb: 3 }}>
      <CardHeader 
        title="Inventory Optimization Recommendations"
        action={
          <Button
            startIcon={<FileDownloadIcon />}
            onClick={exportToCsv}
            disabled={loading || recommendations.length === 0}
            variant="outlined"
          >
            Export to CSV
          </Button>
        }
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
              checkboxSelection={false}
              disableRowSelectionOnClick
              components={{
                Footer: calculateSummary,
              }}
              sx={{
                '& .MuiDataGrid-cell:focus': {
                  outline: 'none',
                },
                '& .MuiDataGrid-columnHeader:focus': {
                  outline: 'none',
                },
              }}
            />
          </Box>
        )}
      </CardContent>
    </Card>
  );
};

export default InventoryRecommendations;
