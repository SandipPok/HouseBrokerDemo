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
        public List<string> ImageUrls { get; set; } = new();
        public User? Broker { get; set; }

        public void UpdatePrice(decimal newAmount) => Price = Price with { Amount = newAmount };

        /// <summary>
        /// Add an image URL to the property
        /// </summary>
        /// <param name="imageUrl">The URL of the image to add</param>
        public void AddImage(string imageUrl)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl) && !ImageUrls.Contains(imageUrl))
            {
                ImageUrls.Add(imageUrl);
            }
        }

        /// <summary>
        /// Remove an image URL from the property
        /// </summary>
        /// <param name="imageUrl">The URL of the image to remove</param>
        public void RemoveImage(string imageUrl)
        {
            ImageUrls.Remove(imageUrl);
        }
    }
}