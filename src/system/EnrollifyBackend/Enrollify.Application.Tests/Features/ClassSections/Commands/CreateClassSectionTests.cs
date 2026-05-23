using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.ClassSections;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Application.Features.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.ClassSections.Commands;

public class CreateClassSectionTests
{
    private readonly Mock<IReadRepository<AcademicYear>> _academicYearReadRepositoryMock = new();
    private readonly Mock<IReadRepository<ClassSection>> _classSectionReadRepositoryMock = new();
    private readonly Mock<IReadRepository<CourseCurriculumAssignment>> _courseCurriculumAssignmentReadRepositoryMock = new();
    private readonly Mock<IClassSectionRepository> _classSectionRepositoryMock = new();
    private readonly Mock<IClassSectionSubjectOfferingRepository> _mockClassSectionSubjectOfferingRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeTransactionScope _fakeTransaction = new();
    private readonly FakeLogger<CreateClassSection.Handler> _logger;
    private readonly CreateClassSection.Handler _handler;

    public CreateClassSectionTests()
    {
        _logger = new FakeLogger<CreateClassSection.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new CreateClassSection.Handler(
            _academicYearReadRepositoryMock.Object,
            _classSectionReadRepositoryMock.Object,
            _courseCurriculumAssignmentReadRepositoryMock.Object,
            _classSectionRepositoryMock.Object,
            _mockClassSectionSubjectOfferingRepository.Object,
            _unitOfWorkMock.Object,
            _logger);

        // Default setup for transaction - use fake implementation
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fakeTransaction);
    }

    #region Curriculum Scenarios

    [Fact(DisplayName = "No course-curriculum assignment found for course and academic year")]
    public async Task Handle_NoActiveCurriculumFound_ReturnsInvalidResult()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);

        // _courseCurriculumAssignmentReadRepositoryMock not set up → returns null

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("No course-curriculum assignment found"));
    }

    [Fact(DisplayName = "Course-curriculum assignment found but no subjects for year level and term")]
    public async Task Handle_CurriculumWithNoSubjects_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);

        var curriculum = CreateCurriculum(command, hasSubjects: false);
        var course = CreateCourse(command.courseId);
        var assignment = CreateCourseCurriculumAssignment(command.courseId, course, curriculum);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<CourseCurriculumAssignment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

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
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        Exception? caughtException = null;
        Result<ClassSectionId> result;
        try
        {
            result = await _handler.Handle(command, CancellationToken.None);
        }
        catch (Exception ex)
        {
            caughtException = ex;
            throw;
        }

        // Assert
        if (caughtException != null)
        {
            Assert.Fail($"Unexpected exception: {caughtException.GetType().Name}: {caughtException.Message}\n{caughtException.StackTrace}");
        }
        if (!result.IsSuccess)
        {
            var errors = string.Join("; ", result.Errors);
            Assert.True(result.IsSuccess, $"Expected success but got errors: {errors}");
        }
        Assert.True(result.IsSuccess);
        _courseCurriculumAssignmentReadRepositoryMock.Verify(
            r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<CourseCurriculumAssignment>>(),
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
        SetupHandlerPrerequisites(command);
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

    [Fact(DisplayName = "Subject offering creation fails (ClassSectionSubjectOfferingRepository returns error)")]
    public async Task Handle_SubjectOfferingCreationFails_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();

        _mockClassSectionSubjectOfferingRepository
            .Setup(r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()))
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
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Override with a mock transaction that throws on commit
        var throwingTransactionMock = new Mock<ITransactionScope>();
        throwingTransactionMock
            .Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database commit failed"));
        throwingTransactionMock
            .Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask);
        
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(throwingTransactionMock.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("unexpected error"));
    }

    [Fact(DisplayName = "No curriculum found does not begin transaction")]
    public async Task Handle_NoCurriculumFound_DoesNotBeginTransaction()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);

        // No course-curriculum assignment found
        // _courseCurriculumAssignmentReadRepositoryMock not set up → returns null

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("No course-curriculum assignment found"));
        // Transaction should never be started for validation errors
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.False(_fakeTransaction.IsCommitted);
        Assert.False(_fakeTransaction.IsRolledBack);
    }

    [Fact(DisplayName = "No curriculum subjects does not begin transaction")]
    public async Task Handle_NoCurriculumSubjects_DoesNotBeginTransaction()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);

        // Curriculum found but no subjects
        var curriculum = CreateCurriculum(command, hasSubjects: false);
        var course = CreateCourse(command.courseId);
        var assignment = CreateCourseCurriculumAssignment(command.courseId, course, curriculum);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<CourseCurriculumAssignment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("No curriculum subjects found"));
        // Transaction should never be started when no subjects exist
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.False(_fakeTransaction.IsCommitted);
        Assert.False(_fakeTransaction.IsRolledBack);
    }

    [Fact(DisplayName = "Class section creation failure causes rollback")]
    public async Task Handle_ClassSectionCreationFails_CausesRollback()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);

        // Class section creation fails
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error("Unable to create the class section."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Unable to create the class section"));
        // Transaction began but should not commit due to error
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_fakeTransaction.IsCommitted);
        // Note: Explicit rollback is not called in the command - transaction auto-rollbacks on dispose without commit
    }

    [Fact(DisplayName = "Subject offering creation failure causes rollback")]
    public async Task Handle_SubjectOfferingCreationFails_CausesRollback()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();

        // Subject offering creation fails
        _mockClassSectionSubjectOfferingRepository
            .Setup(r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionSubjectOfferingId>.Error("Unable to create subject offering."));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Unable to create one or more subject offerings"));
        // Transaction began but should not commit due to error
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_fakeTransaction.IsCommitted);
        // Note: Explicit rollback is not called in the command - transaction auto-rollbacks on dispose without commit
    }

    [Fact(DisplayName = "Successful request commits transaction")]
    public async Task Handle_SuccessfulRequest_CommitsTransaction()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(_fakeTransaction.IsCommitted);
        Assert.False(_fakeTransaction.IsRolledBack);
    }

    [Fact(DisplayName = "Exception during processing causes automatic rollback")]
    public async Task Handle_ExceptionDuringProcessing_CausesAutomaticRollback()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);

        // Simulate an unexpected exception during class section creation
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("unexpected error"));
        // Transaction began but should not commit due to exception
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_fakeTransaction.IsCommitted);
        // Transaction automatically rolls back on dispose when exception occurs
    }

    #endregion

    #region Success Scenarios

    [Fact(DisplayName = "Valid command with first section (section code 'A')")]
    public async Task Handle_FirstSection_CreatesSectionCodeA()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
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
        SetupHandlerPrerequisites(command);
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
        SetupHandlerPrerequisites(command);

        var curriculum = CreateCurriculum(command, hasSubjects: true, subjectCount: 3);
        var course = CreateCourse(command.courseId);
        var assignment = CreateCourseCurriculumAssignment(command.courseId, course, curriculum);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<CourseCurriculumAssignment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        SetupSuccessfulClassSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _mockClassSectionSubjectOfferingRepository.Verify(
            r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact(DisplayName = "Valid command logs success")]
    public async Task Handle_ValidCommand_LogsSuccess()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
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

    [Fact(DisplayName = "New ClassSection defaults to Draft status")]
    public async Task Handle_ValidCommand_CreatesClassSectionWithDraftStatus()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);
        SetupSuccessfulCurriculumRetrieval(command);
        SetupSuccessfulSubjectOfferingCreation();

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
        Assert.Equal(ClassSectionStatusEnum.Draft, capturedClassSection.StatusId);
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

    private void SetupHandlerPrerequisites(CreateClassSection.Command command)
    {
        // Reset fake transaction state for each test
        _fakeTransaction.Reset();
        
        var academicYear = CreateAcademicYear(command.academicTermId);

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
        var course = CreateCourse(command.courseId);
        var assignment = CreateCourseCurriculumAssignment(command.courseId, course, curriculum);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<CourseCurriculumAssignment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);
    }

    private void SetupSuccessfulClassSectionCreation()
    {
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));
    }

    private void SetupSuccessfulSubjectOfferingCreation()
    {
        _mockClassSectionSubjectOfferingRepository
            .Setup(r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()))
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
            EffectiveYear = Year.From(2024),
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
                SetEntityProperty(subject, "IsActive", true);
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
            CurriculumId = CurriculumId.From(1),
            AcademicTermId = AcademicTermId.From(1),
            AdviserId = TeacherId.From(1),
            SectionCode = sectionCode
        });
        SetEntityProperty(classSection, "Id", ClassSectionId.From(1));
        return classSection;
    }

    private static CourseCurriculumAssignment CreateCourseCurriculumAssignment(
        CourseId courseId,
        Course course,
        Curriculum curriculum)
    {
        var assignment = new CourseCurriculumAssignment(courseId, AcademicYearId.From(1), curriculum.Id);
        SetEntityProperty(assignment, "Course", course);
        SetEntityProperty(assignment, "Curriculum", curriculum);
        return assignment;
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

/// <summary>
/// Fake transaction implementation for testing
/// </summary>
internal class FakeTransactionScope : ITransactionScope
{
    public bool IsCommitted { get; private set; }
    public bool IsRolledBack { get; private set; }
    public bool IsDisposed { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        IsCommitted = true;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        IsRolledBack = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }

    public void Reset()
    {
        IsCommitted = false;
        IsRolledBack = false;
        IsDisposed = false;
    }
}
