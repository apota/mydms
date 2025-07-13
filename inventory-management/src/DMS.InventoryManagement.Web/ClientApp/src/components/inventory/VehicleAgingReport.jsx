import React, { useState, useEffect } from 'react';
import { 
  Table, Card, Alert, Spin, Select, Button, Row, Col, Statistic, Tag, Progress, Tooltip 
} from 'antd';
import { WarningOutlined, ClockCircleOutlined, BarChartOutlined, ExportOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { getAgingVehicles, getAgingSummary } from '../../services/inventoryService';

const { Option } = Select;

const VehicleAgingReport = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [agingVehicles, setAgingVehicles] = useState([]);
  const [summary, setSummary] = useState({
    totalVehicles: 0,
    criticalCount: 0,
    warningCount: 0,
    healthyCount: 0,
    averageDaysInStock: 0,
    oldestVehicleDays: 0
  });
  
  const [timeThreshold, setTimeThreshold] = useState(60); // Default to 60 days for critical vehicles
  
  useEffect(() => {
    fetchAgingData();
  }, [timeThreshold]);
  
  const fetchAgingData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Fetch summary data
      const summaryData = await getAgingSummary();
      setSummary(summaryData);
      
      // Fetch vehicles past threshold
      const vehiclesData = await getAgingVehicles({ threshold: timeThreshold });
      setAgingVehicles(vehiclesData);
    } catch (err) {
      console.error('Error fetching aging data:', err);
      setError('Failed to load aging report. Please try again later.');
    } finally {
      setLoading(false);
    }
  };
  
  const handleThresholdChange = (value) => {
    setTimeThreshold(parseInt(value, 10));
  };
  
  const handleExport = () => {
    // In a real implementation, this would generate a CSV/Excel export
    console.log('Exporting aging report');
    alert('Report export would be triggered here');
  };
  
  // Calculate aging severity for color coding and display
  const getAgingSeverity = (days) => {
    if (days >= 90) return { level: 'Critical', color: 'red' };
    if (days >= 60) return { level: 'Warning', color: 'orange' };
    if (days >= 30) return { level: 'Monitor', color: 'gold' };
    return { level: 'Healthy', color: 'green' };
  };
  
  const columns = [
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
    },
    {
      title: 'Make/Model',
      dataIndex: 'make',
      key: 'makeModel',
      render: (_, record) => `${record.make} ${record.model}`
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
      title: 'Location',
      dataIndex: ['location', 'name'],
      key: 'location',
    },
    {
      title: 'Days In Stock',
      dataIndex: 'daysInStock',
      key: 'daysInStock',
      sorter: (a, b) => b.daysInStock - a.daysInStock, // Sort descending by default
      defaultSortOrder: 'descend',
      render: (days) => {
        const severity = getAgingSeverity(days);
        return (
          <Tooltip title={severity.level}>
            <Tag color={severity.color}>{days} days</Tag>
          </Tooltip>
        );
      }
    },
    {
      title: 'Aging',
      dataIndex: 'daysInStock',
      key: 'aging',
      render: (days) => {
        // Calculate percentage against 90 days (critical threshold)
        const percentage = Math.min(Math.round((days / 90) * 100), 100);
        const severity = getAgingSeverity(days);
        
        return (
          <Tooltip title={`${severity.level}: ${days} days`}>
            <Progress 
              percent={percentage} 
              size="small" 
              status={severity.level === 'Critical' ? 'exception' : 'active'} 
              strokeColor={severity.color}
              showInfo={false}
            />
          </Tooltip>
        );
      }
    },
    {
      title: 'List Price',
      dataIndex: 'listPrice',
      key: 'listPrice',
      render: (price) => `$${price?.toLocaleString() || 0}`
    },
    {
      title: 'Last Price Change',
      dataIndex: 'pricing',
      key: 'lastPriceChange',
      render: (pricing) => {
        if (!pricing || !pricing.lastPriceChangeDate) return 'Never';
        const daysSinceChange = Math.round(
          (new Date() - new Date(pricing.lastPriceChangeDate)) / (1000 * 60 * 60 * 24)
        );
        return `${daysSinceChange} days ago`;
      }
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Link to={`/inventory/pricing/${record.id}`}>
          <Button type="primary" size="small">Price Review</Button>
        </Link>
      )
    }
  ];

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading aging report...</p>
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
    <div className="aging-report">
      <h1><ClockCircleOutlined /> Vehicle Aging Report</h1>
      
      <Row gutter={[16, 16]} className="aging-stats">
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Total Vehicles" 
              value={summary.totalVehicles} 
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card className="critical-card">
            <Statistic 
              title="Critical (90+ Days)" 
              value={summary.criticalCount} 
              valueStyle={{ color: '#cf1322' }}
              prefix={<WarningOutlined />} 
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card className="warning-card">
            <Statistic 
              title="Warning (60-89 Days)" 
              value={summary.warningCount} 
              valueStyle={{ color: '#faad14' }}
              prefix={<WarningOutlined />} 
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Average Days in Stock" 
              value={summary.averageDaysInStock} 
              precision={0}
              suffix="days"
            />
          </Card>
        </Col>
      </Row>
      
      <Card className="aging-filters">
        <Row justify="space-between" align="middle">
          <Col>
            <span className="filter-label">Show vehicles aged: </span>
            <Select
              value={timeThreshold.toString()}
              onChange={handleThresholdChange}
              style={{ width: 150 }}
            >
              <Option value="30">30+ days</Option>
              <Option value="60">60+ days</Option>
              <Option value="90">90+ days</Option>
              <Option value="120">120+ days</Option>
            </Select>
          </Col>
          <Col>
            <Button 
              type="primary" 
              icon={<BarChartOutlined />}
              style={{ marginRight: 8 }}
            >
              <Link to="/inventory/analytics/aging">View Aging Analytics</Link>
            </Button>
            <Button 
              icon={<ExportOutlined />}
              onClick={handleExport}
            >
              Export Report
            </Button>
          </Col>
        </Row>
      </Card>
      
      <Table 
        dataSource={agingVehicles} 
        columns={columns} 
        rowKey="id"
        pagination={{ pageSize: 15 }}
      />
      
      <div className="report-insights">
        <Card title="Aging Insights" className="insights-card">
          <p>
            <strong>Oldest vehicle:</strong> {summary.oldestVehicleDays} days in inventory
          </p>
          <p>
            <strong>Recommendation:</strong> Review aging vehicles over 90 days and consider 
            price reductions or reconditioning to increase marketability.
          </p>
        </Card>
      </div>
    </div>
  );
};

export default VehicleAgingReport;
