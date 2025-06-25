namespace HouseBroker.Application.Dtos
{
    public record PropertySearchFilters(
        string? Location = null,
        decimal? MinPrice = null,
        decimal? MaxPrice = null,
        string? PropertyType = null,
        int Page = 1,
        int PageSize = 20
    );
}