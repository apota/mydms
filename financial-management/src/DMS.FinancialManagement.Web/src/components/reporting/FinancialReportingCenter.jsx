import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Select,
  DatePicker,
  Spin,
  Alert,
  Typography,
  Row,
  Col,
  Table,
  Tabs,
  Space,
  Divider,
  Form,
  Radio,
} from 'antd';
import {
  DownloadOutlined,
  PrinterOutlined,
  BarChartOutlined,
  PieChartOutlined,
  LineChartOutlined,
  TableOutlined,
  SettingOutlined,
} from '@ant-design/icons';
import moment from 'moment';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
  LineChart, Line, AreaChart, Area, PieChart, Pie, Cell
} from 'recharts';
import { financialService } from '../../services/financialService';

const { TabPane } = Tabs;
const { Title } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

const FinancialReportingCenter = () => {
  const [reportType, setReportType] = useState('profit-loss');
  const [dateRange, setDateRange] = useState([moment().subtract(12, 'months'), moment()]);
  const [reportData, setReportData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [viewMode, setViewMode] = useState('table');
  const [comparisonMode, setComparisonMode] = useState('none');
  const [comparisonData, setComparisonData] = useState(null);
  const [reportingPeriod, setReportingPeriod] = useState('monthly');
  const [customColumns, setCustomColumns] = useState([]);
  
  // Fetch report data
  useEffect(() => {
    fetchReportData();
  }, [reportType, dateRange, reportingPeriod]);
  
  const fetchReportData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        fromDate: dateRange[0].format('YYYY-MM-DD'),
        toDate: dateRange[1].format('YYYY-MM-DD'),
        periodType: reportingPeriod,
      };
      
      const response = await financialService.getFinancialReports(reportType, params);
      setReportData(response);
      
      if (comparisonMode !== 'none') {
        fetchComparisonData();
      }
    } catch (err) {
      setError('Failed to load report data. Please try again.');
      console.error('Error fetching report data:', err);
    } finally {
      setLoading(false);
    }
  };
  
  const fetchComparisonData = async () => {
    try {
      let comparisonDateRange;
      
      if (comparisonMode === 'previous-period') {
        // Calculate previous period date range of same length
        const duration = dateRange[1].diff(dateRange[0], 'days');
        comparisonDateRange = [
          moment(dateRange[0]).subtract(duration + 1, 'days'),
          moment(dateRange[0]).subtract(1, 'days')
        ];
      } else if (comparisonMode === 'previous-year') {
        comparisonDateRange = [
          moment(dateRange[0]).subtract(1, 'year'),
          moment(dateRange[1]).subtract(1, 'year')
        ];
      }
      
      const params = {
        fromDate: comparisonDateRange[0].format('YYYY-MM-DD'),
        toDate: comparisonDateRange[1].format('YYYY-MM-DD'),
        periodType: reportingPeriod,
      };
      
      const response = await financialService.getFinancialReports(reportType, params);
      setComparisonData(response);
    } catch (err) {
      console.error('Error fetching comparison data:', err);
      setComparisonData(null);
    }
  };
  
  // Handle report type change
  const handleReportTypeChange = (value) => {
    setReportType(value);
    // Reset view mode to table for new report type
    setViewMode('table');
  };
  
  // Handle comparison mode change
  const handleComparisonModeChange = (e) => {
    const mode = e.target.value;
    setComparisonMode(mode);
    
    if (mode === 'none') {
      setComparisonData(null);
    } else {
      fetchComparisonData();
    }
  };
  
  // Handle export to CSV
  const handleExportCSV = () => {
    // Implementation would depend on backend API
    console.log('Export report to CSV');
  };
  
  // Handle print
  const handlePrint = () => {
    window.print();
  };
  
  // Render profit and loss report table
  const renderProfitLossTable = () => {
    if (!reportData || !reportData.periods) {
      return <Alert message="No data available" type="info" />;
    }
    
    const periodColumns = reportData.periods.map(period => ({
      title: period.name,
      dataIndex: period.key,
      key: period.key,
      render: (value) => value ? `$${value.toFixed(2)}` : '$0.00',
    }));
    
    const columns = [
      {
        title: 'Category',
        dataIndex: 'category',
        key: 'category',
        fixed: 'left',
      },
      ...periodColumns,
      {
        title: 'Total',
        dataIndex: 'total',
        key: 'total',
        render: (value) => value ? `$${value.toFixed(2)}` : '$0.00',
        fixed: 'right',
      },
    ];
    
    // Format data for the table
    const tableData = [];
    
    // Revenue section
    tableData.push({
      key: 'revenue-header',
      category: <strong>Revenue</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = null;
        return acc;
      }, {}),
      total: null,
    });
    
    reportData.revenue.categories.forEach(category => {
      tableData.push({
        key: `revenue-${category.key}`,
        category: `  ${category.name}`,
        ...reportData.periods.reduce((acc, period) => {
          const periodData = category.periods.find(p => p.key === period.key);
          acc[period.key] = periodData ? periodData.amount : 0;
          return acc;
        }, {}),
        total: category.total,
      });
    });
    
    tableData.push({
      key: 'revenue-total',
      category: <strong>  Total Revenue</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = reportData.revenue.periods.find(p => p.key === period.key)?.amount || 0;
        return acc;
      }, {}),
      total: reportData.revenue.total,
    });
    
    // Expenses section
    tableData.push({
      key: 'expenses-header',
      category: <strong>Expenses</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = null;
        return acc;
      }, {}),
      total: null,
    });
    
    reportData.expenses.categories.forEach(category => {
      tableData.push({
        key: `expenses-${category.key}`,
        category: `  ${category.name}`,
        ...reportData.periods.reduce((acc, period) => {
          const periodData = category.periods.find(p => p.key === period.key);
          acc[period.key] = periodData ? periodData.amount : 0;
          return acc;
        }, {}),
        total: category.total,
      });
    });
    
    tableData.push({
      key: 'expenses-total',
      category: <strong>  Total Expenses</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = reportData.expenses.periods.find(p => p.key === period.key)?.amount || 0;
        return acc;
      }, {}),
      total: reportData.expenses.total,
    });
    
    // Net Profit/Loss
    tableData.push({
      key: 'net-profit',
      category: <strong>Net Profit/Loss</strong>,
      ...reportData.periods.reduce((acc, period) => {
        const revenuePeriod = reportData.revenue.periods.find(p => p.key === period.key);
        const expensesPeriod = reportData.expenses.periods.find(p => p.key === period.key);
        acc[period.key] = (revenuePeriod?.amount || 0) - (expensesPeriod?.amount || 0);
        return acc;
      }, {}),
      total: reportData.revenue.total - reportData.expenses.total,
    });
    
    return (
      <Table
        columns={columns}
        dataSource={tableData}
        pagination={false}
        scroll={{ x: 'max-content' }}
        size="small"
      />
    );
  };
  
  // Render balance sheet table
  const renderBalanceSheetTable = () => {
    if (!reportData || !reportData.asOf) {
      return <Alert message="No data available" type="info" />;
    }
    
    const columns = [
      {
        title: 'Category',
        dataIndex: 'category',
        key: 'category',
      },
      {
        title: 'Amount',
        dataIndex: 'amount',
        key: 'amount',
        render: (value) => value ? `$${value.toFixed(2)}` : '$0.00',
      },
    ];
    
    const tableData = [];
    
    // Assets section
    tableData.push({
      key: 'assets-header',
      category: <strong>Assets</strong>,
      amount: null,
    });
    
    reportData.assets.categories.forEach(category => {
      tableData.push({
        key: `assets-${category.key}`,
        category: `  ${category.name}`,
        amount: category.amount,
      });
    });
    
    tableData.push({
      key: 'assets-total',
      category: <strong>  Total Assets</strong>,
      amount: reportData.assets.total,
    });
    
    // Liabilities section
    tableData.push({
      key: 'liabilities-header',
      category: <strong>Liabilities</strong>,
      amount: null,
    });
    
    reportData.liabilities.categories.forEach(category => {
      tableData.push({
        key: `liabilities-${category.key}`,
        category: `  ${category.name}`,
        amount: category.amount,
      });
    });
    
    tableData.push({
      key: 'liabilities-total',
      category: <strong>  Total Liabilities</strong>,
      amount: reportData.liabilities.total,
    });
    
    // Equity section
    tableData.push({
      key: 'equity-header',
      category: <strong>Equity</strong>,
      amount: null,
    });
    
    reportData.equity.categories.forEach(category => {
      tableData.push({
        key: `equity-${category.key}`,
        category: `  ${category.name}`,
        amount: category.amount,
      });
    });
    
    tableData.push({
      key: 'equity-total',
      category: <strong>  Total Equity</strong>,
      amount: reportData.equity.total,
    });
    
    // Liabilities + Equity Total
    tableData.push({
      key: 'liabilities-equity-total',
      category: <strong>Total Liabilities & Equity</strong>,
      amount: reportData.liabilities.total + reportData.equity.total,
    });
    
    return (
      <Table
        columns={columns}
        dataSource={tableData}
        pagination={false}
        size="small"
      />
    );
  };
  
  // Render cash flow table
  const renderCashFlowTable = () => {
    if (!reportData || !reportData.periods) {
      return <Alert message="No data available" type="info" />;
    }
    
    const periodColumns = reportData.periods.map(period => ({
      title: period.name,
      dataIndex: period.key,
      key: period.key,
      render: (value) => value ? `$${value.toFixed(2)}` : '$0.00',
    }));
    
    const columns = [
      {
        title: 'Category',
        dataIndex: 'category',
        key: 'category',
        fixed: 'left',
      },
      ...periodColumns,
      {
        title: 'Total',
        dataIndex: 'total',
        key: 'total',
        render: (value) => value ? `$${value.toFixed(2)}` : '$0.00',
        fixed: 'right',
      },
    ];
    
    const tableData = [];
    
    // Beginning Cash Balance
    tableData.push({
      key: 'beginning-balance',
      category: <strong>Beginning Cash Balance</strong>,
      ...reportData.beginningBalance.periods.reduce((acc, period) => {
        acc[period.key] = period.amount;
        return acc;
      }, {}),
      total: reportData.beginningBalance.total,
    });
    
    // Cash Inflows section
    tableData.push({
      key: 'inflows-header',
      category: <strong>Cash Inflows</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = null;
        return acc;
      }, {}),
      total: null,
    });
    
    reportData.inflows.categories.forEach(category => {
      tableData.push({
        key: `inflows-${category.key}`,
        category: `  ${category.name}`,
        ...reportData.periods.reduce((acc, period) => {
          const periodData = category.periods.find(p => p.key === period.key);
          acc[period.key] = periodData ? periodData.amount : 0;
          return acc;
        }, {}),
        total: category.total,
      });
    });
    
    tableData.push({
      key: 'inflows-total',
      category: <strong>  Total Cash Inflows</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = reportData.inflows.periods.find(p => p.key === period.key)?.amount || 0;
        return acc;
      }, {}),
      total: reportData.inflows.total,
    });
    
    // Cash Outflows section
    tableData.push({
      key: 'outflows-header',
      category: <strong>Cash Outflows</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = null;
        return acc;
      }, {}),
      total: null,
    });
    
    reportData.outflows.categories.forEach(category => {
      tableData.push({
        key: `outflows-${category.key}`,
        category: `  ${category.name}`,
        ...reportData.periods.reduce((acc, period) => {
          const periodData = category.periods.find(p => p.key === period.key);
          acc[period.key] = periodData ? periodData.amount : 0;
          return acc;
        }, {}),
        total: category.total,
      });
    });
    
    tableData.push({
      key: 'outflows-total',
      category: <strong>  Total Cash Outflows</strong>,
      ...reportData.periods.reduce((acc, period) => {
        acc[period.key] = reportData.outflows.periods.find(p => p.key === period.key)?.amount || 0;
        return acc;
      }, {}),
      total: reportData.outflows.total,
    });
    
    // Net Cash Flow
    tableData.push({
      key: 'net-cash-flow',
      category: <strong>Net Cash Flow</strong>,
      ...reportData.periods.reduce((acc, period) => {
        const inflowsPeriod = reportData.inflows.periods.find(p => p.key === period.key);
        const outflowsPeriod = reportData.outflows.periods.find(p => p.key === period.key);
        acc[period.key] = (inflowsPeriod?.amount || 0) - (outflowsPeriod?.amount || 0);
        return acc;
      }, {}),
      total: reportData.inflows.total - reportData.outflows.total,
    });
    
    // Ending Cash Balance
    tableData.push({
      key: 'ending-balance',
      category: <strong>Ending Cash Balance</strong>,
      ...reportData.periods.reduce((acc, period) => {
        const beginningBalance = reportData.beginningBalance.periods.find(p => p.key === period.key)?.amount || 0;
        const inflowsPeriod = reportData.inflows.periods.find(p => p.key === period.key);
        const outflowsPeriod = reportData.outflows.periods.find(p => p.key === period.key);
        const netCashFlow = (inflowsPeriod?.amount || 0) - (outflowsPeriod?.amount || 0);
        acc[period.key] = beginningBalance + netCashFlow;
        return acc;
      }, {}),
      total: reportData.beginningBalance.total + reportData.inflows.total - reportData.outflows.total,
    });
    
    return (
      <Table
        columns={columns}
        dataSource={tableData}
        pagination={false}
        scroll={{ x: 'max-content' }}
        size="small"
      />
    );
  };
  
  // Render charts for the profit/loss data
  const renderProfitLossChart = () => {
    if (!reportData || !reportData.periods) {
      return <Alert message="No data available for chart" type="info" />;
    }
    
    // Format data for the chart
    const chartData = reportData.periods.map(period => {
      const revenuePeriod = reportData.revenue.periods.find(p => p.key === period.key);
      const expensesPeriod = reportData.expenses.periods.find(p => p.key === period.key);
      const revenue = revenuePeriod?.amount || 0;
      const expenses = expensesPeriod?.amount || 0;
      const profit = revenue - expenses;
      
      return {
        period: period.name,
        revenue,
        expenses,
        profit,
      };
    });
    
    return (
      <div style={{ width: '100%', height: 400 }}>
        <ResponsiveContainer>
          <BarChart
            data={chartData}
            margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
          >
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="period" />
            <YAxis />
            <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
            <Legend />
            <Bar dataKey="revenue" name="Revenue" fill="#00C49F" />
            <Bar dataKey="expenses" name="Expenses" fill="#FF8042" />
            <Bar dataKey="profit" name="Profit/Loss" fill="#0088FE" />
          </BarChart>
        </ResponsiveContainer>
      </div>
    );
  };
  
  // Render chart for balance sheet
  const renderBalanceSheetChart = () => {
    if (!reportData || !reportData.asOf) {
      return <Alert message="No data available for chart" type="info" />;
    }
    
    const assetData = reportData.assets.categories.map(category => ({
      name: category.name,
      value: category.amount,
      type: 'Assets'
    }));
    
    const liabilityData = reportData.liabilities.categories.map(category => ({
      name: category.name,
      value: category.amount,
      type: 'Liabilities'
    }));
    
    const equityData = reportData.equity.categories.map(category => ({
      name: category.name,
      value: category.amount,
      type: 'Equity'
    }));
    
    return (
      <div style={{ width: '100%', height: 500 }}>
        <Row gutter={16}>
          <Col span={12}>
            <Card title="Asset Distribution" bordered={false}>
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <Pie
                    data={assetData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                    nameKey="name"
                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                  >
                    {assetData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                </PieChart>
              </ResponsiveContainer>
            </Card>
          </Col>
          <Col span={12}>
            <Card title="Liabilities & Equity" bordered={false}>
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <Pie
                    data={[...liabilityData, ...equityData]}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                    nameKey="name"
                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                  >
                    {[...liabilityData, ...equityData].map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                </PieChart>
              </ResponsiveContainer>
            </Card>
          </Col>
        </Row>
        <Divider />
        <Row>
          <Col span={24}>
            <Card title="Total Assets vs Liabilities & Equity" bordered={false}>
              <ResponsiveContainer width="100%" height={200}>
                <BarChart
                  data={[
                    { name: 'Assets', value: reportData.assets.total },
                    { name: 'Liabilities', value: reportData.liabilities.total },
                    { name: 'Equity', value: reportData.equity.total },
                    { name: 'Liabilities + Equity', value: reportData.liabilities.total + reportData.equity.total },
                  ]}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                  <Bar dataKey="value" fill="#8884d8">
                    <Cell fill="#00C49F" />
                    <Cell fill="#FF8042" />
                    <Cell fill="#0088FE" />
                    <Cell fill="#FFBB28" />
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </Card>
          </Col>
        </Row>
      </div>
    );
  };
  
  // Render chart for cash flow
  const renderCashFlowChart = () => {
    if (!reportData || !reportData.periods) {
      return <Alert message="No data available for chart" type="info" />;
    }
    
    // Format data for the chart
    const chartData = reportData.periods.map(period => {
      const inflowsPeriod = reportData.inflows.periods.find(p => p.key === period.key);
      const outflowsPeriod = reportData.outflows.periods.find(p => p.key === period.key);
      const beginningBalancePeriod = reportData.beginningBalance.periods.find(p => p.key === period.key);
      
      const inflows = inflowsPeriod?.amount || 0;
      const outflows = outflowsPeriod?.amount || 0;
      const netFlow = inflows - outflows;
      const beginningBalance = beginningBalancePeriod?.amount || 0;
      const endingBalance = beginningBalance + netFlow;
      
      return {
        period: period.name,
        inflows,
        outflows,
        netFlow,
        beginningBalance,
        endingBalance,
      };
    });
    
    return (
      <div style={{ width: '100%', height: 500 }}>
        <Row>
          <Col span={24}>
            <Card title="Cash Flow" bordered={false}>
              <ResponsiveContainer width="100%" height={200}>
                <BarChart
                  data={chartData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="period" />
                  <YAxis />
                  <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                  <Legend />
                  <Bar dataKey="inflows" name="Cash Inflows" fill="#00C49F" />
                  <Bar dataKey="outflows" name="Cash Outflows" fill="#FF8042" />
                  <Bar dataKey="netFlow" name="Net Cash Flow" fill="#0088FE" />
                </BarChart>
              </ResponsiveContainer>
            </Card>
          </Col>
        </Row>
        <Divider />
        <Row>
          <Col span={24}>
            <Card title="Cash Balance" bordered={false}>
              <ResponsiveContainer width="100%" height={200}>
                <LineChart
                  data={chartData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="period" />
                  <YAxis />
                  <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                  <Legend />
                  <Line 
                    type="monotone" 
                    dataKey="beginningBalance" 
                    name="Beginning Balance"
                    stroke="#8884d8" 
                    activeDot={{ r: 8 }} 
                  />
                  <Line 
                    type="monotone" 
                    dataKey="endingBalance" 
                    name="Ending Balance"
                    stroke="#82ca9d" 
                    activeDot={{ r: 8 }} 
                  />
                </LineChart>
              </ResponsiveContainer>
            </Card>
          </Col>
        </Row>
      </div>
    );
  };
  
  // Render the appropriate report table based on report type
  const renderReportTable = () => {
    switch (reportType) {
      case 'profit-loss':
        return renderProfitLossTable();
      case 'balance-sheet':
        return renderBalanceSheetTable();
      case 'cash-flow':
        return renderCashFlowTable();
      default:
        return <Alert message="Select a report type" type="info" />;
    }
  };
  
  // Render the appropriate report chart based on report type
  const renderReportChart = () => {
    switch (reportType) {
      case 'profit-loss':
        return renderProfitLossChart();
      case 'balance-sheet':
        return renderBalanceSheetChart();
      case 'cash-flow':
        return renderCashFlowChart();
      default:
        return <Alert message="Select a report type" type="info" />;
    }
  };

  return (
    <div className="financial-reporting-center">
      <Title level={2}>Financial Reporting Center</Title>
      
      <Card style={{ marginBottom: 20 }}>
        <Form layout="vertical">
          <Row gutter={16}>
            <Col md={8} sm={24}>
              <Form.Item label="Report Type">
                <Select 
                  value={reportType} 
                  onChange={handleReportTypeChange}
                  style={{ width: '100%' }}
                >
                  <Option value="profit-loss">Profit & Loss Statement</Option>
                  <Option value="balance-sheet">Balance Sheet</Option>
                  <Option value="cash-flow">Cash Flow Statement</Option>
                  <Option value="ar-aging">Accounts Receivable Aging</Option>
                  <Option value="ap-aging">Accounts Payable Aging</Option>
                  <Option value="tax-summary">Tax Summary</Option>
                  <Option value="budget-variance">Budget Variance</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col md={8} sm={24}>
              <Form.Item label="Date Range">
                <RangePicker 
                  value={dateRange} 
                  onChange={setDateRange} 
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
            <Col md={8} sm={24}>
              <Form.Item label="Reporting Period">
                <Select 
                  value={reportingPeriod} 
                  onChange={setReportingPeriod}
                  style={{ width: '100%' }}
                >
                  <Option value="daily">Daily</Option>
                  <Option value="weekly">Weekly</Option>
                  <Option value="monthly">Monthly</Option>
                  <Option value="quarterly">Quarterly</Option>
                  <Option value="yearly">Yearly</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col md={8} sm={24}>
              <Form.Item label="Compare With">
                <Radio.Group 
                  value={comparisonMode} 
                  onChange={handleComparisonModeChange}
                  buttonStyle="solid"
                >
                  <Radio.Button value="none">None</Radio.Button>
                  <Radio.Button value="previous-period">Previous Period</Radio.Button>
                  <Radio.Button value="previous-year">Previous Year</Radio.Button>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col md={8} sm={24}>
              <Form.Item label="View Mode">
                <Radio.Group 
                  value={viewMode} 
                  onChange={e => setViewMode(e.target.value)}
                  buttonStyle="solid"
                >
                  <Radio.Button value="table"><TableOutlined /> Table</Radio.Button>
                  <Radio.Button value="chart"><BarChartOutlined /> Chart</Radio.Button>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col md={8} sm={24} style={{ textAlign: 'right', marginTop: 30 }}>
              <Space>
                <Button 
                  icon={<DownloadOutlined />}
                  onClick={handleExportCSV}
                >
                  Export CSV
                </Button>
                <Button 
                  icon={<PrinterOutlined />}
                  onClick={handlePrint}
                >
                  Print
                </Button>
              </Space>
            </Col>
          </Row>
        </Form>
      </Card>
      
      {loading ? (
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Spin size="large" />
        </div>
      ) : error ? (
        <Alert
          message="Error"
          description={error}
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
        />
      ) : (
        <Card>
          <Title level={4}>
            {reportType === 'profit-loss' ? 'Profit & Loss Statement' :
             reportType === 'balance-sheet' ? 'Balance Sheet' :
             reportType === 'cash-flow' ? 'Cash Flow Statement' :
             reportType === 'ar-aging' ? 'Accounts Receivable Aging' : 
             reportType === 'ap-aging' ? 'Accounts Payable Aging' : 
             reportType === 'tax-summary' ? 'Tax Summary' : 'Budget Variance'}
            {reportType !== 'balance-sheet' && (
              <span style={{ marginLeft: 8, fontWeight: 'normal', fontSize: '16px' }}>
                ({dateRange[0].format('MMM D, YYYY')} - {dateRange[1].format('MMM D, YYYY')})
              </span>
            )}
            {reportType === 'balance-sheet' && reportData && (
              <span style={{ marginLeft: 8, fontWeight: 'normal', fontSize: '16px' }}>
                (As of {moment(reportData.asOf).format('MMM D, YYYY')})
              </span>
            )}
          </Title>
          
          {viewMode === 'table' ? renderReportTable() : renderReportChart()}
        </Card>
      )}
    </div>
  );
};

export default FinancialReportingCenter;
