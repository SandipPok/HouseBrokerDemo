using AutoFixture;
using FluentAssertions;
using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Application.Services;
using HouseBroker.Domain.Entities;
using Moq;
using Xunit;

namespace HouseBroker.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly AuthService _authService;
        private readonly Fixture _fixture;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();
            _authService = new AuthService(_mockUserRepo.Object, _mockJwtService.Object, _mockPasswordHasher.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginDto = _fixture.Create<LoginDto>();
            var user = _fixture.Create<User>();
            var token = "test_token";

            _mockUserRepo.Setup(r => r.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            _mockPasswordHasher.Setup(h => h.VerifyPassword(loginDto.Password, user.PasswordHash))
                .Returns(true);

            _mockJwtService.Setup(j => j.GenerateToken(user))
                .Returns(token);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().Be(token);
        }
    }
}
