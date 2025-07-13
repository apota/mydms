import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';

// Layout components
import Layout from './components/common/Layout';

// Pages
import Dashboard from './pages/Dashboard';
import PartsList from './pages/parts/PartsList';
import PartDetails from './pages/parts/PartDetails';
import InventoryList from './pages/inventory/InventoryList';
import InventoryDetail from './pages/inventory/InventoryDetail';
import OrdersList from './pages/orders/OrdersList';
import OrderDetail from './pages/orders/OrderDetail';
import CreateOrder from './pages/orders/CreateOrder';
import SuppliersList from './pages/suppliers/SuppliersList';
import SupplierDetails from './pages/suppliers/SupplierDetails';
import TransactionsList from './pages/transactions/TransactionsList';
import TransactionDetail from './pages/transactions/TransactionDetail';
import IssuePartsForm from './pages/transactions/IssuePartsForm';
import TransferPartsForm from './pages/transactions/TransferPartsForm';
import CoreTrackingList from './pages/core-tracking/CoreTrackingList';
import CoreTrackingDetail from './pages/core-tracking/CoreTrackingDetail';
import TrackNewCore from './pages/core-tracking/TrackNewCore';
import NotFound from './pages/NotFound';

// Context providers
import { AuthProvider } from './context/AuthContext';
import { NotificationProvider } from './context/NotificationContext';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <NotificationProvider>
        <AuthProvider>
          <Router>
            <Routes>
              <Route path="/" element={<Layout />}>
                <Route index element={<Dashboard />} />
                <Route path="parts">
                  <Route index element={<PartsList />} />
                  <Route path=":id" element={<PartDetails />} />
                </Route>                <Route path="inventory">
                  <Route index element={<InventoryList />} />
                  <Route path=":partId/location/:locationId" element={<InventoryDetail />} />
                </Route>
                <Route path="orders">
                  <Route index element={<OrdersList />} />
                  <Route path="new" element={<CreateOrder />} />
                  <Route path=":id" element={<OrderDetail />} />
                </Route>
                <Route path="suppliers">
                  <Route index element={<SuppliersList />} />
                  <Route path=":id" element={<SupplierDetails />} />
                </Route>                <Route path="transactions">
                  <Route index element={<TransactionsList />} />
                  <Route path="issue" element={<IssuePartsForm />} />
                  <Route path="transfer" element={<TransferPartsForm />} />
                  <Route path=":id" element={<TransactionDetail />} />
                </Route><Route path="core-tracking">
                  <Route index element={<CoreTrackingList />} />
                  <Route path="track" element={<TrackNewCore />} />
                  <Route path=":id" element={<CoreTrackingDetail />} />
                </Route>
                <Route path="*" element={<NotFound />} />
              </Route>
            </Routes>
          </Router>
        </AuthProvider>
      </NotificationProvider>
    </ThemeProvider>
  );
}

export default App;
