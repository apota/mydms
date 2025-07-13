import React, { useState, useEffect } from 'react';
import { Button, Card, Container, Row, Col, Table, Badge, Form, Modal, Spinner, Alert, Tabs, Tab } from 'react-bootstrap';
import { inventoryService } from '../../services/inventoryService';
import './MarketplaceManager.css';

/**
 * Marketplace Manager component for handling vehicle listings across multiple marketplaces
 */
const MarketplaceManager = () => {
  const [marketplaces, setMarketplaces] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [selectedVehicle, setSelectedVehicle] = useState(null);
  const [selectedMarketplaces, setSelectedMarketplaces] = useState([]);
  const [vehicleListings, setVehicleListings] = useState({});
  const [vehicleStats, setVehicleStats] = useState({});
  const [isLoading, setIsLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [showDetails, setShowDetails] = useState(false);
  const [showCredentialModal, setShowCredentialModal] = useState(false);
  const [credentialMarketplace, setCredentialMarketplace] = useState(null);
  const [credentials, setCredentials] = useState({
    apiKey: '',
    apiSecret: '',
    username: '',
    password: '',
    dealerId: ''
  });
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  
  // Load marketplaces and vehicles on component mount
  useEffect(() => {
    const loadData = async () => {
      try {
        setIsLoading(true);
        const [marketplacesResponse, vehiclesResponse] = await Promise.all([
          inventoryService.getAvailableMarketplaces(),
          inventoryService.getListableVehicles()
        ]);
        
        setMarketplaces(marketplacesResponse);
        setVehicles(vehiclesResponse);
        
        if (vehiclesResponse.length > 0) {
          setSelectedVehicle(vehiclesResponse[0]);
          loadVehicleListings(vehiclesResponse[0].id);
        }
      } catch (error) {
        setError('Failed to load marketplace data: ' + error.message);
      } finally {
        setIsLoading(false);
      }
    };
    
    loadData();
  }, []);
  
  // Load vehicle listing information when a vehicle is selected
  const loadVehicleListings = async (vehicleId) => {
    try {
      setActionLoading(true);
      
      const [listingStatus, listingStats] = await Promise.all([
        inventoryService.getVehicleListingStatus(vehicleId),
        inventoryService.getVehicleListingStats(vehicleId)
      ]);
      
      setVehicleListings(listingStatus);
      setVehicleStats(listingStats);
      
      // Pre-select marketplaces where the vehicle is already listed
      const preselected = Object.keys(listingStatus).filter(id => 
        listingStatus[id].state === 'Active' || 
        listingStatus[id].state === 'Featured' || 
        listingStatus[id].state === 'Pending'
      );
      
      setSelectedMarketplaces(preselected);
    } catch (error) {
      setError('Failed to load vehicle listing data: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };
  
  // Handle vehicle selection
  const handleVehicleSelect = (vehicle) => {
    setSelectedVehicle(vehicle);
    loadVehicleListings(vehicle.id);
  };
  
  // Toggle marketplace selection
  const toggleMarketplace = (id) => {
    if (selectedMarketplaces.includes(id)) {
      setSelectedMarketplaces(selectedMarketplaces.filter(m => m !== id));
    } else {
      setSelectedMarketplaces([...selectedMarketplaces, id]);
    }
  };
  
  // List vehicle on selected marketplaces
  const listVehicle = async () => {
    if (!selectedVehicle || selectedMarketplaces.length === 0) {
      setError('Please select a vehicle and at least one marketplace');
      return;
    }
    
    try {
      setActionLoading(true);
      setError('');
      setSuccessMessage('');
      
      const result = await inventoryService.listVehicleOnMarketplaces(
        selectedVehicle.id, 
        selectedMarketplaces
      );
      
      // Check for any failures
      const failures = Object.entries(result)
        .filter(([_, r]) => !r.success)
        .map(([id, r]) => `${getMarketplaceName(id)}: ${r.errorMessage}`);
      
      if (failures.length > 0) {
        setError(`Some marketplaces failed: ${failures.join(', ')}`);
      }
      
      if (failures.length < selectedMarketplaces.length) {
        setSuccessMessage('Vehicle successfully listed on selected marketplaces');
      }
      
      // Refresh listings
      await loadVehicleListings(selectedVehicle.id);
    } catch (error) {
      setError('Failed to list vehicle: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };
  
  // Update vehicle listings on selected marketplaces
  const updateListings = async () => {
    if (!selectedVehicle || selectedMarketplaces.length === 0) {
      setError('Please select a vehicle and at least one marketplace');
      return;
    }
    
    try {
      setActionLoading(true);
      setError('');
      setSuccessMessage('');
      
      const result = await inventoryService.updateVehicleOnMarketplaces(
        selectedVehicle.id, 
        selectedMarketplaces
      );
      
      // Check for any failures
      const failures = Object.entries(result)
        .filter(([_, r]) => !r.success)
        .map(([id, r]) => `${getMarketplaceName(id)}: ${r.errorMessage}`);
      
      if (failures.length > 0) {
        setError(`Some updates failed: ${failures.join(', ')}`);
      }
      
      if (failures.length < selectedMarketplaces.length) {
        setSuccessMessage('Vehicle listings successfully updated');
      }
      
      // Refresh listings
      await loadVehicleListings(selectedVehicle.id);
    } catch (error) {
      setError('Failed to update listings: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };
  
  // Remove vehicle listings from selected marketplaces
  const removeListings = async () => {
    if (!selectedVehicle || selectedMarketplaces.length === 0) {
      setError('Please select a vehicle and at least one marketplace');
      return;
    }
    
    try {
      setActionLoading(true);
      setError('');
      setSuccessMessage('');
      
      const result = await inventoryService.removeVehicleFromMarketplaces(
        selectedVehicle.id, 
        selectedMarketplaces
      );
      
      // Check for any failures
      const failures = Object.entries(result)
        .filter(([_, r]) => !r.success)
        .map(([id, r]) => `${getMarketplaceName(id)}: ${r.errorMessage}`);
      
      if (failures.length > 0) {
        setError(`Some removals failed: ${failures.join(', ')}`);
      }
      
      if (failures.length < selectedMarketplaces.length) {
        setSuccessMessage('Vehicle listings successfully removed');
      }
      
      // Refresh listings
      await loadVehicleListings(selectedVehicle.id);
    } catch (error) {
      setError('Failed to remove listings: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };
  
  // Synchronize all inventory
  const synchronizeInventory = async () => {
    try {
      setActionLoading(true);
      setError('');
      setSuccessMessage('');
      
      const result = await inventoryService.synchronizeInventory();
      
      setSuccessMessage(`Inventory sync completed. Total: ${result.totalVehicles}, Success: ${result.successCount}, Failures: ${result.failureCount}`);
      
      // Refresh listings for current vehicle
      if (selectedVehicle) {
        await loadVehicleListings(selectedVehicle.id);
      }
    } catch (error) {
      setError('Failed to synchronize inventory: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };
  
  // Open credential modal for a marketplace
  const openCredentialModal = (marketplace) => {
    setCredentialMarketplace(marketplace);
    setCredentials({
      apiKey: '',
      apiSecret: '',
      username: '',
      password: '',
      dealerId: ''
    });
    setShowCredentialModal(true);
  };
  
  // Handle credential form input changes
  const handleCredentialChange = (e) => {
    const { name, value } = e.target;
    setCredentials({ ...credentials, [name]: value });
  };
  
  // Verify and save marketplace credentials
  const verifyCredentials = async () => {
    try {
      setActionLoading(true);
      
      const result = await inventoryService.verifyMarketplaceCredentials(
        credentialMarketplace.id, 
        credentials
      );
      
      if (result.isValid) {
        setSuccessMessage(`Credentials verified successfully for ${credentialMarketplace.name}`);
        setShowCredentialModal(false);
        
        // Reload marketplace data
        const marketplacesResponse = await inventoryService.getAvailableMarketplaces();
        setMarketplaces(marketplacesResponse);
      } else {
        setError('Invalid credentials. Please check and try again.');
      }
    } catch (error) {
      setError('Failed to verify credentials: ' + error.message);
    } finally {
      setActionLoading(false);
    }
  };
  
  // Helper function to get marketplace name by ID
  const getMarketplaceName = (id) => {
    const marketplace = marketplaces.find(m => m.id === id);
    return marketplace ? marketplace.name : id;
  };
  
  // Helper function to get status badge
  const getStatusBadge = (status) => {
    switch (status) {
      case 'Active':
        return <Badge bg="success">Active</Badge>;
      case 'Pending':
        return <Badge bg="warning">Pending</Badge>;
      case 'Featured':
        return <Badge bg="primary">Featured</Badge>;
      case 'Rejected':
        return <Badge bg="danger">Rejected</Badge>;
      case 'Expired':
        return <Badge bg="secondary">Expired</Badge>;
      case 'Removed':
        return <Badge bg="dark">Removed</Badge>;
      default:
        return <Badge bg="light" text="dark">Unknown</Badge>;
    }
  };
  
  // Helper function to get connection badge
  const getConnectionBadge = (status) => {
    switch (status) {
      case 'Connected':
        return <Badge bg="success">Connected</Badge>;
      case 'Disconnected':
        return <Badge bg="danger">Disconnected</Badge>;
      case 'AuthError':
        return <Badge bg="warning">Auth Error</Badge>;
      case 'ConfigurationError':
        return <Badge bg="warning">Config Error</Badge>;
      case 'RateLimited':
        return <Badge bg="info">Rate Limited</Badge>;
      case 'ServiceUnavailable':
        return <Badge bg="secondary">Service Unavailable</Badge>;
      default:
        return <Badge bg="light" text="dark">Unknown</Badge>;
    }
  };
  
  // Render loading state
  if (isLoading) {
    return (
      <Container className="mt-4">
        <div className="text-center p-5">
          <Spinner animation="border" />
          <p className="mt-3">Loading marketplace data...</p>
        </div>
      </Container>
    );
  }
  
  return (
    <Container fluid className="mt-4 marketplace-manager">
      <h2 className="mb-4">Marketplace Manager</h2>
      
      {error && (
        <Alert variant="danger" onClose={() => setError('')} dismissible>
          {error}
        </Alert>
      )}
      
      {successMessage && (
        <Alert variant="success" onClose={() => setSuccessMessage('')} dismissible>
          {successMessage}
        </Alert>
      )}
      
      <Row>
        <Col md={3}>
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Vehicles</h5>
            </Card.Header>
            <Card.Body className="p-0">
              <div className="vehicle-list">
                {vehicles.length === 0 ? (
                  <p className="text-center p-3">No vehicles available for listing</p>
                ) : (
                  <ul className="list-group list-group-flush">
                    {vehicles.map(vehicle => (
                      <li 
                        key={vehicle.id} 
                        className={`list-group-item ${selectedVehicle?.id === vehicle.id ? 'active' : ''}`}
                        onClick={() => handleVehicleSelect(vehicle)}
                      >
                        <div className="vehicle-item">
                          <div className="vehicle-image">
                            {vehicle.thumbnailUrl ? (
                              <img src={vehicle.thumbnailUrl} alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`} />
                            ) : (
                              <div className="placeholder-image">No Image</div>
                            )}
                          </div>
                          <div className="vehicle-info">
                            <div className="vehicle-title">{vehicle.year} {vehicle.make} {vehicle.model}</div>
                            <div className="vehicle-subtitle">
                              Stock #: {vehicle.stockNumber} | ${vehicle.retailPrice}
                            </div>
                          </div>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </Card.Body>
          </Card>
          
          <Card>
            <Card.Header>
              <h5 className="mb-0">Actions</h5>
            </Card.Header>
            <Card.Body>
              <Button 
                variant="primary" 
                block 
                className="mb-2"
                onClick={listVehicle}
                disabled={actionLoading || !selectedVehicle || selectedMarketplaces.length === 0}
              >
                {actionLoading ? <Spinner animation="border" size="sm" /> : 'List Vehicle'}
              </Button>
              
              <Button 
                variant="info" 
                block 
                className="mb-2"
                onClick={updateListings}
                disabled={actionLoading || !selectedVehicle || selectedMarketplaces.length === 0}
              >
                {actionLoading ? <Spinner animation="border" size="sm" /> : 'Update Listings'}
              </Button>
              
              <Button 
                variant="danger" 
                block 
                className="mb-2"
                onClick={removeListings}
                disabled={actionLoading || !selectedVehicle || selectedMarketplaces.length === 0}
              >
                {actionLoading ? <Spinner animation="border" size="sm" /> : 'Remove Listings'}
              </Button>
              
              <hr />
              
              <Button 
                variant="success" 
                block
                onClick={synchronizeInventory}
                disabled={actionLoading}
              >
                {actionLoading ? <Spinner animation="border" size="sm" /> : 'Synchronize All Inventory'}
              </Button>
            </Card.Body>
          </Card>
        </Col>
        
        <Col md={9}>
          <Card className="mb-4">
            <Card.Header className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Available Marketplaces</h5>
              {selectedVehicle && (
                <span>
                  <strong>Selected Vehicle:</strong> {selectedVehicle.year} {selectedVehicle.make} {selectedVehicle.model} ({selectedVehicle.stockNumber})
                </span>
              )}
            </Card.Header>
            <Card.Body>
              <Table striped responsive>
                <thead>
                  <tr>
                    <th style={{ width: '5%' }}></th>
                    <th style={{ width: '25%' }}>Marketplace</th>
                    <th style={{ width: '15%' }}>Status</th>
                    <th style={{ width: '25%' }}>Features</th>
                    <th style={{ width: '15%' }}>Connection</th>
                    <th style={{ width: '15%' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {marketplaces.map(marketplace => (
                    <tr key={marketplace.id}>
                      <td>
                        <Form.Check 
                          type="checkbox"
                          checked={selectedMarketplaces.includes(marketplace.id)}
                          onChange={() => toggleMarketplace(marketplace.id)}
                          disabled={!marketplace.isConnected}
                        />
                      </td>
                      <td>
                        <div className="d-flex align-items-center">
                          {marketplace.logoUrl && (
                            <img 
                              src={marketplace.logoUrl} 
                              alt={marketplace.name} 
                              className="marketplace-logo me-2"
                            />
                          )}
                          <span>{marketplace.name}</span>
                        </div>
                      </td>
                      <td>
                        {selectedVehicle && vehicleListings[marketplace.id] ? (
                          getStatusBadge(vehicleListings[marketplace.id].state)
                        ) : (
                          <Badge bg="light" text="dark">Not Listed</Badge>
                        )}
                      </td>
                      <td>
                        <div className="marketplace-features">
                          {marketplace.supportedFeatures?.map(feature => (
                            <Badge key={feature} bg="info" className="me-1 mb-1">
                              {feature.replace('_', ' ')}
                            </Badge>
                          ))}
                        </div>
                      </td>
                      <td>
                        {getConnectionBadge(marketplace.connectionStatus)}
                      </td>
                      <td>
                        <Button 
                          variant="outline-primary" 
                          size="sm"
                          className="me-2"
                          onClick={() => openCredentialModal(marketplace)}
                        >
                          Configure
                        </Button>
                        
                        {selectedVehicle && vehicleListings[marketplace.id] && (
                          <Button 
                            variant="outline-info" 
                            size="sm"
                            onClick={() => setShowDetails(true)}
                          >
                            Details
                          </Button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </Card.Body>
          </Card>
          
          {selectedVehicle && Object.keys(vehicleStats).length > 0 && (
            <Card>
              <Card.Header>
                <h5 className="mb-0">Listing Performance</h5>
              </Card.Header>
              <Card.Body>
                <Tabs defaultActiveKey="overview" className="mb-3">
                  <Tab eventKey="overview" title="Overview">
                    <Row>
                      {Object.entries(vehicleStats).map(([marketplaceId, stats]) => (
                        <Col md={6} lg={4} key={marketplaceId}>
                          <Card className="mb-3">
                            <Card.Header>
                              <h6>{getMarketplaceName(marketplaceId)}</h6>
                            </Card.Header>
                            <Card.Body>
                              <Table className="stats-table" size="sm" borderless>
                                <tbody>
                                  <tr>
                                    <td>Total Views:</td>
                                    <td>{stats.views}</td>
                                  </tr>
                                  <tr>
                                    <td>Inquiries:</td>
                                    <td>{stats.inquiries}</td>
                                  </tr>
                                  <tr>
                                    <td>Saves:</td>
                                    <td>{stats.saves}</td>
                                  </tr>
                                  <tr>
                                    <td>Shares:</td>
                                    <td>{stats.shares}</td>
                                  </tr>
                                  <tr>
                                    <td>Engagement Score:</td>
                                    <td>{stats.engagementScore.toFixed(1)}</td>
                                  </tr>
                                </tbody>
                              </Table>
                            </Card.Body>
                          </Card>
                        </Col>
                      ))}
                    </Row>
                  </Tab>
                  <Tab eventKey="daily" title="Daily Views">
                    <p className="text-muted">
                      Daily view trends would be displayed here using charts. 
                      Integrate with a charting library like Chart.js or Recharts.
                    </p>
                  </Tab>
                  <Tab eventKey="comparative" title="Comparative">
                    <p className="text-muted">
                      Comparative metrics against similar listings would be displayed here.
                      This would include position in search results, price competitiveness, etc.
                    </p>
                  </Tab>
                </Tabs>
              </Card.Body>
            </Card>
          )}
        </Col>
      </Row>
      
      {/* Credential Modal */}
      <Modal show={showCredentialModal} onHide={() => setShowCredentialModal(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>
            Configure {credentialMarketplace?.name} Integration
          </Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>API Key</Form.Label>
                  <Form.Control
                    type="text"
                    name="apiKey"
                    value={credentials.apiKey}
                    onChange={handleCredentialChange}
                    placeholder="Enter API Key"
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>API Secret</Form.Label>
                  <Form.Control
                    type="password"
                    name="apiSecret"
                    value={credentials.apiSecret}
                    onChange={handleCredentialChange}
                    placeholder="Enter API Secret"
                  />
                </Form.Group>
              </Col>
            </Row>
            
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Username</Form.Label>
                  <Form.Control
                    type="text"
                    name="username"
                    value={credentials.username}
                    onChange={handleCredentialChange}
                    placeholder="Enter Username"
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Password</Form.Label>
                  <Form.Control
                    type="password"
                    name="password"
                    value={credentials.password}
                    onChange={handleCredentialChange}
                    placeholder="Enter Password"
                  />
                </Form.Group>
              </Col>
            </Row>
            
            <Form.Group className="mb-3">
              <Form.Label>Dealer ID</Form.Label>
              <Form.Control
                type="text"
                name="dealerId"
                value={credentials.dealerId}
                onChange={handleCredentialChange}
                placeholder="Enter Dealer ID"
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowCredentialModal(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={verifyCredentials} disabled={actionLoading}>
            {actionLoading ? <Spinner animation="border" size="sm" /> : 'Verify & Save'}
          </Button>
        </Modal.Footer>
      </Modal>
      
      {/* Listing Details Modal */}
      <Modal show={showDetails} onHide={() => setShowDetails(false)} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Listing Details</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="text-muted">
            This modal would display detailed information about the vehicle listing on each marketplace,
            including listing URLs, history, status changes, etc.
          </p>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowDetails(false)}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default MarketplaceManager;
