import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Table, Statistic, Spin, Alert, Button, Select, DatePicker } from 'antd';
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, PieChart, Pie, Cell } from 'recharts';
import { DollarOutlined, ArrowUpOutlined, ArrowDownOutlined, ShopOutlined } from '@ant-design/icons';
import moment from 'moment';
import inventoryService from '../../services/inventoryService';

const { Option } = Select;
const { RangePicker } = DatePicker;

const MarketplaceReport = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [reportData, setReportData] = useState(null);
  const [marketplaceStats, setMarketplaceStats] = useState([]);
  const [selectedDateRange, setSelectedDateRange] = useState([moment().subtract(30, 'days'), moment()]);
  const [selectedMarketplace, setSelectedMarketplace] = useState('all');
  const [availableMarketplaces, setAvailableMarketplaces] = useState([]);

  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8'];

  // Fetch marketplace reports
  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        
        // Fetch available marketplaces
        const marketplaces = await inventoryService.getAvailableMarketplaces();
        setAvailableMarketplaces(marketplaces);
        
        // Generate random marketplace stats for the demo
        // In a real implementation, this would come from the API
        const mockViewsByMarketplace = [
          { name: 'AutoTrader', value: 1250 },
          { name: 'Cars.com', value: 980 },
          { name: 'CarGurus', value: 1450 },
          { name: 'Facebook', value: 830 },
          { name: 'Dealer Website', value: 1650 }
        ];
        
        // Mock marketplace performance data
        const mockMarketplaceStats = [
          { 
            id: 1,
            name: 'AutoTrader', 
            totalListings: 45,
            activeListings: 42, 
            views: 1250,
            leads: 28,
            costPerLead: 12.50,
            conversionRate: 2.24,
            avgDaysToSell: 32
          },
          { 
            id: 2,
            name: 'Cars.com', 
            totalListings: 38,
            activeListings: 36, 
            views: 980,
            leads: 21,
            costPerLead: 14.75,
            conversionRate: 2.14,
            avgDaysToSell: 35
          },
          { 
            id: 3,
            name: 'CarGurus', 
            totalListings: 50,
            activeListings: 48, 
            views: 1450,
            leads: 32,
            costPerLead: 11.20,
            conversionRate: 2.21,
            avgDaysToSell: 28
          },
          { 
            id: 4,
            name: 'Facebook', 
            totalListings: 30,
            activeListings: 29, 
            views: 830,
            leads: 18,
            costPerLead: 9.25,
            conversionRate: 2.17,
            avgDaysToSell: 33
          },
          { 
            id: 5,
            name: 'Dealer Website', 
            totalListings: 55,
            activeListings: 55, 
            views: 1650,
            leads: 36,
            costPerLead: 8.75,
            conversionRate: 2.18,
            avgDaysToSell: 30
          }
        ];
        
        // Daily metrics for the past 30 days (mock data)
        const mockDailyData = [];
        for (let i = 0; i < 30; i++) {
          const date = moment().subtract(29 - i, 'days').format('MMM DD');
          mockDailyData.push({
            date,
            'AutoTrader': Math.floor(Math.random() * 70) + 30,
            'Cars.com': Math.floor(Math.random() * 50) + 20,
            'CarGurus': Math.floor(Math.random() * 80) + 40,
            'Facebook': Math.floor(Math.random() * 40) + 15,
            'Dealer Website': Math.floor(Math.random() * 90) + 45,
          });
        }
        
        setReportData({
          viewsByMarketplace: mockViewsByMarketplace,
          dailyViews: mockDailyData,
          totalViews: 6160,
          totalLeads: 135,
          leadConversionRate: 2.19,
          avgCostPerLead: 11.29
        });
        
        setMarketplaceStats(mockMarketplaceStats);
        
      } catch (err) {
        console.error('Error fetching marketplace report data:', err);
        setError('Failed to load marketplace report data. Please try again later.');
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, []);

  const handleDateRangeChange = (dates) => {
    setSelectedDateRange(dates);
    // In a real implementation, you would fetch data for the new date range
  };

  const handleMarketplaceChange = (value) => {
    setSelectedMarketplace(value);
    // In a real implementation, you would filter data based on selected marketplace
  };

  const columns = [
    {
      title: 'Marketplace',
      dataIndex: 'name',
      key: 'name',
      render: (text) => <strong>{text}</strong>
    },
    {
      title: 'Active Listings',
      dataIndex: 'activeListings',
      key: 'activeListings',
      render: (text, record) => `${text}/${record.totalListings}`
    },
    {
      title: 'Views',
      dataIndex: 'views',
      key: 'views',
      sorter: (a, b) => a.views - b.views
    },
    {
      title: 'Leads',
      dataIndex: 'leads',
      key: 'leads',
      sorter: (a, b) => a.leads - b.leads
    },
    {
      title: 'Cost Per Lead',
      dataIndex: 'costPerLead',
      key: 'costPerLead',
      render: (value) => `$${value.toFixed(2)}`,
      sorter: (a, b) => a.costPerLead - b.costPerLead
    },
    {
      title: 'Conversion Rate',
      dataIndex: 'conversionRate',
      key: 'conversionRate',
      render: (value) => `${value.toFixed(2)}%`,
      sorter: (a, b) => a.conversionRate - b.conversionRate
    },
    {
      title: 'Avg Days to Sell',
      dataIndex: 'avgDaysToSell',
      key: 'avgDaysToSell',
      sorter: (a, b) => a.avgDaysToSell - b.avgDaysToSell
    }
  ];

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading marketplace report...</p>
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
    <div className="marketplace-report">
      <div className="page-header">
        <h1><ShopOutlined /> Marketplace Performance Report</h1>
        <div className="filters">
          <Select 
            style={{ width: 200, marginRight: 16 }} 
            value={selectedMarketplace}
            onChange={handleMarketplaceChange}
          >
            <Option value="all">All Marketplaces</Option>
            {availableMarketplaces.map(marketplace => (
              <Option key={marketplace.id} value={marketplace.id}>{marketplace.name}</Option>
            ))}
          </Select>
          <RangePicker 
            value={selectedDateRange}
            onChange={handleDateRangeChange}
          />
        </div>
      </div>

      <Row gutter={[16, 16]} className="stat-cards">
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Total Views" 
              value={reportData.totalViews} 
              prefix={<ShopOutlined />} 
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Total Leads" 
              value={reportData.totalLeads} 
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Lead Conversion Rate" 
              value={reportData.leadConversionRate} 
              suffix="%" 
              precision={2}
              valueStyle={{ color: reportData.leadConversionRate > 2 ? '#3f8600' : '#cf1322' }}
              prefix={reportData.leadConversionRate > 2 ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic 
              title="Avg. Cost Per Lead" 
              value={reportData.avgCostPerLead} 
              prefix={<DollarOutlined />} 
              precision={2}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} className="chart-row">
        <Col xs={24} lg={12}>
          <Card title="Views by Marketplace">
            <PieChart width={500} height={300}>
              <Pie
                data={reportData.viewsByMarketplace}
                cx={250}
                cy={150}
                labelLine={false}
                outerRadius={100}
                fill="#8884d8"
                dataKey="value"
                nameKey="name"
                label={({name, percent}) => `${name}: ${(percent * 100).toFixed(0)}%`}
              >
                {reportData.viewsByMarketplace.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(value) => `${value} views`} />
              <Legend />
            </PieChart>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Daily Views by Marketplace">
            <LineChart
              width={500}
              height={300}
              data={reportData.dailyViews}
              margin={{
                top: 5, right: 30, left: 20, bottom: 5,
              }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="AutoTrader" stroke="#0088FE" activeDot={{ r: 8 }} />
              <Line type="monotone" dataKey="Cars.com" stroke="#00C49F" />
              <Line type="monotone" dataKey="CarGurus" stroke="#FFBB28" />
              <Line type="monotone" dataKey="Facebook" stroke="#FF8042" />
              <Line type="monotone" dataKey="Dealer Website" stroke="#8884D8" />
            </LineChart>
          </Card>
        </Col>
      </Row>

      <Card title="Marketplace Performance Metrics">
        <Table 
          dataSource={marketplaceStats} 
          columns={columns}
          rowKey="id"
          pagination={false}
        />
      </Card>

      <div className="action-buttons">
        <Button type="primary" icon={<ShopOutlined />}>
          Manage Marketplace Settings
        </Button>
        <Button>
          Export Report
        </Button>
      </div>
    </div>
  );
};

export default MarketplaceReport;
