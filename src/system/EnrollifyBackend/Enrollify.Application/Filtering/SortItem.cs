namespace Enrollify.Application.Filtering;

public record SortItem
{
    public required string Id { get; init; }
    public required bool Desc { get; init; }
}
