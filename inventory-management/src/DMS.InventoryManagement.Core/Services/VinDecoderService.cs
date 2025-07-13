using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Exceptions;

namespace DMS.InventoryManagement.Core.Services
{
    public interface IVinDecoderService
    {
        Task<VinDecoderResult> DecodeVin(string vin);
    }

    public class VinDecoderService : IVinDecoderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VinDecoderService> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public VinDecoderService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<VinDecoderService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["ExternalServices:VinDecoder:ApiKey"];
            _apiUrl = _configuration["ExternalServices:VinDecoder:ApiUrl"];
            
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl))
            {
                _logger.LogWarning("VIN Decoder service configuration missing.");
            }
        }

        public async Task<VinDecoderResult> DecodeVin(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                throw new ArgumentException("VIN cannot be empty", nameof(vin));
            }

            if (vin.Length != 17)
            {
                throw new ArgumentException("VIN must be 17 characters", nameof(vin));
            }

            try
            {
                var requestUrl = $"{_apiUrl}?vin={vin}&apikey={_apiKey}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("VIN decoder API returned {StatusCode}: {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    throw new ExternalServiceException($"Unable to decode VIN. Service returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<VinDecoderApiResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result?.Results == null || result.Results.Count == 0)
                {
                    _logger.LogWarning("No results returned for VIN {Vin}", vin);
                    throw new ExternalServiceException("VIN decoder returned no results");
                }

                return new VinDecoderResult
                {
                    Make = result.Results[0].Make,
                    Model = result.Results[0].Model,
                    Year = int.Parse(result.Results[0].ModelYear),
                    Trim = result.Results[0].Trim,
                    EngineType = result.Results[0].Engine,
                    FuelType = result.Results[0].FuelType,
                    Transmission = result.Results[0].Transmission,
                    DriveType = result.Results[0].DriveType,
                    BodyStyle = result.Results[0].BodyStyle,
                    MSRP = result.Results[0].Msrp != null ? decimal.Parse(result.Results[0].Msrp) : null
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error contacting VIN decoder service for VIN {Vin}", vin);
                throw new ExternalServiceException("VIN decoder service unavailable", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing VIN decoder response for VIN {Vin}", vin);
                throw new ExternalServiceException("Error parsing VIN decoder response", ex);
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Unexpected error in VIN decoder service for VIN {Vin}", vin);
                throw new ExternalServiceException("Unexpected error in VIN decoder service", ex);
            }
        }
    }

    public class VinDecoderApiResponse
    {
        public int Count { get; set; }
        public string Message { get; set; }
        public string SearchCriteria { get; set; }
        public List<VinDecoderResult> Results { get; set; }
    }

    public class VinDecoderResult
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Trim { get; set; }
        public string EngineType { get; set; }
        public string FuelType { get; set; }
        public string Transmission { get; set; }
        public string DriveType { get; set; }
        public string BodyStyle { get; set; }
        public decimal? MSRP { get; set; }
        public Dictionary<string, string> AdditionalSpecifications { get; set; } = new Dictionary<string, string>();
    }
}
