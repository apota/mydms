using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Core.Services
{
    public class InventoryAnalyticsService : IInventoryAnalyticsService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehicleCostRepository _costRepository;
        private readonly IVehiclePricingRepository _pricingRepository;
        private readonly IVehicleAgingRepository _agingRepository;
        private readonly IExternalMarketDataService _marketDataService;
        private readonly ILogger<InventoryAnalyticsService> _logger;

        public InventoryAnalyticsService(
            IVehicleRepository vehicleRepository,
            IVehicleCostRepository costRepository,
            IVehiclePricingRepository pricingRepository,
            IVehicleAgingRepository agingRepository,
            IExternalMarketDataService marketDataService,
            ILogger<InventoryAnalyticsService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _costRepository = costRepository;
            _pricingRepository = pricingRepository;
            _agingRepository = agingRepository;
            _marketDataService = marketDataService;
            _logger = logger;
        }

        public async Task<AgingAnalyticsDto> GetAgingAnalyticsAsync(
            List<Guid>? locationIds,
            List<string>? vehicleTypes,
            DateRangeDto? dateRange)
        {
            try
            {
                // Get vehicles filtered by location and type
                var vehicles = await _vehicleRepository.GetVehiclesWithAgingDataAsync(
                    locationIds,
                    vehicleTypes);

                // Create aging brackets (0-15 days, 16-30, 31-45, 46-60, 61+)
                var result = new AgingAnalyticsDto
                {
                    AgingBrackets = new Dictionary<string, AgingBracketDto>
                    {
                        ["0-15"] = new AgingBracketDto(),
                        ["16-30"] = new AgingBracketDto(),
                        ["31-45"] = new AgingBracketDto(),
                        ["46-60"] = new AgingBracketDto(),
                        ["61+"] = new AgingBracketDto()
                    }
                };

                // Group vehicles into aging brackets and calculate metrics
                foreach (var vehicle in vehicles)
                {
                    var daysInInventory = (DateTime.UtcNow - vehicle.ListingDate).Days;
                    var agingData = await _agingRepository.GetByVehicleIdAsync(vehicle.Id);
                    var pricingData = await _pricingRepository.GetByVehicleIdAsync(vehicle.Id);
                    var costData = await _costRepository.GetByVehicleIdAsync(vehicle.Id);

                    var vehicleSummary = new VehicleSummaryDto
                    {
                        Id = vehicle.Id,
                        StockNumber = vehicle.StockNumber,
                        Vin = vehicle.Vin,
                        Make = vehicle.Make,
                        Model = vehicle.Model,
                        Year = vehicle.Year.ToString(),
                        Trim = vehicle.Trim,
                        DaysInInventory = daysInInventory,
                        ListPrice = pricingData?.InternetPrice ?? 0,
                        Cost = costData?.TotalCost ?? 0
                    };

                    // Add to appropriate bracket
                    string bracketKey;
                    if (daysInInventory <= 15) bracketKey = "0-15";
                    else if (daysInInventory <= 30) bracketKey = "16-30";
                    else if (daysInInventory <= 45) bracketKey = "31-45";
                    else if (daysInInventory <= 60) bracketKey = "46-60";
                    else bracketKey = "61+";

                    result.AgingBrackets[bracketKey].Total++;
                    result.AgingBrackets[bracketKey].TotalValue += pricingData?.InternetPrice ?? 0;
                    result.AgingBrackets[bracketKey].Vehicles.Add(vehicleSummary);

                    // Add critical alerts for vehicles over threshold
                    if (daysInInventory > 45)
                    {
                        var suggestedPrice = await CalculateSuggestedPriceForAgingVehicleAsync(vehicle, pricingData, daysInInventory);
                        
                        result.CriticalAlerts.Add(new AgingAlertDto
                        {
                            VehicleId = vehicle.Id,
                            StockNumber = vehicle.StockNumber,
                            Description = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} {vehicle.Trim}",
                            DaysInInventory = daysInInventory,
                            CurrentPrice = pricingData?.InternetPrice ?? 0,
                            SuggestedPrice = suggestedPrice,
                            RecommendedAction = GetRecommendedAction(daysInInventory, vehicle.Condition)
                        });
                    }
                }

                // Generate heat map data
                result.HeatMapData = await GenerateAgingHeatMapDataAsync(vehicles);
                
                // Generate trend data
                result.TrendData = await GenerateAgingTrendDataAsync(dateRange);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating aging analytics");
                throw;
            }
        }

        public async Task<InventoryValuationDto> GetInventoryValuationAsync(
            string groupBy,
            List<Guid>? locationIds,
            bool includeSold)
        {
            try
            {
                // Get all vehicles including their costs and prices
                var vehicles = await _vehicleRepository.GetVehiclesWithCostAndPricingDataAsync(
                    locationIds,
                    includeSold ? null : new List<string> { "Sold" });

                var result = new InventoryValuationDto
                {
                    ValuationByGroup = new Dictionary<string, ValuationGroupDto>()
                };

                // Calculate overall totals
                foreach (var vehicle in vehicles)
                {
                    var cost = vehicle.VehicleCost?.TotalCost ?? 0;
                    var retailValue = vehicle.VehiclePricing?.InternetPrice ?? 0;
                    
                    result.TotalCost += cost;
                    result.TotalRetailValue += retailValue;
                    
                    // Group by the requested property
                    string groupKey = GetGroupKeyByProperty(vehicle, groupBy);
                    
                    if (!result.ValuationByGroup.ContainsKey(groupKey))
                    {
                        result.ValuationByGroup[groupKey] = new ValuationGroupDto
                        {
                            Name = groupKey
                        };
                    }
                    
                    var group = result.ValuationByGroup[groupKey];
                    group.VehicleCount++;
                    group.TotalCost += cost;
                    group.TotalRetailValue += retailValue;
                }

                // Calculate potential gross profit and averages
                result.PotentialGrossProfit = result.TotalRetailValue - result.TotalCost;
                
                // Calculate group averages
                foreach (var group in result.ValuationByGroup.Values)
                {
                    if (group.VehicleCount > 0)
                    {
                        group.AverageCost = group.TotalCost / group.VehicleCount;
                        group.AverageRetailValue = group.TotalRetailValue / group.VehicleCount;
                    }
                }

                // Generate trend data for last 12 months
                result.TrendData = await GenerateValuationTrendDataAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory valuation");
                throw;
            }
        }

        public async Task<TurnoverMetricsDto> GetTurnoverMetricsAsync(
            string timePeriod,
            List<string>? vehicleTypes,
            List<Guid>? locationIds)
        {
            try
            {
                // Get sold vehicles for the time period along with inventory snapshots
                var soldVehicles = await _vehicleRepository.GetSoldVehiclesWithDaysToSellAsync(
                    GetStartDateForTimePeriod(timePeriod),
                    DateTime.UtcNow,
                    vehicleTypes,
                    locationIds);
                
                var currentInventory = await _vehicleRepository.GetVehiclesWithAgingDataAsync(
                    locationIds,
                    vehicleTypes);

                var result = new TurnoverMetricsDto();
                
                // Calculate overall metrics
                if (soldVehicles.Any())
                {
                    result.AverageDaysToSell = soldVehicles.Average(v => v.DaysToSell);
                    
                    // Turnover Rate = Number of Vehicles Sold / Average Inventory Level
                    double averageInventory = await GetAverageInventoryLevelAsync(
                        GetStartDateForTimePeriod(timePeriod),
                        DateTime.UtcNow,
                        vehicleTypes,
                        locationIds);
                    
                    result.TurnoverRate = averageInventory > 0
                        ? soldVehicles.Count / averageInventory
                        : 0;
                }
                
                // Calculate turnover by segment
                var segments = GetSegmentsForTimePeriod(timePeriod);
                foreach (var segment in segments)
                {
                    var segmentSoldVehicles = soldVehicles
                        .Where(v => MatchesSegment(v.Vehicle, segment))
                        .ToList();
                    
                    var segmentCurrentInventory = currentInventory
                        .Count(v => MatchesSegment(v, segment));
                    
                    var averageSegmentInventory = await GetAverageInventoryLevelForSegmentAsync(
                        GetStartDateForTimePeriod(timePeriod),
                        DateTime.UtcNow,
                        segment,
                        vehicleTypes,
                        locationIds);
                    
                    result.BySegment.Add(new TurnoverBySegmentDto
                    {
                        Segment = segment,
                        AverageDaysToSell = segmentSoldVehicles.Any()
                            ? segmentSoldVehicles.Average(v => v.DaysToSell)
                            : 0,
                        TurnoverRate = averageSegmentInventory > 0
                            ? segmentSoldVehicles.Count / averageSegmentInventory
                            : 0,
                        VehiclesSold = segmentSoldVehicles.Count,
                        CurrentInventory = segmentCurrentInventory
                    });
                }
                
                // Generate trend data
                result.TrendData = await GenerateTurnoverTrendDataAsync(timePeriod, vehicleTypes, locationIds);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating turnover metrics");
                throw;
            }
        }

        public async Task<InventoryMixAnalysisDto> GetInventoryMixAnalysisAsync(
            string groupingFactor,
            string comparisonPeriod)
        {
            try
            {
                // Get current inventory
                var currentInventory = await _vehicleRepository.GetAllActiveVehiclesAsync();
                
                // Get comparison inventory from the past
                DateTime comparisonDate = GetComparisonDate(comparisonPeriod);
                var historicalInventory = await _vehicleRepository.GetInventorySnapshotAtDateAsync(comparisonDate);
                
                // Get sales data for analysis
                var salesData = await _vehicleRepository.GetSalesDataByPeriodAsync(
                    comparisonDate,
                    DateTime.UtcNow);
                
                var result = new InventoryMixAnalysisDto();
                
                // Create current mix analysis
                var currentGroups = currentInventory
                    .GroupBy(v => GetGroupKeyByProperty(v, groupingFactor))
                    .ToList();
                
                int totalCurrentCount = currentInventory.Count;
                
                foreach (var group in currentGroups)
                {
                    double percentage = totalCurrentCount > 0
                        ? (double)group.Count() / totalCurrentCount * 100
                        : 0;
                    
                    result.CurrentMix.Add(new InventoryMixItemDto
                    {
                        Group = group.Key,
                        Count = group.Count(),
                        Percentage = percentage,
                        AverageTurnover = await GetAverageTurnoverForGroupAsync(group.Key, groupingFactor)
                    });
                }
                
                // Create comparison mix analysis
                var comparisonGroups = historicalInventory
                    .GroupBy(v => GetGroupKeyByProperty(v, groupingFactor))
                    .ToList();
                
                int totalComparisonCount = historicalInventory.Count;
                
                foreach (var group in comparisonGroups)
                {
                    double percentage = totalComparisonCount > 0
                        ? (double)group.Count() / totalComparisonCount * 100
                        : 0;
                    
                    // Find the same group in current mix to calculate percentage change
                    var currentGroup = result.CurrentMix.FirstOrDefault(m => m.Group == group.Key);
                    double percentageChange = 0;
                    
                    if (currentGroup != null && percentage > 0)
                    {
                        percentageChange = (currentGroup.Percentage - percentage) / percentage * 100;
                    }
                    
                    result.ComparisonMix.Add(new InventoryMixItemDto
                    {
                        Group = group.Key,
                        Count = group.Count(),
                        Percentage = percentage,
                        PercentageChange = percentageChange,
                        AverageTurnover = await GetAverageTurnoverForGroupAsync(group.Key, groupingFactor)
                    });
                }
                
                // Generate sales trend data
                result.SalesTrendByGroup = await GenerateSalesTrendByGroupAsync(groupingFactor, salesData);
                
                // Generate recommendations based on analysis
                result.Recommendations = GenerateInventoryMixRecommendations(
                    result.CurrentMix,
                    result.ComparisonMix,
                    salesData,
                    groupingFactor);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory mix analysis");
                throw;
            }
        }

        public async Task<PriceCompetitivenessDto> GetPriceCompetitivenessAsync(
            List<Guid>? vehicleIds,
            int marketRadius)
        {
            try
            {
                // Get vehicles to analyze
                var vehicles = vehicleIds != null && vehicleIds.Any()
                    ? await _vehicleRepository.GetVehiclesByIdsAsync(vehicleIds)
                    : await _vehicleRepository.GetAllActiveVehiclesAsync();
                
                var result = new PriceCompetitivenessDto
                {
                    MarketSummary = new MarketSummaryDto
                    {
                        InventoryByCompetitor = new Dictionary<string, int>(),
                        PriceRangeByTrim = new Dictionary<string, decimal>()
                    }
                };
                
                // For each vehicle, get market data and analyze price competitiveness
                foreach (var vehicle in vehicles)
                {
                    var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicle.Id);
                    if (pricing == null) continue;
                    
                    var currentPrice = pricing.InternetPrice;
                    var description = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} {vehicle.Trim}";
                    
                    // Get competitive market data for similar vehicles
                    var marketData = await _marketDataService.GetCompetitiveListingsAsync(
                        vehicle.Year,
                        vehicle.Make,
                        vehicle.Model,
                        vehicle.Trim,
                        vehicle.Mileage,
                        marketRadius);
                    
                    if (marketData == null || !marketData.Listings.Any())
                    {
                        // No competitive data available
                        result.VehicleAnalysis.Add(new VehicleCompetitivenessDto
                        {
                            VehicleId = vehicle.Id,
                            StockNumber = vehicle.StockNumber,
                            Description = description,
                            CurrentPrice = currentPrice,
                            PricePosition = "Unknown",
                            CompetitorCount = 0
                        });
                        continue;
                    }
                    
                    var listings = marketData.Listings;
                    
                    // Calculate market statistics
                    decimal avgPrice = listings.Average(l => l.Price);
                    decimal minPrice = listings.Min(l => l.Price);
                    decimal maxPrice = listings.Max(l => l.Price);
                    
                    // Determine price position percentile
                    var sortedPrices = listings.Select(l => l.Price).OrderBy(p => p).ToList();
                    int position = sortedPrices.FindIndex(p => p >= currentPrice);
                    int percentile = position >= 0
                        ? (int)Math.Round((double)position / sortedPrices.Count * 100)
                        : 100;
                    
                    string pricePosition = DeterminePricePosition(currentPrice, avgPrice, percentile);
                    int daysOnMarket = (int)(DateTime.UtcNow - vehicle.ListingDate).TotalDays;
                    
                    // Add to vehicle analysis
                    result.VehicleAnalysis.Add(new VehicleCompetitivenessDto
                    {
                        VehicleId = vehicle.Id,
                        StockNumber = vehicle.StockNumber,
                        Description = description,
                        CurrentPrice = currentPrice,
                        AverageMarketPrice = avgPrice,
                        MinMarketPrice = minPrice,
                        MaxMarketPrice = maxPrice,
                        PercentilePosition = percentile,
                        PricePosition = pricePosition,
                        DaysOnMarket = daysOnMarket,
                        CompetitorCount = listings.Count
                    });
                    
                    // Update market summary data
                    UpdateMarketSummaryData(result.MarketSummary, listings, marketData.AverageDaysOnMarket, vehicle);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing price competitiveness");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task<decimal> CalculateSuggestedPriceForAgingVehicleAsync(
            Vehicle vehicle,
            VehiclePricing? pricing,
            int daysInInventory)
        {
            if (pricing == null) return 0;
            
            // Base suggestion on market data and days in inventory
            var marketData = await _marketDataService.GetCompetitiveListingsAsync(
                vehicle.Year,
                vehicle.Make,
                vehicle.Model,
                vehicle.Trim,
                vehicle.Mileage,
                50); // Default 50 mile radius
            
            if (marketData == null || !marketData.Listings.Any())
                return pricing.InternetPrice;
            
            decimal avgMarketPrice = marketData.Listings.Average(l => l.Price);
            
            // Apply higher discount for older inventory
            decimal suggestedPrice;
            
            if (daysInInventory > 90)
                suggestedPrice = Math.Min(pricing.InternetPrice, avgMarketPrice * 0.90m);
            else if (daysInInventory > 60)
                suggestedPrice = Math.Min(pricing.InternetPrice, avgMarketPrice * 0.95m);
            else
                suggestedPrice = Math.Min(pricing.InternetPrice, avgMarketPrice * 0.98m);
            
            // Don't go below floor price if defined
            if (pricing.FloorPrice > 0)
                suggestedPrice = Math.Max(suggestedPrice, pricing.FloorPrice);
            
            return Math.Round(suggestedPrice, 2);
        }

        private string GetRecommendedAction(int daysInInventory, string condition)
        {
            if (daysInInventory > 90)
                return "Consider wholesale or significant price reduction";
            if (daysInInventory > 60)
                return "Aggressive price reduction and marketing focus";
            if (daysInInventory > 45)
                return "Moderate price reduction and front-line placement";
            
            return "Regular pricing review";
        }

        private async Task<List<AgingHeatMapItemDto>> GenerateAgingHeatMapDataAsync(List<Vehicle> vehicles)
        {
            var heatMap = new List<AgingHeatMapItemDto>();
            
            // Create combinations of make and model for the heatmap
            var makeModelGroups = vehicles
                .GroupBy(v => new { v.Make, v.Model })
                .Select(g => new
                {
                    Make = g.Key.Make,
                    Model = g.Key.Model,
                    Count = g.Count(),
                    AverageDays = (int)g.Average(v => (DateTime.UtcNow - v.ListingDate).TotalDays)
                })
                .ToList();
            
            foreach (var group in makeModelGroups)
            {
                string colorCode = GetColorCodeByDays(group.AverageDays);
                
                heatMap.Add(new AgingHeatMapItemDto
                {
                    Category = group.Make,
                    Subcategory = group.Model,
                    Count = group.Count,
                    AverageDaysInInventory = group.AverageDays,
                    ColorCode = colorCode
                });
            }
            
            return heatMap;
        }

        private string GetColorCodeByDays(int days)
        {
            // Return color codes from green to red based on aging
            if (days <= 15) return "#4CAF50"; // Green
            if (days <= 30) return "#8BC34A"; // Light Green
            if (days <= 45) return "#FFEB3B"; // Yellow
            if (days <= 60) return "#FF9800"; // Orange
            return "#F44336"; // Red
        }

        private async Task<List<AgingTrendDto>> GenerateAgingTrendDataAsync(DateRangeDto? dateRange)
        {
            // Default to last 90 days if not specified
            DateTime startDate = dateRange?.StartDate ?? DateTime.UtcNow.AddDays(-90);
            DateTime endDate = dateRange?.EndDate ?? DateTime.UtcNow;
            
            var trendData = new List<AgingTrendDto>();
            
            // Get inventory snapshots from history
            var snapshots = await _vehicleRepository.GetInventorySnapshotsForPeriodAsync(startDate, endDate);
            
            foreach (var snapshot in snapshots)
            {
                var agingCategories = new Dictionary<string, int>
                {
                    ["0-15"] = 0,
                    ["16-30"] = 0,
                    ["31-45"] = 0,
                    ["46-60"] = 0,
                    ["61+"] = 0
                };
                
                // Calculate aging for each vehicle in the snapshot
                foreach (var vehicle in snapshot.Vehicles)
                {
                    int daysInInventory = (snapshot.Date - vehicle.ListingDate).Days;
                    
                    if (daysInInventory <= 15) agingCategories["0-15"]++;
                    else if (daysInInventory <= 30) agingCategories["16-30"]++;
                    else if (daysInInventory <= 45) agingCategories["31-45"]++;
                    else if (daysInInventory <= 60) agingCategories["46-60"]++;
                    else agingCategories["61+"]++;
                }
                
                trendData.Add(new AgingTrendDto
                {
                    Date = snapshot.Date,
                    AverageDaysInInventory = snapshot.Vehicles.Any()
                        ? snapshot.Vehicles.Average(v => (snapshot.Date - v.ListingDate).TotalDays)
                        : 0,
                    TotalVehicles = snapshot.Vehicles.Count,
                    ByAgingCategory = agingCategories
                });
            }
            
            return trendData;
        }

        private string GetGroupKeyByProperty(Vehicle vehicle, string propertyName)
        {
            return propertyName.ToLower() switch
            {
                "make" => vehicle.Make,
                "model" => vehicle.Model,
                "year" => vehicle.Year.ToString(),
                "bodystyle" => vehicle.BodyStyle,
                "condition" => vehicle.Condition,
                "status" => vehicle.Status,
                _ => vehicle.Make // Default to make
            };
        }

        private async Task<List<ValuationTrendDto>> GenerateValuationTrendDataAsync()
        {
            // Get monthly snapshots for the last 12 months
            var startDate = DateTime.UtcNow.AddMonths(-11).Date.AddDays(1 - DateTime.UtcNow.Day);
            var endDate = DateTime.UtcNow;
            
            var snapshots = await _vehicleRepository.GetMonthlyInventoryValuationAsync(startDate, endDate);
            
            return snapshots.Select(s => new ValuationTrendDto
            {
                Date = s.Date,
                TotalValue = s.TotalValue,
                VehicleCount = s.VehicleCount
            }).ToList();
        }

        private DateTime GetStartDateForTimePeriod(string timePeriod)
        {
            return timePeriod.ToLower() switch
            {
                "day" => DateTime.UtcNow.AddDays(-1),
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddMonths(-1),
                "quarter" => DateTime.UtcNow.AddMonths(-3),
                "year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddMonths(-1) // Default to month
            };
        }

        private async Task<double> GetAverageInventoryLevelAsync(
            DateTime startDate,
            DateTime endDate,
            List<string>? vehicleTypes,
            List<Guid>? locationIds)
        {
            // Get inventory snapshots for the period
            var snapshots = await _vehicleRepository.GetDailyInventoryCountsAsync(
                startDate,
                endDate,
                vehicleTypes,
                locationIds);
            
            return snapshots.Any()
                ? snapshots.Average(s => s.Count)
                : 0;
        }

        private List<string> GetSegmentsForTimePeriod(string timePeriod)
        {
            // Define segments based on time period
            // For this example, we'll use the same segments regardless of time period
            return new List<string>
            {
                "Sedan", "SUV", "Truck", "Van", "Luxury", "Economy"
            };
        }

        private bool MatchesSegment(Vehicle vehicle, string segment)
        {
            // This is a simplified example - in a real implementation,
            // we would have a more sophisticated way to categorize vehicles
            if (segment == "Luxury")
                return IsLuxuryVehicle(vehicle.Make);
            
            if (segment == "Economy")
                return IsEconomyVehicle(vehicle.Make, vehicle.Model);
            
            // Match by body style (would normally come from vehicle specs)
            return string.Equals(vehicle.BodyStyle, segment, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLuxuryVehicle(string make)
        {
            var luxuryBrands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Acura", "Audi", "BMW", "Cadillac", "Infiniti", "Jaguar",
                "Land Rover", "Lexus", "Lincoln", "Mercedes-Benz", "Porsche",
                "Volvo", "Genesis", "Tesla"
            };
            
            return luxuryBrands.Contains(make);
        }

        private bool IsEconomyVehicle(string make, string model)
        {
            var economyBrands = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Mitsubishi", "Suzuki", "Kia", "Hyundai", "Chevrolet Spark",
                "Nissan Versa", "Toyota Yaris", "Honda Fit"
            };
            
            return economyBrands.Contains(make) || economyBrands.Contains($"{make} {model}");
        }

        private async Task<double> GetAverageInventoryLevelForSegmentAsync(
            DateTime startDate,
            DateTime endDate,
            string segment,
            List<string>? vehicleTypes,
            List<Guid>? locationIds)
        {
            // In a real implementation, we would query segment-specific data
            // For this example, we'll simulate by assuming segments are evenly distributed
            var totalAverageInventory = await GetAverageInventoryLevelAsync(
                startDate, endDate, vehicleTypes, locationIds);
            
            // Assume segment distribution (simplified - would come from actual data)
            var segmentDistribution = new Dictionary<string, double>
            {
                ["Sedan"] = 0.3,
                ["SUV"] = 0.4,
                ["Truck"] = 0.15,
                ["Van"] = 0.05,
                ["Luxury"] = 0.05,
                ["Economy"] = 0.05
            };
            
            return segmentDistribution.TryGetValue(segment, out var distribution)
                ? totalAverageInventory * distribution
                : 0;
        }

        private async Task<List<TurnoverTrendDto>> GenerateTurnoverTrendDataAsync(
            string timePeriod,
            List<string>? vehicleTypes,
            List<Guid>? locationIds)
        {
            // Determine data points based on time period
            DateTime startDate = GetStartDateForTimePeriod(timePeriod);
            DateTime endDate = DateTime.UtcNow;
            
            // Get turnover data for the period
            var turnoverData = await _vehicleRepository.GetHistoricalTurnoverDataAsync(
                startDate,
                endDate,
                vehicleTypes,
                locationIds);
            
            return turnoverData.Select(d => new TurnoverTrendDto
            {
                Date = d.Date,
                TurnoverRate = d.TurnoverRate,
                AverageDaysToSell = d.AverageDaysToSell
            }).ToList();
        }

        private DateTime GetComparisonDate(string comparisonPeriod)
        {
            return comparisonPeriod.ToLower() switch
            {
                "previous-month" => DateTime.UtcNow.AddMonths(-1),
                "previous-quarter" => DateTime.UtcNow.AddMonths(-3),
                "previous-year" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddMonths(-1)
            };
        }

        private async Task<double> GetAverageTurnoverForGroupAsync(string group, string groupingFactor)
        {
            // Get turnover data for the specified group
            // This is a simplified implementation
            var startDate = DateTime.UtcNow.AddMonths(-3);
            var endDate = DateTime.UtcNow;
            
            var soldVehicles = await _vehicleRepository.GetSoldVehiclesByGroupAsync(
                startDate,
                endDate,
                groupingFactor,
                group);
            
            // Calculate average days to sell
            return soldVehicles.Any()
                ? soldVehicles.Average(v => v.DaysToSell)
                : 0;
        }

        private async Task<List<SalesTrendDto>> GenerateSalesTrendByGroupAsync(
            string groupingFactor,
            List<Vehicle> salesData)
        {
            var trends = new List<SalesTrendDto>();
            
            // Group sales data by requested factor
            var groupedSales = salesData
                .GroupBy(v => GetGroupKeyByProperty(v, groupingFactor))
                .ToList();
            
            // For each group, create monthly trend data
            foreach (var group in groupedSales)
            {
                var monthlySales = new List<MonthlyDataPointDto>();
                
                // Get last 12 months
                for (int i = 0; i < 12; i++)
                {
                    var month = DateTime.UtcNow.AddMonths(-i);
                    var startOfMonth = new DateTime(month.Year, month.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    
                    int count = group
                        .Count(v => v.SellingDate >= startOfMonth && v.SellingDate <= endOfMonth);
                    
                    monthlySales.Add(new MonthlyDataPointDto
                    {
                        Month = startOfMonth,
                        Count = count
                    });
                }
                
                // Sort by date ascending
                monthlySales = monthlySales.OrderBy(m => m.Month).ToList();
                
                trends.Add(new SalesTrendDto
                {
                    Group = group.Key,
                    MonthlySales = monthlySales
                });
            }
            
            return trends;
        }

        private List<MixRecommendationDto> GenerateInventoryMixRecommendations(
            List<InventoryMixItemDto> currentMix,
            List<InventoryMixItemDto> comparisonMix,
            List<Vehicle> salesData,
            string groupingFactor)
        {
            var recommendations = new List<MixRecommendationDto>();
            
            // Group sales data by the factor
            var salesByGroup = salesData
                .GroupBy(v => GetGroupKeyByProperty(v, groupingFactor))
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
            
            int totalSales = salesData.Count;
            
            // Calculate ideal percentage based on sales velocity
            foreach (var group in currentMix)
            {
                if (!salesByGroup.TryGetValue(group.Group, out int groupSales))
                {
                    groupSales = 0;
                }
                
                double salesPercentage = totalSales > 0
                    ? (double)groupSales / totalSales * 100
                    : 0;
                
                // Compare current percentage with sales percentage
                string recommendationType;
                string reason;
                double idealPercentage;
                
                if (group.Percentage < salesPercentage * 0.7)
                {
                    recommendationType = "Increase";
                    reason = "Current inventory mix is significantly lower than sales demand";
                    idealPercentage = salesPercentage;
                }
                else if (group.Percentage > salesPercentage * 1.3)
                {
                    recommendationType = "Decrease";
                    reason = "Current inventory mix is significantly higher than sales demand";
                    idealPercentage = salesPercentage;
                }
                else
                {
                    recommendationType = "Maintain";
                    reason = "Current inventory mix aligns well with sales demand";
                    idealPercentage = group.Percentage;
                }
                
                recommendations.Add(new MixRecommendationDto
                {
                    Group = group.Group,
                    RecommendationType = recommendationType,
                    Reason = reason,
                    IdealPercentage = idealPercentage
                });
            }
            
            return recommendations;
        }

        private string DeterminePricePosition(
            decimal currentPrice,
            decimal averagePrice,
            int percentile)
        {
            if (percentile <= 20)
                return "Underpriced";
            if (percentile >= 80)
                return "Overpriced";
            
            // Within 5% of average is considered competitive
            decimal priceRatio = currentPrice / averagePrice;
            if (priceRatio >= 0.95m && priceRatio <= 1.05m)
                return "Very Competitive";
            
            return "Competitive";
        }

        private void UpdateMarketSummaryData(
            MarketSummaryDto summary,
            List<dynamic> listings,
            double averageDaysOnMarket,
            Vehicle vehicle)
        {
            summary.TotalCompetitorVehicles += listings.Count;
            summary.AverageDaysOnMarket = averageDaysOnMarket;
            
            // Update inventory by competitor
            foreach (var listing in listings)
            {
                string dealer = listing.DealerName ?? "Unknown";
                
                if (!summary.InventoryByCompetitor.ContainsKey(dealer))
                {
                    summary.InventoryByCompetitor[dealer] = 0;
                }
                
                summary.InventoryByCompetitor[dealer]++;
            }
            
            // Update price range by trim
            string trimKey = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} {vehicle.Trim}";
            
            if (!summary.PriceRangeByTrim.ContainsKey(trimKey))
            {
                var trimListings = listings
                    .Where(l => string.Equals(l.Trim, vehicle.Trim, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                if (trimListings.Any())
                {
                    summary.PriceRangeByTrim[trimKey] = trimListings.Max(l => l.Price) - trimListings.Min(l => l.Price);
                }
            }
        }

        #endregion
    }
}
