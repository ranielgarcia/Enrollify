using Ardalis.Result;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Aggregates.UserAggregate.Specifications;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Users.Get;

public record GetUserByEmailQuery(UserEmail email) : IQuery<Result<UserDTO>>;

public class GetUserByEmailQueryHandler(IReadRepository<User> _repository)
    : IQueryHandler<GetUserByEmailQuery, Result<UserDTO>>
{
    public async ValueTask<Result<UserDTO>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        var spec = new UserByEmailSpec(query.email);
        var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        if (entity == null) return Result.NotFound();

        return UserDTO.FromUser(entity);
    }
}
