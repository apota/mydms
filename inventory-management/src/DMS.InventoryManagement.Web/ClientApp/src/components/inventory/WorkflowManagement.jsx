import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Badge, Form, Modal } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import inventoryService from '../../services/inventoryService';
import './WorkflowManagement.css';

const WorkflowManagement = () => {
  const [workflows, setWorkflows] = useState([]);
  const [vehicles, setVehicles] = useState([]);
  const [selectedWorkflow, setSelectedWorkflow] = useState(null);
  const [workflowDefinitions, setWorkflowDefinitions] = useState([]);
  const [workflowType, setWorkflowType] = useState('all');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showWorkflowModal, setShowWorkflowModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedVehicle, setSelectedVehicle] = useState(null);
  const [selectedDefinition, setSelectedDefinition] = useState(null);
  const [priority, setPriority] = useState(3);
  
  const navigate = useNavigate();
  const { vehicleId } = useParams();
  
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Load workflow definitions
        const definitions = await inventoryService.getWorkflowDefinitions();
        setWorkflowDefinitions(definitions);
        
        if (vehicleId) {
          // Load specific vehicle workflows
          const vehicleWorkflows = await inventoryService.getVehicleWorkflows(vehicleId);
          setWorkflows(vehicleWorkflows);
          
          const vehicleDetails = await inventoryService.getVehicleById(vehicleId);
          setVehicles([vehicleDetails]);
        } else {
          // Load active workflows
          const agingVehicles = await inventoryService.getAgingVehicles();
          const reconditioningVehicles = await inventoryService.getVehiclesInReconditioning();
          
          // Combine vehicles and remove duplicates
          const allVehicles = [...agingVehicles, ...reconditioningVehicles]
            .reduce((unique, item) => {
              const exists = unique.find(v => v.id === item.id);
              if (!exists) {
                unique.push(item);
              }
              return unique;
            }, []);
            
          setVehicles(allVehicles);
          
          // Just load first few workflows as an example
          if (allVehicles.length > 0) {
            const sampleWorkflows = await inventoryService.getVehicleWorkflows(allVehicles[0].id);
            setWorkflows(sampleWorkflows);
          }
        }
      } catch (err) {
        setError(`Error loading workflow data: ${err.message}`);
      } finally {
        setLoading(false);
      }
    };
    
    loadData();
  }, [vehicleId]);
  
  const handleWorkflowTypeChange = (e) => {
    setWorkflowType(e.target.value);
  };
  
  const handleSelectVehicle = async (vehicle) => {
    try {
      setLoading(true);
      setSelectedVehicle(vehicle);
      
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(vehicle.id);
      setWorkflows(vehicleWorkflows);
    } catch (err) {
      setError(`Error loading vehicle workflows: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleShowWorkflowModal = () => {
    setShowWorkflowModal(true);
  };
  
  const handleCloseWorkflowModal = () => {
    setShowWorkflowModal(false);
    setSelectedDefinition(null);
    setPriority(3);
  };
  
  const handleShowDetailModal = (workflow) => {
    setSelectedWorkflow(workflow);
    setShowDetailModal(true);
  };
  
  const handleCloseDetailModal = () => {
    setShowDetailModal(false);
    setSelectedWorkflow(null);
  };
  
  const handleCreateWorkflow = async () => {
    if (!selectedVehicle || !selectedDefinition) {
      setError('Please select a vehicle and workflow definition');
      return;
    }
    
    try {
      setLoading(true);
      
      await inventoryService.createWorkflowInstance({
        workflowDefinitionId: selectedDefinition,
        vehicleId: selectedVehicle.id,
        priority
      });
      
      // Reload workflows
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(selectedVehicle.id);
      setWorkflows(vehicleWorkflows);
      
      handleCloseWorkflowModal();
    } catch (err) {
      setError(`Error creating workflow: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleAdvanceWorkflow = async (workflowId) => {
    try {
      setLoading(true);
      
      await inventoryService.advanceWorkflow(workflowId);
      
      // Reload workflows
      const vehicleWorkflows = await inventoryService.getVehicleWorkflows(
        selectedVehicle ? selectedVehicle.id : vehicleId
      );
      setWorkflows(vehicleWorkflows);
    } catch (err) {
      setError(`Error advancing workflow: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleViewVehicle = (vehicleId) => {
    navigate(`/vehicle/${vehicleId}`);
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
  
  const getWorkflowTypeName = (type) => {
    switch (type) {
      case 'Acquisition':
        return 'Acquisition';
      case 'Reconditioning':
        return 'Reconditioning';
      case 'AgingManagement':
        return 'Aging Management';
      default:
        return type;
    }
  };
  
  const filteredWorkflows = workflowType === 'all' 
    ? workflows 
    : workflows.filter(w => w.workflowDefinition.workflowType === workflowType);
  
  const filteredVehicles = vehicles.filter(v => {
    if (!selectedVehicle) return true;
    return v.id === selectedVehicle.id;
  });
  
  return (
    <Container fluid className="workflow-management-container">
      <h1>Workflow Management</h1>
      
      {error && (
        <div className="alert alert-danger">{error}</div>
      )}
      
      <Row className="mb-4">
        <Col md={6}>
          <Card>
            <Card.Header>
              <div className="d-flex justify-content-between align-items-center">
                <h4>Vehicles</h4>
                {selectedVehicle && (
                  <Button 
                    variant="primary" 
                    size="sm"
                    onClick={handleShowWorkflowModal}
                  >
                    Create Workflow
                  </Button>
                )}
              </div>
            </Card.Header>
            <Card.Body className="p-0">
              <div className="vehicle-table-container">
                <Table striped hover>
                  <thead>
                    <tr>
                      <th>Stock #</th>
                      <th>Year</th>
                      <th>Make</th>
                      <th>Model</th>
                      <th>VIN</th>
                      <th>Status</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredVehicles.map(vehicle => (
                      <tr 
                        key={vehicle.id} 
                        className={selectedVehicle && selectedVehicle.id === vehicle.id ? 'selected-row' : ''}
                        onClick={() => handleSelectVehicle(vehicle)}
                      >
                        <td>{vehicle.stockNumber}</td>
                        <td>{vehicle.year}</td>
                        <td>{vehicle.make}</td>
                        <td>{vehicle.model}</td>
                        <td>{vehicle.vin}</td>
                        <td>
                          <Badge bg={
                            vehicle.status === 'InStock' ? 'success' : 
                            vehicle.status === 'Reconditioning' ? 'warning' :
                            vehicle.status === 'Sold' ? 'info' : 'secondary'
                          }>
                            {vehicle.status}
                          </Badge>
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
                    
                    {filteredVehicles.length === 0 && (
                      <tr>
                        <td colSpan="7" className="text-center">No vehicles found</td>
                      </tr>
                    )}
                  </tbody>
                </Table>
              </div>
            </Card.Body>
          </Card>
        </Col>
        
        <Col md={6}>
          <Card>
            <Card.Header>
              <div className="d-flex justify-content-between align-items-center">
                <h4>Workflows</h4>
                <Form.Select 
                  style={{ width: '200px' }} 
                  value={workflowType}
                  onChange={handleWorkflowTypeChange}
                >
                  <option value="all">All Types</option>
                  <option value="Acquisition">Acquisition</option>
                  <option value="Reconditioning">Reconditioning</option>
                  <option value="AgingManagement">Aging Management</option>
                </Form.Select>
              </div>
            </Card.Header>
            <Card.Body className="p-0">
              <div className="workflow-table-container">
                <Table striped hover>
                  <thead>
                    <tr>
                      <th>Workflow Type</th>
                      <th>Status</th>
                      <th>Start Date</th>
                      <th>Priority</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredWorkflows.map(workflow => (
                      <tr key={workflow.id}>
                        <td>{getWorkflowTypeName(workflow.workflowDefinition.workflowType)}</td>
                        <td>{getWorkflowStatusBadge(workflow.status)}</td>
                        <td>{new Date(workflow.startDate).toLocaleDateString()}</td>
                        <td>
                          <Badge bg={
                            workflow.priority === 1 ? 'danger' :
                            workflow.priority === 2 ? 'warning' :
                            workflow.priority === 3 ? 'primary' :
                            'info'
                          }>
                            {workflow.priority === 1 ? 'Highest' :
                             workflow.priority === 2 ? 'High' :
                             workflow.priority === 3 ? 'Normal' :
                             workflow.priority === 4 ? 'Low' : 'Lowest'}
                          </Badge>
                        </td>
                        <td>
                          <Button 
                            variant="outline-secondary" 
                            size="sm"
                            className="me-1"
                            onClick={() => handleShowDetailModal(workflow)}
                          >
                            Details
                          </Button>
                          {workflow.status === 'InProgress' && (
                            <Button 
                              variant="outline-success" 
                              size="sm"
                              onClick={() => handleAdvanceWorkflow(workflow.id)}
                            >
                              Advance
                            </Button>
                          )}
                        </td>
                      </tr>
                    ))}
                    
                    {filteredWorkflows.length === 0 && (
                      <tr>
                        <td colSpan="5" className="text-center">No workflows found</td>
                      </tr>
                    )}
                  </tbody>
                </Table>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>
      
      {/* Create Workflow Modal */}
      <Modal show={showWorkflowModal} onHide={handleCloseWorkflowModal}>
        <Modal.Header closeButton>
          <Modal.Title>Create New Workflow</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {selectedVehicle && (
            <div className="mb-3">
              <p>
                <strong>Vehicle:</strong> {selectedVehicle.year} {selectedVehicle.make} {selectedVehicle.model} (VIN: {selectedVehicle.vin})
              </p>
            </div>
          )}
          
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Workflow Type</Form.Label>
              <Form.Select 
                onChange={(e) => setSelectedDefinition(e.target.value)}
                value={selectedDefinition || ''}
              >
                <option value="">Select Workflow Type</option>
                {workflowDefinitions.map(def => (
                  <option key={def.id} value={def.id}>
                    {def.name} ({getWorkflowTypeName(def.workflowType)})
                  </option>
                ))}
              </Form.Select>
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Priority</Form.Label>
              <Form.Select 
                onChange={(e) => setPriority(parseInt(e.target.value))}
                value={priority}
              >
                <option value="1">Highest</option>
                <option value="2">High</option>
                <option value="3">Normal</option>
                <option value="4">Low</option>
                <option value="5">Lowest</option>
              </Form.Select>
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseWorkflowModal}>
            Cancel
          </Button>
          <Button variant="primary" onClick={handleCreateWorkflow}>
            Create Workflow
          </Button>
        </Modal.Footer>
      </Modal>
      
      {/* Workflow Detail Modal */}
      <Modal show={showDetailModal} onHide={handleCloseDetailModal} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>Workflow Details</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {selectedWorkflow && (
            <>
              <div className="workflow-detail-header">
                <h5>
                  {selectedWorkflow.workflowDefinition.name} 
                  <Badge bg="info" className="ms-2">
                    {getWorkflowTypeName(selectedWorkflow.workflowDefinition.workflowType)}
                  </Badge>
                </h5>
                <p className="text-muted">ID: {selectedWorkflow.id}</p>
                <div className="d-flex justify-content-between">
                  <div>
                    <strong>Status:</strong> {getWorkflowStatusBadge(selectedWorkflow.status)}
                  </div>
                  <div>
                    <strong>Priority:</strong> {
                      selectedWorkflow.priority === 1 ? 'Highest' :
                      selectedWorkflow.priority === 2 ? 'High' :
                      selectedWorkflow.priority === 3 ? 'Normal' :
                      selectedWorkflow.priority === 4 ? 'Low' : 'Lowest'
                    }
                  </div>
                </div>
                <div className="d-flex justify-content-between">
                  <div>
                    <strong>Start Date:</strong> {new Date(selectedWorkflow.startDate).toLocaleString()}
                  </div>
                  <div>
                    {selectedWorkflow.completionDate && (
                      <><strong>Completion Date:</strong> {new Date(selectedWorkflow.completionDate).toLocaleString()}</>
                    )}
                  </div>
                </div>
              </div>
              
              <hr />
              
              <h6>Workflow Steps</h6>
              <Table bordered>
                <thead>
                  <tr>
                    <th>Step</th>
                    <th>Status</th>
                    <th>Start Date</th>
                    <th>Completion Date</th>
                    <th>Assigned To</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedWorkflow.stepInstances && selectedWorkflow.stepInstances.map(step => (
                    <tr key={step.id}>
                      <td>{step.workflowStep.name}</td>
                      <td>
                        <Badge bg={
                          step.status === 'NotStarted' ? 'secondary' :
                          step.status === 'InProgress' ? 'primary' :
                          step.status === 'Completed' ? 'success' :
                          step.status === 'WaitingForApproval' ? 'warning' :
                          step.status === 'Approved' ? 'success' :
                          step.status === 'Rejected' ? 'danger' : 'info'
                        }>
                          {step.status}
                        </Badge>
                      </td>
                      <td>{step.startDate ? new Date(step.startDate).toLocaleString() : '-'}</td>
                      <td>{step.completionDate ? new Date(step.completionDate).toLocaleString() : '-'}</td>
                      <td>{step.assignedTo || '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleCloseDetailModal}>
            Close
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
};

export default WorkflowManagement;
