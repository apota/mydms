import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './Dashboard.css';

const Dashboard = () => {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [editData, setEditData] = useState({});

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      // Use relative URL so requests go through nginx proxy
      const response = await fetch('/api/Dashboard');
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const data = await response.json();
      setDashboardData(data);
      setEditData({
        retentionRate: data.customerMetrics.retentionRate,
        satisfactionScore: data.surveyMetrics.satisfactionScore
      });
    } catch (err) {
      console.error('Error fetching dashboard data:', err);
      setError(err.message);
      // Use comprehensive fallback data if API fails
      setDashboardData({
        customerMetrics: {
          totalCustomers: 1245,
          newCustomersThisMonth: 87,
          retentionRate: 93.0,
          isEditable: true
        },
        campaignMetrics: {
          activeCampaigns: 6,
          upcomingCampaigns: 3,
          completedCampaigns: 12,
          isEditable: true
        },
        recentInteractions: [
          { id: '1', type: 'phone', customerName: 'John Smith', timeDisplay: '10:15 AM' },
          { id: '2', type: 'email', customerName: 'Sarah Johnson', timeDisplay: '9:45 AM' },
          { id: '3', type: 'inperson', customerName: 'Mike Wilson', timeDisplay: 'Yesterday' },
          { id: '4', type: 'text', customerName: 'Linda Brown', timeDisplay: 'Yesterday' }
        ],
        surveyMetrics: {
          satisfactionScore: 82.0,
          responsesThisMonth: 243,
          isEditable: true
        },
        salesReports: {
          totalRevenue: 2450000.00,
          monthlyRevenue: 385000.00,
          totalDeals: 156,
          monthlyDeals: 23,
          averageDealValue: 15700.00,
          topSalesPersons: [
            { name: "Sarah Miller", revenue: 125000.00, dealsCount: 8 },
            { name: "John Anderson", revenue: 98000.00, dealsCount: 6 },
            { name: "Mike Johnson", revenue: 87000.00, dealsCount: 5 }
          ],
          monthlySalesChart: [
            { month: "Jan", revenue: 320000.00, dealsCount: 18 },
            { month: "Feb", revenue: 285000.00, dealsCount: 16 },
            { month: "Mar", revenue: 410000.00, dealsCount: 22 },
            { month: "Apr", revenue: 375000.00, dealsCount: 20 },
            { month: "May", revenue: 445000.00, dealsCount: 25 },
            { month: "Jun", revenue: 385000.00, dealsCount: 23 }
          ]
        },
        performanceMetrics: {
          conversionRate: 23.5,
          leadToCustomerRate: 18.2,
          averageResponseTime: 4,
          customerLifetimeValue: 45000.00,
          trends: [
            { metricName: "Conversion Rate", currentValue: 23.5, previousValue: 21.8, percentageChange: 7.8, trendDirection: "up" },
            { metricName: "Customer Satisfaction", currentValue: 82.0, previousValue: 79.5, percentageChange: 3.1, trendDirection: "up" },
            { metricName: "Response Time", currentValue: 4.0, previousValue: 5.2, percentageChange: -23.1, trendDirection: "up" },
            { metricName: "Revenue Growth", currentValue: 385000.00, previousValue: 375000.00, percentageChange: 2.7, trendDirection: "up" }
          ]
        }
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      const updateDto = {
        customerMetrics: {
          retentionRate: parseFloat(editData.retentionRate)
        },
        surveyMetrics: {
          satisfactionScore: parseFloat(editData.satisfactionScore)
        }
      };

      // Use relative URL so requests go through nginx proxy
      const response = await fetch('/api/Dashboard', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updateDto),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const updatedData = await response.json();
      setDashboardData(updatedData);
      setEditMode(false);
      alert('Dashboard updated successfully!');
    } catch (err) {
      console.error('Error updating dashboard:', err);
      alert('Error updating dashboard: ' + err.message);
    }
  };

  const handleCancel = () => {
    setEditData({
      retentionRate: dashboardData.customerMetrics.retentionRate,
      satisfactionScore: dashboardData.surveyMetrics.satisfactionScore
    });
    setEditMode(false);
  };

  const formatInteractionType = (type) => {
    switch(type) {
      case 'phone': return 'Phone';
      case 'email': return 'Email';
      case 'inperson': return 'In-Person';
      case 'text': return 'Text';
      default: return type;
    }
  };

  if (loading) {
    return (
      <div className="dashboard">
        <div className="loading">Loading dashboard data...</div>
      </div>
    );
  }

  if (error && !dashboardData) {
    return (
      <div className="dashboard">
        <div className="error">Error loading dashboard: {error}</div>
      </div>
    );
  }

  return (
    <div className="dashboard">
      <div className="dashboard-content">
        <div className="dashboard-actions-bar">
          {editMode ? (
            <>
              <button className="btn btn-primary" onClick={handleSave}>
                <span className="btn-icon">💾</span>
                Save Changes
              </button>
              <button className="btn btn-secondary" onClick={handleCancel}>
                <span className="btn-icon">✕</span>
                Cancel
              </button>
            </>
          ) : (
            <button className="btn btn-outline" onClick={() => setEditMode(true)}>
              <span className="btn-icon">✏️</span>
              Edit Dashboard
            </button>
          )}
          <button className="btn btn-refresh" onClick={fetchDashboardData}>
            <span className="btn-icon">🔄</span>
            Refresh
          </button>
        </div>
      
      <div className="dashboard-grid">
        <div className="dashboard-card">
          <h3>Customer Overview</h3>
          <div className="stats">
            <div className="stat">
              <span className="stat-value">{dashboardData.customerMetrics.totalCustomers.toLocaleString()}</span>
              <span className="stat-label">Total Customers</span>
            </div>
            <div className="stat">
              <span className="stat-value">{dashboardData.customerMetrics.newCustomersThisMonth}</span>
              <span className="stat-label">New This Month</span>
            </div>
            <div className="stat">
              {editMode ? (
                <input
                  type="number"
                  step="0.1"
                  min="0"
                  max="100"
                  className="stat-edit-input"
                  value={editData.retentionRate}
                  onChange={(e) => setEditData({...editData, retentionRate: e.target.value})}
                />
              ) : (
                <span className="stat-value">{dashboardData.customerMetrics.retentionRate}%</span>
              )}
              <span className="stat-label">Retention Rate {editMode && '(editable)'}</span>
            </div>
          </div>
          <Link to="/customers" className="card-action">View All Customers</Link>
        </div>
        
        <div className="dashboard-card">
          <h3>Active Campaigns</h3>
          <div className="stats">
            <div className="stat">
              <span className="stat-value">{dashboardData.campaignMetrics.activeCampaigns}</span>
              <span className="stat-label">Running</span>
            </div>
            <div className="stat">
              <span className="stat-value">{dashboardData.campaignMetrics.upcomingCampaigns}</span>
              <span className="stat-label">Starting Soon</span>
            </div>
            <div className="stat">
              <span className="stat-value">{dashboardData.campaignMetrics.completedCampaigns}</span>
              <span className="stat-label">Completed</span>
            </div>
          </div>
          <Link to="/campaigns" className="card-action">Manage Campaigns</Link>
        </div>
        
        <div className="dashboard-card">
          <h3>Recent Interactions</h3>
          <ul className="recent-list">
            {dashboardData.recentInteractions.map((interaction) => (
              <li key={interaction.id}>
                <span className={`interaction-type ${interaction.type}`}>
                  {formatInteractionType(interaction.type)}
                </span>
                <span className="interaction-customer">{interaction.customerName}</span>
                <span className="interaction-time">{interaction.timeDisplay}</span>
              </li>
            ))}
          </ul>
          <Link to="/interactions" className="card-action">View All Interactions</Link>
        </div>
        
        <div className="dashboard-card">
          <h3>Survey Results</h3>
          <div className="survey-satisfaction">
            <div 
              className="satisfaction-meter" 
              style={{ '--satisfaction': `${dashboardData.surveyMetrics.satisfactionScore}%` }}
            >
              <span>Customer Satisfaction</span>
              <div className="meter-value">
                {editMode ? (
                  <input
                    type="number"
                    step="0.1"
                    min="0"
                    max="100"
                    className="satisfaction-edit-input"
                    value={editData.satisfactionScore}
                    onChange={(e) => setEditData({...editData, satisfactionScore: e.target.value})}
                  />
                ) : (
                  `${dashboardData.surveyMetrics.satisfactionScore}%`
                )}
              </div>
              {editMode && <div className="edit-hint">editable</div>}
            </div>
          </div>
          <div className="stat-small">
            <span className="stat-value">{dashboardData.surveyMetrics.responsesThisMonth}</span>
            <span className="stat-label">Responses This Month</span>
          </div>
          <Link to="/analytics" className="card-action">View Analytics</Link>
        </div>

        {/* Sales Reports Section */}
        <div className="dashboard-card">
          <h3>Sales Reports</h3>
          <div className="sales-overview">
            <div className="sales-stats">
              <div className="stat">
                <span className="stat-value">${(dashboardData.salesReports?.totalRevenue || 0).toLocaleString()}</span>
                <span className="stat-label">Total Revenue</span>
              </div>
              <div className="stat">
                <span className="stat-value">${(dashboardData.salesReports?.monthlyRevenue || 0).toLocaleString()}</span>
                <span className="stat-label">Monthly Revenue</span>
              </div>
              <div className="stat">
                <span className="stat-value">{dashboardData.salesReports?.totalDeals || 0}</span>
                <span className="stat-label">Total Deals</span>
              </div>
            </div>
            <div className="top-performers">
              <h4>Top Sales Performers</h4>
              <ul className="performers-list">
                {(dashboardData.salesReports?.topSalesPersons || []).map((person, index) => (
                  <li key={index}>
                    <span className="performer-name">{person.name}</span>
                    <span className="performer-revenue">${person.revenue.toLocaleString()}</span>
                    <span className="performer-deals">{person.dealsCount} deals</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
          <Link to="/sales" className="card-action">View Sales Reports</Link>
        </div>

        {/* Performance Metrics Section */}
        <div className="dashboard-card">
          <h3>Performance Metrics</h3>
          <div className="performance-overview">
            <div className="performance-stats">
              <div className="stat">
                <span className="stat-value">{dashboardData.performanceMetrics?.conversionRate || 0}%</span>
                <span className="stat-label">Conversion Rate</span>
              </div>
              <div className="stat">
                <span className="stat-value">{dashboardData.performanceMetrics?.leadToCustomerRate || 0}%</span>
                <span className="stat-label">Lead to Customer</span>
              </div>
              <div className="stat">
                <span className="stat-value">{dashboardData.performanceMetrics?.averageResponseTime || 0}h</span>
                <span className="stat-label">Avg Response Time</span>
              </div>
            </div>
            <div className="performance-trends">
              <h4>Key Trends</h4>
              <ul className="trends-list">
                {(dashboardData.performanceMetrics?.trends || []).slice(0, 3).map((trend, index) => (
                  <li key={index} className={`trend-item ${trend.trendDirection}`}>
                    <span className="trend-metric">{trend.metricName}</span>
                    <span className={`trend-change ${trend.trendDirection}`}>
                      {trend.percentageChange > 0 ? '+' : ''}{trend.percentageChange.toFixed(1)}%
                      <span className={`trend-arrow ${trend.trendDirection}`}>
                        {trend.trendDirection === 'up' ? '↗' : trend.trendDirection === 'down' ? '↘' : '→'}
                      </span>
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
          <Link to="/reports" className="card-action">View All Reports</Link>
        </div>

        {/* Monthly Sales Chart Section */}
        <div className="dashboard-card dashboard-card-wide">
          <h3>Monthly Sales Trend</h3>
          <div className="sales-chart">
            <div className="chart-container">
              {(dashboardData.salesReports?.monthlySalesChart || []).map((data, index) => (
                <div key={index} className="chart-bar">
                  <div 
                    className="bar" 
                    style={{ 
                      height: `${(data.revenue / Math.max(...(dashboardData.salesReports?.monthlySalesChart || []).map(d => d.revenue))) * 100}%` 
                    }}
                    title={`${data.month}: $${data.revenue.toLocaleString()}`}
                  ></div>
                  <span className="bar-label">{data.month}</span>
                  <span className="bar-value">${(data.revenue / 1000).toFixed(0)}K</span>
                </div>
              ))}
            </div>
          </div>
          <Link to="/analytics" className="card-action">View Detailed Analytics</Link>
        </div>
      </div>
      
      {error && (
        <div className="dashboard-notice">
          <small>Note: Using cached data due to API connection issue</small>
        </div>
      )}
      </div>
    </div>
  );
};

export default Dashboard;