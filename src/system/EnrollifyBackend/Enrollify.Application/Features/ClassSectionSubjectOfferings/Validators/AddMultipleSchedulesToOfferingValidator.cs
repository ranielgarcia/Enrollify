using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;
using FluentValidation;
using static Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands.AddMultipleSchedulesToOffering;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Validators;

public class AddMultipleSchedulesToOfferingValidator : AbstractValidator<Command>
{
    public AddMultipleSchedulesToOfferingValidator(
        IClassSectionSubjectOfferingRepository offeringRepo,
        IReadRepository<ClassSectionSubjectOffering> readRepo,
        IReadRepository<ClassSection> sectionReadRepo)
    {
        // HC-01, HC-02, HC-03 — checked per schedule in a single async pass
        RuleFor(x => x)
            .CustomAsync(async (cmd, context, ct) =>
            {
                var offering = await readRepo.FirstOrDefaultAsync(
                    new GetClassSectionSubjectOfferingByIdSpec(cmd.OfferingId), ct);

                if (offering is null) return; // Not found — handler will surface 404

                var section = await sectionReadRepo.FirstOrDefaultAsync(
                    new GetClassSectionByIdSpec(offering.ClassSectionId), ct);

                if (section is null) return; // Not found — handler will surface 404

                foreach (ScheduleToAdd scheduleToAdd in cmd.Schedules)
                {
                    if (!DayOfWeekEnum.TryFromValue(scheduleToAdd.DayOfWeek, out DayOfWeekEnum? dayOfWeek))
                        continue; // Invalid day value — caught by the command's own validation

                    // HC-01 — Teacher double-booked
                    if (offering.TeacherId.HasValue)
                    {
                        bool teacherConflict = await offeringRepo.HasTeacherScheduleConflictAsync(
                            offering.TeacherId.Value,
                            section.AcademicTermId,
                            dayOfWeek!,
                            scheduleToAdd.StartTime,
                            scheduleToAdd.EndTime,
                            excludeOfferingId: null,
                            ct);

                        if (teacherConflict)
                            context.AddFailure(
                                new FluentValidation.Results.ValidationFailure(
                                    nameof(cmd.Schedules),
                                    $"The assigned teacher already has a class scheduled on {scheduleToAdd.DayOfWeek} at this time.")
                                {
                                    ErrorCode = "TEACHER_DOUBLE_BOOKED"
                                });
                    }

                    // HC-02 — Room double-booked
                    if (offering.RoomId.HasValue)
                    {
                        bool roomConflict = await offeringRepo.HasRoomScheduleConflictAsync(
                            offering.RoomId.Value,
                            section.AcademicTermId,
                            dayOfWeek!,
                            scheduleToAdd.StartTime,
                            scheduleToAdd.EndTime,
                            excludeOfferingId: null,
                            ct);

                        if (roomConflict)
                            context.AddFailure(
                                new FluentValidation.Results.ValidationFailure(
                                    nameof(cmd.Schedules),
                                    $"The assigned room is already booked on {scheduleToAdd.DayOfWeek} at this time.")
                                {
                                    ErrorCode = "ROOM_DOUBLE_BOOKED"
                                });
                    }

                    // HC-03 — Section overlap
                    bool sectionConflict = await offeringRepo.HasSectionScheduleOverlapAsync(
                        offering.ClassSectionId,
                        dayOfWeek!,
                        scheduleToAdd.StartTime,
                        scheduleToAdd.EndTime,
                        excludeScheduleId: null,
                        ct);

                    if (sectionConflict)
                        context.AddFailure(
                            new FluentValidation.Results.ValidationFailure(
                                nameof(cmd.Schedules),
                                $"Another subject in this section is already scheduled on {scheduleToAdd.DayOfWeek} at this time.")
                            {
                                ErrorCode = "SECTION_OVERLAP"
                            });
                }
            });
    }
}

