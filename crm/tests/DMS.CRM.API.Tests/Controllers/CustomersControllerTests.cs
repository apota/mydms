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
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly Mock<ILogger<CustomersController>> _mockLogger;
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockLogger = new Mock<ILogger<CustomersController>>();
            _controller = new CustomersController(_mockCustomerService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsOkResult_WithCustomers()
        {
            // Arrange
            var testCustomers = new List<CustomerDto>
            {
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" }
            };
            
            _mockCustomerService.Setup(service => service.GetAllCustomersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(testCustomers);

            // Act
            var result = await _controller.GetAllCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetCustomerById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testCustomer = new CustomerDto
            {
                Id = testId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            _mockCustomerService.Setup(service => service.GetCustomerByIdAsync(testId))
                .ReturnsAsync(testCustomer);

            // Act
            var result = await _controller.GetCustomerById(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CustomerDto>(okResult.Value);
            Assert.Equal(testId, returnValue.Id);
            Assert.Equal("John", returnValue.FirstName);
        }

        [Fact]
        public async Task GetCustomerById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockCustomerService.Setup(service => service.GetCustomerByIdAsync(testId))
                .ReturnsAsync((CustomerDto)null);

            // Act
            var result = await _controller.GetCustomerById(testId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCustomer_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var customerCreateDto = new CustomerCreateDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890"
            };

            var createdCustomer = new CustomerDto
            {
                Id = Guid.NewGuid(),
                FirstName = customerCreateDto.FirstName,
                LastName = customerCreateDto.LastName,
                Email = customerCreateDto.Email,
                PhoneNumber = customerCreateDto.PhoneNumber
            };

            _mockCustomerService.Setup(service => service.CreateCustomerAsync(It.IsAny<CustomerCreateDto>()))
                .ReturnsAsync(createdCustomer);

            // Act
            var result = await _controller.CreateCustomer(customerCreateDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetCustomerById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<CustomerDto>(createdAtActionResult.Value);
            Assert.Equal(createdCustomer.Id, returnValue.Id);
            Assert.Equal(customerCreateDto.FirstName, returnValue.FirstName);
        }

        [Fact]
        public async Task SearchCustomers_WithValidQuery_ReturnsMatchingCustomers()
        {
            // Arrange
            var searchTerm = "Smith";
            var testCustomers = new List<CustomerDto>
            {
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" },
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "Robert", LastName = "Smith" }
            };
            
            _mockCustomerService.Setup(service => service.SearchCustomersAsync(searchTerm, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(testCustomers);

            // Act
            var result = await _controller.SearchCustomers(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerDto>)returnValue).Count);
        }

        [Fact]
        public async Task GetCustomerVehicles_WithValidId_ReturnsCustomerVehicles()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var testVehicles = new List<CustomerVehicleDto>
            {
                new CustomerVehicleDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Make = "Toyota",
                    Model = "Camry",
                    Year = 2020
                },
                new CustomerVehicleDto
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Make = "Honda",
                    Model = "Accord",
                    Year = 2018
                }
            };
            
            _mockCustomerService.Setup(service => service.GetCustomerVehiclesAsync(customerId, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(testVehicles);

            // Act
            var result = await _controller.GetCustomerVehicles(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CustomerVehicleDto>>(okResult.Value);
            Assert.Equal(2, ((List<CustomerVehicleDto>)returnValue).Count);
        }

        [Fact]
        public async Task UpdateCustomer_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customerUpdateDto = new CustomerUpdateDto
            {
                FirstName = "John",
                LastName = "Updated",
                Email = "john.updated@example.com"
            };

            var updatedCustomer = new CustomerDto
            {
                Id = customerId,
                FirstName = customerUpdateDto.FirstName,
                LastName = customerUpdateDto.LastName,
                Email = customerUpdateDto.Email
            };

            _mockCustomerService.Setup(service => service.UpdateCustomerAsync(customerId, It.IsAny<CustomerUpdateDto>()))
                .ReturnsAsync(updatedCustomer);

            // Act
            var result = await _controller.UpdateCustomer(customerId, customerUpdateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<CustomerDto>(okResult.Value);
            Assert.Equal(customerId, returnValue.Id);
            Assert.Equal("Updated", returnValue.LastName);
        }

        [Fact]
        public async Task DeleteCustomer_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockCustomerService.Setup(service => service.DeleteCustomerAsync(customerId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCustomer(customerId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
