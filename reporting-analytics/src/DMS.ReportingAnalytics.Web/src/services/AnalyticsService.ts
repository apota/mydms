import axios from 'axios';
import type { 
  KpiResult, 
  TrendResult, 
  ForecastResult, 
  ForecastRequest, 
  ComparisonResult,
  AdHocQueryRequest,
  AdHocQueryResult,
  Insight,
  InventoryRecommendation,
  CustomerChurnPrediction
} from '../types/analyticsTypes';

/**
 * Service for interacting with the Analytics API
 */
class AnalyticsService {
  private api: any;
  
  constructor() {
    const baseURL = window.ENV?.REACT_APP_API_URL || 'http://localhost:5000';
    
    this.api = axios.create({
      baseURL: `${baseURL}/api/analytics`,
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    // Add request interceptor for authentication if needed
    this.api.interceptors.request.use((config: any) => {
      const token = localStorage.getItem('auth_token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }
  
  /**
   * Gets KPIs for a department or all departments
   * @param department Department name or "all"
   */
  async getKpis(department: string = 'all'): Promise<KpiResult[]> {
    const response = await this.api.get(`/kpis?department=${department}`);
    return response.data;
  }
  
  /**
   * Gets trend analysis for a specific metric
   * @param metricId Metric identifier
   * @param timeFrame Time granularity (day, week, month, quarter, year)
   * @param compareWith Optional comparison period
   */
  async getTrend(metricId: string, timeFrame: string = 'month', compareWith?: string): Promise<TrendResult> {
    let url = `/trends/${metricId}?timeFrame=${timeFrame}`;
    if (compareWith) {
      url += `&compareWith=${compareWith}`;
    }
    
    const response = await this.api.get(url);
    return response.data;
  }
  
  /**
   * Generates a forecast based on historical data
   * @param request Forecast request parameters
   */
  async generateForecast(request: ForecastRequest): Promise<ForecastResult> {
    const response = await this.api.post('/forecast', request);
    return response.data;
  }
  
  /**
   * Gets period-over-period comparisons for a group of metrics
   * @param metricGroup Group of metrics to compare
   * @param currentPeriod Current period identifier
   * @param previousPeriod Previous period identifier
   */
  async getComparison(metricGroup: string, currentPeriod: string, previousPeriod: string): Promise<ComparisonResult> {
    const url = `/comparisons?metricGroup=${metricGroup}&currentPeriod=${currentPeriod}&previousPeriod=${previousPeriod}`;
    const response = await this.api.get(url);
    return response.data;
  }
  
  /**
   * Executes an ad-hoc analytics query
   * @param request Query parameters
   */
  async executeAdHocQuery(request: AdHocQueryRequest): Promise<AdHocQueryResult> {
    const response = await this.api.post('/ad-hoc', request);
    return response.data;
  }
  
  /**
   * Gets automated insights from data
   * @param area Business area or "all"
   * @param maxResults Maximum number of insights to return
   */
  async getInsights(area: string = 'all', maxResults: number = 10): Promise<Insight[]> {
    const url = `/insights?area=${area}&maxResults=${maxResults}`;
    const response = await this.api.get(url);
    return response.data;
  }
  
  /**
   * Gets inventory optimization recommendations
   */
  async getInventoryRecommendations(): Promise<InventoryRecommendation[]> {
    const response = await this.api.get('/recommendations/inventory');
    return response.data;
  }
  
  /**
   * Gets customer churn predictions
   * @param minRiskScore Minimum risk score threshold (0-1)
   */
  async getCustomerChurnPredictions(minRiskScore: number = 0.5): Promise<CustomerChurnPrediction[]> {
    const response = await this.api.get(`/predictions/customer-churn?minRiskScore=${minRiskScore}`);
    return response.data;
  }
}

export default AnalyticsService;
