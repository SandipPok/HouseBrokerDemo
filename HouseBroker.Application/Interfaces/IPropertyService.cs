using HouseBroker.Application.Dtos;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Interfaces
{
    public interface IPropertyService
    {
        Task<Property?> GetByIdAsync(int id);
        Task<PropertyDto?> GetByIdWithBrokerAsync(int id);
        Task<IEnumerable<Property>> GetAllAsync();
        Task<IEnumerable<PropertyDto>> GetAllWithBrokerAsync();
        Task CreateAsync(Property property);
        Task<PropertyDto> CreateAsync(CreatePropertyDto createDto, int brokerId);
        Task UpdateAsync(Property property);
        Task UpdateAsync(int id, UpdatePropertyDto updateDto, int brokerId);
        Task DeleteAsync(int id);
        Task<PaginatedResult<Property>> SearchAsync(PropertySearchFilters filters);
        Task<PaginatedResult<PropertyDto>> SearchWithBrokerAsync(PropertySearchFilters filters);
    }
}