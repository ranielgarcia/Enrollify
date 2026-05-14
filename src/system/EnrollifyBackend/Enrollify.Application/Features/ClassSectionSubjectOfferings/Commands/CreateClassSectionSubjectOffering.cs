using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Mediator;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class CreateClassSectionSubjectOffering
{
    // Note: We only need ClassSectionId and SubjectId to create a ClassSectionSubjectOffering.
    // Subsequent updates will be made to set the Teacher, Room, UnitsOverride, etc.
    // But these initial values will not be allowed to update after creation, as they are required to create the ClassSectionSubjectOffering in the first place.
    public sealed record Command(
        ClassSectionId classSectionId,
        SubjectId subjectId
        ) : ICommand<Result<ClassSectionSubjectOfferingId>>;

    public sealed class Handler : ICommandHandler<Command, Result<ClassSectionSubjectOfferingId>>
    {
        private readonly IClassSectionSubjectOfferingRepository _classSectionSubjectOfferingRepository;

        public Handler(IClassSectionSubjectOfferingRepository classSectionSubjectOfferingRepository)
        {
            _classSectionSubjectOfferingRepository = classSectionSubjectOfferingRepository;
        }

        public async ValueTask<Result<ClassSectionSubjectOfferingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var newClassSectionSubjectOffering = new ClassSectionSubjectOffering(
                command.classSectionId,
                command.subjectId
            );

            var result = await _classSectionSubjectOfferingRepository.Create(newClassSectionSubjectOffering, cancellationToken);
            return result;
        }
    }

}
