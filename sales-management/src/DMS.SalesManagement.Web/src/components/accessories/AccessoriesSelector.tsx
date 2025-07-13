// @ts-nocheck
import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  CardMedia,
  Checkbox,
  CircularProgress,
  Divider,
  FormControlLabel,
  Grid,
  Paper,
  Typography,
  Tooltip,
} from '@mui/material';
import { AccessoryDto } from '../../types/integration';
import { getVehicleAccessories } from '../../services/integrationService';

interface AccessoriesSelectorProps {
  vehicleId: string;
  selectedAccessories: string[];
  onChange: (selectedAccessoryIds: string[]) => void;
}

const AccessoriesSelector: React.FC<AccessoriesSelectorProps> = ({
  vehicleId,
  selectedAccessories,
  onChange,
}) => {
  const [accessories, setAccessories] = useState<AccessoryDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  
  useEffect(() => {
    const fetchAccessories = async () => {
      try {
        setLoading(true);
        const accessoriesData = await getVehicleAccessories(vehicleId);
        setAccessories(accessoriesData);
        setError(null);
      } catch (err) {
        setError('Failed to load vehicle accessories. Please try again.');
        console.error('Error loading accessories:', err);
      } finally {
        setLoading(false);
      }
    };
    
    if (vehicleId) {
      fetchAccessories();
    }
  }, [vehicleId]);
  
  const handleAccessoryToggle = (accessoryId: string) => {
    const newSelectedAccessories = selectedAccessories.includes(accessoryId)
      ? selectedAccessories.filter(id => id !== accessoryId)
      : [...selectedAccessories, accessoryId];
    
    onChange(newSelectedAccessories);
  };
  
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress />
      </Box>
    );
  }
  
  if (error) {
    return (
      <Paper elevation={0} sx={{ p: 2, bgcolor: 'error.light', color: 'error.contrastText' }}>
        <Typography>{error}</Typography>
      </Paper>
    );
  }
  
  if (accessories.length === 0) {
    return (
      <Paper elevation={0} sx={{ p: 2 }}>
        <Typography>No accessories available for this vehicle.</Typography>
      </Paper>
    );
  }
  
  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Available Accessories
      </Typography>
      <Grid container spacing={2}>
        {accessories.map((accessory) => (
          <Grid item xs={12} sm={6} md={4} key={accessory.id}>
            <Card 
              sx={{ 
                display: 'flex', 
                flexDirection: 'column',
                height: '100%', 
                border: selectedAccessories.includes(accessory.id) 
                  ? '2px solid' 
                  : '1px solid',
                borderColor: selectedAccessories.includes(accessory.id) 
                  ? 'primary.main' 
                  : 'divider'
              }}
            >
              {accessory.imageUrl && (
                <CardMedia
                  component="img"
                  height="140"
                  image={accessory.imageUrl}
                  alt={accessory.name}
                />
              )}
              <CardContent sx={{ flexGrow: 1 }}>
                <Typography variant="subtitle1" gutterBottom>
                  {accessory.name}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  {accessory.description}
                </Typography>
                <Divider sx={{ my: 1 }} />
                <Box display="flex" justifyContent="space-between" alignItems="center">
                  <Typography variant="h6" color="primary">
                    ${accessory.price.toLocaleString()}
                  </Typography>
                  <Tooltip title={`Installation time: ${accessory.installationTimeMinutes} minutes`}>
                    <Typography variant="caption" color="text.secondary">
                      {accessory.category}
                    </Typography>
                  </Tooltip>
                </Box>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={selectedAccessories.includes(accessory.id)}
                      onChange={() => handleAccessoryToggle(accessory.id)}
                      color="primary"
                    />
                  }
                  label="Add to Deal"
                />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};

export default AccessoriesSelector;
