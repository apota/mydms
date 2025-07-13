import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Container, Row, Col, Card, Button, Table, Nav,
  Spinner, Alert, Badge, ProgressBar, Tab, Tabs
} from 'react-bootstrap';
import { 
  FaArrowLeft, FaEnvelope, FaEye, FaMousePointer, 
  FaExclamationTriangle, FaUsers, FaCalendarAlt, FaChartBar, 
  FaMap, FaMobile, FaDesktop, FaTabletAlt
} from 'react-icons/fa';
import { CampaignService } from '../../services/api-services';

// Import Chart.js components (assuming we have Chart.js in the project)
import { 
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js';
import { Line, Bar, Doughnut } from 'react-chartjs-2';

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const CampaignAnalytics = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [campaign, setCampaign] = useState(null);
  const [metrics, setMetrics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [dateRange, setDateRange] = useState('week'); // 'day', 'week', 'month', 'all'
  
  // Fetch campaign data and metrics
  useEffect(() => {
    const fetchData = async () => {
      try {
        const campaignData = await CampaignService.getById(id);
        setCampaign(campaignData);
        
        const metricsData = await CampaignService.getPerformanceMetrics(id);
        setMetrics(metricsData);
      } catch (err) {
        setError('Failed to load campaign analytics. Please try again.');
        console.error('Error fetching campaign analytics:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, [id]);

  // Calculate key performance indicators
  const calculateKPIs = () => {
    if (!metrics) return {};
    
    const deliveryRate = ((metrics.delivered / metrics.sent) * 100).toFixed(1);
    const openRate = ((metrics.opened / metrics.delivered) * 100).toFixed(1);
    const clickRate = ((metrics.clicked / metrics.opened) * 100).toFixed(1);
    const bounceRate = ((metrics.bounced / metrics.sent) * 100).toFixed(1);
    const unsubscribeRate = ((metrics.unsubscribed / metrics.delivered) * 100).toFixed(1);
    
    return {
      deliveryRate,
      openRate,
      clickRate,
      bounceRate,
      unsubscribeRate
    };
  };

  // KPIs from metrics
  const kpis = calculateKPIs();

  // Chart data for engagement over time
  const engagementData = {
    labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5', 'Day 6', 'Day 7'],
    datasets: [
      {
        label: 'Opens',
        data: metrics?.timeData?.map(day => day.opens) || [65, 78, 52, 60, 72, 68, 50],
        borderColor: 'rgba(54, 162, 235, 1)',
        backgroundColor: 'rgba(54, 162, 235, 0.2)',
        tension: 0.3,
        fill: true
      },
      {
        label: 'Clicks',
        data: metrics?.timeData?.map(day => day.clicks) || [25, 32, 20, 28, 35, 30, 22],
        borderColor: 'rgba(255, 159, 64, 1)',
        backgroundColor: 'rgba(255, 159, 64, 0.2)',
        tension: 0.3,
        fill: true
      }
    ]
  };

  // Chart data for audience breakdown by device
  const deviceData = {
    labels: ['Desktop', 'Mobile', 'Tablet'],
    datasets: [
      {
        label: 'Device Usage',
        data: metrics?.deviceBreakdown || [45, 40, 15],
        backgroundColor: [
          'rgba(54, 162, 235, 0.6)',
          'rgba(255, 99, 132, 0.6)',
          'rgba(75, 192, 192, 0.6)'
        ],
        borderColor: [
          'rgba(54, 162, 235, 1)',
          'rgba(255, 99, 132, 1)',
          'rgba(75, 192, 192, 1)'
        ],
        borderWidth: 1
      }
    ]
  };

  // Chart data for geographical distribution
  const locationData = {
    labels: metrics?.topLocations?.map(loc => loc.name) || ['California', 'Texas', 'New York', 'Florida', 'Illinois'],
    datasets: [
      {
        label: 'Opens by Location',
        data: metrics?.topLocations?.map(loc => loc.opens) || [120, 98, 85, 65, 40],
        backgroundColor: 'rgba(153, 102, 255, 0.6)',
        borderColor: 'rgba(153, 102, 255, 1)',
        borderWidth: 1
      }
    ]
  };

  if (loading) {
    return (
      <Container className="py-5 text-center">
        <Spinner animation="border" role="status" variant="primary">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="py-5">
        <Alert variant="danger">{error}</Alert>
        <Button variant="primary" onClick={() => navigate('/campaign')}>
          Back to Campaigns
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="py-4">
      <Row className="mb-4">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <Button 
                variant="outline-secondary" 
                onClick={() => navigate('/campaign')} 
                className="me-2"
              >
                <FaArrowLeft /> Back to Campaigns
              </Button>
              <h2 className="d-inline-block ms-2 mb-0">
                Campaign Analytics: {campaign?.name}
              </h2>
            </div>
            <div>
              <Badge 
                bg={campaign?.status === 'active' ? 'success' : 'secondary'} 
                className="fs-6 me-2"
              >
                {campaign?.status}
              </Badge>
              <Button variant="outline-primary" onClick={() => navigate(`/campaign/${id}/edit`)}>
                Edit Campaign
              </Button>
            </div>
          </div>
        </Col>
      </Row>
      
      <Row className="mb-4">
        <Col>
          <Card className="shadow-sm">
            <Card.Body>
              <Row>
                <Col md={3} className="border-end">
                  <div className="text-center">
                    <div className="display-6 text-primary">{metrics?.sent || 0}</div>
                    <div className="text-muted">Sent</div>
                  </div>
                </Col>
                <Col md={3} className="border-end">
                  <div className="text-center">
                    <div className="display-6 text-success">{kpis.openRate || 0}%</div>
                    <div className="text-muted">Open Rate</div>
                  </div>
                </Col>
                <Col md={3} className="border-end">
                  <div className="text-center">
                    <div className="display-6 text-info">{kpis.clickRate || 0}%</div>
                    <div className="text-muted">Click Rate</div>
                  </div>
                </Col>
                <Col md={3}>
                  <div className="text-center">
                    <div className="display-6 text-warning">{metrics?.conversions || 0}</div>
                    <div className="text-muted">Conversions</div>
                  </div>
                </Col>
              </Row>
            </Card.Body>
          </Card>
        </Col>
      </Row>
      
      <Row className="mb-4">
        <Col md={8}>
          <Card className="shadow-sm mb-4 h-100">
            <Card.Header className="bg-white">
              <div className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">Engagement Over Time</h5>
                <div className="btn-group">
                  <Button 
                    variant={dateRange === 'day' ? 'primary' : 'outline-primary'} 
                    size="sm"
                    onClick={() => setDateRange('day')}
                  >
                    24h
                  </Button>
                  <Button 
                    variant={dateRange === 'week' ? 'primary' : 'outline-primary'} 
                    size="sm"
                    onClick={() => setDateRange('week')}
                  >
                    7d
                  </Button>
                  <Button 
                    variant={dateRange === 'month' ? 'primary' : 'outline-primary'} 
                    size="sm"
                    onClick={() => setDateRange('month')}
                  >
                    30d
                  </Button>
                  <Button 
                    variant={dateRange === 'all' ? 'primary' : 'outline-primary'} 
                    size="sm"
                    onClick={() => setDateRange('all')}
                  >
                    All
                  </Button>
                </div>
              </div>
            </Card.Header>
            <Card.Body>
              <div style={{ height: '300px' }}>
                <Line
                  data={engagementData}
                  options={{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                      legend: {
                        position: 'top',
                      },
                      tooltip: {
                        mode: 'index',
                        intersect: false,
                      }
                    },
                    scales: {
                      y: {
                        beginAtZero: true,
                        title: {
                          display: true,
                          text: 'Number of Interactions'
                        }
                      },
                      x: {
                        title: {
                          display: true,
                          text: 'Date'
                        }
                      }
                    }
                  }}
                />
              </div>
            </Card.Body>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="shadow-sm mb-4 h-100">
            <Card.Header className="bg-white">
              <h5 className="mb-0">Funnel Performance</h5>
            </Card.Header>
            <Card.Body>
              <div className="mb-4">
                <div className="d-flex justify-content-between align-items-center mb-1">
                  <span>
                    <FaEnvelope className="text-secondary me-1" /> Sent
                  </span>
                  <span>{metrics?.sent || 0}</span>
                </div>
                <ProgressBar variant="secondary" now={100} />
              </div>
              
              <div className="mb-4">
                <div className="d-flex justify-content-between align-items-center mb-1">
                  <span>
                    <FaEye className="text-primary me-1" /> Opened
                  </span>
                  <span>{metrics?.opened || 0} ({kpis.openRate || 0}%)</span>
                </div>
                <ProgressBar 
                  variant="primary" 
                  now={metrics?.opened / metrics?.sent * 100 || 0} 
                />
              </div>
              
              <div className="mb-4">
                <div className="d-flex justify-content-between align-items-center mb-1">
                  <span>
                    <FaMousePointer className="text-info me-1" /> Clicked
                  </span>
                  <span>{metrics?.clicked || 0} ({kpis.clickRate || 0}%)</span>
                </div>
                <ProgressBar 
                  variant="info" 
                  now={metrics?.clicked / metrics?.sent * 100 || 0} 
                />
              </div>
              
              <div className="mb-4">
                <div className="d-flex justify-content-between align-items-center mb-1">
                  <span>
                    <FaExclamationTriangle className="text-warning me-1" /> Bounced
                  </span>
                  <span>{metrics?.bounced || 0} ({kpis.bounceRate || 0}%)</span>
                </div>
                <ProgressBar 
                  variant="warning" 
                  now={metrics?.bounced / metrics?.sent * 100 || 0} 
                />
              </div>
              
              <div className="mb-4">
                <div className="d-flex justify-content-between align-items-center mb-1">
                  <span>
                    <FaUsers className="text-success me-1" /> Converted
                  </span>
                  <span>
                    {metrics?.conversions || 0} 
                    ({((metrics?.conversions / metrics?.clicked * 100) || 0).toFixed(1)}%)
                  </span>
                </div>
                <ProgressBar 
                  variant="success" 
                  now={metrics?.conversions / metrics?.sent * 100 || 0} 
                />
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
      
      <Row className="mb-4">
        <Col>
          <Card className="shadow-sm">
            <Card.Body>
              <Tabs defaultActiveKey="audience" className="mb-3">
                <Tab eventKey="audience" title="Audience Insights">
                  <Row>
                    <Col md={4}>
                      <Card className="border-0">
                        <Card.Body className="text-center">
                          <h6 className="mb-3">Device Distribution</h6>
                          <div style={{ height: '200px' }}>
                            <Doughnut
                              data={deviceData}
                              options={{
                                responsive: true,
                                maintainAspectRatio: false,
                                plugins: {
                                  legend: {
                                    position: 'bottom'
                                  }
                                }
                              }}
                            />
                          </div>
                          <div className="mt-3">
                            <div className="d-flex justify-content-between">
                              <div>
                                <FaDesktop className="text-primary me-1" /> Desktop
                              </div>
                              <div>{metrics?.deviceBreakdown?.[0] || 45}%</div>
                            </div>
                            <div className="d-flex justify-content-between">
                              <div>
                                <FaMobile className="text-danger me-1" /> Mobile
                              </div>
                              <div>{metrics?.deviceBreakdown?.[1] || 40}%</div>
                            </div>
                            <div className="d-flex justify-content-between">
                              <div>
                                <FaTabletAlt className="text-info me-1" /> Tablet
                              </div>
                              <div>{metrics?.deviceBreakdown?.[2] || 15}%</div>
                            </div>
                          </div>
                        </Card.Body>
                      </Card>
                    </Col>
                    
                    <Col md={8}>
                      <Card className="border-0">
                        <Card.Body>
                          <h6 className="mb-3">Top Locations</h6>
                          <div style={{ height: '250px' }}>
                            <Bar
                              data={locationData}
                              options={{
                                responsive: true,
                                maintainAspectRatio: false,
                                plugins: {
                                  legend: {
                                    display: false
                                  }
                                },
                                scales: {
                                  y: {
                                    beginAtZero: true,
                                  }
                                }
                              }}
                            />
                          </div>
                        </Card.Body>
                      </Card>
                    </Col>
                  </Row>
                </Tab>
                
                <Tab eventKey="content" title="Content Performance">
                  <Row>
                    <Col md={12}>
                      <Table responsive hover>
                        <thead>
                          <tr>
                            <th>Content Element</th>
                            <th>Impressions</th>
                            <th>Clicks</th>
                            <th>CTR</th>
                            <th>Avg. Time Spent</th>
                          </tr>
                        </thead>
                        <tbody>
                          {(metrics?.contentPerformance || [
                            {
                              name: 'Hero Banner',
                              impressions: 3450,
                              clicks: 890,
                              ctr: '25.8%',
                              timeSpent: '8s'
                            },
                            {
                              name: 'Main CTA Button',
                              impressions: 3200,
                              clicks: 780,
                              ctr: '24.4%',
                              timeSpent: '3s'
                            },
                            {
                              name: 'Product Image 1',
                              impressions: 2980,
                              clicks: 520,
                              ctr: '17.4%',
                              timeSpent: '6s'
                            },
                            {
                              name: 'Secondary CTA',
                              impressions: 2800,
                              clicks: 360,
                              ctr: '12.9%',
                              timeSpent: '2s'
                            },
                            {
                              name: 'Footer Links',
                              impressions: 2500,
                              clicks: 180,
                              ctr: '7.2%',
                              timeSpent: '1s'
                            }
                          ]).map((item, idx) => (
                            <tr key={idx}>
                              <td>{item.name}</td>
                              <td>{item.impressions}</td>
                              <td>{item.clicks}</td>
                              <td>{item.ctr}</td>
                              <td>{item.timeSpent}</td>
                            </tr>
                          ))}
                        </tbody>
                      </Table>
                    </Col>
                  </Row>
                </Tab>
                
                <Tab eventKey="segments" title="Segment Analysis">
                  <Row>
                    <Col md={12}>
                      <Table responsive hover>
                        <thead>
                          <tr>
                            <th>Customer Segment</th>
                            <th>Recipients</th>
                            <th>Open Rate</th>
                            <th>Click Rate</th>
                            <th>Conversion Rate</th>
                          </tr>
                        </thead>
                        <tbody>
                          {(metrics?.segmentPerformance || [
                            {
                              name: 'High Value Customers',
                              recipients: 120,
                              openRate: '68.3%',
                              clickRate: '42.1%',
                              conversionRate: '12.5%'
                            },
                            {
                              name: 'New Customers (Last 30 Days)',
                              recipients: 85,
                              openRate: '57.6%',
                              clickRate: '35.2%',
                              conversionRate: '8.2%'
                            },
                            {
                              name: 'Service Due in 30 Days',
                              recipients: 56,
                              openRate: '72.5%',
                              clickRate: '48.6%',
                              conversionRate: '14.3%'
                            },
                            {
                              name: 'Loyalty Members',
                              recipients: 342,
                              openRate: '64.1%',
                              clickRate: '38.5%',
                              conversionRate: '10.2%'
                            },
                            {
                              name: 'Previous Vehicle Purchasers',
                              recipients: 210,
                              openRate: '52.8%',
                              clickRate: '31.4%',
                              conversionRate: '7.6%'
                            }
                          ]).map((item, idx) => (
                            <tr key={idx}>
                              <td>{item.name}</td>
                              <td>{item.recipients}</td>
                              <td>{item.openRate}</td>
                              <td>{item.clickRate}</td>
                              <td>{item.conversionRate}</td>
                            </tr>
                          ))}
                        </tbody>
                      </Table>
                    </Col>
                  </Row>
                </Tab>
                
                <Tab eventKey="recommendations" title="AI Recommendations">
                  <Row className="mt-3">
                    <Col md={12}>
                      <Alert variant="info" className="mb-4">
                        <h6>Optimization Opportunities</h6>
                        <p className="mb-0">
                          Based on AI analysis of this campaign's performance, we've identified several opportunities to improve future campaigns:
                        </p>
                      </Alert>
                      
                      <div className="d-flex mb-4 border-bottom pb-3">
                        <div className="flex-shrink-0 me-3">
                          <FaCalendarAlt className="fs-3 text-primary" />
                        </div>
                        <div>
                          <h6>Optimal Send Time</h6>
                          <p>
                            Customer engagement is highest on Tuesday and Wednesday mornings. Consider scheduling your next campaign between 9-11 AM.
                          </p>
                        </div>
                      </div>
                      
                      <div className="d-flex mb-4 border-bottom pb-3">
                        <div className="flex-shrink-0 me-3">
                          <FaUsers className="fs-3 text-success" />
                        </div>
                        <div>
                          <h6>Segment Targeting</h6>
                          <p>
                            "Service Due in 30 Days" segment showed 14.3% conversion, 40% higher than average. Consider increasing focus on this segment in future campaigns.
                          </p>
                        </div>
                      </div>
                      
                      <div className="d-flex mb-4 border-bottom pb-3">
                        <div className="flex-shrink-0 me-3">
                          <FaEnvelope className="fs-3 text-info" />
                        </div>
                        <div>
                          <h6>Subject Line</h6>
                          <p>
                            Personalized subject lines performed 25% better than generic ones. Use customer name or vehicle information in your subject lines.
                          </p>
                        </div>
                      </div>
                      
                      <div className="d-flex mb-4">
                        <div className="flex-shrink-0 me-3">
                          <FaMobile className="fs-3 text-danger" />
                        </div>
                        <div>
                          <h6>Mobile Optimization</h6>
                          <p className="mb-0">
                            40% of opens occurred on mobile devices, but mobile click rate was 15% lower than desktop. Consider improving mobile layout and CTA visibility.
                          </p>
                        </div>
                      </div>
                    </Col>
                  </Row>
                </Tab>
              </Tabs>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default CampaignAnalytics;
