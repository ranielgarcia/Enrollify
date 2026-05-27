using Ardalis.Result;
using Enrollify.Application.Features.Curriculums.Events;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Curriculums.Commands;

public static class ApproveCurriculum
{
    public sealed record Command(CurriculumId Id) : IRequest<Result<CurriculumId>>;

    public sealed class Handler : IRequestHandler<Command, Result<CurriculumId>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IReadRepository<Curriculum> _curriculumReadRepository;
        private readonly IMediator _mediator;

        public Handler(
            ICurriculumRepository curriculumRepository,
            IReadRepository<Curriculum> curriculumReadRepository,
            IMediator mediator)
        {
            _curriculumRepository = curriculumRepository;
            _curriculumReadRepository = curriculumReadRepository;
            _mediator = mediator;
        }

        public async Task<Result<CurriculumId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var curriculum = await _curriculumReadRepository.GetByIdAsync(command.Id, cancellationToken);
            if (curriculum == null)
                return Result.NotFound($"Curriculum with an id of {command.Id.Value} not found");

            if (curriculum.StatusId == CurriculumStatusEnum.Active)
            {
                return Result.Forbidden("Active curriculums cannot be modified. Please deactivate the curriculum before making changes.");
            }

            curriculum.Approve(DateTimeOffset.UtcNow);

            var result = await _curriculumRepository.UpdateCurriculum(curriculum, cancellationToken);

            if (result.IsSuccess)
            {
                await _mediator.Publish(new CurriculumApprovedEvent(curriculum), cancellationToken);
            }

            return result;
        }
    }
}
