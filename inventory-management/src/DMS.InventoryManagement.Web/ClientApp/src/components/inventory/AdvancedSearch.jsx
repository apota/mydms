import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Form, Button, Spinner, Collapse } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import inventoryService from '../../services/inventoryService';
import './AdvancedSearch.css';

const AdvancedSearch = () => {
  const navigate = useNavigate();

  // Form state
  const [searchQuery, setSearchQuery] = useState('');
  const [make, setMake] = useState('');
  const [model, setModel] = useState('');
  const [yearFrom, setYearFrom] = useState('');
  const [yearTo, setYearTo] = useState('');
  const [priceFrom, setPriceFrom] = useState('');
  const [priceTo, setPriceTo] = useState('');
  const [mileageFrom, setMileageFrom] = useState('');
  const [mileageTo, setMileageTo] = useState('');
  const [vehicleType, setVehicleType] = useState('');
  const [vehicleStatus, setVehicleStatus] = useState('');
  const [features, setFeatures] = useState([]);
  const [selectedFeatures, setSelectedFeatures] = useState([]);
  const [sortBy, setSortBy] = useState('');
  const [sortDirection, setSortDirection] = useState('asc');

  // UI state
  const [loading, setLoading] = useState(false);
  const [searchResults, setSearchResults] = useState([]);
  const [totalResults, setTotalResults] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [showAdvancedOptions, setShowAdvancedOptions] = useState(false);
  const [availableMakes, setAvailableMakes] = useState([]);
  const [availableModels, setAvailableModels] = useState([]);
  
  useEffect(() => {
    // Load makes and popular features for filtering
    async function loadFilterOptions() {
      try {
        const makes = await inventoryService.getAvailableMakes();
        setAvailableMakes(makes);
        
        const features = await inventoryService.getPopularFeatures();
        setFeatures(features);
      } catch (error) {
        console.error('Error loading filter options:', error);
      }
    }
    
    loadFilterOptions();
  }, []);
  
  useEffect(() => {
    // Load models based on selected make
    async function loadModels() {
      if (make) {
        try {
          const models = await inventoryService.getAvailableModelsByMake(make);
          setAvailableModels(models);
        } catch (error) {
          console.error('Error loading models:', error);
          setAvailableModels([]);
        }
      } else {
        setAvailableModels([]);
      }
    }
    
    loadModels();
  }, [make]);

  const handleSearch = async (e) => {
    e.preventDefault();
    setLoading(true);
    setPage(1); // Reset to first page
    
    try {
      const searchCriteria = {
        query: searchQuery,
        make,
        model,
        yearFrom: yearFrom ? parseInt(yearFrom) : null,
        yearTo: yearTo ? parseInt(yearTo) : null,
        priceFrom: priceFrom ? parseFloat(priceFrom) : null,
        priceTo: priceTo ? parseFloat(priceTo) : null,
        mileageFrom: mileageFrom ? parseInt(mileageFrom) : null,
        mileageTo: mileageTo ? parseInt(mileageTo) : null,
        vehicleType: vehicleType || null,
        vehicleStatus: vehicleStatus || null,
        features: selectedFeatures,
        skip: 0,
        take: pageSize,
        sortBy: sortBy || null,
        sortDescending: sortDirection === 'desc'
      };
      
      const results = await inventoryService.searchVehicles(searchCriteria);
      setSearchResults(results.vehicles);
      setTotalResults(results.totalCount);
    } catch (error) {
      console.error('Error searching vehicles:', error);
      alert('An error occurred while searching vehicles');
    } finally {
      setLoading(false);
    }
  };
  
  const loadMore = async () => {
    setLoading(true);
    const nextPage = page + 1;
    
    try {
      const searchCriteria = {
        query: searchQuery,
        make,
        model,
        yearFrom: yearFrom ? parseInt(yearFrom) : null,
        yearTo: yearTo ? parseInt(yearTo) : null,
        priceFrom: priceFrom ? parseFloat(priceFrom) : null,
        priceTo: priceTo ? parseFloat(priceTo) : null,
        mileageFrom: mileageFrom ? parseInt(mileageFrom) : null,
        mileageTo: mileageTo ? parseInt(mileageTo) : null,
        vehicleType: vehicleType || null,
        vehicleStatus: vehicleStatus || null,
        features: selectedFeatures,
        skip: page * pageSize,
        take: pageSize,
        sortBy: sortBy || null,
        sortDescending: sortDirection === 'desc'
      };
      
      const results = await inventoryService.searchVehicles(searchCriteria);
      setSearchResults([...searchResults, ...results.vehicles]);
      setPage(nextPage);
    } catch (error) {
      console.error('Error loading more vehicles:', error);
      alert('An error occurred while loading more vehicles');
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setSearchQuery('');
    setMake('');
    setModel('');
    setYearFrom('');
    setYearTo('');
    setPriceFrom('');
    setPriceTo('');
    setMileageFrom('');
    setMileageTo('');
    setVehicleType('');
    setVehicleStatus('');
    setSelectedFeatures([]);
    setSortBy('');
    setSortDirection('asc');
  };
  
  const toggleFeature = (feature) => {
    if (selectedFeatures.includes(feature)) {
      setSelectedFeatures(selectedFeatures.filter(f => f !== feature));
    } else {
      setSelectedFeatures([...selectedFeatures, feature]);
    }
  };
  
  const viewVehicleDetail = (vehicleId) => {
    navigate(`/inventory/vehicles/${vehicleId}`);
  };

  const renderSearchResults = () => {
    if (loading && searchResults.length === 0) {
      return (
        <div className="text-center my-5">
          <Spinner animation="border" />
          <p className="mt-2">Searching for vehicles...</p>
        </div>
      );
    }
    
    if (searchResults.length === 0) {
      return (
        <div className="text-center my-5">
          <h4>No vehicles found matching your criteria</h4>
          <p>Try adjusting your search filters</p>
        </div>
      );
    }
    
    return (
      <div className="search-results">
        <h3>Search Results ({totalResults} vehicles found)</h3>
        <Row>
          {searchResults.map(vehicle => (
            <Col md={6} lg={4} key={vehicle.id} className="mb-4">
              <Card className="vehicle-card h-100">
                {vehicle.images?.length > 0 ? (
                  <Card.Img 
                    variant="top" 
                    src={vehicle.images[0].url} 
                    className="vehicle-img"
                    alt={`${vehicle.year} ${vehicle.make} ${vehicle.model}`} 
                  />
                ) : (
                  <div className="no-image-placeholder">No Image Available</div>
                )}
                <Card.Body>
                  <Card.Title>
                    {vehicle.year} {vehicle.make} {vehicle.model}
                    {vehicle.trim && ` ${vehicle.trim}`}
                  </Card.Title>
                  <div className="vehicle-details">
                    <div><strong>Stock#:</strong> {vehicle.stockNumber}</div>
                    <div><strong>VIN:</strong> {vehicle.vin}</div>
                    <div><strong>Price:</strong> ${vehicle.listPrice.toLocaleString()}</div>
                    <div><strong>Mileage:</strong> {vehicle.mileage.toLocaleString()}</div>
                    <div><strong>Exterior:</strong> {vehicle.exteriorColor}</div>
                    <div>
                      <strong>Status:</strong> 
                      <span className={`status-badge status-${vehicle.status.toLowerCase()}`}>
                        {vehicle.status}
                      </span>
                    </div>
                    {vehicle.daysInInventory > 0 && (
                      <div>
                        <strong>Days in Stock:</strong> 
                        <span className={`days-badge days-${vehicle.agingAlertLevel?.toLowerCase() || 'normal'}`}>
                          {vehicle.daysInInventory}
                        </span>
                      </div>
                    )}
                  </div>
                  <Button 
                    variant="primary" 
                    className="mt-3 w-100"
                    onClick={() => viewVehicleDetail(vehicle.id)}
                  >
                    View Details
                  </Button>
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
        
        {searchResults.length < totalResults && (
          <div className="text-center my-4">
            <Button 
              variant="outline-primary" 
              onClick={loadMore} 
              disabled={loading}
            >
              {loading ? (
                <>
                  <Spinner as="span" animation="border" size="sm" className="mr-2" />
                  Loading...
                </>
              ) : (
                'Load More'
              )}
            </Button>
          </div>
        )}
      </div>
    );
  };

  return (
    <Container fluid className="advanced-search-container">
      <h2>Advanced Vehicle Search</h2>
      <Form onSubmit={handleSearch}>
        <Card className="mb-4">
          <Card.Body>
            <Row className="mb-3">
              <Col md={8}>
                <Form.Group>
                  <Form.Label>Search</Form.Label>
                  <Form.Control 
                    type="text" 
                    placeholder="Search by make, model, VIN, stock #, etc." 
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                  />
                </Form.Group>
              </Col>
              <Col md={4} className="d-flex align-items-end">
                <Button 
                  variant="link" 
                  onClick={() => setShowAdvancedOptions(!showAdvancedOptions)}
                  className="mb-2"
                >
                  {showAdvancedOptions ? 'Hide Advanced Options' : 'Show Advanced Options'}
                </Button>
              </Col>
            </Row>
            
            <Collapse in={showAdvancedOptions}>
              <div>
                <Row>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Make</Form.Label>
                      <Form.Select 
                        value={make}
                        onChange={(e) => setMake(e.target.value)}
                      >
                        <option value="">Any Make</option>
                        {availableMakes.map((makeName, index) => (
                          <option key={index} value={makeName}>{makeName}</option>
                        ))}
                      </Form.Select>
                    </Form.Group>
                  </Col>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Model</Form.Label>
                      <Form.Select 
                        value={model}
                        onChange={(e) => setModel(e.target.value)}
                        disabled={!make}
                      >
                        <option value="">Any Model</option>
                        {availableModels.map((modelName, index) => (
                          <option key={index} value={modelName}>{modelName}</option>
                        ))}
                      </Form.Select>
                    </Form.Group>
                  </Col>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Year</Form.Label>
                      <div className="d-flex">
                        <Form.Select 
                          value={yearFrom}
                          onChange={(e) => setYearFrom(e.target.value)}
                          className="me-2"
                        >
                          <option value="">From</option>
                          {Array.from({ length: 30 }, (_, i) => new Date().getFullYear() - i).map(year => (
                            <option key={year} value={year}>{year}</option>
                          ))}
                        </Form.Select>
                        <Form.Select 
                          value={yearTo}
                          onChange={(e) => setYearTo(e.target.value)}
                        >
                          <option value="">To</option>
                          {Array.from({ length: 30 }, (_, i) => new Date().getFullYear() - i).map(year => (
                            <option key={year} value={year}>{year}</option>
                          ))}
                        </Form.Select>
                      </div>
                    </Form.Group>
                  </Col>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Vehicle Type</Form.Label>
                      <Form.Select 
                        value={vehicleType}
                        onChange={(e) => setVehicleType(e.target.value)}
                      >
                        <option value="">Any Type</option>
                        <option value="New">New</option>
                        <option value="Used">Used</option>
                        <option value="CertifiedPreOwned">Certified Pre-Owned</option>
                      </Form.Select>
                    </Form.Group>
                  </Col>
                </Row>
                
                <Row>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Price Range</Form.Label>
                      <div className="d-flex">
                        <Form.Control 
                          type="number" 
                          placeholder="Min" 
                          value={priceFrom}
                          onChange={(e) => setPriceFrom(e.target.value)}
                          className="me-2"
                        />
                        <Form.Control 
                          type="number" 
                          placeholder="Max" 
                          value={priceTo}
                          onChange={(e) => setPriceTo(e.target.value)}
                        />
                      </div>
                    </Form.Group>
                  </Col>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Mileage Range</Form.Label>
                      <div className="d-flex">
                        <Form.Control 
                          type="number" 
                          placeholder="Min" 
                          value={mileageFrom}
                          onChange={(e) => setMileageFrom(e.target.value)}
                          className="me-2"
                        />
                        <Form.Control 
                          type="number" 
                          placeholder="Max" 
                          value={mileageTo}
                          onChange={(e) => setMileageTo(e.target.value)}
                        />
                      </div>
                    </Form.Group>
                  </Col>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Status</Form.Label>
                      <Form.Select 
                        value={vehicleStatus}
                        onChange={(e) => setVehicleStatus(e.target.value)}
                      >
                        <option value="">Any Status</option>
                        <option value="InTransit">In Transit</option>
                        <option value="Receiving">Receiving</option>
                        <option value="InStock">In Stock</option>
                        <option value="Reconditioning">Reconditioning</option>
                        <option value="FrontLine">Front Line</option>
                        <option value="OnHold">On Hold</option>
                        <option value="Sold">Sold</option>
                        <option value="Delivered">Delivered</option>
                      </Form.Select>
                    </Form.Group>
                  </Col>
                  <Col md={6} lg={3}>
                    <Form.Group className="mb-3">
                      <Form.Label>Sort By</Form.Label>
                      <div className="d-flex">
                        <Form.Select 
                          value={sortBy}
                          onChange={(e) => setSortBy(e.target.value)}
                          className="me-2"
                        >
                          <option value="">Default</option>
                          <option value="year">Year</option>
                          <option value="price">Price</option>
                          <option value="mileage">Mileage</option>
                          <option value="make">Make</option>
                          <option value="model">Model</option>
                        </Form.Select>
                        <Form.Select 
                          value={sortDirection}
                          onChange={(e) => setSortDirection(e.target.value)}
                        >
                          <option value="asc">Ascending</option>
                          <option value="desc">Descending</option>
                        </Form.Select>
                      </div>
                    </Form.Group>
                  </Col>
                </Row>
                
                <Row>
                  <Col>
                    <Form.Group className="mb-3">
                      <Form.Label>Features</Form.Label>
                      <div className="feature-tags">
                        {features.map((feature, index) => (
                          <Button
                            key={index}
                            variant={selectedFeatures.includes(feature) ? "primary" : "outline-primary"}
                            size="sm"
                            onClick={() => toggleFeature(feature)}
                            className="me-2 mb-2"
                          >
                            {feature}
                          </Button>
                        ))}
                      </div>
                    </Form.Group>
                  </Col>
                </Row>
              </div>
            </Collapse>
            
            <div className="d-flex justify-content-between mt-3">
              <Button variant="secondary" onClick={handleReset}>
                Reset
              </Button>
              <Button variant="primary" type="submit" disabled={loading}>
                Search
              </Button>
            </div>
          </Card.Body>
        </Card>
      </Form>
      
      {renderSearchResults()}
    </Container>
  );
};

export default AdvancedSearch;
