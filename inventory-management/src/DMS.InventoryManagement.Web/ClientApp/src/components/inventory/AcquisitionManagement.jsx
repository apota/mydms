import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Badge, Form, Modal, Alert, Tabs, Tab } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import inventoryService from '../../services/inventoryService';
import './AcquisitionManagement.css';

const AcquisitionManagement = () => {
  const [vehicles, setVehicles] = useState([]);
  const [selectedVehicle, setSelectedVehicle] = useState(null);
  const [workflow, setWorkflow] = useState(null);
  const [inspection, setInspection] = useState(null);
  const [documents, setDocuments] = useState([]);
  const [statistics, setStatistics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showInspectionModal, setShowInspectionModal] = useState(false);
  const [showDocumentsModal, setShowDocumentsModal] = useState(false);
  const [showCompleteModal, setShowCompleteModal] = useState(false);
  const [notes, setNotes] = useState('');
  
  // Inspection form state
  const [inspectionForm, setInspectionForm] = useState({
    inspector: '',
    status: 'Passed',
    notes: '',
    checklistItems: {
      exterior: false,
      interior: false,
      mechanical: false,
      electrical: false,
      tires: false,
      documents: false
    },
    issues: []
  });
  
  const [newIssue, setNewIssue] = useState({
    category: 'Mechanical',
    description: '',
    severity: 'Minor',
    estimatedRepairCost: 0
  });
  
  // Selected documents
  const [selectedDocuments, setSelectedDocuments] = useState([]);
  
  const navigate = useNavigate();
  const { vehicleId } = useParams();
  
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Load acquisition vehicles
        const acquisitionVehicles = await inventoryService.getVehiclesInAcquisition();
        setVehicles(acquisitionVehicles);
        
        // Load acquisition statistics
        const stats = await inventoryService.getAcquisitionStatistics();
        setStatistics(stats);
        
        if (vehicleId) {
          await loadVehicleDetails(vehicleId);
        } else if (acquisitionVehicles.length > 0) {
          await loadVehicleDetails(acquisitionVehicles[0].id);
        }
      } catch (err) {
        setError(`Error loading acquisition data: ${err.message}`);
      } finally {
        setLoading(false);
      }
    };
    
    loadData();
  }, [vehicleId]);
  
  const loadVehicleDetails = async (id) => {
    try {
      // Get vehicle details
      const vehicle = await inventoryService.getVehicleById(id);
      setSelectedVehicle(vehicle);
      
      // Get acquisition workflow
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(id);
      const acquisitionWorkflow = vehicleWorkflows.find(
        w => w.workflowDefinition && w.workflowDefinition.workflowType === 'Acquisition' && 
        (w.status === 'NotStarted' || w.status === 'InProgress')
      );
      
      setWorkflow(acquisitionWorkflow || null);
      
      // Get inspection if exists
      try {
        const vehicleInspection = await inventoryService.getVehicleInspection(id);
        setInspection(vehicleInspection);
      } catch (err) {
        setInspection(null);
      }
      
      // Get documents
      const vehicleDocuments = await inventoryService.getVehicleDocuments(id);
      setDocuments(vehicleDocuments);
    } catch (err) {
      setError(`Error loading vehicle details: ${err.message}`);
    }
  };
  
  const handleSelectVehicle = async (vehicle) => {
    await loadVehicleDetails(vehicle.id);
  };
  
  const handleCreateWorkflow = async () => {
    if (!selectedVehicle) return;
    
    try {
      setLoading(true);
      const newWorkflow = await inventoryService.createAcquisitionWorkflow(selectedVehicle.id);
      setWorkflow(newWorkflow);
      
      // Reload vehicle to get updated status
      const vehicle = await inventoryService.getVehicleById(selectedVehicle.id);
      setSelectedVehicle(vehicle);
    } catch (err) {
      setError(`Error creating workflow: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleShowInspectionModal = () => {
    // Pre-fill with existing data if available
    if (inspection) {
      setInspectionForm({
        inspector: inspection.inspector || '',
        status: inspection.status || 'Passed',
        notes: inspection.notes || '',
        checklistItems: inspection.checklistItems || {
          exterior: false,
          interior: false,
          mechanical: false,
          electrical: false,
          tires: false,
          documents: false
        },
        issues: inspection.issues || []
      });
    } else {
      // Reset form
      setInspectionForm({
        inspector: '',
        status: 'Passed',
        notes: '',
        checklistItems: {
          exterior: false,
          interior: false,
          mechanical: false,
          electrical: false,
          tires: false,
          documents: false
        },
        issues: []
      });
    }
    
    setShowInspectionModal(true);
  };
  
  const handleCloseInspectionModal = () => {
    setShowInspectionModal(false);
  };
  
  const handleShowDocumentsModal = () => {
    setSelectedDocuments(documents.filter(d => d.category === 'Acquisition').map(d => d.id));
    setShowDocumentsModal(true);
  };
  
  const handleCloseDocumentsModal = () => {
    setShowDocumentsModal(false);
  };
  
  const handleShowCompleteModal = () => {
    setNotes('');
    setShowCompleteModal(true);
  };
  
  const handleCloseCompleteModal = () => {
    setShowCompleteModal(false);
  };
  
  const handleAddIssue = () => {
    if (!newIssue.description) return;
    
    setInspectionForm({
      ...inspectionForm,
      issues: [...inspectionForm.issues, { ...newIssue }]
    });
    
    // Reset new issue form
    setNewIssue({
      category: 'Mechanical',
      description: '',
      severity: 'Minor',
      estimatedRepairCost: 0
    });
  };
  
  const handleRemoveIssue = (index) => {
    const issues = [...inspectionForm.issues];
    issues.splice(index, 1);
    
    setInspectionForm({
      ...inspectionForm,
      issues
    });
  };
  
  const handleSubmitInspection = async () => {
    if (!selectedVehicle) return;
    
    try {
      setLoading(true);
      
      // Determine status based on issues
      let status = inspectionForm.status;
      if (inspectionForm.issues.some(i => i.severity === 'Critical')) {
        status = 'Failed';
      } else if (inspectionForm.issues.length > 0) {
        status = 'PassedWithIssues';
      }
      
      const inspectionData = {
        vehicleId: selectedVehicle.id,
        inspector: inspectionForm.inspector,
        notes: inspectionForm.notes,
        status,
        checklistItems: inspectionForm.checklistItems,
        issues: inspectionForm.issues
      };
      
      const result = await inventoryService.recordVehicleInspection(inspectionData);
      setInspection(result);
      
      // Reload workflow to get updated status
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(selectedVehicle.id);
      const acquisitionWorkflow = vehicleWorkflows.find(
        w => w.workflowDefinition && w.workflowDefinition.workflowType === 'Acquisition' && 
        (w.status === 'NotStarted' || w.status === 'InProgress')
      );
      
      setWorkflow(acquisitionWorkflow || null);
      
      handleCloseInspectionModal();
    } catch (err) {
      setError(`Error submitting inspection: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleSubmitDocuments = async () => {
    if (!selectedVehicle || !selectedDocuments.length) return;
    
    try {
      setLoading(true);
      
      await inventoryService.updateAcquisitionDocuments(selectedVehicle.id, selectedDocuments);
      
      // Reload documents
      const vehicleDocuments = await inventoryService.getVehicleDocuments(selectedVehicle.id);
      setDocuments(vehicleDocuments);
      
      // Reload workflow to get updated status
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(selectedVehicle.id);
      const acquisitionWorkflow = vehicleWorkflows.find(
        w => w.workflowDefinition && w.workflowDefinition.workflowType === 'Acquisition' && 
        (w.status === 'NotStarted' || w.status === 'InProgress')
      );
      
      setWorkflow(acquisitionWorkflow || null);
      
      handleCloseDocumentsModal();
    } catch (err) {
      setError(`Error updating documents: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleCompleteIntake = async () => {
    if (!selectedVehicle) return;
    
    try {
      setLoading(true);
      
      const result = await inventoryService.completeVehicleIntake(selectedVehicle.id, notes);
      setSelectedVehicle(result);
      
      // Reload workflow
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(selectedVehicle.id);
      const acquisitionWorkflow = vehicleWorkflows.find(
        w => w.workflowDefinition && w.workflowDefinition.workflowType === 'Acquisition'
      );
      
      setWorkflow(acquisitionWorkflow || null);
      
      // Reload vehicles
      const acquisitionVehicles = await inventoryService.getVehiclesInAcquisition();
      setVehicles(acquisitionVehicles);
      
      handleCloseCompleteModal();
    } catch (err) {
      setError(`Error completing intake: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleViewVehicle = (vehicleId) => {
    navigate(`/vehicle/${vehicleId}`);
  };
  
  const getVehicleStatusBadge = (status) => {
    switch (status) {
      case 'InTransit':
        return <Badge bg="info">In Transit</Badge>;
      case 'Receiving':
        return <Badge bg="primary">Receiving</Badge>;
      case 'InStock':
        return <Badge bg="success">In Stock</Badge>;
      case 'Reconditioning':
        return <Badge bg="warning">Reconditioning</Badge>;
      case 'FrontLine':
        return <Badge bg="success">Front Line</Badge>;
      case 'OnHold':
        return <Badge bg="warning">On Hold</Badge>;
      case 'Sold':
        return <Badge bg="secondary">Sold</Badge>;
      default:
        return <Badge bg="secondary">{status}</Badge>;
    }
  };
  
  const getWorkflowStatusBadge = (status) => {
    switch (status) {
      case 'NotStarted':
        return <Badge bg="secondary">Not Started</Badge>;
      case 'InProgress':
        return <Badge bg="primary">In Progress</Badge>;
      case 'OnHold':
        return <Badge bg="warning">On Hold</Badge>;
      case 'Completed':
        return <Badge bg="success">Completed</Badge>;
      case 'Cancelled':
        return <Badge bg="danger">Cancelled</Badge>;
      default:
        return <Badge bg="info">{status}</Badge>;
    }
  };
  
  const getStepStatusBadge = (status) => {
    switch (status) {
      case 'NotStarted':
        return <Badge bg="secondary">Not Started</Badge>;
      case 'InProgress':
        return <Badge bg="primary">In Progress</Badge>;
      case 'Completed':
        return <Badge bg="success">Completed</Badge>;
      case 'WaitingForApproval':
        return <Badge bg="warning">Waiting for Approval</Badge>;
      case 'Approved':
        return <Badge bg="success">Approved</Badge>;
      case 'Rejected':
        return <Badge bg="danger">Rejected</Badge>;
      default:
        return <Badge bg="info">{status}</Badge>;
    }
  };
  
  return (
    <Container fluid className="acquisition-management-container">
      <h1>Acquisition Management</h1>
      
      {error && (
        <Alert variant="danger">{error}</Alert>
      )}
      
      {loading && !selectedVehicle && (
        <div className="text-center my-5">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      )}
      
      <Tabs defaultActiveKey="vehicles" className="mb-3">
        <Tab eventKey="vehicles" title="Vehicles in Acquisition">
          <Row className="mb-4">
            <Col md={5}>
              <Card>
                <Card.Header>
                  <h4>Vehicles</h4>
                </Card.Header>
                <Card.Body className="p-0">
                  <div className="vehicle-table-container">
                    <Table striped hover>
                      <thead>
                        <tr>
                          <th>Stock #</th>
                          <th>Year/Make/Model</th>
                          <th>Status</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {vehicles.map(vehicle => (
                          <tr 
                            key={vehicle.id} 
                            className={selectedVehicle && selectedVehicle.id === vehicle.id ? 'selected-row' : ''}
                            onClick={() => handleSelectVehicle(vehicle)}
                          >
                            <td>{vehicle.stockNumber}</td>
                            <td>
                              {vehicle.year} {vehicle.make} {vehicle.model}
                              <div className="small text-muted">{vehicle.vin}</div>
                            </td>
                            <td>
                              {getVehicleStatusBadge(vehicle.status)}
                            </td>
                            <td>
                              <Button 
                                variant="outline-primary" 
                                size="sm"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleViewVehicle(vehicle.id);
                                }}
                              >
                                View
                              </Button>
                            </td>
                          </tr>
                        ))}
                        
                        {vehicles.length === 0 && (
                          <tr>
                            <td colSpan="4" className="text-center">No vehicles in acquisition</td>
                          </tr>
                        )}
                      </tbody>
                    </Table>
                  </div>
                </Card.Body>
              </Card>
            </Col>
            
            <Col md={7}>
              {selectedVehicle && (
                <div>
                  <Card>
                    <Card.Header>
                      <div className="d-flex justify-content-between align-items-center">
                        <h4>
                          {selectedVehicle.year} {selectedVehicle.make} {selectedVehicle.model}
                          {' '}
                          {getVehicleStatusBadge(selectedVehicle.status)}
                        </h4>
                        {!workflow && (
                          <Button 
                            variant="primary" 
                            size="sm"
                            onClick={handleCreateWorkflow}
                            disabled={loading}
                          >
                            Start Acquisition Process
                          </Button>
                        )}
                      </div>
                    </Card.Header>
                    <Card.Body>
                      <Row>
                        <Col md={6}>
                          <p><strong>VIN:</strong> {selectedVehicle.vin}</p>
                          <p><strong>Stock Number:</strong> {selectedVehicle.stockNumber}</p>
                          <p><strong>Type:</strong> {selectedVehicle.vehicleType}</p>
                          <p><strong>Acquisition Date:</strong> {new Date(selectedVehicle.acquisitionDate).toLocaleDateString()}</p>
                          <p><strong>Source:</strong> {selectedVehicle.acquisitionSource || 'Not specified'}</p>
                        </Col>
                        <Col md={6}>
                          <p><strong>Mileage:</strong> {selectedVehicle.mileage.toLocaleString()}</p>
                          <p><strong>Exterior Color:</strong> {selectedVehicle.exteriorColor || 'Not specified'}</p>
                          <p><strong>Interior Color:</strong> {selectedVehicle.interiorColor || 'Not specified'}</p>
                          <p><strong>Acquisition Cost:</strong> ${selectedVehicle.acquisitionCost.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</p>
                          <p><strong>List Price:</strong> ${selectedVehicle.listPrice.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</p>
                        </Col>
                      </Row>
                      
                      {workflow && (
                        <>
                          <hr />
                          <h5>
                            Acquisition Workflow 
                            {' '}
                            {getWorkflowStatusBadge(workflow.status)}
                          </h5>
                          
                          <Table bordered size="sm">
                            <thead>
                              <tr>
                                <th>Step</th>
                                <th>Status</th>
                                <th>Actions</th>
                              </tr>
                            </thead>
                            <tbody>
                              {workflow.stepInstances.map((step) => (
                                <tr key={step.id}>
                                  <td>{step.workflowStep.name}</td>
                                  <td>{getStepStatusBadge(step.status)}</td>
                                  <td>
                                    {step.status !== 'Completed' && step.workflowStep.name.includes('Inspection') && (
                                      <Button 
                                        variant="outline-primary" 
                                        size="sm"
                                        onClick={handleShowInspectionModal}
                                      >
                                        {inspection ? 'Update Inspection' : 'Perform Inspection'}
                                      </Button>
                                    )}
                                    
                                    {step.status !== 'Completed' && step.workflowStep.name.includes('Document') && (
                                      <Button 
                                        variant="outline-primary" 
                                        size="sm"
                                        onClick={handleShowDocumentsModal}
                                      >
                                        Update Documents
                                      </Button>
                                    )}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </Table>
                          
                          {workflow.status === 'InProgress' && inspection && (
                            <div className="mt-3">
                              <Button 
                                variant="success"
                                onClick={handleShowCompleteModal}
                                disabled={loading}
                              >
                                Complete Intake Process
                              </Button>
                            </div>
                          )}
                        </>
                      )}
                      
                      {!workflow && selectedVehicle.status !== 'InStock' && selectedVehicle.status !== 'FrontLine' && (
                        <Alert variant="info">
                          No active acquisition workflow found. Click the "Start Acquisition Process" button to begin.
                        </Alert>
                      )}
                      
                      {(selectedVehicle.status === 'InStock' || selectedVehicle.status === 'FrontLine') && (
                        <Alert variant="success">
                          This vehicle has completed the acquisition process.
                        </Alert>
                      )}
                    </Card.Body>
                  </Card>
                  
                  {inspection && (
                    <Card className="mt-3">
                      <Card.Header>
                        <h5>Inspection Details</h5>
                      </Card.Header>
                      <Card.Body>
                        <p><strong>Inspector:</strong> {inspection.inspector}</p>
                        <p>
                          <strong>Status:</strong> {' '}
                          <Badge bg={
                            inspection.status === 'Passed' ? 'success' : 
                            inspection.status === 'PassedWithIssues' ? 'warning' : 
                            'danger'
                          }>
                            {inspection.status}
                          </Badge>
                        </p>
                        {inspection.notes && (
                          <p><strong>Notes:</strong> {inspection.notes}</p>
                        )}
                        
                        <h6>Checklist Items</h6>
                        <Row>
                          {Object.entries(inspection.checklistItems).map(([key, value]) => (
                            <Col md={4} key={key}>
                              <div>
                                <i className={value ? 'bi bi-check-circle-fill text-success' : 'bi bi-x-circle-fill text-danger'} />
                                {' '}
                                {key.charAt(0).toUpperCase() + key.slice(1)}
                              </div>
                            </Col>
                          ))}
                        </Row>
                        
                        {inspection.issues.length > 0 && (
                          <>
                            <h6 className="mt-3">Issues Found</h6>
                            <Table bordered size="sm">
                              <thead>
                                <tr>
                                  <th>Category</th>
                                  <th>Description</th>
                                  <th>Severity</th>
                                  <th>Est. Repair Cost</th>
                                </tr>
                              </thead>
                              <tbody>
                                {inspection.issues.map((issue, index) => (
                                  <tr key={index}>
                                    <td>{issue.category}</td>
                                    <td>{issue.description}</td>
                                    <td>
                                      <Badge bg={
                                        issue.severity === 'Minor' ? 'info' :
                                        issue.severity === 'Moderate' ? 'warning' :
                                        issue.severity === 'Major' ? 'danger' :
                                        'dark'
                                      }>
                                        {issue.severity}
                                      </Badge>
                                    </td>
                                    <td>
                                      {issue.estimatedRepairCost ? 
                                        `$${issue.estimatedRepairCost.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : 
                                        'N/A'}
                                    </td>
                                  </tr>
                                ))}
                              </tbody>
                            </Table>
                          </>
                        )}
                      </Card.Body>
                    </Card>
                  )}
                </div>
              )}
              
              {!selectedVehicle && !loading && (
                <Alert variant="info">
                  Select a vehicle to view acquisition details
                </Alert>
              )}
            </Col>
          </Row>
        </Tab>
        
        <Tab eventKey="statistics" title="Acquisition Statistics">
          {statistics ? (
            <Card>
              <Card.Header>
                <h4>Acquisition Statistics (Last 30 Days)</h4>
              </Card.Header>
              <Card.Body>
                <Row>
                  <Col md={4}>
                    <div className="stats-card">
                      <h5>Total Acquired</h5>
                      <div className="stats-value">{statistics.totalAcquired}</div>
                    </div>
                  </Col>
                  <Col md={4}>
                    <div className="stats-card">
                      <h5>Avg. Acquisition Cost</h5>
                      <div className="stats-value">
                        ${statistics.averageAcquisitionCost.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </div>
                    </div>
                  </Col>
                  <Col md={4}>
                    <div className="stats-card">
                      <h5>Avg. Time to Frontline</h5>
                      <div className="stats-value">
                        {statistics.averageTimeToFrontline.toFixed(1)} days
                      </div>
                    </div>
                  </Col>
                </Row>
                
                <Row className="mt-4">
                  <Col md={6}>
                    <h5>Acquisition Sources</h5>
                    <Table striped bordered>
                      <thead>
                        <tr>
                          <th>Source</th>
                          <th>Count</th>
                        </tr>
                      </thead>
                      <tbody>
                        {Object.entries(statistics.acquisitionSources).map(([source, count]) => (
                          <tr key={source}>
                            <td>{source}</td>
                            <td>{count}</td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  </Col>
                  <Col md={6}>
                    <h5>Vehicles by Status</h5>
                    <Table striped bordered>
                      <thead>
                        <tr>
                          <th>Status</th>
                          <th>Count</th>
                        </tr>
                      </thead>
                      <tbody>
                        {Object.entries(statistics.vehiclesByStatus).map(([status, count]) => (
                          <tr key={status}>
                            <td>{status}</td>
                            <td>{count}</td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  </Col>
                </Row>
                
                <Row className="mt-3">
                  <Col>
                    <Alert variant="warning">
                      <i className="bi bi-exclamation-triangle-fill me-2"></i>
                      Pending Inspections: <strong>{statistics.pendingInspections}</strong>
                    </Alert>
                  </Col>
                </Row>
              </Card.Body>
            </Card>
          ) : (
            <div className="text-center my-5">
              <div className="spinner-border" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          )}
        </Tab>
      </Tabs>
      
      {/* Inspection Modal */}
      <Modal show={showInspectionModal} onHide={handleCloseInspectionModal} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Vehicle Inspection</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Inspector Name</Form.Label>
              <Form.Control 
                type="text" 
                value={inspectionForm.inspector}
                onChange={(e) => setInspectionForm({...inspectionForm, inspector: e.target.value})}
                required
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Initial Status</Form.Label>
              <Form.Select
                value={inspectionForm.status}
                onChange={(e) => setInspectionForm({...inspectionForm, status: e.target.value})}
              >
                <option value="Passed">Passed</option>
                <option value="PassedWithIssues">Passed with Issues</option>
                <option value="Failed">Failed</option>
              </Form.Select>
              <Form.Text className="text-muted">
                Note: Status will automatically change to "Failed" if critical issues are found.
              </Form.Text>
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Notes</Form.Label>
              <Form.Control 
                as="textarea" 
                rows={3}
                value={inspectionForm.notes}
                onChange={(e) => setInspectionForm({...inspectionForm, notes: e.target.value})}
              />
            </Form.Group>
            
            <h5>Checklist Items</h5>
            <Row>
              {Object.keys(inspectionForm.checklistItems).map((key) => (
                <Col md={4} key={key}>
                  <Form.Check 
                    type="checkbox"
                    id={`check-${key}`}
                    label={key.charAt(0).toUpperCase() + key.slice(1)}
                    checked={inspectionForm.checklistItems[key]}
                    onChange={(e) => setInspectionForm({
                      ...inspectionForm,
                      checklistItems: {
                        ...inspectionForm.checklistItems,
                        [key]: e.target.checked
                      }
                    })}
                  />
                </Col>
              ))}
            </Row>
            
            <h5 className="mt-4">Issues Found</h5>
            {inspectionForm.issues.length > 0 ? (
              <Table bordered size="sm">
                <thead>
                  <tr>
                    <th>Category</th>
                    <th>Description</th>
                    <th>Severity</th>
                    <th>Est. Repair Cost</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {inspectionForm.issues.map((issue, index) => (
                    <tr key={index}>
                      <td>{issue.category}</td>
                      <td>{issue.description}</td>
                      <td>{issue.severity}</td>
                      <td>
                        {issue.estimatedRepairCost ? 
                          `$${issue.estimatedRepairCost.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : 
                          'N/A'}
                      </td>
                      <td>
                        <Button 
                          variant="outline-danger" 
                          size="sm"
                          onClick={() => handleRemoveIssue(index)}
                        >
                          <i className="bi bi-trash"></i>
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            ) : (
              <p className="text-muted">No issues added yet</p>
            )}
            
            <h6>Add Issue</h6>
            <Row className="g-2 mb-3">
              <Col md={3}>
                <Form.Select
                  value={newIssue.category}
                  onChange={(e) => setNewIssue({...newIssue, category: e.target.value})}
                >
                  <option value="Mechanical">Mechanical</option>
                  <option value="Electrical">Electrical</option>
                  <option value="Exterior">Exterior</option>
                  <option value="Interior">Interior</option>
                  <option value="Documentation">Documentation</option>
                  <option value="Other">Other</option>
                </Form.Select>
              </Col>
              <Col md={3}>
                <Form.Select
                  value={newIssue.severity}
                  onChange={(e) => setNewIssue({...newIssue, severity: e.target.value})}
                >
                  <option value="Minor">Minor</option>
                  <option value="Moderate">Moderate</option>
                  <option value="Major">Major</option>
                  <option value="Critical">Critical</option>
                </Form.Select>
              </Col>
              <Col md={3}>
                <Form.Control
                  type="number"
                  placeholder="Est. Cost"
                  value={newIssue.estimatedRepairCost}
                  onChange={(e) => setNewIssue({...newIssue, estimatedRepairCost: parseFloat(e.target.value) || 0})}
                />
              </Col>
            </Row>
            <Row className="g-2 mb-3">
              <Col md={9}>
                <Form.Control
                  placeholder="Description"
                  value={newIssue.description}
                  onChange={(e) => setNewIssue({...newIssue, description: e.target.value})}
                />
              </Col>
              <Col md={3}>
                <Button onClick={handleAddIssue} variant="outline-primary" className="w-100">
                  Add Issue
                </Button>
              </Col>
            </Row>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseInspectionModal}>
            Cancel
          </Button>
          <Button 
            variant="primary" 
            onClick={handleSubmitInspection}
            disabled={!inspectionForm.inspector}
          >
            Submit Inspection
          </Button>
        </Modal.Footer>
      </Modal>
      
      {/* Documents Modal */}
      <Modal show={showDocumentsModal} onHide={handleCloseDocumentsModal}>
        <Modal.Header closeButton>
          <Modal.Title>Acquisition Documents</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {documents.length === 0 ? (
            <Alert variant="info">
              No documents uploaded yet. Please upload documents for this vehicle first.
            </Alert>
          ) : (
            <>
              <p>Select the documents related to acquisition:</p>
              <Table bordered>
                <thead>
                  <tr>
                    <th>Select</th>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Upload Date</th>
                  </tr>
                </thead>
                <tbody>
                  {documents.map(doc => (
                    <tr key={doc.id}>
                      <td>
                        <Form.Check 
                          type="checkbox"
                          checked={selectedDocuments.includes(doc.id)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setSelectedDocuments([...selectedDocuments, doc.id]);
                            } else {
                              setSelectedDocuments(selectedDocuments.filter(id => id !== doc.id));
                            }
                          }}
                        />
                      </td>
                      <td>{doc.fileName}</td>
                      <td>{doc.fileType}</td>
                      <td>{new Date(doc.uploadDate).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseDocumentsModal}>
            Cancel
          </Button>
          <Button 
            variant="primary" 
            onClick={handleSubmitDocuments}
            disabled={selectedDocuments.length === 0}
          >
            Update Documents
          </Button>
        </Modal.Footer>
      </Modal>
      
      {/* Complete Modal */}
      <Modal show={showCompleteModal} onHide={handleCloseCompleteModal}>
        <Modal.Header closeButton>
          <Modal.Title>Complete Intake Process</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p>
            Are you sure you want to complete the intake process for this vehicle?
            This will change the vehicle status based on inspection results.
          </p>
          
          <Form.Group className="mb-3">
            <Form.Label>Notes (Optional)</Form.Label>
            <Form.Control 
              as="textarea" 
              rows={3}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </Form.Group>
          
          {inspection && inspection.status === 'Failed' && (
            <Alert variant="warning">
              <i className="bi bi-exclamation-triangle-fill me-2"></i>
              This vehicle failed inspection. It will be sent to reconditioning upon completion.
            </Alert>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseCompleteModal}>
            Cancel
          </Button>
          <Button 
            variant="success" 
            onClick={handleCompleteIntake}
          >
            Complete Intake
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default AcquisitionManagement;
