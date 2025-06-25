using Dapper;
using HouseBroker.Application.Interfaces;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IPasswordHasher _passwordHasher;

        public UserRepository(IDbConnectionFactory connectionFactory, IPasswordHasher passwordHasher)
        {
            _connectionFactory = connectionFactory;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
            INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role) 
            OUTPUT INSERTED.Id, INSERTED.CreatedDate
            VALUES (@FirstName, @LastName, @Email, @PasswordHash, @Role)";

            var result = await connection.QuerySingleAsync<User>(sql, user);
            user.Id = result.Id;
            user.CreatedDate = result.CreatedDate;
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            return _passwordHasher.VerifyPassword(password, user.PasswordHash);
        }
    }
}
