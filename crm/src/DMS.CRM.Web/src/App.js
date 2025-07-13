import React from 'react';
import { Routes, Route, NavLink } from 'react-router-dom';
import Dashboard from './components/Dashboard';
import CustomerList from './components/CustomerList';
import CampaignList from './components/CampaignList';
import SurveyAnalytics from './components/SurveyAnalytics';
import './App.css';

function App() {
  const handleBackToDashboard = () => {
    window.location.href = 'http://localhost:3000';
  };

  return (
    <div className="app-container">
      <header className="app-header">
        <div className="header-left">
          <div className="app-title">
            <span className="app-icon">👥</span>
            <span className="title-text">DMS CRM Management</span>
          </div>
        </div>
        
        <nav className="header-nav">
          <NavLink to="/" className={({isActive}) => isActive ? "nav-link active" : "nav-link"}>
            Dashboard
          </NavLink>
          <NavLink to="/customers" className={({isActive}) => isActive ? "nav-link active" : "nav-link"}>
            Customer Management
          </NavLink>
          <NavLink to="/campaigns" className={({isActive}) => isActive ? "nav-link active" : "nav-link"}>
            Campaign Analytics
          </NavLink>
          <NavLink to="/analytics" className={({isActive}) => isActive ? "nav-link active" : "nav-link"}>
            Performance Analytics
          </NavLink>
        </nav>

        <div className="header-right">
          <button className="back-to-dashboard-btn" onClick={handleBackToDashboard}>
            <span className="btn-icon">🏠</span>
            Back to Dashboard
          </button>
        </div>
      </header>

      <main className="app-main">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/customers" element={<CustomerList />} />
          <Route path="/campaigns" element={<CampaignList />} />
          <Route path="/analytics" element={<SurveyAnalytics />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
