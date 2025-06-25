using AutoFixture;
using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Application.Services;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Enums;
using HouseBroker.Domain.Exceptions;
using HouseBroker.Domain.ValueObjects;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace HouseBroker.Tests.Services
{
    public class PropertyServiceTestsExtended
    {
        private readonly Mock<IPropertyRepository> _mockRepo;
        private readonly PropertyService _service;
        private readonly Fixture _fixture;

        public PropertyServiceTestsExtended()
        {
            _mockRepo = new Mock<IPropertyRepository>();
            _service = new PropertyService(_mockRepo.Object);
            _fixture = new Fixture();
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsProperty()
        {
            // Arrange
            var propertyId = 1;
            var expectedProperty = CreateValidProperty();

            _mockRepo.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(expectedProperty);

            // Act
            var result = await _service.GetByIdAsync(propertyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProperty.Id, result.Id);
            _mockRepo.Verify(r => r.GetByIdAsync(propertyId), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var propertyId = 999;
            _mockRepo.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync((Property?)null);

            // Act
            var result = await _service.GetByIdAsync(propertyId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetByIdWithBrokerAsync Tests

        [Fact]
        public async Task GetByIdWithBrokerAsync_ExistingId_ReturnsPropertyWithBroker()
        {
            // Arrange
            var propertyId = 1;
            var expectedPropertyDto = CreateValidPropertyDto();

            _mockRepo.Setup(r => r.GetByIdWithBrokerAsync(propertyId))
                .ReturnsAsync(expectedPropertyDto);

            // Act
            var result = await _service.GetByIdWithBrokerAsync(propertyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPropertyDto.Id, result.Id);
            Assert.NotNull(result.Broker);
            Assert.Equal(expectedPropertyDto.Broker.Email, result.Broker.Email);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidProperty_CallsRepository()
        {
            // Arrange
            var property = CreateValidProperty();

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Property>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.CreateAsync(property);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(property), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithCreateDto_CreatesPropertyAndCallsRepository()
        {
            // Arrange
            var createDto = new CreatePropertyDto
            {
                Type = PropertyType.House,
                Location = new Location("123 Main St", "Downtown", "CityCenter"),
                Price = new Money(250000, "USD"),
                Description = "Beautiful house",
                Features = "3 bedrooms, 2 bathrooms",
                ImageUrls = new List<string> { "image1.jpg", "image2.jpg" }
            };
            var brokerId = 1;

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Property>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.CreateAsync(createDto, brokerId);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(It.Is<Property>(p =>
                p.Type == createDto.Type &&
                p.BrokerId == brokerId &&
                p.ImageUrls.Count == 2)), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidProperty_UpdatesSuccessfully()
        {
            // Arrange
            var property = CreateValidProperty();
            var existingProperty = CreateValidProperty();
            existingProperty.BrokerId = property.BrokerId; // Same broker

            _mockRepo.Setup(r => r.GetByIdAsync(property.Id))
                .ReturnsAsync(existingProperty);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Property>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(property);

            // Assert
            _mockRepo.Verify(r => r.UpdateAsync(property), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithUnauthorizedBroker_ThrowsUnauthorizedException()
        {
            // Arrange
            var property = CreateValidProperty();
            property.BrokerId = 1;

            var existingProperty = CreateValidProperty();
            existingProperty.BrokerId = 2; // Different broker

            _mockRepo.Setup(r => r.GetByIdAsync(property.Id))
                .ReturnsAsync(existingProperty);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _service.UpdateAsync(property));
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingProperty_ThrowsKeyNotFoundException()
        {
            // Arrange
            var property = CreateValidProperty();

            _mockRepo.Setup(r => r.GetByIdAsync(property.Id))
                .ReturnsAsync((Property?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(property));
        }

        [Fact]
        public async Task UpdateAsync_WithUpdateDto_UpdatesSuccessfully()
        {
            // Arrange
            var propertyId = 1;
            var brokerId = 1;
            var updateDto = new UpdatePropertyDto
            {
                Type = PropertyType.Apartment,
                Location = new Location("456 Oak St", "Uptown", "CityNorth"),
                Price = new Money(300000, "USD"),
                Description = "Updated description",
                Features = "Updated features",
                ImageUrls = new List<string> { "newimage1.jpg" }
            };

            var existingProperty = CreateValidProperty();
            existingProperty.BrokerId = brokerId;

            _mockRepo.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(existingProperty);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Property>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateAsync(propertyId, updateDto, brokerId);

            // Assert
            _mockRepo.Verify(r => r.UpdateAsync(It.Is<Property>(p =>
                p.Type == updateDto.Type &&
                p.Description == updateDto.Description &&
                p.ImageUrls.Count == 1)), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidId_CallsRepository()
        {
            // Arrange
            var propertyId = 1;
            _mockRepo.Setup(r => r.DeleteAsync(propertyId))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(propertyId);

            // Assert
            _mockRepo.Verify(r => r.DeleteAsync(propertyId), Times.Once);
        }

        #endregion

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithFilters_ReturnsFilteredResults()
        {
            // Arrange
            var filters = new PropertySearchFilters(
                Location: "Downtown",
                MinPrice: 100000,
                MaxPrice: 500000,
                PropertyType: "House"
            );

            var expectedResult = new PaginatedResult<Property>(
                Items: new List<Property> { CreateValidProperty() },
                Page: 1,
                PageSize: 20,
                TotalCount: 1
            );

            _mockRepo.Setup(r => r.SearchAsync(filters))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SearchAsync(filters);

            // Assert
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task SearchWithBrokerAsync_WithFilters_ReturnsFilteredResultsWithBrokerInfo()
        {
            // Arrange
            var filters = new PropertySearchFilters(Location: "Downtown");
            var expectedResult = new PaginatedResult<PropertyDto>(
                Items: new List<PropertyDto> { CreateValidPropertyDto() },
                Page: 1,
                PageSize: 20,
                TotalCount: 1
            );

            _mockRepo.Setup(r => r.SearchWithBrokerAsync(filters))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.SearchWithBrokerAsync(filters);

            // Assert
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Single(result.Items);
            Assert.NotNull(result.Items.First().Broker);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllProperties()
        {
            // Arrange
            var expectedProperties = new List<Property>
            {
                CreateValidProperty(),
                CreateValidProperty()
            };

            _mockRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(expectedProperties);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllWithBrokerAsync_ReturnsAllPropertiesWithBrokerInfo()
        {
            // Arrange
            var expectedProperties = new List<PropertyDto>
            {
                CreateValidPropertyDto(),
                CreateValidPropertyDto()
            };

            _mockRepo.Setup(r => r.GetAllWithBrokerAsync())
                .ReturnsAsync(expectedProperties);

            // Act
            var result = await _service.GetAllWithBrokerAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.NotNull(p.Broker));
        }

        #endregion

        #region Helper Methods

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
