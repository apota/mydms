import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Table, Button, Spinner, Alert, Tabs, Tab, Form } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
  faChartBar, faChartLine, faTachometerAlt, faCalendarAlt, 
  faCarSide, faMoneyBillWave, faSearch, faFilter, faDownload
} from '@fortawesome/free-solid-svg-icons';
import { 
  BarChart, Bar, PieChart, Pie, LineChart, Line, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer 
} from 'recharts';
import inventoryService from '../../services/inventoryService';
import './InventoryAnalytics.css';

const InventoryAnalytics = () => {
  // State variables for all analytics data
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTab, setActiveTab] = useState('turnover');
  
  // Filter state
  const [dateRange, setDateRange] = useState({
    startDate: new Date(new Date().setMonth(new Date().getMonth() - 3)).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0]
  });
  const [vehicleTypes, setVehicleTypes] = useState(['New', 'Used', 'Certified']);
  const [locationIds, setLocationIds] = useState([]);
  const [locations, setLocations] = useState([]);
  
  // Analytics data
  const [turnoverData, setTurnoverData] = useState(null);
  const [agingData, setAgingData] = useState(null);
  const [valuationData, setValuationData] = useState(null);
  const [mixData, setMixData] = useState(null);
  const [competitivenessData, setCompetitivenessData] = useState(null);
  
  // COLORS for charts
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];
  
  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        
        // Load locations for filters
        const locationsData = await inventoryService.getLocations();
        setLocations(locationsData);
        
        // Set all location IDs by default
        setLocationIds(locationsData.map(loc => loc.id));
        
        // Load initial data based on active tab
        await loadTabData(activeTab);
        
        setError(null);
      } catch (err) {
        console.error('Error loading inventory analytics data:', err);
        setError('Failed to load analytics data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    loadData();
  }, []);
  
  const loadTabData = async (tabKey) => {
    try {
      setLoading(true);
      
      const filters = {
        startDate: dateRange.startDate,
        endDate: dateRange.endDate,
        vehicleTypes: vehicleTypes,
        locationIds: locationIds
      };
      
      switch(tabKey) {
        case 'turnover':
          const turnoverResult = await inventoryService.getTurnoverMetrics(filters);
          setTurnoverData(turnoverResult);
          break;
          
        case 'aging':
          const agingResult = await inventoryService.getAgingReport(filters);
          setAgingData(agingResult);
          break;
          
        case 'valuation':
          const valuationResult = await inventoryService.getValuationReport(filters);
          setValuationData(valuationResult);
          break;
          
        case 'mix':
          const mixResult = await inventoryService.getInventoryMixAnalysis(filters);
          setMixData(mixResult);
          break;
          
        case 'competitiveness':
          const competitivenessResult = await inventoryService.getPriceCompetitiveness(filters);
          setCompetitivenessData(competitivenessResult);
          break;
          
        default:
          break;
      }
    } catch (err) {
      console.error(`Error loading data for tab ${tabKey}:`, err);
      setError(`Failed to load ${tabKey} data. Please try again later.`);
    } finally {
      setLoading(false);
    }
  };
  
  const handleTabChange = (tabKey) => {
    setActiveTab(tabKey);
    loadTabData(tabKey);
  };
  
  const handleFilterApply = () => {
    loadTabData(activeTab);
  };
  
  const handleResetFilters = () => {
    setDateRange({
      startDate: new Date(new Date().setMonth(new Date().getMonth() - 3)).toISOString().split('T')[0],
      endDate: new Date().toISOString().split('T')[0]
    });
    setVehicleTypes(['New', 'Used', 'Certified']);
    setLocationIds(locations.map(loc => loc.id));
  };
  
  const handleExportData = () => {
    // In a real implementation, this would call an API to export data
    alert('Export functionality would be implemented here');
  };
  
  // Render mock data for demo purposes
  // In a real implementation, this would use the actual data from the API
  const renderTurnoverTab = () => {
    const mockTurnoverData = {
      overall: {
        dayToSell: 32,
        turnoverRate: 3.4,
        averageDaysInStock: 38,
        totalSold: 124
      },
      byType: [
        { type: 'New', averageDays: 28, soldCount: 45 },
        { type: 'Used', averageDays: 35, soldCount: 67 },
        { type: 'Certified', averageDays: 32, soldCount: 12 }
      ],
      byCategory: [
        { category: 'SUVs', averageDays: 26, soldCount: 42, turnoverRate: 4.1 },
        { category: 'Sedans', averageDays: 39, soldCount: 36, turnoverRate: 2.8 },
        { category: 'Trucks', averageDays: 22, soldCount: 31, turnoverRate: 5.1 },
        { category: 'Luxury', averageDays: 45, soldCount: 15, turnoverRate: 2.1 }
      ],
      monthlyTrend: [
        { month: 'Jan', newDays: 30, usedDays: 40, certifiedDays: 35 },
        { month: 'Feb', newDays: 28, usedDays: 38, certifiedDays: 32 },
        { month: 'Mar', newDays: 32, usedDays: 36, certifiedDays: 30 },
        { month: 'Apr', newDays: 29, usedDays: 37, certifiedDays: 33 },
        { month: 'May', newDays: 27, usedDays: 35, certifiedDays: 31 },
        { month: 'Jun', newDays: 28, usedDays: 33, certifiedDays: 32 }
      ]
    };
    
    return (
      <div className="turnover-metrics">
        <Row className="mb-4">
          <Col md={3}>
            <Card className="metric-card">
              <Card.Body className="text-center">
                <div className="metric-value">{mockTurnoverData.overall.turnoverRate}</div>
                <div className="metric-title">Inventory Turns (Annual)</div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="metric-card">
              <Card.Body className="text-center">
                <div className="metric-value">{mockTurnoverData.overall.dayToSell}</div>
                <div className="metric-title">Average Days to Sell</div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="metric-card">
              <Card.Body className="text-center">
                <div className="metric-value">{mockTurnoverData.overall.averageDaysInStock}</div>
                <div className="metric-title">Average Days in Stock</div>
              </Card.Body>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="metric-card">
              <Card.Body className="text-center">
                <div className="metric-value">{mockTurnoverData.overall.totalSold}</div>
                <div className="metric-title">Total Vehicles Sold</div>
              </Card.Body>
            </Card>
          </Col>
        </Row>
        
        <Row className="mb-4">
          <Col md={12}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">
                  <FontAwesomeIcon icon={faChartLine} className="me-2" />
                  Days to Sell by Vehicle Type
                </h5>
              </Card.Header>
              <Card.Body>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={mockTurnoverData.monthlyTrend}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Line type="monotone" dataKey="newDays" stroke="#8884d8" name="New" />
                    <Line type="monotone" dataKey="usedDays" stroke="#82ca9d" name="Used" />
                    <Line type="monotone" dataKey="certifiedDays" stroke="#ffc658" name="Certified" />
                  </LineChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
        </Row>
        
        <Row>
          <Col md={8}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Turnover Performance by Category</h5>
              </Card.Header>
              <Card.Body>
                <Table striped bordered hover responsive>
                  <thead>
                    <tr>
                      <th>Vehicle Category</th>
                      <th>Average Days to Sell</th>
                      <th>Turnover Rate</th>
                      <th>Units Sold</th>
                    </tr>
                  </thead>
                  <tbody>
                    {mockTurnoverData.byCategory.map((item, index) => (
                      <tr key={index}>
                        <td>{item.category}</td>
                        <td>{item.averageDays}</td>
                        <td>{item.turnoverRate}</td>
                        <td>{item.soldCount}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Units Sold by Type</h5>
              </Card.Header>
              <Card.Body>
                <ResponsiveContainer width="100%" height={250}>
                  <PieChart>
                    <Pie
                      data={mockTurnoverData.byType}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="soldCount"
                      nameKey="type"
                      label={({type, percent}) => `${type}: ${(percent * 100).toFixed(0)}%`}
                    >
                      {mockTurnoverData.byType.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => `${value} units`} />
                  </PieChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </div>
    );
  };
  
  const renderAgingTab = () => {
    const mockAgingData = {
      brackets: [
        { name: '0-15 days', vehicles: 45, value: 1350000 },
        { name: '16-30 days', vehicles: 38, value: 1140000 },
        { name: '31-45 days', vehicles: 22, value: 660000 },
        { name: '46-60 days', vehicles: 12, value: 360000 },
        { name: '61+ days', vehicles: 8, value: 240000 }
      ],
      criticalVehicles: [
        { id: '1', stockNumber: 'S12345', year: 2022, make: 'Ford', model: 'F-150', daysInInventory: 72, internetPrice: 45000 },
        { id: '2', stockNumber: 'S12346', year: 2021, make: 'Honda', model: 'Accord', daysInInventory: 68, internetPrice: 25000 },
        { id: '3', stockNumber: 'S12347', year: 2021, make: 'Toyota', model: 'Camry', daysInInventory: 65, internetPrice: 28000 },
        { id: '4', stockNumber: 'S12348', year: 2020, make: 'Chevrolet', model: 'Equinox', daysInInventory: 63, internetPrice: 22000 },
        { id: '5', stockNumber: 'S12349', year: 2022, make: 'BMW', model: '3 Series', daysInInventory: 62, internetPrice: 48000 }
      ]
    };
    
    return (
      <div className="aging-analysis">
        <Row className="mb-4">
          <Col md={12}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">
                  <FontAwesomeIcon icon={faCalendarAlt} className="me-2" />
                  Inventory Aging Distribution
                </h5>
              </Card.Header>
              <Card.Body>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={mockAgingData.brackets}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis yAxisId="left" orientation="left" stroke="#8884d8" />
                    <YAxis yAxisId="right" orientation="right" stroke="#82ca9d" />
                    <Tooltip formatter={(value, name) => {
                      if (name === 'vehicles') return `${value} units`;
                      return `$${value.toLocaleString()}`;
                    }} />
                    <Legend />
                    <Bar yAxisId="left" dataKey="vehicles" fill="#8884d8" name="Vehicles" />
                    <Bar yAxisId="right" dataKey="value" fill="#82ca9d" name="Value ($)" />
                  </BarChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
        </Row>
        
        <Row>
          <Col md={12}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Critical Aging Inventory (60+ Days)</h5>
              </Card.Header>
              <Card.Body>
                <Table striped bordered hover responsive>
                  <thead>
                    <tr>
                      <th>Stock #</th>
                      <th>Vehicle</th>
                      <th>Days in Inventory</th>
                      <th>Price</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {mockAgingData.criticalVehicles.map((vehicle) => (
                      <tr key={vehicle.id}>
                        <td>{vehicle.stockNumber}</td>
                        <td>{vehicle.year} {vehicle.make} {vehicle.model}</td>
                        <td className="text-danger">{vehicle.daysInInventory} days</td>
                        <td>${vehicle.internetPrice.toLocaleString()}</td>
                        <td>
                          <Button size="sm" variant="warning" className="me-1">Price Adjust</Button>
                          <Button size="sm" variant="info">View Details</Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </div>
    );
  };
  
  const renderInventoryMixTab = () => {
    const mockMixData = {
      byBodyStyle: [
        { name: 'SUV', value: 85 },
        { name: 'Sedan', value: 65 },
        { name: 'Truck', value: 45 },
        { name: 'Van', value: 20 },
        { name: 'Coupe', value: 15 }
      ],
      byMake: [
        { name: 'Toyota', value: 45 },
        { name: 'Honda', value: 40 },
        { name: 'Ford', value: 35 },
        { name: 'Chevrolet', value: 30 },
        { name: 'Nissan', value: 25 },
        { name: 'Others', value: 55 }
      ],
      trend: [
        { month: 'Jan', suv: 80, sedan: 60, truck: 40, van: 18, coupe: 12 },
        { month: 'Feb', suv: 82, sedan: 62, truck: 42, van: 19, coupe: 13 },
        { month: 'Mar', suv: 81, sedan: 63, truck: 44, van: 20, coupe: 14 },
        { month: 'Apr', suv: 83, sedan: 64, truck: 43, van: 21, coupe: 14 },
        { month: 'May', suv: 84, sedan: 65, truck: 45, van: 20, coupe: 15 },
        { month: 'Jun', suv: 85, sedan: 65, truck: 45, van: 20, coupe: 15 }
      ]
    };
    
    return (
      <div className="inventory-mix-analysis">
        <Row className="mb-4">
          <Col md={6}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Distribution by Body Style</h5>
              </Card.Header>
              <Card.Body>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={mockMixData.byBodyStyle}
                      cx="50%"
                      cy="50%"
                      labelLine={true}
                      outerRadius={100}
                      fill="#8884d8"
                      dataKey="value"
                      nameKey="name"
                      label={({name, percent}) => `${name}: ${(percent * 100).toFixed(0)}%`}
                    >
                      {mockMixData.byBodyStyle.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => `${value} vehicles`} />
                  </PieChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
          <Col md={6}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Distribution by Make</h5>
              </Card.Header>
              <Card.Body>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={mockMixData.byMake}
                      cx="50%"
                      cy="50%"
                      labelLine={true}
                      outerRadius={100}
                      fill="#8884d8"
                      dataKey="value"
                      nameKey="name"
                      label={({name, percent}) => `${name}: ${(percent * 100).toFixed(0)}%`}
                    >
                      {mockMixData.byMake.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => `${value} vehicles`} />
                  </PieChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
        </Row>
        
        <Row>
          <Col md={12}>
            <Card>
              <Card.Header>
                <h5 className="mb-0">Inventory Mix Trend (6 Months)</h5>
              </Card.Header>
              <Card.Body>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={mockMixData.trend}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip formatter={(value) => `${value} vehicles`} />
                    <Legend />
                    <Bar dataKey="suv" stackId="a" fill="#8884d8" name="SUV" />
                    <Bar dataKey="sedan" stackId="a" fill="#82ca9d" name="Sedan" />
                    <Bar dataKey="truck" stackId="a" fill="#ffc658" name="Truck" />
                    <Bar dataKey="van" stackId="a" fill="#ff8042" name="Van" />
                    <Bar dataKey="coupe" stackId="a" fill="#a4de6c" name="Coupe" />
                  </BarChart>
                </ResponsiveContainer>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </div>
    );
  };
  
  const renderFiltersCard = () => {
    return (
      <Card className="mb-4">
        <Card.Header>
          <h5 className="mb-0">
            <FontAwesomeIcon icon={faFilter} className="me-2" />
            Filters
          </h5>
        </Card.Header>
        <Card.Body>
          <Form>
            <Row>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Date Range</Form.Label>
                  <Row>
                    <Col>
                      <Form.Control
                        type="date"
                        value={dateRange.startDate}
                        onChange={(e) => setDateRange({...dateRange, startDate: e.target.value})}
                      />
                    </Col>
                    <Col>
                      <Form.Control
                        type="date"
                        value={dateRange.endDate}
                        onChange={(e) => setDateRange({...dateRange, endDate: e.target.value})}
                      />
                    </Col>
                  </Row>
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Vehicle Types</Form.Label>
                  {['New', 'Used', 'Certified'].map((type) => (
                    <Form.Check
                      key={type}
                      type="checkbox"
                      id={`check-${type}`}
                      label={type}
                      checked={vehicleTypes.includes(type)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setVehicleTypes([...vehicleTypes, type]);
                        } else {
                          setVehicleTypes(vehicleTypes.filter(t => t !== type));
                        }
                      }}
                    />
                  ))}
                </Form.Group>
              </Col>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>Locations</Form.Label>
                  <div style={{maxHeight: '100px', overflowY: 'auto'}}>
                    {locations.map((location) => (
                      <Form.Check
                        key={location.id}
                        type="checkbox"
                        id={`loc-${location.id}`}
                        label={location.name}
                        checked={locationIds.includes(location.id)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setLocationIds([...locationIds, location.id]);
                          } else {
                            setLocationIds(locationIds.filter(id => id !== location.id));
                          }
                        }}
                      />
                    ))}
                  </div>
                </Form.Group>
              </Col>
            </Row>
            <div className="d-flex justify-content-end">
              <Button variant="secondary" className="me-2" onClick={handleResetFilters}>
                Reset
              </Button>
              <Button variant="primary" onClick={handleFilterApply}>
                Apply Filters
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    );
  };
  
  if (loading && !turnoverData && !agingData && !valuationData && !mixData && !competitivenessData) {
    return (
      <div className="text-center my-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
        <p>Loading analytics data...</p>
      </div>
    );
  }
  
  return (
    <div className="inventory-analytics">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>
          <FontAwesomeIcon icon={faTachometerAlt} className="me-2" />
          Inventory Analytics
        </h1>
        <div className="d-flex">
          <Button variant="outline-primary" className="me-2" onClick={handleExportData}>
            <FontAwesomeIcon icon={faDownload} className="me-2" />
            Export Data
          </Button>
        </div>
      </div>
      
      {error && (
        <Alert variant="danger" className="mb-4">
          {error}
        </Alert>
      )}
      
      {renderFiltersCard()}
      
      <Card>
        <Card.Header>
          <Tabs
            activeKey={activeTab}
            onSelect={handleTabChange}
            className="card-header-tabs"
            id="inventory-analytics-tabs"
          >
            <Tab 
              eventKey="turnover" 
              title={
                <span>
                  <FontAwesomeIcon icon={faChartLine} className="me-2" />
                  Turnover Metrics
                </span>
              }
            />
            <Tab 
              eventKey="aging" 
              title={
                <span>
                  <FontAwesomeIcon icon={faCalendarAlt} className="me-2" />
                  Aging Analysis
                </span>
              }
            />
            <Tab 
              eventKey="valuation" 
              title={
                <span>
                  <FontAwesomeIcon icon={faMoneyBillWave} className="me-2" />
                  Inventory Valuation
                </span>
              }
            />
            <Tab 
              eventKey="mix" 
              title={
                <span>
                  <FontAwesomeIcon icon={faCarSide} className="me-2" />
                  Inventory Mix
                </span>
              }
            />
            <Tab 
              eventKey="competitiveness" 
              title={
                <span>
                  <FontAwesomeIcon icon={faChartBar} className="me-2" />
                  Price Competitiveness
                </span>
              }
            />
          </Tabs>
        </Card.Header>
        <Card.Body>
          {loading ? (
            <div className="text-center my-5">
              <Spinner animation="border" role="status">
                <span className="visually-hidden">Loading...</span>
              </Spinner>
              <p>Loading {activeTab} data...</p>
            </div>
          ) : (
            <>
              {activeTab === 'turnover' && renderTurnoverTab()}
              {activeTab === 'aging' && renderAgingTab()}
              {activeTab === 'mix' && renderInventoryMixTab()}
              {/* Other tabs would be implemented similarly */}
              {(activeTab === 'valuation' || activeTab === 'competitiveness') && (
                <Alert variant="info">
                  This analytics view is currently under development and will be available soon.
                </Alert>
              )}
            </>
          )}
        </Card.Body>
      </Card>
    </div>
  );
};

export default InventoryAnalytics;
