using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.ClassSections;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Features.ClassSectionSubjectOfferings;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.ClassSections.Commands;

public class CreateClassSectionTests
{
    private readonly Mock<IReadRepository<Course>> _courseReadRepositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _academicYearReadRepositoryMock = new();
    private readonly Mock<IReadRepository<ClassSection>> _classSectionReadRepositoryMock = new();
    private readonly Mock<IReadRepository<Curriculum>> _curriculumReadRepositoryMock = new();
    private readonly Mock<IClassSectionRepository> _classSectionRepositoryMock = new();
    private readonly Mock<IValidator<CreateClassSection.Command>> _validatorMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransactionScope> _transactionMock = new();
    private readonly FakeLogger<CreateClassSection.Handler> _logger;
    private readonly CreateClassSection.Handler _handler;

    public CreateClassSectionTests()
    {
        _logger = new FakeLogger<CreateClassSection.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new CreateClassSection.Handler(
            _courseReadRepositoryMock.Object,
            _academicYearReadRepositoryMock.Object,
            _classSectionReadRepositoryMock.Object,
            _curriculumReadRepositoryMock.Object,
            _classSectionRepositoryMock.Object,
            _validatorMock.Object,
            _mediatorMock.Object,
            _unitOfWorkMock.Object,
            _logger);

        // Default setup for transaction
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);
    }

    #region Validation Scenarios

    [Fact(DisplayName = "Invalid command - validation fails (courseId doesn't exist)")]
    public async Task Handle_CourseIdDoesNotExist_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand();
        var validationResult = new ValidationResult(
            new[] { new ValidationFailure("courseId", "The specified course does not exist.") });

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("course"));
    }

    [Fact(DisplayName = "Invalid command - validation fails (academicTermId doesn't exist)")]
    public async Task Handle_AcademicTermIdDoesNotExist_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand();
        var validationResult = new ValidationResult(
            new[] { new ValidationFailure("academicTermId", "The specified academic term does not exist.") });

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("academic term"));
    }

    [Fact(DisplayName = "Invalid command - validation fails (adviserId doesn't exist)")]
    public async Task Handle_AdviserIdDoesNotExist_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand();
        var validationResult = new ValidationResult(
            new[] { new ValidationFailure("adviserId", "The specified adviser does not exist.") });

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("adviser"));
    }

    [Fact(DisplayName = "Invalid command - validation fails (studentCapacity <= 0)")]
    public async Task Handle_StudentCapacityLessThanOrEqualToZero_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand(studentCapacity: 0);
        var validationResult = new ValidationResult(
            new[] { new ValidationFailure("studentCapacity", "Student capacity must be greater than zero.") });

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("capacity"));
    }

    [Fact(DisplayName = "Invalid command - validation fails (class section name already exists)")]
    public async Task Handle_ClassSectionNameAlreadyExists_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand();
        var validationResult = new ValidationResult(
            new[] { new ValidationFailure("", "A class section with the same name already exists for this academic term.") });

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("already exists"));
    }

    [Fact(DisplayName = "Valid command - all validation passes")]
    public async Task Handle_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Curriculum Scenarios

    [Fact(DisplayName = "No active curriculum found for course, year level, and term")]
    public async Task Handle_NoActiveCurriculumFound_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Curriculum>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curriculum?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("No active curriculum found"));
    }

    [Fact(DisplayName = "Curriculum found but no subjects for year level and term")]
    public async Task Handle_CurriculumWithNoSubjects_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);

        var curriculum = CreateCurriculum(command, hasSubjects: false);
        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Curriculum>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("No curriculum subjects found"));
    }

    [Fact(DisplayName = "Curriculum with subjects found - success path")]
    public async Task Handle_CurriculumWithSubjectsFound_Succeeds()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _curriculumReadRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Curriculum>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Repository/Mediator Interaction Scenarios

    [Fact(DisplayName = "Class section creation fails (repository returns error)")]
    public async Task Handle_ClassSectionCreationFails_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);

        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionId>.Error("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Unable to create the class section"));
    }

    [Fact(DisplayName = "Subject offering creation fails (mediator returns error)")]
    public async Task Handle_SubjectOfferingCreationFails_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();

        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<CreateClassSectionSubjectOffering.Command>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionSubjectOfferingId>.Error("Failed to create offering"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Unable to create one or more subject offerings"));
    }

    [Fact(DisplayName = "Transaction commit fails (exception thrown)")]
    public async Task Handle_TransactionCommitFails_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        _transactionMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database commit failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("unexpected error"));
    }

    #endregion

    #region Success Scenarios

    [Fact(DisplayName = "Valid command with first section (section code 'A')")]
    public async Task Handle_FirstSection_CreatesSectionCodeA()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulSubjectOfferingCreation();

        // No existing sections, so should get 'A'
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<ClassSection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());

        ClassSection? capturedClassSection = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedClassSection = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedClassSection);
        Assert.Equal(SectionCode.From('A'), capturedClassSection.SectionCode);
    }

    [Fact(DisplayName = "Valid command with existing sections (section code 'B', 'C', etc.)")]
    public async Task Handle_WithExistingSections_CreatesNextSectionCode()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulSubjectOfferingCreation();

        // Existing section with code 'A'
        var existingSection = CreateClassSection(SectionCode.From('A'));
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<ClassSection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection> { existingSection });

        ClassSection? capturedClassSection = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedClassSection = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(2)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedClassSection);
        Assert.Equal(SectionCode.From('B'), capturedClassSection.SectionCode);
    }

    [Fact(DisplayName = "Valid command creates all subject offerings")]
    public async Task Handle_ValidCommand_CreatesAllSubjectOfferings()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);

        var curriculum = CreateCurriculum(command, hasSubjects: true, subjectCount: 3);
        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Curriculum>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateClassSectionSubjectOffering.Command>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact(DisplayName = "Valid command commits transaction")]
    public async Task Handle_ValidCommand_CommitsTransaction()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _transactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Valid command logs success")]
    public async Task Handle_ValidCommand_LogsSuccess()
    {
        // Arrange
        var command = CreateCommand();
        SetupSuccessfulValidation(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var logs = _logger.Collector.GetSnapshot();
        Assert.Contains(logs, l => l.Level == LogLevel.Information && l.Message.Contains("Successfully created class section"));
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
            adviserId ?? TeacherId.From(1),
            studentCapacity);
    }

    private void SetupSuccessfulValidation(CreateClassSection.Command command)
    {
        var course = CreateCourse(command.courseId);
        var academicYear = CreateAcademicYear(command.academicTermId);

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _courseReadRepositoryMock
            .Setup(r => r.GetByIdAsync(command.courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _academicYearReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);

        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<ClassSection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
    }

    private void SetupSuccessfulCurriculumRetrieval(CreateClassSection.Command command)
    {
        var curriculum = CreateCurriculum(command, hasSubjects: true);
        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<Curriculum>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);
    }

    private void SetupSuccessfulClassSectionCreation()
    {
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));
    }

    private void SetupSuccessfulSubjectOfferingCreation()
    {
        _mediatorMock
            .Setup(m => m.Send(
                It.IsAny<CreateClassSectionSubjectOffering.Command>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ClassSectionSubjectOfferingId.From(1)));
    }

    private static Course CreateCourse(CourseId courseId)
    {
        var course = new Course(
            CourseCode.From("BSCS"),
            "Bachelor of Science in Computer Science",
            4,
            "Computer Science degree program",
            CollegeId.From(1));
        SetEntityProperty(course, "Id", courseId);
        return course;
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

        var academicTerms = new List<AcademicTerm> { academicTerm };
        SetEntityProperty(academicYear, "_academicTerms", academicTerms);

        return academicYear;
    }

    private static Curriculum CreateCurriculum(
        CreateClassSection.Command command,
        bool hasSubjects,
        int subjectCount = 2)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = command.courseId,
            EffectiveYear = 2024,
            Version = "2024-A",
            Description = "Computer Science Curriculum 2024"
        });
        SetEntityProperty(curriculum, "Id", CurriculumId.From(1));
        SetEntityProperty(curriculum, "StatusId", CurriculumStatusEnum.Active);

        if (hasSubjects)
        {
            var subjects = new List<CurriculumSubject>();
            for (int i = 1; i <= subjectCount; i++)
            {
                var subject = new CurriculumSubject(
                    CurriculumId.From(1),
                    SubjectId.From(i),
                    command.yearLevel,
                    TermNumber.From(1),
                    false,
                    null,
                    null);
                SetEntityProperty(subject, "Id", CurriculumSubjectId.From(i));
                subjects.Add(subject);
            }
            SetEntityProperty(curriculum, "_curriculumSubjects", subjects);
        }

        return curriculum;
    }

    private static ClassSection CreateClassSection(SectionCode sectionCode)
    {
        var classSection = new ClassSection(new ClassSectionForCreation
        {
            Name = $"BSCS-{YearLevel.From(1)}{sectionCode}",
            YearLevel = YearLevel.From(1),
            CourseId = CourseId.From(1),
            AcademicTermId = AcademicTermId.From(1),
            AdviserId = TeacherId.From(1),
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
            // Try to set private field
            var field = typeof(T).GetField(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(entity, value);
        }
    }

    #endregion
}
