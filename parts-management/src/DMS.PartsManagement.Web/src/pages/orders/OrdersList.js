import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Form, InputGroup, Spinner, Badge, Dropdown } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { FaSearch, FaFilter, FaEllipsisV, FaFileExport } from 'react-icons/fa';
import api from '../../services/api';

const OrdersList = () => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('');
  const [selectedSupplier, setSelectedSupplier] = useState('');
  const [suppliers, setSuppliers] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const ordersPerPage = 10;
  const navigate = useNavigate();

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        setLoading(true);
        let ordersData;
        const skip = (currentPage - 1) * ordersPerPage;
        
        if (selectedStatus) {
          ordersData = await api.orders.getOrdersByStatus(selectedStatus, skip, ordersPerPage);
        } else if (selectedSupplier) {
          ordersData = await api.orders.getOrdersBySupplier(selectedSupplier, skip, ordersPerPage);
        } else {
          ordersData = await api.orders.getAllOrders(skip, ordersPerPage);
        }
        
        // In a real application, you would get pagination info from the API
        setOrders(ordersData);
        setTotalPages(Math.ceil(ordersData.length / ordersPerPage));
        setError(null);
      } catch (err) {
        console.error('Error fetching orders:', err);
        setError('Failed to load order data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    const fetchSuppliers = async () => {
      try {
        const suppliersData = await api.suppliers.getAllSuppliers();
        setSuppliers(suppliersData);
      } catch (err) {
        console.error('Error fetching suppliers:', err);
      }
    };

    fetchOrders();
    fetchSuppliers();
  }, [currentPage, selectedStatus, selectedSupplier]);

  const filteredOrders = orders.filter(order => {
    return (
      order.orderNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (order.supplierName && order.supplierName.toLowerCase().includes(searchTerm.toLowerCase()))
    );
  });

  const handleViewOrder = (id) => {
    navigate(`/orders/${id}`);
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
          <h2 className="mb-4">Purchase Orders</h2>
        </Col>
      </Row>
      <Row className="mb-4">
        <Col md={5}>
          <InputGroup>
            <InputGroup.Text>
              <FaSearch />
            </InputGroup.Text>
            <Form.Control
              placeholder="Search by order number or supplier..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </InputGroup>
        </Col>
        <Col md={3}>
          <Form.Select 
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value)}
          >
            <option value="">All Statuses</option>
            <option value="Draft">Draft</option>
            <option value="Submitted">Submitted</option>
            <option value="Processing">Processing</option>
            <option value="PartiallyReceived">Partially Received</option>
            <option value="Received">Received</option>
            <option value="Cancelled">Cancelled</option>
          </Form.Select>
        </Col>
        <Col md={3}>
          <Form.Select 
            value={selectedSupplier}
            onChange={(e) => setSelectedSupplier(e.target.value)}
          >
            <option value="">All Suppliers</option>
            {suppliers.map(supplier => (
              <option key={supplier.id} value={supplier.id}>
                {supplier.name}
              </option>
            ))}
          </Form.Select>
        </Col>
        <Col md={1} className="d-flex justify-content-end">
          <Button variant="outline-secondary">
            <FaFileExport />
          </Button>
        </Col>
      </Row>

      <Row>
        <Col md={12}>
          <Card>
            <Card.Body>
              {error ? (
                <div className="text-danger mb-3">{error}</div>
              ) : (
                <Table responsive hover>
                  <thead>
                    <tr>
                      <th>Order #</th>
                      <th>Date</th>
                      <th>Supplier</th>
                      <th>Total Items</th>
                      <th>Total Value</th>
                      <th>Expected Delivery</th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredOrders.length === 0 ? (
                      <tr>
                        <td colSpan="8" className="text-center">No orders found.</td>
                      </tr>
                    ) : (
                      filteredOrders.map((order) => (
                        <tr key={order.id}>
                          <td>{order.orderNumber}</td>
                          <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                          <td>{order.supplierName}</td>
                          <td>{order.totalItems}</td>
                          <td>${order.totalValue.toFixed(2)}</td>
                          <td>
                            {order.expectedDeliveryDate 
                              ? new Date(order.expectedDeliveryDate).toLocaleDateString()
                              : 'N/A'}
                          </td>
                          <td>{getStatusBadge(order.status)}</td>
                          <td>
                            <div className="d-flex">
                              <Button 
                                variant="outline-primary" 
                                size="sm"
                                onClick={() => handleViewOrder(order.id)}
                                className="me-2"
                              >
                                View
                              </Button>
                              <Dropdown>
                                <Dropdown.Toggle variant="outline-secondary" size="sm" id={`dropdown-${order.id}`}>
                                  <FaEllipsisV />
                                </Dropdown.Toggle>
                                <Dropdown.Menu>
                                  <Dropdown.Item onClick={() => navigate(`/orders/${order.id}/edit`)}>Edit</Dropdown.Item>
                                  <Dropdown.Item onClick={() => navigate(`/orders/${order.id}/receive`)}>Receive</Dropdown.Item>
                                  <Dropdown.Divider />
                                  <Dropdown.Item className="text-danger">Cancel</Dropdown.Item>
                                </Dropdown.Menu>
                              </Dropdown>
                            </div>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </Table>
              )}
            </Card.Body>
            <Card.Footer>
              <Row>
                <Col className="d-flex justify-content-between align-items-center">
                  <div>
                    Showing {Math.min((currentPage - 1) * ordersPerPage + 1, filteredOrders.length)} to {Math.min(currentPage * ordersPerPage, filteredOrders.length)} of {filteredOrders.length} orders
                  </div>
                  <div>
                    <Button 
                      variant="outline-secondary" 
                      size="sm" 
                      onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                      disabled={currentPage === 1}
                      className="me-2"
                    >
                      Previous
                    </Button>
                    <Button 
                      variant="outline-secondary" 
                      size="sm"
                      onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                      disabled={currentPage === totalPages}
                    >
                      Next
                    </Button>
                  </div>
                </Col>
              </Row>
            </Card.Footer>
          </Card>
        </Col>
      </Row>

      <Row className="mt-4 mb-5">
        <Col className="d-flex justify-content-end">
          <Link to="/orders/new">
            <Button variant="primary">
              Create New Order
            </Button>
          </Link>
        </Col>
      </Row>
    </Container>
  );
};

export default OrdersList;
