import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Container } from 'react-bootstrap';
import NavBar from './components/layout/NavBar';
import Footer from './components/layout/Footer';
import InventoryDashboard from './components/inventory/InventoryDashboard';
import VehicleDetailView from './components/inventory/VehicleDetailView';
import VehicleSearch from './components/inventory/VehicleSearch';
import VehiclePricing from './components/inventory/VehiclePricing';
import VehicleDocuments from './components/inventory/VehicleDocuments';
import VehicleForm from './components/inventory/VehicleForm';
import AgingHeatMap from './components/inventory/AgingHeatMap';
import PriceCompetitiveness from './components/inventory/PriceCompetitiveness';
import InventoryMixAnalysis from './components/inventory/InventoryMixAnalysis';
import TurnoverMetrics from './components/inventory/TurnoverMetrics';
import MarketplaceManager from './components/inventory/MarketplaceManager';
import WorkflowManagement from './components/inventory/WorkflowManagement';
import AcquisitionManagement from './components/inventory/AcquisitionManagement';
import './App.css';

function App() {
  return (
    <Router>
      <div className="app-container">
        <NavBar />
        <Container fluid className="main-content">
          <Routes>
            <Route path="/" element={<Navigate to="/inventory" replace />} />
            <Route path="/inventory" element={<InventoryDashboard />} />
            <Route path="/inventory/search" element={<VehicleSearch />} />
            <Route path="/inventory/add" element={<VehicleForm />} />
            <Route path="/inventory/vehicles/:id" element={<VehicleDetailView />} />
            <Route path="/inventory/vehicles/:id/edit" element={<VehicleForm />} />
            <Route path="/inventory/vehicles/:id/pricing" element={<VehiclePricing />} />
            <Route path="/inventory/vehicles/:id/documents" element={<VehicleDocuments />} />
            
            {/* Advanced Analytics Routes */}
            <Route path="/inventory/analytics/aging" element={<AgingHeatMap />} />
            <Route path="/inventory/analytics/pricing" element={<PriceCompetitiveness />} />
            <Route path="/inventory/analytics/mix" element={<InventoryMixAnalysis />} />
            <Route path="/inventory/analytics/turnover" element={<TurnoverMetrics />} />
              {/* Marketplace Integration */}
            <Route path="/inventory/marketplace" element={<MarketplaceManager />} />
            
            {/* Workflow Management */}
            <Route path="/inventory/workflows" element={<WorkflowManagement />} />
            <Route path="/inventory/workflows/vehicle/:vehicleId" element={<WorkflowManagement />} />
            
            {/* Acquisition Management */}
            <Route path="/inventory/acquisition" element={<AcquisitionManagement />} />
            <Route path="/inventory/acquisition/vehicle/:vehicleId" element={<AcquisitionManagement />} />
            
            <Route path="*" element={<Navigate to="/inventory" replace />} />
          </Routes>
        </Container>
        <Footer />
      </div>
    </Router>
  );
}

export default App;
