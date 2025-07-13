import React, { useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Button,
  Grid,
  Chip,
  Avatar,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Fab,
} from '@mui/material';
import {
  DirectionsCar,
  Add,
  Edit,
  Delete,
  Search,
  FilterList,
  PhotoCamera,
} from '@mui/icons-material';

// Sample inventory data
const sampleVehicles = [
  {
    id: 1,
    vin: '1HGBH41JXMN109186',
    make: 'Honda',
    model: 'Accord',
    year: 2023,
    trim: 'EX-L',
    color: 'Pearl White',
    mileage: 15000,
    status: 'Available',
    price: 28500,
    daysInInventory: 45,
    location: 'Lot A-15',
    condition: 'Excellent',
  },
  {
    id: 2,
    vin: '2T1BURHE0JC123456',
    make: 'Toyota',
    model: 'Camry',
    year: 2024,
    trim: 'SE',
    color: 'Midnight Black',
    mileage: 8500,
    status: 'Sold',
    price: 31200,
    daysInInventory: 12,
    location: 'Lot B-03',
    condition: 'Like New',
  },
  {
    id: 3,
    vin: '1FTFW1ET5DFC12345',
    make: 'Ford',
    model: 'F-150',
    year: 2022,
    trim: 'XLT',
    color: 'Magnetic Gray',
    mileage: 32000,
    status: 'Reserved',
    price: 42500,
    daysInInventory: 78,
    location: 'Lot C-08',
    condition: 'Good',
  },
];

const getStatusColor = (status) => {
  switch (status) {
    case 'Available':
      return 'success';
    case 'Sold':
      return 'default';
    case 'Reserved':
      return 'warning';
    case 'Service':
      return 'info';
    default:
      return 'default';
  }
};

export default function Inventory() {
  const [vehicles, setVehicles] = useState(sampleVehicles);
  const [searchTerm, setSearchTerm] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedVehicle, setSelectedVehicle] = useState(null);
  const [newVehicle, setNewVehicle] = useState({
    vin: '',
    make: '',
    model: '',
    year: new Date().getFullYear(),
    trim: '',
    color: '',
    mileage: 0,
    price: 0,
    status: 'Available',
    location: '',
    condition: 'Excellent',
  });

  const filteredVehicles = vehicles.filter(vehicle =>
    vehicle.vin.toLowerCase().includes(searchTerm.toLowerCase()) ||
    vehicle.make.toLowerCase().includes(searchTerm.toLowerCase()) ||
    vehicle.model.toLowerCase().includes(searchTerm.toLowerCase()) ||
    vehicle.color.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleAddVehicle = () => {
    const vehicle = {
      ...newVehicle,
      id: vehicles.length + 1,
      daysInInventory: 0,
    };
    setVehicles([...vehicles, vehicle]);
    setOpenDialog(false);
    setNewVehicle({
      vin: '',
      make: '',
      model: '',
      year: new Date().getFullYear(),
      trim: '',
      color: '',
      mileage: 0,
      price: 0,
      status: 'Available',
      location: '',
      condition: 'Excellent',
    });
  };

  const handleEditVehicle = (vehicle) => {
    setSelectedVehicle(vehicle);
    setNewVehicle(vehicle);
    setOpenDialog(true);
  };

  const handleUpdateVehicle = () => {
    setVehicles(vehicles.map(v => v.id === selectedVehicle.id ? newVehicle : v));
    setOpenDialog(false);
    setSelectedVehicle(null);
  };

  const handleDeleteVehicle = (id) => {
    setVehicles(vehicles.filter(v => v.id !== id));
  };

  const inventoryStats = {
    total: vehicles.length,
    available: vehicles.filter(v => v.status === 'Available').length,
    sold: vehicles.filter(v => v.status === 'Sold').length,
    avgDays: Math.round(vehicles.reduce((sum, v) => sum + v.daysInInventory, 0) / vehicles.length),
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight="bold">
          Vehicle Inventory
        </Typography>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={() => setOpenDialog(true)}
        >
          Add Vehicle
        </Button>
      </Box>

      {/* Inventory Stats */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom variant="overline">
                    Total Vehicles
                  </Typography>
                  <Typography variant="h4" fontWeight="bold">
                    {inventoryStats.total}
                  </Typography>
                </Box>
                <Avatar sx={{ bgcolor: 'primary.main' }}>
                  <DirectionsCar />
                </Avatar>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom variant="overline">
                    Available
                  </Typography>
                  <Typography variant="h4" fontWeight="bold" color="success.main">
                    {inventoryStats.available}
                  </Typography>
                </Box>
                <Avatar sx={{ bgcolor: 'success.main' }}>
                  <DirectionsCar />
                </Avatar>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom variant="overline">
                    Sold This Month
                  </Typography>
                  <Typography variant="h4" fontWeight="bold" color="info.main">
                    {inventoryStats.sold}
                  </Typography>
                </Box>
                <Avatar sx={{ bgcolor: 'info.main' }}>
                  <DirectionsCar />
                </Avatar>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <Box>
                  <Typography color="text.secondary" gutterBottom variant="overline">
                    Avg Days in Inventory
                  </Typography>
                  <Typography variant="h4" fontWeight="bold" color="warning.main">
                    {inventoryStats.avgDays}
                  </Typography>
                </Box>
                <Avatar sx={{ bgcolor: 'warning.main' }}>
                  <DirectionsCar />
                </Avatar>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Search and Filters */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            <TextField
              placeholder="Search by VIN, make, model, or color..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              InputProps={{
                startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} />,
              }}
              sx={{ flexGrow: 1 }}
            />
            <Button variant="outlined" startIcon={<FilterList />}>
              Filters
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Vehicle Table */}
      <Card>
        <CardContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>VIN</TableCell>
                  <TableCell>Vehicle</TableCell>
                  <TableCell>Year</TableCell>
                  <TableCell>Color</TableCell>
                  <TableCell>Mileage</TableCell>
                  <TableCell>Price</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Days in Inventory</TableCell>
                  <TableCell>Location</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredVehicles.map((vehicle) => (
                  <TableRow key={vehicle.id} hover>
                    <TableCell>
                      <Typography variant="body2" fontFamily="monospace">
                        {vehicle.vin}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Box>
                        <Typography variant="subtitle2">
                          {vehicle.make} {vehicle.model}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {vehicle.trim}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>{vehicle.year}</TableCell>
                    <TableCell>{vehicle.color}</TableCell>
                    <TableCell>{vehicle.mileage.toLocaleString()} mi</TableCell>
                    <TableCell>${vehicle.price.toLocaleString()}</TableCell>
                    <TableCell>
                      <Chip
                        label={vehicle.status}
                        color={getStatusColor(vehicle.status)}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography
                        variant="body2"
                        color={vehicle.daysInInventory > 60 ? 'error.main' : 'text.primary'}
                      >
                        {vehicle.daysInInventory} days
                      </Typography>
                    </TableCell>
                    <TableCell>{vehicle.location}</TableCell>
                    <TableCell>
                      <IconButton
                        size="small"
                        onClick={() => handleEditVehicle(vehicle)}
                      >
                        <Edit />
                      </IconButton>
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => handleDeleteVehicle(vehicle.id)}
                      >
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Add/Edit Vehicle Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {selectedVehicle ? 'Edit Vehicle' : 'Add New Vehicle'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="VIN"
                value={newVehicle.vin}
                onChange={(e) => setNewVehicle({ ...newVehicle, vin: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Make"
                value={newVehicle.make}
                onChange={(e) => setNewVehicle({ ...newVehicle, make: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Model"
                value={newVehicle.model}
                onChange={(e) => setNewVehicle({ ...newVehicle, model: e.target.value })}
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Year"
                type="number"
                value={newVehicle.year}
                onChange={(e) => setNewVehicle({ ...newVehicle, year: parseInt(e.target.value) })}
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Trim"
                value={newVehicle.trim}
                onChange={(e) => setNewVehicle({ ...newVehicle, trim: e.target.value })}
              />
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Color"
                value={newVehicle.color}
                onChange={(e) => setNewVehicle({ ...newVehicle, color: e.target.value })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Mileage"
                type="number"
                value={newVehicle.mileage}
                onChange={(e) => setNewVehicle({ ...newVehicle, mileage: parseInt(e.target.value) })}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Price"
                type="number"
                value={newVehicle.price}
                onChange={(e) => setNewVehicle({ ...newVehicle, price: parseInt(e.target.value) })}
              />
            </Grid>
            <Grid item xs={4}>
              <FormControl fullWidth>
                <InputLabel>Status</InputLabel>
                <Select
                  value={newVehicle.status}
                  onChange={(e) => setNewVehicle({ ...newVehicle, status: e.target.value })}
                >
                  <MenuItem value="Available">Available</MenuItem>
                  <MenuItem value="Sold">Sold</MenuItem>
                  <MenuItem value="Reserved">Reserved</MenuItem>
                  <MenuItem value="Service">In Service</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={4}>
              <TextField
                fullWidth
                label="Location"
                value={newVehicle.location}
                onChange={(e) => setNewVehicle({ ...newVehicle, location: e.target.value })}
              />
            </Grid>
            <Grid item xs={4}>
              <FormControl fullWidth>
                <InputLabel>Condition</InputLabel>
                <Select
                  value={newVehicle.condition}
                  onChange={(e) => setNewVehicle({ ...newVehicle, condition: e.target.value })}
                >
                  <MenuItem value="Excellent">Excellent</MenuItem>
                  <MenuItem value="Very Good">Very Good</MenuItem>
                  <MenuItem value="Good">Good</MenuItem>
                  <MenuItem value="Fair">Fair</MenuItem>
                  <MenuItem value="Poor">Poor</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={selectedVehicle ? handleUpdateVehicle : handleAddVehicle}
          >
            {selectedVehicle ? 'Update' : 'Add'} Vehicle
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
