import React, { useState, useEffect } from 'react';
import { 
  Row, Col, Card, Form, Input, Select, Button, DatePicker, 
  Table, Tag, Pagination, Collapse, Slider, Checkbox, Spin, Alert
} from 'antd';
import { SearchOutlined, FilterOutlined, CarOutlined, ReloadOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { getVehicles } from '../../services/inventoryService';

const { Option } = Select;
const { Panel } = Collapse;
const { RangePicker } = DatePicker;

const VehicleSearch = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [vehicleData, setVehicleData] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [filterCollapsed, setFilterCollapsed] = useState(false);
  
  // Form instance for the search and filter form
  const [form] = Form.useForm();

  // Initial fetch of vehicles
  useEffect(() => {
    fetchVehicles();
  }, [currentPage, pageSize]);

  const fetchVehicles = async (filters = {}) => {
    try {
      setLoading(true);
      setError(null);
      
      // Combine pagination with any filters
      const params = {
        page: currentPage,
        pageSize,
        ...filters
      };
      
      const response = await getVehicles(params);
      setVehicleData(response.vehicles);
      setTotalCount(response.totalCount);
    } catch (err) {
      console.error('Error fetching vehicles:', err);
      setError('Failed to load vehicles. Please try again later.');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (values) => {
    // Reset to first page when searching with new filters
    setCurrentPage(1);
    
    // Clean up the filter values (remove empty/undefined values)
    const filters = Object.entries(values).reduce((acc, [key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        acc[key] = value;
      }
      return acc;
    }, {});
    
    fetchVehicles(filters);
  };

  const handleReset = () => {
    form.resetFields();
    setCurrentPage(1);
    fetchVehicles();
  };

  const handlePageChange = (page, size) => {
    setCurrentPage(page);
    setPageSize(size);
  };

  const vehicleTypes = [
    { value: 'New', label: 'New' },
    { value: 'Used', label: 'Used' },
    { value: 'CertifiedPreOwned', label: 'Certified Pre-Owned' }
  ];

  const vehicleColumns = [
    {
      title: 'Stock #',
      dataIndex: 'stockNumber',
      key: 'stockNumber',
      render: (text, record) => <Link to={`/inventory/vehicles/${record.id}`}>{text}</Link>
    },
    {
      title: 'VIN',
      dataIndex: 'vin',
      key: 'vin',
    },
    {
      title: 'Year',
      dataIndex: 'year',
      key: 'year',
      sorter: (a, b) => a.year - b.year
    },
    {
      title: 'Make',
      dataIndex: 'make',
      key: 'make',
      sorter: (a, b) => a.make.localeCompare(b.make)
    },
    {
      title: 'Model',
      dataIndex: 'model',
      key: 'model',
      sorter: (a, b) => a.model.localeCompare(b.model)
    },
    {
      title: 'Type',
      dataIndex: 'vehicleType',
      key: 'vehicleType',
      render: (type) => {
        let color = 'blue';
        if (type === 'Used') color = 'green';
        if (type === 'CertifiedPreOwned') color = 'purple';
        return <Tag color={color}>{type}</Tag>;
      },
      filters: vehicleTypes.map(type => ({ text: type.label, value: type.value })),
      onFilter: (value, record) => record.vehicleType === value
    },
    {
      title: 'Location',
      dataIndex: ['location', 'name'],
      key: 'location',
    },
    {
      title: 'Days In Stock',
      dataIndex: 'daysInStock',
      key: 'daysInStock',
      render: (days) => {
        let color = 'green';
        if (days > 30) color = 'orange';
        if (days > 60) color = 'red';
        return <Tag color={color}>{days} days</Tag>;
      },
      sorter: (a, b) => a.daysInStock - b.daysInStock
    },
    {
      title: 'List Price',
      dataIndex: 'listPrice',
      key: 'listPrice',
      render: (price) => `$${price?.toLocaleString() || 0}`,
      sorter: (a, b) => a.listPrice - b.listPrice
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'blue';
        if (status === 'Available') color = 'green';
        if (status === 'Sold') color = 'red';
        if (status === 'InTransit') color = 'orange';
        if (status === 'OnHold') color = 'gold';
        return <Tag color={color}>{status}</Tag>;
      },
      filters: [
        { text: 'Available', value: 'Available' },
        { text: 'Sold', value: 'Sold' },
        { text: 'In Transit', value: 'InTransit' },
        { text: 'On Hold', value: 'OnHold' }
      ],
      onFilter: (value, record) => record.status === value
    }
  ];

  return (
    <div className="vehicle-search">
      <h1><CarOutlined /> Vehicle Inventory</h1>
      
      <Card className="search-filter-card">
        <Collapse 
          bordered={false}
          defaultActiveKey={['1']}
          onChange={() => setFilterCollapsed(!filterCollapsed)}
        >
          <Panel 
            header={
              <div className="filter-header">
                <FilterOutlined /> Advanced Search & Filters
              </div>
            } 
            key="1"
          >
            <Form
              form={form}
              layout="vertical"
              onFinish={handleSearch}
            >
              <Row gutter={16}>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="keyword" label="Keyword Search">
                    <Input 
                      placeholder="Search by stock#, VIN, make, model..."
                      prefix={<SearchOutlined />}
                    />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="vehicleType" label="Vehicle Type">
                    <Select placeholder="Select vehicle type" allowClear>
                      {vehicleTypes.map(type => (
                        <Option key={type.value} value={type.value}>{type.label}</Option>
                      ))}
                    </Select>
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="make" label="Make">
                    <Input placeholder="Vehicle make" />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="model" label="Model">
                    <Input placeholder="Vehicle model" />
                  </Form.Item>
                </Col>
              </Row>
              
              <Row gutter={16}>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="yearRange" label="Year Range">
                    <Slider 
                      range 
                      min={1990} 
                      max={new Date().getFullYear() + 1} 
                      defaultValue={[2010, new Date().getFullYear()]} 
                    />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="priceRange" label="Price Range ($)">
                    <Slider 
                      range 
                      min={0} 
                      max={100000} 
                      step={1000} 
                      defaultValue={[0, 50000]}
                      tipFormatter={value => `$${value.toLocaleString()}`} 
                    />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="locations" label="Locations">
                    <Select 
                      mode="multiple" 
                      placeholder="Select locations"
                      allowClear
                    >
                      <Option value="main">Main Dealership</Option>
                      <Option value="northwest">Northwest Lot</Option>
                      <Option value="downtown">Downtown Showroom</Option>
                    </Select>
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="status" label="Vehicle Status">
                    <Select placeholder="Select status" allowClear>
                      <Option value="Available">Available</Option>
                      <Option value="Sold">Sold</Option>
                      <Option value="InTransit">In Transit</Option>
                      <Option value="OnHold">On Hold</Option>
                    </Select>
                  </Form.Item>
                </Col>
              </Row>
              
              <Row gutter={16}>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="features" label="Features">
                    <Select 
                      mode="multiple" 
                      placeholder="Select features"
                      allowClear
                    >
                      <Option value="leather">Leather Seats</Option>
                      <Option value="sunroof">Sunroof</Option>
                      <Option value="navigation">Navigation</Option>
                      <Option value="bluetooth">Bluetooth</Option>
                      <Option value="backup">Backup Camera</Option>
                    </Select>
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="daysInInventory" label="Days in Inventory">
                    <Select placeholder="Select range" allowClear>
                      <Option value="0-15">0-15 days</Option>
                      <Option value="16-30">16-30 days</Option>
                      <Option value="31-60">31-60 days</Option>
                      <Option value="60+">Over 60 days</Option>
                    </Select>
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6}>
                  <Form.Item name="arrivalDate" label="Arrival Date">
                    <RangePicker />
                  </Form.Item>
                </Col>
                <Col xs={24} sm={12} md={6} className="search-actions">
                  <Button type="primary" htmlType="submit" icon={<SearchOutlined />}>
                    Search
                  </Button>
                  <Button onClick={handleReset} icon={<ReloadOutlined />} style={{ marginLeft: 8 }}>
                    Reset
                  </Button>
                </Col>
              </Row>
            </Form>
          </Panel>
        </Collapse>
      </Card>
      
      {error && (
        <Alert
          message="Error"
          description={error}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}
      
      <Card>
        <div className="results-header">
          <div className="results-count">
            {!loading && (
              <span>Found <strong>{totalCount}</strong> vehicles</span>
            )}
          </div>
          <div className="results-actions">
            <Button type="primary">
              <Link to="/inventory/vehicles/add">Add New Vehicle</Link>
            </Button>
          </div>
        </div>
        
        <Table
          dataSource={vehicleData}
          columns={vehicleColumns}
          rowKey="id"
          loading={loading}
          pagination={false}
          scroll={{ x: 'max-content' }}
        />
        
        <div className="pagination-container">
          <Pagination
            current={currentPage}
            pageSize={pageSize}
            total={totalCount}
            onChange={handlePageChange}
            showSizeChanger
            showTotal={(total, range) => `${range[0]}-${range[1]} of ${total} items`}
          />
        </div>
      </Card>
    </div>
  );
};

export default VehicleSearch;
