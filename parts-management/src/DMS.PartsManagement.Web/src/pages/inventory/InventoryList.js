import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Form, InputGroup, Badge, Spinner } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { FaSearch, FaFilter, FaExclamationTriangle } from 'react-icons/fa';
import api from '../../services/api';

const InventoryList = () => {
  const [inventory, setInventory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [showLowStockOnly, setShowLowStockOnly] = useState(false);
  const [locations, setLocations] = useState([]);
  const [selectedLocation, setSelectedLocation] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchInventory = async () => {
      try {
        setLoading(true);
        let inventoryData;
        
        if (showLowStockOnly) {
          inventoryData = await api.inventory.getLowStockInventory();
        } else if (selectedLocation) {
          inventoryData = await api.inventory.getInventoryByLocation(selectedLocation);
        } else {
          inventoryData = await api.inventory.getAllInventory();
        }
        
        setInventory(inventoryData);
        setError(null);
      } catch (err) {
        console.error('Error fetching inventory:', err);
        setError('Failed to load inventory data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    const fetchLocations = async () => {
      try {
        // In a real application, you would fetch locations from an API endpoint
        // For now, we'll use placeholder data
        setLocations([
          { id: 1, name: 'Main Warehouse' },
          { id: 2, name: 'Service Center' },
          { id: 3, name: 'Distribution Center' }
        ]);
      } catch (err) {
        console.error('Error fetching locations:', err);
      }
    };

    fetchInventory();
    fetchLocations();
  }, [showLowStockOnly, selectedLocation]);

  const filteredInventory = inventory.filter(item => {
    return (
      item.partName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.partNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.locationName.toLowerCase().includes(searchTerm.toLowerCase())
    );
  });

  const handleLocationChange = (e) => {
    setSelectedLocation(e.target.value);
  };

  const handleLowStockToggle = () => {
    setShowLowStockOnly(!showLowStockOnly);
  };

  const handleViewDetails = (partId, locationId) => {
    navigate(`/inventory/${partId}/location/${locationId}`);
  };

  const getStockStatusBadge = (quantity, minStockLevel) => {
    if (quantity <= 0) {
      return <Badge bg="danger">Out of Stock</Badge>;
    } else if (quantity <= minStockLevel) {
      return <Badge bg="warning" text="dark">Low Stock</Badge>;
    } else {
      return <Badge bg="success">In Stock</Badge>;
    }
  };

  if (loading) {
    return (
      <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </Container>
    );
  }

  return (
    <Container fluid className="mt-4">
      <Row>
        <Col>
          <h2 className="mb-4">Inventory Management</h2>
        </Col>
      </Row>
      <Row className="mb-4">
        <Col md={6}>
          <InputGroup>
            <InputGroup.Text>
              <FaSearch />
            </InputGroup.Text>
            <Form.Control
              placeholder="Search by part name, number, or location..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </InputGroup>
        </Col>
        <Col md={3}>
          <Form.Select 
            value={selectedLocation}
            onChange={handleLocationChange}
          >
            <option value="">All Locations</option>
            {locations.map(location => (
              <option key={location.id} value={location.id}>
                {location.name}
              </option>
            ))}
          </Form.Select>
        </Col>
        <Col md={3}>
          <Form.Check 
            type="switch"
            id="low-stock-switch"
            label="Show Low Stock Only"
            checked={showLowStockOnly}
            onChange={handleLowStockToggle}
            className="d-flex align-items-center"
          />
        </Col>
      </Row>

      <Row>
        <Col md={12}>
          <Card>
            <Card.Body>
              {error ? (
                <div className="text-danger mb-3">{error}</div>
              ) : (
                <Table responsive hover>
                  <thead>
                    <tr>
                      <th>Part Number</th>
                      <th>Part Name</th>
                      <th>Location</th>
                      <th>Quantity</th>
                      <th>Status</th>
                      <th>Min Stock</th>
                      <th>Last Updated</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredInventory.length === 0 ? (
                      <tr>
                        <td colSpan="8" className="text-center">No inventory records found.</td>
                      </tr>
                    ) : (
                      filteredInventory.map((item) => (
                        <tr key={`${item.partId}-${item.locationId}`}>
                          <td>{item.partNumber}</td>
                          <td>{item.partName}</td>
                          <td>{item.locationName}</td>
                          <td>{item.quantity}</td>
                          <td>{getStockStatusBadge(item.quantity, item.minStockLevel)}</td>
                          <td>{item.minStockLevel}</td>
                          <td>{new Date(item.lastUpdated).toLocaleDateString()}</td>
                          <td>
                            <Button 
                              variant="outline-primary" 
                              size="sm"
                              onClick={() => handleViewDetails(item.partId, item.locationId)}
                            >
                              View
                            </Button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Row className="mt-4 mb-5">
        <Col className="d-flex justify-content-end">
          <Link to="/inventory/adjust">
            <Button variant="primary" className="me-2">
              Adjust Inventory
            </Button>
          </Link>
          <Link to="/inventory/transfer">
            <Button variant="secondary">
              Transfer Inventory
            </Button>
          </Link>
        </Col>
      </Row>
    </Container>
  );
};

export default InventoryList;
