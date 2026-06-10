using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;
using FluentValidation;
using static Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands.UpdateClassSectionSubjectOffering;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Validators;

public class UpdateClassSectionSubjectOfferingValidator : AbstractValidator<Command>
{
    public UpdateClassSectionSubjectOfferingValidator(
        IClassSectionSubjectOfferingRepository offeringRepo,
        IReadRepository<ClassSectionSubjectOffering> readRepo,
        IReadRepository<ClassSection> sectionReadRepo)
    {
        // HC-01 and HC-02 — validate new teacher/room against the offering's existing schedules.
        // Only runs when teacher or room is actually changing.
        RuleFor(x => x)
            .CustomAsync(async (cmd, context, ct) =>
            {
                var offering = await readRepo.FirstOrDefaultAsync(
                    new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(cmd.Id), ct);

                if (offering is null) return; // Not found — handler will surface 404

                var schedules = offering.ClassSchedules.ToList();
                if (schedules.Count == 0) return; // No schedules yet — nothing to conflict with

                var section = await sectionReadRepo.FirstOrDefaultAsync(
                    new GetClassSectionByIdSpec(offering.ClassSectionId), ct);

                if (section is null) return; // Not found — handler will surface 404

                // HC-01 — Teacher double-booked (only when a new teacher is being assigned)
                if (cmd.TeacherId.HasValue && cmd.TeacherId != offering.TeacherId)
                {
                    foreach (var schedule in schedules)
                    {
                        bool teacherConflict = await offeringRepo.HasTeacherScheduleConflictAsync(
                            cmd.TeacherId.Value,
                            section.AcademicTermId,
                            schedule.DayOfWeek,
                            schedule.StartTime,
                            schedule.EndTime,
                            excludeOfferingId: cmd.Id,
                            ct);

                        if (teacherConflict)
                        {
                            context.AddFailure(
                                new FluentValidation.Results.ValidationFailure(
                                    nameof(cmd.TeacherId),
                                    "The assigned teacher already has a class scheduled at one or more of this offering's scheduled times.")
                                {
                                    ErrorCode = "TEACHER_DOUBLE_BOOKED"
                                });
                            break; // One failure per resource is sufficient
                        }
                    }
                }

                // HC-02 — Room double-booked (only when a new room is being assigned)
                if (cmd.RoomId.HasValue && cmd.RoomId != offering.RoomId)
                {
                    foreach (var schedule in schedules)
                    {
                        bool roomConflict = await offeringRepo.HasRoomScheduleConflictAsync(
                            cmd.RoomId.Value,
                            section.AcademicTermId,
                            schedule.DayOfWeek,
                            schedule.StartTime,
                            schedule.EndTime,
                            excludeOfferingId: cmd.Id,
                            ct);

                        if (roomConflict)
                        {
                            context.AddFailure(
                                new FluentValidation.Results.ValidationFailure(
                                    nameof(cmd.RoomId),
                                    "The assigned room is already booked at one or more of this offering's scheduled times.")
                                {
                                    ErrorCode = "ROOM_DOUBLE_BOOKED"
                                });
                            break;
                        }
                    }
                }
            });
    }
}

