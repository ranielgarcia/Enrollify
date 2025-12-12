using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core;

// just a marker interface
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    UserId CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get;  }
    UserId? UpdatedBy { get;  }
    DateTimeOffset? DeletedAt { get; }
    UserId? DeletedBy { get; }
    bool IsActive { get; }
}
