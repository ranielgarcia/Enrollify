using Enrollify.Application.Common.Persistence;

namespace Enrollify.Infrastructure.Persistence;

public interface IDbExceptionTranslator
{
    PersistenceError? Translate(DbUpdateException ex);
}
