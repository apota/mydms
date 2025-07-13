import React, { useState, useEffect } from 'react';
import { 
  Container, Row, Col, Card, Button, Table, Badge, 
  Spinner, Alert, Form, InputGroup, Modal
} from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { FaPlus, FaSearch, FaCalendarAlt, FaChartBar, FaFilter, FaEdit, FaTrash } from 'react-icons/fa';
import { CampaignService } from '../../services/api-services';
import Calendar from 'react-calendar';
import 'react-calendar/dist/Calendar.css';

const CampaignDashboard = () => {
  const [campaigns, setCampaigns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all');
  const [showCalendarView, setShowCalendarView] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [campaignToDelete, setCampaignToDelete] = useState(null);
  const [page, setPage] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const pageSize = 10;

  // Fetch campaigns on component mount
  useEffect(() => {
    fetchCampaigns();
  }, [page]);

  const fetchCampaigns = async () => {
    try {
      setLoading(true);
      const data = await CampaignService.getAll(page * pageSize, pageSize);
      setCampaigns(data.items);
      setTotalPages(Math.ceil(data.totalCount / pageSize));
      setLoading(false);
    } catch (err) {
      setError('Failed to load campaigns. Please try again later.');
      setLoading(false);
      console.error('Error fetching campaigns:', err);
    }
  };

  const handleDeleteClick = (campaign) => {
    setCampaignToDelete(campaign);
    setShowDeleteModal(true);
  };

  const handleDeleteConfirm = async () => {
    try {
      await CampaignService.delete(campaignToDelete.id);
      setCampaigns(campaigns.filter(c => c.id !== campaignToDelete.id));
      setShowDeleteModal(false);
    } catch (err) {
      setError('Failed to delete campaign. Please try again later.');
      console.error('Error deleting campaign:', err);
    }
  };

  const filteredCampaigns = campaigns.filter(campaign => {
    const matchesSearch = campaign.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
                          campaign.description.toLowerCase().includes(searchTerm.toLowerCase());
    
    if (filterStatus === 'all') return matchesSearch;
    return matchesSearch && campaign.status.toLowerCase() === filterStatus.toLowerCase();
  });

  const getBadgeVariant = (status) => {
    switch(status.toLowerCase()) {
      case 'active': return 'success';
      case 'draft': return 'secondary';
      case 'scheduled': return 'info';
      case 'completed': return 'primary';
      case 'cancelled': return 'danger';
      default: return 'warning';
    }
  };

  const renderCalendarView = () => {
    const campaignEvents = campaigns.map(campaign => ({
      date: new Date(campaign.startDate),
      campaign
    }));
    
    return (
      <div className="campaign-calendar mb-4">
        <Card>
          <Card.Header className="d-flex justify-content-between align-items-center">
            <h5 className="mb-0">Campaign Calendar</h5>
            <Button variant="outline-primary" size="sm" onClick={() => setShowCalendarView(false)}>
              Switch to List View
            </Button>
          </Card.Header>
          <Card.Body>
            <Calendar
              tileContent={({ date }) => {
                const eventsForDay = campaignEvents.filter(
                  event => event.date.toDateString() === date.toDateString()
                );
                return eventsForDay.length > 0 ? (
                  <div className="calendar-event-indicator">
                    <Badge bg="primary" pill>{eventsForDay.length}</Badge>
                  </div>
                ) : null;
              }}
            />
            <div className="calendar-events mt-3">
              {campaignEvents
                .sort((a, b) => a.date - b.date)
                .slice(0, 5)
                .map((event, idx) => (
                  <div key={idx} className="calendar-event-item p-2 border-bottom">
                    <p className="mb-0">
                      <strong>{event.date.toLocaleDateString()}</strong>: {event.campaign.name}
                      <Badge bg={getBadgeVariant(event.campaign.status)} className="ms-2">
                        {event.campaign.status}
                      </Badge>
                    </p>
                  </div>
                ))}
            </div>
          </Card.Body>
        </Card>
      </div>
    );
  };

  const renderListView = () => (
    <Card className="shadow-sm">
      <Card.Header className="bg-white">
        <div className="d-flex justify-content-between align-items-center">
          <h5 className="mb-0">Active Campaigns</h5>
          <div>
            <Button 
              variant="outline-primary" 
              size="sm" 
              className="me-2"
              onClick={() => setShowCalendarView(true)}
            >
              <FaCalendarAlt className="me-1" /> Calendar View
            </Button>
            <Link to="/campaign/new">
              <Button variant="primary" size="sm">
                <FaPlus className="me-1" /> Create Campaign
              </Button>
            </Link>
          </div>
        </div>
      </Card.Header>
      <Card.Body>
        <Row className="mb-3">
          <Col md={8}>
            <InputGroup>
              <InputGroup.Text>
                <FaSearch />
              </InputGroup.Text>
              <Form.Control
                placeholder="Search campaigns..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </InputGroup>
          </Col>
          <Col md={4}>
            <Form.Select 
              value={filterStatus} 
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="all">All Statuses</option>
              <option value="draft">Draft</option>
              <option value="scheduled">Scheduled</option>
              <option value="active">Active</option>
              <option value="completed">Completed</option>
              <option value="cancelled">Cancelled</option>
            </Form.Select>
          </Col>
        </Row>

        {loading ? (
          <div className="text-center my-4">
            <Spinner animation="border" role="status" variant="primary">
              <span className="visually-hidden">Loading...</span>
            </Spinner>
          </div>
        ) : error ? (
          <Alert variant="danger">{error}</Alert>
        ) : (
          <>
            <Table responsive hover className="align-middle">
              <thead className="table-light">
                <tr>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                  <th>Target Audience</th>
                  <th>Metrics</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredCampaigns.length === 0 ? (
                  <tr>
                    <td colSpan="8" className="text-center py-4">
                      No campaigns found. Create your first campaign!
                    </td>
                  </tr>
                ) : (
                  filteredCampaigns.map(campaign => (
                    <tr key={campaign.id}>
                      <td>
                        <Link to={`/campaign/${campaign.id}`} className="text-decoration-none">
                          <strong>{campaign.name}</strong>
                        </Link>
                        <div className="small text-muted text-truncate" style={{maxWidth: '200px'}}>
                          {campaign.description}
                        </div>
                      </td>
                      <td>{campaign.type}</td>
                      <td>
                        <Badge bg={getBadgeVariant(campaign.status)}>
                          {campaign.status}
                        </Badge>
                      </td>
                      <td>{new Date(campaign.startDate).toLocaleDateString()}</td>
                      <td>{campaign.endDate ? new Date(campaign.endDate).toLocaleDateString() : 'Ongoing'}</td>
                      <td>{campaign.targetAudience || 'All Customers'}</td>
                      <td>
                        <div className="d-flex align-items-center">
                          <div className="me-2">
                            <div className="small fw-bold">{campaign.metrics?.sent || 0} Sent</div>
                            <div className="small">{campaign.metrics?.opened || 0} Opened</div>
                          </div>
                          <Link to={`/campaign/${campaign.id}/analytics`}>
                            <Button variant="outline-info" size="sm">
                              <FaChartBar />
                            </Button>
                          </Link>
                        </div>
                      </td>
                      <td>
                        <div className="d-flex">
                          <Link to={`/campaign/${campaign.id}/edit`} className="me-2">
                            <Button variant="outline-primary" size="sm">
                              <FaEdit />
                            </Button>
                          </Link>
                          <Button 
                            variant="outline-danger" 
                            size="sm"
                            onClick={() => handleDeleteClick(campaign)}
                          >
                            <FaTrash />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </Table>
            
            {totalPages > 1 && (
              <div className="d-flex justify-content-center mt-4">
                <Button 
                  variant="outline-primary" 
                  size="sm" 
                  className="me-2" 
                  disabled={page === 0}
                  onClick={() => setPage(page - 1)}
                >
                  Previous
                </Button>
                <span className="mx-2 align-self-center">
                  Page {page + 1} of {totalPages}
                </span>
                <Button 
                  variant="outline-primary" 
                  size="sm" 
                  disabled={page === totalPages - 1}
                  onClick={() => setPage(page + 1)}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        )}
      </Card.Body>
    </Card>
  );

  return (
    <Container fluid className="py-4">
      <Row className="mb-4">
        <Col>
          <h2>Campaign Manager</h2>
          <p className="text-muted">
            Create, manage and track marketing campaigns to engage with customers
          </p>
        </Col>
      </Row>
      
      <Row className="mb-4">
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-primary">{campaigns.length}</h3>
              <p className="text-muted mb-0">Total Campaigns</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-success">
                {campaigns.filter(c => c.status.toLowerCase() === 'active').length}
              </h3>
              <p className="text-muted mb-0">Active Campaigns</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-info">
                {campaigns.filter(c => c.status.toLowerCase() === 'scheduled').length}
              </h3>
              <p className="text-muted mb-0">Scheduled Campaigns</p>
            </Card.Body>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="shadow-sm mb-4">
            <Card.Body className="text-center">
              <h3 className="display-4 text-warning">
                {campaigns.reduce((sum, campaign) => sum + (campaign.metrics?.clicks || 0), 0)}
              </h3>
              <p className="text-muted mb-0">Total Interactions</p>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Row>
        <Col>
          {showCalendarView ? renderCalendarView() : renderListView()}
        </Col>
      </Row>

      {/* Delete Confirmation Modal */}
      <Modal show={showDeleteModal} onHide={() => setShowDeleteModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Confirm Delete</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          Are you sure you want to delete the campaign "{campaignToDelete?.name}"? 
          This action cannot be undone.
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowDeleteModal(false)}>
            Cancel
          </Button>
          <Button variant="danger" onClick={handleDeleteConfirm}>
            Delete Campaign
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default CampaignDashboard;
