using AutoFixture;
using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Enums;
using HouseBroker.Domain.ValueObjects;
using HouseBroker.Presentation.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;
using Assert = Xunit.Assert;

namespace HouseBroker.Tests.Controllers
{
    public class PropertiesControllerTests
    {
        private readonly Mock<IPropertyService> _mockService;
        private readonly Mock<ILogger<PropertiesController>> _mockLogger;
        private readonly PropertiesController _controller;
        private readonly Fixture _fixture;

        public PropertiesControllerTests()
        {
            _mockService = new Mock<IPropertyService>();
            _mockLogger = new Mock<ILogger<PropertiesController>>();
            _controller = new PropertiesController(_mockService.Object, _mockLogger.Object);
            _fixture = new Fixture();
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkWithProperties()
        {
            // Arrange
            var properties = new List<PropertyDto>
            {
                CreateValidPropertyDto(),
                CreateValidPropertyDto()
            };

            _mockService.Setup(s => s.GetAllWithBrokerAsync())
                .ReturnsAsync(properties);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperties = Assert.IsAssignableFrom<IEnumerable<PropertyDto>>(okResult.Value);
            Assert.Equal(2, returnedProperties.Count());
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkWithProperty()
        {
            // Arrange
            var propertyId = 1;
            var property = CreateValidPropertyDto();

            _mockService.Setup(s => s.GetByIdWithBrokerAsync(propertyId))
                .ReturnsAsync(property);

            // Act
            var result = await _controller.GetById(propertyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProperty = Assert.IsType<PropertyDto>(okResult.Value);
            Assert.Equal(property.Id, returnedProperty.Id);
        }

        [Fact]
        public async Task GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var propertyId = 999;

            _mockService.Setup(s => s.GetByIdWithBrokerAsync(propertyId))
                .ReturnsAsync((PropertyDto?)null);

            // Act
            var result = await _controller.GetById(propertyId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ValidProperty_ReturnsOk()
        {
            // Arrange
            var createDto = new CreatePropertyDto
            {
                Type = PropertyType.House,
                Location = new Location("123 Main St", "Downtown", "CityCenter"),
                Price = new Money(250000, "USD"),
                Description = "Beautiful house",
                Features = "3 bedrooms, 2 bathrooms",
                ImageUrls = new List<string> { "image1.jpg" }
            };

            SetupControllerWithUser("1", "Broker");

            var createdProperty = CreateValidPropertyDto();
            _mockService.Setup(s => s.CreateAsync(createDto, 1))
                .ReturnsAsync(createdProperty);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
            var returnedProperty = Assert.IsType<PropertyDto>(createdResult.Value);
            Assert.Equal(createdProperty.Id, returnedProperty.Id);
        }

        [Fact]
        public async Task Create_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreatePropertyDto
            {
                Type = PropertyType.House,
                Location = new Location("123 Main St", "Downtown", "CityCenter"),
                Price = new Money(250000, "USD")
            };

            SetupControllerWithUser("1", "Broker");

            _mockService.Setup(s => s.CreateAsync(createDto, 1))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            var propertyId = 1;
            var updateDto = new UpdatePropertyDto
            {
                Type = PropertyType.Apartment,
                Location = new Location("456 Oak St", "Uptown", "CityNorth"),
                Price = new Money(300000, "USD"),
                Description = "Updated description",
                Features = "Updated features"
            };

            SetupControllerWithUser("1", "Broker");

            _mockService.Setup(s => s.UpdateAsync(propertyId, updateDto, 1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(propertyId, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var propertyId = 1;
            var updateDto = new UpdatePropertyDto
            {
                Type = PropertyType.House,
                Location = new Location("123 Main St", "Downtown", "CityCenter"),
                Price = new Money(250000, "USD")
            };

            SetupControllerWithUser("1", "Broker");

            _mockService.Setup(s => s.UpdateAsync(propertyId, updateDto, 1))
                .ThrowsAsync(new UnauthorizedAccessException("Not authorized"));

            // Act
            var result = await _controller.Update(propertyId, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ExistingProperty_ReturnsNoContent()
        {
            // Arrange
            var propertyId = 1;
            var property = CreateValidProperty();
            property.BrokerId = 1;

            SetupControllerWithUser("1", "Broker");

            _mockService.Setup(s => s.GetByIdAsync(propertyId))
                .ReturnsAsync(property);
            _mockService.Setup(s => s.DeleteAsync(propertyId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(propertyId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingProperty_ReturnsNotFound()
        {
            // Arrange
            var propertyId = 999;

            SetupControllerWithUser("1", "Broker");

            _mockService.Setup(s => s.GetByIdAsync(propertyId))
                .ReturnsAsync((Property?)null);

            // Act
            var result = await _controller.Delete(propertyId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_UnauthorizedBroker_ReturnsForbid()
        {
            // Arrange
            var propertyId = 1;
            var property = CreateValidProperty();
            property.BrokerId = 2; // Different broker

            SetupControllerWithUser("1", "Broker");

            _mockService.Setup(s => s.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            // Act
            var result = await _controller.Delete(propertyId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        #endregion

        #region Search Tests

        [Fact]
        public async Task Search_WithFilters_ReturnsOkWithResults()
        {
            // Arrange
            var filters = new PropertySearchFilters(
                Location: "Downtown",
                MinPrice: 100000,
                MaxPrice: 500000
            );

            var searchResults = new PaginatedResult<PropertyDto>(
                Items: new List<PropertyDto> { CreateValidPropertyDto() },
                Page: 1,
                PageSize: 20,
                TotalCount: 1
            );

            _mockService.Setup(s => s.SearchWithBrokerAsync(filters))
                .ReturnsAsync(searchResults);

            // Act
            var result = await _controller.Search(filters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResults = Assert.IsType<PaginatedResult<PropertyDto>>(okResult.Value);
            Assert.Equal(1, returnedResults.TotalCount);
        }

        [Fact]
        public async Task Search_NoFilters_ReturnsAllProperties()
        {
            // Arrange
            var filters = new PropertySearchFilters();
            var searchResults = new PaginatedResult<PropertyDto>(
                Items: new List<PropertyDto> { CreateValidPropertyDto(), CreateValidPropertyDto() },
                Page: 1,
                PageSize: 20,
                TotalCount: 2
            );

            _mockService.Setup(s => s.SearchWithBrokerAsync(filters))
                .ReturnsAsync(searchResults);

            // Act
            var result = await _controller.Search(filters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedResults = Assert.IsType<PaginatedResult<PropertyDto>>(okResult.Value);
            Assert.Equal(2, returnedResults.TotalCount);
        }

        #endregion

        #region Helper Methods

        private void SetupControllerWithUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        private Property CreateValidProperty()
        {
            return new Property
            {
                Id = _fixture.Create<int>(),
                Type = PropertyType.House,
                Location = new Location("123 Main St", "Downtown", "CityCenter"),
                Price = new Money(250000, "USD"),
                Description = "Test Property",
                Features = "3 bedrooms, 2 bathrooms",
                BrokerId = 1,
                ImageUrls = new List<string> { "image1.jpg", "image2.jpg" }
            };
        }

        private PropertyDto CreateValidPropertyDto()
        {
            return new PropertyDto
            {
                Id = _fixture.Create<int>(),
                Type = PropertyType.House,
                Location = new Location("123 Main St", "Downtown", "CityCenter"),
                Price = new Money(250000, "USD"),
                Description = "Test Property",
                Features = "3 bedrooms, 2 bathrooms",
                ImageUrls = new List<string> { "image1.jpg", "image2.jpg" },
                Broker = new BrokerContactDto
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                }
            };
        }

        #endregion
    }
}
