using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using FluentValidation;

namespace Enrollify.IntegrationTests._Tests.Application.ClassSections;

/// <summary>
/// Integration tests verifying that BulkInitializeClassSectionsForAcademicYearValidator
/// is invoked by Mediator's ValidationBehavior pipeline.
/// </summary>
[Collection("Application")]
public class BulkInitializeClassSectionsValidationTests
{
    private readonly ApplicationTestFixture _fixture;

    public BulkInitializeClassSectionsValidationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Empty TargetCourses collection - throws ValidationException")]
    public async Task BulkInitializeClassSections_EmptyTargetCourses_ThrowsValidationException()
    {
        // Arrange
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            TargetCourses: new List<BulkInitializeClassSectionsForAcademicYear.TargetCourse>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _fixture.SendAsync(command);
        });

        Assert.Contains(exception.Errors, e =>
            e.PropertyName == nameof(BulkInitializeClassSectionsForAcademicYear.Command.TargetCourses) &&
            e.ErrorMessage.Contains("At least one course is required"));
    }

    [Fact(DisplayName = "Duplicate course IDs - throws ValidationException")]
    public async Task BulkInitializeClassSections_DuplicateCourseIds_ThrowsValidationException()
    {
        // Arrange
        var courseId = CourseId.From(999);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [
                new(courseId, NumberOfSections: 1),
                new(courseId, NumberOfSections: 2)
            ]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _fixture.SendAsync(command);
        });

        Assert.Contains(exception.Errors, e =>
            e.ErrorMessage.Contains("Duplicate course IDs are not allowed"));
    }

    [Fact(DisplayName = "Zero course ID - throws ValidationException")]
    public async Task BulkInitializeClassSections_ZeroCourseId_ThrowsValidationException()
    {
        // Arrange
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [new(CourseId.From(0), NumberOfSections: 1)]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _fixture.SendAsync(command);
        });

        Assert.Contains(exception.Errors, e =>
            e.ErrorMessage.Contains("Course ID must be a valid non-zero value"));
    }

    [Fact(DisplayName = "Number of sections exceeds 26 - throws ValidationException")]
    public async Task BulkInitializeClassSections_TooManySections_ThrowsValidationException()
    {
        // Arrange
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [new(CourseId.From(1), NumberOfSections: 27)]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _fixture.SendAsync(command);
        });

        Assert.Contains(exception.Errors, e =>
            e.ErrorMessage.Contains("Number of sections cannot exceed 26"));
    }

    [Fact(DisplayName = "Non-existent academic term - throws ValidationException")]
    public async Task BulkInitializeClassSections_NonExistentAcademicTerm_ThrowsValidationException()
    {
        // Arrange
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(99999), // Non-existent
            YearLevel.From(1),
            [new(CourseId.From(1), NumberOfSections: 1)]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _fixture.SendAsync(command);
        });

        Assert.Contains(exception.Errors, e =>
            e.ErrorMessage.Contains("The specified academic term does not exist or is inactive"));
    }

    [Fact(DisplayName = "Non-existent course IDs - throws ValidationException with specific IDs")]
    public async Task BulkInitializeClassSections_NonExistentCourseIds_ThrowsValidationExceptionWithIds()
    {
        // Arrange
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [
                new(CourseId.From(88888), NumberOfSections: 1),
                new(CourseId.From(99999), NumberOfSections: 1)
            ]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _fixture.SendAsync(command);
        });

        Assert.Contains(exception.Errors, e =>
            e.PropertyName == nameof(BulkInitializeClassSectionsForAcademicYear.Command.TargetCourses) &&
            e.ErrorMessage.Contains("The following course IDs do not exist"));
    }
}
