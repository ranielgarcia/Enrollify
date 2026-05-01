namespace Enrollify.Application;

public record PagedResult<T>(
  IReadOnlyList<T> Items,
  int Page,
  int PageSize,
  int TotalCount)
{
    public int TotalPages { get; } = (int)Math.Ceiling(TotalCount / (double)PageSize);
}
