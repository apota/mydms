/**
 * Types for the analytics components and services
 */

export interface KpiResult {
  kpiId: string;
  name: string;
  value: number;
  previousValue?: number;
  changePercent?: number;
  trend: string; // up, down, flat
  unit: string; // currency, percent, count
  department: string;
}

export interface TrendPoint {
  date: string;
  value: number;
}

export interface TrendResult {
  metricId: string;
  metricName: string;
  timeFrame: string;
  points: TrendPoint[];
  comparisonPoints?: TrendPoint[];
}

export interface ForecastRequest {
  metricName: string;
  timeGranularity: string; // day, week, month
  periods: number;
  filter?: string;
}

export interface ForecastPoint {
  date: string;
  value: number;
  lowerBound?: number;
  upperBound?: number;
}

export interface ForecastResult {
  metricName: string;
  points: ForecastPoint[];
  confidenceLevel: number;
}

export interface MetricComparison {
  metricId: string;
  metricName: string;
  currentValue: number;
  previousValue: number;
  changePercent: number;
  trend: string; // up, down, flat
}

export interface ComparisonResult {
  metricGroup: string;
  currentPeriod: string;
  previousPeriod: string;
  metrics: MetricComparison[];
}

export interface AdHocQueryRequest {
  dataMartName: string;
  dimensions: string[];
  measures: string[];
  filter?: string;
  sortBy?: string[];
  limit?: number;
}

export interface AdHocQueryResult {
  columns: string[];
  rows: any[];
  totalCount: number;
}

export interface InsightDataPoint {
  label: string;
  value: number;
}

export interface Insight {
  insightId: string;
  title: string;
  description: string;
  category: string;
  discoveredDate: string;
  significance: number; // 0-1 scale
  dataPoints?: InsightDataPoint[];
  recommendedAction?: string;
}

export interface InventoryRecommendation {
  make: string;
  model: string;
  year: number;
  currentStock: number;
  recommendedStock: number;
  stockDelta: number;
  action: string; // Increase, Decrease, Maintain
  salesVelocity: number;
  daysSupply: number;
}

export interface CustomerChurnPrediction {
  customerId: string;
  customerName: string;
  churnRiskScore: number;
  riskCategory: string; // High, Medium, Low
  lifetimeValue: number;
  daysSinceLastPurchase: number;
  churnFactors?: string[];
  recommendedActions?: string[];
}
