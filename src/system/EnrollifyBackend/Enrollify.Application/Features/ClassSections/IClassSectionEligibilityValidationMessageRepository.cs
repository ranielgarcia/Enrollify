using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections;

/// <summary>
/// Manages the <see cref="ClassSectionEnrollmentEligibilityValidationMessage"/> projection.
/// The replace strategy atomically clears all existing messages for a section and inserts fresh ones.
/// </summary>
public interface IClassSectionEligibilityValidationMessageRepository
{
  /// <summary>
  /// Deletes all existing validation messages for the given section and inserts
  /// the provided <paramref name="messages"/> in their place.
  /// </summary>
  Task ReplaceAllForSectionAsync(
    ClassSectionId classSectionId,
    IEnumerable<ClassSectionEnrollmentEligibilityValidationMessage> messages,
    CancellationToken cancellationToken = default);
}

