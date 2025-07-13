import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Alert, Table, Form, Spinner, Button, Badge } from 'react-bootstrap';
import { useParams, Link } from 'react-router-dom';
import inventoryService from '../../services/inventoryService';
import './PriceCompetitiveness.css';

const PriceCompetitiveness = () => {
  const [competitivenessData, setCompetitivenessData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedVehicles, setSelectedVehicles] = useState([]);
  const [availableVehicles, setAvailableVehicles] = useState([]);
  const [marketRadius, setMarketRadius] = useState(50);
  
  useEffect(() => {
    // Initial data load - get list of vehicles
    const fetchVehicles = async () => {
      try {
        const response = await inventoryService.getVehicles({
          status: 'Available',
          pageSize: 1000 // Get all available vehicles
        });
        
        setAvailableVehicles(response.data.items || []);
      } catch (err) {
        console.error("Error fetching vehicles:", err);
        setError("Failed to load vehicle list. Please try again later.");
      }
    };
    
    fetchVehicles();
  }, []);

  // Load competitiveness data when selected vehicles change
  useEffect(() => {
    const fetchCompetitiveness = async () => {
      if (selectedVehicles.length === 0) {
        setCompetitivenessData(null);
        return;
      }
      
      try {
        setLoading(true);
        
        const response = await inventoryService.getPriceCompetitiveness({
          vehicleIds: selectedVehicles,
          marketRadius: marketRadius
        });
        
        setCompetitivenessData(response.data);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching price competitiveness data:", err);
        setError("Failed to load market price data. Please try again later.");
        setLoading(false);
      }
    };

    fetchCompetitiveness();
  }, [selectedVehicles, marketRadius]);

  const handleVehicleSelection = (e) => {
    const { value, checked } = e.target;
    if (checked) {
      setSelectedVehicles([...selectedVehicles, value]);
    } else {
      setSelectedVehicles(selectedVehicles.filter(id => id !== value));
    }
  };
  
  const handleRadiusChange = (e) => {
    setMarketRadius(parseInt(e.target.value, 10));
  };
  
  const getPositionVariant = (position) => {
    switch(position) {
      case 'Underpriced':
        return 'success';
      case 'Competitive':
        return 'info';
      case 'Very Competitive':
        return 'primary';
      case 'Overpriced':
        return 'danger';
      default:
        return 'secondary';
    }
  };
  
  const handleSelectAll = () => {
    // If all are already selected, clear selection
    if (selectedVehicles.length === availableVehicles.length) {
      setSelectedVehicles([]);
    } else {
      // Otherwise select all
      setSelectedVehicles(availableVehicles.map(v => v.id));
    }
  };

  if (error) {
    return (
      <Container className="my-4">
        <Alert variant="danger">{error}</Alert>
      </Container>
    );
  }

  return (
    <Container fluid className="my-4">
      <h2>Price Competitiveness Analysis</h2>
      <p>Compare your inventory pricing against local market competitors to ensure optimal pricing strategy.</p>
      
      <Row>
        <Col md={3}>
          <Card>
            <Card.Header>Analysis Settings</Card.Header>
            <Card.Body>
              <Form>
                <Form.Group className="mb-3">
                  <Form.Label>Market Radius (miles)</Form.Label>
                  <Form.Control 
                    type="range" 
                    min={10} 
                    max={100} 
                    step={5}
                    value={marketRadius}
                    onChange={handleRadiusChange}
                  />
                  <div className="d-flex justify-content-between">
                    <span>10</span>
                    <span><strong>{marketRadius}</strong></span>
                    <span>100</span>
                  </div>
                </Form.Group>
                
                <Form.Group className="mb-3">
                  <Form.Label className="d-flex justify-content-between align-items-center">
                    <span>Select Vehicles</span>
                    <Button 
                      variant="outline-secondary" 
                      size="sm"
                      onClick={handleSelectAll}
                    >
                      {selectedVehicles.length === availableVehicles.length ? 'Unselect All' : 'Select All'}
                    </Button>
                  </Form.Label>
                  
                  <div className="vehicle-selector">
                    {availableVehicles.length > 0 ? (
                      availableVehicles.map(vehicle => (
                        <Form.Check 
                          key={vehicle.id}
                          type="checkbox"
                          id={`vehicle-${vehicle.id}`}
                          label={`${vehicle.year} ${vehicle.make} ${vehicle.model} - ${vehicle.stockNumber}`}
                          value={vehicle.id}
                          checked={selectedVehicles.includes(vehicle.id)}
                          onChange={handleVehicleSelection}
                          className="mb-1"
                        />
                      ))
                    ) : (
                      <p className="text-center">Loading vehicles...</p>
                    )}
                  </div>
                </Form.Group>
              </Form>
            </Card.Body>
          </Card>
          
          {competitivenessData && (
            <Card className="mt-3">
              <Card.Header>Market Summary</Card.Header>
              <Card.Body>
                <div className="market-stats">
                  <div className="stat-item">
                    <div className="stat-label">Competitor Vehicles</div>
                    <div className="stat-value">{competitivenessData.marketSummary.totalCompetitorVehicles}</div>
                  </div>
                  <div className="stat-item">
                    <div className="stat-label">Avg Days On Market</div>
                    <div className="stat-value">{competitivenessData.marketSummary.averageDaysOnMarket.toFixed(1)}</div>
                  </div>
                </div>
                
                <h6 className="mt-4">Top Competitors</h6>
                <ul className="competitor-list">
                  {competitivenessData.marketSummary.inventoryByCompetitor && 
                   Object.entries(competitivenessData.marketSummary.inventoryByCompetitor)
                    .sort((a, b) => b[1] - a[1])
                    .slice(0, 5)
                    .map(([name, count], index) => (
                      <li key={index}>
                        <span className="competitor-name">{name}</span>
                        <span className="competitor-count">{count}</span>
                      </li>
                    ))}
                </ul>
              </Card.Body>
            </Card>
          )}
        </Col>
        
        <Col md={9}>
          <Card>
            <Card.Header>Vehicle Price Analysis</Card.Header>
            <Card.Body>
              {loading ? (
                <div className="text-center py-5">
                  <Spinner animation="border" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </Spinner>
                  <p className="mt-3">Analyzing market data for selected vehicles...</p>
                </div>
              ) : selectedVehicles.length === 0 ? (
                <div className="text-center py-5">
                  <Alert variant="info">
                    Please select at least one vehicle to analyze market competitiveness
                  </Alert>
                </div>
              ) : !competitivenessData || competitivenessData.vehicleAnalysis.length === 0 ? (
                <div className="text-center py-5">
                  <Alert variant="warning">
                    No market data available for the selected vehicles
                  </Alert>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <Table striped bordered hover>
                      <thead>
                        <tr>
                          <th>Vehicle</th>
                          <th>Your Price</th>
                          <th>Market Avg</th>
                          <th>Diff</th>
                          <th>Position</th>
                          <th>Market Range</th>
                          <th>Days Listed</th>
                          <th>Similar Vehicles</th>
                        </tr>
                      </thead>
                      <tbody>
                        {competitivenessData.vehicleAnalysis.map((vehicle, index) => {
                          const priceDiff = vehicle.currentPrice - vehicle.averageMarketPrice;
                          const priceDiffPercent = (priceDiff / vehicle.averageMarketPrice * 100).toFixed(1);
                          
                          return (
                            <tr key={index}>
                              <td>
                                <Link to={`/inventory/vehicle/${vehicle.vehicleId}`}>
                                  {vehicle.description}
                                </Link>
                                <div className="small text-muted">#{vehicle.stockNumber}</div>
                              </td>
                              <td>${vehicle.currentPrice.toLocaleString()}</td>
                              <td>${vehicle.averageMarketPrice.toLocaleString()}</td>
                              <td className={priceDiff < 0 ? 'text-success' : priceDiff > 0 ? 'text-danger' : 'text-muted'}>
                                {priceDiff !== 0 && (priceDiff > 0 ? '+' : '')}{priceDiff.toLocaleString()} 
                                <br/>
                                <small>({priceDiff > 0 ? '+' : ''}{priceDiffPercent}%)</small>
                              </td>
                              <td>
                                <Badge bg={getPositionVariant(vehicle.pricePosition)}>
                                  {vehicle.pricePosition}
                                </Badge>
                              </td>
                              <td>
                                <div className="price-range-container">
                                  <div className="price-range">
                                    <div 
                                      className="current-price-marker" 
                                      style={{ 
                                        left: `${vehicle.percentilePosition}%` 
                                      }}
                                      title={`${vehicle.percentilePosition}th percentile`}
                                    ></div>
                                  </div>
                                  <div className="price-range-labels">
                                    <span>${vehicle.minMarketPrice.toLocaleString()}</span>
                                    <span>${vehicle.maxMarketPrice.toLocaleString()}</span>
                                  </div>
                                </div>
                              </td>
                              <td>{vehicle.daysOnMarket} days</td>
                              <td>{vehicle.competitorCount}</td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </Table>
                  </div>
                  
                  <div className="position-distribution mt-4">
                    <h5>Price Position Distribution</h5>
                    <div className="distribution-container">
                      {['Underpriced', 'Competitive', 'Very Competitive', 'Overpriced'].map(position => {
                        const count = competitivenessData.vehicleAnalysis.filter(v => v.pricePosition === position).length;
                        const percentage = (count / competitivenessData.vehicleAnalysis.length * 100).toFixed(0);
                        
                        return (
                          <div key={position} className="distribution-bar-container">
                            <div className="distribution-label">{position}</div>
                            <div className="distribution-bar-wrapper">
                              <div 
                                className={`distribution-bar bg-${getPositionVariant(position)}`}
                                style={{ width: `${percentage}%` }}
                              >
                                <span className="distribution-value">{count}</span>
                              </div>
                            </div>
                            <div className="distribution-percentage">{percentage}%</div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </>
              )}
            </Card.Body>
          </Card>
          
          <Card className="mt-4">
            <Card.Header>Price Adjustment Recommendations</Card.Header>
            <Card.Body>
              {loading ? (
                <div className="text-center py-3">
                  <p>Loading recommendations...</p>
                </div>
              ) : !competitivenessData || selectedVehicles.length === 0 ? (
                <div className="text-center py-3">
                  <p>Select vehicles to view price recommendations</p>
                </div>
              ) : (
                <div className="recommendations">
                  {competitivenessData.vehicleAnalysis
                    .filter(v => 
                      v.pricePosition === 'Overpriced' || 
                      (v.pricePosition === 'Underpriced' && v.daysOnMarket > 30)
                    )
                    .map((vehicle, index) => {
                      const isOverpriced = vehicle.pricePosition === 'Overpriced';
                      const suggestedPrice = isOverpriced 
                        ? vehicle.averageMarketPrice * 0.98 // Slightly below average for overpriced
                        : vehicle.averageMarketPrice * 1.02; // Slightly above average for underpriced with slow turnover
                      const priceDiff = suggestedPrice - vehicle.currentPrice;
                      const priceDiffPercent = (priceDiff / vehicle.currentPrice * 100).toFixed(1);
                      
                      return (
                        <Card key={index} className={`mb-3 recommendation-card ${isOverpriced ? 'border-danger' : 'border-warning'}`}>
                          <Card.Body>
                            <Row>
                              <Col md={7}>
                                <h5>{vehicle.description}</h5>
                                <p className="small">Stock #: {vehicle.stockNumber} | {vehicle.daysOnMarket} days on market</p>
                                <p>
                                  <strong>Current Price:</strong> ${vehicle.currentPrice.toLocaleString()} | 
                                  <strong> Market Avg:</strong> ${vehicle.averageMarketPrice.toLocaleString()}
                                </p>
                                <div className="recommendation-action">
                                  <Badge bg={isOverpriced ? 'danger' : 'warning'} className="me-2">
                                    {isOverpriced ? 'Price Reduction Recommended' : 'Price Increase Suggested'}
                                  </Badge>
                                  <span>
                                    <strong>Suggested Price:</strong> ${Math.round(suggestedPrice).toLocaleString()} 
                                    <span className={`ms-2 ${priceDiff < 0 ? 'text-danger' : 'text-success'}`}>
                                      ({priceDiff > 0 ? '+' : ''}{priceDiffPercent}%)
                                    </span>
                                  </span>
                                </div>
                              </Col>
                              <Col md={5} className="d-flex align-items-center justify-content-end">
                                <div className="d-flex flex-column align-items-end">
                                  <div className="mb-2">
                                    {isOverpriced ? (
                                      <span>Priced higher than {vehicle.percentilePosition}% of similar vehicles</span>
                                    ) : (
                                      <span>Slow turnover despite competitive price</span>
                                    )}
                                  </div>
                                  <div>
                                    <Button 
                                      variant={isOverpriced ? 'outline-danger' : 'outline-warning'} 
                                      size="sm" 
                                      className="me-2"
                                    >
                                      Apply Suggestion
                                    </Button>
                                    <Link to={`/inventory/vehicle/${vehicle.vehicleId}/pricing`} className="btn btn-sm btn-primary">
                                      View Details
                                    </Link>
                                  </div>
                                </div>
                              </Col>
                            </Row>
                          </Card.Body>
                        </Card>
                      );
                    })}
                    
                  {competitivenessData.vehicleAnalysis.filter(v => 
                    v.pricePosition === 'Overpriced' || 
                    (v.pricePosition === 'Underpriced' && v.daysOnMarket > 30)
                  ).length === 0 && (
                    <Alert variant="success">
                      No immediate price adjustments recommended. Your selected vehicles appear to be priced appropriately for the market.
                    </Alert>
                  )}
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default PriceCompetitiveness;
