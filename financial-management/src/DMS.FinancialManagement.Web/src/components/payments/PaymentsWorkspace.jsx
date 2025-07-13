import React, { useState, useEffect } from 'react';
import {
  Table,
  Card,
  Button,
  Input,
  DatePicker,
  Tag,
  Space,
  Tabs,
  Modal,
  Form,
  Select,
  InputNumber,
  Spin,
  Alert,
  Tooltip,
  Typography,
  Row,
  Col,
  Drawer,
  Steps,
  Badge,
  Divider,
  Statistic,
} from 'antd';
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DollarOutlined,
  FileDoneOutlined,
  PrinterOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  BankOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import moment from 'moment';
import { financialService } from '../../services/financialService';

const { TabPane } = Tabs;
const { RangePicker } = DatePicker;
const { Option } = Select;
const { Title, Text } = Typography;
const { Step } = Steps;

const PaymentsWorkspace = () => {
  // State for payments
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedPayment, setSelectedPayment] = useState(null);
  const [paymentModalVisible, setPaymentModalVisible] = useState(false);
  const [paymentForm] = Form.useForm();
  const [currentTab, setCurrentTab] = useState('all-payments');
  const [searchText, setSearchText] = useState('');
  const [dateRange, setDateRange] = useState([]);
  const [statusFilter, setStatusFilter] = useState('all');
  
  // State for bank accounts and reconciliation
  const [bankAccounts, setBankAccounts] = useState([]);
  const [selectedAccount, setSelectedAccount] = useState(null);
  const [reconciliations, setReconciliations] = useState([]);
  const [reconciliationItems, setReconciliationItems] = useState([]);
  const [reconciliationModalVisible, setReconciliationModalVisible] = useState(false);
  const [reconciliationForm] = Form.useForm();
  const [currentReconciliation, setCurrentReconciliation] = useState(null);
  const [reconciliationStep, setReconciliationStep] = useState(0);
  
  // State for payment details drawer
  const [detailsDrawerVisible, setDetailsDrawerVisible] = useState(false);

  useEffect(() => {
    fetchPayments();
    fetchBankAccounts();
  }, []);

  // Fetch payments based on filters
  const fetchPayments = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        status: statusFilter !== 'all' ? statusFilter : null,
        fromDate: dateRange[0]?.format('YYYY-MM-DD'),
        toDate: dateRange[1]?.format('YYYY-MM-DD'),
      };
      
      const response = await financialService.getPayments(params);
      setPayments(response);
    } catch (err) {
      setError('Failed to load payments. Please try again.');
      console.error('Error fetching payments:', err);
    } finally {
      setLoading(false);
    }
  };

  // Fetch bank accounts
  const fetchBankAccounts = async () => {
    try {
      const accounts = await financialService.getBankAccounts();
      setBankAccounts(accounts);
    } catch (err) {
      console.error('Error fetching bank accounts:', err);
    }
  };

  // Fetch reconciliations for a specific bank account
  const fetchReconciliations = async (accountId) => {
    try {
      setLoading(true);
      const reconciliationData = await financialService.getBankReconciliations(accountId);
      setReconciliations(reconciliationData);
    } catch (err) {
      console.error('Error fetching reconciliations:', err);
    } finally {
      setLoading(false);
    }
  };

  // Handle tab change
  const handleTabChange = (key) => {
    setCurrentTab(key);
    
    if (key === 'bank-reconciliation' && bankAccounts.length === 0) {
      fetchBankAccounts();
    }
  };
  
  // Handle bank account selection
  const handleBankAccountSelect = (accountId) => {
    setSelectedAccount(accountId);
    fetchReconciliations(accountId);
  };

  // Create new payment
  const handleCreatePayment = () => {
    paymentForm.resetFields();
    setPaymentModalVisible(true);
  };

  // View payment details
  const handleViewPayment = (payment) => {
    setSelectedPayment(payment);
    setDetailsDrawerVisible(true);
  };

  // Handle void payment
  const handleVoidPayment = async (payment) => {
    try {
      setLoading(true);
      await financialService.voidPayment(payment.id);
      fetchPayments();
      setError(null);
    } catch (err) {
      setError('Failed to void payment. Please try again.');
      console.error('Error voiding payment:', err);
    } finally {
      setLoading(false);
    }
  };

  // Start new reconciliation
  const handleStartReconciliation = () => {
    reconciliationForm.resetFields();
    reconciliationForm.setFieldsValue({
      bankAccountId: selectedAccount,
      statementDate: moment(),
      beginningBalance: 0,
      endingBalance: 0,
    });
    setCurrentReconciliation(null);
    setReconciliationStep(0);
    setReconciliationModalVisible(true);
  };

  // Continue existing reconciliation
  const handleContinueReconciliation = (reconciliation) => {
    setCurrentReconciliation(reconciliation);
    setReconciliationStep(1); // Move to transaction matching step
    loadReconciliationItems(reconciliation.id);
    setReconciliationModalVisible(true);
  };

  // Load reconciliation items
  const loadReconciliationItems = async (reconciliationId) => {
    try {
      setLoading(true);
      // This would fetch unreconciled transactions and statement items
      // For now we'll just mock some data
      setReconciliationItems([
        {
          id: '1',
          type: 'payment',
          date: '2025-06-01',
          description: 'Payment to Vendor A',
          amount: -1200,
          cleared: false,
        },
        {
          id: '2',
          type: 'deposit',
          date: '2025-06-05',
          description: 'Customer payment',
          amount: 3500,
          cleared: false,
        },
        {
          id: '3',
          type: 'check',
          date: '2025-06-10',
          description: 'Check #1234',
          amount: -750,
          cleared: false,
        },
      ]);
    } catch (err) {
      console.error('Error loading reconciliation items:', err);
    } finally {
      setLoading(false);
    }
  };

  // Submit payment form
  const handlePaymentFormSubmit = async (values) => {
    try {
      setLoading(true);
      
      const paymentData = {
        ...values,
        paymentDate: values.paymentDate.format('YYYY-MM-DD'),
      };
      
      await financialService.createPayment(paymentData);
      setPaymentModalVisible(false);
      fetchPayments();
      setError(null);
    } catch (err) {
      setError('Failed to create payment. Please try again.');
      console.error('Error creating payment:', err);
    } finally {
      setLoading(false);
    }
  };

  // Start reconciliation process
  const handleReconciliationStart = async (values) => {
    try {
      setLoading(true);
      
      const reconciliationData = {
        bankAccountId: values.bankAccountId,
        statementDate: values.statementDate.format('YYYY-MM-DD'),
        beginningBalance: values.beginningBalance,
        endingBalance: values.endingBalance,
        status: 'in_progress',
      };
      
      const response = await financialService.createBankReconciliation(
        values.bankAccountId, 
        reconciliationData
      );
      
      setCurrentReconciliation(response);
      setReconciliationStep(1);
      loadReconciliationItems(response.id);
    } catch (err) {
      console.error('Error starting reconciliation:', err);
    } finally {
      setLoading(false);
    }
  };

  // Complete reconciliation
  const handleCompleteReconciliation = async () => {
    try {
      setLoading(true);
      
      // Update reconciliation status to completed
      await financialService.updateBankReconciliation(
        currentReconciliation.bankAccountId,
        currentReconciliation.id,
        { ...currentReconciliation, status: 'completed' }
      );
      
      setReconciliationModalVisible(false);
      fetchReconciliations(selectedAccount);
    } catch (err) {
      console.error('Error completing reconciliation:', err);
    } finally {
      setLoading(false);
    }
  };

  // Toggle cleared status of an item
  const handleToggleClearedStatus = (itemId) => {
    setReconciliationItems(items => 
      items.map(item => 
        item.id === itemId ? { ...item, cleared: !item.cleared } : item
      )
    );
  };

  // Payment columns for table
  const paymentColumns = [
    {
      title: 'Payment #',
      dataIndex: 'paymentNumber',
      key: 'paymentNumber',
      sorter: (a, b) => a.paymentNumber.localeCompare(b.paymentNumber),
    },
    {
      title: 'Type',
      dataIndex: 'entityType',
      key: 'entityType',
      render: (type) => type === 'customer' ? 'Customer Payment' : 'Vendor Payment',
    },
    {
      title: 'Entity',
      dataIndex: 'entityName',
      key: 'entityName',
      sorter: (a, b) => a.entityName.localeCompare(b.entityName),
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount) => `$${Math.abs(amount).toFixed(2)}`,
      sorter: (a, b) => a.amount - b.amount,
    },
    {
      title: 'Payment Date',
      dataIndex: 'paymentDate',
      key: 'paymentDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.paymentDate).unix() - moment(b.paymentDate).unix(),
    },
    {
      title: 'Method',
      dataIndex: 'paymentMethod',
      key: 'paymentMethod',
      filters: [
        { text: 'Check', value: 'check' },
        { text: 'Credit Card', value: 'credit_card' },
        { text: 'Bank Transfer', value: 'bank_transfer' },
        { text: 'Cash', value: 'cash' },
      ],
      onFilter: (value, record) => record.paymentMethod === value,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        let icon = null;
        
        switch(status.toLowerCase()) {
          case 'completed':
            color = 'green';
            icon = <CheckCircleOutlined />;
            break;
          case 'pending':
            color = 'blue';
            icon = <ClockCircleOutlined />;
            break;
          case 'void':
            color = 'red';
            icon = <CloseCircleOutlined />;
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
      filters: [
        { text: 'Completed', value: 'completed' },
        { text: 'Pending', value: 'pending' },
        { text: 'Void', value: 'void' },
      ],
      onFilter: (value, record) => record.status.toLowerCase() === value,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="View Details">
            <Button 
              icon={<FileDoneOutlined />} 
              onClick={() => handleViewPayment(record)}
              type="text"
            />
          </Tooltip>
          <Tooltip title="Print">
            <Button 
              icon={<PrinterOutlined />} 
              onClick={() => console.log('Print payment', record)}
              type="text"
            />
          </Tooltip>
          {record.status !== 'void' && (
            <Tooltip title="Void Payment">
              <Button 
                icon={<CloseCircleOutlined />} 
                onClick={() => handleVoidPayment(record)}
                type="text"
                danger
              />
            </Tooltip>
          )}
        </Space>
      ),
    },
  ];

  // Bank account columns
  const bankAccountColumns = [
    {
      title: 'Account Name',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Account Number',
      dataIndex: 'accountNumber',
      key: 'accountNumber',
      render: (accountNumber) => `xxxx-xxxx-${accountNumber.slice(-4)}`,
    },
    {
      title: 'Bank',
      dataIndex: 'bankName',
      key: 'bankName',
    },
    {
      title: 'Current Balance',
      dataIndex: 'currentBalance',
      key: 'currentBalance',
      render: (amount) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Last Reconciled',
      dataIndex: 'lastReconciled',
      key: 'lastReconciled',
      render: (date) => date ? moment(date).format('MM/DD/YYYY') : 'Never',
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Button 
          type="primary"
          onClick={() => handleBankAccountSelect(record.id)}
        >
          Select
        </Button>
      ),
    },
  ];

  // Reconciliation columns
  const reconciliationColumns = [
    {
      title: 'Statement Date',
      dataIndex: 'statementDate',
      key: 'statementDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.statementDate).unix() - moment(b.statementDate).unix(),
    },
    {
      title: 'Beginning Balance',
      dataIndex: 'beginningBalance',
      key: 'beginningBalance',
      render: (amount) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Ending Balance',
      dataIndex: 'endingBalance',
      key: 'endingBalance',
      render: (amount) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        
        switch(status) {
          case 'completed':
            color = 'green';
            break;
          case 'in_progress':
            color = 'blue';
            break;
          default:
            color = 'default';
        }
        
        return <Tag color={color}>{status.toUpperCase()}</Tag>;
      },
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        record.status === 'in_progress' ? (
          <Button 
            type="primary"
            onClick={() => handleContinueReconciliation(record)}
          >
            Continue
          </Button>
        ) : (
          <Button 
            onClick={() => console.log('View reconciliation', record)}
          >
            View
          </Button>
        )
      ),
    },
  ];

  // Reconciliation item columns
  const reconciliationItemColumns = [
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
      title: 'Type',
      dataIndex: 'type',
      key: 'type',
      render: (type) => type.charAt(0).toUpperCase() + type.slice(1),
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount) => amount < 0 
        ? `-$${Math.abs(amount).toFixed(2)}` 
        : `$${amount.toFixed(2)}`,
    },
    {
      title: 'Cleared',
      dataIndex: 'cleared',
      key: 'cleared',
      render: (cleared) => cleared 
        ? <CheckCircleOutlined style={{ color: 'green' }} /> 
        : <ClockCircleOutlined style={{ color: 'orange' }} />,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Button
          type={record.cleared ? 'default' : 'primary'}
          onClick={() => handleToggleClearedStatus(record.id)}
        >
          {record.cleared ? 'Unmark' : 'Mark Cleared'}
        </Button>
      ),
    },
  ];

  // Filter payments by search text
  const filteredPayments = payments.filter(payment => {
    const searchLower = searchText.toLowerCase();
    return (
      payment.paymentNumber?.toLowerCase().includes(searchLower) ||
      payment.entityName?.toLowerCase().includes(searchLower) ||
      payment.reference?.toLowerCase().includes(searchLower)
    );
  });

  // Calculate reconciliation statistics
  const calculateReconciliationStats = () => {
    const clearedItems = reconciliationItems.filter(item => item.cleared);
    const unclearedItems = reconciliationItems.filter(item => !item.cleared);
    
    const clearedTotal = clearedItems.reduce((sum, item) => sum + item.amount, 0);
    const unclearedTotal = unclearedItems.reduce((sum, item) => sum + item.amount, 0);
    
    const expectedBalance = currentReconciliation?.beginningBalance + clearedTotal;
    const difference = currentReconciliation ? expectedBalance - currentReconciliation.endingBalance : 0;
    
    return {
      clearedCount: clearedItems.length,
      clearedTotal,
      unclearedCount: unclearedItems.length,
      unclearedTotal,
      expectedBalance,
      difference,
    };
  };

  return (
    <div className="payments-workspace">
      <Title level={2}>Payments Management</Title>
      
      <Tabs activeKey={currentTab} onChange={handleTabChange}>
        <TabPane tab="All Payments" key="all-payments">
          <Card>
            <div style={{ marginBottom: 16 }}>
              <Row gutter={16} align="middle">
                <Col xs={24} sm={8} md={6} lg={4}>
                  <Input
                    placeholder="Search payments..."
                    prefix={<SearchOutlined />}
                    value={searchText}
                    onChange={e => setSearchText(e.target.value)}
                  />
                </Col>
                <Col xs={24} sm={8} md={7} lg={5}>
                  <RangePicker
                    value={dateRange}
                    onChange={setDateRange}
                    style={{ width: '100%' }}
                  />
                </Col>
                <Col xs={24} sm={8} md={5} lg={4}>
                  <Select
                    style={{ width: '100%' }}
                    placeholder="Status"
                    value={statusFilter}
                    onChange={setStatusFilter}
                  >
                    <Option value="all">All Status</Option>
                    <Option value="completed">Completed</Option>
                    <Option value="pending">Pending</Option>
                    <Option value="void">Void</Option>
                  </Select>
                </Col>
                <Col xs={24} sm={8} md={6} lg={5}>
                  <Button
                    type="primary"
                    icon={<SearchOutlined />}
                    onClick={fetchPayments}
                    style={{ marginRight: 8 }}
                  >
                    Search
                  </Button>
                  <Button onClick={() => {
                    setSearchText('');
                    setDateRange([]);
                    setStatusFilter('all');
                  }}>
                    Reset
                  </Button>
                </Col>
                <Col xs={24} sm={8} md={24} lg={6} style={{ textAlign: 'right' }}>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleCreatePayment}
                  >
                    New Payment
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
              columns={paymentColumns}
              dataSource={filteredPayments}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
        
        <TabPane tab="Bank Reconciliation" key="bank-reconciliation">
          <Card>
            {!selectedAccount ? (
              <>
                <Title level={4}>Select a Bank Account</Title>
                <Table
                  columns={bankAccountColumns}
                  dataSource={bankAccounts}
                  rowKey="id"
                  loading={loading}
                  pagination={{ pageSize: 5 }}
                />
              </>
            ) : (
              <>
                <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between' }}>
                  <Title level={4}>
                    {bankAccounts.find(a => a.id === selectedAccount)?.name} - Reconciliation History
                  </Title>
                  <Space>
                    <Button onClick={() => setSelectedAccount(null)}>
                      Back to Accounts
                    </Button>
                    <Button
                      type="primary"
                      icon={<PlusOutlined />}
                      onClick={handleStartReconciliation}
                    >
                      New Reconciliation
                    </Button>
                  </Space>
                </div>
                
                <Table
                  columns={reconciliationColumns}
                  dataSource={reconciliations}
                  rowKey="id"
                  loading={loading}
                  pagination={{ pageSize: 10 }}
                />
              </>
            )}
          </Card>
        </TabPane>
      </Tabs>
      
      {/* Payment Creation Modal */}
      <Modal
        title="Create New Payment"
        visible={paymentModalVisible}
        onCancel={() => setPaymentModalVisible(false)}
        footer={null}
        width={800}
      >
        <Form
          form={paymentForm}
          layout="vertical"
          onFinish={handlePaymentFormSubmit}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="entityType"
                label="Payment For"
                rules={[{ required: true, message: 'Please select entity type' }]}
              >
                <Select placeholder="Select entity type">
                  <Option value="customer">Customer</Option>
                  <Option value="vendor">Vendor</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="entityId"
                label="Entity"
                rules={[{ required: true, message: 'Please select entity' }]}
              >
                <Select
                  placeholder="Select entity"
                  showSearch
                  filterOption={(input, option) =>
                    option.children.toLowerCase().indexOf(input.toLowerCase()) >= 0
                  }
                >
                  {/* This would be populated based on the entityType selection */}
                  <Option value="entity1">Entity 1</Option>
                  <Option value="entity2">Entity 2</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="amount"
                label="Payment Amount"
                rules={[{ required: true, message: 'Please enter payment amount' }]}
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
            <Col span={12}>
              <Form.Item
                name="paymentDate"
                label="Payment Date"
                rules={[{ required: true, message: 'Please select payment date' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="paymentMethod"
                label="Payment Method"
                rules={[{ required: true, message: 'Please select payment method' }]}
              >
                <Select placeholder="Select payment method">
                  <Option value="check">Check</Option>
                  <Option value="credit_card">Credit Card</Option>
                  <Option value="bank_transfer">Bank Transfer</Option>
                  <Option value="cash">Cash</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="reference"
                label="Reference / Check Number"
              >
                <Input placeholder="Enter reference or check number" />
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item
            name="notes"
            label="Notes"
          >
            <Input.TextArea rows={4} placeholder="Enter payment notes" />
          </Form.Item>
          
          <div style={{ textAlign: 'right' }}>
            <Button
              style={{ marginRight: 8 }}
              onClick={() => setPaymentModalVisible(false)}
            >
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              Create Payment
            </Button>
          </div>
        </Form>
      </Modal>
      
      {/* Bank Reconciliation Modal */}
      <Modal
        title="Bank Reconciliation"
        visible={reconciliationModalVisible}
        onCancel={() => setReconciliationModalVisible(false)}
        footer={null}
        width={900}
      >
        <Steps current={reconciliationStep} style={{ marginBottom: 24 }}>
          <Step title="Setup" description="Enter statement details" />
          <Step title="Match" description="Review transactions" />
          <Step title="Finish" description="Complete reconciliation" />
        </Steps>
        
        {reconciliationStep === 0 && (
          <Form
            form={reconciliationForm}
            layout="vertical"
            onFinish={handleReconciliationStart}
          >
            <Form.Item
              name="bankAccountId"
              hidden
            >
              <Input />
            </Form.Item>
            
            <Form.Item
              name="statementDate"
              label="Statement Date"
              rules={[{ required: true, message: 'Please select statement date' }]}
            >
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
            
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="beginningBalance"
                  label="Beginning Balance"
                  rules={[{ required: true, message: 'Please enter beginning balance' }]}
                >
                  <InputNumber
                    style={{ width: '100%' }}
                    formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                    parser={value => value.replace(/\$\s?|(,*)/g, '')}
                    precision={2}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="endingBalance"
                  label="Ending Balance"
                  rules={[{ required: true, message: 'Please enter ending balance' }]}
                >
                  <InputNumber
                    style={{ width: '100%' }}
                    formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                    parser={value => value.replace(/\$\s?|(,*)/g, '')}
                    precision={2}
                  />
                </Form.Item>
              </Col>
            </Row>
            
            <div style={{ textAlign: 'right' }}>
              <Button
                style={{ marginRight: 8 }}
                onClick={() => setReconciliationModalVisible(false)}
              >
                Cancel
              </Button>
              <Button type="primary" htmlType="submit" loading={loading}>
                Start Reconciliation
              </Button>
            </div>
          </Form>
        )}
        
        {reconciliationStep === 1 && (
          <>
            <div style={{ marginBottom: 16 }}>
              <Title level={5}>Mark transactions that appear on your bank statement</Title>
              <Text>Mark each transaction that matches your bank statement as "cleared".</Text>
            </div>
            
            <Table
              columns={reconciliationItemColumns}
              dataSource={reconciliationItems}
              rowKey="id"
              pagination={false}
              size="small"
            />
            
            <div style={{ textAlign: 'right', marginTop: 16 }}>
              <Button
                style={{ marginRight: 8 }}
                onClick={() => setReconciliationStep(0)}
              >
                Back
              </Button>
              <Button
                type="primary"
                onClick={() => setReconciliationStep(2)}
              >
                Continue
              </Button>
            </div>
          </>
        )}
        
        {reconciliationStep === 2 && (
          <>
            <div style={{ marginBottom: 16 }}>
              <Title level={5}>Finish Reconciliation</Title>
              <Text>Review the summary and complete the reconciliation.</Text>
            </div>
            
            {currentReconciliation && (
              <>
                <Row gutter={16}>
                  <Col span={8}>
                    <Card>
                      <Statistic
                        title="Statement Beginning Balance"
                        value={currentReconciliation.beginningBalance}
                        precision={2}
                        prefix="$"
                      />
                    </Card>
                  </Col>
                  <Col span={8}>
                    <Card>
                      <Statistic
                        title="Cleared Transactions"
                        value={calculateReconciliationStats().clearedCount}
                        suffix={` items ($${Math.abs(calculateReconciliationStats().clearedTotal).toFixed(2)})`}
                      />
                    </Card>
                  </Col>
                  <Col span={8}>
                    <Card>
                      <Statistic
                        title="Calculated Ending Balance"
                        value={calculateReconciliationStats().expectedBalance}
                        precision={2}
                        prefix="$"
                      />
                    </Card>
                  </Col>
                </Row>
                
                <Divider />
                
                <Row gutter={16}>
                  <Col span={8}>
                    <Card>
                      <Statistic
                        title="Statement Ending Balance"
                        value={currentReconciliation.endingBalance}
                        precision={2}
                        prefix="$"
                      />
                    </Card>
                  </Col>
                  <Col span={8}>
                    <Card>
                      <Statistic
                        title="Uncleared Transactions"
                        value={calculateReconciliationStats().unclearedCount}
                        suffix={` items ($${Math.abs(calculateReconciliationStats().unclearedTotal).toFixed(2)})`}
                      />
                    </Card>
                  </Col>
                  <Col span={8}>
                    <Card>
                      <Statistic
                        title="Difference"
                        value={Math.abs(calculateReconciliationStats().difference)}
                        precision={2}
                        prefix="$"
                        valueStyle={{ 
                          color: calculateReconciliationStats().difference === 0 ? 'green' : 'red' 
                        }}
                        suffix={calculateReconciliationStats().difference === 0 ? 
                          <CheckCircleOutlined /> : <ExclamationCircleOutlined />}
                      />
                    </Card>
                  </Col>
                </Row>
              </>
            )}
            
            <div style={{ textAlign: 'right', marginTop: 24 }}>
              <Button
                style={{ marginRight: 8 }}
                onClick={() => setReconciliationStep(1)}
              >
                Back
              </Button>
              <Button
                type="primary"
                onClick={handleCompleteReconciliation}
                disabled={calculateReconciliationStats().difference !== 0}
                loading={loading}
              >
                Complete Reconciliation
              </Button>
            </div>
          </>
        )}
      </Modal>
      
      {/* Payment Details Drawer */}
      <Drawer
        title={`Payment Details: ${selectedPayment?.paymentNumber || ''}`}
        placement="right"
        onClose={() => setDetailsDrawerVisible(false)}
        visible={detailsDrawerVisible}
        width={600}
      >
        {selectedPayment && (
          <>
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Statistic
                  title="Amount"
                  value={Math.abs(selectedPayment.amount)}
                  precision={2}
                  prefix="$"
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="Status"
                  value={selectedPayment.status.toUpperCase()}
                  valueStyle={{ 
                    color: selectedPayment.status === 'completed' ? 'green' : 
                           selectedPayment.status === 'void' ? 'red' : 'blue' 
                  }}
                />
              </Col>
              
              <Col span={12}>
                <Statistic
                  title="Payment Date"
                  value={moment(selectedPayment.paymentDate).format('MM/DD/YYYY')}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="Payment Method"
                  value={selectedPayment.paymentMethod}
                />
              </Col>
              
              <Col span={24}>
                <Card title="Payment Details" size="small">
                  <p><strong>Entity Type:</strong> {selectedPayment.entityType}</p>
                  <p><strong>Entity:</strong> {selectedPayment.entityName}</p>
                  <p><strong>Reference:</strong> {selectedPayment.reference || 'N/A'}</p>
                  <p><strong>Notes:</strong> {selectedPayment.notes || 'N/A'}</p>
                </Card>
              </Col>
              
              {selectedPayment.relatedDocuments && (
                <Col span={24}>
                  <Card title="Related Documents" size="small">
                    <Table
                      columns={[
                        {
                          title: 'Type',
                          dataIndex: 'type',
                          key: 'type',
                        },
                        {
                          title: 'Number',
                          dataIndex: 'number',
                          key: 'number',
                        },
                        {
                          title: 'Amount',
                          dataIndex: 'amount',
                          key: 'amount',
                          render: (amount) => `$${amount.toFixed(2)}`
                        },
                        {
                          title: 'Action',
                          key: 'action',
                          render: (_, record) => (
                            <Button
                              type="link"
                              onClick={() => console.log('View document', record)}
                            >
                              View
                            </Button>
                          )
                        }
                      ]}
                      dataSource={selectedPayment.relatedDocuments}
                      rowKey="id"
                      pagination={false}
                      size="small"
                    />
                  </Card>
                </Col>
              )}
            </Row>
            
            <div style={{ marginTop: 24, textAlign: 'right' }}>
              <Space>
                <Button onClick={() => setDetailsDrawerVisible(false)}>
                  Close
                </Button>
                <Button 
                  type="primary" 
                  icon={<PrinterOutlined />}
                  onClick={() => console.log('Print payment', selectedPayment)}
                >
                  Print Receipt
                </Button>
              </Space>
            </div>
          </>
        )}
      </Drawer>
    </div>
  );
};

export default PaymentsWorkspace;
