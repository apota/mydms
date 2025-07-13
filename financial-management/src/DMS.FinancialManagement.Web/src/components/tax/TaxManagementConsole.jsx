import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Table,
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
  Divider,
  Tag,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Progress,
  List,
  Tooltip,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  DownloadOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
  ExclamationCircleOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  QuestionCircleOutlined,
  SearchOutlined,
} from '@ant-design/icons';
import moment from 'moment';
import { financialService } from '../../services/financialService';

const { TabPane } = Tabs;
const { Title, Text } = Typography;
const { RangePicker } = DatePicker;
const { Option } = Select;

const TaxManagementConsole = () => {
  const [taxRates, setTaxRates] = useState([]);
  const [taxReports, setTaxReports] = useState([]);
  const [taxFilings, setTaxFilings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [currentTab, setCurrentTab] = useState('tax-rates');
  
  // Tax rate state
  const [taxRateModalVisible, setTaxRateModalVisible] = useState(false);
  const [taxRateForm] = Form.useForm();
  const [editingTaxRate, setEditingTaxRate] = useState(null);
  
  // Tax reports state
  const [reportPeriod, setReportPeriod] = useState([moment().startOf('year'), moment()]);
  const [reportType, setReportType] = useState('sales-tax');
  const [reportDetailsDrawerVisible, setReportDetailsDrawerVisible] = useState(false);
  const [currentReportDetails, setCurrentReportDetails] = useState(null);
  
  // Tax filings state
  const [filingPeriod, setFilingPeriod] = useState([moment().subtract(1, 'year'), moment()]);
  const [filingDetailsDrawerVisible, setFilingDetailsDrawerVisible] = useState(false);
  const [currentFilingDetails, setCurrentFilingDetails] = useState(null);
  
  // Load initial data
  useEffect(() => {
    fetchTaxRates();
  }, []);
  
  // Fetch tax rates
  const fetchTaxRates = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await financialService.getTaxRates();
      setTaxRates(response);
    } catch (err) {
      setError('Failed to load tax rates. Please try again.');
      console.error('Error fetching tax rates:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Fetch tax reports
  const fetchTaxReports = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        reportType,
        fromDate: reportPeriod[0].format('YYYY-MM-DD'),
        toDate: reportPeriod[1].format('YYYY-MM-DD'),
      };
      
      const response = await financialService.getTaxReports(params);
      setTaxReports(response);
    } catch (err) {
      setError('Failed to load tax reports. Please try again.');
      console.error('Error fetching tax reports:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Fetch tax filings
  const fetchTaxFilings = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        fromDate: filingPeriod[0].format('YYYY-MM-DD'),
        toDate: filingPeriod[1].format('YYYY-MM-DD'),
      };
      
      const response = await financialService.getFinancialReports('tax-filings', params);
      setTaxFilings(response?.filings || []);
    } catch (err) {
      setError('Failed to load tax filings. Please try again.');
      console.error('Error fetching tax filings:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Handle tab change
  const handleTabChange = (key) => {
    setCurrentTab(key);
    
    if (key === 'tax-reports' && taxReports.length === 0) {
      fetchTaxReports();
    } else if (key === 'tax-filings' && taxFilings.length === 0) {
      fetchTaxFilings();
    }
  };
  
  // Create or edit tax rate
  const handleTaxRateFormSubmit = async (values) => {
    try {
      setLoading(true);
      setError(null);
      
      if (editingTaxRate) {
        // Update existing tax rate
        await financialService.updateTaxRate(editingTaxRate.id, values);
      } else {
        // Create new tax rate
        await financialService.createTaxRate(values);
      }
      
      setTaxRateModalVisible(false);
      fetchTaxRates();
      taxRateForm.resetFields();
      setEditingTaxRate(null);
    } catch (err) {
      setError('Failed to save tax rate. Please try again.');
      console.error('Error saving tax rate:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // Edit tax rate
  const handleEditTaxRate = (taxRate) => {
    setEditingTaxRate(taxRate);
    taxRateForm.setFieldsValue({
      name: taxRate.name,
      code: taxRate.code,
      rate: taxRate.rate,
      isActive: taxRate.isActive,
      description: taxRate.description,
      jurisdiction: taxRate.jurisdiction,
      applicableItems: taxRate.applicableItems,
    });
    setTaxRateModalVisible(true);
  };
  
  // Delete tax rate
  const handleDeleteTaxRate = async (taxRateId) => {
    try {
      setLoading(true);
      setError(null);
      
      await financialService.deleteTaxRate(taxRateId);
      fetchTaxRates();
    } catch (err) {
      setError('Failed to delete tax rate. Please try again.');
      console.error('Error deleting tax rate:', err);
    } finally {
      setLoading(false);
    }
  };
  
  // View report details
  const handleViewReportDetails = (report) => {
    setCurrentReportDetails(report);
    setReportDetailsDrawerVisible(true);
  };
  
  // View filing details
  const handleViewFilingDetails = (filing) => {
    setCurrentFilingDetails(filing);
    setFilingDetailsDrawerVisible(true);
  };
  
  // Generate new report
  const handleGenerateReport = async () => {
    fetchTaxReports();
  };
  
  // Generate tax filing forms
  const handleGenerateFilingForms = async (reportId) => {
    // Implementation would depend on backend API
    console.log('Generate filing forms for report ID:', reportId);
  };
  
  // Submit tax filing
  const handleSubmitFiling = async (filingId) => {
    // Implementation would depend on backend API
    console.log('Submit filing ID:', filingId);
  };
  
  // Export tax data
  const handleExportData = async (format, dataType, id) => {
    // Implementation would depend on backend API
    console.log(`Export ${dataType} ID ${id} as ${format}`);
  };
  
  // Tax rate columns
  const taxRateColumns = [
    {
      title: 'Name',
      dataIndex: 'name',
      key: 'name',
      sorter: (a, b) => a.name.localeCompare(b.name),
    },
    {
      title: 'Code',
      dataIndex: 'code',
      key: 'code',
    },
    {
      title: 'Rate',
      dataIndex: 'rate',
      key: 'rate',
      render: (rate) => `${(rate * 100).toFixed(2)}%`,
      sorter: (a, b) => a.rate - b.rate,
    },
    {
      title: 'Jurisdiction',
      dataIndex: 'jurisdiction',
      key: 'jurisdiction',
      filters: [
        { text: 'Federal', value: 'Federal' },
        { text: 'State', value: 'State' },
        { text: 'Local', value: 'Local' },
        { text: 'Special District', value: 'Special District' },
      ],
      onFilter: (value, record) => record.jurisdiction === value,
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive) => (
        isActive ? 
          <Tag color="green">Active</Tag> : 
          <Tag color="red">Inactive</Tag>
      ),
      filters: [
        { text: 'Active', value: true },
        { text: 'Inactive', value: false },
      ],
      onFilter: (value, record) => record.isActive === value,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Button
            icon={<EditOutlined />}
            type="text"
            onClick={() => handleEditTaxRate(record)}
          />
          <Popconfirm
            title="Are you sure you want to delete this tax rate?"
            onConfirm={() => handleDeleteTaxRate(record.id)}
            okText="Yes"
            cancelText="No"
            icon={<QuestionCircleOutlined style={{ color: 'red' }} />}
          >
            <Button
              icon={<DeleteOutlined />}
              type="text"
              danger
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];
  
  // Tax report columns
  const taxReportColumns = [
    {
      title: 'Report Type',
      dataIndex: 'type',
      key: 'type',
      filters: [
        { text: 'Sales Tax', value: 'sales-tax' },
        { text: 'VAT', value: 'vat' },
        { text: 'Income Tax', value: 'income-tax' },
        { text: 'Payroll Tax', value: 'payroll-tax' },
      ],
      onFilter: (value, record) => record.type === value,
      render: (type) => {
        let displayName = '';
        switch (type) {
          case 'sales-tax':
            displayName = 'Sales Tax';
            break;
          case 'vat':
            displayName = 'VAT';
            break;
          case 'income-tax':
            displayName = 'Income Tax';
            break;
          case 'payroll-tax':
            displayName = 'Payroll Tax';
            break;
          default:
            displayName = type;
        }
        return displayName;
      },
    },
    {
      title: 'Period',
      dataIndex: 'period',
      key: 'period',
      render: (period) => `${moment(period.startDate).format('MM/DD/YYYY')} - ${moment(period.endDate).format('MM/DD/YYYY')}`,
      sorter: (a, b) => moment(a.period.startDate).unix() - moment(b.period.startDate).unix(),
    },
    {
      title: 'Generated',
      dataIndex: 'generatedDate',
      key: 'generatedDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.generatedDate).unix() - moment(b.generatedDate).unix(),
    },
    {
      title: 'Total Tax',
      dataIndex: 'totalTax',
      key: 'totalTax',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.totalTax - b.totalTax,
    },
    {
      title: 'Filing Status',
      dataIndex: 'filingStatus',
      key: 'filingStatus',
      render: (status) => {
        let color = 'default';
        let icon = null;
        
        switch(status) {
          case 'not_filed':
            color = 'orange';
            icon = <ClockCircleOutlined />;
            break;
          case 'filed':
            color = 'green';
            icon = <CheckCircleOutlined />;
            break;
          case 'overdue':
            color = 'red';
            icon = <ExclamationCircleOutlined />;
            break;
          default:
            color = 'default';
        }
        
        return (
          <Tag color={color} icon={icon}>
            {status === 'not_filed' ? 'Not Filed' : 
             status === 'filed' ? 'Filed' : 
             status === 'overdue' ? 'Overdue' : status}
          </Tag>
        );
      },
      filters: [
        { text: 'Not Filed', value: 'not_filed' },
        { text: 'Filed', value: 'filed' },
        { text: 'Overdue', value: 'overdue' },
      ],
      onFilter: (value, record) => record.filingStatus === value,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            onClick={() => handleViewReportDetails(record)}
          >
            View Details
          </Button>
          {record.filingStatus === 'not_filed' && (
            <Button
              type="primary"
              size="small"
              onClick={() => handleGenerateFilingForms(record.id)}
            >
              Generate Forms
            </Button>
          )}
          <Dropdown menu={{
            items: [
              {
                key: 'pdf',
                label: (
                  <Button
                    type="text"
                    icon={<FilePdfOutlined />}
                    onClick={() => handleExportData('pdf', 'report', record.id)}
                  >
                    Export as PDF
                  </Button>
                ),
              },
              {
                key: 'excel',
                label: (
                  <Button
                    type="text"
                    icon={<FileExcelOutlined />}
                    onClick={() => handleExportData('excel', 'report', record.id)}
                  >
                    Export as Excel
                  </Button>
                ),
              },
            ]
          }}
          >
            <Button icon={<DownloadOutlined />} />
          </Dropdown>
        </Space>
      ),
    },
  ];
  
  // Tax filing columns
  const taxFilingColumns = [
    {
      title: 'Filing Type',
      dataIndex: 'type',
      key: 'type',
      filters: [
        { text: 'Sales Tax', value: 'sales-tax' },
        { text: 'VAT', value: 'vat' },
        { text: 'Income Tax', value: 'income-tax' },
        { text: 'Payroll Tax', value: 'payroll-tax' },
      ],
      onFilter: (value, record) => record.type === value,
      render: (type) => {
        let displayName = '';
        switch (type) {
          case 'sales-tax':
            displayName = 'Sales Tax';
            break;
          case 'vat':
            displayName = 'VAT';
            break;
          case 'income-tax':
            displayName = 'Income Tax';
            break;
          case 'payroll-tax':
            displayName = 'Payroll Tax';
            break;
          default:
            displayName = type;
        }
        return displayName;
      },
    },
    {
      title: 'Period',
      dataIndex: 'period',
      key: 'period',
      render: (period) => `${period.name} (${moment(period.startDate).format('MM/DD/YYYY')} - ${moment(period.endDate).format('MM/DD/YYYY')})`,
    },
    {
      title: 'Authority',
      dataIndex: 'authority',
      key: 'authority',
    },
    {
      title: 'Due Date',
      dataIndex: 'dueDate',
      key: 'dueDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.dueDate).unix() - moment(b.dueDate).unix(),
    },
    {
      title: 'Total Tax',
      dataIndex: 'totalTax',
      key: 'totalTax',
      render: (amount) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        let icon = null;
        
        switch(status) {
          case 'pending':
            color = 'blue';
            icon = <ClockCircleOutlined />;
            break;
          case 'submitted':
            color = 'green';
            icon = <CheckCircleOutlined />;
            break;
          case 'late':
            color = 'red';
            icon = <ExclamationCircleOutlined />;
            break;
          default:
            color = 'default';
        }
        
        return (
          <Tag color={color} icon={icon}>
            {status.toUpperCase()}
          </Tag>
        );
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            onClick={() => handleViewFilingDetails(record)}
          >
            View Details
          </Button>
          {record.status === 'pending' && (
            <Button
              type="primary"
              size="small"
              onClick={() => handleSubmitFiling(record.id)}
            >
              Submit Filing
            </Button>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div className="tax-management-console">
      <Title level={2}>Tax Management Console</Title>
      
      <Tabs activeKey={currentTab} onChange={handleTabChange}>
        <TabPane tab="Tax Rates" key="tax-rates">
          <Card>
            <div style={{ marginBottom: 16, textAlign: 'right' }}>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => {
                  setEditingTaxRate(null);
                  taxRateForm.resetFields();
                  setTaxRateModalVisible(true);
                }}
              >
                Add Tax Rate
              </Button>
            </div>
            
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
            
            <Table
              columns={taxRateColumns}
              dataSource={taxRates}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
        
        <TabPane tab="Tax Reports" key="tax-reports">
          <Card>
            <div style={{ marginBottom: 16 }}>
              <Row gutter={16} align="middle">
                <Col xs={24} md={8} lg={6}>
                  <Select 
                    style={{ width: '100%' }}
                    value={reportType}
                    onChange={setReportType}
                    placeholder="Report Type"
                  >
                    <Option value="sales-tax">Sales Tax</Option>
                    <Option value="vat">VAT</Option>
                    <Option value="income-tax">Income Tax</Option>
                    <Option value="payroll-tax">Payroll Tax</Option>
                  </Select>
                </Col>
                <Col xs={24} md={10} lg={8}>
                  <RangePicker
                    style={{ width: '100%' }}
                    value={reportPeriod}
                    onChange={setReportPeriod}
                  />
                </Col>
                <Col xs={24} md={6} lg={6}>
                  <Button
                    type="primary"
                    icon={<SearchOutlined />}
                    onClick={handleGenerateReport}
                  >
                    Generate Report
                  </Button>
                </Col>
              </Row>
            </div>
            
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
            
            <Table
              columns={taxReportColumns}
              dataSource={taxReports}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
        
        <TabPane tab="Tax Filings" key="tax-filings">
          <Card>
            <div style={{ marginBottom: 16 }}>
              <Row gutter={16} align="middle">
                <Col span={12}>
                  <RangePicker
                    style={{ width: '100%' }}
                    value={filingPeriod}
                    onChange={setFilingPeriod}
                  />
                </Col>
                <Col span={4}>
                  <Button
                    type="primary"
                    icon={<SearchOutlined />}
                    onClick={fetchTaxFilings}
                  >
                    Search
                  </Button>
                </Col>
              </Row>
            </div>
            
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
            
            <Table
              columns={taxFilingColumns}
              dataSource={taxFilings}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
      </Tabs>
      
      {/* Tax Rate Form Modal */}
      <Modal
        title={editingTaxRate ? "Edit Tax Rate" : "Add Tax Rate"}
        visible={taxRateModalVisible}
        onCancel={() => setTaxRateModalVisible(false)}
        footer={null}
        width={700}
      >
        <Form
          form={taxRateForm}
          layout="vertical"
          onFinish={handleTaxRateFormSubmit}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="name"
                label="Tax Name"
                rules={[{ required: true, message: 'Please enter tax name' }]}
              >
                <Input placeholder="Enter tax name" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="code"
                label="Tax Code"
                rules={[{ required: true, message: 'Please enter tax code' }]}
              >
                <Input placeholder="Enter tax code" />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={8}>
              <Form.Item
                name="rate"
                label="Tax Rate (%)"
                rules={[{ required: true, message: 'Please enter tax rate' }]}
              >
                <InputNumber
                  style={{ width: '100%' }}
                  min={0}
                  max={100}
                  precision={2}
                  formatter={value => `${value}%`}
                  parser={value => value.replace('%', '')}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="jurisdiction"
                label="Jurisdiction"
                rules={[{ required: true, message: 'Please select jurisdiction' }]}
              >
                <Select placeholder="Select jurisdiction">
                  <Option value="Federal">Federal</Option>
                  <Option value="State">State</Option>
                  <Option value="Local">Local</Option>
                  <Option value="Special District">Special District</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item
                name="isActive"
                label="Status"
                valuePropName="checked"
                initialValue={true}
              >
                <Select>
                  <Option value={true}>Active</Option>
                  <Option value={false}>Inactive</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item
            name="applicableItems"
            label="Applicable Items"
            rules={[{ required: true, message: 'Please select applicable items' }]}
          >
            <Select
              mode="multiple"
              placeholder="Select applicable items"
              style={{ width: '100%' }}
            >
              <Option value="all">All Items</Option>
              <Option value="services">Services</Option>
              <Option value="products">Products</Option>
              <Option value="parts">Parts</Option>
              <Option value="labor">Labor</Option>
            </Select>
          </Form.Item>
          
          <Form.Item
            name="description"
            label="Description"
          >
            <Input.TextArea rows={4} placeholder="Enter description" />
          </Form.Item>
          
          <div style={{ textAlign: 'right' }}>
            <Button
              style={{ marginRight: 8 }}
              onClick={() => setTaxRateModalVisible(false)}
            >
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              {editingTaxRate ? "Update" : "Create"}
            </Button>
          </div>
        </Form>
      </Modal>
      
      {/* Report Details Drawer */}
      <Drawer
        title="Tax Report Details"
        placement="right"
        width={700}
        onClose={() => setReportDetailsDrawerVisible(false)}
        visible={reportDetailsDrawerVisible}
        extra={
          <Space>
            <Button 
              icon={<FilePdfOutlined />}
              onClick={() => currentReportDetails && handleExportData('pdf', 'report', currentReportDetails.id)}
            >
              PDF
            </Button>
            <Button 
              icon={<FileExcelOutlined />}
              onClick={() => currentReportDetails && handleExportData('excel', 'report', currentReportDetails.id)}
            >
              Excel
            </Button>
            {currentReportDetails && currentReportDetails.filingStatus === 'not_filed' && (
              <Button 
                type="primary"
                onClick={() => currentReportDetails && handleGenerateFilingForms(currentReportDetails.id)}
              >
                Generate Filing
              </Button>
            )}
          </Space>
        }
      >
        {currentReportDetails && (
          <>
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Statistic 
                  title="Report Type" 
                  value={
                    currentReportDetails.type === 'sales-tax' ? 'Sales Tax' :
                    currentReportDetails.type === 'vat' ? 'VAT' :
                    currentReportDetails.type === 'income-tax' ? 'Income Tax' :
                    currentReportDetails.type === 'payroll-tax' ? 'Payroll Tax' : 
                    currentReportDetails.type
                  } 
                />
              </Col>
              <Col span={12}>
                <Statistic title="Total Tax" value={`$${currentReportDetails.totalTax.toFixed(2)}`} />
              </Col>
              <Col span={12}>
                <Statistic 
                  title="Period" 
                  value={`${moment(currentReportDetails.period.startDate).format('MM/DD/YYYY')} - ${moment(currentReportDetails.period.endDate).format('MM/DD/YYYY')}`} 
                />
              </Col>
              <Col span={12}>
                <Statistic 
                  title="Filing Status" 
                  value={
                    currentReportDetails.filingStatus === 'not_filed' ? 'Not Filed' :
                    currentReportDetails.filingStatus === 'filed' ? 'Filed' :
                    currentReportDetails.filingStatus === 'overdue' ? 'Overdue' :
                    currentReportDetails.filingStatus
                  }
                  valueStyle={{ 
                    color: 
                      currentReportDetails.filingStatus === 'filed' ? 'green' : 
                      currentReportDetails.filingStatus === 'overdue' ? 'red' : 
                      'orange'
                  }}
                />
              </Col>
            </Row>
            
            <Divider />
            
            {currentReportDetails.details && (
              <>
                <Title level={5}>Tax Breakdown</Title>
                <Table
                  dataSource={currentReportDetails.details}
                  columns={[
                    {
                      title: 'Tax Category',
                      dataIndex: 'category',
                      key: 'category',
                    },
                    {
                      title: 'Taxable Amount',
                      dataIndex: 'taxableAmount',
                      key: 'taxableAmount',
                      render: (amount) => `$${amount.toFixed(2)}`,
                    },
                    {
                      title: 'Tax Rate',
                      dataIndex: 'taxRate',
                      key: 'taxRate',
                      render: (rate) => `${(rate * 100).toFixed(2)}%`,
                    },
                    {
                      title: 'Tax Amount',
                      dataIndex: 'taxAmount',
                      key: 'taxAmount',
                      render: (amount) => `$${amount.toFixed(2)}`,
                    },
                  ]}
                  rowKey="category"
                  pagination={false}
                  size="small"
                />
              </>
            )}
            
            {currentReportDetails.transactions && (
              <>
                <Divider />
                <Title level={5}>Transactions</Title>
                <Table
                  dataSource={currentReportDetails.transactions}
                  columns={[
                    {
                      title: 'Date',
                      dataIndex: 'date',
                      key: 'date',
                      render: (date) => moment(date).format('MM/DD/YYYY'),
                    },
                    {
                      title: 'Transaction',
                      dataIndex: 'description',
                      key: 'description',
                    },
                    {
                      title: 'Taxable Amount',
                      dataIndex: 'taxableAmount',
                      key: 'taxableAmount',
                      render: (amount) => `$${amount.toFixed(2)}`,
                    },
                    {
                      title: 'Tax Amount',
                      dataIndex: 'taxAmount',
                      key: 'taxAmount',
                      render: (amount) => `$${amount.toFixed(2)}`,
                    },
                  ]}
                  rowKey="id"
                  pagination={{ pageSize: 5 }}
                  size="small"
                />
              </>
            )}
          </>
        )}
      </Drawer>
      
      {/* Filing Details Drawer */}
      <Drawer
        title="Tax Filing Details"
        placement="right"
        width={700}
        onClose={() => setFilingDetailsDrawerVisible(false)}
        visible={filingDetailsDrawerVisible}
        extra={
          currentFilingDetails && currentFilingDetails.status === 'pending' ? (
            <Button 
              type="primary" 
              onClick={() => currentFilingDetails && handleSubmitFiling(currentFilingDetails.id)}
            >
              Submit Filing
            </Button>
          ) : null
        }
      >
        {currentFilingDetails && (
          <>
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic 
                  title="Filing Type" 
                  value={
                    currentFilingDetails.type === 'sales-tax' ? 'Sales Tax' :
                    currentFilingDetails.type === 'vat' ? 'VAT' :
                    currentFilingDetails.type === 'income-tax' ? 'Income Tax' :
                    currentFilingDetails.type === 'payroll-tax' ? 'Payroll Tax' : 
                    currentFilingDetails.type
                  } 
                />
              </Col>
              <Col span={8}>
                <Statistic title="Authority" value={currentFilingDetails.authority} />
              </Col>
              <Col span={8}>
                <Statistic 
                  title="Status" 
                  value={currentFilingDetails.status.toUpperCase()}
                  valueStyle={{ 
                    color: 
                      currentFilingDetails.status === 'submitted' ? 'green' : 
                      currentFilingDetails.status === 'late' ? 'red' : 
                      'blue'
                  }}
                />
              </Col>
            </Row>
            
            <Row gutter={[16, 16]}>
              <Col span={8}>
                <Statistic title="Period" value={currentFilingDetails.period.name} />
              </Col>
              <Col span={8}>
                <Statistic 
                  title="Due Date" 
                  value={moment(currentFilingDetails.dueDate).format('MM/DD/YYYY')}
                  valueStyle={{ 
                    color: 
                      moment(currentFilingDetails.dueDate).isBefore(moment()) && 
                      currentFilingDetails.status !== 'submitted' ? 'red' : 'inherit'
                  }}
                />
              </Col>
              <Col span={8}>
                <Statistic title="Total Tax" value={`$${currentFilingDetails.totalTax.toFixed(2)}`} />
              </Col>
            </Row>
            
            {currentFilingDetails.status === 'submitted' && (
              <Row gutter={[16, 16]}>
                <Col span={8}>
                  <Statistic title="Submission Date" value={moment(currentFilingDetails.submittedDate).format('MM/DD/YYYY')} />
                </Col>
                <Col span={8}>
                  <Statistic title="Confirmation #" value={currentFilingDetails.confirmationNumber} />
                </Col>
                <Col span={8}>
                  <Statistic title="Payment Method" value={currentFilingDetails.paymentMethod} />
                </Col>
              </Row>
            )}
            
            <Divider />
            
            <Title level={5}>Form Information</Title>
            {currentFilingDetails.forms && (
              <List
                bordered
                dataSource={currentFilingDetails.forms}
                renderItem={item => (
                  <List.Item
                    actions={[
                      <Button
                        key="view" 
                        type="link"
                        onClick={() => console.log('View form', item)}
                      >
                        View
                      </Button>,
                      <Button
                        key="download" 
                        type="link"
                        icon={<DownloadOutlined />}
                        onClick={() => handleExportData('pdf', 'form', item.id)}
                      >
                        Download
                      </Button>
                    ]}
                  >
                    <List.Item.Meta
                      title={item.name}
                      description={item.description}
                    />
                  </List.Item>
                )}
              />
            )}
            
            <Divider />
            
            <Title level={5}>Filing Checklist</Title>
            {currentFilingDetails.checklist && (
              <List
                dataSource={currentFilingDetails.checklist}
                renderItem={item => (
                  <List.Item>
                    <List.Item.Meta
                      avatar={
                        item.completed ? 
                          <CheckCircleOutlined style={{ color: 'green', fontSize: '20px' }} /> :
                          <ClockCircleOutlined style={{ color: 'orange', fontSize: '20px' }} />
                      }
                      title={item.task}
                      description={item.description}
                    />
                    {item.completed ? 
                      <Tag color="green">Completed</Tag> : 
                      <Button 
                        size="small" 
                        type="primary"
                        disabled={currentFilingDetails.status !== 'pending'}
                      >
                        Complete
                      </Button>
                    }
                  </List.Item>
                )}
              />
            )}
            
            {currentFilingDetails.notes && (
              <>
                <Divider />
                <Title level={5}>Notes</Title>
                <Card>{currentFilingDetails.notes}</Card>
              </>
            )}
          </>
        )}
      </Drawer>
    </div>
  );
};

export default TaxManagementConsole;
