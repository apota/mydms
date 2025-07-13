import React, { useState, useEffect } from 'react';
import {
  Container, Row, Col, Card, Button, Spinner, Alert, 
  ProgressBar, Badge, Table, Nav, Tab, Form
} from 'react-bootstrap';
import { 
  FaChartLine, FaUserFriends, FaCalendarAlt, FaCog, FaFilter,
  FaRobot, FaBullseye, FaExclamationTriangle, FaClipboardCheck,
  FaDownload, FaSync, FaArrowUp, FaArrowDown
} from 'react-icons/fa';
import { AIAnalyticsService } from '../../services/api-services';

// Mock chart data
const getChartData = () => ({
  customerAcquisition: {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    values: [120, 132, 145, 140, 160, 175],
    predicted: [175, 190, 205, 220, 230, 245]
  },
  customerRetention: {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    values: [94, 92, 93, 95, 95, 96],
    predicted: [96, 95, 97, 96, 96, 97]
  }
});

const PredictiveAnalytics = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [segments, setSegments] = useState([]);
  const [timeframe, setTimeframe] = useState('monthly');
  const [selectedMetric, setSelectedMetric] = useState('acquisition');
  const [chartData, setChartData] = useState(getChartData());
  
  // Fetch segments data
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        // Fetch segments
        const discoveredSegments = await AIAnalyticsService.discoverCustomerSegments();
        setSegments(discoveredSegments);
        
        // Set chart data
        setChartData(getChartData());
      } catch (err) {
        setError("Failed to load predictive analytics data. Please try again.");
        console.error("Error fetching predictive analytics:", err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, []);

  // Methods for updating the view
  const handleTimeframeChange = (newTimeframe) => {
    setTimeframe(newTimeframe);
    // In a real app, would reload data for the new timeframe
  };
  
  const handleMetricChange = (newMetric) => {
    setSelectedMetric(newMetric);
    // In a real app, would update the chart data for the new metric
  };
  
  if (loading) {
    return (
      <Container fluid className="py-4">
        <Card className="shadow-sm">
          <Card.Body className="text-center p-5">
            <Spinner animation="border" role="status">
              <span className="visually-hidden">Loading...</span>
            </Spinner>
            <p className="mt-3 text-muted">Loading predictive analytics...</p>
          </Card.Body>
        </Card>
      </Container>
    );
  }
  
  if (error) {
    return (
      <Container fluid className="py-4">
        <Alert variant="danger">{error}</Alert>
      </Container>
    );
  }
  
  // Helper methods for rendering
  const trendArrow = (value) => {
    if (value > 0) return <FaArrowUp className="text-success" />;
    if (value < 0) return <FaArrowDown className="text-danger" />;
    return null;
  };
  
  return (
    <Container fluid className="py-4">
      <Row className="mb-4">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <h2>
              <FaRobot className="me-2 text-primary" /> Predictive Analytics Dashboard
            </h2>
            <div>
              <Button variant="outline-secondary" className="me-2">
                <FaDownload className="me-1" /> Export
              </Button>
              <Button variant="primary">
                <FaSync className="me-1" /> Refresh Predictions
              </Button>
            </div>
          </div>
          <p className="text-muted">
            AI-powered insights and predictions to help optimize your dealership operations
          </p>
        </Col>
      </Row>
      
      {/* Key Metrics */}
      <Row className="mb-4">
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-primary">92%</h3>
              <p className="text-muted mb-0">Prediction Accuracy</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-success">
                8.2% <small className="text-success">{trendArrow(1)}</small>
              </h3>
              <p className="text-muted mb-0">Forecasted Growth</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-warning">
                5.3% <small className="text-danger">{trendArrow(-1)}</small>
              </h3>
              <p className="text-muted mb-0">Predicted Churn Rate</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-info">
                $2,850 <small className="text-success">{trendArrow(1)}</small>
              </h3>
              <p className="text-muted mb-0">Avg. Customer LTV</p>
            </Card.Body>
          </Card>
        </Col>
      </Row>
      
      {/* Main content */}
      <Row>
        <Col md={8}>
          <Card className="shadow-sm mb-4">
            <Card.Header className="bg-white">
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">Forecast & Trends</h5>
                <div>
                  <Form.Select 
                    size="sm" 
                    style={{ width: 'auto', display: 'inline-block' }}
                    value={selectedMetric}
                    onChange={(e) => handleMetricChange(e.target.value)}
                    className="me-2"
                  >
                    <option value="acquisition">Customer Acquisition</option>
                    <option value="retention">Customer Retention</option>
                    <option value="revenue">Revenue Forecast</option>
                    <option value="service">Service Appointments</option>
                  </Form.Select>
                  
                  <div className="btn-group">
                    <Button 
                      variant={timeframe === 'weekly' ? 'primary' : 'outline-primary'} 
                      size="sm"
                      onClick={() => handleTimeframeChange('weekly')}
                    >
                      Weekly
                    </Button>
                    <Button 
                      variant={timeframe === 'monthly' ? 'primary' : 'outline-primary'} 
                      size="sm"
                      onClick={() => handleTimeframeChange('monthly')}
                    >
                      Monthly
                    </Button>
                    <Button 
                      variant={timeframe === 'quarterly' ? 'primary' : 'outline-primary'} 
                      size="sm"
                      onClick={() => handleTimeframeChange('quarterly')}
                    >
                      Quarterly
                    </Button>
                  </div>
                </div>
              </div>
            </Card.Header>
            <Card.Body>
              {/* In a real app, this would be a Chart.js or similar visualization */}
              <div className="chart-placeholder bg-light p-4 text-center" style={{ height: '300px' }}>
                <p className="mb-3">
                  {selectedMetric === 'acquisition' ? 'Customer Acquisition Forecast' : 'Customer Retention Forecast'}
                </p>
                <div className="d-flex justify-content-between">
                  {(selectedMetric === 'acquisition' ? 
                    chartData.customerAcquisition.labels : 
                    chartData.customerRetention.labels).map((month, idx) => (
                    <div key={idx} className="d-flex flex-column align-items-center">
                      <div className="chart-bar bg-primary mb-1" 
                        style={{ 
                          height: (selectedMetric === 'acquisition' ? 
                            chartData.customerAcquisition.values[idx] / 2.5 : 
                            chartData.customerRetention.values[idx]) + 'px',
                          width: '20px'
                        }}>
                      </div>
                      <div className="chart-bar bg-info opacity-75" 
                        style={{ 
                          height: (selectedMetric === 'acquisition' ? 
                            chartData.customerAcquisition.predicted[idx] / 2.5 : 
                            chartData.customerRetention.predicted[idx]) + 'px',
                          width: '20px'
                        }}>
                      </div>
                      <small>{month}</small>
                    </div>
                  ))}
                </div>
                <div className="mt-3">
                  <span className="me-3">
                    <span className="bg-primary d-inline-block me-1" style={{ width: '12px', height: '12px' }}></span>
                    Actual
                  </span>
                  <span>
                    <span className="bg-info opacity-75 d-inline-block me-1" style={{ width: '12px', height: '12px' }}></span>
                    Predicted
                  </span>
                </div>
              </div>
              
              <div className="mt-4">
                <h6>Key Insights</h6>
                <ul>
                  <li>Predicted {selectedMetric === 'acquisition' ? '12%' : '2%'} growth over the next quarter</li>
                  <li>Seasonal trends suggest higher {selectedMetric === 'acquisition' ? 'acquisition' : 'retention'} in Q4</li>
                  <li>Current trajectory exceeds annual target by {selectedMetric === 'acquisition' ? '8%' : '3%'}</li>
                </ul>
              </div>
            </Card.Body>
          </Card>
          
          <Card className="shadow-sm mb-4">
            <Card.Header className="bg-white">
              <h5 className="mb-0">
                <FaExclamationTriangle className="me-2 text-warning" /> Risk Detection
              </h5>
            </Card.Header>
            <Card.Body>
              <Table responsive hover>
                <thead className="table-light">
                  <tr>
                    <th>Risk Category</th>
                    <th>Probability</th>
                    <th>Impact</th>
                    <th>Recommended Action</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>High churn in luxury vehicle segment</td>
                    <td>
                      <Badge bg="danger">High (75%)</Badge>
                    </td>
                    <td>Potential $450k revenue loss</td>
                    <td>Launch targeted loyalty program for premium customers</td>
                  </tr>
                  <tr>
                    <td>Service capacity shortage in Q3</td>
                    <td>
                      <Badge bg="warning">Medium (45%)</Badge>
                    </td>
                    <td>Customer satisfaction decline</td>
                    <td>Temporarily increase service staff during peak periods</td>
                  </tr>
                  <tr>
                    <td>Parts inventory shortfall</td>
                    <td>
                      <Badge bg="info">Low (15%)</Badge>
                    </td>
                    <td>Delayed repairs</td>
                    <td>Review inventory management system</td>
                  </tr>
                </tbody>
              </Table>
            </Card.Body>
          </Card>
        </Col>
        
        <Col md={4}>
          <Card className="shadow-sm mb-4">
            <Card.Header className="bg-white">
              <h5 className="mb-0">
                <FaUserFriends className="me-2 text-primary" /> AI-Discovered Segments
              </h5>
            </Card.Header>
            <Card.Body>
              {segments.map((segment, idx) => (
                <div key={idx} className="mb-3 pb-3 border-bottom">
                  <div className="d-flex justify-content-between">
                    <h6>{segment.name}</h6>
                    <Badge bg="primary" pill>{segment.size} customers</Badge>
                  </div>
                  <p className="text-muted small mb-1">{segment.description}</p>
                  <small>
                    <strong>Avg. Value:</strong> ${segment.averageValue.toLocaleString()}
                  </small>
                </div>
              ))}
              
              <div className="text-center mt-4">
                <Button variant="outline-primary" size="sm">
                  View All Segments
                </Button>
              </div>
            </Card.Body>
          </Card>
          
          <Card className="shadow-sm mb-4">
            <Card.Header className="bg-white">
              <h5 className="mb-0">
                <FaBullseye className="me-2 text-danger" /> Opportunity Alerts
              </h5>
            </Card.Header>
            <Card.Body>
              <Alert variant="success" className="mb-3 d-flex align-items-start">
                <div>
                  <strong>Service Due Reminder Campaign</strong>
                  <p className="mb-1 small">352 customers due for service in next 30 days</p>
                  <small>Potential Revenue: $105,600</small>
                </div>
                <Button variant="success" size="sm" className="ms-auto">
                  Create Campaign
                </Button>
              </Alert>
              
              <Alert variant="info" className="mb-3 d-flex align-items-start">
                <div>
                  <strong>Vehicle Upgrade Opportunity</strong>
                  <p className="mb-1 small">215 customers with vehicles 4+ years old</p>
                  <small>Estimated Conversion: 8-12%</small>
                </div>
                <Button variant="info" size="sm" className="ms-auto">
                  View Customers
                </Button>
              </Alert>
              
              <Alert variant="warning" className="mb-0 d-flex align-items-start">
                <div>
                  <strong>Loyalty Program Upsell</strong>
                  <p className="mb-1 small">189 regular customers not in loyalty program</p>
                  <small>Potential Loyalty Signups: 35%</small>
                </div>
                <Button variant="warning" size="sm" className="ms-auto">
                  Create Offer
                </Button>
              </Alert>
            </Card.Body>
          </Card>
          
          <Card className="shadow-sm mb-4">
            <Card.Header className="bg-white">
              <h5 className="mb-0">
                <FaClipboardCheck className="me-2 text-success" /> Model Performance
              </h5>
            </Card.Header>
            <Card.Body>
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <span>Churn Prediction</span>
                  <span>92% accuracy</span>
                </div>
                <ProgressBar variant="success" now={92} />
              </div>
              
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <span>Customer Segmentation</span>
                  <span>89% accuracy</span>
                </div>
                <ProgressBar variant="success" now={89} />
              </div>
              
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <span>Revenue Forecast</span>
                  <span>87% accuracy</span>
                </div>
                <ProgressBar variant="info" now={87} />
              </div>
              
              <div className="mb-3">
                <div className="d-flex justify-content-between mb-1">
                  <span>Sentiment Analysis</span>
                  <span>85% accuracy</span>
                </div>
                <ProgressBar variant="info" now={85} />
              </div>
              
              <div>
                <div className="d-flex justify-content-between mb-1">
                  <span>Next Best Action</span>
                  <span>81% accuracy</span>
                </div>
                <ProgressBar variant="info" now={81} />
              </div>
              
              <div className="text-center mt-4">
                <small className="text-muted">Last model training: 2023-06-20</small>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default PredictiveAnalytics;
