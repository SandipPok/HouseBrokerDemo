using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
    }
}