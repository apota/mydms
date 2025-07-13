import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Card, Alert } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowLeft, faSave } from '@fortawesome/free-solid-svg-icons';
import inventoryService from '../../services/inventoryService';
import './VehicleForm.css';

const VehicleForm = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEditMode = !!id;
    
    const [vehicle, setVehicle] = useState({
        vin: '',
        stockNumber: '',
        make: '',
        model: '',
        year: new Date().getFullYear(),
        trim: '',
        bodyStyle: '',
        exteriorColor: '',
        interiorColor: '',
        mileage: 0,
        fuelType: '',
        transmission: '',
        engine: '',
        cylinders: '',
        driveTrain: '',
        status: 'Available',
        condition: 'Used',
        features: [],
        locationId: ''
    });
    
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [validated, setValidated] = useState(false);
    const [locations, setLocations] = useState([]);
    
    useEffect(() => {
        fetchLocations();
        
        if (isEditMode) {
            fetchVehicleData();
        }
    }, [id]);
    
    const fetchLocations = async () => {
        try {
            const locationsData = await inventoryService.getLocations();
            setLocations(locationsData);
        } catch (err) {
            console.error('Failed to fetch locations:', err);
        }
    };
    
    const fetchVehicleData = async () => {
        try {
            setLoading(true);
            const vehicleData = await inventoryService.getVehicleById(id);
            setVehicle(vehicleData);
            setLoading(false);
        } catch (err) {
            setError('Failed to load vehicle data. ' + err.message);
            setLoading(false);
        }
    };
    
    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setVehicle({
            ...vehicle,
            [name]: value
        });
    };
    
    const handleNumberChange = (e) => {
        const { name, value } = e.target;
        setVehicle({
            ...vehicle,
            [name]: value === '' ? '' : Number(value)
        });
    };
    
    const handleFeaturesChange = (e) => {
        const { value } = e.target;
        const featuresArray = value.split('\n').filter(feature => feature.trim() !== '');
        setVehicle({
            ...vehicle,
            features: featuresArray
        });
    };
    
    const handleSubmit = async (e) => {
        e.preventDefault();
        
        const form = e.currentTarget;
        if (form.checkValidity() === false) {
            e.stopPropagation();
            setValidated(true);
            return;
        }
        
        setLoading(true);
        setError(null);
        setSuccess(null);
        
        try {
            if (isEditMode) {
                await inventoryService.updateVehicle(id, vehicle);
                setSuccess('Vehicle updated successfully');
            } else {
                const newVehicle = await inventoryService.createVehicle(vehicle);
                setSuccess('Vehicle created successfully');
                setTimeout(() => {
                    navigate(`/inventory/vehicles/${newVehicle.id}`);
                }, 1500);
            }
            setLoading(false);
        } catch (err) {
            setError('Error saving vehicle: ' + err.message);
            setLoading(false);
        }
    };
    
    if (loading && isEditMode) {
        return (
            <Container className="py-4 text-center">
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </Container>
        );
    }
    
    return (
        <Container className="vehicle-form-container py-4">
            <Button variant="outline-secondary" className="mb-3" onClick={() => navigate(-1)}>
                <FontAwesomeIcon icon={faArrowLeft} /> Back
            </Button>
            
            <h1 className="mb-4">{isEditMode ? 'Edit Vehicle' : 'Add New Vehicle'}</h1>
            
            {error && (
                <Alert variant="danger" className="mb-4">
                    {error}
                </Alert>
            )}
            
            {success && (
                <Alert variant="success" className="mb-4">
                    {success}
                </Alert>
            )}
            
            <Form noValidate validated={validated} onSubmit={handleSubmit}>
                <Card className="mb-4">
                    <Card.Header>
                        <h5 className="mb-0">Basic Information</h5>
                    </Card.Header>
                    <Card.Body>
                        <Row>
                            <Col md={6}>
                                <Form.Group className="mb-3" controlId="vin">
                                    <Form.Label>VIN <span className="text-danger">*</span></Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="vin"
                                        value={vehicle.vin}
                                        onChange={handleInputChange}
                                        required
                                        maxLength={17}
                                        placeholder="Enter 17-character VIN"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        Please enter a valid VIN.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            <Col md={6}>
                                <Form.Group className="mb-3" controlId="stockNumber">
                                    <Form.Label>Stock Number <span className="text-danger">*</span></Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="stockNumber"
                                        value={vehicle.stockNumber}
                                        onChange={handleInputChange}
                                        required
                                        placeholder="Enter stock number"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        Please enter a stock number.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                        </Row>
                        
                        <Row>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="year">
                                    <Form.Label>Year <span className="text-danger">*</span></Form.Label>
                                    <Form.Control
                                        type="number"
                                        name="year"
                                        value={vehicle.year}
                                        onChange={handleNumberChange}
                                        required
                                        min={1900}
                                        max={new Date().getFullYear() + 1}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        Please enter a valid year.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="make">
                                    <Form.Label>Make <span className="text-danger">*</span></Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="make"
                                        value={vehicle.make}
                                        onChange={handleInputChange}
                                        required
                                        placeholder="Enter make"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        Please enter a make.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="model">
                                    <Form.Label>Model <span className="text-danger">*</span></Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="model"
                                        value={vehicle.model}
                                        onChange={handleInputChange}
                                        required
                                        placeholder="Enter model"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        Please enter a model.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                        </Row>
                        
                        <Row>
                            <Col md={6}>
                                <Form.Group className="mb-3" controlId="trim">
                                    <Form.Label>Trim</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="trim"
                                        value={vehicle.trim}
                                        onChange={handleInputChange}
                                        placeholder="Enter trim level"
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={6}>
                                <Form.Group className="mb-3" controlId="bodyStyle">
                                    <Form.Label>Body Style</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="bodyStyle"
                                        value={vehicle.bodyStyle}
                                        onChange={handleInputChange}
                                        placeholder="Enter body style"
                                    />
                                </Form.Group>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>
                
                <Card className="mb-4">
                    <Card.Header>
                        <h5 className="mb-0">Details & Specifications</h5>
                    </Card.Header>
                    <Card.Body>
                        <Row>
                            <Col md={6}>
                                <Form.Group className="mb-3" controlId="mileage">
                                    <Form.Label>Mileage <span className="text-danger">*</span></Form.Label>
                                    <Form.Control
                                        type="number"
                                        name="mileage"
                                        value={vehicle.mileage}
                                        onChange={handleNumberChange}
                                        required
                                        min={0}
                                        placeholder="Enter mileage"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        Please enter a valid mileage.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            <Col md={6}>
                                <Form.Group className="mb-3" controlId="transmission">
                                    <Form.Label>Transmission</Form.Label>
                                    <Form.Select
                                        name="transmission"
                                        value={vehicle.transmission}
                                        onChange={handleInputChange}
                                    >
                                        <option value="">Select transmission</option>
                                        <option value="Automatic">Automatic</option>
                                        <option value="Manual">Manual</option>
                                        <option value="CVT">CVT</option>
                                        <option value="Dual Clutch">Dual Clutch</option>
                                        <option value="Other">Other</option>
                                    </Form.Select>
                                </Form.Group>
                            </Col>
                        </Row>
                        
                        <Row>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="exteriorColor">
                                    <Form.Label>Exterior Color</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="exteriorColor"
                                        value={vehicle.exteriorColor}
                                        onChange={handleInputChange}
                                        placeholder="Enter exterior color"
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="interiorColor">
                                    <Form.Label>Interior Color</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="interiorColor"
                                        value={vehicle.interiorColor}
                                        onChange={handleInputChange}
                                        placeholder="Enter interior color"
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="fuelType">
                                    <Form.Label>Fuel Type</Form.Label>
                                    <Form.Select
                                        name="fuelType"
                                        value={vehicle.fuelType}
                                        onChange={handleInputChange}
                                    >
                                        <option value="">Select fuel type</option>
                                        <option value="Gasoline">Gasoline</option>
                                        <option value="Diesel">Diesel</option>
                                        <option value="Hybrid">Hybrid</option>
                                        <option value="Electric">Electric</option>
                                        <option value="Plug-in Hybrid">Plug-in Hybrid</option>
                                        <option value="Other">Other</option>
                                    </Form.Select>
                                </Form.Group>
                            </Col>
                        </Row>
                        
                        <Row>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="engine">
                                    <Form.Label>Engine</Form.Label>
                                    <Form.Control
                                        type="text"
                                        name="engine"
                                        value={vehicle.engine}
                                        onChange={handleInputChange}
                                        placeholder="E.g., 2.0L Turbo"
                                    />
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="cylinders">
                                    <Form.Label>Cylinders</Form.Label>
                                    <Form.Select
                                        name="cylinders"
                                        value={vehicle.cylinders}
                                        onChange={handleInputChange}
                                    >
                                        <option value="">Select cylinders</option>
                                        <option value="3">3</option>
                                        <option value="4">4</option>
                                        <option value="6">6</option>
                                        <option value="8">8</option>
                                        <option value="10">10</option>
                                        <option value="12">12</option>
                                        <option value="Electric">Electric (N/A)</option>
                                    </Form.Select>
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="driveTrain">
                                    <Form.Label>Drivetrain</Form.Label>
                                    <Form.Select
                                        name="driveTrain"
                                        value={vehicle.driveTrain}
                                        onChange={handleInputChange}
                                    >
                                        <option value="">Select drivetrain</option>
                                        <option value="FWD">FWD</option>
                                        <option value="RWD">RWD</option>
                                        <option value="AWD">AWD</option>
                                        <option value="4WD">4WD</option>
                                    </Form.Select>
                                </Form.Group>
                            </Col>
                        </Row>
                        
                        <Row>
                            <Col md={12}>
                                <Form.Group className="mb-3" controlId="features">
                                    <Form.Label>Features (one per line)</Form.Label>
                                    <Form.Control
                                        as="textarea"
                                        rows={5}
                                        name="features"
                                        value={vehicle.features.join('\n')}
                                        onChange={handleFeaturesChange}
                                        placeholder="Enter features, one per line"
                                    />
                                </Form.Group>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>
                
                <Card className="mb-4">
                    <Card.Header>
                        <h5 className="mb-0">Status & Location</h5>
                    </Card.Header>
                    <Card.Body>
                        <Row>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="condition">
                                    <Form.Label>Condition <span className="text-danger">*</span></Form.Label>
                                    <Form.Select
                                        name="condition"
                                        value={vehicle.condition}
                                        onChange={handleInputChange}
                                        required
                                    >
                                        <option value="New">New</option>
                                        <option value="Used">Used</option>
                                        <option value="Certified">Certified</option>
                                    </Form.Select>
                                    <Form.Control.Feedback type="invalid">
                                        Please select a condition.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="status">
                                    <Form.Label>Status <span className="text-danger">*</span></Form.Label>
                                    <Form.Select
                                        name="status"
                                        value={vehicle.status}
                                        onChange={handleInputChange}
                                        required
                                    >
                                        <option value="Available">Available</option>
                                        <option value="In-Transit">In-Transit</option>
                                        <option value="On-Hold">On-Hold</option>
                                        <option value="Service">Service</option>
                                        <option value="Sold">Sold</option>
                                        <option value="Wholesale">Wholesale</option>
                                    </Form.Select>
                                    <Form.Control.Feedback type="invalid">
                                        Please select a status.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            <Col md={4}>
                                <Form.Group className="mb-3" controlId="locationId">
                                    <Form.Label>Location <span className="text-danger">*</span></Form.Label>
                                    <Form.Select
                                        name="locationId"
                                        value={vehicle.locationId}
                                        onChange={handleInputChange}
                                        required
                                    >
                                        <option value="">Select location</option>
                                        {locations.map(location => (
                                            <option key={location.id} value={location.id}>
                                                {location.name}
                                            </option>
                                        ))}
                                    </Form.Select>
                                    <Form.Control.Feedback type="invalid">
                                        Please select a location.
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>
                
                <div className="d-flex justify-content-end">
                    <Button variant="secondary" className="me-2" onClick={() => navigate(-1)}>
                        Cancel
                    </Button>
                    <Button variant="primary" type="submit" disabled={loading}>
                        <FontAwesomeIcon icon={faSave} className="me-1" />
                        {loading ? 'Saving...' : 'Save Vehicle'}
                    </Button>
                </div>
            </Form>
        </Container>
    );
};

export default VehicleForm;
