using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}