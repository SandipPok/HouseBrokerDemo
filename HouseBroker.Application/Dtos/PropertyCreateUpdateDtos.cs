using HouseBroker.Domain.Enums;
using HouseBroker.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace HouseBroker.Application.Dtos
{
    /// <summary>
    /// DTO for creating a new property
    /// </summary>
    public class CreatePropertyDto
    {
        [Required(ErrorMessage = "Property type is required")]
        public PropertyType Type { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public required Location Location { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public required Money Price { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Features cannot exceed 500 characters")]
        public string? Features { get; set; }

        public List<string> ImageUrls { get; set; } = new();
    }

    /// <summary>
    /// DTO for updating an existing property
    /// </summary>
    public class UpdatePropertyDto
    {
        [Required(ErrorMessage = "Property type is required")]
        public PropertyType Type { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public required Location Location { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public required Money Price { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Features cannot exceed 500 characters")]
        public string? Features { get; set; }

        public List<string> ImageUrls { get; set; } = new();
    }
}
