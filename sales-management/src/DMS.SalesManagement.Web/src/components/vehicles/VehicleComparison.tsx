// @ts-nocheck
import React, { useState } from 'react';
import {
  Box,
  Chip,
  CircularProgress,
  Divider,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Card,
  CardMedia,
  CardContent,
  Button,
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import CompareArrowsIcon from '@mui/icons-material/CompareArrows';
import NorthEastIcon from '@mui/icons-material/NorthEast';
import SouthEastIcon from '@mui/icons-material/SouthEast';
import { DealVehicle } from '../../types/deal';

interface Vehicle extends DealVehicle {
  features?: string[];
  engineType?: string;
  transmission?: string;
  fuelEconomy?: string;
  warranty?: string;
  safetyRating?: string;
  msrp?: number;
}

interface VehicleComparisonProps {
  selectedVehicle: Vehicle;
  alternativeVehicles?: Vehicle[];
  onVehicleSelect: (vehicle: Vehicle) => void;
  isLoading?: boolean;
}

const VehicleComparison: React.FC<VehicleComparisonProps> = ({
  selectedVehicle,
  alternativeVehicles = [],
  onVehicleSelect,
  isLoading = false,
}) => {
  const [showComparison, setShowComparison] = useState<boolean>(false);
  const [vehiclesToCompare, setVehiclesToCompare] = useState<Vehicle[]>([]);
  
  // Function to toggle vehicle in comparison list
  const toggleVehicleComparison = (vehicle: Vehicle) => {
    if (vehiclesToCompare.some(v => v.id === vehicle.id)) {
      setVehiclesToCompare(vehiclesToCompare.filter(v => v.id !== vehicle.id));
    } else {
      if (vehiclesToCompare.length < 2) { // Limit to comparing 2 alternatives
        setVehiclesToCompare([...vehiclesToCompare, vehicle]);
      }
    }
  };
  
  // Handle toggle comparison view
  const handleToggleComparison = () => {
    if (!showComparison && vehiclesToCompare.length === 0 && alternativeVehicles.length > 0) {
      // Auto-select first alternative for comparison if none selected
      setVehiclesToCompare([alternativeVehicles[0]]);
    }
    setShowComparison(!showComparison);
  };

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress />
      </Box>
    );
  }
  
  if (!selectedVehicle) {
    return (
      <Paper elevation={0} sx={{ p: 2 }}>
        <Typography>No vehicle selected.</Typography>
      </Paper>
    );
  }
  
  // Render individual vehicle card
  const renderVehicleCard = (vehicle: Vehicle, isSelected: boolean, isComparison: boolean) => (
    <Card 
      sx={{ 
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        border: isSelected ? '2px solid' : '1px solid',
        borderColor: isSelected ? 'primary.main' : 'divider'
      }}
    >
      {vehicle.imageUrl && (
        <CardMedia
          component="img"
          height="160"
          image={vehicle.imageUrl}
          alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`}
        />
      )}
      <CardContent>
        <Typography variant="h6">
          {vehicle.year} {vehicle.make} {vehicle.model}
        </Typography>
        <Box display="flex" justifyContent="space-between" alignItems="center" mt={1}>
          <Typography variant="h5" color="primary">
            ${vehicle.listPrice.toLocaleString()}
          </Typography>
          {!isSelected && (
            <Button 
              variant="contained" 
              color="primary" 
              size="small"
              onClick={() => onVehicleSelect(vehicle)}
            >
              Select
            </Button>
          )}
          {isSelected && (
            <Chip 
              color="primary" 
              label="Selected" 
              size="small" 
            />
          )}
        </Box>
        {!isComparison && alternativeVehicles.length > 0 && (
          <Button
            variant="outlined"
            color="primary"
            size="small"
            startIcon={<CompareArrowsIcon />}
            onClick={() => toggleVehicleComparison(vehicle)}
            fullWidth
            sx={{ mt: 2 }}
            disabled={vehiclesToCompare.some(v => v.id === vehicle.id)}
          >
            Add to Comparison
          </Button>
        )}
      </CardContent>
    </Card>
  );
  
  // Render detailed comparison table
  const renderComparisonTable = () => {
    const vehicles = [selectedVehicle, ...vehiclesToCompare];
    
    // Define comparison categories and properties
    const categories = [
      {
        name: 'Price',
        properties: [
          { 
            name: 'List Price', 
            getValue: (v: Vehicle) => `$${v.listPrice.toLocaleString()}`,
            compare: (a: Vehicle, b: Vehicle) => {
              if (a.listPrice === b.listPrice) return null;
              return a.listPrice < b.listPrice ? 'better' : 'worse';
            }
          },
          { 
            name: 'MSRP', 
            getValue: (v: Vehicle) => v.msrp ? `$${v.msrp.toLocaleString()}` : 'N/A',
            compare: (a: Vehicle, b: Vehicle) => {
              if (!a.msrp || !b.msrp || a.msrp === b.msrp) return null;
              return a.msrp < b.msrp ? 'better' : 'worse';
            }
          }
        ]
      },
      {
        name: 'Specifications',
        properties: [
          { 
            name: 'Engine', 
            getValue: (v: Vehicle) => v.engineType || 'N/A',
            compare: () => null // Subjective, no comparison
          },
          { 
            name: 'Transmission', 
            getValue: (v: Vehicle) => v.transmission || 'N/A',
            compare: () => null // Subjective, no comparison
          },
          { 
            name: 'Fuel Economy', 
            getValue: (v: Vehicle) => v.fuelEconomy || 'N/A',
            compare: (a: Vehicle, b: Vehicle) => {
              if (!a.fuelEconomy || !b.fuelEconomy) return null;
              const aValue = parseInt(a.fuelEconomy);
              const bValue = parseInt(b.fuelEconomy);
              if (isNaN(aValue) || isNaN(bValue) || aValue === bValue) return null;
              return aValue > bValue ? 'better' : 'worse';
            }
          }
        ]
      },
      {
        name: 'Additional Information',
        properties: [
          { 
            name: 'Warranty', 
            getValue: (v: Vehicle) => v.warranty || 'N/A',
            compare: () => null // Too complex for simple comparison
          },
          { 
            name: 'Safety Rating', 
            getValue: (v: Vehicle) => v.safetyRating || 'N/A',
            compare: (a: Vehicle, b: Vehicle) => {
              if (!a.safetyRating || !b.safetyRating) return null;
              const aValue = parseInt(a.safetyRating);
              const bValue = parseInt(b.safetyRating);
              if (isNaN(aValue) || isNaN(bValue) || aValue === bValue) return null;
              return aValue > bValue ? 'better' : 'worse';
            }
          }
        ]
      }
    ];
    
    const renderComparisonIndicator = (mainVehicle: Vehicle, compareVehicle: Vehicle, property: any) => {
      const result = property.compare(mainVehicle, compareVehicle);
      if (result === 'better') {
        return <NorthEastIcon fontSize="small" color="success" />;
      } else if (result === 'worse') {
        return <SouthEastIcon fontSize="small" color="error" />;
      }
      return null;
    };
    
    return (
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold', width: '20%' }}>Feature</TableCell>
              {vehicles.map((vehicle) => (
                <TableCell key={vehicle.id} align="center" sx={{ fontWeight: 'bold' }}>
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {categories.map((category) => (
              <React.Fragment key={category.name}>
                <TableRow sx={{ backgroundColor: 'action.hover' }}>
                  <TableCell colSpan={vehicles.length + 1} sx={{ fontWeight: 'bold' }}>
                    {category.name}
                  </TableCell>
                </TableRow>
                {category.properties.map((property) => (
                  <TableRow key={property.name}>
                    <TableCell>{property.name}</TableCell>
                    {vehicles.map((vehicle, index) => (
                      <TableCell key={`${vehicle.id}-${property.name}`} align="center">
                        <Box display="flex" alignItems="center" justifyContent="center">
                          {property.getValue(vehicle)}
                          {index > 0 && renderComparisonIndicator(vehicle, selectedVehicle, property)}
                        </Box>
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </React.Fragment>
            ))}
            <TableRow sx={{ backgroundColor: 'action.hover' }}>
              <TableCell colSpan={vehicles.length + 1} sx={{ fontWeight: 'bold' }}>
                Features
              </TableCell>
            </TableRow>
            {Array.from(
              new Set(
                vehicles.flatMap((vehicle) => vehicle.features || [])
              )
            ).sort().map((feature) => (
              <TableRow key={feature}>
                <TableCell>{feature}</TableCell>
                {vehicles.map((vehicle) => (
                  <TableCell key={`${vehicle.id}-${feature}`} align="center">
                    {(vehicle.features || []).includes(feature) ? (
                      <CheckCircleIcon color="success" fontSize="small" />
                    ) : (
                      <CancelIcon color="error" fontSize="small" />
                    )}
                  </TableCell>
                ))}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    );
  };
  
  return (
    <Box>
      {!showComparison ? (
        <>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">Selected Vehicle</Typography>
            {alternativeVehicles.length > 0 && (
              <Button
                variant="outlined"
                startIcon={<CompareArrowsIcon />}
                onClick={handleToggleComparison}
              >
                Compare Vehicles ({vehiclesToCompare.length})
              </Button>
            )}
          </Box>
          
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              {renderVehicleCard(selectedVehicle, true, false)}
            </Grid>
            
            {alternativeVehicles.length > 0 && (
              <>
                <Grid item xs={12}>
                  <Divider>
                    <Chip label="Alternative Options" />
                  </Divider>
                </Grid>
                
                {alternativeVehicles.map((vehicle) => (
                  <Grid item xs={12} md={4} key={vehicle.id}>
                    {renderVehicleCard(vehicle, false, false)}
                  </Grid>
                ))}
              </>
            )}
          </Grid>
        </>
      ) : (
        <>
          <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
            <Typography variant="h6">Vehicle Comparison</Typography>
            <Button
              variant="outlined"
              onClick={handleToggleComparison}
            >
              Back to Vehicle List
            </Button>
          </Box>
          
          {renderComparisonTable()}
        </>
      )}
    </Box>
  );
};

export default VehicleComparison;
