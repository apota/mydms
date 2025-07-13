import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { 
    Container, Row, Col, Card, Button, Badge,
    Nav, Tab, Form, Alert, Modal
} from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
    faArrowLeft, faEdit, faCar, faMoneyBill, faMapMarkerAlt,
    faImage, faFileAlt, faHistory, faExchangeAlt, faTags, 
    faChartLine, faChartBar, faDollarSign, faCheckCircle, faExclamationTriangle
} from '@fortawesome/free-solid-svg-icons';
import Slider from 'react-slick';
import {
    AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, Legend,
    BarChart, Bar, ResponsiveContainer, PieChart, Pie, Cell
} from 'recharts';
import inventoryService from '../../services/inventoryService';
import './VehicleDetailView.css';

const VehicleDetailView = () => {
    const { id } = useParams();
    const navigate = useNavigate();
      const [vehicle, setVehicle] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [activeTab, setActiveTab] = useState('details');
    const [marketData, setMarketData] = useState(null);
    const [marketDataLoading, setMarketDataLoading] = useState(false);
    
    // Modal states
    const [showTransferModal, setShowTransferModal] = useState(false);
    const [showStatusModal, setShowStatusModal] = useState(false);
    const [locationOptions, setLocationOptions] = useState([]);
    const [statusOptions] = useState([
        'Available', 'Sold', 'In-Transit', 'On-Hold', 'Service', 'Wholesale'
    ]);
    
    // Form states
    const [transferForm, setTransferForm] = useState({
        destinationLocationId: '',
        destinationZoneId: '',
        transferReason: ''
    });
    
    const [statusForm, setStatusForm] = useState({
        newStatus: '',
        statusChangeReason: '',
        additionalInfo: {}
    });
    
    // For image upload
    const [selectedImages, setSelectedImages] = useState([]);
    const [uploadProgress, setUploadProgress] = useState(0);
    const [uploadError, setUploadError] = useState(null);
    
    useEffect(() => {
        loadVehicleData();
        loadLocations();
    }, [id]);
    
    const loadVehicleData = async () => {
        try {
            setLoading(true);
            const data = await inventoryService.getVehicleById(id);
            setVehicle(data);
            setLoading(false);
        } catch (err) {
            setError('Failed to load vehicle data');
            setLoading(false);
            console.error('Error loading vehicle:', err);
        }
    };
    
    const loadLocations = async () => {
        try {
            const locations = await inventoryService.getLocations();
            setLocationOptions(locations);
        } catch (err) {
            console.error('Error loading locations:', err);
        }
    };
    
    const handleTransferSubmit = async (e) => {
        e.preventDefault();
        try {
            await inventoryService.transferVehicle(id, transferForm);
            setShowTransferModal(false);
            loadVehicleData(); // Reload vehicle data
            alert('Vehicle transferred successfully');
        } catch (err) {
            alert('Failed to transfer vehicle: ' + err.message);
        }
    };
    
    const handleStatusSubmit = async (e) => {
        e.preventDefault();
        try {
            await inventoryService.updateVehicleStatus(id, statusForm);
            setShowStatusModal(false);
            loadVehicleData(); // Reload vehicle data
            alert('Vehicle status updated successfully');
        } catch (err) {
            alert('Failed to update vehicle status: ' + err.message);
        }
    };
    
    const handleImageUpload = async (e) => {
        e.preventDefault();
        
        if (selectedImages.length === 0) {
            setUploadError('Please select at least one image');
            return;
        }
        
        const formData = new FormData();
        for (let i = 0; i < selectedImages.length; i++) {
            formData.append('images', selectedImages[i]);
        }
        
        try {
            setUploadProgress(0);
            setUploadError(null);
            
            await inventoryService.uploadVehicleImages(id, formData, (progress) => {
                setUploadProgress(progress);
            });
            
            setSelectedImages([]);
            setUploadProgress(0);
            loadVehicleData(); // Reload vehicle data to show new images
            alert('Images uploaded successfully');
        } catch (err) {
            setUploadError('Failed to upload images: ' + err.message);
            setUploadProgress(0);
        }
    };
    
    const handleFileSelect = (e) => {
        setSelectedImages(Array.from(e.target.files));
        setUploadError(null);
    };
    
    // Slider settings for vehicle images
    const sliderSettings = {
        dots: true,
        infinite: true,
        speed: 500,
        slidesToShow: 1,
        slidesToScroll: 1,
        autoplay: false
    };
    
    useEffect(() => {
        if (activeTab === 'marketAnalysis' && vehicle && !marketData) {
            loadMarketData();
        }
    }, [activeTab, vehicle, marketData]);
    
    const loadMarketData = async () => {
        if (!vehicle) return;
        
        try {
            setMarketDataLoading(true);
            
            // In a production environment, this would call the market pricing API
            // using the ExternalMarketDataService
            // For now, we're using mock data in the UI
            
            // Example API call that would be used:
            // const data = await inventoryService.getMarketAnalysis(vehicle.id);
            
            // Simulate an API delay
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // Mock data structure similar to what would come from the API
            const mockData = {
                marketComparison: {
                    vehiclePrice: vehicle.pricing?.internetPrice || 0,
                    marketAverage: (vehicle.pricing?.internetPrice || 0) + 2145,
                    priceDifference: -2145,
                    percentilePlacement: 45, // where in market this falls (0-100)
                },
                competitiveListings: [
                    { name: 'ABC Motors', price: 28450, distance: 12 },
                    { name: 'XYZ Auto', price: 27995, distance: 18 },
                    { name: 'Your Price', price: vehicle.pricing?.internetPrice || 26500, distance: 0, isYours: true },
                    { name: 'City Cars', price: 29500, distance: 22 },
                    { name: 'Best Deals', price: 26995, distance: 35 }
                ],
                priceRecommendation: {
                    minPrice: (vehicle.pricing?.internetPrice || 0) - 500,
                    maxPrice: (vehicle.pricing?.internetPrice || 0) + 1200,
                    similarVehicleCount: 32,
                    predictedDaysToSell: 28,
                    pricingStrategy: 'Competitive'
                },
                priceTrend: [
                    { date: 'Jan', price: 29500 },
                    { date: 'Feb', price: 29100 },
                    { date: 'Mar', price: 28700 },
                    { date: 'Apr', price: 28200 },
                    { date: 'May', price: 27800 },
                    { date: 'Jun', price: vehicle.pricing?.internetPrice || 26500 }
                ],
                priceChangePercent: -10.2
            };
            
            setMarketData(mockData);
            setMarketDataLoading(false);
        } catch (err) {
            console.error('Error loading market data:', err);
            setMarketDataLoading(false);
        }
    };
    
    if (loading) {
        return (
            <Container className="py-4 text-center">
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </Container>
        );
    }
    
    if (error || !vehicle) {
        return (
            <Container className="py-4">
                <Alert variant="danger">
                    {error || 'Failed to load vehicle data'}
                </Alert>
                <Button variant="secondary" onClick={() => navigate('/inventory')}>
                    <FontAwesomeIcon icon={faArrowLeft} /> Back to Inventory
                </Button>
            </Container>
        );
    }
    
    const getStatusBadgeClass = (status) => {
        switch (status.toLowerCase()) {
            case 'available':
                return 'success';
            case 'sold':
                return 'primary';
            case 'in-transit':
                return 'warning';
            case 'on-hold':
                return 'info';
            case 'service':
                return 'secondary';
            case 'wholesale':
                return 'danger';
            default:
                return 'dark';
        }
    };
    
    return (
        <Container fluid className="vehicle-detail-view py-4">
            <Row className="mb-4">
                <Col>
                    <Button variant="outline-secondary" className="mb-3" onClick={() => navigate('/inventory')}>
                        <FontAwesomeIcon icon={faArrowLeft} /> Back to Inventory
                    </Button>
                    <div className="d-flex justify-content-between align-items-start">
                        <div>
                            <h1 className="detail-title">
                                {vehicle.year} {vehicle.make} {vehicle.model} {vehicle.trim}
                            </h1>
                            <div className="detail-subtitle">
                                <span className="stock-number me-3">Stock # {vehicle.stockNumber}</span>
                                <span className="vin">VIN: {vehicle.vin}</span>
                            </div>
                        </div>
                        <div className="text-end">
                            <h2 className="vehicle-price">${vehicle.internetPrice?.toLocaleString()}</h2>
                            <Badge bg={getStatusBadgeClass(vehicle.status)} className="vehicle-status-badge">
                                {vehicle.status}
                            </Badge>
                        </div>
                    </div>
                </Col>
            </Row>
            
            <Row className="mb-4">
                <Col md={7}>
                    <Card className="image-gallery-card">
                        <Card.Body>
                            {vehicle.images?.length > 0 ? (
                                <Slider {...sliderSettings} className="vehicle-image-slider">
                                    {vehicle.images.map((image, index) => (
                                        <div key={index} className="vehicle-image-slide">
                                            <img 
                                                src={image.url} 
                                                alt={`${vehicle.year} ${vehicle.make} ${vehicle.model} - View ${index + 1}`}
                                                className="img-fluid"
                                            />
                                        </div>
                                    ))}
                                </Slider>
                            ) : (
                                <div className="no-image-placeholder">
                                    <FontAwesomeIcon icon={faCar} size="4x" />
                                    <p>No images available</p>
                                </div>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
                
                <Col md={5}>
                    <Card className="mb-3">
                        <Card.Header>
                            <h5 className="mb-0">
                                <FontAwesomeIcon icon={faCar} className="me-2" />
                                Quick Info
                            </h5>
                        </Card.Header>
                        <Card.Body>
                            <Row className="quick-info-grid">
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Mileage</div>
                                    <div className="info-value">{vehicle.mileage?.toLocaleString()} mi</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Exterior Color</div>
                                    <div className="info-value">{vehicle.exteriorColor}</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Interior Color</div>
                                    <div className="info-value">{vehicle.interiorColor}</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Body Style</div>
                                    <div className="info-value">{vehicle.bodyStyle}</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Fuel Type</div>
                                    <div className="info-value">{vehicle.fuelType}</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Transmission</div>
                                    <div className="info-value">{vehicle.transmission}</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Engine</div>
                                    <div className="info-value">{vehicle.engine}</div>
                                </Col>
                                <Col xs={6} className="quick-info-item">
                                    <div className="info-label">Drivetrain</div>
                                    <div className="info-value">{vehicle.driveTrain}</div>
                                </Col>
                            </Row>
                        </Card.Body>
                    </Card>
                    
                    <Card>
                        <Card.Header>
                            <h5 className="mb-0">
                                <FontAwesomeIcon icon={faTags} className="me-2" />
                                Status & Location
                            </h5>
                        </Card.Header>
                        <Card.Body>
                            <div className="detail-grid">
                                <div className="detail-item">
                                    <div className="detail-label">Status</div>
                                    <div className="detail-value">
                                        <Badge bg={getStatusBadgeClass(vehicle.status)}>
                                            {vehicle.status}
                                        </Badge>
                                    </div>
                                </div>
                                <div className="detail-item">
                                    <div className="detail-label">Days in Inventory</div>
                                    <div className="detail-value">
                                        <span className={vehicle.daysInInventory > 60 ? 'text-danger' : ''}>
                                            {vehicle.daysInInventory}
                                        </span>
                                    </div>
                                </div>
                                <div className="detail-item">
                                    <div className="detail-label">Acquisition Date</div>
                                    <div className="detail-value">
                                        {new Date(vehicle.acquisitionDate).toLocaleDateString()}
                                    </div>
                                </div>
                                <div className="detail-item">
                                    <div className="detail-label">Current Location</div>
                                    <div className="detail-value">
                                        {vehicle.location?.name || 'Not assigned'}
                                    </div>
                                </div>
                            </div>
                            
                            <div className="d-flex mt-3">
                                <Button 
                                    variant="outline-primary" 
                                    className="me-2 flex-grow-1"
                                    onClick={() => setShowStatusModal(true)}
                                >
                                    <FontAwesomeIcon icon={faExchangeAlt} className="me-1" />
                                    Change Status
                                </Button>
                                <Button 
                                    variant="outline-secondary" 
                                    className="flex-grow-1"
                                    onClick={() => setShowTransferModal(true)}
                                >
                                    <FontAwesomeIcon icon={faMapMarkerAlt} className="me-1" />
                                    Transfer
                                </Button>
                            </div>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
            
            <Card className="mb-4">
                <Card.Header>
                    <Tab.Container id="vehicle-tabs" activeKey={activeTab} onSelect={setActiveTab}>                        <Nav variant="tabs">
                            <Nav.Item>
                                <Nav.Link eventKey="details">
                                    <FontAwesomeIcon icon={faCar} className="me-2" />
                                    Details & Features
                                </Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey="pricing">
                                    <FontAwesomeIcon icon={faMoneyBill} className="me-2" />
                                    Pricing & Cost
                                </Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey="marketAnalysis">
                                    <FontAwesomeIcon icon={faChartLine} className="me-2" />
                                    Market Analysis
                                </Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey="images">
                                    <FontAwesomeIcon icon={faImage} className="me-2" />
                                    Images
                                </Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey="documents">
                                    <FontAwesomeIcon icon={faFileAlt} className="me-2" />
                                    Documents
                                </Nav.Link>
                            </Nav.Item>
                            <Nav.Item>
                                <Nav.Link eventKey="history">
                                    <FontAwesomeIcon icon={faHistory} className="me-2" />
                                    History
                                </Nav.Link>
                            </Nav.Item>
                        </Nav>
                    </Tab.Container>
                </Card.Header>
                <Card.Body>
                    <Tab.Content>
                        <Tab.Pane eventKey="details">
                            <h4 className="mb-3">Vehicle Features</h4>
                            <Row>
                                {vehicle.features?.length > 0 ? (
                                    vehicle.features.map((feature, index) => (
                                        <Col md={4} key={index} className="feature-item">
                                            <span className="feature-bullet">•</span> {feature}
                                        </Col>
                                    ))
                                ) : (
                                    <Col className="text-muted">No features listed</Col>
                                )}
                            </Row>
                            
                            <h4 className="mb-3 mt-4">Vehicle Description</h4>
                            <div className="vehicle-description">
                                {vehicle.description || 'No description available.'}
                            </div>
                        </Tab.Pane>
                        
                        <Tab.Pane eventKey="pricing">
                            <Row>
                                <Col md={6}>
                                    <Card className="pricing-card">
                                        <Card.Header>
                                            <h5 className="mb-0">Pricing Information</h5>
                                        </Card.Header>
                                        <Card.Body>
                                            <div className="detail-grid">
                                                {vehicle.pricing && (
                                                    <>
                                                        <div className="detail-item">
                                                            <div className="detail-label">MSRP</div>
                                                            <div className="detail-value">
                                                                ${vehicle.pricing.msrp?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Internet Price</div>
                                                            <div className="detail-value highlight">
                                                                ${vehicle.pricing.internetPrice?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Sticker Price</div>
                                                            <div className="detail-value">
                                                                ${vehicle.pricing.stickingPrice?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Floor Price</div>
                                                            <div className="detail-value">
                                                                ${vehicle.pricing.floorPrice?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        {vehicle.pricing.specialPrice && (
                                                            <>
                                                                <div className="detail-item">
                                                                    <div className="detail-label">Special Price</div>
                                                                    <div className="detail-value text-danger">
                                                                        ${vehicle.pricing.specialPrice?.toLocaleString()}
                                                                    </div>
                                                                </div>
                                                                <div className="detail-item">
                                                                    <div className="detail-label">Special Dates</div>
                                                                    <div className="detail-value">
                                                                        {new Date(vehicle.pricing.specialStartDate).toLocaleDateString()}
                                                                        {' to '}
                                                                        {new Date(vehicle.pricing.specialEndDate).toLocaleDateString()}
                                                                    </div>
                                                                </div>
                                                            </>
                                                        )}
                                                    </>
                                                )}
                                            </div>
                                            
                                            <Link to={`/inventory/vehicles/${vehicle.id}/pricing`} className="btn btn-outline-primary mt-3 w-100">
                                                <FontAwesomeIcon icon={faEdit} className="me-1" />
                                                Edit Pricing
                                            </Link>
                                        </Card.Body>
                                    </Card>
                                </Col>
                                
                                <Col md={6}>
                                    <Card className="cost-card">
                                        <Card.Header>
                                            <h5 className="mb-0">Cost Information</h5>
                                        </Card.Header>
                                        <Card.Body>
                                            <div className="detail-grid">
                                                {vehicle.cost && (
                                                    <>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Acquisition Cost</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.acquisitionCost?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Transport Cost</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.transportCost?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Reconditioning Cost</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.reconditioningCost?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Certification Cost</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.certificationCost?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item">
                                                            <div className="detail-label">Additional Costs</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.additionalCosts?.reduce((sum, cost) => sum + cost.amount, 0)?.toLocaleString() || '0'}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item highlight-total">
                                                            <div className="detail-label">Total Cost</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.totalCost?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                        <div className="detail-item highlight-profit">
                                                            <div className="detail-label">Target Gross Profit</div>
                                                            <div className="detail-value">
                                                                ${vehicle.cost.targetGrossProfit?.toLocaleString()}
                                                            </div>
                                                        </div>
                                                    </>
                                                )}
                                            </div>
                                            
                                            <Link to={`/inventory/vehicles/${vehicle.id}/costs`} className="btn btn-outline-primary mt-3 w-100">
                                                <FontAwesomeIcon icon={faEdit} className="me-1" />
                                                Edit Costs
                                            </Link>
                                        </Card.Body>
                                    </Card>
                                </Col>
                            </Row>
                        </Tab.Pane>
                        
                        <Tab.Pane eventKey="images">
                            <div className="d-flex justify-content-between align-items-center mb-3">
                                <h4 className="mb-0">Vehicle Images</h4>
                                <div>
                                    <Form.Group controlId="imageUpload" className="d-flex align-items-center">
                                        <Form.Label className="mb-0 me-2">Add Images:</Form.Label>
                                        <Form.Control 
                                            type="file" 
                                            multiple 
                                            accept="image/*"
                                            onChange={handleFileSelect}
                                        />
                                        <Button 
                                            variant="primary" 
                                            className="ms-2" 
                                            onClick={handleImageUpload}
                                            disabled={selectedImages.length === 0}
                                        >
                                            Upload
                                        </Button>
                                    </Form.Group>
                                </div>
                            </div>
                            
                            {uploadError && (
                                <Alert variant="danger" className="mb-3">
                                    {uploadError}
                                </Alert>
                            )}
                            
                            {uploadProgress > 0 && uploadProgress < 100 && (
                                <div className="progress mb-3">
                                    <div 
                                        className="progress-bar" 
                                        role="progressbar" 
                                        style={{ width: `${uploadProgress}%` }}
                                        aria-valuenow={uploadProgress} 
                                        aria-valuemin="0" 
                                        aria-valuemax="100"
                                    >
                                        {uploadProgress}%
                                    </div>
                                </div>
                            )}
                            
                            <Row className="image-gallery">
                                {vehicle.images?.length > 0 ? (
                                    vehicle.images.map((image, index) => (
                                        <Col md={3} key={index} className="mb-3">
                                            <div className="image-card">
                                                <div className="image-container">
                                                    <img 
                                                        src={image.url} 
                                                        alt={`${vehicle.year} ${vehicle.make} ${vehicle.model} - Image ${index + 1}`}
                                                        className="img-fluid"
                                                    />
                                                    {image.isPrimary && (
                                                        <span className="primary-badge">Primary</span>
                                                    )}
                                                </div>
                                                <div className="image-actions">
                                                    {!image.isPrimary && (
                                                        <Button 
                                                            variant="outline-secondary" 
                                                            size="sm"
                                                            onClick={() => inventoryService.setVehiclePrimaryImage(vehicle.id, image.id)
                                                                .then(() => loadVehicleData())}
                                                        >
                                                            Set as Primary
                                                        </Button>
                                                    )}
                                                    <Button 
                                                        variant="outline-danger" 
                                                        size="sm"
                                                        onClick={() => inventoryService.deleteVehicleImage(vehicle.id, image.id)
                                                            .then(() => loadVehicleData())}
                                                    >
                                                        Delete
                                                    </Button>
                                                </div>
                                            </div>
                                        </Col>
                                    ))
                                ) : (
                                    <Col className="text-center py-5 text-muted">
                                        <FontAwesomeIcon icon={faImage} size="3x" className="mb-3" />
                                        <p>No images available. Upload images using the form above.</p>
                                    </Col>
                                )}
                            </Row>
                        </Tab.Pane>
                          <Tab.Pane eventKey="marketAnalysis">
                            <div className="market-analysis-container">
                                <Row className="mb-4">
                                    <Col md={7}>
                                        <Card>
                                            <Card.Header>
                                                <h5 className="mb-0"><FontAwesomeIcon icon={faChartLine} className="me-2" />Market Price Analysis</h5>
                                            </Card.Header>
                                            <Card.Body>
                                                <div className="price-comparison">
                                                    <h6>Your Price vs. Market Average</h6>
                                                    <div className="price-meters">
                                                        <Row className="mb-3">
                                                            <Col xs={4} className="text-center text-success">
                                                                <h6>Below Market</h6>
                                                                <h3>-$2,145</h3>
                                                            </Col>
                                                            <Col xs={4} className="text-center border-start border-end">
                                                                <h6>Your Price</h6>
                                                                <h3>${vehicle.pricing?.internetPrice?.toLocaleString() || '0'}</h3>
                                                            </Col>
                                                            <Col xs={4} className="text-center">
                                                                <h6>Market Average</h6>
                                                                <h3>${(vehicle.pricing?.internetPrice + 2145).toLocaleString() || '0'}</h3>
                                                            </Col>
                                                        </Row>
                                                        <div className="progress position-relative mt-3">
                                                            <div 
                                                                className="progress-bar bg-success" 
                                                                role="progressbar" 
                                                                style={{width: '45%'}}
                                                                aria-valuenow="45" 
                                                                aria-valuemin="0" 
                                                                aria-valuemax="100"
                                                            >
                                                            </div>
                                                            <div 
                                                                className="position-absolute" 
                                                                style={{left: '45%', top: '-10px', transform: 'translateX(-50%)'}}
                                                            >
                                                                <FontAwesomeIcon icon={faCar} className="text-dark" />
                                                            </div>
                                                        </div>
                                                        <div className="d-flex justify-content-between mt-1">
                                                            <small>Underpriced</small>
                                                            <small>Market Average</small>
                                                            <small>Overpriced</small>
                                                        </div>
                                                    </div>
                                                </div>
                                                
                                                <hr />
                                                
                                                <div className="competitive-listings mt-4">
                                                    <h6>Nearby Competitive Listings (5)</h6>
                                                    <ResponsiveContainer width="100%" height={200}>
                                                        <BarChart
                                                            data={[
                                                                { name: 'ABC Motors', price: 28450, distance: 12 },
                                                                { name: 'XYZ Auto', price: 27995, distance: 18 },
                                                                { name: 'Your Price', price: vehicle.pricing?.internetPrice || 26500, distance: 0, isYours: true },
                                                                { name: 'City Cars', price: 29500, distance: 22 },
                                                                { name: 'Best Deals', price: 26995, distance: 35 }
                                                            ].sort((a, b) => a.price - b.price)}
                                                            margin={{ top: 20, right: 20, bottom: 20, left: 20 }}
                                                        >
                                                            <CartesianGrid strokeDasharray="3 3" />
                                                            <XAxis dataKey="name" />
                                                            <YAxis domain={['dataMin - 1000', 'dataMax + 1000']} tickFormatter={(value) => `$${value.toLocaleString()}`} />
                                                            <Tooltip formatter={(value) => `$${value.toLocaleString()}`} />
                                                            <Bar dataKey="price" fill="#8884d8" name="Price">
                                                                {
                                                                    data => data.map((entry, index) => (
                                                                        <Cell key={`cell-${index}`} fill={entry.isYours ? '#82ca9d' : '#8884d8'} />
                                                                    ))
                                                                }
                                                            </Bar>
                                                        </BarChart>
                                                    </ResponsiveContainer>
                                                </div>
                                            </Card.Body>
                                        </Card>
                                    </Col>
                                    <Col md={5}>
                                        <Card>
                                            <Card.Header>
                                                <h5 className="mb-0"><FontAwesomeIcon icon={faDollarSign} className="me-2" />Price Recommendations</h5>
                                            </Card.Header>
                                            <Card.Body>
                                                <div className="price-recommendation">
                                                    <div className="rec-item mb-4">
                                                        <div className="d-flex justify-content-between align-items-center">
                                                            <span>Optimal Price Range</span>
                                                            <Badge bg="info">Based on 32 similar vehicles</Badge>
                                                        </div>
                                                        <h4 className="mt-2">${(vehicle.pricing?.internetPrice - 500).toLocaleString()} - ${(vehicle.pricing?.internetPrice + 1200).toLocaleString()}</h4>
                                                    </div>
                                                    
                                                    <div className="rec-item mb-4">
                                                        <div className="d-flex justify-content-between align-items-center">
                                                            <span>Predicted Days to Sell</span>
                                                            <Badge bg="success">Fast Turn</Badge>
                                                        </div>
                                                        <h4 className="mt-2">28 days</h4>
                                                        <small className="text-muted">Based on current pricing</small>
                                                    </div>
                                                    
                                                    <div className="rec-item">
                                                        <span>Pricing Strategy</span>
                                                        <h5 className="mt-2 text-success">
                                                            <FontAwesomeIcon icon={faCheckCircle} className="me-2" />
                                                            Competitively Priced
                                                        </h5>
                                                        <small>Your price is positioned well in the market</small>
                                                    </div>
                                                </div>
                                            </Card.Body>
                                        </Card>
                                    </Col>
                                </Row>
                                
                                <Row>
                                    <Col md={12}>
                                        <Card>
                                            <Card.Header>
                                                <h5 className="mb-0"><FontAwesomeIcon icon={faChartBar} className="me-2" />Price Trends (Last 6 Months)</h5>
                                            </Card.Header>
                                            <Card.Body>
                                                <ResponsiveContainer width="100%" height={300}>
                                                    <AreaChart
                                                        data={[
                                                            { date: 'Jan', price: 29500 },
                                                            { date: 'Feb', price: 29100 },
                                                            { date: 'Mar', price: 28700 },
                                                            { date: 'Apr', price: 28200 },
                                                            { date: 'May', price: 27800 },
                                                            { date: 'Jun', price: vehicle.pricing?.internetPrice || 26500 }
                                                        ]}
                                                        margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
                                                    >
                                                        <CartesianGrid strokeDasharray="3 3" />
                                                        <XAxis dataKey="date" />
                                                        <YAxis domain={['dataMin - 1000', 'dataMax + 1000']} tickFormatter={(value) => `$${value.toLocaleString()}`} />
                                                        <Tooltip formatter={(value) => `$${value.toLocaleString()}`} />
                                                        <Legend />
                                                        <Area type="monotone" dataKey="price" stroke="#8884d8" fill="#8884d8" name="Average Market Price" />
                                                    </AreaChart>
                                                </ResponsiveContainer>
                                                <div className="mt-3 text-center">
                                                    <Alert variant="info">
                                                        <FontAwesomeIcon icon={faExclamationTriangle} className="me-2" />
                                                        Market prices for this model have declined 10.2% over the last 6 months
                                                    </Alert>
                                                </div>
                                            </Card.Body>
                                        </Card>
                                    </Col>
                                </Row>
                            </div>
                        </Tab.Pane>
                        
                        <Tab.Pane eventKey="documents">
                            {/* Documents tab content */}
                            <div className="documents-container py-3">
                                <p>Documents functionality to be implemented.</p>
                            </div>
                        </Tab.Pane>
                        
                        <Tab.Pane eventKey="history">
                            {/* History tab content */}
                            <div className="history-container py-3">
                                <p>Vehicle history timeline to be implemented.</p>
                            </div>
                        </Tab.Pane>
                    </Tab.Content>
                </Card.Body>
            </Card>
            
            {/* Transfer Modal */}
            <Modal show={showTransferModal} onHide={() => setShowTransferModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Transfer Vehicle</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form onSubmit={handleTransferSubmit}>
                        <Form.Group className="mb-3" controlId="destinationLocationId">
                            <Form.Label>Destination Location</Form.Label>
                            <Form.Select 
                                value={transferForm.destinationLocationId}
                                onChange={(e) => setTransferForm({...transferForm, destinationLocationId: e.target.value})}
                                required
                            >
                                <option value="">Select a location</option>
                                {locationOptions.map(location => (
                                    <option key={location.id} value={location.id}>
                                        {location.name}
                                    </option>
                                ))}
                            </Form.Select>
                        </Form.Group>
                        
                        <Form.Group className="mb-3" controlId="destinationZoneId">
                            <Form.Label>Zone (Optional)</Form.Label>
                            <Form.Select 
                                value={transferForm.destinationZoneId}
                                onChange={(e) => setTransferForm({...transferForm, destinationZoneId: e.target.value})}
                            >
                                <option value="">Select a zone</option>
                                {transferForm.destinationLocationId && locationOptions
                                    .find(l => l.id === transferForm.destinationLocationId)?.zones.map(zone => (
                                        <option key={zone.id} value={zone.id}>
                                            {zone.name}
                                        </option>
                                    ))}
                            </Form.Select>
                        </Form.Group>
                        
                        <Form.Group className="mb-3" controlId="transferReason">
                            <Form.Label>Transfer Reason</Form.Label>
                            <Form.Control 
                                as="textarea" 
                                rows={3}
                                value={transferForm.transferReason}
                                onChange={(e) => setTransferForm({...transferForm, transferReason: e.target.value})}
                                required
                            />
                        </Form.Group>
                        
                        <div className="d-flex justify-content-end">
                            <Button variant="secondary" className="me-2" onClick={() => setShowTransferModal(false)}>
                                Cancel
                            </Button>
                            <Button variant="primary" type="submit">
                                Transfer Vehicle
                            </Button>
                        </div>
                    </Form>
                </Modal.Body>
            </Modal>
            
            {/* Status Change Modal */}
            <Modal show={showStatusModal} onHide={() => setShowStatusModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Update Vehicle Status</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form onSubmit={handleStatusSubmit}>
                        <Form.Group className="mb-3" controlId="newStatus">
                            <Form.Label>New Status</Form.Label>
                            <Form.Select 
                                value={statusForm.newStatus}
                                onChange={(e) => setStatusForm({...statusForm, newStatus: e.target.value})}
                                required
                            >
                                <option value="">Select a status</option>
                                {statusOptions.map(status => (
                                    <option key={status} value={status}>
                                        {status}
                                    </option>
                                ))}
                            </Form.Select>
                        </Form.Group>
                        
                        <Form.Group className="mb-3" controlId="statusChangeReason">
                            <Form.Label>Reason for Status Change</Form.Label>
                            <Form.Control 
                                as="textarea" 
                                rows={3}
                                value={statusForm.statusChangeReason}
                                onChange={(e) => setStatusForm({...statusForm, statusChangeReason: e.target.value})}
                                required
                            />
                        </Form.Group>
                        
                        {statusForm.newStatus === 'Sold' && (
                            <>
                                <Form.Group className="mb-3" controlId="salePrice">
                                    <Form.Label>Sale Price</Form.Label>
                                    <Form.Control 
                                        type="number" 
                                        value={statusForm.additionalInfo.salePrice || ''}
                                        onChange={(e) => setStatusForm({
                                            ...statusForm, 
                                            additionalInfo: {
                                                ...statusForm.additionalInfo,
                                                salePrice: e.target.value
                                            }
                                        })}
                                        required
                                    />
                                </Form.Group>
                                
                                <Form.Group className="mb-3" controlId="saleDate">
                                    <Form.Label>Sale Date</Form.Label>
                                    <Form.Control 
                                        type="date" 
                                        value={statusForm.additionalInfo.saleDate || new Date().toISOString().split('T')[0]}
                                        onChange={(e) => setStatusForm({
                                            ...statusForm, 
                                            additionalInfo: {
                                                ...statusForm.additionalInfo,
                                                saleDate: e.target.value
                                            }
                                        })}
                                        required
                                    />
                                </Form.Group>
                            </>
                        )}
                        
                        <div className="d-flex justify-content-end">
                            <Button variant="secondary" className="me-2" onClick={() => setShowStatusModal(false)}>
                                Cancel
                            </Button>
                            <Button variant="primary" type="submit">
                                Update Status
                            </Button>
                        </div>
                    </Form>
                </Modal.Body>
            </Modal>
        </Container>
    );
};

export default VehicleDetailView;
