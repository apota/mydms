import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Form, Button, Tabs, Tab, Alert, Spinner, Table } from 'react-bootstrap';
import { FaUpload, FaFileCsv, FaCar, FaGavel } from 'react-icons/fa';
import inventoryService from '../../services/inventoryService';
import './VehicleImport.css';

const VehicleImport = () => {
  // Common state
  const [activeTab, setActiveTab] = useState('csv');
  const [importResult, setImportResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  // CSV Import state
  const [csvFile, setCsvFile] = useState(null);
  const [mappingTemplates, setMappingTemplates] = useState([]);
  const [selectedTemplate, setSelectedTemplate] = useState('');
  
  // Manufacturer Import state
  const [manufacturers, setManufacturers] = useState([]);
  const [selectedManufacturer, setSelectedManufacturer] = useState('');
  const [manufacturerOptions, setManufacturerOptions] = useState({
    newInventoryOnly: true,
    updateExisting: true,
    dealerCodes: [],
    startDate: null,
    endDate: null
  });
  const [dealerCode, setDealerCode] = useState('');
  
  // Auction Import state
  const [auctions, setAuctions] = useState([]);
  const [selectedAuction, setSelectedAuction] = useState('');
  const [auctionOptions, setAuctionOptions] = useState({
    wonAuctionsOnly: true,
    activeAuctionsOnly: false,
    watchedAuctionsOnly: false,
    startDate: null,
    endDate: null,
    makes: []
  });
  const [auctionMake, setAuctionMake] = useState('');
  
  useEffect(() => {
    // Load templates, manufacturers, and auctions
    const loadData = async () => {
      try {
        const templates = await inventoryService.getMappingTemplates();
        setMappingTemplates(templates);
        
        if (templates.length > 0) {
          setSelectedTemplate(templates[0].name);
        }
        
        const manufacturerCodes = await inventoryService.getManufacturerCodes();
        setManufacturers(manufacturerCodes);
        
        if (manufacturerCodes.length > 0) {
          setSelectedManufacturer(manufacturerCodes[0]);
        }
        
        const auctionCodes = await inventoryService.getAuctionCodes();
        setAuctions(auctionCodes);
        
        if (auctionCodes.length > 0) {
          setSelectedAuction(auctionCodes[0]);
        }
      } catch (err) {
        console.error('Error loading import data:', err);
        setError('Failed to load import data. Please try again later.');
      }
    };
    
    loadData();
  }, []);
  
  const handleFileChange = (e) => {
    const file = e.target.files[0];
    setCsvFile(file);
  };
  
  const handleCsvImport = async (e) => {
    e.preventDefault();
    
    if (!csvFile) {
      setError('Please select a CSV file to import');
      return;
    }
    
    if (!selectedTemplate) {
      setError('Please select a mapping template');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const result = await inventoryService.importVehiclesFromCsv(csvFile, selectedTemplate);
      setImportResult(result);
    } catch (err) {
      console.error('Error importing CSV:', err);
      setError(err.message || 'Failed to import CSV file');
    } finally {
      setLoading(false);
    }
  };
  
  const handleManufacturerImport = async (e) => {
    e.preventDefault();
    
    if (!selectedManufacturer) {
      setError('Please select a manufacturer');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const result = await inventoryService.importVehiclesFromManufacturer(
        selectedManufacturer, 
        manufacturerOptions
      );
      setImportResult(result);
    } catch (err) {
      console.error('Error importing from manufacturer:', err);
      setError(err.message || 'Failed to import from manufacturer');
    } finally {
      setLoading(false);
    }
  };
  
  const handleAuctionImport = async (e) => {
    e.preventDefault();
    
    if (!selectedAuction) {
      setError('Please select an auction platform');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    try {
      const result = await inventoryService.importVehiclesFromAuction(
        selectedAuction, 
        auctionOptions
      );
      setImportResult(result);
    } catch (err) {
      console.error('Error importing from auction:', err);
      setError(err.message || 'Failed to import from auction');
    } finally {
      setLoading(false);
    }
  };
  
  const addDealerCode = () => {
    if (dealerCode) {
      setManufacturerOptions({
        ...manufacturerOptions,
        dealerCodes: [...manufacturerOptions.dealerCodes, dealerCode]
      });
      setDealerCode('');
    }
  };
  
  const removeDealerCode = (code) => {
    setManufacturerOptions({
      ...manufacturerOptions,
      dealerCodes: manufacturerOptions.dealerCodes.filter(c => c !== code)
    });
  };
  
  const addAuctionMake = () => {
    if (auctionMake) {
      setAuctionOptions({
        ...auctionOptions,
        makes: [...auctionOptions.makes, auctionMake]
      });
      setAuctionMake('');
    }
  };
  
  const removeAuctionMake = (make) => {
    setAuctionOptions({
      ...auctionOptions,
      makes: auctionOptions.makes.filter(m => m !== make)
    });
  };
  
  const renderImportResult = () => {
    if (!importResult) return null;
    
    return (
      <Card className="mt-4">
        <Card.Header>
          <h4>Import Result</h4>
        </Card.Header>
        <Card.Body>
          <Alert variant={importResult.success ? 'success' : 'warning'}>
            {importResult.success 
              ? `Successfully imported ${importResult.successCount} of ${importResult.totalRecords} vehicles` 
              : `Import completed with issues: ${importResult.successCount} of ${importResult.totalRecords} vehicles imported`}
          </Alert>
          
          <div className="mt-3">
            <h5>Summary</h5>
            <ul>
              <li>Total Records: {importResult.totalRecords}</li>
              <li>Successfully Imported: {importResult.successCount}</li>
              <li>Errors: {importResult.errorCount}</li>
            </ul>
          </div>
          
          {importResult.warnings && importResult.warnings.length > 0 && (
            <div className="mt-3">
              <h5>Warnings ({importResult.warnings.length})</h5>
              <Table striped bordered hover>
                <thead>
                  <tr>
                    <th>Row</th>
                    <th>Field</th>
                    <th>Message</th>
                  </tr>
                </thead>
                <tbody>
                  {importResult.warnings.map((warning, index) => (
                    <tr key={index}>
                      <td>{warning.rowNumber}</td>
                      <td>{warning.fieldName}</td>
                      <td>{warning.message}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          )}
          
          {importResult.errors && importResult.errors.length > 0 && (
            <div className="mt-3">
              <h5>Errors ({importResult.errors.length})</h5>
              <ul className="error-list">
                {importResult.errors.map((error, index) => (
                  <li key={index}>{error}</li>
                ))}
              </ul>
            </div>
          )}
          
          {importResult.importedVehicleIds && importResult.importedVehicleIds.length > 0 && (
            <div className="mt-3 text-right">
              <Button
                variant="primary"
                href={`/inventory/vehicles?ids=${importResult.importedVehicleIds.join(',')}`}
              >
                View Imported Vehicles
              </Button>
            </div>
          )}
        </Card.Body>
      </Card>
    );
  };
  
  return (
    <Container className="vehicle-import-container">
      <h2>Vehicle Import</h2>
      
      {error && (
        <Alert variant="danger" onClose={() => setError(null)} dismissible>
          {error}
        </Alert>
      )}
      
      <Tabs
        activeKey={activeTab}
        onSelect={(key) => setActiveTab(key)}
        className="mb-4"
      >
        <Tab eventKey="csv" title={<><FaFileCsv /> CSV Import</>}>
          <Card>
            <Card.Body>
              <Form onSubmit={handleCsvImport}>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>CSV File</Form.Label>
                      <Form.Control
                        type="file"
                        accept=".csv"
                        onChange={handleFileChange}
                        required
                      />
                      <Form.Text className="text-muted">
                        Select a CSV file containing vehicle data
                      </Form.Text>
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Mapping Template</Form.Label>
                      <Form.Select
                        value={selectedTemplate}
                        onChange={(e) => setSelectedTemplate(e.target.value)}
                        required
                      >
                        <option value="">Select Template</option>
                        {mappingTemplates.map((template, index) => (
                          <option key={index} value={template.name}>
                            {template.name} - {template.description}
                          </option>
                        ))}
                      </Form.Select>
                      <Form.Text className="text-muted">
                        Select a template that matches your CSV format
                      </Form.Text>
                    </Form.Group>
                  </Col>
                </Row>
                <div className="d-flex justify-content-end">
                  <Button
                    type="submit"
                    variant="primary"
                    disabled={loading || !csvFile || !selectedTemplate}
                  >
                    {loading ? (
                      <>
                        <Spinner as="span" animation="border" size="sm" className="me-2" />
                        Importing...
                      </>
                    ) : (
                      <>
                        <FaUpload className="me-2" /> Import Vehicles
                      </>
                    )}
                  </Button>
                </div>
              </Form>
            </Card.Body>
          </Card>
        </Tab>
        
        <Tab eventKey="manufacturer" title={<><FaCar /> Manufacturer Import</>}>
          <Card>
            <Card.Body>
              <Form onSubmit={handleManufacturerImport}>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Manufacturer</Form.Label>
                      <Form.Select
                        value={selectedManufacturer}
                        onChange={(e) => setSelectedManufacturer(e.target.value)}
                        required
                      >
                        <option value="">Select Manufacturer</option>
                        {manufacturers.map((manufacturer, index) => (
                          <option key={index} value={manufacturer}>
                            {manufacturer}
                          </option>
                        ))}
                      </Form.Select>
                    </Form.Group>
                  </Col>
                </Row>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Check
                        type="checkbox"
                        label="New Inventory Only"
                        checked={manufacturerOptions.newInventoryOnly}
                        onChange={(e) => setManufacturerOptions({
                          ...manufacturerOptions,
                          newInventoryOnly: e.target.checked
                        })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Check
                        type="checkbox"
                        label="Update Existing Vehicles"
                        checked={manufacturerOptions.updateExisting}
                        onChange={(e) => setManufacturerOptions({
                          ...manufacturerOptions,
                          updateExisting: e.target.checked
                        })}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Date Range (Arrival Date)</Form.Label>
                      <Row>
                        <Col>
                          <Form.Control
                            type="date"
                            value={manufacturerOptions.startDate || ''}
                            onChange={(e) => setManufacturerOptions({
                              ...manufacturerOptions,
                              startDate: e.target.value || null
                            })}
                            placeholder="From"
                          />
                        </Col>
                        <Col>
                          <Form.Control
                            type="date"
                            value={manufacturerOptions.endDate || ''}
                            onChange={(e) => setManufacturerOptions({
                              ...manufacturerOptions,
                              endDate: e.target.value || null
                            })}
                            placeholder="To"
                          />
                        </Col>
                      </Row>
                    </Form.Group>
                  </Col>
                </Row>
                <Row>
                  <Col>
                    <Form.Group className="mb-3">
                      <Form.Label>Dealer Codes</Form.Label>
                      <div className="d-flex mb-2">
                        <Form.Control
                          type="text"
                          placeholder="Enter dealer code"
                          value={dealerCode}
                          onChange={(e) => setDealerCode(e.target.value)}
                        />
                        <Button 
                          variant="outline-secondary" 
                          className="ms-2"
                          onClick={addDealerCode}
                        >
                          Add
                        </Button>
                      </div>
                      {manufacturerOptions.dealerCodes.length > 0 && (
                        <div className="dealer-codes-list">
                          {manufacturerOptions.dealerCodes.map((code, index) => (
                            <span key={index} className="dealer-code-tag">
                              {code}
                              <button 
                                type="button" 
                                className="btn-close btn-close-sm"
                                onClick={() => removeDealerCode(code)}
                              ></button>
                            </span>
                          ))}
                        </div>
                      )}
                    </Form.Group>
                  </Col>
                </Row>
                <div className="d-flex justify-content-end">
                  <Button
                    type="submit"
                    variant="primary"
                    disabled={loading || !selectedManufacturer}
                  >
                    {loading ? (
                      <>
                        <Spinner as="span" animation="border" size="sm" className="me-2" />
                        Importing...
                      </>
                    ) : (
                      <>
                        <FaUpload className="me-2" /> Import Vehicles
                      </>
                    )}
                  </Button>
                </div>
              </Form>
            </Card.Body>
          </Card>
        </Tab>
        
        <Tab eventKey="auction" title={<><FaGavel /> Auction Import</>}>
          <Card>
            <Card.Body>
              <Form onSubmit={handleAuctionImport}>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Auction Platform</Form.Label>
                      <Form.Select
                        value={selectedAuction}
                        onChange={(e) => setSelectedAuction(e.target.value)}
                        required
                      >
                        <option value="">Select Auction Platform</option>
                        {auctions.map((auction, index) => (
                          <option key={index} value={auction}>
                            {auction}
                          </option>
                        ))}
                      </Form.Select>
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Date Range (Auction Date)</Form.Label>
                      <Row>
                        <Col>
                          <Form.Control
                            type="date"
                            value={auctionOptions.startDate || ''}
                            onChange={(e) => setAuctionOptions({
                              ...auctionOptions,
                              startDate: e.target.value || null
                            })}
                            placeholder="From"
                          />
                        </Col>
                        <Col>
                          <Form.Control
                            type="date"
                            value={auctionOptions.endDate || ''}
                            onChange={(e) => setAuctionOptions({
                              ...auctionOptions,
                              endDate: e.target.value || null
                            })}
                            placeholder="To"
                          />
                        </Col>
                      </Row>
                    </Form.Group>
                  </Col>
                </Row>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Check
                        type="checkbox"
                        label="Won Auctions Only"
                        checked={auctionOptions.wonAuctionsOnly}
                        onChange={(e) => setAuctionOptions({
                          ...auctionOptions,
                          wonAuctionsOnly: e.target.checked
                        })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Check
                        type="checkbox"
                        label="Active Auctions Only"
                        checked={auctionOptions.activeAuctionsOnly}
                        onChange={(e) => setAuctionOptions({
                          ...auctionOptions,
                          activeAuctionsOnly: e.target.checked
                        })}
                      />
                    </Form.Group>
                    <Form.Group className="mb-3">
                      <Form.Check
                        type="checkbox"
                        label="Watched Auctions Only"
                        checked={auctionOptions.watchedAuctionsOnly}
                        onChange={(e) => setAuctionOptions({
                          ...auctionOptions,
                          watchedAuctionsOnly: e.target.checked
                        })}
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Make Filter</Form.Label>
                      <div className="d-flex mb-2">
                        <Form.Control
                          type="text"
                          placeholder="Enter make"
                          value={auctionMake}
                          onChange={(e) => setAuctionMake(e.target.value)}
                        />
                        <Button 
                          variant="outline-secondary" 
                          className="ms-2"
                          onClick={addAuctionMake}
                        >
                          Add
                        </Button>
                      </div>
                      {auctionOptions.makes.length > 0 && (
                        <div className="makes-list">
                          {auctionOptions.makes.map((make, index) => (
                            <span key={index} className="make-tag">
                              {make}
                              <button 
                                type="button" 
                                className="btn-close btn-close-sm"
                                onClick={() => removeAuctionMake(make)}
                              ></button>
                            </span>
                          ))}
                        </div>
                      )}
                    </Form.Group>
                  </Col>
                </Row>
                <div className="d-flex justify-content-end">
                  <Button
                    type="submit"
                    variant="primary"
                    disabled={loading || !selectedAuction}
                  >
                    {loading ? (
                      <>
                        <Spinner as="span" animation="border" size="sm" className="me-2" />
                        Importing...
                      </>
                    ) : (
                      <>
                        <FaUpload className="me-2" /> Import Vehicles
                      </>
                    )}
                  </Button>
                </div>
              </Form>
            </Card.Body>
          </Card>
        </Tab>
      </Tabs>
      
      {renderImportResult()}
    </Container>
  );
};

export default VehicleImport;
