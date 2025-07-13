import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Nav, Tab, Button, Table, Form, Badge, Spinner, Modal } from 'react-bootstrap';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { FaEdit, FaPhone, FaEnvelope, FaGlobe, FaMapMarkerAlt, FaChartLine, FaHistory, FaFileAlt } from 'react-icons/fa';
import api from '../../services/api';

const SupplierDetails = () => {
  const { id } = useParams();
  const [supplier, setSupplier] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editSupplier, setEditSupplier] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchSupplier = async () => {
      try {
        setLoading(true);
        const supplierData = await api.suppliers.getSupplierById(id);
        setSupplier(supplierData);
        setEditSupplier({...supplierData});
        setError(null);
      } catch (err) {
        console.error('Error fetching supplier details:', err);
        setError('Failed to load supplier details. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchSupplier();
    }
  }, [id]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setEditSupplier({
      ...editSupplier,
      [name]: value
    });
  };

  const handleUpdateSupplier = async () => {
    try {
      await api.suppliers.updateSupplier(id, editSupplier);
      setSupplier(editSupplier);
      setShowEditModal(false);
    } catch (err) {
      console.error('Error updating supplier:', err);
      alert('Failed to update supplier. Please try again.');
    }
  };

  if (loading) {
    return (
      <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '60vh' }}>
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="mt-4">
        <Row>
          <Col>
            <div className="alert alert-danger">{error}</div>
            <Button variant="secondary" onClick={() => navigate('/suppliers')}>
              Back to Suppliers List
            </Button>
          </Col>
        </Row>
      </Container>
    );
  }

  return (
    <Container fluid className="mt-4">
      <Row>
        <Col>
          <Button 
            variant="outline-secondary" 
            className="mb-3"
            onClick={() => navigate('/suppliers')}
          >
            &larr; Back to Suppliers
          </Button>
          <h2 className="mb-4">{supplier?.name}</h2>
        </Col>
        <Col className="text-end">
          <Button variant="outline-primary" onClick={() => setShowEditModal(true)}>
            <FaEdit className="me-2" /> Edit
          </Button>
        </Col>
      </Row>
      
      {supplier && (
        <>
          <Row>
            <Col md={4}>
              <Card className="mb-4">
                <Card.Header>Supplier Information</Card.Header>
                <Card.Body>
                  <div className="mb-3">
                    <strong>Contact Person:</strong>
                    <p>{supplier.contactName || 'Not specified'}</p>
                  </div>
                  
                  <div className="mb-3">
                    <strong>Contact Information:</strong>
                    <p>
                      {supplier.email && (
                        <div className="mb-2">
                          <FaEnvelope className="me-2 text-secondary" />
                          <a href={`mailto:${supplier.email}`}>{supplier.email}</a>
                        </div>
                      )}
                      
                      {supplier.phone && (
                        <div className="mb-2">
                          <FaPhone className="me-2 text-secondary" />
                          <a href={`tel:${supplier.phone}`}>{supplier.phone}</a>
                        </div>
                      )}
                      
                      {supplier.website && (
                        <div className="mb-2">
                          <FaGlobe className="me-2 text-secondary" />
                          <a href={supplier.website} target="_blank" rel="noopener noreferrer">
                            {supplier.website.replace(/^https?:\/\//, '')}
                          </a>
                        </div>
                      )}
                    </p>
                  </div>
                  
                  {supplier.address && (
                    <div className="mb-3">
                      <strong>Address:</strong>
                      <p>
                        <FaMapMarkerAlt className="me-2 text-secondary" />
                        {supplier.address.split('\n').map((line, i) => (
                          <span key={i}>
                            {line}
                            <br />
                          </span>
                        ))}
                      </p>
                    </div>
                  )}
                  
                  {supplier.notes && (
                    <div>
                      <strong>Notes:</strong>
                      <p>{supplier.notes}</p>
                    </div>
                  )}
                </Card.Body>
              </Card>
              
              <Card className="mb-4">
                <Card.Header>Metrics</Card.Header>
                <Card.Body>
                  <div className="d-flex justify-content-between mb-2">
                    <span>On-Time Delivery Rate:</span>
                    <span>{supplier.metrics?.onTimeDeliveryRate || '95'}%</span>
                  </div>
                  
                  <div className="d-flex justify-content-between mb-2">
                    <span>Quality Rating:</span>
                    <span>{supplier.metrics?.qualityRating || '4.5'}/5</span>
                  </div>
                  
                  <div className="d-flex justify-content-between mb-2">
                    <span>Average Response Time:</span>
                    <span>{supplier.metrics?.avgResponseTime || '2'} days</span>
                  </div>
                  
                  <div className="d-flex justify-content-between">
                    <span>Price Competitiveness:</span>
                    <span>{supplier.metrics?.priceRating || '4'}/5</span>
                  </div>
                </Card.Body>
              </Card>
              
              <Card>
                <Card.Header>Actions</Card.Header>
                <Card.Body>
                  <div className="d-grid gap-2">
                    <Button 
                      variant="primary"
                      as={Link}
                      to={`/orders/new?supplierId=${supplier.id}`}
                    >
                      Create New Order
                    </Button>
                    <Button variant="outline-secondary">
                      <FaChartLine className="me-2" /> View Analytics
                    </Button>
                    <Button variant="outline-secondary">
                      <FaFileAlt className="me-2" /> Generate Report
                    </Button>
                  </div>
                </Card.Body>
              </Card>
            </Col>
            <Col md={8}>
              <Tab.Container defaultActiveKey="orders">
                <Card className="mb-4">
                  <Card.Header>
                    <Nav variant="tabs">
                      <Nav.Item>
                        <Nav.Link eventKey="orders">Orders</Nav.Link>
                      </Nav.Item>
                      <Nav.Item>
                        <Nav.Link eventKey="parts">Parts</Nav.Link>
                      </Nav.Item>
                      <Nav.Item>
                        <Nav.Link eventKey="history">History</Nav.Link>
                      </Nav.Item>
                      <Nav.Item>
                        <Nav.Link eventKey="documents">Documents</Nav.Link>
                      </Nav.Item>
                    </Nav>
                  </Card.Header>
                  <Card.Body>
                    <Tab.Content>
                      <Tab.Pane eventKey="orders">
                        <h5 className="mb-3">Recent Orders</h5>
                        <Table responsive hover>
                          <thead>
                            <tr>
                              <th>Order #</th>
                              <th>Date</th>
                              <th>Total</th>
                              <th>Status</th>
                              <th>Actions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {supplier.recentOrders && supplier.recentOrders.length > 0 ? (
                              supplier.recentOrders.map((order) => (
                                <tr key={order.id}>
                                  <td>{order.orderNumber}</td>
                                  <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                                  <td>${order.total.toFixed(2)}</td>
                                  <td>
                                    <Badge bg={
                                      order.status === 'Received' ? 'success' : 
                                      order.status === 'PartiallyReceived' ? 'warning' :
                                      order.status === 'Processing' ? 'info' :
                                      order.status === 'Submitted' ? 'primary' :
                                      order.status === 'Cancelled' ? 'danger' : 'secondary'
                                    }>
                                      {order.status}
                                    </Badge>
                                  </td>
                                  <td>
                                    <Button 
                                      variant="outline-primary" 
                                      size="sm"
                                      as={Link}
                                      to={`/orders/${order.id}`}
                                    >
                                      View
                                    </Button>
                                  </td>
                                </tr>
                              ))
                            ) : (
                              <tr>
                                <td colSpan="5" className="text-center">
                                  No orders found for this supplier
                                </td>
                              </tr>
                            )}
                          </tbody>
                        </Table>
                        <div className="text-end">
                          <Link to={`/orders?supplierId=${supplier.id}`}>
                            View All Orders &rarr;
                          </Link>
                        </div>
                      </Tab.Pane>
                      <Tab.Pane eventKey="parts">
                        <h5 className="mb-3">Parts Supplied</h5>
                        <Table responsive hover>
                          <thead>
                            <tr>
                              <th>Part #</th>
                              <th>Name</th>
                              <th>Last Price</th>
                              <th>Lead Time</th>
                              <th>Actions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {supplier.suppliedParts && supplier.suppliedParts.length > 0 ? (
                              supplier.suppliedParts.map((part) => (
                                <tr key={part.id}>
                                  <td>{part.partNumber}</td>
                                  <td>{part.name}</td>
                                  <td>${part.lastPrice.toFixed(2)}</td>
                                  <td>{part.leadTime} days</td>
                                  <td>
                                    <Button 
                                      variant="outline-primary" 
                                      size="sm"
                                      as={Link}
                                      to={`/parts/${part.id}`}
                                    >
                                      View
                                    </Button>
                                  </td>
                                </tr>
                              ))
                            ) : (
                              <tr>
                                <td colSpan="5" className="text-center">
                                  No parts associated with this supplier
                                </td>
                              </tr>
                            )}
                          </tbody>
                        </Table>
                      </Tab.Pane>
                      <Tab.Pane eventKey="history">
                        <h5 className="mb-3">Interaction History</h5>
                        <div className="activity-timeline">
                          {supplier.history && supplier.history.length > 0 ? (
                            supplier.history.map((item, index) => (
                              <div key={index} className="timeline-item mb-4">
                                <div className="d-flex">
                                  <div className="flex-shrink-0 me-3">
                                    {new Date(item.date).toLocaleDateString()}
                                  </div>
                                  <div className="flex-grow-1">
                                    <h6 className="mb-1">{item.title}</h6>
                                    <p className="mb-0">{item.description}</p>
                                    <small className="text-muted">By {item.user}</small>
                                  </div>
                                </div>
                              </div>
                            ))
                          ) : (
                            <p className="text-center">No history records available</p>
                          )}
                        </div>
                      </Tab.Pane>
                      <Tab.Pane eventKey="documents">
                        <h5 className="mb-3">Documents</h5>
                        <Row>
                          {supplier.documents && supplier.documents.length > 0 ? (
                            supplier.documents.map((doc, index) => (
                              <Col md={6} key={index} className="mb-3">
                                <Card>
                                  <Card.Body>
                                    <div className="d-flex align-items-center">
                                      <div className="flex-shrink-0 me-3">
                                        <FaFileAlt size={24} className="text-primary" />
                                      </div>
                                      <div className="flex-grow-1">
                                        <h6 className="mb-0">{doc.name}</h6>
                                        <small className="text-muted">
                                          {new Date(doc.date).toLocaleDateString()}
                                        </small>
                                      </div>
                                      <div>
                                        <Button variant="link" size="sm">
                                          View
                                        </Button>
                                      </div>
                                    </div>
                                  </Card.Body>
                                </Card>
                              </Col>
                            ))
                          ) : (
                            <Col>
                              <p className="text-center">No documents available</p>
                            </Col>
                          )}
                        </Row>
                      </Tab.Pane>
                    </Tab.Content>
                  </Card.Body>
                </Card>
              </Tab.Container>
              
              <Card>
                <Card.Header>
                  <h5 className="mb-0">Performance Analysis</h5>
                </Card.Header>
                <Card.Body>
                  <p className="text-center text-muted">
                    Charts and analytics will be displayed here, showing performance trends, price history, and quality metrics.
                  </p>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        </>
      )}
      
      {/* Edit Supplier Modal */}
      <Modal show={showEditModal} onHide={() => setShowEditModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Edit Supplier</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {editSupplier && (
            <Form>
              <Form.Group className="mb-3">
                <Form.Label>Supplier Name*</Form.Label>
                <Form.Control
                  type="text"
                  name="name"
                  value={editSupplier.name}
                  onChange={handleInputChange}
                  required
                />
              </Form.Group>
              
              <Form.Group className="mb-3">
                <Form.Label>Contact Person</Form.Label>
                <Form.Control
                  type="text"
                  name="contactName"
                  value={editSupplier.contactName || ''}
                  onChange={handleInputChange}
                />
              </Form.Group>
              
              <Row>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Email</Form.Label>
                    <Form.Control
                      type="email"
                      name="email"
                      value={editSupplier.email || ''}
                      onChange={handleInputChange}
                    />
                  </Form.Group>
                </Col>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Phone</Form.Label>
                    <Form.Control
                      type="text"
                      name="phone"
                      value={editSupplier.phone || ''}
                      onChange={handleInputChange}
                    />
                  </Form.Group>
                </Col>
              </Row>
              
              <Form.Group className="mb-3">
                <Form.Label>Address</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={2}
                  name="address"
                  value={editSupplier.address || ''}
                  onChange={handleInputChange}
                />
              </Form.Group>
              
              <Form.Group className="mb-3">
                <Form.Label>Website</Form.Label>
                <Form.Control
                  type="url"
                  name="website"
                  value={editSupplier.website || ''}
                  onChange={handleInputChange}
                  placeholder="https://"
                />
              </Form.Group>
              
              <Form.Group className="mb-3">
                <Form.Label>Notes</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={3}
                  name="notes"
                  value={editSupplier.notes || ''}
                  onChange={handleInputChange}
                />
              </Form.Group>
            </Form>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowEditModal(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleUpdateSupplier}>
            Save Changes
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default SupplierDetails;
