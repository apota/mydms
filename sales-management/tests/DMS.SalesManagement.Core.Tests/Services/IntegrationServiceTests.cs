using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Services;
using DMS.SalesManagement.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace DMS.SalesManagement.Core.Tests.Services
{
    public class IntegrationServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<IntegrationService>> _loggerMock;
        
        public IntegrationServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<IntegrationService>>();
            
            // Setup configuration
            _configurationMock.Setup(c => c["IntegrationUrls:PartsManagementApi"]).Returns("http://parts-api/api");
            _configurationMock.Setup(c => c["IntegrationUrls:FinancialManagementApi"]).Returns("http://financial-api/api");
            _configurationMock.Setup(c => c["IntegrationUrls:ServiceManagementApi"]).Returns("http://service-api/api");
            _configurationMock.Setup(c => c["IntegrationUrls:CrmApi"]).Returns("http://crm-api/api");
        }
        
        [Fact]
        public async Task GetVehicleAccessoriesAsync_ReturnsAccessories_WhenSuccessful()
        {
            // Arrange
            var vehicleId = "VEH-123";
            var httpClient = SetupHttpClient(
                $"http://parts-api/api/integration/sales/vehicles/{vehicleId}/accessories",
                HttpStatusCode.OK,
                "[{\"id\":\"ACC-1\",\"name\":\"Roof Rack\",\"description\":\"Aluminum roof rack\",\"price\":299.99,\"category\":\"External\",\"isInstalled\":false,\"imageUrl\":\"http://example.com/rack.jpg\",\"installationTimeMinutes\":45}]"
            );
            
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            
            var service = new IntegrationService(httpClient, _configurationMock.Object, _loggerMock.Object);
            
            // Act
            var result = await service.GetVehicleAccessoriesAsync(vehicleId);
            
            // Assert
            var accessories = Assert.IsAssignableFrom<IEnumerable<AccessoryDto>>(result);
            var accessoriesList = new List<AccessoryDto>(accessories);
            
            Assert.Single(accessoriesList);
            Assert.Equal("ACC-1", accessoriesList[0].Id);
            Assert.Equal("Roof Rack", accessoriesList[0].Name);
            Assert.Equal(299.99M, accessoriesList[0].Price);
        }
        
        [Fact]
        public async Task ReservePartsForDealAsync_ReturnsReservation_WhenSuccessful()
        {
            // Arrange
            var dealId = "DEAL-456";
            var request = new ReservePartsRequestDto
            {
                Parts = new List<ReservePartItemDto>
                {
                    new ReservePartItemDto { PartId = "PART-1", Quantity = 2 }
                },
                RequiredDate = DateTime.Now.AddDays(3),
                Notes = "Test reservation"
            };
            
            var httpClient = SetupHttpClient(
                $"http://parts-api/api/integration/sales/deals/{dealId}/reserve-parts",
                HttpStatusCode.OK,
                "{\"reservationId\":\"RES-1\",\"dealId\":\"DEAL-456\",\"reservedParts\":[{\"partId\":\"PART-1\",\"partNumber\":\"P12345\",\"name\":\"Test Part\",\"quantity\":2,\"unitPrice\":50.99,\"isAvailable\":true}],\"reservationDate\":\"2023-06-15T12:00:00Z\",\"expirationDate\":\"2023-06-22T12:00:00Z\",\"totalPrice\":101.98,\"allPartsAvailable\":true}"
            );
            
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            
            var service = new IntegrationService(httpClient, _configurationMock.Object, _loggerMock.Object);
            
            // Act
            var result = await service.ReservePartsForDealAsync(dealId, request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("RES-1", result.ReservationId);
            Assert.Equal(dealId, result.DealId);
            Assert.Single(result.ReservedParts);
            Assert.Equal(101.98M, result.TotalPrice);
            Assert.True(result.AllPartsAvailable);
        }
        
        [Fact]
        public async Task GetFinancialQuotesForDealAsync_ReturnsQuotes_WhenSuccessful()
        {
            // Arrange
            var dealId = "DEAL-789";
            var httpClient = SetupHttpClient(
                $"http://financial-api/api/integration/sales/deals/{dealId}/quotes",
                HttpStatusCode.OK,
                "[{\"id\":\"QUOTE-1\",\"lenderName\":\"ABC Bank\",\"productType\":\"Loan\",\"amount\":25000.00,\"interestRate\":3.9,\"termMonths\":60,\"monthlyPayment\":462.03,\"downPayment\":5000.00,\"expirationDate\":\"2023-07-15T00:00:00Z\",\"isPromotional\":true,\"promotionDescription\":\"Summer Special Rate\",\"requirements\":[\"Proof of Income\",\"Credit Check\"]}]"
            );
            
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            
            var service = new IntegrationService(httpClient, _configurationMock.Object, _loggerMock.Object);
            
            // Act
            var result = await service.GetFinancialQuotesForDealAsync(dealId);
            
            // Assert
            var quotes = Assert.IsAssignableFrom<IEnumerable<FinancialQuoteDto>>(result);
            var quotesList = new List<FinancialQuoteDto>(quotes);
            
            Assert.Single(quotesList);
            Assert.Equal("QUOTE-1", quotesList[0].Id);
            Assert.Equal("ABC Bank", quotesList[0].LenderName);
            Assert.Equal(3.9M, quotesList[0].InterestRate);
            Assert.True(quotesList[0].IsPromotional);
        }
        
        private HttpClient SetupHttpClient(string expectedUri, HttpStatusCode statusCode, string content)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
                
            return new HttpClient(handlerMock.Object);
        }
    }
}
