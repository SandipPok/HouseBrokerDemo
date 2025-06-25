using AutoFixture;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Application.Services;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Exceptions;
using Moq;
using Xunit;

namespace HouseBroker.Tests.Services
{
    public class PropertyServiceTests
    {
        private readonly Mock<IPropertyRepository> _mockRepo;
        private readonly PropertyService _service;
        private readonly Fixture _fixture;

        public PropertyServiceTests()
        {
            _mockRepo = new Mock<IPropertyRepository>();
            _service = new PropertyService(_mockRepo.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateAsync_ValidProperty_ReturnsCreatedProperty()
        {
            var brokerId = 1;

            var dto = _fixture.Build<Property>()
                .Without(p => p.Id)
                .With(p => p.BrokerId, brokerId)
                .Create();

            var expected = _fixture.Build<Property>()
                .With(p => p.BrokerId, brokerId)
                .Create();

            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Property>()))
                .Returns(Task.FromResult(expected));

            await _service.CreateAsync(dto);

            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Property>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UnauthorizedBroker_ThrowsUnauthorizedException()
        {
            var propertyId = 1;
            var brokerId = 1;

            var dto = _fixture.Build<Property>()
                .With(p => p.BrokerId, brokerId)
                .Create();

            var existing = _fixture.Build<Property>()
                .With(p => p.Id, propertyId)
                .With(p => p.BrokerId, 2)
                .Create();

            _mockRepo.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(existing);

            await Xunit.Assert.ThrowsAsync<UnauthorizedException>(() =>
                _service.UpdateAsync(dto));
        }
    }
}