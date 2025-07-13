import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Alert, Spinner, Form } from 'react-bootstrap';
import { useParams, Link } from 'react-router-dom';
import inventoryService from '../../services/inventoryService';
import './AgingHeatMap.css';

const AgingHeatMap = () => {
  const [agingData, setAgingData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    locations: [],
    vehicleTypes: ['New', 'Used', 'Certified']
  });
  const [selectedLocations, setSelectedLocations] = useState([]);
  const [selectedVehicleTypes, setSelectedVehicleTypes] = useState(['New', 'Used', 'Certified']);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        // First get the list of locations for filtering
        const locationsResponse = await inventoryService.getLocations();
        
        // Set up available locations for filtering
        setFilters(prev => ({
          ...prev,
          locations: locationsResponse.data
        }));
        
        // Then get the aging data
        const response = await inventoryService.getAgingAnalytics({
          locationIds: selectedLocations.length > 0 ? selectedLocations : null,
          vehicleTypes: selectedVehicleTypes
        });
        
        setAgingData(response.data);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching aging data:", err);
        setError("Failed to load aging data. Please try again later.");
        setLoading(false);
      }
    };

    fetchData();
  }, [selectedLocations, selectedVehicleTypes]);

  const handleLocationChange = (e) => {
    const { value, checked } = e.target;
    if (checked) {
      setSelectedLocations([...selectedLocations, value]);
    } else {
      setSelectedLocations(selectedLocations.filter(id => id !== value));
    }
  };

  const handleVehicleTypeChange = (e) => {
    const { value, checked } = e.target;
    if (checked) {
      setSelectedVehicleTypes([...selectedVehicleTypes, value]);
    } else {
      setSelectedVehicleTypes(selectedVehicleTypes.filter(type => type !== value));
    }
  };

  // Helper to get CSS class based on aging range
  const getAgingClass = (days) => {
    if (days <= 15) return 'aging-normal';
    if (days <= 30) return 'aging-warning-low';
    if (days <= 45) return 'aging-warning-medium';
    if (days <= 60) return 'aging-warning-high';
    return 'aging-critical';
  };

  if (loading) {
    return (
      <Container className="my-4 text-center">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
        <p>Loading aging analytics data...</p>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="my-4">
        <Alert variant="danger">{error}</Alert>
      </Container>
    );
  }

  return (
    <Container fluid className="my-4">
      <h2>Inventory Aging Analysis</h2>
      <p>Visualize aging patterns across your inventory to identify slow-moving vehicles.</p>
      
      <Row className="mb-4">
        <Col md={3}>
          <Card>
            <Card.Header>Filters</Card.Header>
            <Card.Body>
              <Form>
                <Form.Group className="mb-3">
                  <Form.Label>Locations</Form.Label>
                  {filters.locations.map(location => (
                    <Form.Check 
                      key={location.id}
                      type="checkbox"
                      id={`location-${location.id}`}
                      label={location.name}
                      value={location.id}
                      onChange={handleLocationChange}
                    />
                  ))}
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label>Vehicle Types</Form.Label>
                  {filters.vehicleTypes.map(type => (
                    <Form.Check 
                      key={type}
                      type="checkbox"
                      id={`type-${type}`}
                      label={type}
                      value={type}
                      checked={selectedVehicleTypes.includes(type)}
                      onChange={handleVehicleTypeChange}
                    />
                  ))}
                </Form.Group>
              </Form>
            </Card.Body>
          </Card>
          
          {agingData && (
            <Card className="mt-3">
              <Card.Header>Summary</Card.Header>
              <Card.Body>
                <Row className="aging-summary">
                  <Col>
                    <div className="summary-category">
                      <div className="aging-label">0-15 days</div>
                      <div className="aging-count aging-normal">{agingData.agingBrackets['0-15'].total}</div>
                    </div>
                  </Col>
                  <Col>
                    <div className="summary-category">
                      <div className="aging-label">16-30 days</div>
                      <div className="aging-count aging-warning-low">{agingData.agingBrackets['16-30'].total}</div>
                    </div>
                  </Col>
                </Row>
                <Row className="aging-summary">
                  <Col>
                    <div className="summary-category">
                      <div className="aging-label">31-45 days</div>
                      <div className="aging-count aging-warning-medium">{agingData.agingBrackets['31-45'].total}</div>
                    </div>
                  </Col>
                  <Col>
                    <div className="summary-category">
                      <div className="aging-label">46-60 days</div>
                      <div className="aging-count aging-warning-high">{agingData.agingBrackets['46-60'].total}</div>
                    </div>
                  </Col>
                </Row>
                <Row className="aging-summary">
                  <Col>
                    <div className="summary-category">
                      <div className="aging-label">61+ days</div>
                      <div className="aging-count aging-critical">{agingData.agingBrackets['61+'].total}</div>
                    </div>
                  </Col>
                </Row>
              </Card.Body>
            </Card>
          )}
        </Col>
        
        <Col md={9}>
          {agingData && agingData.criticalAlerts.length > 0 && (
            <Card className="mb-4">
              <Card.Header className="text-white bg-danger">
                Critical Aging Alerts ({agingData.criticalAlerts.length})
              </Card.Header>
              <Card.Body style={{ maxHeight: '200px', overflowY: 'auto' }}>
                {agingData.criticalAlerts.map((alert, index) => (
                  <Alert 
                    key={index} 
                    variant={alert.daysInInventory > 60 ? "danger" : "warning"}
                    className="mb-2"
                  >
                    <div className="d-flex justify-content-between">
                      <div>
                        <strong>{alert.description}</strong> - {alert.daysInInventory} days in inventory
                      </div>
                      <div>
                        <Link to={`/inventory/vehicle/${alert.vehicleId}`} className="btn btn-sm btn-outline-primary">View</Link>
                      </div>
                    </div>
                    <div className="mt-2 small">
                      <span>Current: ${alert.currentPrice.toLocaleString()}</span>
                      <span className="mx-2">|</span>
                      <span>Suggested: ${alert.suggestedPrice.toLocaleString()}</span>
                      <span className="mx-2">|</span>
                      <span>{alert.recommendedAction}</span>
                    </div>
                  </Alert>
                ))}
              </Card.Body>
            </Card>
          )}
          
          <Card className="mb-4">
            <Card.Header>Inventory Aging Heat Map</Card.Header>
            <Card.Body>
              {agingData && agingData.heatMapData.length > 0 ? (
                <div className="heatmap-container">
                  <div className="heatmap-legend">
                    <span className="legend-item">
                      <span className="legend-color aging-normal"></span>
                      <span className="legend-label">0-15 days</span>
                    </span>
                    <span className="legend-item">
                      <span className="legend-color aging-warning-low"></span>
                      <span className="legend-label">16-30 days</span>
                    </span>
                    <span className="legend-item">
                      <span className="legend-color aging-warning-medium"></span>
                      <span className="legend-label">31-45 days</span>
                    </span>
                    <span className="legend-item">
                      <span className="legend-color aging-warning-high"></span>
                      <span className="legend-label">46-60 days</span>
                    </span>
                    <span className="legend-item">
                      <span className="legend-color aging-critical"></span>
                      <span className="legend-label">61+ days</span>
                    </span>
                  </div>
                  
                  <div className="heatmap-grid">
                    {agingData.heatMapData.map((item, index) => (
                      <div 
                        key={index} 
                        className={`heatmap-cell ${getAgingClass(item.averageDaysInInventory)}`}
                        title={`${item.category} ${item.subcategory}: ${item.count} vehicles, avg ${item.averageDaysInInventory} days`}
                      >
                        <div className="cell-header">
                          {item.category} {item.subcategory}
                        </div>
                        <div className="cell-content">
                          <div className="cell-count">{item.count}</div>
                          <div className="cell-days">{item.averageDaysInInventory} days</div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <div className="text-center py-5">
                  <p>No heat map data available with the current filters.</p>
                </div>
              )}
            </Card.Body>
          </Card>
          
          <Card>
            <Card.Header>Aging Trends Over Time</Card.Header>
            <Card.Body>
              {agingData && agingData.trendData.length > 0 ? (
                <div className="trends-chart-container">
                  <div className="chart-placeholder">
                    {/* In a real implementation, we would use a charting library like Chart.js or Recharts */}
                    <div className="text-center py-5">
                      <p>Aging trend chart would be rendered here using a charting library</p>
                      <p>Data points: {agingData.trendData.length}</p>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="text-center py-5">
                  <p>No trend data available with the current filters.</p>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default AgingHeatMap;
