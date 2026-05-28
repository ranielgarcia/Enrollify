using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Core.Constants;

/// <summary>
/// Constants for the built-in system user (Id=1, system@enrollify.local).
/// Used as a fallback audit identity for system-triggered operations that run
/// without an HTTP request context (e.g. startup background jobs).
/// </summary>
public static class SystemUserConstants
{
  /// <summary>
  /// The ID of the seeded system user (system@enrollify.local).
  /// This user is always present in the database and is used as the audit author
  /// for automated/background operations where no human user is authenticated.
  /// </summary>
  public static readonly UserId SystemUserId = UserId.From(1);
}
