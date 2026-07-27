using Enrollify.Application.Features.ClassSectionScheduling.Extensions;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;

public static class DeleteClassSection
{
    public sealed record Command(ClassSectionId Id, bool reOrderClassSectionCodes = false) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IClassSectionRepository _classSectionRepository;
        private readonly IReadRepository<ClassSection> _readRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDomainEventBus _eventBus;
        private readonly ILogger<Handler> _logger;

        public Handler(IClassSectionRepository classSectionRepository,
            IReadRepository<ClassSection> readRepository,
            IUnitOfWork unitOfWork,
            IDomainEventBus eventBus,
            ILogger<Handler> logger)
        {
            _classSectionRepository = classSectionRepository;
            _readRepository = readRepository;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
          await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var classSectionToDelete = await _readRepository.GetByIdAsync(command.Id, cancellationToken);

            if (classSectionToDelete is null)
            {
                _logger.LogWarning("Unable to find class section with an id of {ClassSectionId}", command.Id.Value);
                return Result.NotFound($"Unable to find class section with an id of {command.Id}");
            }

            if (command.reOrderClassSectionCodes)
            {
                _logger.LogInformation("Re-ordering class section codes...");
                var existingClassSections = await _readRepository.ListAsync(
                    new GetExistingClassSectionsByCourseYearLevelAndTerm(classSectionToDelete.IntendedYearLevel, classSectionToDelete.CourseId, classSectionToDelete.AcademicTermId), cancellationToken);

                var remainingClassSections = existingClassSections.Where(x => x.Id != classSectionToDelete.Id).OrderBy(x => x.SectionCode).ToList();

                if (remainingClassSections.Count > 0)
                {
                    SectionCode? nextSectionCode = null; // basically, starts with letter A
                    foreach(var classSection in remainingClassSections)
                    {
                        var newSectionCode = nextSectionCode.GetNextSectionCode();
                        _logger.LogInformation("From {FromSectionCode} to {ToSectionCode}", classSection.SectionCode, newSectionCode);
                        classSection.UpdateSectionCode(newSectionCode);
                        nextSectionCode = newSectionCode;
                    }
                }
            }

            var deleteResult = await _classSectionRepository.Delete(classSectionToDelete, cancellationToken);
            if (!deleteResult.IsSuccess)
            {
              await transaction.RollbackAsync(cancellationToken);
              return deleteResult;
            }

            // TODO: when all class sections are deleted, we should also unlock the course-curriculum assignment for that course and academic year
            await _eventBus.PublishAsync(new ClassSectionDeletedEvent(classSectionToDelete.Id));

            await _unitOfWork.SaveChangesAndFlushMessagesThenCommitAsync(cancellationToken);
            return deleteResult;
        }
    }
}
