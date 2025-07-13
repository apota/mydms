import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Nav, Tab, Button, Alert } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBook, faList, faPlus, faSave, faSync } from '@fortawesome/free-solid-svg-icons';
import financialService from '../../services/financialService';
import ChartOfAccounts from './ChartOfAccounts';
import JournalEntries from './JournalEntries';
import CreateJournalEntry from './CreateJournalEntry';
import GeneralLedgerReports from './GeneralLedgerReports';

const GeneralLedgerWorkspace = () => {
  const [activeTab, setActiveTab] = useState('chart-of-accounts');
  const [chartOfAccounts, setChartOfAccounts] = useState([]);
  const [journalEntries, setJournalEntries] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showCreateEntry, setShowCreateEntry] = useState(false);

  useEffect(() => {
    loadChartOfAccounts();
    
    // Load initial journal entries (last 30 days)
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 30);
    loadJournalEntries({ startDate, endDate });
  }, []);

  const loadChartOfAccounts = async () => {
    setIsLoading(true);
    try {
      const data = await financialService.getChartOfAccounts();
      setChartOfAccounts(data);
      setError(null);
    } catch (err) {
      console.error('Error loading chart of accounts:', err);
      setError('Failed to load chart of accounts. Please try again later.');
    } finally {
      setIsLoading(false);
    }
  };

  const loadJournalEntries = async (params) => {
    setIsLoading(true);
    try {
      const data = await financialService.getJournalEntries(params);
      setJournalEntries(data);
      setError(null);
    } catch (err) {
      console.error('Error loading journal entries:', err);
      setError('Failed to load journal entries. Please try again later.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateJournalEntry = async (journalEntry) => {
    try {
      await financialService.createJournalEntry(journalEntry);
      
      // Refresh journal entries
      const endDate = new Date();
      const startDate = new Date();
      startDate.setDate(startDate.getDate() - 30);
      await loadJournalEntries({ startDate, endDate });
      
      setShowCreateEntry(false);
      return true;
    } catch (err) {
      console.error('Error creating journal entry:', err);
      setError('Failed to create journal entry. Please try again.');
      return false;
    }
  };

  return (
    <Container fluid className="py-4">
      <Row className="mb-4">
        <Col>
          <h2 className="mb-3">General Ledger</h2>
          {error && <Alert variant="danger">{error}</Alert>}
        </Col>
        <Col xs="auto" className="d-flex align-items-center">
          {activeTab === 'journal-entries' && (
            <Button 
              variant="primary" 
              className="me-2"
              onClick={() => setShowCreateEntry(true)}
            >
              <FontAwesomeIcon icon={faPlus} className="me-2" />
              New Journal Entry
            </Button>
          )}
          <Button 
            variant="outline-secondary" 
            onClick={() => {
              if (activeTab === 'chart-of-accounts') {
                loadChartOfAccounts();
              } else if (activeTab === 'journal-entries') {
                const endDate = new Date();
                const startDate = new Date();
                startDate.setDate(startDate.getDate() - 30);
                loadJournalEntries({ startDate, endDate });
              }
            }}
          >
            <FontAwesomeIcon icon={faSync} className="me-1" />
            Refresh
          </Button>
        </Col>
      </Row>

      <Card>
        <Card.Header>
          <Nav variant="tabs" defaultActiveKey="chart-of-accounts" onSelect={setActiveTab}>
            <Nav.Item>
              <Nav.Link eventKey="chart-of-accounts">
                <FontAwesomeIcon icon={faBook} className="me-2" />
                Chart of Accounts
              </Nav.Link>
            </Nav.Item>
            <Nav.Item>
              <Nav.Link eventKey="journal-entries">
                <FontAwesomeIcon icon={faList} className="me-2" />
                Journal Entries
              </Nav.Link>
            </Nav.Item>
            <Nav.Item>
              <Nav.Link eventKey="reports">
                <FontAwesomeIcon icon={faBook} className="me-2" />
                Reports
              </Nav.Link>
            </Nav.Item>
          </Nav>
        </Card.Header>
        <Card.Body>
          <Tab.Content>
            <Tab.Pane eventKey="chart-of-accounts" active={activeTab === 'chart-of-accounts'}>
              <ChartOfAccounts 
                accounts={chartOfAccounts} 
                isLoading={isLoading}
              />
            </Tab.Pane>
            <Tab.Pane eventKey="journal-entries" active={activeTab === 'journal-entries'}>
              <JournalEntries 
                entries={journalEntries} 
                isLoading={isLoading}
                onDateRangeChange={loadJournalEntries}
              />
            </Tab.Pane>
            <Tab.Pane eventKey="reports" active={activeTab === 'reports'}>
              <GeneralLedgerReports 
                accounts={chartOfAccounts}
              />
            </Tab.Pane>
          </Tab.Content>
        </Card.Body>
      </Card>

      <CreateJournalEntry 
        show={showCreateEntry} 
        onHide={() => setShowCreateEntry(false)}
        onSave={handleCreateJournalEntry}
        accounts={chartOfAccounts}
      />
    </Container>
  );
};

export default GeneralLedgerWorkspace;
