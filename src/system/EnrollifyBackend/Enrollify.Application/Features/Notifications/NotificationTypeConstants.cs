namespace Enrollify.Application.Features.Notifications;

/// <summary>
/// Canonical notification type string constants.
/// These values are sent as the <c>Type</c> field of every <see cref="DTOs.NotificationDto"/>
/// and are used by the frontend to route type-specific query invalidations.
///
/// Convention: use PascalCase matching the command/event class name that produced the notification.
/// </summary>
public static class NotificationTypeConstants
{
    // ── Class Sections ────────────────────────────────────────────────────────
    public const string BulkInitializeClassSectionsForAcademicYear = "BulkInitializeClassSectionsForAcademicYear";
    public const string CreateClassSection = "CreateClassSection";
    public const string BulkAssignClassSectionAdviser = "BulkAssignClassSectionAdviser";
    public const string ComputeAndGetValidationIssuesForClassSection = "ComputeAndGetValidationIssuesForClassSection";

    // ── Courses / Curricula ───────────────────────────────────────────────────
    public const string CourseCreatedEvent = "CourseCreatedEvent";
    public const string SyncCourseCurriculumAssignmentsForAcademicYear = "SyncCourseCurriculumAssignmentsForAcademicYear";
}
