import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Alert, Table, Form, Spinner, Button, Tab, Tabs } from 'react-bootstrap';
import { useParams } from 'react-router-dom';
import inventoryService from '../../services/inventoryService';
import './InventoryMixAnalysis.css';

const InventoryMixAnalysis = () => {
  const [mixData, setMixData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [groupingFactor, setGroupingFactor] = useState('make');
  const [comparisonPeriod, setComparisonPeriod] = useState('previous-month');
  
  const groupingOptions = [
    { value: 'make', label: 'Manufacturer' },
    { value: 'model', label: 'Model' },
    { value: 'bodystyle', label: 'Body Style' },
    { value: 'year', label: 'Year' },
    { value: 'condition', label: 'Condition' }
  ];
  
  const periodOptions = [
    { value: 'previous-month', label: 'Previous Month' },
    { value: 'previous-quarter', label: 'Previous Quarter' },
    { value: 'previous-year', label: 'Previous Year' }
  ];

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        const response = await inventoryService.getInventoryMixAnalysis({
          groupingFactor,
          comparisonPeriod
        });
        
        setMixData(response.data);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching inventory mix data:", err);
        setError("Failed to load inventory mix data. Please try again later.");
        setLoading(false);
      }
    };

    fetchData();
  }, [groupingFactor, comparisonPeriod]);

  const handleGroupingChange = (e) => {
    setGroupingFactor(e.target.value);
  };
  
  const handlePeriodChange = (e) => {
    setComparisonPeriod(e.target.value);
  };
  
  const getChangeColor = (percentageChange) => {
    if (percentageChange > 5) return 'text-success';
    if (percentageChange < -5) return 'text-danger';
    return 'text-muted';
  };
  
  const getChangeIcon = (percentageChange) => {
    if (percentageChange > 0) return '▲';
    if (percentageChange < 0) return '▼';
    return '—';
  };
  
  const getRecommendationBadge = (type) => {
    switch(type.toLowerCase()) {
      case 'increase':
        return <span className="recommendation-badge increase">Increase</span>;
      case 'decrease':
        return <span className="recommendation-badge decrease">Decrease</span>;
      default:
        return <span className="recommendation-badge maintain">Maintain</span>;
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
      <h2>Inventory Mix Analysis</h2>
      <p>Analyze your inventory composition to optimize stock levels and improve turnover.</p>
      
      <Row className="mb-4">
        <Col md={3}>
          <Card>
            <Card.Header>Analysis Parameters</Card.Header>
            <Card.Body>
              <Form>
                <Form.Group className="mb-3">
                  <Form.Label>Group By</Form.Label>
                  <Form.Select 
                    value={groupingFactor}
                    onChange={handleGroupingChange}
                  >
                    {groupingOptions.map(option => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
                
                <Form.Group className="mb-3">
                  <Form.Label>Compare To</Form.Label>
                  <Form.Select 
                    value={comparisonPeriod}
                    onChange={handlePeriodChange}
                  >
                    {periodOptions.map(option => (
                      <option key={option.value} value={option.value}>{option.label}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
              </Form>
            </Card.Body>
          </Card>
          
          {mixData && mixData.recommendations && mixData.recommendations.length > 0 && (
            <Card className="mt-3">
              <Card.Header>Recommendations</Card.Header>
              <Card.Body>
                <div className="recommendations-list">
                  {mixData.recommendations.map((rec, index) => (
                    <Card key={index} className="recommendation-item mb-3">
                      <Card.Body>
                        <div className="d-flex justify-content-between align-items-center mb-2">
                          <h6 className="mb-0">{rec.group}</h6>
                          {getRecommendationBadge(rec.recommendationType)}
                        </div>
                        <p className="recommendation-reason">{rec.reason}</p>
                        <div className="recommendation-metric">
                          <div className="metric-label">Current</div>
                          <div className="metric-value">
                            {mixData.currentMix.find(m => m.group === rec.group)?.percentage.toFixed(1)}%
                          </div>
                        </div>
                        <div className="recommendation-metric">
                          <div className="metric-label">Ideal</div>
                          <div className="metric-value">{rec.idealPercentage.toFixed(1)}%</div>
                        </div>
                      </Card.Body>
                    </Card>
                  ))}
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
              <p className="mt-3">Analyzing inventory mix data...</p>
            </div>
          ) : !mixData ? (
            <Alert variant="warning">
              No inventory mix data available.
            </Alert>
          ) : (
            <>
              <Card className="mb-4">
                <Card.Header>Inventory Mix Comparison</Card.Header>
                <Card.Body>
                  <Tabs defaultActiveKey="table" className="mb-3">
                    <Tab eventKey="table" title="Table View">
                      <div className="table-responsive">
                        <Table striped bordered hover>
                          <thead>
                            <tr>
                              <th>{groupingOptions.find(g => g.value === groupingFactor)?.label}</th>
                              <th colSpan="2">Current Inventory</th>
                              <th colSpan="2">{periodOptions.find(p => p.value === comparisonPeriod)?.label}</th>
                              <th>% Change</th>
                              <th>Turnover</th>
                            </tr>
                          </thead>
                          <tbody>
                            {mixData.currentMix.sort((a, b) => b.count - a.count).map((item, index) => {
                              const comparisonItem = mixData.comparisonMix.find(c => c.group === item.group);
                              
                              return (
                                <tr key={index}>
                                  <td>{item.group}</td>
                                  <td>{item.count}</td>
                                  <td>{item.percentage.toFixed(1)}%</td>
                                  <td>{comparisonItem?.count || 0}</td>
                                  <td>{(comparisonItem?.percentage || 0).toFixed(1)}%</td>
                                  <td className={getChangeColor(item.percentageChange)}>
                                    {getChangeIcon(item.percentageChange)} {item.percentageChange.toFixed(1)}%
                                  </td>
                                  <td>{item.averageTurnover.toFixed(1)} days</td>
                                </tr>
                              );
                            })}
                          </tbody>
                        </Table>
                      </div>
                    </Tab>
                    <Tab eventKey="chart" title="Chart View">
                      <div className="chart-container">
                        <div className="text-center py-5">
                          {/* In a real implementation, a proper chart would be rendered here */}
                          <p>Inventory mix comparison chart would be displayed here</p>
                          <p>Current items: {mixData.currentMix.length}</p>
                          <p>Comparison items: {mixData.comparisonMix.length}</p>
                        </div>
                      </div>
                    </Tab>
                  </Tabs>
                </Card.Body>
              </Card>
              
              <Card>
                <Card.Header>Sales Trend Analysis</Card.Header>
                <Card.Body>
                  <Tabs defaultActiveKey="trends" className="mb-3">
                    <Tab eventKey="trends" title="Sales Trends">
                      <div className="trends-chart-container">
                        <div className="text-center py-5">
                          {/* In a real implementation, a proper chart would be rendered here */}
                          <p>Sales trend chart would be displayed here</p>
                          <p>Number of groups: {mixData.salesTrendByGroup.length}</p>
                        </div>
                      </div>
                    </Tab>
                    <Tab eventKey="turnover" title="Turnover Analysis">
                      <div className="turnover-analysis">
                        <Row>
                          {mixData.currentMix
                            .sort((a, b) => a.averageTurnover - b.averageTurnover)
                            .slice(0, 3)
                            .map((item, index) => (
                              <Col md={4} key={index}>
                                <Card className="turnover-card fast-turnover">
                                  <Card.Body className="text-center">
                                    <div className="turnover-label">Fast Turnover</div>
                                    <div className="turnover-group">{item.group}</div>
                                    <div className="turnover-value">{item.averageTurnover.toFixed(1)} days</div>
                                    <div className="inventory-count">{item.count} in stock</div>
                                  </Card.Body>
                                </Card>
                              </Col>
                            ))}
                        </Row>
                        <Row className="mt-4">
                          {mixData.currentMix
                            .sort((a, b) => b.averageTurnover - a.averageTurnover)
                            .slice(0, 3)
                            .map((item, index) => (
                              <Col md={4} key={index}>
                                <Card className="turnover-card slow-turnover">
                                  <Card.Body className="text-center">
                                    <div className="turnover-label">Slow Turnover</div>
                                    <div className="turnover-group">{item.group}</div>
                                    <div className="turnover-value">{item.averageTurnover.toFixed(1)} days</div>
                                    <div className="inventory-count">{item.count} in stock</div>
                                  </Card.Body>
                                </Card>
                              </Col>
                            ))}
                        </Row>
                      </div>
                    </Tab>
                  </Tabs>
                </Card.Body>
              </Card>
            </>
          )}
        </Col>
      </Row>
    </Container>
  );
};

export default InventoryMixAnalysis;
