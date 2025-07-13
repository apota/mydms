import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Table,
  Input,
  Select,
  DatePicker,
  Spin,
  Alert,
  Typography,
  Row,
  Col,
  Tabs,
  Space,
  Drawer,
  Form,
  InputNumber,
  Modal,
  Progress,
  Divider,
  Tooltip,
  Tag,
  Statistic,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  CopyOutlined,
  DeleteOutlined,
  DownloadOutlined,
  SearchOutlined,
  BarChartOutlined,
  PieChartOutlined,
  TableOutlined,
  WarningOutlined,
  CaretUpOutlined,
  CaretDownOutlined,
  CheckCircleOutlined,
  FileExcelOutlined,
} from '@ant-design/icons';
import moment from 'moment';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  Legend, ResponsiveContainer, LineChart, Line, PieChart, Pie, Cell
} from 'recharts';
import { financialService } from '../../services/financialService';

const { TabPane } = Tabs;
const { Title, Text } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8', '#82ca9d'];

const BudgetManagement = () => {
  const [budgets, setBudgets] = useState([]);
  const [budgetDetails, setBudgetDetails] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [currentTab, setCurrentTab] = useState('active-budgets');
  const [viewMode, setViewMode] = useState('table');
  
  // Budget form state
  const [budgetModalVisible, setBudgetModalVisible] = useState(false);
  const [budgetForm] = Form.useForm();
  const [editingBudget, setEditingBudget] = useState(null);
  
  // Budget allocation state
  const [allocationModalVisible, setAllocationModalVisible] = useState(false);
  const [allocationForm] = Form.useForm();
  const [selectedBudget, setSelectedBudget] = useState(null);
  
  // Budget details drawer
  const [detailsDrawerVisible, setDetailsDrawerVisible] = useState(false);
  const [selectedBudgetDetails, setSelectedBudgetDetails] = useState(null);
  
  // Filters
  const [yearFilter, setYearFilter] = useState(moment().year().toString());
  const [statusFilter, setStatusFilter] = useState('all');
  const [searchText, setSearchText] = useState('');
  
  // Load initial data
  useEffect(() => {
    fetchBudgets();
  }, []);
  
  // Fetch budgets
  const fetchBudgets = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        year: yearFilter !== 'all' ? yearFilter : null,
        status: statusFilter !== 'all' ? statusFilter : null,
      };
      
      const response = await financialService.getFinancialReports('budgets', params);
      setBudgets(response?.budgets || []);
    } catch (err) {
      setError('Failed to load budgets. Please try again.');
      console.error('Error fetching budgets:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Fetch budget details
  const fetchBudgetDetails = async (budgetId) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await financialService.getFinancialReports('budget-details', { budgetId });
      setBudgetDetails(response);
      setSelectedBudgetDetails(response);
      setDetailsDrawerVisible(true);
    } catch (err) {
      setError('Failed to load budget details. Please try again.');
      console.error('Error fetching budget details:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Create or update budget
  const handleBudgetFormSubmit = async (values) => {
    try {
      setLoading(true);
      setError(null);
      
      const budgetData = {
        ...values,
        startDate: values.period[0].format('YYYY-MM-DD'),
        endDate: values.period[1].format('YYYY-MM-DD'),
      };
      
      delete budgetData.period;
      
      if (editingBudget) {
        await financialService.updateBudget(editingBudget.id, budgetData);
      } else {
        await financialService.createBudget(budgetData);
      }
      
      setBudgetModalVisible(false);
      budgetForm.resetFields();
      setEditingBudget(null);
      fetchBudgets();
    } catch (err) {
      setError('Failed to save budget. Please try again.');
      console.error('Error saving budget:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Update budget allocations
  const handleAllocationFormSubmit = async (values) => {
    try {
      setLoading(true);
      setError(null);
      
      await financialService.updateBudgetAllocations(selectedBudget.id, values.allocations);
      
      setAllocationModalVisible(false);
      allocationForm.resetFields();
      setSelectedBudget(null);
      fetchBudgets();
    } catch (err) {
      setError('Failed to save budget allocations. Please try again.');
      console.error('Error saving budget allocations:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Delete budget
  const handleDeleteBudget = async (budgetId) => {
    try {
      setLoading(true);
      setError(null);
      
      await financialService.deleteBudget(budgetId);
      fetchBudgets();
    } catch (err) {
      setError('Failed to delete budget. Please try again.');
      console.error('Error deleting budget:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Clone budget
  const handleCloneBudget = async (budget) => {
    budgetForm.resetFields();
    
    budgetForm.setFieldsValue({
      name: `Copy of ${budget.name}`,
      description: budget.description,
      amount: budget.amount,
      type: budget.type,
      period: [moment(budget.startDate), moment(budget.endDate)],
      status: 'draft',
    });
    
    setEditingBudget(null);
    setBudgetModalVisible(true);
  };
  
  // Edit budget
  const handleEditBudget = (budget) => {
    setEditingBudget(budget);
    
    budgetForm.setFieldsValue({
      name: budget.name,
      description: budget.description,
      amount: budget.amount,
      type: budget.type,
      period: [moment(budget.startDate), moment(budget.endDate)],
      status: budget.status,
    });
    
    setBudgetModalVisible(true);
  };
  
  // Edit budget allocations
  const handleEditAllocations = (budget) => {
    setSelectedBudget(budget);
    
    // This would typically fetch the current allocations and set them in the form
    // For now, we'll just simulate it with some default allocations
    const defaultAllocations = [
      { category: 'operations', amount: budget.amount * 0.3 },
      { category: 'marketing', amount: budget.amount * 0.2 },
      { category: 'sales', amount: budget.amount * 0.2 },
      { category: 'it', amount: budget.amount * 0.15 },
      { category: 'administration', amount: budget.amount * 0.1 },
      { category: 'other', amount: budget.amount * 0.05 },
    ];
    
    allocationForm.setFieldsValue({
      allocations: defaultAllocations,
    });
    
    setAllocationModalVisible(true);
  };
  
  // Handle tab change
  const handleTabChange = (key) => {
    setCurrentTab(key);
    
    if (key === 'active-budgets') {
      setStatusFilter('active');
    } else if (key === 'all-budgets') {
      setStatusFilter('all');
    } else if (key === 'draft-budgets') {
      setStatusFilter('draft');
    }
    
    fetchBudgets();
  };
  
  // Budget columns for table
  const budgetColumns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a, b) => a.name.localeCompare(b.name),
      render: (text, record) => (
        <a onClick={() => fetchBudgetDetails(record.id)}>{text}</a>
      ),
    },
    {
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type) => {
        let displayType = '';
        switch (type) {
          case 'department':
            displayType = 'Department';
            break;
          case 'project':
            displayType = 'Project';
            break;
          case 'annual':
            displayType = 'Annual';
            break;
          case 'quarterly':
            displayType = 'Quarterly';
            break;
          default:
            displayType = type;
        }
        return displayType;
      },
      filters: [
        { text: 'Department', value: 'department' },
        { text: 'Project', value: 'project' },
        { text: 'Annual', value: 'annual' },
        { text: 'Quarterly', value: 'quarterly' },
      ],
      onFilter: (value, record) => record.type === value,
    },
    {
      title: 'Period',
      dataIndex: 'startDate',
      key: 'period',
      render: (_, record) => `${moment(record.startDate).format('MM/DD/YYYY')} - ${moment(record.endDate).format('MM/DD/YYYY')}`,
      sorter: (a, b) => moment(a.startDate).unix() - moment(b.startDate).unix(),
    },
    {
      title: 'Total Budget',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.amount - b.amount,
    },
    {
      title: 'Spent',
      dataIndex: 'spent',
      key: 'spent',
      render: (spent, record) => `$${spent.toFixed(2)} (${Math.round(spent / record.amount * 100)}%)`,
    },
    {
      title: 'Remaining',
      key: 'remaining',
      render: (_, record) => {
        const remaining = record.amount - record.spent;
        return `$${remaining.toFixed(2)}`;
      },
      sorter: (a, b) => (a.amount - a.spent) - (b.amount - b.spent),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        
        switch (status) {
          case 'active':
            color = 'green';
            break;
          case 'draft':
            color = 'blue';
            break;
          case 'closed':
            color = 'gray';
            break;
          case 'overspent':
            color = 'red';
            break;
          default:
            color = 'default';
        }
        
        return <Tag color={color}>{status.toUpperCase()}</Tag>;
      },
      filters: [
        { text: 'Active', value: 'active' },
        { text: 'Draft', value: 'draft' },
        { text: 'Closed', value: 'closed' },
        { text: 'Overspent', value: 'overspent' },
      ],
      onFilter: (value, record) => record.status === value,
    },
    {
      title: 'Progress',
      key: 'progress',
      render: (_, record) => {
        const percent = Math.round(record.spent / record.amount * 100);
        let status = 'normal';
        
        if (percent >= 100) {
          status = 'exception';
        } else if (percent >= 90) {
          status = 'warning';
        } else if (percent >= 0) {
          status = 'active';
        }
        
        return <Progress percent={percent} size="small" status={status} />;
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Edit Budget">
            <Button
              icon={<EditOutlined />}
              type="text"
              onClick={() => handleEditBudget(record)}
              disabled={record.status === 'closed'}
            />
          </Tooltip>
          <Tooltip title="Edit Allocations">
            <Button
              icon={<BarChartOutlined />}
              type="text"
              onClick={() => handleEditAllocations(record)}
              disabled={record.status === 'closed'}
            />
          </Tooltip>
          <Tooltip title="Clone Budget">
            <Button
              icon={<CopyOutlined />}
              type="text"
              onClick={() => handleCloneBudget(record)}
            />
          </Tooltip>
          <Tooltip title="Delete Budget">
            <Button
              icon={<DeleteOutlined />}
              type="text"
              danger
              onClick={() => handleDeleteBudget(record.id)}
              disabled={record.status !== 'draft'}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];
  
  // Filter budgets by search text
  const filteredBudgets = budgets.filter(budget => {
    if (!searchText) return true;
    
    const searchLower = searchText.toLowerCase();
    return (
      budget.name.toLowerCase().includes(searchLower) ||
      budget.description.toLowerCase().includes(searchLower)
    );
  });
  
  // Render budget chart
  const renderBudgetChart = () => {
    if (!budgets || budgets.length === 0) {
      return <Alert message="No budget data available for chart" type="info" />;
    }
    
    const chartData = budgets.map(budget => ({
      name: budget.name,
      budget: budget.amount,
      spent: budget.spent,
      remaining: budget.amount - budget.spent,
    }));
    
    return (
      <ResponsiveContainer width="100%" height={400}>
        <BarChart
          data={chartData}
          margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" />
          <YAxis />
          <RechartsTooltip formatter={(value) => `$${value.toFixed(2)}`} />
          <Legend />
          <Bar dataKey="budget" name="Total Budget" fill="#8884d8" />
          <Bar dataKey="spent" name="Spent" fill="#82ca9d" />
          <Bar dataKey="remaining" name="Remaining" fill="#ffc658" />
        </BarChart>
      </ResponsiveContainer>
    );
  };
  
  // Render budget allocation chart
  const renderBudgetAllocationChart = () => {
    if (!selectedBudgetDetails || !selectedBudgetDetails.allocations) {
      return <Alert message="No allocation data available" type="info" />;
    }
    
    const data = selectedBudgetDetails.allocations.map((item, index) => ({
      name: item.category,
      value: item.amount,
      fill: COLORS[index % COLORS.length],
    }));
    
    return (
      <ResponsiveContainer width="100%" height={300}>
        <PieChart>
          <Pie
            data={data}
            cx="50%"
            cy="50%"
            labelLine={false}
            outerRadius={100}
            fill="#8884d8"
            dataKey="value"
            nameKey="name"
            label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
          >
            {data.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
            ))}
          </Pie>
          <RechartsTooltip formatter={(value) => `$${value.toFixed(2)}`} />
        </PieChart>
      </ResponsiveContainer>
    );
  };
  
  // Render monthly spending chart
  const renderMonthlySpendingChart = () => {
    if (!selectedBudgetDetails || !selectedBudgetDetails.monthlySpending) {
      return <Alert message="No monthly spending data available" type="info" />;
    }
    
    return (
      <ResponsiveContainer width="100%" height={300}>
        <LineChart
          data={selectedBudgetDetails.monthlySpending}
          margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="month" />
          <YAxis />
          <RechartsTooltip formatter={(value) => `$${value.toFixed(2)}`} />
          <Legend />
          <Line 
            type="monotone" 
            dataKey="budget" 
            stroke="#8884d8" 
            name="Budget" 
            activeDot={{ r: 8 }} 
          />
          <Line 
            type="monotone" 
            dataKey="actual" 
            stroke="#82ca9d" 
            name="Actual" 
            activeDot={{ r: 8 }} 
          />
        </LineChart>
      </ResponsiveContainer>
    );
  };

  return (
    <div className="budget-management">
      <Title level={2}>Budget Management</Title>
      
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16} align="middle">
          <Col xs={24} md={6} lg={5}>
            <Input
              placeholder="Search budgets..."
              prefix={<SearchOutlined />}
              value={searchText}
              onChange={e => setSearchText(e.target.value)}
            />
          </Col>
          <Col xs={24} md={6} lg={4}>
            <Select
              style={{ width: '100%' }}
              placeholder="Year"
              value={yearFilter}
              onChange={setYearFilter}
            >
              <Option value="all">All Years</Option>
              <Option value="2025">2025</Option>
              <Option value="2024">2024</Option>
              <Option value="2023">2023</Option>
            </Select>
          </Col>
          <Col xs={24} md={6} lg={4}>
            <Button
              type="primary"
              icon={<SearchOutlined />}
              onClick={fetchBudgets}
            >
              Search
            </Button>
          </Col>
          <Col xs={24} md={6} lg={11} style={{ textAlign: 'right' }}>
            <Space>
              <Radio.Group 
                value={viewMode} 
                onChange={e => setViewMode(e.target.value)}
                buttonStyle="solid"
              >
                <Radio.Button value="table"><TableOutlined /> Table</Radio.Button>
                <Radio.Button value="chart"><BarChartOutlined /> Chart</Radio.Button>
              </Radio.Group>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  setEditingBudget(null);
                  budgetForm.resetFields();
                  setBudgetModalVisible(true);
                }}
              >
                Create Budget
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>
      
      <Tabs activeKey={currentTab} onChange={handleTabChange}>
        <TabPane tab="Active Budgets" key="active-budgets">
          <Card>
            {error && (
              <Alert
                message="Error"
                description={error}
                type="error"
                showIcon
                closable
                style={{ marginBottom: 16 }}
              />
            )}
            
            {loading ? (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <Spin size="large" />
              </div>
            ) : viewMode === 'table' ? (
              <Table
                columns={budgetColumns}
                dataSource={filteredBudgets}
                rowKey="id"
                pagination={{ pageSize: 10 }}
              />
            ) : (
              renderBudgetChart()
            )}
          </Card>
        </TabPane>
        
        <TabPane tab="All Budgets" key="all-budgets">
          <Card>
            {error && (
              <Alert
                message="Error"
                description={error}
                type="error"
                showIcon
                closable
                style={{ marginBottom: 16 }}
              />
            )}
            
            {loading ? (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <Spin size="large" />
              </div>
            ) : viewMode === 'table' ? (
              <Table
                columns={budgetColumns}
                dataSource={filteredBudgets}
                rowKey="id"
                pagination={{ pageSize: 10 }}
              />
            ) : (
              renderBudgetChart()
            )}
          </Card>
        </TabPane>
        
        <TabPane tab="Draft Budgets" key="draft-budgets">
          <Card>
            {error && (
              <Alert
                message="Error"
                description={error}
                type="error"
                showIcon
                closable
                style={{ marginBottom: 16 }}
              />
            )}
            
            {loading ? (
              <div style={{ textAlign: 'center', padding: '40px 0' }}>
                <Spin size="large" />
              </div>
            ) : (
              <Table
                columns={budgetColumns}
                dataSource={filteredBudgets}
                rowKey="id"
                pagination={{ pageSize: 10 }}
              />
            )}
          </Card>
        </TabPane>
      </Tabs>
      
      {/* Budget Form Modal */}
      <Modal
        title={editingBudget ? "Edit Budget" : "Create New Budget"}
        visible={budgetModalVisible}
        onCancel={() => setBudgetModalVisible(false)}
        footer={null}
        width={700}
      >
        <Form
          form={budgetForm}
          layout="vertical"
          onFinish={handleBudgetFormSubmit}
        >
          <Row gutter={16}>
            <Col span={24}>
              <Form.Item
                name="name"
                label="Budget Name"
                rules={[{ required: true, message: 'Please enter budget name' }]}
              >
                <Input placeholder="Enter budget name" />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="type"
                label="Budget Type"
                rules={[{ required: true, message: 'Please select budget type' }]}
              >
                <Select placeholder="Select budget type">
                  <Option value="department">Department</Option>
                  <Option value="project">Project</Option>
                  <Option value="annual">Annual</Option>
                  <Option value="quarterly">Quarterly</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="amount"
                label="Total Budget Amount"
                rules={[{ required: true, message: 'Please enter budget amount' }]}
              >
                <InputNumber
                  style={{ width: '100%' }}
                  formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={value => value.replace(/\$\s?|(,*)/g, '')}
                  min={0}
                  precision={2}
                />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={16}>
              <Form.Item
                name="period"
                label="Budget Period"
                rules={[{ required: true, message: 'Please select budget period' }]}
              >
                <RangePicker style={{ width: '100%' }} format="MM/DD/YYYY" />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="status"
                label="Status"
                rules={[{ required: true, message: 'Please select status' }]}
                initialValue="draft"
              >
                <Select placeholder="Select status">
                  <Option value="draft">Draft</Option>
                  <Option value="active">Active</Option>
                  <Option value="closed">Closed</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item
            name="description"
            label="Description"
          >
            <Input.TextArea rows={4} placeholder="Enter budget description" />
          </Form.Item>
          
          <div style={{ textAlign: 'right' }}>
            <Button
              style={{ marginRight: 8 }}
              onClick={() => setBudgetModalVisible(false)}
            >
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              {editingBudget ? "Update Budget" : "Create Budget"}
            </Button>
          </div>
        </Form>
      </Modal>
      
      {/* Budget Allocation Modal */}
      <Modal
        title="Budget Allocations"
        visible={allocationModalVisible}
        onCancel={() => setAllocationModalVisible(false)}
        footer={null}
        width={700}
      >
        {selectedBudget && (
          <Form
            form={allocationForm}
            layout="vertical"
            onFinish={handleAllocationFormSubmit}
          >
            <Title level={5}>
              {selectedBudget.name} - Total: ${selectedBudget.amount.toFixed(2)}
            </Title>
            <Divider />
            
            <Form.List name="allocations">
              {(fields, { add, remove }) => (
                <>
                  {fields.map(({ key, name, ...restField }) => (
                    <Row gutter={16} key={key} align="middle" style={{ marginBottom: 16 }}>
                      <Col span={12}>
                        <Form.Item
                          {...restField}
                          name={[name, 'category']}
                          rules={[{ required: true, message: 'Missing category' }]}
                        >
                          <Select placeholder="Select category">
                            <Option value="operations">Operations</Option>
                            <Option value="marketing">Marketing</Option>
                            <Option value="sales">Sales</Option>
                            <Option value="it">IT</Option>
                            <Option value="administration">Administration</Option>
                            <Option value="other">Other</Option>
                          </Select>
                        </Form.Item>
                      </Col>
                      <Col span={10}>
                        <Form.Item
                          {...restField}
                          name={[name, 'amount']}
                          rules={[{ required: true, message: 'Missing amount' }]}
                        >
                          <InputNumber
                            style={{ width: '100%' }}
                            formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                            parser={value => value.replace(/\$\s?|(,*)/g, '')}
                            min={0}
                            precision={2}
                          />
                        </Form.Item>
                      </Col>
                      <Col span={2} style={{ textAlign: 'right' }}>
                        <Button
                          type="text"
                          danger
                          icon={<DeleteOutlined />}
                          onClick={() => remove(name)}
                        />
                      </Col>
                    </Row>
                  ))}
                  <Form.Item>
                    <Button 
                      type="dashed" 
                      onClick={() => add()} 
                      block 
                      icon={<PlusOutlined />}
                    >
                      Add Allocation
                    </Button>
                  </Form.Item>
                  
                  <Form.Item>
                    {() => {
                      const values = allocationForm.getFieldsValue();
                      const total = values.allocations?.reduce((sum, allocation) => sum + (allocation.amount || 0), 0) || 0;
                      const remaining = selectedBudget.amount - total;
                      
                      return (
                        <Alert
                          message={`Allocated: $${total.toFixed(2)} | Remaining: $${remaining.toFixed(2)}`}
                          type={remaining < 0 ? "error" : remaining > 0 ? "warning" : "success"}
                          showIcon
                        />
                      );
                    }}
                  </Form.Item>
                </>
              )}
            </Form.List>
            
            <div style={{ textAlign: 'right' }}>
              <Button
                style={{ marginRight: 8 }}
                onClick={() => setAllocationModalVisible(false)}
              >
                Cancel
              </Button>
              <Button type="primary" htmlType="submit" loading={loading}>
                Save Allocations
              </Button>
            </div>
          </Form>
        )}
      </Modal>
      
      {/* Budget Details Drawer */}
      <Drawer
        title="Budget Details"
        placement="right"
        width={800}
        onClose={() => setDetailsDrawerVisible(false)}
        visible={detailsDrawerVisible}
        extra={
          <Space>
            <Button
              icon={<FileExcelOutlined />}
              onClick={() => console.log('Export to Excel')}
            >
              Export
            </Button>
            <Button
              type="primary"
              onClick={() => selectedBudgetDetails && handleEditBudget(selectedBudgetDetails)}
              disabled={selectedBudgetDetails?.status === 'closed'}
            >
              Edit
            </Button>
          </Space>
        }
      >
        {selectedBudgetDetails && (
          <>
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic 
                  title="Budget Name" 
                  value={selectedBudgetDetails.name}
                />
              </Col>
              <Col span={8}>
                <Statistic 
                  title="Type" 
                  value={
                    selectedBudgetDetails.type === 'department' ? 'Department' :
                    selectedBudgetDetails.type === 'project' ? 'Project' :
                    selectedBudgetDetails.type === 'annual' ? 'Annual' :
                    selectedBudgetDetails.type === 'quarterly' ? 'Quarterly' :
                    selectedBudgetDetails.type
                  }
                />
              </Col>
              <Col span={8}>
                <Statistic 
                  title="Status" 
                  value={selectedBudgetDetails.status.toUpperCase()}
                  valueStyle={{ 
                    color: 
                      selectedBudgetDetails.status === 'active' ? 'green' : 
                      selectedBudgetDetails.status === 'overspent' ? 'red' : 
                      'inherit'
                  }}
                />
              </Col>
            </Row>
            
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic 
                  title="Total Budget" 
                  value={selectedBudgetDetails.amount}
                  precision={2}
                  prefix="$"
                />
              </Col>
              <Col span={8}>
                <Statistic 
                  title="Spent" 
                  value={selectedBudgetDetails.spent}
                  precision={2}
                  prefix="$"
                  suffix={`(${Math.round(selectedBudgetDetails.spent / selectedBudgetDetails.amount * 100)}%)`}
                />
              </Col>
              <Col span={8}>
                <Statistic 
                  title="Remaining" 
                  value={selectedBudgetDetails.amount - selectedBudgetDetails.spent}
                  precision={2}
                  prefix="$"
                  valueStyle={{ 
                    color: selectedBudgetDetails.amount - selectedBudgetDetails.spent < 0 ? 'red' : 'inherit'
                  }}
                />
              </Col>
            </Row>
            
            <Divider orientation="left">Budget Allocations</Divider>
            {renderBudgetAllocationChart()}
            
            <Divider orientation="left">Monthly Spending</Divider>
            {renderMonthlySpendingChart()}
            
            <Divider orientation="left">Recent Transactions</Divider>
            {selectedBudgetDetails.recentTransactions && (
              <Table
                dataSource={selectedBudgetDetails.recentTransactions}
                columns={[
                  {
                    title: 'Date',
                    dataIndex: 'date',
                    key: 'date',
                    render: (date) => moment(date).format('MM/DD/YYYY'),
                  },
                  {
                    title: 'Description',
                    dataIndex: 'description',
                    key: 'description',
                  },
                  {
                    title: 'Category',
                    dataIndex: 'category',
                    key: 'category',
                    render: (category) => (
                      <Tag color="blue">{category}</Tag>
                    ),
                  },
                  {
                    title: 'Amount',
                    dataIndex: 'amount',
                    key: 'amount',
                    render: (amount) => `$${amount.toFixed(2)}`,
                  },
                  {
                    title: 'Type',
                    dataIndex: 'type',
                    key: 'type',
                    render: (type) => (
                      type === 'expense' ? (
                        <Tag color="red">Expense</Tag>
                      ) : (
                        <Tag color="green">Adjustment</Tag>
                      )
                    ),
                  },
                ]}
                rowKey="id"
                pagination={{ pageSize: 5 }}
                size="small"
              />
            )}
            
            {selectedBudgetDetails.notes && (
              <>
                <Divider orientation="left">Notes</Divider>
                <p>{selectedBudgetDetails.notes}</p>
              </>
            )}
          </>
        )}
      </Drawer>
    </div>
  );
};

export default BudgetManagement;
