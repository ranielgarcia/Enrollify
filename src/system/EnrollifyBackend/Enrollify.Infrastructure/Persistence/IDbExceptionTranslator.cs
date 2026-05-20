using Enrollify.Application.Command.Persistence;

namespace Enrollify.Infrastructure.Persistence;

public interface IDbExceptionTranslator
{
    PersistenceError? Translate(DbUpdateException ex);
}
