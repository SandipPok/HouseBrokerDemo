namespace HouseBroker.Domain.ValueObjects
{
    public record Location(string Street, string City, string PostalCode)
    {
        public override string ToString() => $"{Street}, {City}, {PostalCode}";

        public bool Contains(string query) =>
            Street.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            City.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            PostalCode.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}