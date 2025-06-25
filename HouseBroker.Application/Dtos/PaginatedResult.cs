namespace HouseBroker.Application.Dtos
{
    public record PaginatedResult<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount
    )
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}