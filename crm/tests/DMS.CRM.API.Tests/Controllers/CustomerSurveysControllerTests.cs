using DMS.CRM.API.Controllers;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DMS.CRM.API.Tests.Controllers
{
    public class CustomerSurveysControllerTests
    {
        private readonly Mock<ICustomerSurveyService> _mockSurveyService;
        private readonly Mock<ILogger<CustomerSurveysController>> _mockLogger;
        private readonly CustomerSurveysController _controller;

        public CustomerSurveysControllerTests()
        {
            _mockSurveyService = new Mock<ICustomerSurveyService>();
            _mockLogger = new Mock<ILogger<CustomerSurveysController>>();
            _controller = new CustomerSurveysController(_mockSurveyService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllCustomerSurveys_ReturnsOkResult_WithSurveys()
        {
            // Arrange
            var testSurveys = new List<CustomerSurveyDto>
            {
                new CustomerSurveyDto { Id = Guid.NewGuid(), Name = "Customer Satisfaction Survey" },
                new CustomerSurveyDto { Id = Guid.NewGuid(), Name = "Service Quality Survey" }
            };
            
            _mockSurveyService.Setup(service => service.GetAllSurveysAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(testSurveys);

            // Act
            var result = await _controller.GetAllCustomerSurveys();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerSurveyDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerSurveyDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetCustomerSurveyById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testSurvey = new CustomerSurveyDto
            {
                Id = testId,
                Name = "Test Survey"
            };

            _mockSurveyService.Setup(service => service.GetSurveyByIdAsync(testId))
                .ReturnsAsync(testSurvey);

            // Act
            var result = await _controller.GetCustomerSurveyById(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CustomerSurveyDto>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
        }

        [Fact]
        public async Task GetCustomerSurveyById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockSurveyService.Setup(service => service.GetSurveyByIdAsync(testId))
                .ReturnsAsync((CustomerSurveyDto)null);

            // Act
            var result = await _controller.GetCustomerSurveyById(testId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCustomerSurvey_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var surveyCreateDto = new CustomerSurveyCreateDto
            {
                Name = "New Test Survey",
                Description = "Survey Description",
                Type = "Satisfaction",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(14)
            };

            var createdSurvey = new CustomerSurveyDto
            {
                Id = Guid.NewGuid(),
                Name = surveyCreateDto.Name,
                Description = surveyCreateDto.Description,
                Type = surveyCreateDto.Type,
                StartDate = surveyCreateDto.StartDate,
                EndDate = surveyCreateDto.EndDate
            };

            _mockSurveyService.Setup(service => service.CreateSurveyAsync(It.IsAny<CustomerSurveyCreateDto>()))
                .ReturnsAsync(createdSurvey);

            // Act
            var result = await _controller.CreateCustomerSurvey(surveyCreateDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetCustomerSurveyById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<CustomerSurveyDto>(createdAtActionResult.Value);
            Assert.Equal(createdSurvey.Id, returnValue.Id);
            Assert.Equal(surveyCreateDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetSurveysByCustomerId_ReturnsOkResult_WithSurveys()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var testSurveys = new List<CustomerSurveyDto>
            {
                new CustomerSurveyDto { Id = Guid.NewGuid(), Name = "Service Feedback" },
                new CustomerSurveyDto { Id = Guid.NewGuid(), Name = "Product Satisfaction" }
            };

            _mockSurveyService.Setup(service => service.GetSurveysByCustomerIdAsync(customerId, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(testSurveys);

            // Act
            var result = await _controller.GetSurveysByCustomerId(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerSurveyDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerSurveyDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetSurveyAnalytics_ReturnsOkResult_WithAnalytics()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddMonths(-3);
            var endDate = DateTime.UtcNow;
            var analytics = new Dictionary<string, object>
            {
                { "totalSurveys", 10 },
                { "totalResponses", 120 },
                { "responseRate", 0.85 },
                { "averageSatisfaction", 4.2 }
            };

            _mockSurveyService.Setup(service => service.GetSurveyAnalyticsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(analytics);

            // Act
            var result = await _controller.GetSurveyAnalytics(startDate, endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Dictionary<string, object>>(okResult.Value);
            Assert.Equal(4, returnValue.Count);
            Assert.Equal(4.2, returnValue["averageSatisfaction"]);
        }
    }
}
