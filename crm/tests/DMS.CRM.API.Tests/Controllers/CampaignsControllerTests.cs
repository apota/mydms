using DMS.CRM.API.Controllers;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DMS.CRM.API.Tests.Controllers
{
    public class CampaignsControllerTests
    {
        private readonly Mock<ICampaignService> _mockCampaignService;
        private readonly Mock<ILogger<CampaignsController>> _mockLogger;
        private readonly CampaignsController _controller;

        public CampaignsControllerTests()
        {
            _mockCampaignService = new Mock<ICampaignService>();
            _mockLogger = new Mock<ILogger<CampaignsController>>();
            _controller = new CampaignsController(_mockCampaignService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllCampaigns_ReturnsOkResult_WithCampaigns()
        {
            // Arrange
            var testCampaigns = GetTestCampaigns();
            _mockCampaignService.Setup(service => service.GetAllCampaignsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(testCampaigns);

            // Act
            var result = await _controller.GetAllCampaigns(0, 50);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CampaignDto>>(okResult.Value);
            Assert.Equal(3, ((List<CampaignDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetCampaignById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testCampaign = new CampaignDto
            {
                Id = testId,
                Name = "Test Campaign"
            };

            _mockCampaignService.Setup(service => service.GetCampaignByIdAsync(testId))
                .ReturnsAsync(testCampaign);

            // Act
            var result = await _controller.GetCampaignById(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CampaignDto>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public async Task GetCampaignById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockCampaignService.Setup(service => service.GetCampaignByIdAsync(testId))
                .ReturnsAsync((CampaignDto)null);

            // Act
            var result = await _controller.GetCampaignById(testId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCampaign_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var campaignCreateDto = new CampaignCreateDto
            {
                Name = "New Test Campaign",
                Description = "Test Description",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(30),
                Type = "Promotion",
                Budget = 10000,
                Status = "Draft"
            };

            var createdCampaign = new CampaignDto
            {
                Id = Guid.NewGuid(),
                Name = campaignCreateDto.Name,
                Description = campaignCreateDto.Description,
                StartDate = campaignCreateDto.StartDate,
                EndDate = campaignCreateDto.EndDate,
                Type = campaignCreateDto.Type,
                Budget = campaignCreateDto.Budget,
                Status = campaignCreateDto.Status
            };

            _mockCampaignService.Setup(service => service.CreateCampaignAsync(It.IsAny<CampaignCreateDto>()))
                .ReturnsAsync(createdCampaign);

            // Act
            var result = await _controller.CreateCampaign(campaignCreateDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetCampaignById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<CampaignDto>(createdAtActionResult.Value);
            Assert.Equal(createdCampaign.Id, returnValue.Id);
            Assert.Equal(campaignCreateDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task CreateCampaign_WithNullData_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateCampaign(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateCampaign_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var campaignUpdateDto = new CampaignUpdateDto
            {
                Name = "Updated Test Campaign",
                Description = "Updated Description",
                Status = "Active"
            };

            var updatedCampaign = new CampaignDto
            {
                Id = testId,
                Name = campaignUpdateDto.Name,
                Description = campaignUpdateDto.Description,
                Status = campaignUpdateDto.Status
            };

            _mockCampaignService.Setup(service => service.UpdateCampaignAsync(testId, It.IsAny<CampaignUpdateDto>()))
                .ReturnsAsync(updatedCampaign);

            // Act
            var result = await _controller.UpdateCampaign(testId, campaignUpdateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CampaignDto>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
            Assert.Equal(campaignUpdateDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task DeleteCampaign_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockCampaignService.Setup(service => service.DeleteCampaignAsync(testId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCampaign(testId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ActivateCampaign_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockCampaignService.Setup(service => service.ActivateCampaignAsync(testId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ActivateCampaign(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal("Campaign activated successfully", message.Message);
        }

        [Fact]
        public async Task GetCampaignMetrics_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var metrics = new Dictionary<string, object>
            {
                { "impressions", 5000 },
                { "clicks", 250 },
                { "conversions", 50 },
                { "conversionRate", 0.2 }
            };

            _mockCampaignService.Setup(service => service.GetCampaignMetricsAsync(testId))
                .ReturnsAsync(metrics);

            // Act
            var result = await _controller.GetCampaignMetrics(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Dictionary<string, object>>(okResult.Value);
            Assert.Equal(4, returnValue.Count);
            Assert.Equal(5000, returnValue["impressions"]);
        }

        [Fact]
        public async Task GetCampaignTargetCustomers_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
            };

            _mockCampaignService.Setup(service => service.GetCampaignTargetCustomersAsync(testId, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(customers);

            // Act
            var result = await _controller.GetCampaignTargetCustomers(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetCampaignsByType_ReturnsOkResult_WithCampaigns()
        {
            // Arrange
            var campaignType = CampaignType.Email;
            var testCampaigns = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Email Promotion",
                    Type = campaignType,
                    Status = CampaignStatus.Active
                },
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Newsletter Campaign",
                    Type = campaignType,
                    Status = CampaignStatus.Draft
                }
            };

            _mockCampaignService.Setup(service => service.GetCampaignsByTypeAsync(campaignType))
                .ReturnsAsync(testCampaigns);

            // Act
            var result = await _controller.GetCampaignsByType(campaignType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CampaignDto>>(okResult.Value);
            Assert.Equal(2, ((List<CampaignDto>)returnValue).Count);
            Assert.All(((List<CampaignDto>)returnValue), c => Assert.Equal(campaignType, c.Type));
        }

        [Fact]
        public async Task GetCampaignsByStatus_ReturnsOkResult_WithCampaigns()
        {
            // Arrange
            var campaignStatus = CampaignStatus.Active;
            var testCampaigns = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Active Campaign 1",
                    Type = CampaignType.Email,
                    Status = campaignStatus
                },
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Active Campaign 2",
                    Type = CampaignType.SMS,
                    Status = campaignStatus
                }
            };

            _mockCampaignService.Setup(service => service.GetCampaignsByStatusAsync(campaignStatus))
                .ReturnsAsync(testCampaigns);

            // Act
            var result = await _controller.GetCampaignsByStatus(campaignStatus);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CampaignDto>>(okResult.Value);
            Assert.Equal(2, ((List<CampaignDto>)returnValue).Count);
            Assert.All(((List<CampaignDto>)returnValue), c => Assert.Equal(campaignStatus, c.Status));
        }

        [Fact]
        public async Task GetCampaignsByDateRange_WithValidDates_ReturnsOkResult()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow.AddDays(7);
            var testCampaigns = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Current Campaign",
                    StartDate = startDate.AddDays(1),
                    EndDate = endDate.AddDays(-1)
                },
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Upcoming Campaign",
                    StartDate = endDate.AddDays(-2),
                    EndDate = endDate.AddDays(10)
                }
            };

            _mockCampaignService.Setup(service => service.GetCampaignsByDateRangeAsync(startDate, endDate))
                .ReturnsAsync(testCampaigns);

            // Act
            var result = await _controller.GetCampaignsByDateRange(startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CampaignDto>>(okResult.Value);
            Assert.Equal(2, ((List<CampaignDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetCampaignsByDateRange_WithInvalidDates_ReturnsBadRequest()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(7);
            var endDate = DateTime.UtcNow.AddDays(-7);

            // Act
            var result = await _controller.GetCampaignsByDateRange(startDate, endDate);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCampaignsBySegment_ReturnsOkResult_WithCampaigns()
        {
            // Arrange
            var segmentId = Guid.NewGuid();
            var testCampaigns = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Segment Campaign 1",
                    Type = CampaignType.Email,
                    Status = CampaignStatus.Active
                },
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Segment Campaign 2",
                    Type = CampaignType.SMS,
                    Status = CampaignStatus.Draft
                }
            };

            _mockCampaignService.Setup(service => service.GetCampaignsBySegmentAsync(segmentId))
                .ReturnsAsync(testCampaigns);

            // Act
            var result = await _controller.GetCampaignsBySegment(segmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CampaignDto>>(okResult.Value);
            Assert.Equal(2, ((List<CampaignDto>)returnValue).Count);
        }

        [Fact]
        public async Task UpdateCampaignMetrics_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var metricsUpdateDto = new CampaignMetricsUpdateDto
            {
                Sent = 1000,
                Delivered = 950,
                Opened = 500,
                Clicked = 200,
                Converted = 50,
                ROI = 2.5m
            };

            var updatedCampaign = new CampaignDto
            {
                Id = testId,
                Name = "Test Campaign"
            };

            _mockCampaignService.Setup(service => service.UpdateCampaignMetricsAsync(testId, metricsUpdateDto))
                .ReturnsAsync(updatedCampaign);

            // Act
            var result = await _controller.UpdateCampaignMetrics(testId, metricsUpdateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CampaignDto>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public async Task UpdateCampaignMetrics_WithNullData_ReturnsBadRequest()
        {
            // Arrange
            var testId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateCampaignMetrics(testId, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        private List<CampaignDto> GetTestCampaigns()
        {
            return new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Summer Sale",
                    Description = "Summer promotion",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(30),
                    Type = "Promotion",
                    Budget = 5000,
                    Status = "Active"
                },
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Winter Special",
                    Description = "Winter promotion",
                    StartDate = DateTime.UtcNow.AddDays(60),
                    EndDate = DateTime.UtcNow.AddDays(90),
                    Type = "Seasonal",
                    Budget = 7500,
                    Status = "Draft"
                },
                new CampaignDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Loyalty Program",
                    Description = "Customer loyalty campaign",
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(335),
                    Type = "Loyalty",
                    Budget = 15000,
                    Status = "Active"
                }
            };
        }
    }
}
