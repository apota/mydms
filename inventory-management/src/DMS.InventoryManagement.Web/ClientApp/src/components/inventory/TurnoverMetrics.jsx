import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Alert, Table, Form, Spinner, Tab, Tabs } from 'react-bootstrap';
import inventoryService from '../../services/inventoryService';
import './TurnoverMetrics.css';

const TurnoverMetrics = () => {
  const [turnoverData, setTurnoverData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [timePeriod, setTimePeriod] = useState('month');
  const [vehicleTypes, setVehicleTypes] = useState(['New', 'Used', 'Certified']);
  const [locations, setLocations] = useState([]);
  const [selectedLocations, setSelectedLocations] = useState([]);
  
  const timePeriodOptions = [
    { value: 'week', label: 'Last Week' },
    { value: 'month', label: 'Last Month' },
    { value: 'quarter', label: 'Last Quarter' },
    { value: 'year', label: 'Last Year' }
  ];

  useEffect(() => {
    const fetchData = async () => {
      try {
        // First get the list of locations for filtering
        const locationsResponse = await inventoryService.getLocations();
        
        setLocations(locationsResponse.data);
        
        // Then get turnover data
        setLoading(true);
        
        const response = await inventoryService.getTurnoverMetrics({
          timePeriod,
          vehicleTypes,
          locationIds: selectedLocations.length > 0 ? selectedLocations : null
        });
        
        setTurnoverData(response.data);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching turnover data:", err);
        setError("Failed to load turnover metrics data. Please try again later.");
        setLoading(false);
      }
    };

    fetchData();
  }, [timePeriod, vehicleTypes, selectedLocations]);

  const handleTimePeriodChange = (e) => {
    setTimePeriod(e.target.value);
  };
  
  const handleVehicleTypeChange = (e) => {
    const { value, checked } = e.target;
    if (checked) {
      setVehicleTypes([...vehicleTypes, value]);
    } else {
      setVehicleTypes(vehicleTypes.filter(type => type !== value));
    }
  };
  
  const handleLocationChange = (e) => {
    const { value, checked } = e.target;
    if (checked) {
      setSelectedLocations([...selectedLocations, value]);
    } else {
      setSelectedLocations(selectedLocations.filter(id => id !== value));
    }
  };
  
  const getTurnoverRateBadge = (rate) => {
    if (rate >= 2) return <span className="badge bg-success">Excellent</span>;
    if (rate >= 1) return <span className="badge bg-primary">Good</span>;
    if (rate >= 0.5) return <span className="badge bg-warning text-dark">Average</span>;
    return <span className="badge bg-danger">Low</span>;
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
      <h2>Inventory Turnover Metrics</h2>
      <p>Track how quickly your inventory sells and identify opportunities to improve turnover rates.</p>
      
      <Row>
        <Col md={3}>
          <Card>
            <Card.Header>Filters</Card.Header>
            <Card.Body>
              <Form>
                <Form.Group className="mb-3">
                  <Form.Label>Time Period</Form.Label>
                  <Form.Select 
                    value={timePeriod}
                    onChange={handleTimePeriodChange}
                  >
                    {timePeriodOptions.map(option => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
                
                <Form.Group className="mb-3">
                  <Form.Label>Vehicle Types</Form.Label>
                  <div>
                    {['New', 'Used', 'Certified'].map(type => (
                      <Form.Check 
                        key={type}
                        type="checkbox"
                        id={`type-${type}`}
                        label={type}
                        value={type}
                        checked={vehicleTypes.includes(type)}
                        onChange={handleVehicleTypeChange}
                      />
                    ))}
                  </div>
                </Form.Group>
                
                <Form.Group className="mb-3">
                  <Form.Label>Locations</Form.Label>
                  <div className="location-selector">
                    {locations.map(location => (
                      <Form.Check 
                        key={location.id}
                        type="checkbox"
                        id={`location-${location.id}`}
                        label={location.name}
                        value={location.id}
                        onChange={handleLocationChange}
                      />
                    ))}
                  </div>
                </Form.Group>
              </Form>
            </Card.Body>
          </Card>
          
          {turnoverData && (
            <Card className="mt-3">
              <Card.Header>Overall Metrics</Card.Header>
              <Card.Body>
                <div className="turnover-metrics-summary">
                  <div className="metric-box">
                    <div className="metric-label">Average Days to Sell</div>
                    <div className="metric-value">{turnoverData.averageDaysToSell.toFixed(1)}</div>
                    <div className="metric-days">days</div>
                  </div>
                  <div className="metric-box">
                    <div className="metric-label">Turnover Rate</div>
                    <div className="metric-value">{turnoverData.turnoverRate.toFixed(2)}</div>
                    <div className="metric-subtitle">times per {timePeriod}</div>
                  </div>
                </div>
                
                <div className="turnover-rating mt-3">
                  <div className="rating-label">Performance Rating:</div>
                  <div className="rating-value">
                    {getTurnoverRateBadge(turnoverData.turnoverRate)}
                  </div>
                </div>
              </Card.Body>
            </Card>
          )}
        </Col>
        
        <Col md={9}>
          {loading ? (
            <div className="text-center py-5">
              <Spinner animation="border" role="status">
                <span className="visually-hidden">Loading...</span>
              </Spinner>
              <p className="mt-3">Loading turnover metrics data...</p>
            </div>
          ) : !turnoverData ? (
            <Alert variant="warning">
              No turnover data available with the selected filters.
            </Alert>
          ) : (
            <>
              <Card className="mb-4">
                <Card.Header>Turnover by Segment</Card.Header>
                <Card.Body>
                  <Tabs defaultActiveKey="table" className="mb-3">
                    <Tab eventKey="table" title="Table View">
                      <div className="table-responsive">
                        <Table striped bordered hover>
                          <thead>
                            <tr>
                              <th>Segment</th>
                              <th>Avg Days to Sell</th>
                              <th>Turnover Rate</th>
                              <th>Vehicles Sold</th>
                              <th>Current Inventory</th>
                              <th>Performance</th>
                            </tr>
                          </thead>
                          <tbody>
                            {turnoverData.bySegment
                              .sort((a, b) => b.turnoverRate - a.turnoverRate)
                              .map((segment, index) => (
                                <tr key={index}>
                                  <td>{segment.segment}</td>
                                  <td>{segment.averageDaysToSell.toFixed(1)}</td>
                                  <td>{segment.turnoverRate.toFixed(2)}</td>
                                  <td>{segment.vehiclesSold}</td>
                                  <td>{segment.currentInventory}</td>
                                  <td>{getTurnoverRateBadge(segment.turnoverRate)}</td>
                                </tr>
                              ))}
                          </tbody>
                        </Table>
                      </div>
                    </Tab>
                    <Tab eventKey="chart" title="Chart View">
                      <div className="chart-container">
                        <div className="turnover-chart-tabs">
                          <div className="turnover-chart-placeholder text-center py-5">
                            {/* In a real implementation, a proper chart would be rendered here using a library like Chart.js */}
                            <p>Turnover rate by segment chart would be displayed here</p>
                            <p>Number of segments: {turnoverData.bySegment.length}</p>
                          </div>
                        </div>
                      </div>
                    </Tab>
                  </Tabs>
                </Card.Body>
              </Card>
              
              <Row>
                <Col md={6}>
                  <Card>
                    <Card.Header>Fastest Turning Segments</Card.Header>
                    <Card.Body>
                      <div className="segment-ranking">
                        {turnoverData.bySegment
                          .sort((a, b) => b.turnoverRate - a.turnoverRate)
                          .slice(0, 5)
                          .map((segment, index) => (
                            <div key={index} className="segment-rank-item">
                              <div className="rank-number">{index + 1}</div>
                              <div className="rank-details">
                                <div className="rank-segment">{segment.segment}</div>
                                <div className="rank-metrics">
                                  <span>{segment.turnoverRate.toFixed(2)} turns</span>
                                  <span className="mx-2">|</span>
                                  <span>{segment.averageDaysToSell.toFixed(1)} days</span>
                                </div>
                              </div>
                              <div className="rank-performance">
                                {getTurnoverRateBadge(segment.turnoverRate)}
                              </div>
                            </div>
                          ))}
                      </div>
                    </Card.Body>
                  </Card>
                </Col>
                <Col md={6}>
                  <Card>
                    <Card.Header>Slowest Turning Segments</Card.Header>
                    <Card.Body>
                      <div className="segment-ranking">
                        {turnoverData.bySegment
                          .sort((a, b) => a.turnoverRate - b.turnoverRate)
                          .slice(0, 5)
                          .map((segment, index) => (
                            <div key={index} className="segment-rank-item">
                              <div className="rank-number">{index + 1}</div>
                              <div className="rank-details">
                                <div className="rank-segment">{segment.segment}</div>
                                <div className="rank-metrics">
                                  <span>{segment.turnoverRate.toFixed(2)} turns</span>
                                  <span className="mx-2">|</span>
                                  <span>{segment.averageDaysToSell.toFixed(1)} days</span>
                                </div>
                              </div>
                              <div className="rank-performance">
                                {getTurnoverRateBadge(segment.turnoverRate)}
                              </div>
                            </div>
                          ))}
                      </div>
                    </Card.Body>
                  </Card>
                </Col>
              </Row>
              
              <Card className="mt-4">
                <Card.Header>Turnover Trend Over Time</Card.Header>
                <Card.Body>
                  <div className="trend-chart-container">
                    <div className="text-center py-5">
                      {/* In a real implementation, a proper chart would be rendered here */}
                      <p>Turnover trend chart would be displayed here</p>
                      <p>Number of data points: {turnoverData.trendData.length}</p>
                    </div>
                  </div>
                </Card.Body>
              </Card>
            </>
          )}
        </Col>
      </Row>
    </Container>
  );
};

export default TurnoverMetrics;
