namespace HouseBroker.Domain.ValueObjects
{
    public record Money(decimal Amount, string Currency = "USD")
    {
        public override string ToString() => $"{Amount} {Currency}";
    }
}