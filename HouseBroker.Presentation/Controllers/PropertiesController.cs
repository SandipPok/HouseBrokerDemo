using HouseBroker.Application.Dtos;
using HouseBroker.Application.Interfaces;
using HouseBroker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HouseBroker.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(
            IPropertyService propertyService,
            ILogger<PropertiesController> logger)
        {
            _propertyService = propertyService;
            _logger = logger;
        }

        /// <summary>
        /// Get all properties with broker contact details
        /// </summary>
        /// <returns>List of properties with broker information</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var properties = await _propertyService.GetAllWithBrokerAsync();
            return Ok(properties);
        }

        /// <summary>
        /// Get property by ID with broker contact details
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <returns>Property details with broker information</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var property = await _propertyService.GetByIdWithBrokerAsync(id);
            if (property == null)
                return NotFound();

            return Ok(property);
        }

        /// <summary>
        /// Create new property listing
        /// </summary>
        /// <param name="createDto">Property data</param>
        /// <returns>Created property with broker details</returns>
        [HttpPost]
        [Authorize(Policy = "BrokerOnly")]
        [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreatePropertyDto createDto)
        {
            try
            {
                var brokerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var createdProperty = await _propertyService.CreateAsync(createDto, brokerId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdProperty.Id },
                    createdProperty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating property");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update existing property
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <param name="updateDto">Updated property data</param>
        [HttpPut("{id}")]
        [Authorize(Policy = "BrokerOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyDto updateDto)
        {
            try
            {
                var brokerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _propertyService.UpdateAsync(id, updateDto, brokerId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating property {id}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a property
        /// </summary>
        /// <param name="id">Property ID</param>
        [HttpDelete("{id}")]
        [Authorize(Policy = "BrokerOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var property = await _propertyService.GetByIdAsync(id);
                if (property == null)
                    return NotFound();

                var brokerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (property.BrokerId != brokerId)
                    return Forbid();

                await _propertyService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting property {id}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search properties with filters and broker contact details
        /// </summary>
        /// <param name="filters">Search filters</param>
        /// <returns>Filtered properties with broker information</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResult<PropertyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] PropertySearchFilters filters)
        {
            var results = await _propertyService.SearchWithBrokerAsync(filters);
            return Ok(results);
        }
    }
}
