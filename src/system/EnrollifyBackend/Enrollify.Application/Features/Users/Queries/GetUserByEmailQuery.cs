using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Aggregates.UserAggregate.Specifications;

namespace Enrollify.Application.Features.Users.Queries;

public record GetUserByEmailQuery(UserEmail email) : IRequest<Result<UserDto>>;

public class GetUserByEmailQueryHandler(IReadRepository<User> _repository)
    : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        var spec = new UserByEmailSpec(query.email);
        var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
        if (entity == null) return Result.NotFound();

        return UserDto.FromUser(entity);
    }
}
