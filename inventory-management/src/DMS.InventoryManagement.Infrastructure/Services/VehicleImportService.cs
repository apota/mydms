using CsvHelper;
using CsvHelper.Configuration;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IVehicleImportService for importing vehicles from various sources
    /// </summary>
    public class VehicleImportService : IVehicleImportService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<VehicleImportService> _logger;

        /// <summary>
        /// Initializes a new instance of the VehicleImportService class
        /// </summary>
        /// <param name="vehicleRepository">The vehicle repository</param>
        /// <param name="httpClientFactory">The HTTP client factory</param>
        /// <param name="logger">The logger</param>
        public VehicleImportService(
            IVehicleRepository vehicleRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<VehicleImportService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<VehicleImportResult> ImportFromCsvAsync(string filePath, string mappingTemplate)
        {
            _logger.LogInformation("Importing vehicles from CSV file: {filePath} using template: {template}", filePath, mappingTemplate);
            
            var result = new VehicleImportResult();
            
            try
            {
                if (!File.Exists(filePath))
                {
                    result.Success = false;
                    result.Errors.Add($"File not found: {filePath}");
                    return result;
                }
                
                // Get the mapping template
                var templates = await GetAvailableMappingTemplatesAsync();
                var template = templates.FirstOrDefault(t => t.Name.Equals(mappingTemplate, StringComparison.OrdinalIgnoreCase));
                
                if (template == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Mapping template not found: {mappingTemplate}");
                    return result;
                }

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                
                // Read all records
                var records = csv.GetRecords<dynamic>().ToList();
                result.TotalRecords = records.Count;
                
                foreach (var record in records)
                {
                    try
                    {
                        var vehicle = new Vehicle();
                        
                        // Apply the mapping template to map CSV columns to vehicle properties
                        foreach (var mapping in template.ColumnMappings)
                        {
                            string sourceColumn = mapping.Key;
                            string targetProperty = mapping.Value;
                            
                            if (((IDictionary<string, object>)record).TryGetValue(sourceColumn, out object value))
                            {
                                ApplyValueToVehicle(vehicle, targetProperty, value?.ToString());
                            }
                        }
                        
                        // Validate the vehicle
                        var validationWarnings = ValidateVehicle(vehicle, result.ImportedVehicles.Count + 1);
                        
                        if (validationWarnings.Any())
                        {
                            result.Warnings.AddRange(validationWarnings);
                        }

                        // Set default values for required fields if not provided
                        if (vehicle.Id == Guid.Empty)
                        {
                            vehicle.Id = Guid.NewGuid();
                        }

                        if (string.IsNullOrEmpty(vehicle.StockNumber))
                        {
                            vehicle.StockNumber = GenerateStockNumber();
                        }

                        vehicle.CreatedAt = DateTime.UtcNow;

                        // Add to repository
                        await _vehicleRepository.AddAsync(vehicle);
                        result.ImportedVehicles.Add(vehicle);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing vehicle from CSV row");
                        result.ErrorCount++;
                        result.Errors.Add($"Error in row {result.ImportedVehicles.Count + result.ErrorCount + 1}: {ex.Message}");
                    }
                }
                
                // Save changes
                await _vehicleRepository.SaveChangesAsync();
                
                result.Success = result.ErrorCount == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vehicles from CSV file: {filePath}", filePath);
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                return result;
            }
        }

        /// <inheritdoc/>
        public async Task<VehicleImportResult> ImportFromManufacturerFeedAsync(string manufacturerCode, ManufacturerImportOptions options)
        {
            _logger.LogInformation("Importing vehicles from manufacturer feed: {manufacturer}", manufacturerCode);
            
            var result = new VehicleImportResult();
            
            try
            {
                // Implementation would connect to manufacturer API
                // This is a mock implementation
                
                // Simulate API call delay
                await Task.Delay(2000);

                // Create sample data
                var sampleVehicles = CreateSampleManufacturerVehicles(manufacturerCode, options);
                result.TotalRecords = sampleVehicles.Count;

                foreach (var vehicle in sampleVehicles)
                {
                    try
                    {
                        // Check if vehicle already exists
                        var existingVehicle = await _vehicleRepository.GetByVinAsync(vehicle.VIN);
                        
                        if (existingVehicle != null)
                        {
                            if (options.UpdateExisting)
                            {
                                // Update existing vehicle
                                existingVehicle.Make = vehicle.Make;
                                existingVehicle.Model = vehicle.Model;
                                existingVehicle.Year = vehicle.Year;
                                existingVehicle.Trim = vehicle.Trim;
                                existingVehicle.ExteriorColor = vehicle.ExteriorColor;
                                existingVehicle.InteriorColor = vehicle.InteriorColor;
                                existingVehicle.MSRP = vehicle.MSRP;
                                existingVehicle.UpdatedAt = DateTime.UtcNow;
                                
                                // Add to result
                                result.ImportedVehicles.Add(existingVehicle);
                                result.SuccessCount++;
                            }
                            else
                            {
                                // Skip existing vehicle
                                result.Warnings.Add(new ValidationWarning
                                {
                                    FieldName = "VIN",
                                    Message = $"Vehicle with VIN {vehicle.VIN} already exists and update option is disabled",
                                    RowNumber = result.SuccessCount + result.ErrorCount + 1
                                });
                            }
                        }
                        else
                        {
                            // Add new vehicle
                            await _vehicleRepository.AddAsync(vehicle);
                            result.ImportedVehicles.Add(vehicle);
                            result.SuccessCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing vehicle from manufacturer feed");
                        result.ErrorCount++;
                        result.Errors.Add($"Error processing VIN {vehicle.VIN}: {ex.Message}");
                    }
                }

                // Save changes
                await _vehicleRepository.SaveChangesAsync();
                
                result.Success = result.ErrorCount == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vehicles from manufacturer feed: {manufacturer}", manufacturerCode);
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                return result;
            }
        }

        /// <inheritdoc/>
        public async Task<VehicleImportResult> ImportFromAuctionAsync(string auctionCode, AuctionImportOptions options)
        {
            _logger.LogInformation("Importing vehicles from auction: {auction}", auctionCode);
            
            var result = new VehicleImportResult();
            
            try
            {
                // Implementation would connect to auction API
                // This is a mock implementation
                
                // Simulate API call delay
                await Task.Delay(2000);

                // Create sample data
                var sampleVehicles = CreateSampleAuctionVehicles(auctionCode, options);
                result.TotalRecords = sampleVehicles.Count;

                foreach (var vehicle in sampleVehicles)
                {
                    try
                    {
                        // Check if vehicle already exists
                        var existingVehicle = await _vehicleRepository.GetByVinAsync(vehicle.VIN);
                        
                        if (existingVehicle != null)
                        {
                            // Skip existing vehicle
                            result.Warnings.Add(new ValidationWarning
                            {
                                FieldName = "VIN",
                                Message = $"Vehicle with VIN {vehicle.VIN} already exists",
                                RowNumber = result.SuccessCount + result.ErrorCount + 1
                            });
                        }
                        else
                        {
                            // Add new vehicle
                            await _vehicleRepository.AddAsync(vehicle);
                            result.ImportedVehicles.Add(vehicle);
                            result.SuccessCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing vehicle from auction");
                        result.ErrorCount++;
                        result.Errors.Add($"Error processing VIN {vehicle.VIN}: {ex.Message}");
                    }
                }

                // Save changes
                await _vehicleRepository.SaveChangesAsync();
                
                result.Success = result.ErrorCount == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing vehicles from auction: {auction}", auctionCode);
                result.Success = false;
                result.Errors.Add($"Import failed: {ex.Message}");
                return result;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MappingTemplate>> GetAvailableMappingTemplatesAsync()
        {
            return await Task.FromResult(new List<MappingTemplate>
            {
                new MappingTemplate
                {
                    Name = "Standard",
                    Description = "Standard CSV format with basic vehicle information",
                    ColumnMappings = new Dictionary<string, string>
                    {
                        { "VIN", "VIN" },
                        { "Stock", "StockNumber" },
                        { "Make", "Make" },
                        { "Model", "Model" },
                        { "Year", "Year" },
                        { "Trim", "Trim" },
                        { "Ext Color", "ExteriorColor" },
                        { "Int Color", "InteriorColor" },
                        { "Mileage", "Mileage" },
                        { "Type", "VehicleType" },
                        { "Status", "Status" },
                        { "Cost", "AcquisitionCost" },
                        { "List Price", "ListPrice" },
                        { "MSRP", "MSRP" }
                    }
                },
                new MappingTemplate
                {
                    Name = "Auction",
                    Description = "CSV format for auction data",
                    ColumnMappings = new Dictionary<string, string>
                    {
                        { "VIN Number", "VIN" },
                        { "Stock #", "StockNumber" },
                        { "Vehicle Make", "Make" },
                        { "Vehicle Model", "Model" },
                        { "Vehicle Year", "Year" },
                        { "Trim Level", "Trim" },
                        { "Exterior", "ExteriorColor" },
                        { "Interior", "InteriorColor" },
                        { "Odometer", "Mileage" },
                        { "Vehicle Condition", "VehicleType" },
                        { "Acquisition Cost", "AcquisitionCost" },
                        { "Reserve Price", "ListPrice" },
                        { "Original MSRP", "MSRP" }
                    }
                },
                new MappingTemplate
                {
                    Name = "DealerTrack",
                    Description = "DealerTrack export format",
                    ColumnMappings = new Dictionary<string, string>
                    {
                        { "VIN", "VIN" },
                        { "StockNo", "StockNumber" },
                        { "Make", "Make" },
                        { "Model", "Model" },
                        { "ModelYear", "Year" },
                        { "Series", "Trim" },
                        { "ExtColorDesc", "ExteriorColor" },
                        { "IntColorDesc", "InteriorColor" },
                        { "Mileage", "Mileage" },
                        { "NewUsed", "VehicleType" },
                        { "Status", "Status" },
                        { "Cost", "AcquisitionCost" },
                        { "InternetPrice", "ListPrice" },
                        { "MSRP", "MSRP" }
                    }
                }
            });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAvailableManufacturerCodesAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "TOYOTA",
                "HONDA",
                "FORD",
                "CHEVROLET",
                "BMW",
                "MERCEDES",
                "AUDI",
                "NISSAN",
                "HYUNDAI",
                "KIA"
            });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAvailableAuctionCodesAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "MANHEIM",
                "ADESA",
                "COPART",
                "IAAI",
                "CARMAX"
            });
        }

        private void ApplyValueToVehicle(Vehicle vehicle, string propertyName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            switch (propertyName)
            {
                case "VIN":
                    vehicle.VIN = value.Trim();
                    break;
                case "StockNumber":
                    vehicle.StockNumber = value.Trim();
                    break;
                case "Make":
                    vehicle.Make = value.Trim();
                    break;
                case "Model":
                    vehicle.Model = value.Trim();
                    break;
                case "Year":
                    if (int.TryParse(value, out int year))
                    {
                        vehicle.Year = year;
                    }
                    break;
                case "Trim":
                    vehicle.Trim = value.Trim();
                    break;
                case "ExteriorColor":
                    vehicle.ExteriorColor = value.Trim();
                    break;
                case "InteriorColor":
                    vehicle.InteriorColor = value.Trim();
                    break;
                case "Mileage":
                    if (int.TryParse(value.Replace(",", ""), out int mileage))
                    {
                        vehicle.Mileage = mileage;
                    }
                    break;
                case "VehicleType":
                    if (Enum.TryParse<VehicleType>(value, true, out VehicleType vehicleType))
                    {
                        vehicle.VehicleType = vehicleType;
                    }
                    else
                    {
                        // Try to map common values
                        string normalizedValue = value.Trim().ToLowerInvariant();
                        if (normalizedValue == "new")
                        {
                            vehicle.VehicleType = VehicleType.New;
                        }
                        else if (normalizedValue == "used" || normalizedValue == "pre-owned")
                        {
                            vehicle.VehicleType = VehicleType.Used;
                        }
                        else if (normalizedValue == "cpo" || normalizedValue == "certified" || normalizedValue == "certified pre-owned")
                        {
                            vehicle.VehicleType = VehicleType.CertifiedPreOwned;
                        }
                    }
                    break;
                case "Status":
                    if (Enum.TryParse<VehicleStatus>(value, true, out VehicleStatus vehicleStatus))
                    {
                        vehicle.Status = vehicleStatus;
                    }
                    else
                    {
                        // Default to InStock if status can't be parsed
                        vehicle.Status = VehicleStatus.InStock;
                    }
                    break;
                case "AcquisitionCost":
                    if (decimal.TryParse(value.Replace("$", "").Replace(",", ""), out decimal acquisitionCost))
                    {
                        vehicle.AcquisitionCost = acquisitionCost;
                    }
                    break;
                case "ListPrice":
                    if (decimal.TryParse(value.Replace("$", "").Replace(",", ""), out decimal listPrice))
                    {
                        vehicle.ListPrice = listPrice;
                    }
                    break;
                case "MSRP":
                    if (decimal.TryParse(value.Replace("$", "").Replace(",", ""), out decimal msrp))
                    {
                        vehicle.MSRP = msrp;
                    }
                    break;
            }
        }

        private List<ValidationWarning> ValidateVehicle(Vehicle vehicle, int rowNumber)
        {
            var warnings = new List<ValidationWarning>();
            
            // Check required fields
            if (string.IsNullOrEmpty(vehicle.VIN))
            {
                warnings.Add(new ValidationWarning
                {
                    RowNumber = rowNumber,
                    FieldName = "VIN",
                    Message = "VIN is required"
                });
            }
            else if (vehicle.VIN.Length != 17)
            {
                warnings.Add(new ValidationWarning
                {
                    RowNumber = rowNumber,
                    FieldName = "VIN",
                    Message = "VIN must be exactly 17 characters"
                });
            }
            
            if (string.IsNullOrEmpty(vehicle.Make))
            {
                warnings.Add(new ValidationWarning
                {
                    RowNumber = rowNumber,
                    FieldName = "Make",
                    Message = "Make is required"
                });
            }
            
            if (string.IsNullOrEmpty(vehicle.Model))
            {
                warnings.Add(new ValidationWarning
                {
                    RowNumber = rowNumber,
                    FieldName = "Model",
                    Message = "Model is required"
                });
            }
            
            if (vehicle.Year < 1900 || vehicle.Year > DateTime.Now.Year + 1)
            {
                warnings.Add(new ValidationWarning
                {
                    RowNumber = rowNumber,
                    FieldName = "Year",
                    Message = $"Year must be between 1900 and {DateTime.Now.Year + 1}"
                });
            }
            
            return warnings;
        }

        private string GenerateStockNumber()
        {
            // Format: Year-Month-Random 3 digits
            return $"{DateTime.Now:yy}{DateTime.Now:MM}{new Random().Next(100, 999)}";
        }

        private List<Vehicle> CreateSampleManufacturerVehicles(string manufacturerCode, ManufacturerImportOptions options)
        {
            var result = new List<Vehicle>();
            var random = new Random();
            
            // Generate sample data based on manufacturer
            string make = manufacturerCode switch
            {
                "TOYOTA" => "Toyota",
                "HONDA" => "Honda",
                "FORD" => "Ford",
                "CHEVROLET" => "Chevrolet",
                "BMW" => "BMW",
                "MERCEDES" => "Mercedes-Benz",
                "AUDI" => "Audi",
                "NISSAN" => "Nissan",
                "HYUNDAI" => "Hyundai",
                "KIA" => "Kia",
                _ => "Generic"
            };

            // Sample models for each make
            var models = manufacturerCode switch
            {
                "TOYOTA" => new[] { "Camry", "Corolla", "RAV4", "Highlander", "Tacoma" },
                "HONDA" => new[] { "Civic", "Accord", "CR-V", "Pilot", "Odyssey" },
                "FORD" => new[] { "F-150", "Explorer", "Escape", "Edge", "Mustang" },
                "CHEVROLET" => new[] { "Silverado", "Equinox", "Malibu", "Traverse", "Camaro" },
                "BMW" => new[] { "3 Series", "5 Series", "X3", "X5", "7 Series" },
                "MERCEDES" => new[] { "C-Class", "E-Class", "GLC", "GLE", "S-Class" },
                "AUDI" => new[] { "A4", "A6", "Q5", "Q7", "e-tron" },
                "NISSAN" => new[] { "Altima", "Rogue", "Sentra", "Pathfinder", "Maxima" },
                "HYUNDAI" => new[] { "Elantra", "Sonata", "Tucson", "Santa Fe", "Palisade" },
                "KIA" => new[] { "Forte", "Optima", "Sportage", "Sorento", "Telluride" },
                _ => new[] { "Sedan", "SUV", "Truck", "Coupe", "Convertible" }
            };

            // Sample trim levels
            var trims = new[] { "Base", "Sport", "Limited", "Touring", "Platinum", "SE", "XLE" };
            
            // Sample colors
            var exteriorColors = new[] { "White", "Black", "Silver", "Gray", "Blue", "Red", "Green" };
            var interiorColors = new[] { "Black", "Gray", "Beige", "Brown", "White" };

            // Generate sample vehicles
            int count = random.Next(5, 20);
            
            for (int i = 0; i < count; i++)
            {
                int year = options.NewInventoryOnly ? DateTime.Now.Year : random.Next(DateTime.Now.Year - 3, DateTime.Now.Year + 1);
                string model = models[random.Next(0, models.Length)];
                string vin = GenerateRandomVin(make, model, year);
                
                var vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    VIN = vin,
                    StockNumber = $"MF{make.Substring(0, 1)}{DateTime.Now:yyMM}{random.Next(100, 999)}",
                    Make = make,
                    Model = model,
                    Year = year,
                    Trim = trims[random.Next(0, trims.Length)],
                    ExteriorColor = exteriorColors[random.Next(0, exteriorColors.Length)],
                    InteriorColor = interiorColors[random.Next(0, interiorColors.Length)],
                    Mileage = options.NewInventoryOnly ? random.Next(1, 100) : 0,
                    VehicleType = options.NewInventoryOnly ? VehicleType.New : VehicleType.Used,
                    Status = VehicleStatus.InTransit,
                    MSRP = (decimal)(20000 + random.NextDouble() * 50000),
                    ListPrice = (decimal)(19000 + random.NextDouble() * 49000),
                    AcquisitionDate = DateTime.UtcNow,
                    AcquisitionSource = $"Manufacturer: {make}",
                    CreatedAt = DateTime.UtcNow
                };
                
                result.Add(vehicle);
            }
            
            return result;
        }

        private List<Vehicle> CreateSampleAuctionVehicles(string auctionCode, AuctionImportOptions options)
        {
            var result = new List<Vehicle>();
            var random = new Random();
            
            // Generate sample data based on auction
            var makes = options.Makes.Any() ? options.Makes.ToArray() : new[] 
            {
                "Toyota", "Honda", "Ford", "Chevrolet", "BMW", "Mercedes-Benz", "Audi", "Nissan", "Hyundai", "Kia"
            };

            // Sample models for common makes
            var modelsByMake = new Dictionary<string, string[]>
            {
                ["Toyota"] = new[] { "Camry", "Corolla", "RAV4", "Highlander", "Tacoma" },
                ["Honda"] = new[] { "Civic", "Accord", "CR-V", "Pilot", "Odyssey" },
                ["Ford"] = new[] { "F-150", "Explorer", "Escape", "Edge", "Mustang" },
                ["Chevrolet"] = new[] { "Silverado", "Equinox", "Malibu", "Traverse", "Camaro" },
                ["BMW"] = new[] { "3 Series", "5 Series", "X3", "X5", "7 Series" },
                ["Mercedes-Benz"] = new[] { "C-Class", "E-Class", "GLC", "GLE", "S-Class" },
                ["Audi"] = new[] { "A4", "A6", "Q5", "Q7", "e-tron" },
                ["Nissan"] = new[] { "Altima", "Rogue", "Sentra", "Pathfinder", "Maxima" },
                ["Hyundai"] = new[] { "Elantra", "Sonata", "Tucson", "Santa Fe", "Palisade" },
                ["Kia"] = new[] { "Forte", "Optima", "Sportage", "Sorento", "Telluride" }
            };
            
            // Sample trim levels
            var trims = new[] { "Base", "Sport", "Limited", "Touring", "Platinum", "SE", "XLE" };
            
            // Sample colors
            var exteriorColors = new[] { "White", "Black", "Silver", "Gray", "Blue", "Red", "Green" };
            var interiorColors = new[] { "Black", "Gray", "Beige", "Brown", "White" };

            // Generate sample vehicles
            int count = random.Next(5, 15);
            
            for (int i = 0; i < count; i++)
            {
                string make = makes[random.Next(0, makes.Length)];
                var models = modelsByMake.ContainsKey(make) ? modelsByMake[make] : new[] { "Generic Model" };
                string model = models[random.Next(0, models.Length)];
                int year = random.Next(DateTime.Now.Year - 8, DateTime.Now.Year);
                string vin = GenerateRandomVin(make, model, year);
                
                var vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    VIN = vin,
                    StockNumber = $"AU{auctionCode.Substring(0, 1)}{DateTime.Now:yyMM}{random.Next(100, 999)}",
                    Make = make,
                    Model = model,
                    Year = year,
                    Trim = trims[random.Next(0, trims.Length)],
                    ExteriorColor = exteriorColors[random.Next(0, exteriorColors.Length)],
                    InteriorColor = interiorColors[random.Next(0, interiorColors.Length)],
                    Mileage = random.Next(10000, 100000),
                    VehicleType = VehicleType.Used,
                    Status = options.WonAuctionsOnly ? VehicleStatus.InTransit : VehicleStatus.Receiving,
                    ListPrice = (decimal)(10000 + random.NextDouble() * 30000),
                    AcquisitionCost = (decimal)(8000 + random.NextDouble() * 25000),
                    AcquisitionDate = DateTime.UtcNow,
                    AcquisitionSource = $"Auction: {auctionCode}",
                    CreatedAt = DateTime.UtcNow
                };
                
                result.Add(vehicle);
            }
            
            return result;
        }

        private string GenerateRandomVin(string make, string model, int year)
        {
            // Generate a random VIN-like string
            // Format: WMI (3) + VDS (6) + Check Digit (1) + VIS (7)
            var sb = new StringBuilder();
            var random = new Random();
            
            // World Manufacturer Identifier (WMI) - first 3 characters
            string wmi = make switch
            {
                "Toyota" => "JT3",
                "Honda" => "JHM",
                "Ford" => "1FA",
                "Chevrolet" => "1G1",
                "BMW" => "WBA",
                "Mercedes-Benz" => "WDD",
                "Audi" => "WAU",
                "Nissan" => "JN1",
                "Hyundai" => "KMH",
                "Kia" => "KND",
                _ => "ZZZ"
            };
            sb.Append(wmi);
            
            // Vehicle Descriptor Section (VDS) - characters 4-9
            for (int i = 0; i < 6; i++)
            {
                sb.Append("ABCDEFGHJKLMNPRSTUVWXYZ0123456789"[random.Next(33)]);
            }
            
            // Check Digit (character 9) - we're not calculating a real check digit
            sb.Append("0123456789X"[random.Next(11)]);
            
            // Vehicle Identifier Section (VIS) - characters 10-17
            // 10th position is the model year
            sb.Append("ABCDEFGHJKLMNPRSTVWXY123456789"[year % 30]);
            
            // 11th position is the plant code
            sb.Append("ABCDEFGHJKLMNPRSTUVWXYZ0123456789"[random.Next(33)]);
            
            // 12-17 are the production sequence
            for (int i = 0; i < 6; i++)
            {
                sb.Append("0123456789"[random.Next(10)]);
            }
            
            return sb.ToString();
        }
    }
}
