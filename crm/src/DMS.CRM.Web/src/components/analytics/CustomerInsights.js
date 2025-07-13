import React, { useState, useEffect } from 'react';
import {
  Container, Row, Col, Card, Button, Spinner, Alert,
  Table, Badge, ProgressBar, Nav, Tab
} from 'react-bootstrap';
import { 
  FaUserAlt, FaExchangeAlt, FaChartLine, FaSearch,
  FaCalendarAlt, FaMailBulk, FaUserFriends, FaAngleDoubleUp,
  FaRobot, FaBrain, FaThumbsUp, FaThumbsDown, FaLightbulb
} from 'react-icons/fa';
import { AIAnalyticsService, CustomerService } from '../../services/api-services';

const CustomerInsights = ({ customerId }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [churnPrediction, setChurnPrediction] = useState(null);
  const [nextBestAction, setNextBestAction] = useState(null);
  const [sentimentTrend, setSentimentTrend] = useState(null);
  
  // Fetch customer insights data
  useEffect(() => {
    const fetchInsights = async () => {
      if (!customerId) return;
      
      try {
        setLoading(true);
        
        // Fetch data in parallel
        const [churnData, actionData] = await Promise.all([
          AIAnalyticsService.predictCustomerChurn(customerId),
          AIAnalyticsService.getNextBestAction(customerId)
        ]);
        
        // Generate mock sentiment trend data (in real app, this would come from the API)
        const sentimentData = {
          trend: [
            { month: 'Jan', score: 0.4 },
            { month: 'Feb', score: 0.3 },
            { month: 'Mar', score: 0.5 },
            { month: 'Apr', score: 0.2 },
            { month: 'May', score: 0.7 },
            { month: 'Jun', score: 0.8 }
          ],
          recentFeedback: [
            { 
              date: '2023-06-15', 
              text: "Really satisfied with my recent service experience. The staff was professional and quick.",
              score: 0.8,
              source: "Post-service survey"
            },
            { 
              date: '2023-05-20', 
              text: "Appointment was delayed by 30 minutes, but the work was done well.",
              score: 0.3,
              source: "Service desk feedback"
            }
          ]
        };
        
        setChurnPrediction(churnData);
        setNextBestAction(actionData);
        setSentimentTrend(sentimentData);
      } catch (err) {
        setError("Failed to load customer insights. Please try again.");
        console.error("Error fetching customer insights:", err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchInsights();
  }, [customerId]);
  
  if (loading) {
    return (
      <Card className="shadow-sm mb-4">
        <Card.Body className="text-center p-5">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Loading...</span>
          </Spinner>
          <p className="mt-3 text-muted">Analyzing customer data...</p>
        </Card.Body>
      </Card>
    );
  }
  
  if (error) {
    return (
      <Alert variant="danger">{error}</Alert>
    );
  }
  
  // Helper function to get badge variant based on churn risk
  const getChurnRiskBadge = (risk) => {
    if (!risk) return <Badge bg="secondary">Unknown</Badge>;
    
    switch (risk.toLowerCase()) {
      case 'high':
        return <Badge bg="danger">High Risk</Badge>;
      case 'medium':
        return <Badge bg="warning">Medium Risk</Badge>;
      case 'low':
        return <Badge bg="success">Low Risk</Badge>;
      default:
        return <Badge bg="secondary">{risk}</Badge>;
    }
  };
  
  // Helper function to get sentiment badge
  const getSentimentBadge = (score) => {
    if (score > 0.5) return <Badge bg="success">Positive</Badge>;
    if (score > 0) return <Badge bg="info">Neutral</Badge>;
    return <Badge bg="danger">Negative</Badge>;
  };
  
  return (
    <Card className="shadow-sm mb-4">
      <Card.Header className="bg-white">
        <div className="d-flex justify-content-between align-items-center">
          <h5 className="mb-0">
            <FaRobot className="me-2 text-primary" /> AI-Powered Customer Insights
          </h5>
          <Button variant="outline-primary" size="sm">
            <FaSearch className="me-1" /> Analyze More Data
          </Button>
        </div>
      </Card.Header>
      <Card.Body>
        <Tab.Container defaultActiveKey="actions">
          <Row>
            <Col md={12}>
              <Nav variant="pills" className="mb-4">
                <Nav.Item>
                  <Nav.Link eventKey="actions">
                    <FaLightbulb className="me-1" /> Recommended Actions
                  </Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="churn">
                    <FaUserFriends className="me-1" /> Churn Analysis
                  </Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="sentiment">
                    <FaBrain className="me-1" /> Sentiment Analysis
                  </Nav.Link>
                </Nav.Item>
              </Nav>
            </Col>
            
            <Col md={12}>
              <Tab.Content>
                <Tab.Pane eventKey="actions">
                  <Row>
                    <Col md={12}>
                      <h6 className="mb-3">Next Best Actions</h6>
                      {nextBestAction?.recommendedActions?.map((action, idx) => (
                        <Card className="mb-3 border-left-accent" key={idx}>
                          <Card.Body>
                            <h6>{action.actionType}</h6>
                            <p className="text-muted mb-2">{action.description}</p>
                            <div className="d-flex justify-content-between align-items-center">
                              <div>
                                <small className="text-muted me-3">
                                  <strong>Expected outcome:</strong> {action.expectedOutcome}
                                </small>
                                <small className="text-muted">
                                  <strong>Timeframe:</strong> {action.timeframe}
                                </small>
                              </div>
                              <div>
                                <Button variant="outline-primary" size="sm" className="me-2">
                                  Execute Action
                                </Button>
                                <Button variant="link" size="sm">
                                  Skip
                                </Button>
                              </div>
                            </div>
                          </Card.Body>
                        </Card>
                      ))}
                    </Col>
                  </Row>
                </Tab.Pane>
                
                <Tab.Pane eventKey="churn">
                  <Row>
                    <Col md={4}>
                      <div className="text-center mb-4">
                        <h1 className="display-4">
                          {Math.round(churnPrediction?.churnProbability * 100) || 0}%
                        </h1>
                        <p className="text-muted mb-1">Churn Probability</p>
                        <div>
                          {getChurnRiskBadge(churnPrediction?.riskLevel)}
                        </div>
                      </div>
                      
                      <div className="text-center pt-3">
                        <p className="mb-2">Prediction Confidence</p>
                        <ProgressBar 
                          now={75} 
                          variant="info" 
                          className="mb-3" 
                          style={{ height: '8px' }}
                        />
                        <small className="text-muted">
                          Last updated: {new Date().toLocaleString()}
                        </small>
                      </div>
                    </Col>
                    
                    <Col md={8}>
                      <h6 className="mb-3">Contributing Factors</h6>
                      <Table hover size="sm" responsive>
                        <thead className="table-light">
                          <tr>
                            <th>Factor</th>
                            <th>Impact</th>
                            <th>Description</th>
                          </tr>
                        </thead>
                        <tbody>
                          {churnPrediction?.contributingFactors?.map((factor, idx) => (
                            <tr key={idx}>
                              <td>{factor.factorName}</td>
                              <td>
                                <ProgressBar 
                                  now={factor.importance * 100} 
                                  variant={factor.importance > 0.7 ? "danger" : (factor.importance > 0.4 ? "warning" : "info")} 
                                  style={{ height: '8px' }}
                                />
                              </td>
                              <td>{factor.description}</td>
                            </tr>
                          ))}
                        </tbody>
                      </Table>
                      
                      <h6 className="mt-4 mb-3">Recommended Actions to Reduce Churn Risk</h6>
                      <ul className="mb-0">
                        {churnPrediction?.recommendedActions?.map((action, idx) => (
                          <li key={idx}>{action}</li>
                        ))}
                      </ul>
                    </Col>
                  </Row>
                </Tab.Pane>
                
                <Tab.Pane eventKey="sentiment">
                  <Row>
                    <Col md={12}>
                      <div className="d-flex justify-content-between align-items-center mb-3">
                        <h6 className="mb-0">Sentiment Trend (6 Months)</h6>
                        <div>
                          <Badge bg="success" className="me-2">
                            <FaThumbsUp className="me-1" /> Positive: 60%
                          </Badge>
                          <Badge bg="danger">
                            <FaThumbsDown className="me-1" /> Negative: 15%
                          </Badge>
                        </div>
                      </div>
                      
                      {/* In a real application, this would be a chart component */}
                      <div className="sentiment-chart bg-light p-3 mb-4" style={{ height: '200px' }}>
                        <div className="text-center text-muted">
                          [Sentiment Trend Visualization]
                          <div>
                            {sentimentTrend?.trend?.map((point, idx) => (
                              <span key={idx} className="mx-2">
                                {point.month}: {point.score > 0.5 ? '😊' : (point.score > 0 ? '😐' : '😔')}
                              </span>
                            ))}
                          </div>
                        </div>
                      </div>
                      
                      <h6 className="mb-3">Recent Feedback</h6>
                      {sentimentTrend?.recentFeedback?.map((feedback, idx) => (
                        <Card className="mb-3" key={idx}>
                          <Card.Body>
                            <div className="d-flex justify-content-between">
                              <div>
                                <p className="mb-1">{feedback.text}</p>
                                <small className="text-muted">
                                  Source: {feedback.source} | Date: {new Date(feedback.date).toLocaleDateString()}
                                </small>
                              </div>
                              <div className="ms-3">
                                {getSentimentBadge(feedback.score)}
                              </div>
                            </div>
                          </Card.Body>
                        </Card>
                      ))}
                      
                      <h6 className="mb-3 mt-4">Common Topics in Feedback</h6>
                      <div>
                        <Badge bg="primary" className="me-2 mb-2 p-2">Service Quality</Badge>
                        <Badge bg="primary" className="me-2 mb-2 p-2">Staff Friendliness</Badge>
                        <Badge bg="primary" className="me-2 mb-2 p-2">Waiting Time</Badge>
                        <Badge bg="primary" className="me-2 mb-2 p-2">Vehicle Performance</Badge>
                        <Badge bg="primary" className="me-2 mb-2 p-2">Pricing</Badge>
                      </div>
                    </Col>
                  </Row>
                </Tab.Pane>
              </Tab.Content>
            </Col>
          </Row>
        </Tab.Container>
      </Card.Body>
      <Card.Footer className="text-muted bg-white">
        <small>
          <FaRobot className="me-1" /> AI insights are generated based on customer interaction history, feedback, and behavior patterns.
        </small>
      </Card.Footer>
    </Card>
  );
};

export default CustomerInsights;
