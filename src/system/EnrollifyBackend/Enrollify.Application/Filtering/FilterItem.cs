namespace Enrollify.Application.Filtering;

public record FilterItem
{
    public required string Id { get; init; }
    public required string Value { get; init; }
    public required string Variant { get; init; }
    public required string Operator { get; init; }
    public required string FilterId { get; init; }
}
