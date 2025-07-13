const createService = require('../service-template');

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
    status: 'Available',
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
    status: 'Available',
    price: 42500,
    daysInInventory: 78,
    location: 'Lot C-08',
    condition: 'Good',
  },
];

const routes = {
  '/vehicles': (req, res) => {
    res.json({ vehicles: sampleVehicles });
  },
  
  '/vehicles/:id': (req, res) => {
    const vehicle = sampleVehicles.find(v => v.id === parseInt(req.params.id));
    if (vehicle) {
      res.json(vehicle);
    } else {
      res.status(404).json({ error: 'Vehicle not found' });
    }
  },
  
  '/search': (req, res) => {
    const { q } = req.query;
    const results = sampleVehicles
      .filter(vehicle =>
        vehicle.vin.toLowerCase().includes(q.toLowerCase()) ||
        vehicle.make.toLowerCase().includes(q.toLowerCase()) ||
        vehicle.model.toLowerCase().includes(q.toLowerCase()) ||
        vehicle.color.toLowerCase().includes(q.toLowerCase())
      )
      .map(vehicle => ({
        type: 'vehicle',
        id: vehicle.id,
        title: `${vehicle.year} ${vehicle.make} ${vehicle.model}`,
        subtitle: `VIN: ${vehicle.vin} - $${vehicle.price.toLocaleString()}`
      }));

    res.json({ results });
  }
};

createService('Inventory Service', 8082, routes);
