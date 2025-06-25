using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces;
using HouseBroker.Application.Interfaces.Repositories;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Exceptions;

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

        public async Task<PropertyDto> CreateAsync(CreatePropertyDto createDto, int brokerId)
        {
            var property = new Property
            {
                Type = createDto.Type,
                Location = createDto.Location,
                Price = createDto.Price,
                Description = createDto.Description,
                Features = createDto.Features,
                BrokerId = brokerId,
                ImageUrls = createDto.ImageUrls
            };

            await _propertyRepository.AddAsync(property);
            
            // Return the created property with broker details
            var createdProperty = await _propertyRepository.GetByIdWithBrokerAsync(property.Id);
            return createdProperty!;
        }

        public async Task DeleteAsync(int id) =>
            await _propertyRepository.DeleteAsync(id);

        public async Task<IEnumerable<Property>> GetAllAsync() =>
            await _propertyRepository.GetAllAsync();

        public async Task<IEnumerable<PropertyDto>> GetAllWithBrokerAsync() =>
            await _propertyRepository.GetAllWithBrokerAsync();

        public async Task<Property?> GetByIdAsync(int id) =>
            await _propertyRepository.GetByIdAsync(id);

        public async Task<PropertyDto?> GetByIdWithBrokerAsync(int id) =>
            await _propertyRepository.GetByIdWithBrokerAsync(id);

        public async Task UpdateAsync(Property property)
        {
            var existingProperty = await GetByIdAsync(property.Id);
            if (existingProperty == null)
            {
                throw new KeyNotFoundException($"Property with ID {property.Id} not found.");
            }

            // Authorization check
            if (existingProperty.BrokerId != property.BrokerId)
            {
                throw new UnauthorizedException();
            }
            await _propertyRepository.UpdateAsync(property);
        }

        public async Task UpdateAsync(int id, UpdatePropertyDto updateDto, int brokerId)
        {
            var existingProperty = await GetByIdAsync(id);
            if (existingProperty == null)
            {
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }

            // Authorization check
            if (existingProperty.BrokerId != brokerId)
            {
                throw new UnauthorizedException();
            }

            // Update the property
            existingProperty.Type = updateDto.Type;
            existingProperty.Location = updateDto.Location;
            existingProperty.Price = updateDto.Price;
            existingProperty.Description = updateDto.Description;
            existingProperty.Features = updateDto.Features;
            existingProperty.ImageUrls = updateDto.ImageUrls;

            await _propertyRepository.UpdateAsync(existingProperty);
        }

        public async Task<PaginatedResult<Property>> SearchAsync(PropertySearchFilters filters)
            => await _propertyRepository.SearchAsync(filters);

        public async Task<PaginatedResult<PropertyDto>> SearchWithBrokerAsync(PropertySearchFilters filters)
            => await _propertyRepository.SearchWithBrokerAsync(filters);
    }
}