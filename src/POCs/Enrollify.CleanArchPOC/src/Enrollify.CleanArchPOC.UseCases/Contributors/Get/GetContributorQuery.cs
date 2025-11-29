using Enrollify.CleanArchPOC.Core.ContributorAggregate;

namespace Enrollify.CleanArchPOC.UseCases.Contributors.Get;

public record GetContributorQuery(ContributorId ContributorId) : IQuery<Result<ContributorDto>>;
