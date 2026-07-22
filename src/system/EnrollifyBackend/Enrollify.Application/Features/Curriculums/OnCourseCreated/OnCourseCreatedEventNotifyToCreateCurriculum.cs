using Enrollify.Application.Features.Courses.Events;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services.NotificationServices.Models;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

namespace Enrollify.Application.Features.Curriculums.OnCourseCreated;

public class OnCourseCreatedEventNotifyToCreateCurriculum
(IMediator mediator, INotificationPublisher notificationPublisher, IReadRepository<Course> courseRepository)
  : INotificationHandler<CourseCreatedEvent>
{
  public async Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
  {
    var course = await courseRepository.GetByIdAsync(notification.CourseId, cancellationToken);
    // TODO: Use the correct Target Role
    await notificationPublisher.InfoTargetRoleNotification(new NotificationForTargetRoleCreation(
      "CourseCreatedEvent",
      $"Complete Course Setup: {course!.Name}",
      $"The course has been created. Create its curriculum to finish the setup.",
      NotificationCategoryEnum.Academic,
      [RolesEnum.SystemAdmin]
    ));
  }
}
