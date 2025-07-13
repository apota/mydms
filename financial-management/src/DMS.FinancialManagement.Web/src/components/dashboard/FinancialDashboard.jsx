import React, { useEffect, useState } from 'react';
import { Card, Row, Col, Table, Statistic, Spin, Alert, Tabs } from 'antd';
import { 
  DollarCircleOutlined, 
  ArrowUpOutlined, 
  ArrowDownOutlined,
  ClockCircleOutlined,
  BarChartOutlined
} from '@ant-design/icons';
import axios from 'axios';
import moment from 'moment';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { financialService } from '../../services/financialService';

const { TabPane } = Tabs;

const FinancialDashboard = () => {
  const [cashPosition, setCashPosition] = useState(null);
  const [arSummary, setArSummary] = useState(null);
  const [apSummary, setApSummary] = useState(null);
  const [dailySales, setDailySales] = useState(null);
  const [profitLoss, setProfitLoss] = useState(null);
  const [budgetComparison, setBudgetComparison] = useState(null);
  const [recentTransactions, setRecentTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        
        // Cash Position
        const cashPositionData = await financialService.getCashPosition();
        setCashPosition(cashPositionData);
        
        // AR Summary
        const arData = await financialService.getAccountsReceivableAging();
        setArSummary(arData);
        
        // AP Summary
        const apData = await financialService.getAccountsPayableAging();
        setApSummary(apData);
        
        // Daily Sales
        const salesData = await financialService.getDailySalesMetrics();
        setDailySales(salesData);
        
        // Monthly P&L
        const plData = await financialService.getMonthlyProfitLoss();
        setProfitLoss(plData);
        
        // Budget Comparison
        const budgetData = await financialService.getBudgetComparison();
        setBudgetComparison(budgetData);
        
        // Recent Transactions
        const transactionData = await financialService.getRecentTransactions();
        setRecentTransactions(transactionData);
        
        setLoading(false);
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError('Failed to load dashboard data. Please try again later.');
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  const renderCashPosition = () => (
    <Card title="Current Cash Position" className="dashboard-card">
      <Row gutter={16}>
        {cashPosition && cashPosition.accounts.map(account => (
          <Col span={8} key={account.id}>
            <Statistic
              title={account.name}
              value={account.balance}
              precision={2}
              valueStyle={{ color: account.balance >= 0 ? '#3f8600' : '#cf1322' }}
              prefix={<DollarCircleOutlined />}
              suffix="USD"
            />
          </Col>
        ))}
      </Row>
      <div className="total-section">
        <Statistic
          title="Total Cash"
          value={cashPosition ? cashPosition.totalBalance : 0}
          precision={2}
          valueStyle={{ color: cashPosition && cashPosition.totalBalance >= 0 ? '#3f8600' : '#cf1322' }}
          prefix={<DollarCircleOutlined />}
          suffix="USD"
        />
      </div>
    </Card>
  );

  const renderArSummary = () => (
    <Card title="Accounts Receivable Aging" className="dashboard-card">
      {arSummary && (
        <>
          <Row gutter={16}>
            <Col span={6}>
              <Statistic 
                title="Current" 
                value={arSummary.current} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
              />
            </Col>
            <Col span={6}>
              <Statistic 
                title="1-30 Days" 
                value={arSummary.days1To30} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
              />
            </Col>
            <Col span={6}>
              <Statistic 
                title="31-60 Days" 
                value={arSummary.days31To60} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
                valueStyle={{ color: '#ff9900' }}
              />
            </Col>
            <Col span={6}>
              <Statistic 
                title="Over 60 Days" 
                value={arSummary.daysOver60} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
                valueStyle={{ color: '#cf1322' }}
              />
            </Col>
          </Row>
          <div className="total-section">
            <Statistic
              title="Total Receivables"
              value={arSummary.total}
              precision={2}
              prefix={<DollarCircleOutlined />}
              suffix="USD"
            />
          </div>
        </>
      )}
    </Card>
  );

  const renderApSummary = () => (
    <Card title="Accounts Payable Summary" className="dashboard-card">
      {apSummary && (
        <>
          <Row gutter={16}>
            <Col span={8}>
              <Statistic 
                title="Due Today" 
                value={apSummary.dueToday} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
                valueStyle={{ color: '#cf1322' }}
              />
            </Col>
            <Col span={8}>
              <Statistic 
                title="Due This Week" 
                value={apSummary.dueThisWeek} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
                valueStyle={{ color: '#ff9900' }}
              />
            </Col>
            <Col span={8}>
              <Statistic 
                title="Due Next Week" 
                value={apSummary.dueNextWeek} 
                precision={2}
                prefix={<DollarCircleOutlined />} 
              />
            </Col>
          </Row>
          <div className="total-section">
            <Statistic
              title="Total Payables"
              value={apSummary.total}
              precision={2}
              prefix={<DollarCircleOutlined />}
              suffix="USD"
            />
          </div>
        </>
      )}
    </Card>
  );

  const renderDailySales = () => (
    <Card title="Daily Sales & Revenue" className="dashboard-card">
      {dailySales && (
        <>
          <Row gutter={16}>
            <Col span={8}>
              <Statistic
                title="Today's Sales"
                value={dailySales.today}
                precision={2}
                valueStyle={{ color: dailySales.todayChange >= 0 ? '#3f8600' : '#cf1322' }}
                prefix={<DollarCircleOutlined />}
                suffix={
                  <span style={{ fontSize: '14px', marginLeft: '5px' }}>
                    {dailySales.todayChange >= 0 ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
                    {Math.abs(dailySales.todayChange)}%
                  </span>
                }
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="This Week"
                value={dailySales.thisWeek}
                precision={2}
                valueStyle={{ color: dailySales.weekChange >= 0 ? '#3f8600' : '#cf1322' }}
                prefix={<DollarCircleOutlined />}
                suffix={
                  <span style={{ fontSize: '14px', marginLeft: '5px' }}>
                    {dailySales.weekChange >= 0 ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
                    {Math.abs(dailySales.weekChange)}%
                  </span>
                }
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="This Month"
                value={dailySales.thisMonth}
                precision={2}
                valueStyle={{ color: dailySales.monthChange >= 0 ? '#3f8600' : '#cf1322' }}
                prefix={<DollarCircleOutlined />}
                suffix={
                  <span style={{ fontSize: '14px', marginLeft: '5px' }}>
                    {dailySales.monthChange >= 0 ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
                    {Math.abs(dailySales.monthChange)}%
                  </span>
                }
              />
            </Col>
          </Row>
          <div style={{ height: '200px', marginTop: '20px' }}>
            <ResponsiveContainer width="100%" height="100%">
              <BarChart
                data={dailySales.dailyBreakdown}
                margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
              >
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                <Legend />
                <Bar dataKey="sales" name="Sales" fill="#82ca9d" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </>
      )}
    </Card>
  );

  const renderProfitLoss = () => (
    <Card title="Monthly Profit & Loss" className="dashboard-card">
      {profitLoss && (
        <div style={{ height: '250px' }}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart
              data={profitLoss.monthlyData}
              margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
              <Legend />
              <Bar dataKey="revenue" name="Revenue" fill="#82ca9d" />
              <Bar dataKey="expense" name="Expenses" fill="#ff7875" />
              <Bar dataKey="profit" name="Profit" fill="#1890ff" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </Card>
  );

  const renderBudgetComparison = () => (
    <Card title="Budget vs. Actual" className="dashboard-card">
      {budgetComparison && (
        <>
          <Tabs defaultActiveKey="revenue">
            <TabPane tab="Revenue" key="revenue">
              <div style={{ height: '250px' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={budgetComparison.revenue}
                    margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                    <Legend />
                    <Bar dataKey="budget" name="Budget" fill="#8884d8" />
                    <Bar dataKey="actual" name="Actual" fill="#82ca9d" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </TabPane>
            <TabPane tab="Expenses" key="expenses">
              <div style={{ height: '250px' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={budgetComparison.expenses}
                    margin={{ top: 20, right: 30, left: 20, bottom: 5 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip formatter={(value) => `$${value.toFixed(2)}`} />
                    <Legend />
                    <Bar dataKey="budget" name="Budget" fill="#8884d8" />
                    <Bar dataKey="actual" name="Actual" fill="#ff7875" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </TabPane>
          </Tabs>
        </>
      )}
    </Card>
  );

  const renderRecentTransactions = () => (
    <Card title="Recent Transaction Activity" className="dashboard-card">
      <Table
        dataSource={recentTransactions}
        columns={[
          {
            title: 'Date',
            dataIndex: 'date',
            key: 'date',
            render: (date) => moment(date).format('MM/DD/YYYY'),
          },
          {
            title: 'Reference',
            dataIndex: 'reference',
            key: 'reference',
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
          },
          {
            title: 'Amount',
            dataIndex: 'amount',
            key: 'amount',
            render: (amount) => (
              <span style={{ color: amount >= 0 ? '#3f8600' : '#cf1322' }}>
                ${Math.abs(amount).toFixed(2)}
              </span>
            ),
          },
        ]}
        pagination={false}
        size="small"
      />
    </Card>
  );

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
        <p>Loading dashboard data...</p>
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
    <div className="financial-dashboard">
      <h1><BarChartOutlined /> Financial Dashboard</h1>
      
      <Row gutter={[16, 16]}>
        <Col span={24}>
          {renderCashPosition()}
        </Col>
      </Row>
      
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        <Col span={12}>
          {renderArSummary()}
        </Col>
        <Col span={12}>
          {renderApSummary()}
        </Col>
      </Row>
      
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        <Col span={24}>
          {renderDailySales()}
        </Col>
      </Row>
      
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        <Col span={12}>
          {renderProfitLoss()}
        </Col>
        <Col span={12}>
          {renderBudgetComparison()}
        </Col>
      </Row>
        <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        <Col span={24}>
          {renderGeneralLedgerSection()}
        </Col>
      </Row>
      
      <Row gutter={[16, 16]} style={{ marginTop: '16px' }}>
        <Col span={24}>
          {renderRecentTransactions()}
        </Col>
      </Row>
    </div>
  );
  
  const renderGeneralLedgerSection = () => (
    <Card title="General Ledger" 
      className="dashboard-card" 
      extra={<a href="/general-ledger">View Details</a>}
    >
      <Row gutter={16}>
        <Col span={8}>
          <Card className="inner-card">
            <Statistic
              title="Chart of Accounts"
              value="View"
              valueStyle={{ fontSize: '16px' }}
              prefix={<BarChartOutlined />}
            />
            <div className="mt-2">
              <a href="/general-ledger?tab=chart-of-accounts">
                Manage Account Structure
              </a>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card className="inner-card">
            <Statistic
              title="Journal Entries"
              value="Create"
              valueStyle={{ fontSize: '16px' }}
              prefix={<DollarCircleOutlined />}
            />
            <div className="mt-2">
              <a href="/general-ledger?tab=journal-entries">
                Record Financial Transactions
              </a>
            </div>
          </Card>
        </Col>
        <Col span={8}>
          <Card className="inner-card">
            <Statistic
              title="Financial Reports"
              value="Generate"
              valueStyle={{ fontSize: '16px' }}
              prefix={<BarChartOutlined />}
            />
            <div className="mt-2">
              <a href="/general-ledger?tab=reports">
                Access Financial Statements
              </a>
            </div>
          </Card>
        </Col>
      </Row>
    </Card>
  );
};

export default FinancialDashboard;
