import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Form, InputGroup, Spinner, Badge, Modal } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { FaSearch, FaPlus, FaEdit, FaPhone, FaEnvelope } from 'react-icons/fa';
import api from '../../services/api';

const SuppliersList = () => {
  const [suppliers, setSuppliers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [newSupplier, setNewSupplier] = useState({
    name: '',
    contactName: '',
    email: '',
    phone: '',
    address: '',
    website: '',
    notes: ''
  });
  const [validationError, setValidationError] = useState({});
  const navigate = useNavigate();

  useEffect(() => {
    const fetchSuppliers = async () => {
      try {
        setLoading(true);
        const suppliersData = await api.suppliers.getAllSuppliers();
        setSuppliers(suppliersData);
        setError(null);
      } catch (err) {
        console.error('Error fetching suppliers:', err);
        setError('Failed to load suppliers data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchSuppliers();
  }, []);

  const filteredSuppliers = suppliers.filter(supplier => {
    return (
      supplier.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      (supplier.contactName && supplier.contactName.toLowerCase().includes(searchTerm.toLowerCase())) ||
      (supplier.email && supplier.email.toLowerCase().includes(searchTerm.toLowerCase()))
    );
  });

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setNewSupplier({
      ...newSupplier,
      [name]: value
    });
    
    // Clear validation error when field is updated
    if (validationError[name]) {
      setValidationError({
        ...validationError,
        [name]: null
      });
    }
  };

  const validateForm = () => {
    const errors = {};
    
    if (!newSupplier.name.trim()) {
      errors.name = 'Supplier name is required';
    }
    
    if (newSupplier.email && !/^\S+@\S+\.\S+$/.test(newSupplier.email)) {
      errors.email = 'Invalid email address';
    }
    
    return errors;
  };

  const handleAddSupplier = async () => {
    const errors = validateForm();
    
    if (Object.keys(errors).length > 0) {
      setValidationError(errors);
      return;
    }
    
    try {
      const result = await api.suppliers.createSupplier(newSupplier);
      
      // Add the new supplier to the list
      setSuppliers([...suppliers, { ...newSupplier, id: result.id }]);
      
      // Reset form and close modal
      setNewSupplier({
        name: '',
        contactName: '',
        email: '',
        phone: '',
        address: '',
        website: '',
        notes: ''
      });
      setShowAddModal(false);
    } catch (err) {
      console.error('Error creating supplier:', err);
      alert('Failed to create supplier. Please try again.');
    }
  };

  const handleViewSupplier = (id) => {
    navigate(`/suppliers/${id}`);
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
          <h2 className="mb-4">Suppliers</h2>
        </Col>
      </Row>
      <Row className="mb-4">
        <Col md={6}>
          <InputGroup>
            <InputGroup.Text>
              <FaSearch />
            </InputGroup.Text>
            <Form.Control
              placeholder="Search by name, contact or email..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </InputGroup>
        </Col>
        <Col md={6} className="d-flex justify-content-end">
          <Button variant="primary" onClick={() => setShowAddModal(true)}>
            <FaPlus className="me-2" /> Add Supplier
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
                      <th>Name</th>
                      <th>Contact Person</th>
                      <th>Email</th>
                      <th>Phone</th>
                      <th>Active Orders</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredSuppliers.length === 0 ? (
                      <tr>
                        <td colSpan="6" className="text-center">No suppliers found.</td>
                      </tr>
                    ) : (
                      filteredSuppliers.map((supplier) => (
                        <tr key={supplier.id}>
                          <td>{supplier.name}</td>
                          <td>{supplier.contactName || '-'}</td>
                          <td>
                            {supplier.email ? (
                              <a href={`mailto:${supplier.email}`}>
                                <FaEnvelope className="me-1" /> {supplier.email}
                              </a>
                            ) : '-'}
                          </td>
                          <td>
                            {supplier.phone ? (
                              <a href={`tel:${supplier.phone}`}>
                                <FaPhone className="me-1" /> {supplier.phone}
                              </a>
                            ) : '-'}
                          </td>
                          <td>{supplier.activeOrders || 0}</td>
                          <td>
                            <Button 
                              variant="outline-primary" 
                              size="sm"
                              onClick={() => handleViewSupplier(supplier.id)}
                              className="me-2"
                            >
                              View
                            </Button>
                            <Button 
                              variant="outline-secondary" 
                              size="sm"
                              as={Link}
                              to={`/orders/new?supplierId=${supplier.id}`}
                            >
                              New Order
                            </Button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
      
      {/* Add Supplier Modal */}
      <Modal show={showAddModal} onHide={() => setShowAddModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Add New Supplier</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Supplier Name*</Form.Label>
              <Form.Control
                type="text"
                name="name"
                value={newSupplier.name}
                onChange={handleInputChange}
                isInvalid={!!validationError.name}
              />
              <Form.Control.Feedback type="invalid">
                {validationError.name}
              </Form.Control.Feedback>
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Contact Person</Form.Label>
              <Form.Control
                type="text"
                name="contactName"
                value={newSupplier.contactName}
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
                    value={newSupplier.email}
                    onChange={handleInputChange}
                    isInvalid={!!validationError.email}
                  />
                  <Form.Control.Feedback type="invalid">
                    {validationError.email}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Phone</Form.Label>
                  <Form.Control
                    type="text"
                    name="phone"
                    value={newSupplier.phone}
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
                value={newSupplier.address}
                onChange={handleInputChange}
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Website</Form.Label>
              <Form.Control
                type="url"
                name="website"
                value={newSupplier.website}
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
                value={newSupplier.notes}
                onChange={handleInputChange}
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowAddModal(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleAddSupplier}>
            Add Supplier
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default SuppliersList;
