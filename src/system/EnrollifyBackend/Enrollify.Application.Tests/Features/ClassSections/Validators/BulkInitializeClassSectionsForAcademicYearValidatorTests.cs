using Ardalis.Specification;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.ClassSections.Validators;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.ClassSections.Validators;

public class BulkInitializeClassSectionsForAcademicYearValidatorTests
{
    private readonly Mock<IReadRepository<Course>> _courseRepositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _academicYearRepositoryMock = new();
    private readonly Mock<IReadRepository<Curriculum>> _curriculumRepositoryMock = new();
    private readonly BulkInitializeClassSectionsForAcademicYearValidator _validator;

    public BulkInitializeClassSectionsForAcademicYearValidatorTests()
    {
        _validator = new BulkInitializeClassSectionsForAcademicYearValidator(
            _courseRepositoryMock.Object,
            _academicYearRepositoryMock.Object,
            _curriculumRepositoryMock.Object);
    }

    #region RequestPayload Collection Validation

    [Fact(DisplayName = "Empty requestPayload - returns invalid result")]
    public async Task ValidateAsync_EmptyRequestPayload_HasCollectionError()
    {
        var command = CreateCommand(payloadCount: 0);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "requestPayload" &&
            e.ErrorMessage.Contains("At least one payload entry is required"));
    }

    [Fact(DisplayName = "Non-empty requestPayload - no collection-level error")]
    public async Task ValidateAsync_NonEmptyRequestPayload_NoCollectionError()
    {
        var command = CreateCommand();
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "requestPayload");
    }

    #endregion

    #region AcademicTermId Validation

    [Fact(DisplayName = "AcademicTermId does not exist - returns error")]
    public async Task ValidateAsync_AcademicTermDoesNotExist_HasAcademicTermError()
    {
        var command = CreateCommand(academicTermId: AcademicTermId.From(99));
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(); // No matching academic term
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("Academic term") ||
            e.ErrorMessage.Contains("do not exist"));
    }

    [Fact(DisplayName = "AcademicTermId exists - no academic term error")]
    public async Task ValidateAsync_AcademicTermExists_NoAcademicTermError()
    {
        var command = CreateCommand();
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("Academic term"));
    }

    #endregion

    #region YearLevel Validation

    [Fact(DisplayName = "YearLevel is zero - returns invalid result")]
    public async Task ValidateAsync_YearLevelIsZero_HasYearLevelError()
    {
        var command = CreateCommand(yearLevel: YearLevel.From(0));
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Year level must be between 1 and 6"));
    }

    [Fact(DisplayName = "YearLevel is greater than 6 - returns invalid result")]
    public async Task ValidateAsync_YearLevelIsGreaterThanSix_HasYearLevelError()
    {
        var command = CreateCommand(yearLevel: YearLevel.From(7));
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Year level must be between 1 and 6"));
    }

    [Fact(DisplayName = "YearLevel is valid - no year level error")]
    public async Task ValidateAsync_YearLevelIsValid_NoYearLevelError()
    {
        var command = CreateCommand(yearLevel: YearLevel.From(3));
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("Year level"));
    }

    #endregion

    #region CourseId Validation

    [Fact(DisplayName = "CourseId does not exist - returns error with indexed property name")]
    public async Task ValidateAsync_CourseDoesNotExist_HasCourseErrorAtIndexZero()
    {
        var command = CreateCommand(courseId: CourseId.From(99));
        SetupCourses();
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "requestPayload[0].courseId" &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact(DisplayName = "CourseId exists - no course error")]
    public async Task ValidateAsync_CourseExists_NoCourseError()
    {
        var command = CreateCommand();
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "requestPayload[0].courseId");
    }

    #endregion

    #region CurriculumId Validation

    [Fact(DisplayName = "CurriculumId provided but curriculum does not exist - returns error with indexed property name")]
    public async Task ValidateAsync_CurriculumDoesNotExist_HasCurriculumErrorAtIndexZero()
    {
        var courseId = CourseId.From(1);
        var command = CreateCommand(courseId: courseId, curriculumId: CurriculumId.From(99));
        SetupCourses(courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "requestPayload[0].curriculumId" &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact(DisplayName = "CurriculumId provided but curriculum belongs to different course - returns error with indexed property name")]
    public async Task ValidateAsync_CurriculumBelongsToDifferentCourse_HasCurriculumErrorAtIndexZero()
    {
        var courseId = CourseId.From(1);
        var differentCourseId = CourseId.From(2);
        var curriculumId = CurriculumId.From(5);
        var command = CreateCommand(courseId: courseId, curriculumId: curriculumId);
        SetupCourses(courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula(CreateCurriculum(curriculumId, differentCourseId));

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "requestPayload[0].curriculumId" &&
            e.ErrorMessage.Contains("does not belong to course"));
    }

    [Fact(DisplayName = "CurriculumId provided and belongs to the correct course - no curriculum error")]
    public async Task ValidateAsync_CurriculumBelongsToCorrectCourse_NoCurriculumError()
    {
        var courseId = CourseId.From(1);
        var curriculumId = CurriculumId.From(5);
        var command = CreateCommand(courseId: courseId, curriculumId: curriculumId);
        SetupCourses(courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula(CreateCurriculum(curriculumId, courseId));

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "requestPayload[0].curriculumId");
    }

    [Fact(DisplayName = "CurriculumId is zero (not provided) - curriculum ListAsync is never called")]
    public async Task ValidateAsync_CurriculumIdIsZero_CurriculumListAsyncNeverCalled()
    {
        var command = CreateCommand(curriculumId: CurriculumId.From(0));
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "requestPayload[0].curriculumId");
        _curriculumRepositoryMock.Verify(
            r => r.ListAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region NumberOfSections Validation

    [Fact(DisplayName = "NumberOfSections is zero - returns invalid result")]
    public async Task ValidateAsync_NumberOfSectionsIsZero_HasNumberOfSectionsError()
    {
        var command = CreateCommand(numberOfSections: 0);
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    [Fact(DisplayName = "NumberOfSections is negative - returns invalid result")]
    public async Task ValidateAsync_NumberOfSectionsIsNegative_HasNumberOfSectionsError()
    {
        var command = CreateCommand(numberOfSections: -3);
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    [Fact(DisplayName = "NumberOfSections is positive - no numberOfSections error")]
    public async Task ValidateAsync_NumberOfSectionsIsPositive_NoNumberOfSectionsError()
    {
        var command = CreateCommand(numberOfSections: 3);
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    #endregion

    #region Full Valid Command

    [Fact(DisplayName = "All fields valid without curriculum - validation passes")]
    public async Task ValidateAsync_AllFieldsValidWithoutCurriculum_IsValid()
    {
        var command = CreateCommand(numberOfSections: 2);
        SetupCourses(command.requestPayload[0].courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "All fields valid with matching curriculum - validation passes")]
    public async Task ValidateAsync_AllFieldsValidWithMatchingCurriculum_IsValid()
    {
        var courseId = CourseId.From(1);
        var curriculumId = CurriculumId.From(5);
        var command = CreateCommand(courseId: courseId, curriculumId: curriculumId, numberOfSections: 2);
        SetupCourses(courseId);
        SetupAcademicYears(command.academicTermId);
        SetupCurricula(CreateCurriculum(curriculumId, courseId));

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Bulk Validation (Multiple Payloads)

    [Fact(DisplayName = "Multiple payloads all valid - validation passes with exactly one bulk DB call per entity type")]
    public async Task ValidateAsync_MultiplePayloadsAllValid_IsValidAndUsesOneBulkCallPerEntityType()
    {
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(10);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            YearLevel.From(1),
        [
            new(courseId1, CurriculumId.From(0), 2),
            new(courseId2, CurriculumId.From(0), 3),
        ]);
        SetupCourses(courseId1, courseId2);
        SetupAcademicYears(academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        _courseRepositoryMock.Verify(
            r => r.ListAsync(It.IsAny<ISpecification<Course>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _academicYearRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Multiple payloads - missing course in second entry produces error at index 1 only")]
    public async Task ValidateAsync_MultiplePayloadsMissingCourseInSecondEntry_HasErrorAtIndexOneOnly()
    {
        var courseId1 = CourseId.From(1);
        var missingCourseId = CourseId.From(99);
        var academicTermId = AcademicTermId.From(10);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            YearLevel.From(1),
        [
            new(courseId1, CurriculumId.From(0), 1),
            new(missingCourseId, CurriculumId.From(0), 1),
        ]);
        SetupCourses(courseId1);
        SetupAcademicYears(academicTermId);
        SetupCurricula();

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "requestPayload[0].courseId");
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "requestPayload[1].courseId" &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact(DisplayName = "Multiple payloads with curricula - exactly one bulk DB call for curricula")]
    public async Task ValidateAsync_MultiplePayloadsWithCurricula_UsesOneBulkCurriculumCall()
    {
        var courseId = CourseId.From(1);
        var academicTermId = AcademicTermId.From(10);
        var curriculumId1 = CurriculumId.From(5);
        var curriculumId2 = CurriculumId.From(6);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            YearLevel.From(1),
        [
            new(courseId, curriculumId1, 1),
            new(courseId, curriculumId2, 2),
        ]);
        SetupCourses(courseId);
        SetupAcademicYears(academicTermId);
        SetupCurricula(
            CreateCurriculum(curriculumId1, courseId),
            CreateCurriculum(curriculumId2, courseId));

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        _curriculumRepositoryMock.Verify(
            r => r.ListAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static BulkInitializeClassSectionsForAcademicYear.Command CreateCommand(
        AcademicTermId? academicTermId = null,
        YearLevel? yearLevel = null,
        CourseId? courseId = null,
        CurriculumId? curriculumId = null,
        int numberOfSections = 1,
        int payloadCount = 1)
    {
        var payloads = payloadCount == 0
            ? new List<BulkInitializeClassSectionsForAcademicYear.Payload>()
            : Enumerable.Range(0, payloadCount)
                .Select(_ => new BulkInitializeClassSectionsForAcademicYear.Payload(
                    courseId ?? CourseId.From(1),
                    curriculumId ?? CurriculumId.From(0),
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
            // No academic years - validator will fail
            _academicYearRepositoryMock
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AcademicYear?)null);
            return;
        }

        // Create an academic year with terms matching the provided IDs
        var startDate = AcademicYearStartDate.From(new DateTime(2024, 8, 1));
        var endDate = AcademicYearEndDate.From(new DateTime(2025, 7, 31));
        var academicYear = new AcademicYear(startDate, endDate);
        SetEntityProperty(academicYear, "Id", AcademicYearId.From(1));

        // Any call to FirstOrDefaultAsync for academic year will return this academic year
        // (validator uses GetAcademicYearByAcademicTermIdSpec which fetches by term ID)
        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);
    }

    private void SetupCurricula(params Curriculum[] curricula)
    {
        _curriculumRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. curricula]);
    }

    private static Curriculum CreateCurriculum(CurriculumId curriculumId, CourseId courseId)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = courseId,
            EffectiveYear = 2024,
            Version = "2024-A",
            Description = "Test curriculum"
        });
        SetEntityProperty(curriculum, "Id", curriculumId);
        return curriculum;
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
