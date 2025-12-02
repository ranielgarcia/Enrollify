using Enrollify.Application.Authentication.GetContext;
using Enrollify.Core.Services.Authentication;
using Enrollify.WebAPI.Extensions;
using Enrollify.WebAPI.Features.Users;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features;

public sealed class PermissionDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string Resource { get; set; } = null!;
}

public sealed class RoleDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
}

public sealed class UserContextDTO
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
}

public sealed class UserContextMapper : ResponseMapper<UserContextDTO, UserContext>
{
    public override UserContextDTO FromEntity(UserContext e)
    {
        var roles = e.Roles.Select(r => new RoleDTO
        {
            Id = r.Id.Value,
            Name = r.Name.Value,
            Description = r.Description.Value,
            Permissions = r.Permissions.Select(p => new PermissionDTO
            {
                Id = p.Id.Value,
                Name = p.Name.Value,
                Description = p.Description.Value,
                Action = p.Action.Value,
                Resource = p.Resource.Value
            }).ToList()
        });

        return new UserContextDTO
        {
            Id = e.Id.Value,
            Email = e.Email.Value,
            FullName = e.FullName,
        };
    }
}

public class MeEndpoint : EndpointWithoutRequest<UserContextDTO, UserContextMapper>
{
    private readonly IMediator _mediator;

    public MeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("me");
        // Policies(PolicyName.HasValidRole);
    }

    public override async Task<Results<Ok<UserContextDTO>, NotFound, ProblemHttpResult>> HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserContextQuery(), ct);

        return result.ToGetByIdResult(Map.FromEntity);
    }

}
