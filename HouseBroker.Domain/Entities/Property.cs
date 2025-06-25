using HouseBroker.Domain.Enums;
using HouseBroker.Domain.ValueObjects;

namespace HouseBroker.Domain.Entities
{
    public class Property
    {
        public int Id { get; set; }
        public required PropertyType Type { get; set; }
        public required Location Location { get; set; }
        public required Money Price { get; set; }
        public string? Description { get; set; }
        public string? Features { get; set; }
        public int BrokerId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public void UpdatePrice(decimal newAmount) => Price = Price with { Amount = newAmount };
    }
}