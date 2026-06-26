namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;

public static class CompleteClassSection
{
    public sealed record Command(ClassSectionId Id) : IRequest<Result<ClassSectionId>>;

    public sealed class Handler : IRequestHandler<Command, Result<ClassSectionId>>
    {
        private readonly IReadRepository<ClassSection> _classSectionReadRepository;
        private readonly IClassSectionRepository _classSectionRepository;

        public Handler(
            IReadRepository<ClassSection> classSectionReadRepository,
            IClassSectionRepository classSectionRepository)
        {
            _classSectionReadRepository = classSectionReadRepository;
            _classSectionRepository = classSectionRepository;
        }

        public async Task<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var section = await _classSectionReadRepository.GetByIdAsync(command.Id, cancellationToken);
            if (section is null)
                return Result.NotFound($"Class section with ID {command.Id.Value} not found.");

            try
            {
                section.Complete();
            }
            catch (ArgumentException ex)
            {
                return Result.Invalid(new ValidationError(ex.Message));
            }

            var result = await _classSectionRepository.Update(section, cancellationToken);
            return result.IsSuccess
                ? Result.Success(result.Value)
                : Result.Error(string.Join("; ", result.Errors));
        }
    }
}
