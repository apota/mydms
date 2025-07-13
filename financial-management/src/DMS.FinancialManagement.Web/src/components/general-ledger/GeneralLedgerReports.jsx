import React, { useState } from 'react';
import { Card, Row, Col, Form, Button, Spinner, Table, Tabs, Tab } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faDownload, faSearch, faPrint } from '@fortawesome/free-solid-svg-icons';
import PropTypes from 'prop-types';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';

const GeneralLedgerReports = ({ accounts, isLoading }) => {
  const [activeTab, setActiveTab] = useState('general-ledger');
  const [startDate, setStartDate] = useState(() => {
    const date = new Date();
    date.setDate(1); // First day of current month
    return date;
  });
  const [endDate, setEndDate] = useState(new Date());
  const [selectedAccountId, setSelectedAccountId] = useState('');
  const [reportData, setReportData] = useState(null);
  const [reportLoading, setReportLoading] = useState(false);

  const handleGenerateReport = (e) => {
    e.preventDefault();
    setReportLoading(true);

    // Simulate API call to generate report data
    setTimeout(() => {
      // This would be replaced with actual API data
      const mockReportData = {
        title: 'General Ledger Report',
        period: `${startDate.toLocaleDateString()} - ${endDate.toLocaleDateString()}`,
        account: accounts.find(a => a.id === selectedAccountId) || { accountNumber: 'All', name: 'Accounts' },
        entries: [
          {
            date: '2023-05-01',
            entryNumber: 'JE-2023-0001',
            description: 'Monthly rent payment',
            debit: 2500.00,
            credit: 0,
            balance: 2500.00
          },
          {
            date: '2023-05-05',
            entryNumber: 'JE-2023-0002',
            description: 'Office supplies purchase',
            debit: 350.75,
            credit: 0,
            balance: 2850.75
          },
          {
            date: '2023-05-10',
            entryNumber: 'JE-2023-0003',
            description: 'Client payment received',
            debit: 0,
            credit: 1200.00,
            balance: 1650.75
          },
          {
            date: '2023-05-15',
            entryNumber: 'JE-2023-0004',
            description: 'Utility bill payment',
            debit: 450.25,
            credit: 0,
            balance: 2101.00
          }
        ]
      };

      setReportData(mockReportData);
      setReportLoading(false);
    }, 1500);
  };

  const handlePrintReport = () => {
    window.print();
  };

  const handleDownloadReport = (format) => {
    alert(`Downloading report in ${format} format...`);
  };

  return (
    <div>
      <Card className="mb-4">
        <Card.Body>
          <Tabs
            id="report-tabs"
            activeKey={activeTab}
            onSelect={(k) => setActiveTab(k)}
            className="mb-4"
          >
            <Tab eventKey="general-ledger" title="General Ledger">
              <Form onSubmit={handleGenerateReport}>
                <Row className="mb-3">
                  <Col md={4}>
                    <Form.Group>
                      <Form.Label>Start Date</Form.Label>
                      <DatePicker
                        selected={startDate}
                        onChange={date => setStartDate(date)}
                        selectsStart
                        startDate={startDate}
                        endDate={endDate}
                        className="form-control"
                      />
                    </Form.Group>
                  </Col>
                  <Col md={4}>
                    <Form.Group>
                      <Form.Label>End Date</Form.Label>
                      <DatePicker
                        selected={endDate}
                        onChange={date => setEndDate(date)}
                        selectsEnd
                        startDate={startDate}
                        endDate={endDate}
                        minDate={startDate}
                        className="form-control"
                      />
                    </Form.Group>
                  </Col>
                  <Col md={4}>
                    <Form.Group>
                      <Form.Label>Account (Optional)</Form.Label>
                      <Form.Select
                        value={selectedAccountId}
                        onChange={e => setSelectedAccountId(e.target.value)}
                      >
                        <option value="">All Accounts</option>
                        {Object.entries(accounts.reduce((acc, account) => {
                          if (!acc[account.accountType]) {
                            acc[account.accountType] = [];
                          }
                          acc[account.accountType].push(account);
                          return acc;
                        }, {})).map(([type, accounts]) => (
                          <optgroup key={type} label={type}>
                            {accounts.map(account => (
                              <option key={account.id} value={account.id}>
                                {account.accountNumber} - {account.name}
                              </option>
                            ))}
                          </optgroup>
                        ))}
                      </Form.Select>
                    </Form.Group>
                  </Col>
                </Row>
                <Row>
                  <Col>
                    <Button variant="primary" type="submit">
                      <FontAwesomeIcon icon={faSearch} className="me-2" />
                      Generate Report
                    </Button>
                  </Col>
                </Row>
              </Form>
            </Tab>
            <Tab eventKey="trial-balance" title="Trial Balance">
              <p>Set parameters for the Trial Balance report.</p>
            </Tab>
            <Tab eventKey="balance-sheet" title="Balance Sheet">
              <p>Set parameters for the Balance Sheet report.</p>
            </Tab>
            <Tab eventKey="income-statement" title="Income Statement">
              <p>Set parameters for the Income Statement report.</p>
            </Tab>
            <Tab eventKey="cash-flow" title="Cash Flow Statement">
              <p>Set parameters for the Cash Flow Statement report.</p>
            </Tab>
          </Tabs>
        </Card.Body>
      </Card>

      {reportLoading ? (
        <div className="text-center p-5">
          <Spinner animation="border" variant="primary" />
          <p className="mt-3">Generating report...</p>
        </div>
      ) : reportData && (
        <Card className="print-section">
          <Card.Body>
            <div className="d-flex justify-content-between mb-4">
              <h3>{reportData.title}</h3>
              <div>
                <Button variant="outline-secondary" className="me-2" onClick={handlePrintReport}>
                  <FontAwesomeIcon icon={faPrint} className="me-2" />
                  Print
                </Button>
                <Button variant="outline-primary" className="me-2" onClick={() => handleDownloadReport('pdf')}>
                  <FontAwesomeIcon icon={faDownload} className="me-2" />
                  PDF
                </Button>
                <Button variant="outline-success" onClick={() => handleDownloadReport('excel')}>
                  <FontAwesomeIcon icon={faDownload} className="me-2" />
                  Excel
                </Button>
              </div>
            </div>

            <div className="mb-4">
              <Row>
                <Col md={6}>
                  <p><strong>Period:</strong> {reportData.period}</p>
                </Col>
                <Col md={6}>
                  <p><strong>Account:</strong> {reportData.account.accountNumber} - {reportData.account.name}</p>
                </Col>
              </Row>
            </div>

            <Table bordered striped hover>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Entry #</th>
                  <th>Description</th>
                  <th className="text-end">Debit</th>
                  <th className="text-end">Credit</th>
                  <th className="text-end">Balance</th>
                </tr>
              </thead>
              <tbody>
                {reportData.entries.map((entry, index) => (
                  <tr key={index}>
                    <td>{new Date(entry.date).toLocaleDateString()}</td>
                    <td>{entry.entryNumber}</td>
                    <td>{entry.description}</td>
                    <td className="text-end">
                      {entry.debit ? entry.debit.toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      }) : '-'}
                    </td>
                    <td className="text-end">
                      {entry.credit ? entry.credit.toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      }) : '-'}
                    </td>
                    <td className="text-end">
                      {entry.balance.toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      })}
                    </td>
                  </tr>
                ))}
                <tr className="table-secondary">
                  <td colSpan="3"><strong>Totals</strong></td>
                  <td className="text-end">
                    <strong>
                      {reportData.entries.reduce((sum, entry) => sum + (entry.debit || 0), 0).toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      })}
                    </strong>
                  </td>
                  <td className="text-end">
                    <strong>
                      {reportData.entries.reduce((sum, entry) => sum + (entry.credit || 0), 0).toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      })}
                    </strong>
                  </td>
                  <td className="text-end">
                    <strong>
                      {reportData.entries[reportData.entries.length - 1].balance.toLocaleString('en-US', {
                        style: 'currency',
                        currency: 'USD',
                      })}
                    </strong>
                  </td>
                </tr>
              </tbody>
            </Table>
          </Card.Body>
        </Card>
      )}
    </div>
  );
};

GeneralLedgerReports.propTypes = {
  accounts: PropTypes.array,
  isLoading: PropTypes.bool
};

GeneralLedgerReports.defaultProps = {
  accounts: [],
  isLoading: false
};

export default GeneralLedgerReports;
