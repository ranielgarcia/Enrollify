using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.NotificationAggregate;

public class Notification : EntityBase<NotificationId>, IAggregateRoot, IAuditable
{
  private Notification()
  {}

    public string Type { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public NotificationSeverityEnum Severity { get; private set; }
    public NotificationCategoryEnum Category { get; private set; }
    public NotificationReferenceTypeEnum? ReferenceType { get; private set; }
    public int? ReferenceId { get; private set; }
    public NotificationTargetScopeEnum TargetScope{ get; private set; }
    public UserId? TargetUserId { get; private set; }
    public RoleId? TargetRoleId { get; private set; }
    public int RetentionDays { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }


    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public User? CreatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public UserId? UpdatedBy { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }
    public User? DeletedByUser { get; private set; }
    public bool IsActive { get; private set; }


    private readonly List<NotificationRecipient> _recipients = [];
    public IReadOnlyCollection<NotificationRecipient> Recipients => _recipients.AsReadOnly();

    public static Notification Create(
        string type, string title, string message,
        NotificationSeverityEnum severity, NotificationCategoryEnum category,
        NotificationTargetScopeEnum scope, int retentionDays,
        UserId? targetUserId = null, RoleId? targetRoleId = null,
        NotificationReferenceTypeEnum? referenceType = null, int? referenceId = null)
    {
        var notif = new Notification
        {
            Type          = type,
            Title         = title,
            Message       = message,
            Severity      = severity,
            Category      = category,
            TargetScope   = scope,
            TargetUserId  = targetUserId,
            TargetRoleId  = targetRoleId,
            RetentionDays = retentionDays,
            ReferenceType = referenceType,
            ReferenceId   = referenceId,
            ExpiresAt     = DateTime.UtcNow.AddDays(retentionDays)
        };
        return notif;
    }

    public void AddRecipient(UserId userId)
    {
        if (_recipients.Any(r => (int)r.UserId == (int)userId && r.IsActive))
            return;

        _recipients.Add(new NotificationRecipient(Id, userId));
    }
}

