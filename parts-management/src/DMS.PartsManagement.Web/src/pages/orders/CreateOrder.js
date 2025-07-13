import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Form, Button, Table, InputGroup, Spinner, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaPlus, FaTrash, FaSearch } from 'react-icons/fa';
import api from '../../services/api';

const CreateOrder = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);
  const [suppliers, setSuppliers] = useState([]);
  const [parts, setParts] = useState([]);
  const [filteredParts, setFilteredParts] = useState([]);
  const [partSearch, setPartSearch] = useState('');
  const [showPartSearch, setShowPartSearch] = useState(false);
  
  const [order, setOrder] = useState({
    supplierId: '',
    expectedDeliveryDate: '',
    notes: '',
    items: []
  });

  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        setLoading(true);
        // Fetch suppliers
        const suppliersData = await api.suppliers.getAllSuppliers();
        setSuppliers(suppliersData);
        
        // Fetch parts
        const partsData = await api.parts.getAllParts();
        setParts(partsData);
        setFilteredParts(partsData);
        
        setError(null);
      } catch (err) {
        console.error('Error fetching initial data:', err);
        setError('Failed to load required data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchInitialData();
  }, []);

  useEffect(() => {
    if (partSearch) {
      const filtered = parts.filter(part => 
        part.name.toLowerCase().includes(partSearch.toLowerCase()) || 
        part.partNumber.toLowerCase().includes(partSearch.toLowerCase())
      );
      setFilteredParts(filtered);
    } else {
      setFilteredParts(parts);
    }
  }, [partSearch, parts]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setOrder({
      ...order,
      [name]: value
    });
  };

  const addOrderItem = (part) => {
    const newItem = {
      id: Date.now(), // Temporary ID for UI purposes
      partId: part.id,
      partNumber: part.partNumber,
      partName: part.name,
      quantity: 1,
      unitPrice: part.defaultPrice || 0
    };
    
    setOrder({
      ...order,
      items: [...order.items, newItem]
    });
    
    setShowPartSearch(false);
    setPartSearch('');
  };

  const removeOrderItem = (itemId) => {
    setOrder({
      ...order,
      items: order.items.filter(item => item.id !== itemId)
    });
  };

  const updateOrderItem = (itemId, field, value) => {
    const updatedItems = order.items.map(item => {
      if (item.id === itemId) {
        return { ...item, [field]: value };
      }
      return item;
    });
    
    setOrder({
      ...order,
      items: updatedItems
    });
  };

  const calculateSubtotal = () => {
    return order.items.reduce((total, item) => {
      return total + (item.quantity * item.unitPrice);
    }, 0);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!order.supplierId) {
      setError('Please select a supplier');
      return;
    }
    
    if (order.items.length === 0) {
      setError('Please add at least one item to the order');
      return;
    }
    
    try {
      setSubmitting(true);
      setError(null);
      
      // Format order items for API
      const formattedItems = order.items.map(item => ({
        partId: item.partId,
        quantity: parseInt(item.quantity),
        unitPrice: parseFloat(item.unitPrice)
      }));
      
      const orderData = {
        supplierId: parseInt(order.supplierId),
        expectedDeliveryDate: order.expectedDeliveryDate || null,
        notes: order.notes,
        items: formattedItems
      };
      
      // Create order
      const result = await api.orders.createOrder(orderData);
      
      setSuccess(true);
      
      // Navigate to the new order after a short delay
      setTimeout(() => {
        navigate(`/orders/${result.id}`);
      }, 1500);
      
    } catch (err) {
      console.error('Error creating order:', err);
      setError('Failed to create order. Please check your data and try again.');
    } finally {
      setSubmitting(false);
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

  return (
    <Container fluid className="mt-4">
      <Row>
        <Col>
          <h2 className="mb-4">Create New Purchase Order</h2>
        </Col>
      </Row>
      
      {error && (
        <Row className="mb-3">
          <Col>
            <Alert variant="danger">{error}</Alert>
          </Col>
        </Row>
      )}
      
      {success && (
        <Row className="mb-3">
          <Col>
            <Alert variant="success">
              Order created successfully! Redirecting to order details...
            </Alert>
          </Col>
        </Row>
      )}
      
      <Form onSubmit={handleSubmit}>
        <Row>
          <Col md={8}>
            <Card className="mb-4">
              <Card.Header>Order Information</Card.Header>
              <Card.Body>
                <Row className="mb-3">
                  <Col md={6}>
                    <Form.Group>
                      <Form.Label>Supplier</Form.Label>
                      <Form.Select
                        name="supplierId"
                        value={order.supplierId}
                        onChange={handleInputChange}
                        required
                      >
                        <option value="">Select Supplier</option>
                        {suppliers.map(supplier => (
                          <option key={supplier.id} value={supplier.id}>
                            {supplier.name}
                          </option>
                        ))}
                      </Form.Select>
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group>
                      <Form.Label>Expected Delivery Date</Form.Label>
                      <Form.Control
                        type="date"
                        name="expectedDeliveryDate"
                        value={order.expectedDeliveryDate}
                        onChange={handleInputChange}
                      />
                    </Form.Group>
                  </Col>
                </Row>
                <Row>
                  <Col>
                    <Form.Group>
                      <Form.Label>Notes</Form.Label>
                      <Form.Control
                        as="textarea"
                        rows={3}
                        name="notes"
                        value={order.notes}
                        onChange={handleInputChange}
                        placeholder="Enter any additional notes for this order"
                      />
                    </Form.Group>
                  </Col>
                </Row>
              </Card.Body>
            </Card>
            
            <Card className="mb-4">
              <Card.Header className="d-flex justify-content-between align-items-center">
                <h5 className="mb-0">Order Items</h5>
                <Button 
                  variant="outline-primary" 
                  size="sm"
                  onClick={() => setShowPartSearch(true)}
                >
                  <FaPlus className="me-1" /> Add Item
                </Button>
              </Card.Header>
              <Card.Body>
                {showPartSearch && (
                  <div className="mb-4">
                    <h6>Search for Parts</h6>
                    <Row className="mb-3">
                      <Col>
                        <InputGroup>
                          <InputGroup.Text>
                            <FaSearch />
                          </InputGroup.Text>
                          <Form.Control
                            placeholder="Search by part name or number"
                            value={partSearch}
                            onChange={(e) => setPartSearch(e.target.value)}
                            autoFocus
                          />
                        </InputGroup>
                      </Col>
                    </Row>
                    <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
                      <Table hover size="sm">
                        <thead>
                          <tr>
                            <th>Part #</th>
                            <th>Name</th>
                            <th>Default Price</th>
                            <th>Action</th>
                          </tr>
                        </thead>
                        <tbody>
                          {filteredParts.length === 0 ? (
                            <tr>
                              <td colSpan="4" className="text-center">No parts found</td>
                            </tr>
                          ) : (
                            filteredParts.map(part => (
                              <tr key={part.id}>
                                <td>{part.partNumber}</td>
                                <td>{part.name}</td>
                                <td>${part.defaultPrice?.toFixed(2) || '0.00'}</td>
                                <td>
                                  <Button
                                    variant="outline-success"
                                    size="sm"
                                    onClick={() => addOrderItem(part)}
                                  >
                                    Add
                                  </Button>
                                </td>
                              </tr>
                            ))
                          )}
                        </tbody>
                      </Table>
                    </div>
                    <div className="text-end mt-2">
                      <Button 
                        variant="outline-secondary"
                        size="sm"
                        onClick={() => setShowPartSearch(false)}
                      >
                        Close
                      </Button>
                    </div>
                  </div>
                )}
                
                <Table responsive>
                  <thead>
                    <tr>
                      <th>Part #</th>
                      <th>Description</th>
                      <th className="text-center" style={{ width: '120px' }}>Quantity</th>
                      <th className="text-end" style={{ width: '150px' }}>Unit Price</th>
                      <th className="text-end" style={{ width: '150px' }}>Total</th>
                      <th style={{ width: '80px' }}></th>
                    </tr>
                  </thead>
                  <tbody>
                    {order.items.length === 0 ? (
                      <tr>
                        <td colSpan="6" className="text-center">No items added. Click "Add Item" to add parts to this order.</td>
                      </tr>
                    ) : (
                      order.items.map((item) => (
                        <tr key={item.id}>
                          <td>{item.partNumber}</td>
                          <td>{item.partName}</td>
                          <td>
                            <Form.Control
                              type="number"
                              min="1"
                              value={item.quantity}
                              onChange={(e) => updateOrderItem(item.id, 'quantity', parseInt(e.target.value) || 1)}
                              style={{ width: '80px' }}
                            />
                          </td>
                          <td>
                            <InputGroup size="sm">
                              <InputGroup.Text>$</InputGroup.Text>
                              <Form.Control
                                type="number"
                                min="0"
                                step="0.01"
                                value={item.unitPrice}
                                onChange={(e) => updateOrderItem(item.id, 'unitPrice', parseFloat(e.target.value) || 0)}
                              />
                            </InputGroup>
                          </td>
                          <td className="text-end">
                            ${(item.quantity * item.unitPrice).toFixed(2)}
                          </td>
                          <td>
                            <Button
                              variant="outline-danger"
                              size="sm"
                              onClick={() => removeOrderItem(item.id)}
                            >
                              <FaTrash />
                            </Button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                  <tfoot>
                    <tr>
                      <th colSpan="4" className="text-end">Subtotal:</th>
                      <th className="text-end">${calculateSubtotal().toFixed(2)}</th>
                      <th></th>
                    </tr>
                  </tfoot>
                </Table>
              </Card.Body>
            </Card>
          </Col>
          
          <Col md={4}>
            <Card className="mb-4">
              <Card.Header>Order Summary</Card.Header>
              <Card.Body>
                <Row className="mb-3">
                  <Col xs={6}>Supplier:</Col>
                  <Col xs={6} className="text-end">
                    {order.supplierId ? suppliers.find(s => s.id.toString() === order.supplierId.toString())?.name : 'Not selected'}
                  </Col>
                </Row>
                <Row className="mb-3">
                  <Col xs={6}>Total Items:</Col>
                  <Col xs={6} className="text-end">{order.items.length}</Col>
                </Row>
                <Row className="mb-3">
                  <Col xs={6}>Total Quantity:</Col>
                  <Col xs={6} className="text-end">
                    {order.items.reduce((sum, item) => sum + parseInt(item.quantity), 0)}
                  </Col>
                </Row>
                <Row>
                  <Col xs={6}><strong>Order Total:</strong></Col>
                  <Col xs={6} className="text-end">
                    <strong>${calculateSubtotal().toFixed(2)}</strong>
                  </Col>
                </Row>
              </Card.Body>
            </Card>
            
            <div className="d-grid gap-2">
              <Button 
                variant="primary" 
                type="submit"
                disabled={submitting || order.items.length === 0}
              >
                {submitting ? (
                  <>
                    <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />
                    Creating...
                  </>
                ) : (
                  'Create Purchase Order'
                )}
              </Button>
              <Button 
                variant="outline-secondary" 
                onClick={() => navigate('/orders')}
                disabled={submitting}
              >
                Cancel
              </Button>
            </div>
          </Col>
        </Row>
      </Form>
    </Container>
  );
};

export default CreateOrder;
