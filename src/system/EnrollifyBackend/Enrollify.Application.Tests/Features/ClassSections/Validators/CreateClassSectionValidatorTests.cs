using Ardalis.Specification;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.Validators;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.ClassSections.Validators;

public class CreateClassSectionValidatorTests
{
    private readonly Mock<IReadRepository<Course>> _courseRepositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _academicYearRepositoryMock = new();
    private readonly Mock<IReadRepository<Teacher>> _teacherRepositoryMock = new();
    private readonly Mock<IReadRepository<ClassSection>> _classSectionRepositoryMock = new();
    private readonly Mock<IReadRepository<CourseCurriculumAssignment>> _courseCurriculumAssignmentRepositoryMock = new();
    private readonly CreateClassSectionValidator _validator;

    public CreateClassSectionValidatorTests()
    {
        _validator = new CreateClassSectionValidator(
            _courseRepositoryMock.Object,
            _academicYearRepositoryMock.Object,
            _teacherRepositoryMock.Object,
            _classSectionRepositoryMock.Object,
            _courseCurriculumAssignmentRepositoryMock.Object);
    }

    #region CourseId Validation

    [Fact(DisplayName = "CourseId does not exist - returns invalid result")]
    public async Task ValidateAsync_CourseDoesNotExist_HasCourseError()
    {
        // Arrange
        var command = CreateCommand();
        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CourseId" && e.ErrorMessage.Contains("course does not exist"));
    }

    [Fact(DisplayName = "CourseId exists - no course error")]
    public async Task ValidateAsync_CourseExists_NoCourseError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "CourseId");
    }

    #endregion

    #region AcademicTermId Validation

    [Fact(DisplayName = "AcademicTermId - academic year not found - returns invalid result")]
    public async Task ValidateAsync_AcademicYearNotFound_HasAcademicTermError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);
        SetupNoExistingSections();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AcademicTermId" && e.ErrorMessage.Contains("academic term does not exist"));
    }

    [Fact(DisplayName = "AcademicTermId - academic year found but term not in it - returns invalid result")]
    public async Task ValidateAsync_AcademicTermNotInAcademicYear_HasAcademicTermError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);

        // Return an academic year whose terms do not include the requested term id
        var academicYear = CreateAcademicYear(AcademicTermId.From(999));
        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);
        SetupNoExistingSections();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AcademicTermId" && e.ErrorMessage.Contains("academic term does not exist"));
    }

    [Fact(DisplayName = "AcademicTermId exists in academic year - no academic term error")]
    public async Task ValidateAsync_AcademicTermExists_NoAcademicTermError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "AcademicTermId");
    }

    #endregion

    #region AdviserId Validation

    [Fact(DisplayName = "AdviserId provided but teacher does not exist - returns invalid result")]
    public async Task ValidateAsync_AdviserDoesNotExist_HasAdviserError()
    {
        // Arrange
        var adviserId = TeacherId.From(99);
        var command = CreateCommand(adviserId: adviserId);
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        _teacherRepositoryMock
            .Setup(r => r.GetByIdAsync(adviserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AdviserId" && e.ErrorMessage.Contains("adviser does not exist"));
    }

    [Fact(DisplayName = "AdviserId provided and teacher exists - no adviser error")]
    public async Task ValidateAsync_AdviserExists_NoAdviserError()
    {
        // Arrange
        var adviserId = TeacherId.From(1);
        var command = CreateCommand(adviserId: adviserId);
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        var teacher = CreateTeacher(adviserId);
        _teacherRepositoryMock
            .Setup(r => r.GetByIdAsync(adviserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "AdviserId");
    }

    #endregion

    #region StudentCapacity Validation

    [Fact(DisplayName = "StudentCapacity is zero - returns invalid result")]
    public async Task ValidateAsync_StudentCapacityIsZero_HasCapacityError()
    {
        // Arrange
        var command = CreateCommand(studentCapacity: 0);
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StudentCapacity" && e.ErrorMessage.Contains("greater than zero"));
    }

    [Fact(DisplayName = "StudentCapacity is negative - returns invalid result")]
    public async Task ValidateAsync_StudentCapacityIsNegative_HasCapacityError()
    {
        // Arrange
        var command = CreateCommand(studentCapacity: -5);
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StudentCapacity" && e.ErrorMessage.Contains("greater than zero"));
    }

    [Fact(DisplayName = "StudentCapacity is positive - no capacity error")]
    public async Task ValidateAsync_StudentCapacityIsPositive_NoCapacityError()
    {
        // Arrange
        var command = CreateCommand(studentCapacity: 30);
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "StudentCapacity");
    }

    #endregion

    #region ClassSection Name Uniqueness Validation

    [Fact(DisplayName = "Class section name already exists for academic term - returns invalid result")]
    public async Task ValidateAsync_ClassSectionNameAlreadyExists_HasDuplicateError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();

        // Simulate a duplicate section already occupying the computed name
        var duplicateSection = CreateClassSection(SectionCode.From('A'));
        _classSectionRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateSection);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("already exists"));
    }

    [Fact(DisplayName = "Class section name is unique for academic term - no duplicate error")]
    public async Task ValidateAsync_ClassSectionNameIsUnique_NoDuplicateError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        _classSectionRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassSection?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("already exists"));
    }

    #endregion

    #region CourseCurriculumAssignment Validation

    [Fact(DisplayName = "No curriculum assignment for cohort entry year - returns CourseId error")]
    public async Task ValidateAsync_NoCurriculumAssignment_HasCurriculumAssignmentError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        _courseCurriculumAssignmentRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CourseCurriculumAssignment?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == "CourseId" &&
            e.ErrorMessage.Contains("No curriculum assignment found"));
    }

    [Fact(DisplayName = "Curriculum assignment exists for cohort entry year - no curriculum assignment error")]
    public async Task ValidateAsync_CurriculumAssignmentExists_NoCurriculumAssignmentError()
    {
        // Arrange
        var command = CreateCommand();
        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        _classSectionRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassSection?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.ErrorMessage.Contains("No curriculum assignment found"));
    }

    #endregion

    #region Full Valid Command

    [Fact(DisplayName = "All fields valid - validation passes")]
    public async Task ValidateAsync_AllFieldsValid_IsValid()
    {
        // Arrange
        var adviserId = TeacherId.From(1);
        var command = CreateCommand(adviserId: adviserId);

        SetupValidCourse(command);
        SetupValidAcademicTerm(command.AcademicTermId);
        SetupNoExistingSections();
        SetupValidCurriculumAssignment();

        _teacherRepositoryMock
            .Setup(r => r.GetByIdAsync(adviserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTeacher(adviserId));

        _classSectionRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassSection?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Helper Methods

    private static CreateClassSection.Command CreateCommand(
        YearLevel? yearLevel = null,
        CourseId? courseId = null,
        AcademicTermId? academicTermId = null,
        TeacherId? adviserId = null,
        int studentCapacity = 30)
    {
        return new CreateClassSection.Command(
            yearLevel ?? YearLevel.From(1),
            courseId ?? CourseId.From(1),
            academicTermId ?? AcademicTermId.From(1),
            adviserId ?? TeacherId.From(0),
            studentCapacity);
    }

    private void SetupValidCourse(CreateClassSection.Command command)
    {
        var course = new Course(
            CourseCode.From("BSCS"),
            "Bachelor of Science in Computer Science",
            4,
            "Computer Science degree program",
            CollegeId.From(1));
        SetEntityProperty(course, "Id", command.CourseId);

        _courseRepositoryMock
            .Setup(r => r.GetByIdAsync(command.CourseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
    }

    private void SetupValidAcademicTerm(AcademicTermId academicTermId)
    {
        var academicYear = CreateAcademicYear(academicTermId);
        _academicYearRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);
    }

    private void SetupValidCurriculumAssignment()
    {
        var assignment = new CourseCurriculumAssignment(CourseId.From(1), AcademicYearId.From(1), CurriculumId.From(1));
        _courseCurriculumAssignmentRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);
    }

    private void SetupNoExistingSections()
    {
        _classSectionRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
    }

    private static AcademicYear CreateAcademicYear(AcademicTermId academicTermId)
    {
        var startDate = AcademicYearStartDate.From(new DateTime(2024, 8, 1));
        var endDate = AcademicYearEndDate.From(new DateTime(2025, 7, 31));
        var academicYear = new AcademicYear(startDate, endDate);
        SetEntityProperty(academicYear, "Id", AcademicYearId.From(1));

        var termStartDate = AcademicTermStartDate.From(new DateTime(2024, 8, 1));
        var termEndDate = AcademicTermEndDate.From(new DateTime(2024, 12, 31));
        var academicTerm = new AcademicTerm(TermNumber.From(1), AcademicYearId.From(1), termStartDate, termEndDate);
        SetEntityProperty(academicTerm, "Id", academicTermId);
        SetEntityProperty(academicYear, "_academicTerms", new List<AcademicTerm> { academicTerm });

        return academicYear;
    }

    private static Teacher CreateTeacher(TeacherId teacherId)
    {
        var teacher = new Teacher(
            "John",
            "M.",
            "Doe",
            TeacherIdentifier.From("T-001"),
            TeacherEmail.From("john.doe@test.com"),
            TeacherPhoneNumber.From("09123456789"),
            DepartmentId.From(1));
        SetEntityProperty(teacher, "Id", teacherId);
        return teacher;
    }

    private static ClassSection CreateClassSection(SectionCode sectionCode)
    {
        var classSection = new ClassSection(new ClassSectionForCreation
        {
            Name = $"BSCS-1{sectionCode}",
            IntendedYearLevel = YearLevel.From(1),
            CourseId = CourseId.From(1),
            CurriculumId = CurriculumId.From(1),
            AcademicTermId = AcademicTermId.From(1),
            CohortAcademicYearId = AcademicYearId.From(1),
            AdviserId = null,
            SectionCode = sectionCode
        });
        SetEntityProperty(classSection, "Id", ClassSectionId.From(1));
        return classSection;
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