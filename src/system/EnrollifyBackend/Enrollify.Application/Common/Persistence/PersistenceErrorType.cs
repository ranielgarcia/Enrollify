namespace Enrollify.Application.Common.Persistence;

public enum PersistenceErrorType
{
    UniqueConstraintViolation,
    ForeignKeyViolation,
    NotFound,
    Unknown
}

public record PersistenceError(
    PersistenceErrorType Type,
    string ConstraintName,
    string Message);
