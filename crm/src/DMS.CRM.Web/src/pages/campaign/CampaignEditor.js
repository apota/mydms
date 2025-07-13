import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container, Row, Col, Card, Form, Button, Alert,
  Spinner, Tabs, Tab, InputGroup, Badge, ListGroup
} from 'react-bootstrap';
import { FaSave, FaArrowLeft, FaCalendarAlt, FaUsers, FaEnvelope, FaSms, FaLink } from 'react-icons/fa';
import { CampaignService, CustomerService } from '../../services/api-services';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const CampaignEditor = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditMode = !!id;
  
  const [campaign, setCampaign] = useState({
    name: '',
    description: '',
    type: 'email',
    status: 'draft',
    startDate: new Date(),
    endDate: null,
    targetAudience: '',
    content: '',
    subject: '',
    segmentIds: [],
    channelPreferences: {
      email: true,
      sms: false,
      push: false,
      social: false
    }
  });
  
  const [loading, setLoading] = useState(isEditMode);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [successMessage, setSuccessMessage] = useState(null);
  const [customerSegments, setCustomerSegments] = useState([]);
  const [selectedCustomers, setSelectedCustomers] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [activeKey, setActiveKey] = useState('details');

  // Fetch campaign and segments data
  useEffect(() => {
    const fetchData = async () => {
      if (isEditMode) {
        try {
          const campaignData = await CampaignService.getById(id);
          setCampaign(prev => ({
            ...prev,
            ...campaignData,
            startDate: new Date(campaignData.startDate),
            endDate: campaignData.endDate ? new Date(campaignData.endDate) : null
          }));
        } catch (err) {
          setError('Failed to load campaign data. Please try again.');
          console.error('Error fetching campaign:', err);
        } finally {
          setLoading(false);
        }
      }
      
      // Fetch customer segments (simplified - in a real app we'd have a SegmentsService)
      // This is a mock for demonstration
      setCustomerSegments([
        { id: '1', name: 'High Value Customers', count: 120 },
        { id: '2', name: 'New Customers (Last 30 Days)', count: 85 },
        { id: '3', name: 'Service Due in 30 Days', count: 56 },
        { id: '4', name: 'Loyalty Members', count: 342 },
        { id: '5', name: 'Previous Vehicle Purchasers', count: 210 }
      ]);
    };
    
    fetchData();
  }, [id, isEditMode]);

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    
    if (name.includes('.')) {
      // Handle nested properties like channelPreferences.email
      const [parent, child] = name.split('.');
      setCampaign(prev => ({
        ...prev,
        [parent]: {
          ...prev[parent],
          [child]: type === 'checkbox' ? checked : value
        }
      }));
    } else {
      setCampaign(prev => ({
        ...prev,
        [name]: value
      }));
    }
  };

  // Handle date changes
  const handleDateChange = (date, name) => {
    setCampaign(prev => ({
      ...prev,
      [name]: date
    }));
  };

  // Handle segment selection
  const handleSegmentToggle = (segmentId) => {
    setCampaign(prev => {
      const segmentIds = prev.segmentIds || [];
      
      if (segmentIds.includes(segmentId)) {
        return {
          ...prev,
          segmentIds: segmentIds.filter(id => id !== segmentId)
        };
      } else {
        return {
          ...prev,
          segmentIds: [...segmentIds, segmentId]
        };
      }
    });
  };

  // Search for customers
  const handleCustomerSearch = async () => {
    if (searchTerm.length < 2) return;
    
    try {
      const results = await CustomerService.search(searchTerm);
      setSearchResults(results.items || []);
    } catch (err) {
      console.error('Error searching customers:', err);
    }
  };

  // Add/remove a customer
  const toggleCustomerSelection = (customer) => {
    setSelectedCustomers(prev => {
      const exists = prev.some(c => c.id === customer.id);
      
      if (exists) {
        return prev.filter(c => c.id !== customer.id);
      } else {
        return [...prev, customer];
      }
    });
  };

  // Save campaign
  const handleSave = async () => {
    try {
      setSaving(true);
      setError(null);
      
      // Validate form
      if (!campaign.name || !campaign.type || !campaign.startDate) {
        setError('Please fill in all required fields');
        setSaving(false);
        return;
      }

      let savedCampaign;
      if (isEditMode) {
        savedCampaign = await CampaignService.update(id, campaign);
      } else {
        savedCampaign = await CampaignService.create(campaign);
      }
      
      // If there are selected customers, add them to the campaign
      if (selectedCustomers.length > 0) {
        await CampaignService.addCustomersToCampaign(
          savedCampaign.id, 
          selectedCustomers.map(c => c.id)
        );
      }
      
      setSuccessMessage(`Campaign successfully ${isEditMode ? 'updated' : 'created'}!`);
      
      // Navigate back to dashboard after a short delay
      setTimeout(() => {
        navigate('/campaign');
      }, 2000);
    } catch (err) {
      setError(`Failed to save campaign: ${err.message}`);
      console.error('Error saving campaign:', err);
    } finally {
      setSaving(false);
    }
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

  return (
    <Container fluid className="py-4">
      <Row className="mb-3">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <Button variant="outline-secondary" onClick={() => navigate('/campaign')} className="me-2">
                <FaArrowLeft /> Back to Campaigns
              </Button>
              <h2 className="d-inline-block ms-2 mb-0">
                {isEditMode ? `Edit Campaign: ${campaign.name}` : 'Create New Campaign'}
              </h2>
            </div>
            <Button 
              variant="primary" 
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? (
                <>
                  <Spinner size="sm" animation="border" className="me-1" />
                  Saving...
                </>
              ) : (
                <>
                  <FaSave className="me-1" /> {isEditMode ? 'Update Campaign' : 'Save Campaign'}
                </>
              )}
            </Button>
          </div>
        </Col>
      </Row>

      {error && (
        <Row className="mb-3">
          <Col>
            <Alert variant="danger" dismissible onClose={() => setError(null)}>
              {error}
            </Alert>
          </Col>
        </Row>
      )}

      {successMessage && (
        <Row className="mb-3">
          <Col>
            <Alert variant="success" dismissible onClose={() => setSuccessMessage(null)}>
              {successMessage}
            </Alert>
          </Col>
        </Row>
      )}

      <Row>
        <Col>
          <Card className="shadow-sm">
            <Card.Body>
              <Tabs
                activeKey={activeKey}
                onSelect={(key) => setActiveKey(key)}
                className="mb-4"
              >
                <Tab eventKey="details" title="Campaign Details">
                  <Row>
                    <Col md={8}>
                      <Form.Group className="mb-3">
                        <Form.Label>Campaign Name <span className="text-danger">*</span></Form.Label>
                        <Form.Control
                          type="text"
                          name="name"
                          value={campaign.name}
                          onChange={handleInputChange}
                          required
                        />
                      </Form.Group>
                      
                      <Form.Group className="mb-3">
                        <Form.Label>Description</Form.Label>
                        <Form.Control
                          as="textarea"
                          name="description"
                          value={campaign.description}
                          onChange={handleInputChange}
                          rows={3}
                        />
                      </Form.Group>
                      
                      <Row>
                        <Col md={6}>
                          <Form.Group className="mb-3">
                            <Form.Label>Campaign Type <span className="text-danger">*</span></Form.Label>
                            <Form.Select
                              name="type"
                              value={campaign.type}
                              onChange={handleInputChange}
                            >
                              <option value="email">Email Campaign</option>
                              <option value="sms">SMS Campaign</option>
                              <option value="multi-channel">Multi-channel Campaign</option>
                              <option value="event">Event Promotion</option>
                              <option value="newsletter">Newsletter</option>
                              <option value="promotion">Promotional Offer</option>
                              <option value="service-reminder">Service Reminder</option>
                              <option value="loyalty">Loyalty Program</option>
                            </Form.Select>
                          </Form.Group>
                        </Col>
                        <Col md={6}>
                          <Form.Group className="mb-3">
                            <Form.Label>Status</Form.Label>
                            <Form.Select
                              name="status"
                              value={campaign.status}
                              onChange={handleInputChange}
                            >
                              <option value="draft">Draft</option>
                              <option value="scheduled">Scheduled</option>
                              <option value="active">Active</option>
                              <option value="paused">Paused</option>
                              <option value="completed">Completed</option>
                              <option value="cancelled">Cancelled</option>
                            </Form.Select>
                          </Form.Group>
                        </Col>
                      </Row>
                      
                      <Row>
                        <Col md={6}>
                          <Form.Group className="mb-3">
                            <Form.Label>
                              <FaCalendarAlt className="me-1" /> Start Date <span className="text-danger">*</span>
                            </Form.Label>
                            <DatePicker
                              selected={campaign.startDate}
                              onChange={(date) => handleDateChange(date, 'startDate')}
                              className="form-control"
                              dateFormat="MM/dd/yyyy"
                            />
                          </Form.Group>
                        </Col>
                        <Col md={6}>
                          <Form.Group className="mb-3">
                            <Form.Label>
                              <FaCalendarAlt className="me-1" /> End Date
                            </Form.Label>
                            <DatePicker
                              selected={campaign.endDate}
                              onChange={(date) => handleDateChange(date, 'endDate')}
                              className="form-control"
                              dateFormat="MM/dd/yyyy"
                              isClearable
                              placeholderText="No end date (ongoing)"
                            />
                          </Form.Group>
                        </Col>
                      </Row>
                      
                      <Form.Group className="mb-3">
                        <Form.Label>Communication Channels</Form.Label>
                        <div className="d-flex flex-wrap gap-3">
                          <Form.Check 
                            type="checkbox"
                            label={<><FaEnvelope className="me-1" /> Email</>}
                            name="channelPreferences.email"
                            checked={campaign.channelPreferences?.email || false}
                            onChange={handleInputChange}
                          />
                          <Form.Check 
                            type="checkbox"
                            label={<><FaSms className="me-1" /> SMS</>}
                            name="channelPreferences.sms"
                            checked={campaign.channelPreferences?.sms || false}
                            onChange={handleInputChange}
                          />
                          <Form.Check 
                            type="checkbox"
                            label="Push Notification"
                            name="channelPreferences.push"
                            checked={campaign.channelPreferences?.push || false}
                            onChange={handleInputChange}
                          />
                          <Form.Check 
                            type="checkbox"
                            label={<><FaLink className="me-1" /> Social Media</>}
                            name="channelPreferences.social"
                            checked={campaign.channelPreferences?.social || false}
                            onChange={handleInputChange}
                          />
                        </div>
                      </Form.Group>
                    </Col>
                    
                    <Col md={4}>
                      <Card className="bg-light border-0">
                        <Card.Body>
                          <h5>Campaign Tips</h5>
                          <p className="small text-muted">
                            Create targeted campaigns to increase engagement with your customers. Here are some tips:
                          </p>
                          <ul className="small text-muted">
                            <li>Define clear goals for your campaign</li>
                            <li>Target specific customer segments for better results</li>
                            <li>Use personalized content to increase engagement</li>
                            <li>Schedule campaigns during optimal times</li>
                            <li>Monitor performance metrics to improve future campaigns</li>
                          </ul>
                        </Card.Body>
                      </Card>
                    </Col>
                  </Row>
                </Tab>
                
                <Tab eventKey="audience" title="Target Audience">
                  <Row>
                    <Col md={6}>
                      <h5 className="mb-3">
                        <FaUsers className="me-2" /> Customer Segments
                      </h5>
                      <p className="text-muted mb-3">
                        Select one or more customer segments to target with this campaign
                      </p>
                      
                      <ListGroup className="mb-4">
                        {customerSegments.map(segment => (
                          <ListGroup.Item key={segment.id} className="d-flex justify-content-between align-items-center">
                            <Form.Check
                              type="checkbox"
                              id={`segment-${segment.id}`}
                              label={segment.name}
                              checked={(campaign.segmentIds || []).includes(segment.id)}
                              onChange={() => handleSegmentToggle(segment.id)}
                            />
                            <Badge bg="primary" pill>{segment.count} customers</Badge>
                          </ListGroup.Item>
                        ))}
                      </ListGroup>
                    </Col>
                    
                    <Col md={6}>
                      <h5 className="mb-3">Add Individual Customers</h5>
                      <InputGroup className="mb-3">
                        <Form.Control
                          placeholder="Search customers by name or email..."
                          value={searchTerm}
                          onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <Button variant="outline-secondary" onClick={handleCustomerSearch}>
                          <FaSearch /> Search
                        </Button>
                      </InputGroup>
                      
                      {searchResults.length > 0 && (
                        <div className="border rounded p-2 mb-3" style={{maxHeight: '200px', overflowY: 'auto'}}>
                          <ListGroup variant="flush">
                            {searchResults.map(customer => (
                              <ListGroup.Item 
                                key={customer.id}
                                className="d-flex justify-content-between align-items-center"
                              >
                                <div>
                                  <div>{customer.firstName} {customer.lastName}</div>
                                  <small className="text-muted">{customer.email}</small>
                                </div>
                                <Button 
                                  size="sm"
                                  variant={selectedCustomers.some(c => c.id === customer.id) ? "danger" : "primary"}
                                  onClick={() => toggleCustomerSelection(customer)}
                                >
                                  {selectedCustomers.some(c => c.id === customer.id) ? "Remove" : "Add"}
                                </Button>
                              </ListGroup.Item>
                            ))}
                          </ListGroup>
                        </div>
                      )}
                      
                      <h6 className="mb-2">Selected Customers ({selectedCustomers.length})</h6>
                      <div className="border rounded p-2" style={{maxHeight: '200px', overflowY: 'auto'}}>
                        {selectedCustomers.length === 0 ? (
                          <p className="text-muted text-center my-3">No customers selected</p>
                        ) : (
                          <ListGroup variant="flush">
                            {selectedCustomers.map(customer => (
                              <ListGroup.Item 
                                key={customer.id}
                                className="d-flex justify-content-between align-items-center"
                              >
                                <div>
                                  {customer.firstName} {customer.lastName}
                                  <small className="text-muted d-block">{customer.email}</small>
                                </div>
                                <Button 
                                  size="sm"
                                  variant="outline-danger"
                                  onClick={() => toggleCustomerSelection(customer)}
                                >
                                  Remove
                                </Button>
                              </ListGroup.Item>
                            ))}
                          </ListGroup>
                        )}
                      </div>
                    </Col>
                  </Row>
                </Tab>
                
                <Tab eventKey="content" title="Campaign Content">
                  <Row>
                    <Col>
                      {campaign.type === 'email' && (
                        <>
                          <Form.Group className="mb-3">
                            <Form.Label>Email Subject <span className="text-danger">*</span></Form.Label>
                            <Form.Control
                              type="text"
                              name="subject"
                              value={campaign.subject || ''}
                              onChange={handleInputChange}
                            />
                          </Form.Group>
                          
                          <Form.Group className="mb-3">
                            <Form.Label>Email Content <span className="text-danger">*</span></Form.Label>
                            <Form.Control
                              as="textarea"
                              name="content"
                              value={campaign.content || ''}
                              onChange={handleInputChange}
                              rows={10}
                              placeholder="Write your email content here or use HTML..."
                            />
                            <Form.Text className="text-muted">
                              You can use HTML tags to format your email. Use {{firstName}}, {{lastName}} etc. for personalization.
                            </Form.Text>
                          </Form.Group>
                          
                          <div className="d-flex justify-content-between mt-4">
                            <Button variant="outline-secondary">
                              Preview Email
                            </Button>
                            <Button variant="outline-primary">
                              Select Template
                            </Button>
                          </div>
                        </>
                      )}
                      
                      {campaign.type === 'sms' && (
                        <Form.Group className="mb-3">
                          <Form.Label>SMS Content <span className="text-danger">*</span></Form.Label>
                          <Form.Control
                            as="textarea"
                            name="content"
                            value={campaign.content || ''}
                            onChange={handleInputChange}
                            rows={5}
                            maxLength={160}
                          />
                          <Form.Text className="text-muted">
                            Maximum 160 characters. Use {{firstName}}, {{lastName}} etc. for personalization.
                          </Form.Text>
                          <div className="text-end mt-2">
                            <small>{(campaign.content || '').length}/160 characters</small>
                          </div>
                        </Form.Group>
                      )}
                      
                      {['multi-channel', 'event', 'newsletter', 'promotion', 'service-reminder', 'loyalty'].includes(campaign.type) && (
                        <Alert variant="info">
                          Please configure the message content for each channel you've selected in the Campaign Details tab.
                        </Alert>
                      )}
                    </Col>
                  </Row>
                </Tab>
                
                <Tab eventKey="schedule" title="Schedule & Settings">
                  <Row>
                    <Col md={6}>
                      <Form.Group className="mb-3">
                        <Form.Label>Send Time Optimization</Form.Label>
                        <Form.Select
                          name="sendTimeOptimization"
                          value={campaign.sendTimeOptimization || 'none'}
                          onChange={handleInputChange}
                        >
                          <option value="none">Send at scheduled time</option>
                          <option value="optimal">Send at optimal time per customer</option>
                          <option value="timezone">Adjust for customer timezone</option>
                        </Form.Select>
                        <Form.Text className="text-muted">
                          Optimize delivery time based on customer engagement history
                        </Form.Text>
                      </Form.Group>
                      
                      <Form.Group className="mb-3">
                        <Form.Label>Frequency Cap</Form.Label>
                        <Form.Select
                          name="frequencyCap"
                          value={campaign.frequencyCap || 'none'}
                          onChange={handleInputChange}
                        >
                          <option value="none">No cap</option>
                          <option value="daily">Max 1 per day</option>
                          <option value="weekly">Max 2 per week</option>
                          <option value="monthly">Max 5 per month</option>
                        </Form.Select>
                        <Form.Text className="text-muted">
                          Limit campaign frequency per customer across all campaigns
                        </Form.Text>
                      </Form.Group>
                    </Col>
                    
                    <Col md={6}>
                      <Form.Group className="mb-3">
                        <Form.Label>A/B Testing</Form.Label>
                        <Form.Check
                          type="switch"
                          id="abTestingSwitch"
                          label="Enable A/B Testing"
                          checked={campaign.abTesting || false}
                          onChange={(e) => setCampaign(prev => ({...prev, abTesting: e.target.checked}))}
                        />
                        <Form.Text className="text-muted">
                          Test different versions of your campaign content
                        </Form.Text>
                      </Form.Group>
                      
                      <Form.Group className="mb-3">
                        <Form.Label>Campaign Tags</Form.Label>
                        <Form.Control
                          type="text"
                          name="tags"
                          value={campaign.tags || ''}
                          onChange={handleInputChange}
                          placeholder="Enter tags separated by commas"
                        />
                        <Form.Text className="text-muted">
                          Add tags to categorize and filter campaigns (e.g., promotion, service, seasonal)
                        </Form.Text>
                      </Form.Group>
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

export default CampaignEditor;
