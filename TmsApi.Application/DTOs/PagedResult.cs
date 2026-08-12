namespace TmsApi.Application.DTOs
{
    public record PagedResult<T>(
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages,
        IReadOnlyList<T> Data
    );
}
