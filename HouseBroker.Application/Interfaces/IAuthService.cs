using HouseBroker.Application.Dtos;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
    }
}