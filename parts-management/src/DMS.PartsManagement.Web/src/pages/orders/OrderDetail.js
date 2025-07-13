import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Badge, Spinner, ListGroup, Breadcrumb, Modal, Form } from 'react-bootstrap';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { FaFileInvoice, FaTruck, FaPrint, FaDownload, FaCheck, FaTimesCircle, FaHistory, FaClock } from 'react-icons/fa';
import api from '../../services/api';

const OrderDetail = () => {
  const { id } = useParams();
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showReceiveModal, setShowReceiveModal] = useState(false);
  const [selectedItems, setSelectedItems] = useState({});
  const [notes, setNotes] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchOrder = async () => {
      try {
        setLoading(true);
        const orderData = await api.orders.getOrderById(id);
        setOrder(orderData);
        
        // Initialize selected items for receiving
        const initialSelectedItems = {};
        orderData.items.forEach(item => {
          initialSelectedItems[item.id] = {
            receivedQuantity: 0,
            isSelected: false
          };
        });
        setSelectedItems(initialSelectedItems);
        
        setError(null);
      } catch (err) {
        console.error('Error fetching order details:', err);
        setError('Failed to load order details. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchOrder();
    }
  }, [id]);

  const handleReceiveOrder = async () => {
    try {
      // Get selected items for receiving
      const itemsToReceive = Object.entries(selectedItems)
        .filter(([_, item]) => item.isSelected && item.receivedQuantity > 0)
        .map(([itemId, item]) => ({
          orderItemId: parseInt(itemId),
          receivedQuantity: parseInt(item.receivedQuantity)
        }));

      if (itemsToReceive.length === 0) {
        alert('Please select at least one item to receive');
        return;
      }

      // Call API to receive items
      await api.orders.receiveOrderItems(id, {
        items: itemsToReceive,
        notes: notes
      });

      // Refresh order data after receiving
      const updatedOrder = await api.orders.getOrderById(id);
      setOrder(updatedOrder);
      
      // Reset form and close modal
      setNotes('');
      setShowReceiveModal(false);
      
      // Reset selected items based on updated order
      const initialSelectedItems = {};
      updatedOrder.items.forEach(item => {
        initialSelectedItems[item.id] = {
          receivedQuantity: 0,
          isSelected: false
        };
      });
      setSelectedItems(initialSelectedItems);
      
    } catch (err) {
      console.error('Error receiving order items:', err);
      alert('Failed to receive order items. Please try again.');
    }
  };

  const handleSelectAllItems = (e) => {
    const isChecked = e.target.checked;
    
    const updatedSelectedItems = {};
    Object.keys(selectedItems).forEach(itemId => {
      updatedSelectedItems[itemId] = {
        ...selectedItems[itemId],
        isSelected: isChecked
      };
    });
    
    setSelectedItems(updatedSelectedItems);
  };

  const handleSelectItem = (itemId, e) => {
    const isChecked = e.target.checked;
    
    setSelectedItems({
      ...selectedItems,
      [itemId]: {
        ...selectedItems[itemId],
        isSelected: isChecked
      }
    });
  };

  const handleReceivedQuantityChange = (itemId, value) => {
    setSelectedItems({
      ...selectedItems,
      [itemId]: {
        ...selectedItems[itemId],
        receivedQuantity: value
      }
    });
  };

  const getStatusBadge = (status) => {
    switch(status) {
      case 'Draft':
        return <Badge bg="secondary">Draft</Badge>;
      case 'Submitted':
        return <Badge bg="primary">Submitted</Badge>;
      case 'Processing':
        return <Badge bg="info">Processing</Badge>;
      case 'PartiallyReceived':
        return <Badge bg="warning" text="dark">Partially Received</Badge>;
      case 'Received':
        return <Badge bg="success">Received</Badge>;
      case 'Cancelled':
        return <Badge bg="danger">Cancelled</Badge>;
      default:
        return <Badge bg="light" text="dark">{status}</Badge>;
    }
  };

  const getRemainingQuantity = (item) => {
    return item.quantity - item.receivedQuantity;
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
            <Button variant="secondary" onClick={() => navigate('/orders')}>
              Back to Orders List
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
          <Breadcrumb>
            <Breadcrumb.Item linkAs={Link} linkProps={{ to: "/orders" }}>Orders</Breadcrumb.Item>
            <Breadcrumb.Item active>Order #{order?.orderNumber}</Breadcrumb.Item>
          </Breadcrumb>
          <h2 className="mb-4">
            Purchase Order #{order?.orderNumber} {getStatusBadge(order?.status)}
          </h2>
        </Col>
      </Row>
      
      {order && (
        <>
          <Row>
            <Col md={8}>
              <Card className="mb-4">
                <Card.Body>
                  <Row>
                    <Col md={6}>
                      <h5>Order Information</h5>
                      <p><strong>Date:</strong> {new Date(order.orderDate).toLocaleDateString()}</p>
                      <p><strong>Status:</strong> {order.status}</p>
                      <p>
                        <strong>Expected Delivery:</strong> {' '}
                        {order.expectedDeliveryDate 
                          ? new Date(order.expectedDeliveryDate).toLocaleDateString()
                          : 'Not specified'}
                      </p>
                      <p><strong>Notes:</strong> {order.notes || 'None'}</p>
                    </Col>
                    <Col md={6}>
                      <h5>Supplier</h5>
                      <p><strong>Name:</strong> {order.supplierName}</p>
                      <p><strong>Contact:</strong> {order.supplierContactName || 'N/A'}</p>
                      <p><strong>Email:</strong> {order.supplierEmail || 'N/A'}</p>
                      <p><strong>Phone:</strong> {order.supplierPhone || 'N/A'}</p>
                    </Col>
                  </Row>
                </Card.Body>
              </Card>
              
              <Card className="mb-4">
                <Card.Header>
                  <h5 className="mb-0">Order Items</h5>
                </Card.Header>
                <Card.Body>
                  <Table responsive>
                    <thead>
                      <tr>
                        <th>Part #</th>
                        <th>Description</th>
                        <th className="text-center">Quantity</th>
                        <th className="text-center">Received</th>
                        <th className="text-end">Unit Price</th>
                        <th className="text-end">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {order.items.map((item) => (
                        <tr key={item.id}>
                          <td>{item.partNumber}</td>
                          <td>{item.partName}</td>
                          <td className="text-center">{item.quantity}</td>
                          <td className="text-center">
                            {item.receivedQuantity} / {item.quantity}
                            {item.receivedQuantity === item.quantity ? (
                              <FaCheck className="ms-2 text-success" />
                            ) : item.receivedQuantity > 0 ? (
                              <FaClock className="ms-2 text-warning" />
                            ) : null}
                          </td>
                          <td className="text-end">${item.unitPrice.toFixed(2)}</td>
                          <td className="text-end">${(item.quantity * item.unitPrice).toFixed(2)}</td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot>
                      <tr>
                        <th colSpan="5" className="text-end">Subtotal:</th>
                        <th className="text-end">${order.subtotal.toFixed(2)}</th>
                      </tr>
                      <tr>
                        <th colSpan="5" className="text-end">Tax:</th>
                        <th className="text-end">${order.tax.toFixed(2)}</th>
                      </tr>
                      <tr>
                        <th colSpan="5" className="text-end">Shipping:</th>
                        <th className="text-end">${order.shipping.toFixed(2)}</th>
                      </tr>
                      <tr>
                        <th colSpan="5" className="text-end">Total:</th>
                        <th className="text-end">${order.total.toFixed(2)}</th>
                      </tr>
                    </tfoot>
                  </Table>
                </Card.Body>
              </Card>
              
              {order.receivingHistory && order.receivingHistory.length > 0 && (
                <Card className="mb-4">
                  <Card.Header className="d-flex justify-content-between align-items-center">
                    <h5 className="mb-0">
                      <FaHistory className="me-2" /> Receiving History
                    </h5>
                  </Card.Header>
                  <ListGroup variant="flush">
                    {order.receivingHistory.map((record) => (
                      <ListGroup.Item key={record.id}>
                        <Row>
                          <Col md={3}>
                            {new Date(record.date).toLocaleDateString()} {new Date(record.date).toLocaleTimeString()}
                          </Col>
                          <Col md={7}>
                            Received {record.totalQuantity} items - {record.notes}
                          </Col>
                          <Col md={2}>
                            By: {record.receivedBy}
                          </Col>
                        </Row>
                      </ListGroup.Item>
                    ))}
                  </ListGroup>
                </Card>
              )}
            </Col>
            
            <Col md={4}>
              <Card className="mb-4">
                <Card.Header>Actions</Card.Header>
                <Card.Body>
                  <div className="d-grid gap-2">
                    {['Draft', 'Submitted', 'Processing', 'PartiallyReceived'].includes(order.status) && (
                      <Button 
                        variant="success" 
                        onClick={() => setShowReceiveModal(true)}
                        disabled={order.status === 'Draft'}
                      >
                        <FaTruck className="me-2" /> Receive Items
                      </Button>
                    )}
                    
                    {order.status === 'Draft' && (
                      <>
                        <Button variant="primary" onClick={() => navigate(`/orders/${id}/edit`)}>
                          Edit Order
                        </Button>
                        <Button variant="outline-primary">
                          Submit Order
                        </Button>
                      </>
                    )}
                    
                    <Button variant="outline-secondary">
                      <FaPrint className="me-2" /> Print
                    </Button>
                    
                    <Button variant="outline-secondary">
                      <FaDownload className="me-2" /> Download PDF
                    </Button>
                    
                    {['Draft', 'Submitted'].includes(order.status) && (
                      <Button variant="outline-danger">
                        <FaTimesCircle className="me-2" /> Cancel Order
                      </Button>
                    )}
                  </div>
                </Card.Body>
              </Card>
              
              <Card className="mb-4">
                <Card.Header>Order Timeline</Card.Header>
                <ListGroup variant="flush">
                  <ListGroup.Item>
                    <small className="text-muted">Created</small>
                    <div>{new Date(order.orderDate).toLocaleString()}</div>
                  </ListGroup.Item>
                  
                  {order.submittedDate && (
                    <ListGroup.Item>
                      <small className="text-muted">Submitted</small>
                      <div>{new Date(order.submittedDate).toLocaleString()}</div>
                    </ListGroup.Item>
                  )}
                  
                  {order.processedDate && (
                    <ListGroup.Item>
                      <small className="text-muted">Processing Started</small>
                      <div>{new Date(order.processedDate).toLocaleString()}</div>
                    </ListGroup.Item>
                  )}
                  
                  {order.firstReceivedDate && (
                    <ListGroup.Item>
                      <small className="text-muted">First Items Received</small>
                      <div>{new Date(order.firstReceivedDate).toLocaleString()}</div>
                    </ListGroup.Item>
                  )}
                  
                  {order.completedDate && (
                    <ListGroup.Item>
                      <small className="text-muted">All Items Received</small>
                      <div>{new Date(order.completedDate).toLocaleString()}</div>
                    </ListGroup.Item>
                  )}
                  
                  {order.cancelledDate && (
                    <ListGroup.Item>
                      <small className="text-muted">Cancelled</small>
                      <div>{new Date(order.cancelledDate).toLocaleString()}</div>
                    </ListGroup.Item>
                  )}
                </ListGroup>
              </Card>
              
              <Card>
                <Card.Header>Related Documents</Card.Header>
                <ListGroup variant="flush">
                  {order.relatedDocuments && order.relatedDocuments.length > 0 ? (
                    order.relatedDocuments.map((doc) => (
                      <ListGroup.Item key={doc.id}>
                        <FaFileInvoice className="me-2" />
                        <a href={doc.url}>{doc.name}</a>
                      </ListGroup.Item>
                    ))
                  ) : (
                    <ListGroup.Item>No related documents</ListGroup.Item>
                  )}
                </ListGroup>
              </Card>
            </Col>
          </Row>
        </>
      )}
      
      {/* Receive Items Modal */}
      <Modal 
        show={showReceiveModal} 
        onHide={() => setShowReceiveModal(false)}
        size="lg"
      >
        <Modal.Header closeButton>
          <Modal.Title>Receive Items - Order #{order?.orderNumber}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Table responsive>
              <thead>
                <tr>
                  <th>
                    <Form.Check
                      type="checkbox"
                      onChange={handleSelectAllItems}
                      label=""
                    />
                  </th>
                  <th>Part #</th>
                  <th>Description</th>
                  <th className="text-center">Ordered</th>
                  <th className="text-center">Previously Received</th>
                  <th className="text-center">Remaining</th>
                  <th className="text-center">Receiving Now</th>
                </tr>
              </thead>
              <tbody>
                {order && order.items.map((item) => {
                  const remaining = getRemainingQuantity(item);
                  return (
                    <tr key={item.id} className={remaining === 0 ? 'table-success' : ''}>
                      <td>
                        <Form.Check
                          type="checkbox"
                          checked={selectedItems[item.id]?.isSelected || false}
                          onChange={(e) => handleSelectItem(item.id, e)}
                          disabled={remaining === 0}
                        />
                      </td>
                      <td>{item.partNumber}</td>
                      <td>{item.partName}</td>
                      <td className="text-center">{item.quantity}</td>
                      <td className="text-center">{item.receivedQuantity}</td>
                      <td className="text-center">{remaining}</td>
                      <td>
                        <Form.Control
                          type="number"
                          min="0"
                          max={remaining}
                          value={selectedItems[item.id]?.receivedQuantity || 0}
                          onChange={(e) => handleReceivedQuantityChange(item.id, parseInt(e.target.value) || 0)}
                          disabled={!selectedItems[item.id]?.isSelected || remaining === 0}
                          style={{ width: '80px' }}
                        />
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </Table>
            
            <Form.Group className="mt-3">
              <Form.Label>Notes</Form.Label>
              <Form.Control
                as="textarea"
                rows={3}
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Enter any notes about this receiving"
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowReceiveModal(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleReceiveOrder}>
            Confirm Receipt
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default OrderDetail;
