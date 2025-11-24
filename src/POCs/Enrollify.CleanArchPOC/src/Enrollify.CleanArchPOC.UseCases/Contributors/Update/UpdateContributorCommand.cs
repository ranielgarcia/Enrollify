using Enrollify.CleanArchPOC.Core.ContributorAggregate;

namespace Enrollify.CleanArchPOC.UseCases.Contributors.Update;

public record UpdateContributorCommand(ContributorId ContributorId, ContributorName NewName) : ICommand<Result<ContributorDto>>;
