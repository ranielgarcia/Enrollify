using Ardalis.Specification;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.ClassSections.Validators;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
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
        // Arrange
        var command = new BulkInitializeClassSectionsForAcademicYear.Command([]);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "requestPayload" && e.ErrorMessage.Contains("At least one payload entry is required"));
    }

    [Fact(DisplayName = "Non-empty requestPayload - no collection-level error")]
    public async Task ValidateAsync_NonEmptyRequestPayload_NoCollectionError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "requestPayload");
    }

    #endregion

    #region CourseId Validation

    [Fact(DisplayName = "CourseId does not exist - returns invalid result")]
    public async Task ValidateAsync_CourseDoesNotExist_HasCourseError()
    {
        // Arrange
        var command = CreateCommand();
        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(command.requestPayload[0].courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("course does not exist"));
    }

    [Fact(DisplayName = "CourseId exists - no course error")]
    public async Task ValidateAsync_CourseExists_NoCourseError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("course does not exist"));
    }

    #endregion

    #region AcademicYearId Validation

    [Fact(DisplayName = "AcademicYearId does not exist - returns invalid result")]
    public async Task ValidateAsync_AcademicYearDoesNotExist_HasAcademicYearError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command.requestPayload[0].courseId);
        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("academic year does not exist"));
    }

    [Fact(DisplayName = "AcademicYearId exists - no academic year error")]
    public async Task ValidateAsync_AcademicYearExists_NoAcademicYearError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("academic year does not exist"));
    }

    #endregion

    #region CurriculumId Validation

    [Fact(DisplayName = "CurriculumId provided but curriculum does not exist - returns invalid result")]
    public async Task ValidateAsync_CurriculumDoesNotExist_HasCurriculumError()
    {
        // Arrange
        var courseId = CourseId.From(1);
        var command = CreateCommand(courseId: courseId, curriculumId: CurriculumId.From(99));
        SetupValidCourse(courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        _curriculumRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curriculum?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("curriculum does not exist or does not belong to the specified course"));
    }

    [Fact(DisplayName = "CurriculumId provided but curriculum belongs to different course - returns invalid result")]
    public async Task ValidateAsync_CurriculumBelongsToDifferentCourse_HasCurriculumError()
    {
        // Arrange
        var courseId = CourseId.From(1);
        var differentCourseId = CourseId.From(2);
        var curriculumId = CurriculumId.From(5);
        var command = CreateCommand(courseId: courseId, curriculumId: curriculumId);
        SetupValidCourse(courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);

        var curriculum = CreateCurriculum(curriculumId, differentCourseId);
        _curriculumRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("curriculum does not exist or does not belong to the specified course"));
    }

    [Fact(DisplayName = "CurriculumId provided and belongs to the correct course - no curriculum error")]
    public async Task ValidateAsync_CurriculumBelongsToCorrectCourse_NoCurriculumError()
    {
        // Arrange
        var courseId = CourseId.From(1);
        var curriculumId = CurriculumId.From(5);
        var command = CreateCommand(courseId: courseId, curriculumId: curriculumId);
        SetupValidCourse(courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);

        var curriculum = CreateCurriculum(curriculumId, courseId);
        _curriculumRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("curriculum does not exist or does not belong to the specified course"));
    }

    [Fact(DisplayName = "CurriculumId is zero (not provided) - curriculum validation is skipped")]
    public async Task ValidateAsync_CurriculumIdIsZero_CurriculumValidationSkipped()
    {
        // Arrange
        var command = CreateCommand(curriculumId: CurriculumId.From(0));
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("curriculum does not exist or does not belong to the specified course"));
        _curriculumRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region NumberOfSections Validation

    [Fact(DisplayName = "NumberOfSections is zero - returns invalid result")]
    public async Task ValidateAsync_NumberOfSectionsIsZero_HasNumberOfSectionsError()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 0);
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    [Fact(DisplayName = "NumberOfSections is negative - returns invalid result")]
    public async Task ValidateAsync_NumberOfSectionsIsNegative_HasNumberOfSectionsError()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: -3);
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    [Fact(DisplayName = "NumberOfSections is positive - no numberOfSections error")]
    public async Task ValidateAsync_NumberOfSectionsIsPositive_NoNumberOfSectionsError()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 3);
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("Number of sections must be greater than zero"));
    }

    #endregion

    #region Full Valid Command

    [Fact(DisplayName = "All fields valid without curriculum - validation passes")]
    public async Task ValidateAsync_AllFieldsValidWithoutCurriculum_IsValid()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 2);
        SetupValidCourse(command.requestPayload[0].courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);
        SetupNoCurriculum();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "All fields valid with matching curriculum - validation passes")]
    public async Task ValidateAsync_AllFieldsValidWithMatchingCurriculum_IsValid()
    {
        // Arrange
        var courseId = CourseId.From(1);
        var curriculumId = CurriculumId.From(5);
        var command = CreateCommand(courseId: courseId, curriculumId: curriculumId, numberOfSections: 2);
        SetupValidCourse(courseId);
        SetupValidAcademicYear(command.requestPayload[0].academicYearId);

        var curriculum = CreateCurriculum(curriculumId, courseId);
        _curriculumRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Helper Methods

    private static BulkInitializeClassSectionsForAcademicYear.Command CreateCommand(
        CourseId? courseId = null,
        AcademicYearId? academicYearId = null,
        CurriculumId? curriculumId = null,
        int numberOfSections = 1)
    {
        var payload = new BulkInitializeClassSectionsForAcademicYear.Payload(
            courseId ?? CourseId.From(1),
            academicYearId ?? AcademicYearId.From(1),
            curriculumId ?? CurriculumId.From(0),
            numberOfSections);

        return new BulkInitializeClassSectionsForAcademicYear.Command([payload]);
    }

    private void SetupValidCourse(CourseId courseId)
    {
        var course = new Course(
            CourseCode.From("BSCS"),
            "Bachelor of Science in Computer Science",
            4,
            "Computer Science degree program",
            CollegeId.From(1));
        SetEntityProperty(course, "Id", courseId);

        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
    }

    private void SetupValidAcademicYear(AcademicYearId academicYearId)
    {
        var startDate = AcademicYearStartDate.From(new DateTime(2024, 8, 1));
        var endDate = AcademicYearEndDate.From(new DateTime(2025, 7, 31));
        var academicYear = new AcademicYear(startDate, endDate);
        SetEntityProperty(academicYear, "Id", academicYearId);

        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);
    }

    private void SetupNoCurriculum()
    {
        _curriculumRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curriculum?)null);
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
