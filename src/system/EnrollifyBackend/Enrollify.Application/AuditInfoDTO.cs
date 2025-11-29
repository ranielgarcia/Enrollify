using Enrollify.Core;
using Enrollify.Core.Aggregates.UserAggregate;
using System.Runtime.Serialization;

namespace Enrollify.Application;

public class AuditInfoDTO
{
    public DateTimeOffset CreatedAt { get; set; }
    public UserId CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public UserId? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public UserId? DeletedBy { get; set; }
    public bool IsActive { get; set; }

    public static AuditInfoDTO FromAuditInfo(AuditInfo auditInfo)
    {
        return new AuditInfoDTO
        {
            CreatedAt = auditInfo.CreatedAt,
            CreatedBy = auditInfo.CreatedBy,
            UpdatedAt = auditInfo.UpdatedAt,
            UpdatedBy = auditInfo.UpdatedBy,
            DeletedAt = auditInfo.DeletedAt,
            DeletedBy = auditInfo.DeletedBy,
            IsActive = auditInfo.IsActive
        };
    }
}
