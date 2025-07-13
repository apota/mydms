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
  MailOutlined,
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

const AccountsReceivableWorkspace = () => {
  const [invoices, setInvoices] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedInvoice, setSelectedInvoice] = useState(null);
  const [showInvoiceForm, setShowInvoiceForm] = useState(false);
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [showCreditMemoForm, setShowCreditMemoForm] = useState(false);
  const [showStatementDrawer, setShowStatementDrawer] = useState(false);
  const [customerStatement, setCustomerStatement] = useState(null);
  const [selectedCustomerId, setSelectedCustomerId] = useState(null);
  const [filters, setFilters] = useState({
    status: null,
    customerId: null,
    dateRange: null,
  });

  const [invoiceForm] = Form.useForm();
  const [paymentForm] = Form.useForm();
  const [creditMemoForm] = Form.useForm();

  useEffect(() => {
    fetchInvoices();
    fetchCustomers();
  }, []);

  const fetchInvoices = async () => {
    try {
      setLoading(true);
      setError(null);
      
      let queryParams = {};
      if (filters.status) queryParams.status = filters.status;
      if (filters.customerId) queryParams.customerId = filters.customerId;
      if (filters.dateRange) {
        queryParams.fromDate = filters.dateRange[0].format('YYYY-MM-DD');
        queryParams.toDate = filters.dateRange[1].format('YYYY-MM-DD');
      }
      
      const data = await financialService.getInvoices(queryParams);
      setInvoices(data);
    } catch (err) {
      console.error('Error fetching invoices:', err);
      setError('Failed to load invoices. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const fetchCustomers = async () => {
    try {
      const data = await financialService.getCustomers();
      setCustomers(data);
    } catch (err) {
      console.error('Error fetching customers:', err);
      // Non-critical error, don't show to user
    }
  };

  const handleFilterChange = (key, value) => {
    setFilters({
      ...filters,
      [key]: value,
    });
  };

  const applyFilters = () => {
    fetchInvoices();
  };

  const clearFilters = () => {
    setFilters({
      status: null,
      customerId: null,
      dateRange: null,
    });
    fetchInvoices();
  };

  const handleCreateInvoice = () => {
    invoiceForm.resetFields();
    setSelectedInvoice(null);
    setShowInvoiceForm(true);
  };

  const handleEditInvoice = (invoice) => {
    setSelectedInvoice(invoice);
    invoiceForm.setFieldsValue({
      customerId: invoice.customerId,
      invoiceDate: moment(invoice.invoiceDate),
      dueDate: moment(invoice.dueDate),
      notes: invoice.notes,
      // Set other fields
    });
    setShowInvoiceForm(true);
  };

  const handleSaveInvoice = async (values) => {
    try {
      setLoading(true);
      let response;
      
      const invoiceData = {
        ...values,
        invoiceDate: values.invoiceDate.format('YYYY-MM-DD'),
        dueDate: values.dueDate.format('YYYY-MM-DD'),
      };
      
      if (selectedInvoice) {
        // Update existing invoice
        response = await financialService.updateInvoice({
          ...invoiceData,
          id: selectedInvoice.id,
        });
      } else {
        // Create new invoice
        response = await financialService.createInvoice(invoiceData);
      }
      
      setShowInvoiceForm(false);
      fetchInvoices();
      
    } catch (err) {
      console.error('Error saving invoice:', err);
      Modal.error({
        title: 'Error',
        content: 'Failed to save invoice. Please try again.',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSendInvoice = async (invoiceId) => {
    try {
      setLoading(true);
      await financialService.sendInvoice(invoiceId);
      fetchInvoices();
      Modal.success({
        title: 'Success',
        content: 'Invoice has been sent to the customer.',
      });
    } catch (err) {
      console.error('Error sending invoice:', err);
      Modal.error({
        title: 'Error',
        content: 'Failed to send invoice. Please try again.',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleRecordPayment = (invoice) => {
    setSelectedInvoice(invoice);
    paymentForm.resetFields();
    paymentForm.setFieldsValue({
      invoiceIds: [invoice.id],
      amount: invoice.totalAmount - invoice.paidAmount,
      paymentDate: moment(),
    });
    setShowPaymentForm(true);
  };

  const handleSavePayment = async (values) => {
    try {
      setLoading(true);
      const paymentData = {
        payment: {
          paymentDate: values.paymentDate.format('YYYY-MM-DD'),
          amount: values.amount,
          paymentMethod: values.paymentMethod,
          reference: values.reference || '',
          entityId: values.customerId,
          entityType: 'Customer',
        },
        invoiceIds: values.invoiceIds,
      };
      
      await financialService.recordInvoicePayment(paymentData);
      setShowPaymentForm(false);
      fetchInvoices();
      
      Modal.success({
        title: 'Success',
        content: 'Payment recorded successfully.',
      });
      
    } catch (err) {
      console.error('Error recording payment:', err);
      Modal.error({
        title: 'Error',
        content: 'Failed to record payment. Please try again.',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCreateCreditMemo = () => {
    creditMemoForm.resetFields();
    setShowCreditMemoForm(true);
  };

  const handleSaveCreditMemo = async (values) => {
    try {
      setLoading(true);
      const creditMemoData = {
        customerId: values.customerId,
        amount: values.amount,
        reason: values.reason,
        issueDate: values.issueDate.format('YYYY-MM-DD'),
        originalInvoiceId: values.originalInvoiceId || null,
      };
      
      await financialService.createCreditMemo(creditMemoData);
      setShowCreditMemoForm(false);
      
      Modal.success({
        title: 'Success',
        content: 'Credit memo created successfully.',
      });
      
    } catch (err) {
      console.error('Error creating credit memo:', err);
      Modal.error({
        title: 'Error',
        content: 'Failed to create credit memo. Please try again.',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleGenerateStatement = async (customerId) => {
    try {
      setLoading(true);
      setSelectedCustomerId(customerId);
      
      const statement = await financialService.getCustomerStatement(customerId);
      setCustomerStatement(statement);
      setShowStatementDrawer(true);
      
    } catch (err) {
      console.error('Error generating statement:', err);
      Modal.error({
        title: 'Error',
        content: 'Failed to generate customer statement. Please try again.',
      });
    } finally {
      setLoading(false);
    }
  };

  const getStatusTag = (status) => {
    switch (status) {
      case 'Draft':
        return <Tag color="default"><ClockCircleOutlined /> Draft</Tag>;
      case 'Sent':
        return <Tag color="blue"><MailOutlined /> Sent</Tag>;
      case 'Overdue':
        return <Tag color="red"><ExclamationCircleOutlined /> Overdue</Tag>;
      case 'Paid':
        return <Tag color="green"><CheckCircleOutlined /> Paid</Tag>;
      case 'Canceled':
        return <Tag color="gray"><CloseCircleOutlined /> Canceled</Tag>;
      default:
        return <Tag>{status}</Tag>;
    }
  };

  const columns = [
    {
      title: 'Invoice #',
      dataIndex: 'invoiceNumber',
      key: 'invoiceNumber',
    },
    {
      title: 'Customer',
      dataIndex: 'customerName',
      key: 'customerName',
    },
    {
      title: 'Date',
      dataIndex: 'invoiceDate',
      key: 'invoiceDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.invoiceDate).unix() - moment(b.invoiceDate).unix(),
    },
    {
      title: 'Due Date',
      dataIndex: 'dueDate',
      key: 'dueDate',
      render: (date) => moment(date).format('MM/DD/YYYY'),
      sorter: (a, b) => moment(a.dueDate).unix() - moment(b.dueDate).unix(),
    },
    {
      title: 'Amount',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      render: (amount) => `$${amount.toFixed(2)}`,
      sorter: (a, b) => a.totalAmount - b.totalAmount,
    },
    {
      title: 'Balance',
      key: 'balance',
      render: (_, record) => `$${(record.totalAmount - record.paidAmount).toFixed(2)}`,
      sorter: (a, b) => (a.totalAmount - a.paidAmount) - (b.totalAmount - b.paidAmount),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status) => getStatusTag(status),
      filters: [
        { text: 'Draft', value: 'Draft' },
        { text: 'Sent', value: 'Sent' },
        { text: 'Overdue', value: 'Overdue' },
        { text: 'Paid', value: 'Paid' },
        { text: 'Canceled', value: 'Canceled' },
      ],
      onFilter: (value, record) => record.status === value,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Edit">
            <Button
              icon={<EditOutlined />}
              size="small"
              onClick={() => handleEditInvoice(record)}
              disabled={record.status === 'Paid' || record.status === 'Canceled'}
            />
          </Tooltip>
          
          {record.status === 'Draft' && (
            <Tooltip title="Send Invoice">
              <Button
                icon={<MailOutlined />}
                size="small"
                onClick={() => handleSendInvoice(record.id)}
              />
            </Tooltip>
          )}
          
          {(record.status === 'Sent' || record.status === 'Overdue') && (
            <Tooltip title="Record Payment">
              <Button
                icon={<DollarOutlined />}
                size="small"
                type="primary"
                onClick={() => handleRecordPayment(record)}
              />
            </Tooltip>
          )}
          
          <Tooltip title="Print">
            <Button icon={<PrinterOutlined />} size="small" />
          </Tooltip>
        </Space>
      ),
    },
  ];

  return (
    <div className="accounts-receivable-workspace">
      <Title level={2}>Accounts Receivable</Title>
      
      <Tabs defaultActiveKey="invoices">
        <TabPane tab="Invoices" key="invoices">
          <Card>
            <div className="filter-section" style={{ marginBottom: 16 }}>
              <Row gutter={16} align="middle">
                <Col>
                  <Select
                    placeholder="Status"
                    allowClear
                    style={{ width: 120 }}
                    value={filters.status}
                    onChange={(value) => handleFilterChange('status', value)}
                  >
                    <Option value="Draft">Draft</Option>
                    <Option value="Sent">Sent</Option>
                    <Option value="Overdue">Overdue</Option>
                    <Option value="Paid">Paid</Option>
                    <Option value="Canceled">Canceled</Option>
                  </Select>
                </Col>
                <Col>
                  <Select
                    placeholder="Customer"
                    allowClear
                    style={{ width: 180 }}
                    value={filters.customerId}
                    onChange={(value) => handleFilterChange('customerId', value)}
                  >
                    {customers.map(customer => (
                      <Option key={customer.id} value={customer.id}>{customer.name}</Option>
                    ))}
                  </Select>
                </Col>
                <Col>
                  <RangePicker
                    value={filters.dateRange}
                    onChange={(dates) => handleFilterChange('dateRange', dates)}
                  />
                </Col>
                <Col>
                  <Button type="primary" icon={<SearchOutlined />} onClick={applyFilters}>
                    Search
                  </Button>
                </Col>
                <Col>
                  <Button onClick={clearFilters}>Clear</Button>
                </Col>
                <Col flex="auto"></Col>
                <Col>
                  <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleCreateInvoice}
                  >
                    New Invoice
                  </Button>
                </Col>
                <Col>
                  <Button
                    icon={<PlusOutlined />}
                    onClick={handleCreateCreditMemo}
                  >
                    New Credit Memo
                  </Button>
                </Col>
              </Row>
            </div>
            
            {error && <Alert message={error} type="error" showIcon style={{ marginBottom: 16 }} />}
            
            <Table
              columns={columns}
              dataSource={invoices}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10 }}
            />
          </Card>
        </TabPane>
        
        <TabPane tab="Customer Statements" key="statements">
          <Card>
            <div style={{ marginBottom: 16 }}>
              <Row gutter={16} align="middle">
                <Col span={6}>
                  <Select
                    placeholder="Select Customer"
                    style={{ width: '100%' }}
                    onChange={(value) => setSelectedCustomerId(value)}
                    value={selectedCustomerId}
                  >
                    {customers.map(customer => (
                      <Option key={customer.id} value={customer.id}>{customer.name}</Option>
                    ))}
                  </Select>
                </Col>
                <Col>
                  <Button
                    type="primary"
                    icon={<FileDoneOutlined />}
                    onClick={() => handleGenerateStatement(selectedCustomerId)}
                    disabled={!selectedCustomerId}
                  >
                    Generate Statement
                  </Button>
                </Col>
              </Row>
            </div>
          </Card>
        </TabPane>
        
        <TabPane tab="Aging Report" key="aging">
          <Card>
            <div style={{ marginBottom: 16 }}>
              <DatePicker />
              <Button type="primary" style={{ marginLeft: 16 }}>
                Generate Report
              </Button>
            </div>
          </Card>
        </TabPane>
      </Tabs>
      
      {/* Invoice Form Modal */}
      <Modal
        title={selectedInvoice ? 'Edit Invoice' : 'Create New Invoice'}
        visible={showInvoiceForm}
        onCancel={() => setShowInvoiceForm(false)}
        footer={null}
        width={800}
      >
        <Form
          form={invoiceForm}
          layout="vertical"
          onFinish={handleSaveInvoice}
        >
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="customerId"
                label="Customer"
                rules={[{ required: true, message: 'Please select a customer' }]}
              >
                <Select placeholder="Select a customer">
                  {customers.map(customer => (
                    <Option key={customer.id} value={customer.id}>{customer.name}</Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="salesOrderId"
                label="Sales Order (Optional)"
              >
                <Select placeholder="Select a sales order" allowClear>
                  {/* Sales order options would go here */}
                </Select>
              </Form.Item>
            </Col>
          </Row>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="invoiceDate"
                label="Invoice Date"
                rules={[{ required: true, message: 'Please select the invoice date' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="dueDate"
                label="Due Date"
                rules={[{ required: true, message: 'Please select the due date' }]}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item
            name="notes"
            label="Notes"
          >
            <Input.TextArea rows={4} />
          </Form.Item>
          
          {/* Line items would go here */}
          
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>
              Save
            </Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setShowInvoiceForm(false)}>
              Cancel
            </Button>
          </Form.Item>
        </Form>
      </Modal>
      
      {/* Payment Form Modal */}
      <Modal
        title="Record Payment"
        visible={showPaymentForm}
        onCancel={() => setShowPaymentForm(false)}
        footer={null}
      >
        <Form
          form={paymentForm}
          layout="vertical"
          onFinish={handleSavePayment}
        >
          <Form.Item
            name="invoiceIds"
            label="Invoices"
            rules={[{ required: true, message: 'Please select at least one invoice' }]}
          >
            <Select mode="multiple" placeholder="Select invoices" disabled={!!selectedInvoice}>
              {invoices
                .filter(invoice => invoice.status === 'Sent' || invoice.status === 'Overdue')
                .map(invoice => (
                  <Option key={invoice.id} value={invoice.id}>
                    {invoice.invoiceNumber} - ${(invoice.totalAmount - invoice.paidAmount).toFixed(2)}
                  </Option>
                ))}
            </Select>
          </Form.Item>
          
          <Form.Item
            name="customerId"
            label="Customer"
            initialValue={selectedInvoice?.customerId}
          >
            <Select placeholder="Select customer" disabled>
              {customers.map(customer => (
                <Option key={customer.id} value={customer.id}>{customer.name}</Option>
              ))}
            </Select>
          </Form.Item>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="amount"
                label="Amount"
                rules={[{ required: true, message: 'Please enter the payment amount' }]}
              >
                <InputNumber
                  formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={value => value.replace(/\$\s?|(,*)/g, '')}
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="paymentDate"
                label="Payment Date"
                rules={[{ required: true, message: 'Please select the payment date' }]}
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
                rules={[{ required: true, message: 'Please select the payment method' }]}
              >
                <Select placeholder="Select payment method">
                  <Option value="Cash">Cash</Option>
                  <Option value="Check">Check</Option>
                  <Option value="CreditCard">Credit Card</Option>
                  <Option value="DebitCard">Debit Card</Option>
                  <Option value="BankTransfer">Bank Transfer</Option>
                  <Option value="ElectronicFundsTransfer">Electronic Funds Transfer</Option>
                  <Option value="Other">Other</Option>
                </Select>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="reference"
                label="Reference"
              >
                <Input placeholder="Check #, transaction ID, etc." />
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>
              Save Payment
            </Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setShowPaymentForm(false)}>
              Cancel
            </Button>
          </Form.Item>
        </Form>
      </Modal>
      
      {/* Credit Memo Form Modal */}
      <Modal
        title="Create Credit Memo"
        visible={showCreditMemoForm}
        onCancel={() => setShowCreditMemoForm(false)}
        footer={null}
      >
        <Form
          form={creditMemoForm}
          layout="vertical"
          onFinish={handleSaveCreditMemo}
        >
          <Form.Item
            name="customerId"
            label="Customer"
            rules={[{ required: true, message: 'Please select a customer' }]}
          >
            <Select placeholder="Select a customer">
              {customers.map(customer => (
                <Option key={customer.id} value={customer.id}>{customer.name}</Option>
              ))}
            </Select>
          </Form.Item>
          
          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="amount"
                label="Amount"
                rules={[{ required: true, message: 'Please enter the credit amount' }]}
              >
                <InputNumber
                  formatter={value => `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={value => value.replace(/\$\s?|(,*)/g, '')}
                  style={{ width: '100%' }}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="issueDate"
                label="Issue Date"
                rules={[{ required: true, message: 'Please select the issue date' }]}
                initialValue={moment()}
              >
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
          
          <Form.Item
            name="originalInvoiceId"
            label="Original Invoice (Optional)"
          >
            <Select placeholder="Select an invoice" allowClear>
              {invoices
                .filter(invoice => invoice.status !== 'Draft' && invoice.status !== 'Canceled')
                .map(invoice => (
                  <Option key={invoice.id} value={invoice.id}>
                    {invoice.invoiceNumber} - ${invoice.totalAmount.toFixed(2)}
                  </Option>
                ))}
            </Select>
          </Form.Item>
          
          <Form.Item
            name="reason"
            label="Reason"
            rules={[{ required: true, message: 'Please enter the reason for credit memo' }]}
          >
            <Input.TextArea rows={4} />
          </Form.Item>
          
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>
              Create Credit Memo
            </Button>
            <Button style={{ marginLeft: 8 }} onClick={() => setShowCreditMemoForm(false)}>
              Cancel
            </Button>
          </Form.Item>
        </Form>
      </Modal>
      
      {/* Customer Statement Drawer */}
      <Drawer
        title={`Customer Statement: ${customerStatement?.customerName || ''}`}
        placement="right"
        width={800}
        onClose={() => setShowStatementDrawer(false)}
        visible={showStatementDrawer}
        extra={
          <Space>
            <Button icon={<PrinterOutlined />}>Print</Button>
            <Button icon={<MailOutlined />}>Email</Button>
          </Space>
        }
      >
        {customerStatement && (
          <div className="customer-statement">
            <Row gutter={16}>
              <Col span={12}>
                <p><strong>Statement Date:</strong> {moment(customerStatement.statementDate).format('MM/DD/YYYY')}</p>
                <p><strong>Customer:</strong> {customerStatement.customerName}</p>
              </Col>
              <Col span={12} style={{ textAlign: 'right' }}>
                <p><strong>Beginning Balance:</strong> ${customerStatement.beginningBalance.toFixed(2)}</p>
                <p><strong>Ending Balance:</strong> ${customerStatement.endingBalance.toFixed(2)}</p>
              </Col>
            </Row>
            
            <Table
              dataSource={customerStatement.lineItems}
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
                  title: 'Reference',
                  dataIndex: 'referenceNumber',
                  key: 'referenceNumber',
                },
                {
                  title: 'Charges',
                  dataIndex: 'charges',
                  key: 'charges',
                  render: (amount) => `$${amount.toFixed(2)}`,
                  align: 'right',
                },
                {
                  title: 'Payments',
                  dataIndex: 'payments',
                  key: 'payments',
                  render: (amount) => `$${amount.toFixed(2)}`,
                  align: 'right',
                },
                {
                  title: 'Balance',
                  dataIndex: 'balance',
                  key: 'balance',
                  render: (amount) => `$${amount.toFixed(2)}`,
                  align: 'right',
                },
              ]}
              pagination={false}
              summary={() => (
                <Table.Summary.Row>
                  <Table.Summary.Cell colSpan={3}><strong>Total</strong></Table.Summary.Cell>
                  <Table.Summary.Cell align="right">
                    <strong>${customerStatement.lineItems.reduce((sum, item) => sum + item.charges, 0).toFixed(2)}</strong>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell align="right">
                    <strong>${customerStatement.lineItems.reduce((sum, item) => sum + item.payments, 0).toFixed(2)}</strong>
                  </Table.Summary.Cell>
                  <Table.Summary.Cell align="right">
                    <strong>${customerStatement.endingBalance.toFixed(2)}</strong>
                  </Table.Summary.Cell>
                </Table.Summary.Row>
              )}
            />
          </div>
        )}
      </Drawer>
    </div>
  );
};

export default AccountsReceivableWorkspace;
