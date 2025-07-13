import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Container, Row, Col, Nav } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { 
  faDollarSign, 
  faFileInvoice, 
  faMoneyBill, 
  faChartLine, 
  faReceipt, 
  faBook,
  faCoins,
  faCalculator
} from '@fortawesome/free-solid-svg-icons';

// Import components
import FinancialDashboard from './components/dashboard/FinancialDashboard';
import AccountsReceivableWorkspace from './components/accounts-receivable/AccountsReceivableWorkspace';
import AccountsPayableWorkspace from './components/accounts-payable/AccountsPayableWorkspace';
import PaymentsWorkspace from './components/payments/PaymentsWorkspace';
import FinancialReportingCenter from './components/reporting/FinancialReportingCenter';
import TaxManagementConsole from './components/tax/TaxManagementConsole';
import BudgetManagement from './components/budgeting/BudgetManagement';
import GeneralLedgerWorkspace from './components/general-ledger/GeneralLedgerWorkspace';

const Sidebar = ({ activeItem }) => {
  return (
    <Nav className="flex-column sidebar bg-light p-3" activeKey={activeItem}>
      <Nav.Link href="/dashboard" className="mb-2">
        <FontAwesomeIcon icon={faChartLine} className="me-2" />
        Dashboard
      </Nav.Link>
      <Nav.Link href="/accounts-receivable" className="mb-2">
        <FontAwesomeIcon icon={faFileInvoice} className="me-2" />
        Accounts Receivable
      </Nav.Link>
      <Nav.Link href="/accounts-payable" className="mb-2">
        <FontAwesomeIcon icon={faMoneyBill} className="me-2" />
        Accounts Payable
      </Nav.Link>
      <Nav.Link href="/payments" className="mb-2">
        <FontAwesomeIcon icon={faDollarSign} className="me-2" />
        Payments
      </Nav.Link>
      <Nav.Link href="/general-ledger" className="mb-2">
        <FontAwesomeIcon icon={faBook} className="me-2" />
        General Ledger
      </Nav.Link>
      <Nav.Link href="/budgeting" className="mb-2">
        <FontAwesomeIcon icon={faCalculator} className="me-2" />
        Budgeting
      </Nav.Link>
      <Nav.Link href="/tax" className="mb-2">
        <FontAwesomeIcon icon={faReceipt} className="me-2" />
        Tax Management
      </Nav.Link>
      <Nav.Link href="/reporting" className="mb-2">
        <FontAwesomeIcon icon={faChartLine} className="me-2" />
        Financial Reporting
      </Nav.Link>
    </Nav>
  );
};

const Layout = ({ children, activeNavItem }) => {
  return (
    <Container fluid>
      <Row>
        <Col md={2} className="px-0 bg-light sidebar-container">
          <Sidebar activeItem={activeNavItem} />
        </Col>
        <Col md={10} className="ms-auto px-4">
          {children}
        </Col>
      </Row>
    </Container>
  );
};

function App() {
  return (
    <Router>
      <div className="App">
        <Routes>
          <Route 
            path="/" 
            element={<Navigate replace to="/dashboard" />} 
          />
          
          <Route 
            path="/dashboard" 
            element={
              <Layout activeNavItem="/dashboard">
                <FinancialDashboard />
              </Layout>
            } 
          />
          
          <Route 
            path="/accounts-receivable/*" 
            element={
              <Layout activeNavItem="/accounts-receivable">
                <AccountsReceivableWorkspace />
              </Layout>
            } 
          />
          
          <Route 
            path="/accounts-payable/*" 
            element={
              <Layout activeNavItem="/accounts-payable">
                <AccountsPayableWorkspace />
              </Layout>
            } 
          />
          
          <Route 
            path="/payments/*" 
            element={
              <Layout activeNavItem="/payments">
                <PaymentsWorkspace />
              </Layout>
            } 
          />
          
          <Route 
            path="/general-ledger/*" 
            element={
              <Layout activeNavItem="/general-ledger">
                <GeneralLedgerWorkspace />
              </Layout>
            } 
          />
          
          <Route 
            path="/budgeting/*" 
            element={
              <Layout activeNavItem="/budgeting">
                <BudgetManagement />
              </Layout>
            } 
          />
          
          <Route 
            path="/tax/*" 
            element={
              <Layout activeNavItem="/tax">
                <TaxManagementConsole />
              </Layout>
            } 
          />
          
          <Route 
            path="/reporting/*" 
            element={
              <Layout activeNavItem="/reporting">
                <FinancialReportingCenter />
              </Layout>
            } 
          />
          
          <Route path="*" element={<div>Page not found</div>} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
