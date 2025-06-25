using HouseBroker.Application.Dtos;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<Property> GetByIdAsync(int id);
        Task<IEnumerable<Property>> GetAllAsync();
        Task CreateAsync(Property property);
        Task UpdateAsync(Property property);
        Task DeleteAsync(int id);
        Task<PaginatedResult<Property>> SearchAsync(PropertySearchFilters filters);
    }
}