using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;

        public PropertyService(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        public async Task CreateAsync(Property property) =>
            await _propertyRepository.AddAsync(property);

        public async Task DeleteAsync(int id) =>
            await _propertyRepository.DeleteAsync(id);

        public async Task<IEnumerable<Property>> GetAllAsync() =>
            await _propertyRepository.GetAllAsync();

        public async Task<Property> GetByIdAsync(int id) =>
            await _propertyRepository.GetByIdAsync(id);

        public async Task UpdateAsync(Property property) =>
            await _propertyRepository.UpdateAsync(property);

        public async Task<PaginatedResult<Property>> SearchAsync(PropertySearchFilters filters)
            => await _propertyRepository.SearchAsync(filters);
    }
}