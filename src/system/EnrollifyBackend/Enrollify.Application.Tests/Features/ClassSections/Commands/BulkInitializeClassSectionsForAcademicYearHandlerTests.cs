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
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.ClassSections.Commands;

public class BulkInitializeClassSectionsForAcademicYearHandlerTests
{
    private readonly Mock<IReadRepository<AcademicYear>> _academicYearReadRepositoryMock = new();
    private readonly Mock<IReadRepository<ClassSection>> _classSectionReadRepositoryMock = new();
    private readonly Mock<IReadRepository<CourseCurriculumAssignment>> _courseCurriculumAssignmentsReadRepositoryMock = new();
    private readonly Mock<IClassSectionRepository> _classSectionRepositoryMock = new();
    private readonly Mock<IClassSectionSubjectOfferingRepository> _classSectionSubjectOfferingRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly FakeBulkTransactionScope _fakeTransaction = new();
    private readonly FakeLogger<BulkInitializeClassSectionsForAcademicYear.Handler> _logger;
    private readonly BulkInitializeClassSectionsForAcademicYear.Handler _handler;

    public BulkInitializeClassSectionsForAcademicYearHandlerTests()
    {
        _logger = new FakeLogger<BulkInitializeClassSectionsForAcademicYear.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new BulkInitializeClassSectionsForAcademicYear.Handler(
            _academicYearReadRepositoryMock.Object,
            _classSectionReadRepositoryMock.Object,
            _courseCurriculumAssignmentsReadRepositoryMock.Object,
            _classSectionRepositoryMock.Object,
            _classSectionSubjectOfferingRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _logger);

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_fakeTransaction);
    }

    #region H5 Pre-validation — subjects check before opening transaction

    [Fact(DisplayName = "Course with no subjects for year level and term - returns error")]
    public async Task Handle_CourseWithNoSubjectsForYearLevelAndTerm_ReturnsError()
    {
        // Arrange
        var command = CreateCommand();
        var course = CreateCourse(command.requestPayload[0].courseId, "BSCS");
        var curriculum = CreateCurriculumWithNoSubjects(command.requestPayload[0].courseId);
        SetupAcademicYear(command.academicTermId);
        SetupCourseCurriculumAssignments(
            (command.requestPayload[0].courseId, course, curriculum));
        SetupExistingSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("No curriculum subjects found"));
    }

    [Fact(DisplayName = "Course with no subjects for year level and term - does not open transaction")]
    public async Task Handle_CourseWithNoSubjectsForYearLevelAndTerm_DoesNotOpenTransaction()
    {
        // Arrange
        var command = CreateCommand();
        var course = CreateCourse(command.requestPayload[0].courseId, "BSCS");
        var curriculum = CreateCurriculumWithNoSubjects(command.requestPayload[0].courseId);
        SetupAcademicYear(command.academicTermId);
        SetupCourseCurriculumAssignments(
            (command.requestPayload[0].courseId, course, curriculum));
        SetupExistingSections();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.False(_fakeTransaction.IsCommitted);
    }

    [Fact(DisplayName = "Multiple payloads - second course has no subjects - error message contains course code")]
    public async Task Handle_MultiplePayloads_SecondCourseHasNoSubjects_ReturnsErrorWithCourseCode()
    {
        // Arrange
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(10);
        var yearLevel = YearLevel.From(1);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            yearLevel,
            [new(courseId1, 1), new(courseId2, 1)]);

        var course1 = CreateCourse(courseId1, "BSCS");
        var course2 = CreateCourse(courseId2, "BSIT");
        var curriculumWithSubjects = CreateCurriculumWithSubjects(courseId1, yearLevel, TermNumber.From(1), subjectCount: 2);
        var curriculumNoSubjects = CreateCurriculumWithNoSubjects(courseId2);

        SetupAcademicYear(academicTermId);
        SetupCourseCurriculumAssignments(
            (courseId1, course1, curriculumWithSubjects),
            (courseId2, course2, curriculumNoSubjects));
        SetupExistingSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Contains("BSIT"));
    }

    #endregion

    #region Section code generation

    [Fact(DisplayName = "No existing sections - first section gets code 'A'")]
    public async Task Handle_NoExistingSections_FirstSectionGetsCodeA()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections(); // empty — no prior sections
        var capturedSections = CaptureCreatedSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(capturedSections);
        Assert.Equal(SectionCode.From('A'), capturedSections[0].SectionCode);
    }

    [Fact(DisplayName = "Existing section 'A' - next section gets code 'B'")]
    public async Task Handle_ExistingSectionCodeA_NextSectionGetsCodeB()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections(CreateExistingSection(command.requestPayload[0].courseId, SectionCode.From('A')));
        var capturedSections = CaptureCreatedSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(capturedSections);
        Assert.Equal(SectionCode.From('B'), capturedSections[0].SectionCode);
    }

    [Fact(DisplayName = "Three sections requested with no existing - creates codes A, B, C in order")]
    public async Task Handle_MultipleRequestedSections_CreatesSequentialSectionCodes()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 3);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        var capturedSections = CaptureCreatedSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, capturedSections.Count);
        Assert.Equal(SectionCode.From('A'), capturedSections[0].SectionCode);
        Assert.Equal(SectionCode.From('B'), capturedSections[1].SectionCode);
        Assert.Equal(SectionCode.From('C'), capturedSections[2].SectionCode);
    }

    #endregion

    #region Subject offerings creation

    [Fact(DisplayName = "Single section with three subjects - creates three subject offerings")]
    public async Task Handle_SingleSectionWithThreeSubjects_CreatesThreeOfferings()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        var course = CreateCourse(command.requestPayload[0].courseId, "BSCS");
        var curriculum = CreateCurriculumWithSubjects(
            command.requestPayload[0].courseId, command.yearLevel, TermNumber.From(1), subjectCount: 3);
        SetupAcademicYear(command.academicTermId);
        SetupCourseCurriculumAssignments((command.requestPayload[0].courseId, course, curriculum));
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _classSectionSubjectOfferingRepositoryMock.Verify(
            r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact(DisplayName = "Two sections each with two subjects - creates four subject offerings total")]
    public async Task Handle_TwoSectionsWithTwoSubjects_CreatesFourOfferings()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 2);
        var course = CreateCourse(command.requestPayload[0].courseId, "BSCS");
        var curriculum = CreateCurriculumWithSubjects(
            command.requestPayload[0].courseId, command.yearLevel, TermNumber.From(1), subjectCount: 2);
        SetupAcademicYear(command.academicTermId);
        SetupCourseCurriculumAssignments((command.requestPayload[0].courseId, course, curriculum));
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _classSectionSubjectOfferingRepositoryMock.Verify(
            r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()),
            Times.Exactly(4));
    }

    #endregion

    #region Section naming

    [Fact(DisplayName = "Section name follows {CourseCode}-{YearLevel}{SectionCode} pattern")]
    public async Task Handle_ValidCommand_SectionNameFollowsPattern()
    {
        // Arrange
        var command = CreateCommand(courseId: CourseId.From(1), yearLevel: YearLevel.From(2), numberOfSections: 1);
        var course = CreateCourse(command.requestPayload[0].courseId, "BSCS");
        var curriculum = CreateCurriculumWithSubjects(
            command.requestPayload[0].courseId, command.yearLevel, TermNumber.From(1), subjectCount: 1);
        SetupAcademicYear(command.academicTermId);
        SetupCourseCurriculumAssignments((command.requestPayload[0].courseId, course, curriculum));
        SetupExistingSections();
        var capturedSections = CaptureCreatedSections();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(capturedSections);
        Assert.Equal($"BSCS-{command.yearLevel}A", capturedSections[0].Name);
    }

    #endregion

    #region Transaction lifecycle

    [Fact(DisplayName = "All payloads succeed - returns success result")]
    public async Task Handle_AllPayloadsSucceed_ReturnsSuccess()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 2);
        SetupStandardSuccessPath(command);
        SetupExistingSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
    }

    [Fact(DisplayName = "All payloads succeed - commits transaction")]
    public async Task Handle_AllPayloadsSucceed_CommitsTransaction()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(_fakeTransaction.IsCommitted);
        Assert.False(_fakeTransaction.IsRolledBack);
    }

    [Fact(DisplayName = "Multiple payloads all succeed - opens transaction exactly once")]
    public async Task Handle_MultiplePayloads_AllSucceed_OpensTransactionOnce()
    {
        // Arrange
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(1);
        var yearLevel = YearLevel.From(1);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            yearLevel,
            [new(courseId1, 1), new(courseId2, 1)]);

        var course1 = CreateCourse(courseId1, "BSCS");
        var course2 = CreateCourse(courseId2, "BSIT");
        var curriculum1 = CreateCurriculumWithSubjects(courseId1, yearLevel, TermNumber.From(1), subjectCount: 2);
        var curriculum2 = CreateCurriculumWithSubjects(courseId2, yearLevel, TermNumber.From(1), subjectCount: 2);

        SetupAcademicYear(academicTermId);
        SetupCourseCurriculumAssignments(
            (courseId1, course1, curriculum1),
            (courseId2, course2, curriculum2));
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Section creation failure

    [Fact(DisplayName = "Section creation fails - returns error")]
    public async Task Handle_SectionCreationFails_ReturnsError()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionId>.Error("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Unable to create class section"));
    }

    [Fact(DisplayName = "Section creation fails - does not commit transaction")]
    public async Task Handle_SectionCreationFails_DoesNotCommitTransaction()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionId>.Error("Database error"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_fakeTransaction.IsCommitted);
    }

    #endregion

    #region Subject offering creation failure

    [Fact(DisplayName = "Subject offering creation fails - returns error")]
    public async Task Handle_SubjectOfferingCreationFails_ReturnsError()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        _classSectionSubjectOfferingRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionSubjectOfferingId>.Error("Failed to create offering"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Unable to create one or more subject offerings"));
    }

    [Fact(DisplayName = "Subject offering creation fails - does not commit transaction")]
    public async Task Handle_SubjectOfferingCreationFails_DoesNotCommitTransaction()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        _classSectionSubjectOfferingRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClassSectionSubjectOfferingId>.Error("Failed to create offering"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_fakeTransaction.IsCommitted);
    }

    #endregion

    #region Exception handling

    [Fact(DisplayName = "Exception during processing - returns unexpected error message")]
    public async Task Handle_ExceptionDuringProcessing_ReturnsUnexpectedError()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected database failure"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("unexpected error"));
    }

    [Fact(DisplayName = "Exception during processing - does not commit transaction")]
    public async Task Handle_ExceptionDuringProcessing_DoesNotCommitTransaction()
    {
        // Arrange
        var command = CreateCommand(numberOfSections: 1);
        SetupStandardSuccessPath(command);
        SetupExistingSections();
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected database failure"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(_fakeTransaction.IsCommitted);
    }

    #endregion

    #region Repository interaction

    [Fact(DisplayName = "Valid command - course-curriculum assignments fetched once regardless of payload count")]
    public async Task Handle_ValidCommand_FetchesCourseCurriculumAssignmentsOnce()
    {
        // Arrange
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(1);
        var yearLevel = YearLevel.From(1);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            yearLevel,
            [new(courseId1, 1), new(courseId2, 1)]);

        var course1 = CreateCourse(courseId1, "BSCS");
        var course2 = CreateCourse(courseId2, "BSIT");
        var curriculum1 = CreateCurriculumWithSubjects(courseId1, yearLevel, TermNumber.From(1), subjectCount: 2);
        var curriculum2 = CreateCurriculumWithSubjects(courseId2, yearLevel, TermNumber.From(1), subjectCount: 2);

        SetupAcademicYear(academicTermId);
        SetupCourseCurriculumAssignments(
            (courseId1, course1, curriculum1),
            (courseId2, course2, curriculum2));
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: one bulk ListAsync call, not one per payload
        _courseCurriculumAssignmentsReadRepositoryMock.Verify(
            r => r.ListAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Valid command - existing class sections fetched once in bulk (H1)")]
    public async Task Handle_ValidCommand_FetchesExistingSectionsOnce()
    {
        // Arrange
        var courseId1 = CourseId.From(1);
        var courseId2 = CourseId.From(2);
        var academicTermId = AcademicTermId.From(1);
        var yearLevel = YearLevel.From(1);
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId,
            yearLevel,
            [new(courseId1, 1), new(courseId2, 1)]);

        var course1 = CreateCourse(courseId1, "BSCS");
        var course2 = CreateCourse(courseId2, "BSIT");
        var curriculum1 = CreateCurriculumWithSubjects(courseId1, yearLevel, TermNumber.From(1), subjectCount: 2);
        var curriculum2 = CreateCurriculumWithSubjects(courseId2, yearLevel, TermNumber.From(1), subjectCount: 2);

        SetupAcademicYear(academicTermId);
        SetupCourseCurriculumAssignments(
            (courseId1, course1, curriculum1),
            (courseId2, course2, curriculum2));
        SetupExistingSections();
        SetupSuccessfulSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: one bulk ListAsync call, not one per payload (H1)
        _classSectionReadRepositoryMock.Verify(
            r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private BulkInitializeClassSectionsForAcademicYear.Command CreateCommand(
        AcademicTermId? academicTermId = null,
        YearLevel? yearLevel = null,
        CourseId? courseId = null,
        int numberOfSections = 1)
    {
        _fakeTransaction.Reset();
        return new BulkInitializeClassSectionsForAcademicYear.Command(
            academicTermId ?? AcademicTermId.From(1),
            yearLevel ?? YearLevel.From(1),
            [new(courseId ?? CourseId.From(1), numberOfSections)]);
    }

    /// <summary>
    /// Sets up mocks for the standard happy path (single payload, 2 subjects, default course/curriculum).
    /// Explicit <see cref="SetupExistingSections"/> and <see cref="CaptureCreatedSections"/> calls still required.
    /// </summary>
    private void SetupStandardSuccessPath(BulkInitializeClassSectionsForAcademicYear.Command command)
    {
        var courseId = command.requestPayload[0].courseId;
        var course = CreateCourse(courseId, "BSCS");
        var curriculum = CreateCurriculumWithSubjects(courseId, command.yearLevel, TermNumber.From(1), subjectCount: 2);
        SetupAcademicYear(command.academicTermId);
        SetupCourseCurriculumAssignments((courseId, course, curriculum));
        SetupSuccessfulSectionCreation();
        SetupSuccessfulSubjectOfferingCreation();
    }

    private void SetupAcademicYear(AcademicTermId academicTermId, TermNumber? termNumber = null)
    {
        var startDate = AcademicYearStartDate.From(new DateTime(2024, 8, 1));
        var endDate = AcademicYearEndDate.From(new DateTime(2025, 7, 31));
        var academicYear = new AcademicYear(startDate, endDate);
        SetEntityProperty(academicYear, "Id", AcademicYearId.From(1));

        var termStartDate = AcademicTermStartDate.From(new DateTime(2024, 8, 1));
        var termEndDate = AcademicTermEndDate.From(new DateTime(2024, 12, 31));
        var term = new AcademicTerm(termNumber ?? TermNumber.From(1), AcademicYearId.From(1), termStartDate, termEndDate);
        SetEntityProperty(term, "Id", academicTermId);

        SetEntityProperty(academicYear, "_academicTerms", new List<AcademicTerm> { term });

        _academicYearReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(academicYear);
    }

    private void SetupCourseCurriculumAssignments(
        params (CourseId courseId, Course course, Curriculum curriculum)[] items)
    {
        var assignments = items
            .Select(item =>
            {
                var assignment = new CourseCurriculumAssignment(
                    item.courseId, AcademicYearId.From(1), item.curriculum.Id);
                SetEntityProperty(assignment, "Course", item.course);
                SetEntityProperty(assignment, "Curriculum", item.curriculum);
                return assignment;
            })
            .ToList();

        _courseCurriculumAssignmentsReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<CourseCurriculumAssignment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignments);
    }

    private void SetupExistingSections(params ClassSection[] existingSections)
    {
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<ClassSection>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSections.ToList());
    }

    private void SetupSuccessfulSectionCreation()
    {
        var idCounter = 0;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Result.Success(ClassSectionId.From(++idCounter)));
    }

    private void SetupSuccessfulSubjectOfferingCreation()
    {
        var idCounter = 0;
        _classSectionSubjectOfferingRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSectionSubjectOffering>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Result.Success(ClassSectionSubjectOfferingId.From(++idCounter)));
    }

    /// <summary>
    /// Registers a Moq Callback so all created <see cref="ClassSection"/> objects are collected and returned.
    /// Also configures the <see cref="IClassSectionRepository.Create"/> mock to return success.
    /// </summary>
    private List<ClassSection> CaptureCreatedSections()
    {
        var captured = new List<ClassSection>();
        var idCounter = 0;
        _classSectionRepositoryMock
            .Setup(r => r.Create(It.IsAny<ClassSection>(), It.IsAny<CancellationToken>()))
            .Callback<ClassSection, CancellationToken>((cs, _) => captured.Add(cs))
            .ReturnsAsync(() => Result.Success(ClassSectionId.From(++idCounter)));
        return captured;
    }

    private static Course CreateCourse(CourseId courseId, string code)
    {
        var course = new Course(
            CourseCode.From(code),
            $"Course {code}",
            4,
            $"Description of {code}",
            CollegeId.From(1));
        SetEntityProperty(course, "Id", courseId);
        return course;
    }

    private static Curriculum CreateCurriculumWithSubjects(
        CourseId courseId,
        YearLevel yearLevel,
        TermNumber termNumber,
        int subjectCount)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = courseId,
            EffectiveYear = Year.From(2024),
            Version = "2024-A",
            Description = "Test curriculum"
        });
        SetEntityProperty(curriculum, "Id", CurriculumId.From(1));

        var subjects = Enumerable.Range(1, subjectCount)
            .Select(i =>
            {
                var subject = new CurriculumSubject(
                    CurriculumId.From(1),
                    SubjectId.From(i),
                    yearLevel,
                    termNumber,
                    false,
                    null,
                    null);
                SetEntityProperty(subject, "Id", CurriculumSubjectId.From(i));
                SetEntityProperty(subject, "IsActive", true);
                return subject;
            })
            .ToList();

        SetEntityProperty(curriculum, "_curriculumSubjects", subjects);
        return curriculum;
    }

    private static Curriculum CreateCurriculumWithNoSubjects(CourseId courseId)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = courseId,
            EffectiveYear = Year.From(2024),
            Version = "2024-A",
            Description = "Test curriculum"
        });
        SetEntityProperty(curriculum, "Id", CurriculumId.From(1));
        // No subjects added — GetSubjectsByYearAndTerm(...).Any() returns false
        return curriculum;
    }

    private static ClassSection CreateExistingSection(CourseId courseId, SectionCode sectionCode)
    {
        return new ClassSection(new ClassSectionForCreation
        {
            Name = $"ExistingSection-{sectionCode}",
            YearLevel = YearLevel.From(1),
            CourseId = courseId,
            CurriculumId = CurriculumId.From(1),
            AcademicTermId = AcademicTermId.From(1),
            AdviserId = null,
            SectionCode = sectionCode
        });
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

    /// <summary>
    /// Fake transaction scoped to this test class to avoid name collision with
    /// the outer-level FakeTransactionScope in <see cref="CreateClassSectionTests"/>.
    /// </summary>
    private sealed class FakeBulkTransactionScope : ITransactionScope
    {
        public bool IsCommitted { get; private set; }
        public bool IsRolledBack { get; private set; }

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

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void Reset()
        {
            IsCommitted = false;
            IsRolledBack = false;
        }
    }
}
