using HouseBroker.Domain.Enums;
using HouseBroker.Domain.ValueObjects;

namespace HouseBroker.Application.Dtos
{
    /// <summary>
    /// DTO for property responses that includes broker contact details
    /// </summary>
    public class PropertyDto
    {
        public int Id { get; set; }
        public PropertyType Type { get; set; }
        public required Location Location { get; set; }
        public required Money Price { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public BrokerContactDto Broker { get; set; } = new();
    }

    /// <summary>
    /// DTO for broker contact information
    /// </summary>
    public class BrokerContactDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
    }
}
