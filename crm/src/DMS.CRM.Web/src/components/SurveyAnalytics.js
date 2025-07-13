import React, { useState, useEffect } from 'react';
import { CustomerSurveyService } from '../services/api-services';

const SurveyAnalytics = () => {
  const [analyticsData, setAnalyticsData] = useState(null);
  const [trendData, setTrendData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [timeRange, setTimeRange] = useState('last6months'); // 'last30days', 'last6months', 'lastyear', 'custom'
  const [timeInterval, setTimeInterval] = useState('month'); // 'day', 'week', 'month', 'quarter', 'year'
  const [customDateRange, setCustomDateRange] = useState({
    startDate: new Date(new Date().setMonth(new Date().getMonth() - 6)),
    endDate: new Date()
  });

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        setLoading(true);
        // Calculate date range based on selection
        let startDate, endDate;
        
        switch (timeRange) {
          case 'last30days':
            startDate = new Date();
            startDate.setDate(startDate.getDate() - 30);
            endDate = new Date();
            break;
          case 'last6months':
            startDate = new Date();
            startDate.setMonth(startDate.getMonth() - 6);
            endDate = new Date();
            break;
          case 'lastyear':
            startDate = new Date();
            startDate.setFullYear(startDate.getFullYear() - 1);
            endDate = new Date();
            break;
          case 'custom':
            startDate = customDateRange.startDate;
            endDate = customDateRange.endDate;
            break;
          default:
            startDate = new Date();
            startDate.setMonth(startDate.getMonth() - 6);
            endDate = new Date();
        }
        
        // Fetch analytics data and satisfaction trend
        const [analytics, trend] = await Promise.all([
          CustomerSurveyService.getAnalytics(startDate, endDate),
          CustomerSurveyService.getSatisfactionTrend(startDate, endDate, timeInterval)
        ]);
        
        setAnalyticsData(analytics);
        setTrendData(trend);
        setLoading(false);
      } catch (err) {
        setError('Failed to fetch survey analytics');
        setLoading(false);
        console.error('Error fetching survey analytics:', err);
      }
    };

    fetchAnalytics();
  }, [timeRange, timeInterval, customDateRange]);

  if (loading) return <div className="loading">Loading survey analytics...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="survey-analytics-container">
      <h2>Customer Survey Analytics</h2>
      
      <div className="control-panel">
        <div className="time-range-selector">
          <label>Time Range:</label>
          <select value={timeRange} onChange={(e) => setTimeRange(e.target.value)}>
            <option value="last30days">Last 30 Days</option>
            <option value="last6months">Last 6 Months</option>
            <option value="lastyear">Last Year</option>
            <option value="custom">Custom Range</option>
          </select>
          
          {timeRange === 'custom' && (
            <div className="custom-date-range">
              <div>
                <label>Start Date:</label>
                <input 
                  type="date" 
                  value={customDateRange.startDate.toISOString().split('T')[0]}
                  onChange={(e) => setCustomDateRange({
                    ...customDateRange,
                    startDate: new Date(e.target.value)
                  })}
                />
              </div>
              <div>
                <label>End Date:</label>
                <input 
                  type="date"
                  value={customDateRange.endDate.toISOString().split('T')[0]}
                  onChange={(e) => setCustomDateRange({
                    ...customDateRange,
                    endDate: new Date(e.target.value)
                  })}
                />
              </div>
            </div>
          )}
        </div>
        
        <div className="time-interval-selector">
          <label>Trend Interval:</label>
          <select value={timeInterval} onChange={(e) => setTimeInterval(e.target.value)}>
            <option value="day">Daily</option>
            <option value="week">Weekly</option>
            <option value="month">Monthly</option>
            <option value="quarter">Quarterly</option>
            <option value="year">Yearly</option>
          </select>
        </div>
      </div>
      
      {analyticsData && (
        <div className="analytics-metrics">
          <div className="metric-card">
            <h3>Total Surveys</h3>
            <div className="metric-value">{analyticsData.totalSurveys}</div>
          </div>
          <div className="metric-card">
            <h3>Total Responses</h3>
            <div className="metric-value">{analyticsData.totalResponses}</div>
          </div>
          <div className="metric-card">
            <h3>Response Rate</h3>
            <div className="metric-value">{(analyticsData.responseRate * 100).toFixed(1)}%</div>
          </div>
          <div className="metric-card">
            <h3>Average Satisfaction</h3>
            <div className="metric-value">{analyticsData.averageSatisfaction.toFixed(1)} / 5</div>
          </div>
          <div className="metric-card">
            <h3>Completion Rate</h3>
            <div className="metric-value">{analyticsData.completionRate.toFixed(1)}%</div>
          </div>
        </div>
      )}
      
      {trendData && (
        <div className="satisfaction-trend-chart">
          <h3>Satisfaction Trend</h3>
          <div className="chart-placeholder">
            {/* This would be replaced by a real chart component in a production app */}
            <p>Chart showing satisfaction trend over time would be displayed here</p>
            <pre>{JSON.stringify(trendData, null, 2)}</pre>
          </div>
        </div>
      )}
      
      {analyticsData?.topSurveysByResponses && (
        <div className="top-surveys">
          <h3>Top Surveys by Response Count</h3>
          <ul>
            {Object.entries(analyticsData.topSurveysByResponses).map(([name, count], index) => (
              <li key={index}>
                <span className="survey-name">{name}</span>
                <span className="response-count">{count} responses</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

export default SurveyAnalytics;
