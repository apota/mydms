/**
 * Types for Vehicle Management
 */

// Basic vehicle information
export interface IVehicle {
  id: string;
  vin: string;
  stockNumber: string;
  year: number;
  make: string;
  model: string;
  trim: string;
  bodyStyle: string;
  exteriorColor: string;
  interiorColor: string;
  mileage: number;
  retailPrice: number;
  costPrice: number;
  listPrice: number;
  condition: VehicleCondition;
  status: VehicleStatus;
  location: VehicleLocation;
  isReserved: boolean;
  daysInInventory: number;
  createdAt: string;
  updatedAt: string;
  thumbnailUrl?: string;
  mainImageUrl?: string;
}

// Vehicle details extend the basic information
export interface IVehicleDetails extends IVehicle {
  engine: string;
  transmission: string;
  fuelType: string;
  driveType: string;
  description: string;
  features: string[];
  options: string[];
  comments: string;
  acquisitionDate: string;
  acquisitionSource: string;
  reconCost: number;
  totalCost: number;
  targetMargin: number;
  keyLocation: string;
  reconStatus: ReconStatus;
  imageUrls: string[];
  documentIds: string[];
  certification?: string;
  warranty?: string;
  serviceHistory?: string[];
}

// Condition of the vehicle
export enum VehicleCondition {
  New = 'New',
  Used = 'Used',
  Certified = 'Certified',
  Demo = 'Demo',
  Lemon = 'Lemon'
}

// Current status in the inventory
export enum VehicleStatus {
  Available = 'Available',
  Sold = 'Sold',
  InTransit = 'InTransit',
  OnOrder = 'OnOrder',
  InRecon = 'InRecon',
  OnHold = 'OnHold',
  Pending = 'Pending',
  Transferred = 'Transferred'
}

// Reconditioning status
export enum ReconStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Delayed = 'Delayed',
  NotRequired = 'NotRequired'
}

// Location within the dealership or offsite
export interface VehicleLocation {
  id: string;
  name: string;
  type: LocationType;
  address?: string;
}

// Type of location
export enum LocationType {
  MainLot = 'MainLot',
  ServiceLot = 'ServiceLot',
  ShowroomFloor = 'ShowroomFloor',
  OffSite = 'OffSite',
  Body = 'Body',
  Auction = 'Auction'
}

// Image information
export interface IVehicleImage {
  id: string;
  vehicleId: string;
  url: string;
  thumbnailUrl: string;
  position: number;
  isPrimary: boolean;
  title: string;
  uploadedAt: string;
}

// Document information
export interface IVehicleDocument {
  id: string;
  vehicleId: string;
  fileName: string;
  fileType: string;
  url: string;
  documentType: DocumentType;
  uploadedAt: string;
  size: number;
}

// Type of document
export enum DocumentType {
  Title = 'Title',
  Registration = 'Registration',
  Insurance = 'Insurance',
  InspectionReport = 'InspectionReport',
  ServiceRecord = 'ServiceRecord',
  CarfaxReport = 'CarfaxReport',
  BillOfSale = 'BillOfSale',
  Other = 'Other'
}

// For vehicle search and filtering
export interface VehicleSearchParams {
  make?: string;
  model?: string;
  yearMin?: number;
  yearMax?: number;
  priceMin?: number;
  priceMax?: number;
  mileageMin?: number;
  mileageMax?: number;
  condition?: VehicleCondition;
  status?: VehicleStatus;
  bodyStyle?: string;
  color?: string;
  location?: string;
  daysInInventoryMin?: number;
  daysInInventoryMax?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}

// For vehicle transfers
export interface VehicleTransferRequest {
  vehicleId: string;
  destinationLocationId: string;
  transferReason: string;
  transferDate: string;
  notes?: string;
}

// For status updates
export interface VehicleStatusUpdateRequest {
  vehicleId: string;
  newStatus: VehicleStatus;
  reason: string;
  notes?: string;
}

// For bulk import results
export interface ImportResult {
  totalProcessed: number;
  successCount: number;
  failureCount: number;
  errors: ImportError[];
}

// For individual import errors
export interface ImportError {
  rowNumber: number;
  message: string;
  data?: any;
}

// Analytics interfaces
export interface AgingStats {
  totalVehicles: number;
  averageDaysInInventory: number;
  agingDistribution: AgingDistribution[];
  costOfAging: number;
}

export interface AgingDistribution {
  daysRange: string;
  count: number;
  percentage: number;
  totalValue: number;
}

export interface ValuationSummary {
  totalInventoryValue: number;
  totalCostValue: number;
  totalListValue: number;
  potentialGrossProfit: number;
  averagePerVehicleValue: number;
  valuationByCategory: CategoryValuation[];
}

export interface CategoryValuation {
  category: string;
  count: number;
  totalValue: number;
  averageValue: number;
  percentOfInventory: number;
}

export interface TurnoverMetric {
  period: string;
  turnoverRate: number;
  salesCount: number;
  averageInventorySize: number;
  targetTurnoverRate: number;
  variance: number;
}

export interface InventoryMixAnalysis {
  categories: MixCategory[];
  recommendations: MixRecommendation[];
}

export interface MixCategory {
  category: string;
  count: number;
  percentage: number;
  averageDaysToSell: number;
  profitMargin: number;
  demandScore: number;
}

export interface MixRecommendation {
  category: string;
  currentCount: number;
  recommendedCount: number;
  action: 'Increase' | 'Decrease' | 'Maintain';
  reason: string;
}

export interface PriceCompetitiveness {
  vehicleId: string;
  vin: string;
  stockNumber: string;
  description: string;
  yourPrice: number;
  marketAveragePrice: number;
  priceDifference: number;
  percentageDifference: number;
  competitivePosition: 'Above Market' | 'At Market' | 'Below Market';
  similarListings: SimilarListing[];
}

export interface SimilarListing {
  source: string;
  price: number;
  mileage: number;
  daysOnMarket: number;
  distance: number;
  url?: string;
}
