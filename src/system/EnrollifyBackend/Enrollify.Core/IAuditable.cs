namespace Enrollify.Core;

public interface IAuditable<T>
{
    public T AuditInfo { get; init; }
}
