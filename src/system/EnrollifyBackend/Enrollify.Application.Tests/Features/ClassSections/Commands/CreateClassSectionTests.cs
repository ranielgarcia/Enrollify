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
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate.Models;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using MediatR;
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
        Assert.Contains(result.Errors, e => e.Contains("Course-Curriculum assignment for the given cohort not found"));
    }

    [Fact(DisplayName = "Course-curriculum assignment found but no subjects for year level and term")]
    public async Task Handle_CurriculumWithNoSubjects_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        SetupHandlerPrerequisites(command);

        var curriculum = CreateCurriculum(command, hasSubjects: false);
        var course = CreateCourse(command.CourseId);
        var assignment = CreateCourseCurriculumAssignment(command.CourseId, course, curriculum);
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

    #region Multi-Curriculum Cohort Scenarios

    [Fact(DisplayName = "Year 1 - uses curriculum from current academic year")]
    public async Task Handle_Year1Command_UsesCurriculumFromCurrentAcademicYear()
    {
        // Arrange
        // Current AY: 2025-2026 (StartDate.Year = 2025). Year 1 cohort entry year = 2025 - (1-1) = 2025.
        var currentAY = CreateAcademicYearWithStartYear(2025, AcademicTermId.From(1));
        var currentAYCurriculumId = CurriculumId.From(10);
        var currentAYCurriculum = CreateCurriculumWithYear(CourseId.From(1), 2025, "2025-A", YearLevel.From(1), TermNumber.From(1), curriculumId: currentAYCurriculumId);
        var course = CreateCourse(CourseId.From(1));
        var assignment = CreateCourseCurriculumAssignment(CourseId.From(1), course, currentAYCurriculum);

        var command = CreateCommand(yearLevel: YearLevel.From(1));
        SetupCohortAcademicYears(currentAY, cohortAY: currentAY); // Year 1 → same AY
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
        SetupSuccessfulSubjectOfferingCreation();

        ClassSection? capturedSection = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedSection = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedSection);
        Assert.Equal(currentAYCurriculumId, capturedSection.CurriculumId);
    }

    [Fact(DisplayName = "Year 2 - uses curriculum from previous academic year (different from Year 1)")]
    public async Task Handle_Year2Command_UsesCurriculumFromPreviousAcademicYear()
    {
        // Arrange
        // Current AY: 2025-2026 (StartDate.Year = 2025). Year 2 cohort entry year = 2025 - (2-1) = 2024.
        var currentAY = CreateAcademicYearWithStartYear(2025, AcademicTermId.From(1));
        var previousAY = CreateAcademicYearWithStartYear(2024, academicYearId: AcademicYearId.From(2));
        var previousAYCurriculumId = CurriculumId.From(20);
        var previousAYCurriculum = CreateCurriculumWithYear(CourseId.From(1), 2024, "2024-A", YearLevel.From(2), TermNumber.From(1), curriculumId: previousAYCurriculumId);
        var course = CreateCourse(CourseId.From(1));
        var assignment = CreateCourseCurriculumAssignment(CourseId.From(1), course, previousAYCurriculum);

        var command = CreateCommand(yearLevel: YearLevel.From(2));
        SetupCohortAcademicYears(currentAY, cohortAY: previousAY);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
        SetupSuccessfulSubjectOfferingCreation();

        ClassSection? capturedSection = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedSection = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedSection);
        Assert.Equal(previousAYCurriculumId, capturedSection.CurriculumId);
    }

    [Fact(DisplayName = "Year 3 - uses curriculum from academic year two years prior")]
    public async Task Handle_Year3Command_UsesCurriculumFromTwoYearsAgo()
    {
        // Arrange
        // Current AY: 2025-2026 (StartDate.Year = 2025). Year 3 cohort entry year = 2025 - (3-1) = 2023.
        var currentAY = CreateAcademicYearWithStartYear(2025, AcademicTermId.From(1));
        var twoYearsAgoAY = CreateAcademicYearWithStartYear(2023, academicYearId: AcademicYearId.From(3));
        var twoYearsAgoCurriculumId = CurriculumId.From(30);
        var twoYearsAgoCurriculum = CreateCurriculumWithYear(CourseId.From(1), 2023, "2023-X", YearLevel.From(3), TermNumber.From(1), curriculumId: twoYearsAgoCurriculumId);
        var course = CreateCourse(CourseId.From(1));
        var assignment = CreateCourseCurriculumAssignment(CourseId.From(1), course, twoYearsAgoCurriculum);

        var command = CreateCommand(yearLevel: YearLevel.From(3));
        SetupCohortAcademicYears(currentAY, cohortAY: twoYearsAgoAY);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
        SetupSuccessfulSubjectOfferingCreation();

        ClassSection? capturedSection = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedSection = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedSection);
        Assert.Equal(twoYearsAgoCurriculumId, capturedSection.CurriculumId);
    }

    [Fact(DisplayName = "Year 1 and Year 2 in same AY - each picks a different curriculum")]
    public async Task Handle_Year1AndYear2InSameAY_EachPickDifferentCurriculum()
    {
        // Demonstrates that two separate CreateClassSection commands issued within the same AY
        // (Year 1 and Year 2) resolve to different cohort entry years and thus different curriculums.
        // Current AY: 2025-2026 (StartDate.Year = 2025).
        //   Year 1 cohort entry = 2025 → Curriculum 2025-A
        //   Year 2 cohort entry = 2024 → Curriculum 2024-A (different)
        var currentAY = CreateAcademicYearWithStartYear(2025, AcademicTermId.From(1));
        var previousAY = CreateAcademicYearWithStartYear(2024, academicYearId: AcademicYearId.From(2));

        var currentAYCurriculumId = CurriculumId.From(10);
        var previousAYCurriculumId = CurriculumId.From(20);

        var currentAYCurriculum = CreateCurriculumWithYear(CourseId.From(1), 2025, "2025-A", YearLevel.From(1), TermNumber.From(1), curriculumId: currentAYCurriculumId);
        var previousAYCurriculum = CreateCurriculumWithYear(CourseId.From(1), 2024, "2024-A", YearLevel.From(2), TermNumber.From(1), curriculumId: previousAYCurriculumId);
        var course = CreateCourse(CourseId.From(1));

        // ── Year 1 command ──
        var year1Command = CreateCommand(yearLevel: YearLevel.From(1));

        _academicYearReadRepositoryMock.Reset();
        _courseCurriculumAssignmentReadRepositoryMock.Reset();
        _classSectionRepositoryMock.Reset();
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_fakeTransaction);

        SetupCohortAcademicYears(currentAY, cohortAY: currentAY);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourseCurriculumAssignment(CourseId.From(1), course, currentAYCurriculum));
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
        SetupSuccessfulSubjectOfferingCreation();

        ClassSection? capturedYear1Section = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedYear1Section = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));

        var year1Result = await _handler.Handle(year1Command, CancellationToken.None);

        // ── Year 2 command ──
        var year2Command = CreateCommand(yearLevel: YearLevel.From(2));

        _academicYearReadRepositoryMock.Reset();
        _courseCurriculumAssignmentReadRepositoryMock.Reset();
        _classSectionRepositoryMock.Reset();
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_fakeTransaction);

        SetupCohortAcademicYears(currentAY, cohortAY: previousAY);
        _courseCurriculumAssignmentReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCourseCurriculumAssignment(CourseId.From(1), course, previousAYCurriculum));
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClassSection>());
        SetupSuccessfulSubjectOfferingCreation();

        ClassSection? capturedYear2Section = null;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => capturedYear2Section = cs)
            .ReturnsAsync(Result.Success(ClassSectionId.From(1)));

        var year2Result = await _handler.Handle(year2Command, CancellationToken.None);

        // Assert
        Assert.True(year1Result.IsSuccess);
        Assert.True(year2Result.IsSuccess);
        Assert.NotNull(capturedYear1Section);
        Assert.NotNull(capturedYear2Section);
        Assert.Equal(currentAYCurriculumId, capturedYear1Section.CurriculumId);
        Assert.Equal(previousAYCurriculumId, capturedYear2Section.CurriculumId);
        Assert.NotEqual(capturedYear1Section.CurriculumId, capturedYear2Section.CurriculumId);
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
        Assert.Contains(result.Errors, e => e.Contains("Course-Curriculum assignment for the given cohort not found"));
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
        var course = CreateCourse(command.CourseId);
        var assignment = CreateCourseCurriculumAssignment(command.CourseId, course, curriculum);
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
        var course = CreateCourse(command.CourseId);
        var assignment = CreateCourseCurriculumAssignment(command.CourseId, course, curriculum);
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

        var academicYear = CreateAcademicYear(command.AcademicTermId);

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
        var course = CreateCourse(command.CourseId);
        var assignment = CreateCourseCurriculumAssignment(command.CourseId, course, curriculum);
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
            CourseId = command.CourseId,
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
                    command.YearLevel,
                    TermNumber.From(1),
                    false,
                    null,
                    null);
                SetEntityProperty(subject, "Id", CurriculumSubjectId.From(i));
                SetEntityProperty(subject, "IsActive", true);
                var stubSubject = new Subject(new SubjectForCreation
                {
                    Code = SubjectCode.From($"SUBJ{i:D3}"),
                    Title = $"Subject {i}",
                    Units = 3m,
                    Description = $"Description {i}",
                    PreferRoomTypeId = RoomTypeId.From(1)
                });
                SetEntityProperty(stubSubject, "Id", SubjectId.From(i));
                SetEntityProperty(subject, "Subject", stubSubject);
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
            IntendedYearLevel = YearLevel.From(1),
            CourseId = CourseId.From(1),
            CurriculumId = CurriculumId.From(1),
            AcademicTermId = AcademicTermId.From(1),
            CohortAcademicYearId = AcademicYearId.From(1),
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

    /// <summary>
    /// Builds an AcademicYear whose StartDate falls in <paramref name="startYear"/>.
    /// If <paramref name="termId"/> is provided, a matching AcademicTerm is added to the year.
    /// If <paramref name="academicYearId"/> is provided, that id is stamped onto the year; otherwise defaults to Id=1.
    /// </summary>
    private static AcademicYear CreateAcademicYearWithStartYear(
        int startYear,
        AcademicTermId? termId = null,
        AcademicYearId? academicYearId = null)
    {
        var ayId = academicYearId ?? AcademicYearId.From(1);
        var startDate = AcademicYearStartDate.From(new DateTime(startYear, 8, 1));
        var endDate = AcademicYearEndDate.From(new DateTime(startYear + 1, 7, 31));
        var academicYear = new AcademicYear(startDate, endDate);
        SetEntityProperty(academicYear, "Id", ayId);

        if (termId.HasValue)
        {
            var termStartDate = AcademicTermStartDate.From(new DateTime(startYear, 8, 1));
            var termEndDate = AcademicTermEndDate.From(new DateTime(startYear, 12, 31));
            var term = new AcademicTerm(TermNumber.From(1), ayId, termStartDate, termEndDate);
            SetEntityProperty(term, "Id", termId.Value);
            SetEntityProperty(academicYear, "_academicTerms", new List<AcademicTerm> { term });
        }
        else
        {
            SetEntityProperty(academicYear, "_academicTerms", new List<AcademicTerm>());
        }

        return academicYear;
    }

    /// <summary>
    /// Configures the AY repository mock with a sequence so:
    ///   1st call  → <paramref name="currentAY"/>  (GetAcademicYearByAcademicTermIdSpec)
    ///   2nd call  → <paramref name="cohortAY"/>   (GetAcademicYearByStartDateYearSpec)
    /// Also resets the fake transaction for the new test.
    /// </summary>
    private void SetupCohortAcademicYears(AcademicYear currentAY, AcademicYear cohortAY)
    {
        _fakeTransaction.Reset();
        _academicYearReadRepositoryMock
            .SetupSequence(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentAY)
            .ReturnsAsync(cohortAY);
    }

    /// <summary>
    /// Creates a curriculum with a distinct year/version/id carrying subjects for the given year level and term.
    /// </summary>
    private static Curriculum CreateCurriculumWithYear(
        CourseId courseId,
        int effectiveYear,
        string version,
        YearLevel yearLevel,
        TermNumber term,
        CurriculumId? curriculumId = null,
        int subjectCount = 2)
    {
        var id = curriculumId ?? CurriculumId.From(1);
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = courseId,
            EffectiveYear = Year.From(effectiveYear),
            Version = version,
            Description = $"Curriculum {version}"
        });
        SetEntityProperty(curriculum, "Id", id);
        SetEntityProperty(curriculum, "StatusId", CurriculumStatusEnum.Active);

        var subjects = new List<CurriculumSubject>();
        for (int i = 1; i <= subjectCount; i++)
        {
            var subject = new CurriculumSubject(id, SubjectId.From(i), yearLevel, term, false, null, null);
            SetEntityProperty(subject, "Id", CurriculumSubjectId.From(i));
            SetEntityProperty(subject, "IsActive", true);
            var stubSubject = new Subject(new SubjectForCreation
            {
                Code = SubjectCode.From($"SUBJ{i:D3}"),
                Title = $"Subject {i}",
                Units = 3m,
                Description = $"Description {i}",
                PreferRoomTypeId = RoomTypeId.From(1)
            });
            SetEntityProperty(stubSubject, "Id", SubjectId.From(i));
            SetEntityProperty(subject, "Subject", stubSubject);
            subjects.Add(subject);
        }
        SetEntityProperty(curriculum, "_curriculumSubjects", subjects);

        return curriculum;
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
