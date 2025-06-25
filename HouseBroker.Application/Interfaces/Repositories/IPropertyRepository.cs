using HouseBroker.Application.Dtos;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Interfaces.Repositories
{
    public interface IPropertyRepository
    {
        Task<Property> GetByIdAsync(int id);
        Task<IEnumerable<Property>> GetAllAsync();
        Task AddAsync(Property property);
        Task UpdateAsync(Property property);
        Task DeleteAsync(int id);
        Task<PaginatedResult<Property>> SearchAsync(PropertySearchFilters filters);
    }
}