import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Statistic, Table, Tag, Button, Spin, Alert, Select } from 'antd';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, PieChart, Pie, Cell } from 'recharts';
import { Link } from 'react-router-dom';
import { 
  CarOutlined, 
  WarningOutlined, 
  DollarOutlined, 
  ClockCircleOutlined,
  CalendarOutlined
} from '@ant-design/icons';
import './InventoryDashboard.css';
import { getInventorySummary, getRecentArrivals, getAgingSummary } from '../../services/inventoryService';

const { Option } = Select;

const InventoryDashboard = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [inventorySummary, setInventorySummary] = useState(null);
  const [recentArrivals, setRecentArrivals] = useState([]);
  const [agingSummary, setAgingSummary] = useState(null);
  const [timeRange, setTimeRange] = useState('30');

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const summaryData = await getInventorySummary();
        const arrivalsData = await getRecentArrivals(10);
        const agingData = await getAgingSummary();
        
        setInventorySummary(summaryData);
        setRecentArrivals(arrivalsData);
        setAgingSummary(agingData);
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError('Failed to load dashboard data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, []);

  const handleTimeRangeChange = (value) => {
    setTimeRange(value);
    // In a real implementation, you would fetch data for the new time range
  };

  // Mock data for charts
  const inventoryTrendData = [
    { month: 'Jan', newVehicles: 40, usedVehicles: 60 },
    { month: 'Feb', newVehicles: 45, usedVehicles: 70 },
    { month: 'Mar', newVehicles: 50, usedVehicles: 65 },
    { month: 'Apr', newVehicles: 55, usedVehicles: 75 },
    { month: 'May', newVehicles: 60, usedVehicles: 80 },
    { month: 'Jun', newVehicles: 65, usedVehicles: 85 }
  ];

  const inventoryMixData = [
    { name: 'SUVs', value: 35 },
    { name: 'Sedans', value: 30 },
    { name: 'Trucks', value: 20 },
    { name: 'Vans', value: 10 },
    { name: 'Luxury', value: 5 }
  ];

  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

  const columns = [
    {
      title: 'Stock #',
      dataIndex: 'stockNumber',
      key: 'stockNumber',
      render: (text, record) => <Link to={`/inventory/vehicles/${record.id}`}>{text}</Link>
    },
    {
      title: 'Year',
      dataIndex: 'year',
      key: 'year',
    },
    {
      title: 'Make',
      dataIndex: 'make',
      key: 'make',
    },
    {
      title: 'Model',
      dataIndex: 'model',
      key: 'model',
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
      }
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
      }
    },
    {
      title: 'Price',
      dataIndex: 'listPrice',
      key: 'listPrice',
      render: (price) => `$${price.toLocaleString()}`
    }
  ];

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <Alert
        message="Error"
        description={error}
        type="error"
        showIcon
      />
    );
  }

  return (
    <div className="inventory-dashboard">
      <h1>Inventory Dashboard</h1>
      
      <div className="time-range-selector">
        <span>Time Range: </span>
        <Select value={timeRange} onChange={handleTimeRangeChange} style={{ width: 120 }}>
          <Option value="7">7 days</Option>
          <Option value="30">30 days</Option>
          <Option value="60">60 days</Option>
          <Option value="90">90 days</Option>
        </Select>
      </div>
      
      <Row gutter={[16, 16]} className="stat-cards">
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Total Vehicles" 
              value={inventorySummary?.totalVehicles || 0} 
              prefix={<CarOutlined />} 
            />
            <div className="stat-footer">
              <span className={inventorySummary?.changePercent >= 0 ? 'positive' : 'negative'}>
                {inventorySummary?.changePercent >= 0 ? '+' : ''}{inventorySummary?.changePercent || 0}% from last month
              </span>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Aging Inventory" 
              value={agingSummary?.warningCount + agingSummary?.criticalCount || 0} 
              prefix={<WarningOutlined />} 
            />
            <div className="stat-footer">
              <span>{agingSummary?.criticalCount || 0} critical, {agingSummary?.warningCount || 0} warning</span>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Total Investment" 
              value={inventorySummary?.totalInvestment || 0} 
              prefix={<DollarOutlined />} 
              precision={0}
              formatter={(value) => `$${value.toLocaleString()}`}
            />
            <div className="stat-footer">
              <span>Average: ${inventorySummary?.averageInvestment?.toLocaleString() || 0}</span>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Average Days in Stock" 
              value={inventorySummary?.avgDaysInStock || 0} 
              prefix={<ClockCircleOutlined />}
              suffix="days"
            />
            <div className="stat-footer">
              <span>Target: 45 days</span>
            </div>
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} className="chart-row">
        <Col xs={24} lg={12}>
          <Card title="Inventory Trend">
            <LineChart
              width={500}
              height={300}
              data={inventoryTrendData}
              margin={{
                top: 5, right: 30, left: 20, bottom: 5,
              }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="newVehicles" stroke="#8884d8" activeDot={{ r: 8 }} name="New" />
              <Line type="monotone" dataKey="usedVehicles" stroke="#82ca9d" name="Used" />
            </LineChart>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Inventory Mix">
            <PieChart width={500} height={300}>
              <Pie
                data={inventoryMixData}
                cx={250}
                cy={150}
                labelLine={false}
                outerRadius={100}
                fill="#8884d8"
                dataKey="value"
                nameKey="name"
                label={({name, percent}) => `${name}: ${(percent * 100).toFixed(0)}%`}
              >
                {inventoryMixData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(value) => `${value} vehicles`} />
              <Legend />
            </PieChart>
          </Card>
        </Col>
      </Row>

      <Card title="Recent Arrivals" className="recent-arrivals">
        <Table 
          dataSource={recentArrivals} 
          columns={columns} 
          rowKey="id" 
          pagination={false}
        />
        <div className="view-all-button">
          <Button type="primary">
            <Link to="/inventory/vehicles">View All Inventory</Link>
          </Button>
        </div>
      </Card>

      <Row gutter={[16, 16]} className="action-row">
        <Col xs={24} sm={12} md={8}>
          <Card className="action-card">
            <h3><WarningOutlined /> Aging Inventory Alert</h3>
            <p>You have {agingSummary?.criticalCount || 0} vehicles that require immediate attention.</p>
            <Button type="primary" danger>
              <Link to="/inventory/aging">View Aging Report</Link>
            </Button>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8}>
          <Card className="action-card">
            <h3><CalendarOutlined /> Coming This Week</h3>
            <p>{inventorySummary?.incomingVehicles || 0} vehicles scheduled for delivery this week.</p>
            <Button type="primary">
              <Link to="/inventory/incoming">View Incoming</Link>
            </Button>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={8}>
          <Card className="action-card">
            <h3><DollarOutlined /> Price Analysis</h3>
            <p>{inventorySummary?.pricingAlerts || 0} vehicles with pricing recommendations.</p>
            <Button type="primary">
              <Link to="/inventory/pricing">View Pricing Report</Link>
            </Button>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default InventoryDashboard;
