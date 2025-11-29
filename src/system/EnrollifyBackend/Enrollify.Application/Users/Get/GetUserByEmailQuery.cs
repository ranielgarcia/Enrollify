using Ardalis.Result;
using Enrollify.Core.Aggregates.UserAggregate;
using Mediator;

namespace Enrollify.Application.Users.Get;

public record GetUserByEmailQuery (UserEmail email) : IQuery<Result<UserDTO>>;
