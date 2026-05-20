using Enrollify.Application.Command.Persistence;
using Microsoft.Data.SqlClient;

namespace Enrollify.Infrastructure.Persistence;

public class SqlServerExceptionTranslator : IDbExceptionTranslator
{
    public PersistenceError? Translate(DbUpdateException ex)
    {
        if (ex.InnerException is not SqlException sqlEx)
            return new PersistenceError(PersistenceErrorType.Unknown, string.Empty, ex.Message);

        return sqlEx.Number switch
        {
            2601 or 2627 => new PersistenceError(
                PersistenceErrorType.UniqueConstraintViolation,
                ExtractConstraintName(sqlEx.Message),
                sqlEx.Message),

            547 => new PersistenceError(
                PersistenceErrorType.ForeignKeyViolation,
                ExtractConstraintName(sqlEx.Message),
                sqlEx.Message),

            _ => new PersistenceError(PersistenceErrorType.Unknown, string.Empty, sqlEx.Message)
        };
    }

    private static string ExtractConstraintName(string message)
    {
        // SQL Server format: "...constraint \"UQ_Buildings_Name\"..."
        var start = message.IndexOf('"');
        if (start < 0) return string.Empty;
        var end = message.IndexOf('"', start + 1);
        return end > start ? message[(start + 1)..end] : string.Empty;
    }
}
