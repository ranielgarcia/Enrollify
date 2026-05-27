using Ardalis.Specification;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.ClassSections.Validators;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.ClassSections.Validators;

public class BulkInitializeClassSectionsForAcademicYearValidatorTests
{
    private readonly Mock<IReadRepository<Course>> _courseRepositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _academicYearRepositoryMock = new();
    private readonly Mock<IReadRepository<CourseCurriculumAssignment>> _curriculumAssignmentRepositoryMock = new();
    private readonly BulkInitializeClassSectionsForAcademicYearValidator _validator;

    public BulkInitializeClassSectionsForAcademicYearValidatorTests()
    {
        _validator = new BulkInitializeClassSectionsForAcademicYearValidator(
            _courseRepositoryMock.Object,
            _academicYearRepositoryMock.Object,
            _curriculumAssignmentRepositoryMock.Object);
    }

    #region RequestPayload Collection Validation

    [Fact(DisplayName = "Empty requestPayload - returns collection-level error")]
    public async Task ValidateAsync_EmptyRequestPayload_HasCollectionError()
    {
        var command = CreateCommand(payloadCount: 0);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "TargetCourses" &&
            e.ErrorMessage.Contains("At least one course is required."));
    }

    [Fact(DisplayName = "Non-empty requestPayload - no collection-level error")]
    public async Task ValidateAsync_NonEmptyRequestPayload_NoCollectionError()
    {
        var command = CreateCommand();
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("At least one payload entry is required"));
    }

    #endregion

    #region Duplicate CourseId Validation

    [Fact(DisplayName = "Duplicate courseIds in payload - returns duplicate error")]
    public async Task ValidateAsync_DuplicateCourseIds_HasDuplicateError()
    {
        var courseId = CourseId.From(1);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [new(courseId, 1), new(courseId, 2)]);
        SetupCourses(courseId);
        SetupAcademicYears(AcademicTermId.From(1));
        SetupCurriculumAssignments(courseId);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("Duplicate course IDs are not allowed"));
    }

    [Fact(DisplayName = "Unique courseIds in payload - no duplicate error")]
    public async Task ValidateAsync_UniqueCourseIds_NoDuplicateError()
    {
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [new(courseId1, 1), new(courseId2, 1)]);
        SetupCourses(courseId1, courseId2);
        SetupAcademicYears(AcademicTermId.From(1));
        SetupCurriculumAssignments(courseId1, courseId2);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("Duplicate course IDs"));
    }

    #endregion

    #region AcademicTermId Validation

    [Fact(DisplayName = "AcademicTermId does not exist - error targets academicTermId property with specific message")]
    public async Task ValidateAsync_AcademicTermDoesNotExist_HasTargetedAcademicTermError()
    {
        var command = CreateCommand(academicTermId: AcademicTermId.From(99));
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(); // returns null — no matching term
        SetupCurriculumAssignments();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(BulkInitializeClassSectionsForAcademicYear.Command.AcademicTermId) &&
            e.ErrorMessage.Contains("does not exist or is inactive"));
    }

    [Fact(DisplayName = "AcademicTermId exists - no academic term error")]
    public async Task ValidateAsync_AcademicTermExists_NoAcademicTermError()
    {
        var command = CreateCommand();
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("does not exist or is inactive"));
    }

    #endregion

    #region YearLevel Validation

    // YearLevel.From(0) and From(7) throw at construction time (Vogen validation), so no boundary tests here

    [Fact(DisplayName = "YearLevel is valid - no year level error")]
    public async Task ValidateAsync_YearLevelIsValid_NoYearLevelError()
    {
        var command = CreateCommand(yearLevel: YearLevel.From(3));
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("Year level"));
    }

    #endregion

    #region CourseId Zero Guard

    [Fact(DisplayName = "CourseId is zero - returns explicit non-zero error instead of silently ignoring")]
    public async Task ValidateAsync_CourseIdIsZero_HasNonZeroCourseIdError()
    {
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [new(CourseId.From(0), 1)]);
        SetupCourses();
        SetupAcademicYears(AcademicTermId.From(1));
        SetupCurriculumAssignments();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("Course ID must be a valid non-zero value"));
    }

    #endregion

    #region CourseId Existence Validation

    [Fact(DisplayName = "CourseId does not exist - error message names the missing course ID")]
    public async Task ValidateAsync_CourseDoesNotExist_ErrorMessageContainsMissingId()
    {
        var command = CreateCommand(courseId: CourseId.From(99));
        SetupCourses(); // returns empty — course 99 is missing
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("course IDs do not exist") &&
            e.ErrorMessage.Contains("99"));
    }

    [Fact(DisplayName = "Multiple payloads - missing course in second entry - error lists the missing ID")]
    public async Task ValidateAsync_MultiplePayloadsMissingCourseInSecondEntry_ErrorListsMissingId()
    {
        var courseId1 = CourseId.From(1);
        var missingCourseId = CourseId.From(99);
        var academicTermId = AcademicTermId.From(10);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            YearLevel.From(1),
            [new(courseId1, 1), new(missingCourseId, 1)]);
        SetupCourses(courseId1);
        SetupAcademicYears(academicTermId);
        SetupCurriculumAssignments(courseId1);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("course IDs do not exist") &&
            e.ErrorMessage.Contains("99"));
    }

    [Fact(DisplayName = "CourseId exists - no course existence error")]
    public async Task ValidateAsync_CourseExists_NoCourseExistenceError()
    {
        var command = CreateCommand();
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("course IDs do not exist"));
    }

    #endregion

    #region NumberOfSections Validation

    [Fact(DisplayName = "NumberOfSections is zero - returns lower bound error")]
    public async Task ValidateAsync_NumberOfSectionsIsZero_HasLowerBoundError()
    {
        var command = CreateCommand(numberOfSections: 0);
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    [Fact(DisplayName = "NumberOfSections is negative - returns lower bound error")]
    public async Task ValidateAsync_NumberOfSectionsIsNegative_HasLowerBoundError()
    {
        var command = CreateCommand(numberOfSections: -3);
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    [Fact(DisplayName = "NumberOfSections exceeds 26 - returns upper bound error")]
    public async Task ValidateAsync_NumberOfSectionsExceeds26_HasUpperBoundError()
    {
        var command = CreateCommand(numberOfSections: 27);
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("cannot exceed 26"));
    }

    [Fact(DisplayName = "NumberOfSections is 26 (maximum allowed) - no upper bound error")]
    public async Task ValidateAsync_NumberOfSectionsIs26_NoUpperBoundError()
    {
        var command = CreateCommand(numberOfSections: 26);
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("cannot exceed 26"));
    }

    [Fact(DisplayName = "NumberOfSections is positive and within range - no sections error")]
    public async Task ValidateAsync_NumberOfSectionsIsPositiveAndWithinRange_NoSectionsError()
    {
        var command = CreateCommand(numberOfSections: 3);
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("Number of sections must be greater than zero") ||
            e.ErrorMessage.Contains("cannot exceed 26"));
    }

    #endregion

    #region CourseCurriculumAssignment Validation

    [Fact(DisplayName = "Course has no curriculum assignment - error names the unassigned course ID")]
    public async Task ValidateAsync_CourseHasNoCurriculumAssignment_ErrorNamesUnassignedCourseId()
    {
        var courseId = CourseId.From(1);
        var command = CreateCommand(courseId: courseId);
        SetupCourses(courseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(); // no assignments returned

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("do not have a curriculum assignment") &&
            e.ErrorMessage.Contains("1"));
    }

    [Fact(DisplayName = "All courses have curriculum assignments - no curriculum assignment error")]
    public async Task ValidateAsync_AllCoursesHaveCurriculumAssignments_NoCurriculumAssignmentError()
    {
        var courseId = CourseId.From(1);
        var command = CreateCommand(courseId: courseId);
        SetupCourses(courseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(courseId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e =>
            e.ErrorMessage.Contains("do not have a curriculum assignment"));
    }

    [Fact(DisplayName = "Multiple payloads - one course has no curriculum assignment - error lists that course ID")]
    public async Task ValidateAsync_MultiplePayloadsOneMissingCurriculumAssignment_ErrorListsCourseId()
    {
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(10);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            YearLevel.From(1),
            [new(courseId1, 1), new(courseId2, 1)]);
        SetupCourses(courseId1, courseId2);
        SetupAcademicYears(academicTermId);
        SetupCurriculumAssignments(courseId1); // courseId2 has no assignment

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("do not have a curriculum assignment") &&
            e.ErrorMessage.Contains("2"));
    }

    #endregion

    #region Full Valid Command

    [Fact(DisplayName = "All fields valid - validation passes with no errors")]
    public async Task ValidateAsync_AllFieldsValid_IsValid()
    {
        var command = CreateCommand(numberOfSections: 2);
        SetupCourses(command.TargetCourses[0].CourseId);
        SetupAcademicYears(command.AcademicTermId);
        SetupCurriculumAssignments(command.TargetCourses[0].CourseId);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Bulk Validation (Multiple Payloads)

    [Fact(DisplayName = "Multiple payloads all valid - validation passes with exactly one bulk course DB call")]
    public async Task ValidateAsync_MultiplePayloadsAllValid_IsValidAndUsesOneBulkCourseCall()
    {
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(10);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            YearLevel.From(1),
            [new(courseId1, 2), new(courseId2, 3)]);
        SetupCourses(courseId1, courseId2);
        SetupAcademicYears(academicTermId);
        SetupCurriculumAssignments(courseId1, courseId2);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        _courseRepositoryMock.Verify(
            r => r.ListAsync(It.IsAny<ISpecification<Course>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static BulkInitializeClassSectionsForAcademicYear.Command CreateCommand(
        AcademicTermId? academicTermId = null,
        YearLevel? yearLevel = null,
        CourseId? courseId = null,
        int numberOfSections = 1,
        int payloadCount = 1)
    {
        var payloads = payloadCount == 0
            ? new List<BulkInitializeClassSectionsForAcademicYear.TargetCourse>()
            : Enumerable.Range(0, payloadCount)
                .Select(_ => new BulkInitializeClassSectionsForAcademicYear.TargetCourse(
                    courseId ?? CourseId.From(1),
                    numberOfSections))
                .ToList();

        return new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId ?? AcademicTermId.From(1),
            yearLevel ?? YearLevel.From(1),
            payloads);
    }

    private void SetupCourses(params CourseId[] existingIds)
    {
        var courses = existingIds.Select((id, i) =>
        {
            var course = new Course(
                CourseCode.From($"CODE{i + 1}"),
                $"Course {i + 1}",
                4,
                $"Description {i + 1}",
                CollegeId.From(1));
            SetEntityProperty(course, "Id", id);
            return course;
        }).ToList();

        _courseRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Course>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(courses);
    }

    private void SetupAcademicYears(params AcademicTermId[] existingTermIds)
    {
        if (existingTermIds.Length == 0)
        {
            _academicYearRepositoryMock
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AcademicYear?)null);
            return;
        }

        var startDate = AcademicYearStartDate.From(new DateTime(2024, 8, 1));
        var endDate = AcademicYearEndDate.From(new DateTime(2025, 7, 31));
        var academicYear = new AcademicYear(startDate, endDate);
        SetEntityProperty(academicYear, "Id", AcademicYearId.From(1));

        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);
    }

    private void SetupCurriculumAssignments(params CourseId[] assignedCourseIds)
    {
        var assignments = assignedCourseIds
            .Select(id => new CourseCurriculumAssignment(id, AcademicYearId.From(1), CurriculumId.From(1)))
            .ToList();

        _curriculumAssignmentRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);
    }

    private static void SetEntityProperty<T>(T entity, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, value);
        }
        else
        {
            var field = typeof(T).GetField(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(entity, value);
        }
    }

    #endregion
}
