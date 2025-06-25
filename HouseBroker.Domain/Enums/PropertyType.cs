using System.Text.Json.Serialization;

namespace HouseBroker.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PropertyType
    {
        Apartment,
        House,
        Condo,
        Townhouse,
        Land,
        Commercial
    }
}