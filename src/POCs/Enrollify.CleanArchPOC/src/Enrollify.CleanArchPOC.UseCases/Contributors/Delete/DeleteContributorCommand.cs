using Enrollify.CleanArchPOC.Core.ContributorAggregate;

namespace Enrollify.CleanArchPOC.UseCases.Contributors.Delete;

public record DeleteContributorCommand(ContributorId ContributorId) : ICommand<Result>;
