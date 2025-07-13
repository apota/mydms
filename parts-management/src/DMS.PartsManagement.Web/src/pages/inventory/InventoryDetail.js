import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Form, Badge, Spinner, ListGroup, Modal } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { FaHistory, FaEdit, FaWarehouse, FaExchangeAlt, FaChartLine } from 'react-icons/fa';
import api from '../../services/api';

const InventoryDetail = () => {
  const { partId, locationId } = useParams();
  const [inventory, setInventory] = useState(null);
  const [part, setPart] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showAdjustModal, setShowAdjustModal] = useState(false);
  const [adjustQuantity, setAdjustQuantity] = useState(0);
  const [adjustNotes, setAdjustNotes] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        // Fetch inventory details
        const inventoryData = await api.inventory.getInventoryByPartAndLocation(
          parseInt(partId), 
          parseInt(locationId)
        );
        setInventory(inventoryData);
        
        // Fetch part details
        const partData = await api.parts.getPartById(parseInt(partId));
        setPart(partData);
        
        // Fetch recent transactions for this inventory
        // In a real application, you would fetch from an API endpoint
        // For now, we'll use placeholder data
        setTransactions([
          {
            id: 1,
            date: new Date(2025, 5, 20),
            type: 'Adjustment',
            quantity: 5,
            notes: 'Manual adjustment',
            user: 'Admin User'
          },
          {
            id: 2,
            date: new Date(2025, 5, 19),
            type: 'Order Fulfillment',
            quantity: -2,
            notes: 'Service order #12345',
            user: 'Service Tech'
          },
          {
            id: 3,
            date: new Date(2025, 5, 15),
            type: 'Receiving',
            quantity: 10,
            notes: 'PO #54321 received',
            user: 'Warehouse Staff'
          }
        ]);
        
        setError(null);
      } catch (err) {
        console.error('Error fetching inventory details:', err);
        setError('Failed to load inventory details. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    if (partId && locationId) {
      fetchData();
    }
  }, [partId, locationId]);

  const handleAdjustInventory = async () => {
    try {
      if (!adjustQuantity || adjustQuantity === 0) {
        return;
      }
      
      await api.inventory.updateQuantity(
        parseInt(partId),
        parseInt(locationId),
        parseInt(adjustQuantity),
        adjustNotes
      );
      
      // Refresh inventory data after adjustment
      const updatedInventory = await api.inventory.getInventoryByPartAndLocation(
        parseInt(partId), 
        parseInt(locationId)
      );
      setInventory(updatedInventory);
      
      // Add new transaction to list for UI update
      setTransactions([
        {
          id: Date.now(),
          date: new Date(),
          type: 'Adjustment',
          quantity: parseInt(adjustQuantity),
          notes: adjustNotes,
          user: 'Current User' // In a real app, get this from auth context
        },
        ...transactions
      ]);
      
      // Reset form and close modal
      setAdjustQuantity(0);
      setAdjustNotes('');
      setShowAdjustModal(false);
    } catch (err) {
      console.error('Error adjusting inventory:', err);
      alert('Failed to adjust inventory. Please try again.');
    }
  };

  const getStockStatusBadge = (quantity, minStockLevel) => {
    if (quantity <= 0) {
      return <Badge bg="danger">Out of Stock</Badge>;
    } else if (quantity <= minStockLevel) {
      return <Badge bg="warning" text="dark">Low Stock</Badge>;
    } else {
      return <Badge bg="success">In Stock</Badge>;
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
            <Button variant="secondary" onClick={() => navigate('/inventory')}>
              Back to Inventory List
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
            onClick={() => navigate('/inventory')}
          >
            &larr; Back to Inventory
          </Button>
          <h2 className="mb-4">Inventory Details</h2>
        </Col>
      </Row>
      
      {inventory && part && (
        <>
          <Row>
            <Col md={8}>
              <Card className="mb-4">
                <Card.Body>
                  <Row>
                    <Col md={6}>
                      <h5>{part.name}</h5>
                      <p className="text-muted">{part.partNumber}</p>
                      <p>{part.description}</p>
                    </Col>
                    <Col md={6} className="text-end">
                      <h3>
                        Quantity: {inventory.quantity} {getStockStatusBadge(inventory.quantity, inventory.minStockLevel)}
                      </h3>
                      <p>Min Stock Level: {inventory.minStockLevel}</p>
                      <p>Location: {inventory.locationName}</p>
                      <p>Last Updated: {new Date(inventory.lastUpdated).toLocaleString()}</p>
                    </Col>
                  </Row>
                </Card.Body>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="mb-4">
                <Card.Header>Actions</Card.Header>
                <Card.Body>
                  <div className="d-grid gap-2">
                    <Button 
                      variant="primary" 
                      onClick={() => setShowAdjustModal(true)}
                    >
                      <FaEdit className="me-2" /> Adjust Quantity
                    </Button>
                    <Button variant="secondary">
                      <FaWarehouse className="me-2" /> Transfer Inventory
                    </Button>
                    <Button variant="outline-secondary">
                      <FaChartLine className="me-2" /> View Forecasts
                    </Button>
                  </div>
                </Card.Body>
              </Card>
            </Col>
          </Row>

          <Row className="mb-5">
            <Col>
              <Card>
                <Card.Header className="d-flex justify-content-between align-items-center">
                  <h5 className="mb-0">
                    <FaHistory className="me-2" /> Recent Transactions
                  </h5>
                </Card.Header>
                <ListGroup variant="flush">
                  {transactions.length === 0 ? (
                    <ListGroup.Item>No recent transactions found.</ListGroup.Item>
                  ) : (
                    transactions.map((transaction) => (
                      <ListGroup.Item key={transaction.id}>
                        <Row>
                          <Col md={3}>
                            {transaction.date.toLocaleDateString()} {transaction.date.toLocaleTimeString()}
                          </Col>
                          <Col md={2}>
                            <Badge bg={transaction.quantity > 0 ? "success" : "danger"}>
                              {transaction.quantity > 0 ? '+' : ''}{transaction.quantity}
                            </Badge>{' '}
                            {transaction.type}
                          </Col>
                          <Col md={5}>
                            {transaction.notes}
                          </Col>
                          <Col md={2}>
                            By: {transaction.user}
                          </Col>
                        </Row>
                      </ListGroup.Item>
                    ))
                  )}
                </ListGroup>
              </Card>
            </Col>
          </Row>
        </>
      )}

      {/* Adjust Inventory Modal */}
      <Modal show={showAdjustModal} onHide={() => setShowAdjustModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Adjust Inventory Quantity</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Part</Form.Label>
              <Form.Control type="text" value={part?.name} readOnly />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Current Quantity</Form.Label>
              <Form.Control type="text" value={inventory?.quantity} readOnly />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Adjustment</Form.Label>
              <Form.Control 
                type="number" 
                value={adjustQuantity}
                onChange={(e) => setAdjustQuantity(e.target.value)}
                placeholder="Enter positive or negative value"
              />
              <Form.Text className="text-muted">
                Use positive values to add, negative to subtract
              </Form.Text>
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Notes</Form.Label>
              <Form.Control 
                as="textarea" 
                rows={3}
                value={adjustNotes}
                onChange={(e) => setAdjustNotes(e.target.value)}
                placeholder="Reason for adjustment"
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowAdjustModal(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleAdjustInventory}>
            Save Changes
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default InventoryDetail;
