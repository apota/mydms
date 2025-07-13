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
  Badge,
} from 'antd';
import {
  SearchOutlined,
  PlusOutlined,
  EditOutlined,
  DollarOutlined,
  FileDoneOutlined,
  PrinterOutlined,
  ExclamationCircleOutlined,
  ClockCircleOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
} from '@ant-design/icons';
import moment from 'moment';
import { financialService } from '../../services/financialService';

const { TabPane } = Tabs;
const { RangePicker } = DatePicker;
const { Option } = Select;
const { Title } = Typography;

const AccountsPayableWorkspace = () => {
  // State management for bills
  const [bills, setBills] = useState([]);
  const [vendors, setVendors] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedBill, setSelectedBill] = useState(null);
  const [billModalVisible, setBillModalVisible] = useState(false);
  const [billForm] = Form.useForm();
  const [currentTab, setCurrentTab] = useState('bills');
  const [searchText, setSearchText] = useState('');
  const [dateRange, setDateRange] = useState([]);
  const [statusFilter, setStatusFilter] = useState('all');
  
  // State for payments
  const [payments, setPayments] = useState([]);
  const [selectedPayment, setSelectedPayment] = useState(null);
  const [paymentModalVisible, setPaymentModalVisible] = useState(false);
  const [paymentForm] = Form.useForm();

  // State for vendor aging
  const [vendorAging, setVendorAging] = useState([]);
  const [vendorAgingLoading, setVendorAgingLoading] = useState(false);
  
  // State for purchase orders
  const [purchaseOrders, setPurchaseOrders] = useState([]);
  const [selectedPO, setSelectedPO] = useState(null);

  // Load initial data
  useEffect(() => {
    fetchBills();
    fetchVendors();
  }, []);

  // Fetch bills based on filters
  const fetchBills = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const params = {
        status: statusFilter !== 'all' ? statusFilter : null,
        fromDate: dateRange[0]?.format('YYYY-MM-DD'),
        toDate: dateRange[1]?.format('YYYY-MM-DD'),
      };
      
      const response = await financialService.getBills(params);
      setBills(response);
    } catch (err) {
      setError('Failed to load bills. Please try again.');
      console.error('Error fetching bills:', err);
    } finally {
      setLoading(false);
    }
  };

  // Fetch vendors
  const fetchVendors = async () => {
    try {
      // Assuming there's a method to get vendors
      // This would need to be added to the financialService or sourced from another service
      const response = await axios.get('/api/vendors');
      setVendors(response.data);
    } catch (err) {
      console.error('Error fetching vendors:', err);
    }
  };

  // Fetch vendor aging report
  const fetchVendorAging = async () => {
    try {
      setVendorAgingLoading(true);
      const response = await financialService.getAccountsPayableAging();
      setVendorAging(response);
    } catch (err) {
      console.error('Error fetching vendor aging:', err);
    } finally {
      setVendorAgingLoading(false);
    }
  };

  // Handle tab change
  const handleTabChange = (key) => {
    setCurrentTab(key);
    
    if (key === 'aging' && vendorAging.length === 0) {
      fetchVendorAging();
    } else if (key === 'purchase-orders' && purchaseOrders.length === 0) {
      fetchPurchaseOrders();
    } else if (key === 'payments' && payments.length === 0) {
      fetchPayments();
    }
  };

  // Fetch purchase orders
  const fetchPurchaseOrders = async () => {
    try {
      setLoading(true);
      // Assuming a service method to get POs
      const response = await axios.get('/api/purchase-orders');
      setPurchaseOrders(response.data);
    } catch (err) {
      console.error('Error fetching purchase orders:', err);
    } finally {
      setLoading(false);
    }
  };

  // Fetch payments
  const fetchPayments = async () => {
    try {
      setLoading(true);
      const response = await financialService.getPayments({ entityType: 'vendor' });
      setPayments(response);
    } catch (err) {
      console.error('Error fetching payments:', err);
    } finally {
      setLoading(false);
    }
  };

  // Create new bill
  const handleCreateBill = () => {
    billForm.resetFields();
    setSelectedBill(null);
    setBillModalVisible(true);
  };

  // Edit bill
  const handleEditBill = (bill) => {
    setSelectedBill(bill);
    billForm.setFieldsValue({
      vendorId: bill.vendorId,
      billNumber: bill.billNumber,
      amount: bill.amount,
      description: bill.description,
      billDate: moment(bill.billDate),
      dueDate: moment(bill.dueDate),
      status: bill.status,
      // Set other fields as needed
    });
    setBillModalVisible(true);
  };

  // Create payment for bill
  const handleCreatePayment = (bill) => {
    paymentForm.resetFields();
    paymentForm.setFieldsValue({
      entityId: bill.id,
      entityType: 'bill',
      amount: bill.amountDue,
      paymentDate: moment(),
      paymentMethod: 'check',
    });
    setSelectedBill(bill);
    setPaymentModalVisible(true);
  };

  // Submit bill form
  const handleBillFormSubmit = async (values) => {
    try {
      setLoading(true);

      const billData = {
        ...values,
        billDate: values.billDate.format('YYYY-MM-DD'),
        dueDate: values.dueDate.format('YYYY-MM-DD'),
      };

      if (selectedBill) {
        await financialService.updateBill(selectedBill.id, billData);
      } else {
        await financialService.createBill(billData);
      }

      setBillModalVisible(false);
      fetchBills();
      
    } catch (err) {
      console.error('Error saving bill:', err);
      setError('Failed to save bill. Please try again.');
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
      fetchBills(); // Refresh bills to update status
      
      if (currentTab === 'payments') {
        fetchPayments(); // Refresh payments if on payments tab
      }
      
    } catch (err) {
      console.error('Error creating payment:', err);
      setError('Failed to create payment. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  // Bill table columns
  const billColumns = [
    {
      title: 'Bill #',
      dataIndex: 'billNumber',
      key: 'billNumber',
      sorter: (a, b) => a.billNumber.localeCompare(b.billNumber),
    },
    {
      title: 'Vendor',
      dataIndex: 'vendorName',
      key: 'vendorName',
      sorter: (a, b) => a.vendorName.localeCompare(b.vendorName),
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.amount - b.amount,
    },
    {
      title: 'Amount Due',
      dataIndex: 'amountDue',
      key: 'amountDue',
      render: (amountDue) => `$${amountDue.toFixed(2)}`,
      sorter: (a, b) => a.amountDue - b.amountDue,
    },
    {
      title: 'Bill Date',
      dataIndex: 'billDate',
      key: 'billDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.billDate).unix() - moment(b.billDate).unix(),
    },
    {
      title: 'Due Date',
      dataIndex: 'dueDate',
      key: 'dueDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.dueDate).unix() - moment(b.dueDate).unix(),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        let icon = null;
        
        switch(status.toLowerCase()) {
          case 'open':
            color = 'blue';
            icon = <ClockCircleOutlined />;
            break;
          case 'paid':
            color = 'green';
            icon = <CheckCircleOutlined />;
            break;
          case 'overdue':
            color = 'red';
            icon = <ExclamationCircleOutlined />;
            break;
          case 'void':
            color = 'gray';
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
        { text: 'Open', value: 'open' },
        { text: 'Paid', value: 'paid' },
        { text: 'Overdue', value: 'overdue' },
        { text: 'Void', value: 'void' },
      ],
      onFilter: (value, record) => record.status.toLowerCase() === value,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Edit Bill">
            <Button 
              icon={<EditOutlined />} 
              onClick={() => handleEditBill(record)} 
              type="text" 
            />
          </Tooltip>
          <Tooltip title="Create Payment">
            <Button 
              icon={<DollarOutlined />} 
              onClick={() => handleCreatePayment(record)} 
              type="text" 
              disabled={record.status === 'paid' || record.status === 'void'} 
            />
          </Tooltip>
          <Tooltip title="View Details">
            <Button 
              icon={<FileDoneOutlined />} 
              onClick={() => handleViewBillDetails(record)} 
              type="text" 
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  // Payment table columns
  const paymentColumns = [
    {
      title: 'Payment #',
      dataIndex: 'paymentNumber',
      key: 'paymentNumber',
    },
    {
      title: 'Vendor',
      dataIndex: 'entityName',
      key: 'entityName',
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Payment Date',
      dataIndex: 'paymentDate',
      key: 'paymentDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
    },
    {
      title: 'Payment Method',
      dataIndex: 'paymentMethod',
      key: 'paymentMethod',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        
        switch(status.toLowerCase()) {
          case 'completed':
            color = 'green';
            break;
          case 'pending':
            color = 'blue';
            break;
          case 'void':
            color = 'red';
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
        <Space size="small">
          <Tooltip title="View Details">
            <Button 
              icon={<FileDoneOutlined />} 
              onClick={() => handleViewPaymentDetails(record)} 
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
          <Tooltip title="Print Receipt">
            <Button 
              icon={<PrinterOutlined />} 
              onClick={() => handlePrintPaymentReceipt(record)} 
              type="text" 
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  // Aging table columns
  const agingColumns = [
    {
      title: 'Vendor',
      dataIndex: 'vendorName',
      key: 'vendorName',
      sorter: (a, b) => a.vendorName.localeCompare(b.vendorName),
    },
    {
      title: 'Current',
      dataIndex: 'current',
      key: 'current',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.current - b.current,
    },
    {
      title: '1-30 Days',
      dataIndex: 'thirtyDays',
      key: 'thirtyDays',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.thirtyDays - b.thirtyDays,
    },
    {
      title: '31-60 Days',
      dataIndex: 'sixtyDays',
      key: 'sixtyDays',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.sixtyDays - b.sixtyDays,
    },
    {
      title: '61-90 Days',
      dataIndex: 'ninetyDays',
      key: 'ninetyDays',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.ninetyDays - b.ninetyDays,
    },
    {
      title: '90+ Days',
      dataIndex: 'ninetyPlusDays',
      key: 'ninetyPlusDays',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.ninetyPlusDays - b.ninetyPlusDays,
    },
    {
      title: 'Total',
      dataIndex: 'total',
      key: 'total',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.total - b.total,
    },
  ];

  // PO table columns
  const purchaseOrderColumns = [
    {
      title: 'PO #',
      dataIndex: 'poNumber',
      key: 'poNumber',
    },
    {
      title: 'Vendor',
      dataIndex: 'vendorName',
      key: 'vendorName',
    },
    {
      title: 'Amount',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount) => `$${amount.toFixed(2)}`,
    },
    {
      title: 'Date Created',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date) => moment(date).format('MM/DD/YYYY'),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => {
        let color = 'default';
        
        switch(status.toLowerCase()) {
          case 'open':
            color = 'blue';
            break;
          case 'received':
            color = 'green';
            break;
          case 'billed':
            color = 'purple';
            break;
          case 'cancelled':
            color = 'red';
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
        <Space size="small">
          <Button 
            icon={<FileDoneOutlined />} 
            onClick={() => handleViewPODetails(record)} 
            type="text" 
          />
          {record.status === 'received' && !record.billCreated && (
            <Button 
              icon={<DollarOutlined />} 
              onClick={() => handleCreateBillFromPO(record)} 
              type="text" 
            >
              Create Bill
            </Button>
          )}
        </Space>
      ),
    },
  ];

  // Handle view bill details
  const handleViewBillDetails = (bill) => {
    setSelectedBill(bill);
    // Implement details view, perhaps a drawer or modal
  };

  // Handle view payment details
  const handleViewPaymentDetails = (payment) => {
    setSelectedPayment(payment);
    // Implement details view
  };

  // Handle void payment
  const handleVoidPayment = async (payment) => {
    try {
      await financialService.voidPayment(payment.id);
      fetchPayments();
      if (currentTab === 'bills') {
        fetchBills(); // Refresh bills to update status
      }
    } catch (err) {
      console.error('Error voiding payment:', err);
      setError('Failed to void payment. Please try again.');
    }
  };

  // Handle print payment receipt
  const handlePrintPaymentReceipt = (payment) => {
    // Implement print functionality
    console.log('Print receipt for payment:', payment);
  };

  // Handle view PO details
  const handleViewPODetails = (po) => {
    setSelectedPO(po);
    // Implement details view
  };

  // Handle creating a bill from PO
  const handleCreateBillFromPO = (po) => {
    billForm.resetFields();
    billForm.setFieldsValue({
      vendorId: po.vendorId,
      poNumber: po.poNumber,
      amount: po.amount,
      description: `Bill for PO #${po.poNumber}`,
      billDate: moment(),
      dueDate: moment().add(30, 'days'),
      status: 'open',
      poId: po.id,
    });
    setBillModalVisible(true);
  };

  // Filter bills by search text
  const filteredBills = bills.filter(bill => {
    const searchLower = searchText.toLowerCase();
    return (
      bill.billNumber.toLowerCase().includes(searchLower) ||
      bill.vendorName.toLowerCase().includes(searchLower) ||
      bill.description.toLowerCase().includes(searchLower)
    );
  });

  return (
    <div className="accounts-payable-workspace">
      <Title level={2}>Accounts Payable</Title>
      
      <Tabs activeKey={currentTab} onChange={handleTabChange}>
        <TabPane tab="Bills" key="bills">
          <Card>
            <div style={{ marginBottom: 16 }}>
              <Row gutter={16} align="middle">
                <Col xs={24} sm={8} md={6} lg={5}>
                  <Input
                    placeholder="Search bills..."
                    prefix={<SearchOutlined />}
                    value={searchText}
                    onChange={e => setSearchText(e.target.value)}
                  />
                </Col>
                <Col xs={24} sm={8} md={7} lg={6}>
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
                    <Option value="open">Open</Option>
                    <Option value="paid">Paid</Option>
                    <Option value="overdue">Overdue</Option>
                    <Option value="void">Void</Option>
                  </Select>
                </Col>
                <Col xs={24} sm={8} md={6} lg={5}>
                  <Button
                    type="primary"
                    icon={<SearchOutlined />}
                    onClick={fetchBills}
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
                <Col xs={24} sm={8} md={24} lg={4} style={{ textAlign: 'right' }}>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleCreateBill}
                  >
                    New Bill
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
              columns={billColumns}
              dataSource={filteredBills}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
        
        <TabPane tab="Payments" key="payments">
          <Card>
            <Table
              columns={paymentColumns}
              dataSource={payments}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
        
        <TabPane tab="Vendor Aging" key="aging">
          <Card>
            {vendorAgingLoading ? (
              <div style={{ textAlign: 'center', padding: '20px' }}>
                <Spin size="large" />
              </div>
            ) : (
              <Table
                columns={agingColumns}
                dataSource={vendorAging}
                rowKey="vendorId"
                pagination={{ pageSize: 10 }}
              />
            )}
          </Card>
        </TabPane>
        
        <TabPane tab="Purchase Orders" key="purchase-orders">
          <Card>
            <Table
              columns={purchaseOrderColumns}
              dataSource={purchaseOrders}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
      </Tabs>
      
      {/* Bill Modal */}
      <Modal
        title={selectedBill ? "Edit Bill" : "Create New Bill"}
        visible={billModalVisible}
        onCancel={() => setBillModalVisible(false)}
        footer={null}
        width={800}
      >
        <Form
          form={billForm}
          layout="vertical"
          onFinish={handleBillFormSubmit}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="vendorId"
                label="Vendor"
                rules={[{ required: true, message: 'Please select a vendor' }]}
              >
                <Select
                  placeholder="Select vendor"
                  showSearch
                  filterOption={(input, option) =>
                    option.children.toLowerCase().indexOf(input.toLowerCase()) >= 0
                  }
                >
                  {vendors.map(vendor => (
                    <Option key={vendor.id} value={vendor.id}>{vendor.name}</Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="billNumber"
                label="Bill Number"
                rules={[{ required: true, message: 'Please enter bill number' }]}
              >
                <Input placeholder="Enter bill number" />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="billDate"
                label="Bill Date"
                rules={[{ required: true, message: 'Please select bill date' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="dueDate"
                label="Due Date"
                rules={[{ required: true, message: 'Please select due date' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="amount"
                label="Amount"
                rules={[{ required: true, message: 'Please enter amount' }]}
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
                name="status"
                label="Status"
                rules={[{ required: true, message: 'Please select status' }]}
              >
                <Select placeholder="Select status">
                  <Option value="open">Open</Option>
                  <Option value="paid">Paid</Option>
                  <Option value="overdue">Overdue</Option>
                  <Option value="void">Void</Option>
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item
            name="description"
            label="Description"
          >
            <Input.TextArea rows={4} placeholder="Enter bill description" />
          </Form.Item>
          
          <Form.Item
            name="poId"
            hidden
          >
            <Input />
          </Form.Item>
          
          <div style={{ textAlign: 'right' }}>
            <Button
              style={{ marginRight: 8 }}
              onClick={() => setBillModalVisible(false)}
            >
              Cancel
            </Button>
            <Button type="primary" htmlType="submit" loading={loading}>
              {selectedBill ? "Update Bill" : "Create Bill"}
            </Button>
          </div>
        </Form>
      </Modal>
      
      {/* Payment Modal */}
      <Modal
        title="Create Payment"
        visible={paymentModalVisible}
        onCancel={() => setPaymentModalVisible(false)}
        footer={null}
      >
        <Form
          form={paymentForm}
          layout="vertical"
          onFinish={handlePaymentFormSubmit}
        >
          <Form.Item
            name="entityId"
            hidden
          >
            <Input />
          </Form.Item>
          
          <Form.Item
            name="entityType"
            hidden
          >
            <Input />
          </Form.Item>
          
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
          
          <Form.Item
            name="paymentDate"
            label="Payment Date"
            rules={[{ required: true, message: 'Please select payment date' }]}
          >
            <DatePicker style={{ width: '100%' }} />
          </Form.Item>
          
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
          
          <Form.Item
            name="reference"
            label="Reference / Check Number"
          >
            <Input placeholder="Enter reference or check number" />
          </Form.Item>
          
          <Form.Item
            name="notes"
            label="Notes"
          >
            <Input.TextArea rows={3} placeholder="Enter payment notes" />
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
    </div>
  );
};

export default AccountsPayableWorkspace;
