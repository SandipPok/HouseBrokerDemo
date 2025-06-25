using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userRepository.GetUserByEmailAsync(registerDto.Email) != null)
                throw new Exception("Email already exists");

            var passwordHash = _passwordHasher.HashPassword(registerDto.Password);

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Role = registerDto.Role,
                PasswordHash = passwordHash
            };

            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email) ??
                throw new UnauthorizedAccessException("Invalid credentials");

            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            return _jwtService.GenerateToken(user);
        }
    }
}