using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.Curriculums;
using Enrollify.Application.Features.Curriculums.Commands;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate.Models;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.Curriculums.Commands;

public class SaveCurriculumContentTests
{
    private readonly Mock<IReadRepository<Curriculum>> _curriculumReadRepositoryMock;
    private readonly Mock<IReadRepository<Subject>> _subjectReadRepositoryMock;
    private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITransactionScope> _transactionScopeMock;
    private readonly SaveCurriculumContent.Handler _handler;

    public SaveCurriculumContentTests()
    {
        _curriculumReadRepositoryMock = new Mock<IReadRepository<Curriculum>>();
        _subjectReadRepositoryMock = new Mock<IReadRepository<Subject>>();
        _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _transactionScopeMock = new Mock<ITransactionScope>();

        // Setup UnitOfWork to return transaction scope
        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionScopeMock.Object);

        _handler = new SaveCurriculumContent.Handler(
            _curriculumReadRepositoryMock.Object,
            _subjectReadRepositoryMock.Object,
            _curriculumRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_CurriculumNotFound_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(999);
        var command = new SaveCurriculumContent.Command(curriculumId, []);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curriculum?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("not found"));
    }

    [Fact(DisplayName = "Active curriculum cannot be modified - returns Forbidden")]
    public async Task Handle_ActiveCurriculum_ReturnsForbidden()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        // Set curriculum status to Active
        SetEntityProperty(curriculum, "StatusId", Enrollify.Core.Constants.CurriculumStatusEnum.Active);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = SubjectCode.From("CS101") }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
        Assert.Contains(result.Errors, e => e.Contains("Active curriculums cannot be modified"));
    }

    [Fact]
    public async Task Handle_SubjectCodeNotFound_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var missingCode = SubjectCode.From("MISSING101");

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = missingCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("MISSING101"));
    }

    [Fact]
    public async Task Handle_AddNewSubject_AddsSubjectToCurriculum()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _curriculumRepositoryMock.Verify(r => r.UpdateCurriculum(curriculum, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_EmptyGrid_RemovesAllSubjects()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectId = SubjectId.From(1);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(subjectId, subjectCode);

        // Add a subject to the curriculum first (with proper Id and IsActive set)
        AddSubjectToCurriculum(curriculum, subjectId, yearLevel: 1, semester: 1, CurriculumSubjectId.From(100));

        var emptyGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>();
        var command = new SaveCurriculumContent.Command(curriculumId, emptyGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(curriculum.CurriculumSubjects);
    }

    [Fact]
    public async Task Handle_UpdateSubjectYearAndSemester_UpdatesExistingSubject()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectId = SubjectId.From(1);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(subjectId, subjectCode);

        // Add subject at year 1, semester 1 (with proper Id and IsActive set)
        AddSubjectToCurriculum(curriculum, subjectId, yearLevel: 1, semester: 1, CurriculumSubjectId.From(100));

        // Move to year 2, semester 2
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [2] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subjectId);
        Assert.NotNull(curriculumSubject);
        Assert.Equal(YearLevel.From(2), curriculumSubject.YearLevel);
        Assert.Equal(TermNumber.From(2), curriculumSubject.TermNumber);
    }

    [Fact]
    public async Task Handle_MultipleSubjectsInGrid_AddsAllSubjects()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subject1 = CreateTestSubject(SubjectId.From(1), SubjectCode.From("CS101"));
        var subject2 = CreateTestSubject(SubjectId.From(2), SubjectCode.From("CS102"));
        var subject3 = CreateTestSubject(SubjectId.From(3), SubjectCode.From("MATH101"));

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = SubjectCode.From("CS101") },
                    new SaveCurriculumContent.SubjectInCurriculum { Code = SubjectCode.From("MATH101") }
                ],
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = SubjectCode.From("CS102") }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject1, subject2, subject3]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, curriculum.CurriculumSubjects.Count);
    }

    [Fact]
    public async Task Handle_UpdateCurriculumFails_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Invalid(new ValidationError("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_PrerequisiteNotInCurriculumGrid_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var mainSubjectCode = SubjectCode.From("CS102");
        var prereqCode = SubjectCode.From("CS101");

        var mainSubject = CreateTestSubject(SubjectId.From(2), mainSubjectCode);
        var prereqSubject = CreateTestSubject(SubjectId.From(1), prereqCode);

        // Only add CS102 to the grid, but CS101 is listed as a prerequisite
        // CS101 is not in the grid itself, so validation should fail
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = mainSubjectCode,
                        Prerequisites = [prereqCode]
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([mainSubject, prereqSubject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        var error = Assert.Single(result.ValidationErrors);
        Assert.Contains("CS101", error.ErrorMessage);
        Assert.Contains("not in the curriculum grid", error.ErrorMessage);
        Assert.Contains("CS102", error.ErrorMessage);
        Assert.Contains("Either add", error.ErrorMessage);
    }

    [Fact]
    public async Task Handle_PrerequisiteNotInGrid_ProducesSingleErrorListingAllDependents()
    {
        // Arrange
        // CC-DSTRUC is missing from the grid but is referenced as a prerequisite by CC-DBMS, CC-OS, and CS-ALGO
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var dstrucCode = SubjectCode.From("CC-DSTRUC");
        var dbmsCode = SubjectCode.From("CC-DBMS");
        var osCode = SubjectCode.From("CC-OS");
        var algoCode = SubjectCode.From("CS-ALGO");

        var dstrucSubject = CreateTestSubject(SubjectId.From(1), dstrucCode);
        var dbmsSubject = CreateTestSubject(SubjectId.From(2), dbmsCode);
        var osSubject = CreateTestSubject(SubjectId.From(3), osCode);
        var algoSubject = CreateTestSubject(SubjectId.From(4), algoCode);

        // CC-DSTRUC is NOT in the grid, but all three subjects list it as a prerequisite
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = dbmsCode,
                        Prerequisites = [dstrucCode]
                    },
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = osCode,
                        Prerequisites = [dstrucCode]
                    },
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = algoCode,
                        Prerequisites = [dstrucCode]
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([dstrucSubject, dbmsSubject, osSubject, algoSubject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);

        // Only one error should be produced for CC-DSTRUC, not three
        var missingPrereqErrors = result.ValidationErrors
            .Where(e => e.ErrorMessage.Contains("CC-DSTRUC") && e.ErrorMessage.Contains("not in the curriculum grid"))
            .ToList();
        Assert.Single(missingPrereqErrors);

        // The single error should mention all three dependent subjects
        var errorMessage = missingPrereqErrors[0].ErrorMessage;
        Assert.Contains("CC-DBMS", errorMessage);
        Assert.Contains("CC-OS", errorMessage);
        Assert.Contains("CS-ALGO", errorMessage);
        Assert.Contains("Either add", errorMessage);
    }

    [Fact]
    public async Task Handle_MultipleMissingPrerequisites_ProducesOneErrorPerMissingPrereq()
    {
        // Arrange
        // Two different prerequisites are missing from the grid
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var missingPrereq1 = SubjectCode.From("MATH101");
        var missingPrereq2 = SubjectCode.From("PHYS101");
        var mainSubjectCode = SubjectCode.From("ENGR201");

        var missingSubject1 = CreateTestSubject(SubjectId.From(1), missingPrereq1);
        var missingSubject2 = CreateTestSubject(SubjectId.From(2), missingPrereq2);
        var mainSubject = CreateTestSubject(SubjectId.From(3), mainSubjectCode);

        // ENGR201 references two missing prerequisites
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = mainSubjectCode,
                        Prerequisites = [missingPrereq1, missingPrereq2]
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([missingSubject1, missingSubject2, mainSubject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);

        // Should have exactly two errors, one per missing prerequisite
        var missingPrereqErrors = result.ValidationErrors
            .Where(e => e.ErrorMessage.Contains("not in the curriculum grid"))
            .ToList();
        Assert.Equal(2, missingPrereqErrors.Count);
        Assert.Contains(missingPrereqErrors, e => e.ErrorMessage.Contains("MATH101"));
        Assert.Contains(missingPrereqErrors, e => e.ErrorMessage.Contains("PHYS101"));
    }

    [Fact]
    public async Task Handle_SelfPrerequisite_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        // Subject has itself as a prerequisite
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = subjectCode,
                        Prerequisites = [subjectCode] // Self-reference
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("cannot be its own prerequisite"));
    }

    [Fact]
    public async Task Handle_DuplicatePrerequisite_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var prereqCode = SubjectCode.From("CS101");
        var mainSubjectCode = SubjectCode.From("CS102");

        var prereqSubject = CreateTestSubject(SubjectId.From(1), prereqCode);
        var mainSubject = CreateTestSubject(SubjectId.From(2), mainSubjectCode);

        // CS102 has CS101 listed twice as a prerequisite
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = prereqCode }
                ],
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = mainSubjectCode,
                        Prerequisites = [prereqCode, prereqCode] // Duplicate
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([prereqSubject, mainSubject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("Duplicate prerequisite"));
    }

    [Fact]
    public async Task Handle_PrerequisiteFromFutureYear_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var firstYearSubjectCode = SubjectCode.From("PROG01");
        var secondYearSubjectCode = SubjectCode.From("OOP");

        var firstYearSubject = CreateTestSubject(SubjectId.From(1), firstYearSubjectCode);
        var secondYearSubject = CreateTestSubject(SubjectId.From(2), secondYearSubjectCode);

        // PROG01 (1st year) has OOP (2nd year) as prerequisite - invalid
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = firstYearSubjectCode,
                        Prerequisites = [secondYearSubjectCode] // OOP is in year 2
                    }
                ]
            },
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = secondYearSubjectCode }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstYearSubject, secondYearSubject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("cannot be from a future year"));
    }

    [Fact]
    public async Task Handle_PrerequisiteFromSameSemester_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subjectACode = SubjectCode.From("CS101");
        var subjectBCode = SubjectCode.From("CS102");

        var subjectA = CreateTestSubject(SubjectId.From(1), subjectACode);
        var subjectB = CreateTestSubject(SubjectId.From(2), subjectBCode);

        // Both subjects in same year/semester, CS102 has CS101 as prerequisite - invalid
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = subjectACode },
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = subjectBCode,
                        Prerequisites = [subjectACode] // Same semester
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subjectA, subjectB]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("must be from an earlier term"));
    }

    [Fact]
    public async Task Handle_PrerequisiteFromFutureSemesterSameYear_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var firstSemSubjectCode = SubjectCode.From("CS101");
        var secondSemSubjectCode = SubjectCode.From("CS102");

        var firstSemSubject = CreateTestSubject(SubjectId.From(1), firstSemSubjectCode);
        var secondSemSubject = CreateTestSubject(SubjectId.From(2), secondSemSubjectCode);

        // CS101 (1st sem) has CS102 (2nd sem) as prerequisite - invalid
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = firstSemSubjectCode,
                        Prerequisites = [secondSemSubjectCode] // CS102 is in sem 2
                    }
                ],
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = secondSemSubjectCode }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstSemSubject, secondSemSubject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("must be from an earlier term"));
    }

    [Fact]
    public async Task Handle_PrerequisiteFromPreviousYear_Succeeds()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var firstYearSubjectCode = SubjectCode.From("CS101");
        var secondYearSubjectCode = SubjectCode.From("CS201");

        var firstYearSubject = CreateTestSubject(SubjectId.From(1), firstYearSubjectCode);
        var secondYearSubject = CreateTestSubject(SubjectId.From(2), secondYearSubjectCode);

        // CS201 (2nd year) has CS101 (1st year) as prerequisite - valid
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = firstYearSubjectCode }
                ]
            },
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = secondYearSubjectCode,
                        Prerequisites = [firstYearSubjectCode]
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstYearSubject, secondYearSubject]);

        var saveCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                saveCount++;
                if (saveCount == 1)
                {
                    var csId = 100;
                    foreach (var cs in curriculum.CurriculumSubjects)
                    {
                        SetEntityProperty(cs, "Id", CurriculumSubjectId.From(csId++));
                        SetEntityProperty(cs, "IsActive", true);
                    }
                }
                return Result.Success(curriculumId);
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_PrerequisiteFromPreviousSemesterSameYear_Succeeds()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var firstSemSubjectCode = SubjectCode.From("CS101");
        var secondSemSubjectCode = SubjectCode.From("CS102");

        var firstSemSubject = CreateTestSubject(SubjectId.From(1), firstSemSubjectCode);
        var secondSemSubject = CreateTestSubject(SubjectId.From(2), secondSemSubjectCode);

        // CS102 (2nd sem) has CS101 (1st sem) as prerequisite - valid
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = firstSemSubjectCode }
                ],
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = secondSemSubjectCode,
                        Prerequisites = [firstSemSubjectCode]
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstSemSubject, secondSemSubject]);

        var saveCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                saveCount++;
                if (saveCount == 1)
                {
                    var csId = 100;
                    foreach (var cs in curriculum.CurriculumSubjects)
                    {
                        SetEntityProperty(cs, "Id", CurriculumSubjectId.From(csId++));
                        SetEntityProperty(cs, "IsActive", true);
                    }
                }
                return Result.Success(curriculumId);
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidPrerequisite_AddsPrerequisiteRelationship()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var prereqCode = SubjectCode.From("CS101");
        var mainSubjectCode = SubjectCode.From("CS102");

        var prereqSubject = CreateTestSubject(SubjectId.From(1), prereqCode);
        var mainSubject = CreateTestSubject(SubjectId.From(2), mainSubjectCode);

        // Both subjects are in the grid, with CS102 having CS101 as prerequisite
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum { Code = prereqCode }
                ],
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = mainSubjectCode,
                        Prerequisites = [prereqCode]
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([prereqSubject, mainSubject]);

        var saveCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                saveCount++;
                if (saveCount == 1)
                {
                    // Simulate database assigning IDs to the newly added CurriculumSubjects
                    var csId = 100;
                    foreach (var cs in curriculum.CurriculumSubjects)
                    {
                        SetEntityProperty(cs, "Id", CurriculumSubjectId.From(csId++));
                        SetEntityProperty(cs, "IsActive", true);
                    }
                }
                return Result.Success(curriculumId);
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var cs102 = curriculum.GetCurriculumSubject(mainSubject.Id);
        Assert.NotNull(cs102);
        Assert.Single(cs102.Prerequisites);
    }

    [Fact]
    public async Task Handle_RemoveSubjectNotInGrid_RemovesFromCurriculum()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subjectToKeep = CreateTestSubject(SubjectId.From(1), SubjectCode.From("CS101"));
        var subjectToRemove = CreateTestSubject(SubjectId.From(2), SubjectCode.From("CS102"));

        // Add both subjects to curriculum initially (with proper Id and IsActive set)
        AddSubjectToCurriculum(curriculum, subjectToKeep.Id, yearLevel: 1, semester: 1, CurriculumSubjectId.From(100));
        AddSubjectToCurriculum(curriculum, subjectToRemove.Id, yearLevel: 1, semester: 2, CurriculumSubjectId.From(101));

        // Only keep CS101 in the grid
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = SubjectCode.From("CS101") }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subjectToKeep]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(curriculum.CurriculumSubjects);
        Assert.NotNull(curriculum.GetCurriculumSubject(subjectToKeep.Id));
        Assert.Null(curriculum.GetCurriculumSubject(subjectToRemove.Id));
    }

    private static Curriculum CreateTestCurriculum(CurriculumId curriculumId)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = CourseId.From(1),
            EffectiveYear = Year.From(2024),
            Version = "2024-A",
            Description = "Test Curriculum"
        });

        // Use reflection to set the Id and IsActive since they are normally set by EF Core
        SetEntityProperty(curriculum, "Id", curriculumId);
        SetEntityProperty(curriculum, "IsActive", true);
        SetEntityProperty(curriculum, "CreatedBy", Core.Aggregates.UserAggregate.UserId.From(1));

        return curriculum;
    }

    private static Subject CreateTestSubject(SubjectId subjectId, SubjectCode code)
    {
        var subject = new Subject(new SubjectForCreation
        {
            Code = code,
            Title = $"Subject {code.Value}",
            Units = 3,
            Description = $"Description for {code.Value}",
            PreferRoomTypeId = RoomTypeId.From(1)
        });

        // Use reflection to set the Id and IsActive since they are normally set by EF Core
        SetEntityProperty(subject, "Id", subjectId);
        SetEntityProperty(subject, "IsActive", true);

        return subject;
    }

    /// <summary>
    /// Adds a subject to the curriculum and simulates database-assigned Id and IsActive.
    /// </summary>
    private static CurriculumSubject AddSubjectToCurriculum(
        Curriculum curriculum,
        SubjectId subjectId,
        int yearLevel,
        int semester,
        CurriculumSubjectId curriculumSubjectId,
        decimal? subjectUnitsOverride = null)
    {
        var curriculumSubject = curriculum.AddSubject(subjectId, yearLevel, semester, isElective: false, electiveGroupName: null, subjectUnitsOverride: subjectUnitsOverride);
        if (curriculumSubject != null)
        {
            SetEntityProperty(curriculumSubject, "Id", curriculumSubjectId);
            SetEntityProperty(curriculumSubject, "IsActive", true);
        }
        return curriculumSubject!;
    }

    private static void SetEntityProperty<T>(T entity, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName);
        property?.SetValue(entity, value);
    }

    private static void SimulateDatabaseIdAssignment(Curriculum curriculum, int startingId = 100)
    {
        // Find curriculum subjects that don't have IDs set yet (newly added ones)
        var curriculumSubjects = curriculum.CurriculumSubjects.Where(cs =>
        {
            var idType = cs.Id.GetType();
            var valueField = idType.GetField("_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (valueField == null) return false;
            var value = valueField.GetValue(cs.Id);
            return value != null && Convert.ToInt32(value) == 0;
        }).ToList();

        var idCounter = startingId;
        foreach (var cs in curriculumSubjects)
        {
            SetEntityProperty(cs, "Id", CurriculumSubjectId.From(idCounter++));
            SetEntityProperty(cs, "IsActive", true);
        }
    }

    #region UnitsOverride Tests

    [Fact(DisplayName = "Adding new subject with UnitsOverride sets the override value")]
    public async Task Handle_AddNewSubjectWithUnitsOverride_SetsOverrideValue()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);
        var unitsOverride = 4.5m;

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = subjectCode,
                    UnitsOverride = unitsOverride
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Mock UpdateCurriculum to simulate database setting IDs
        var callCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId))
            .Callback<Curriculum, CancellationToken>((c, ct) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    SimulateDatabaseIdAssignment(c);
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subject.Id);
        Assert.NotNull(curriculumSubject);
        Assert.Equal(unitsOverride, curriculumSubject.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Adding new subject without UnitsOverride keeps it null")]
    public async Task Handle_AddNewSubjectWithoutUnitsOverride_KeepsItNull()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = subjectCode,
                    UnitsOverride = null
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Mock UpdateCurriculum to simulate database setting IDs
        var callCount = 0; _curriculumRepositoryMock .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>())) .ReturnsAsync(Result.Success(curriculumId)) .Callback<Curriculum, CancellationToken>((c, ct) => { callCount++; if (callCount == 1) { SimulateDatabaseIdAssignment(c); } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subject.Id);
        Assert.NotNull(curriculumSubject);
        Assert.Null(curriculumSubject.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Multiple subjects can have different UnitsOverride values")]
    public async Task Handle_MultipleSubjectsWithDifferentUnitsOverride_SetsEachCorrectly()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subject1 = CreateTestSubject(SubjectId.From(1), SubjectCode.From("CS101"));
        var subject2 = CreateTestSubject(SubjectId.From(2), SubjectCode.From("CS102"));
        var subject3 = CreateTestSubject(SubjectId.From(3), SubjectCode.From("MATH101"));

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = SubjectCode.From("CS101"),
                        UnitsOverride = 2.0m
                    },
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = SubjectCode.From("MATH101"),
                        UnitsOverride = null
                    }
                ],
                [2] = [
                    new SaveCurriculumContent.SubjectInCurriculum
                    {
                        Code = SubjectCode.From("CS102"),
                        UnitsOverride = 4.5m
                    }
                ]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject1, subject2, subject3]);

        // Mock UpdateCurriculum to simulate database setting IDs
        var callCount = 0; _curriculumRepositoryMock .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>())) .ReturnsAsync(Result.Success(curriculumId)) .Callback<Curriculum, CancellationToken>((c, ct) => { callCount++; if (callCount == 1) { SimulateDatabaseIdAssignment(c); } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var curriculumSubject1 = curriculum.GetCurriculumSubject(subject1.Id);
        Assert.NotNull(curriculumSubject1);
        Assert.Equal(2.0m, curriculumSubject1.SubjectUnitsOverride);

        var curriculumSubject2 = curriculum.GetCurriculumSubject(subject2.Id);
        Assert.NotNull(curriculumSubject2);
        Assert.Equal(4.5m, curriculumSubject2.SubjectUnitsOverride);

        var curriculumSubject3 = curriculum.GetCurriculumSubject(subject3.Id);
        Assert.NotNull(curriculumSubject3);
        Assert.Null(curriculumSubject3.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Updating existing subject preserves its UnitsOverride value")]
    public async Task Handle_UpdateExistingSubject_PreservesUnitsOverride()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectId = SubjectId.From(1);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(subjectId, subjectCode);
        var originalUnitsOverride = 3.5m;

        // Add subject with UnitsOverride at year 1, semester 1
        AddSubjectToCurriculum(curriculum, subjectId, yearLevel: 1, semester: 1, CurriculumSubjectId.From(100), subjectUnitsOverride: originalUnitsOverride);

        // Move to year 2, semester 2 (note: we're NOT providing UnitsOverride in the command)
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [2] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subjectId);
        Assert.NotNull(curriculumSubject);
        Assert.Equal(YearLevel.From(2), curriculumSubject.YearLevel);
        Assert.Equal(TermNumber.From(2), curriculumSubject.TermNumber);
        // The UnitsOverride should be updated to null since the command didn't specify one
        Assert.Null(curriculumSubject.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Updating existing subject can change UnitsOverride value")]
    public async Task Handle_UpdateExistingSubjectWithNewUnitsOverride_UpdatesValue()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectId = SubjectId.From(1);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(subjectId, subjectCode);
        var originalUnitsOverride = 3.0m;
        var newUnitsOverride = 4.5m;

        // Add subject with UnitsOverride at year 1, semester 1
        AddSubjectToCurriculum(curriculum, subjectId, yearLevel: 1, semester: 1, CurriculumSubjectId.From(100), subjectUnitsOverride: originalUnitsOverride);

        // Update with new UnitsOverride
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [2] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = subjectCode,
                    UnitsOverride = newUnitsOverride
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subjectId);
        Assert.NotNull(curriculumSubject);
        Assert.Equal(YearLevel.From(2), curriculumSubject.YearLevel);
        Assert.Equal(TermNumber.From(2), curriculumSubject.TermNumber);
        Assert.Equal(newUnitsOverride, curriculumSubject.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Updating existing subject can clear UnitsOverride by setting to null")]
    public async Task Handle_UpdateExistingSubjectClearUnitsOverride_SetsToNull()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectId = SubjectId.From(1);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(subjectId, subjectCode);
        var originalUnitsOverride = 3.5m;

        // Add subject with UnitsOverride at year 1, semester 1
        AddSubjectToCurriculum(curriculum, subjectId, yearLevel: 1, semester: 1, CurriculumSubjectId.From(100), subjectUnitsOverride: originalUnitsOverride);

        // Explicitly clear UnitsOverride by setting to null
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = subjectCode,
                    UnitsOverride = null
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subjectId);
        Assert.NotNull(curriculumSubject);
        Assert.Null(curriculumSubject.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Adding subject with zero UnitsOverride is allowed")]
    public async Task Handle_AddSubjectWithZeroUnitsOverride_AllowsZeroValue()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);
        var zeroUnitsOverride = 0m;

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = subjectCode,
                    UnitsOverride = zeroUnitsOverride
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Mock UpdateCurriculum to simulate database setting IDs
        var callCount = 0; _curriculumRepositoryMock .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>())) .ReturnsAsync(Result.Success(curriculumId)) .Callback<Curriculum, CancellationToken>((c, ct) => { callCount++; if (callCount == 1) { SimulateDatabaseIdAssignment(c); } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var curriculumSubject = curriculum.GetCurriculumSubject(subject.Id);
        Assert.NotNull(curriculumSubject);
        Assert.Equal(zeroUnitsOverride, curriculumSubject.SubjectUnitsOverride);
    }

    [Fact(DisplayName = "Subject with UnitsOverride can be used as prerequisite")]
    public async Task Handle_SubjectWithUnitsOverrideAsPrerequisite_WorksCorrectly()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subject1 = CreateTestSubject(SubjectId.From(1), SubjectCode.From("CS101"));
        var subject2 = CreateTestSubject(SubjectId.From(2), SubjectCode.From("CS201"));

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = SubjectCode.From("CS101"),
                    UnitsOverride = 2.5m,
                    Prerequisites = []
                }]
            },
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = SubjectCode.From("CS201"),
                    UnitsOverride = 3.0m,
                    Prerequisites = [SubjectCode.From("CS101")]
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject1, subject2]);

        // Mock UpdateCurriculum to simulate database setting IDs
        var callCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId))
            .Callback<Curriculum, CancellationToken>((c, ct) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    SimulateDatabaseIdAssignment(c);
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - verify the command succeeds and UnitsOverride values are set correctly
        Assert.True(result.IsSuccess);

        var curriculumSubject1 = curriculum.GetCurriculumSubject(subject1.Id);
        Assert.NotNull(curriculumSubject1);
        Assert.Equal(2.5m, curriculumSubject1.SubjectUnitsOverride);

        var curriculumSubject2 = curriculum.GetCurriculumSubject(subject2.Id);
        Assert.NotNull(curriculumSubject2);
        Assert.Equal(3.0m, curriculumSubject2.SubjectUnitsOverride);

        // Verify the command was called twice (once for subjects, once for prerequisites)
        _curriculumRepositoryMock.Verify(r => r.UpdateCurriculum(curriculum, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region Transaction Tests

    [Fact(DisplayName = "Successful request commits transaction")]
    public async Task Handle_SuccessfulRequest_CommitsTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        var callCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId))
            .Callback<Curriculum, CancellationToken>((c, ct) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    SimulateDatabaseIdAssignment(c);
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "First UpdateCurriculum failure rolls back transaction")]
    public async Task Handle_FirstUpdateCurriculumFails_RollsBackTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Simulate first UpdateCurriculum failure
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Invalid(new ValidationError("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Second UpdateCurriculum failure rolls back transaction")]
    public async Task Handle_SecondUpdateCurriculumFails_RollsBackTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Simulate first UpdateCurriculum success, second failure
        var callCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    SimulateDatabaseIdAssignment(curriculum);
                    return Result.Success(curriculumId);
                }
                return Result.Invalid(new ValidationError("Database error on second update"));
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Missing subject code causes validation error and rollback")]
    public async Task Handle_MissingSubjectCode_RollsBackTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var missingCode = SubjectCode.From("MISSING101");

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = missingCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        // Validation errors happen before transaction starts, so neither rollback nor commit should be called
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Prerequisite validation error does not begin transaction")]
    public async Task Handle_PrerequisiteValidationError_RollsBackTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        // Subject has itself as a prerequisite (invalid)
        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = subjectCode,
                    Prerequisites = [subjectCode] // Self-prerequisite (invalid)
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("cannot be its own prerequisite"));
        // Validation errors happen before transaction starts, so neither rollback nor commit should be called
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Curriculum not found does not begin transaction")]
    public async Task Handle_CurriculumNotFound_DoesNotBeginTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(999);
        var command = new SaveCurriculumContent.Command(curriculumId, []);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curriculum?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        // Transaction should never be started for early validation failures
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Exception during processing causes automatic rollback on dispose")]
    public async Task Handle_ExceptionDuringProcessing_RollsBackOnDispose()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        var subjectCode = SubjectCode.From("CS101");
        var subject = CreateTestSubject(SubjectId.From(1), subjectCode);

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum { Code = subjectCode }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject]);

        // Simulate an unexpected exception in UpdateCurriculum
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        // Transaction should be disposed (which triggers automatic rollback if not committed)
        _transactionScopeMock.Verify(t => t.DisposeAsync(), Times.Once);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Multiple subjects with prerequisites commits successfully")]
    public async Task Handle_MultipleSubjectsWithPrerequisites_CommitsTransaction()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var subject1 = CreateTestSubject(SubjectId.From(1), SubjectCode.From("CS101"));
        var subject2 = CreateTestSubject(SubjectId.From(2), SubjectCode.From("CS201"));
        var subject3 = CreateTestSubject(SubjectId.From(3), SubjectCode.From("CS301"));

        var subjectsGrid = new Dictionary<int, Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>>
        {
            [1] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = SubjectCode.From("CS101"),
                    Prerequisites = []
                }]
            },
            [2] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = SubjectCode.From("CS201"),
                    Prerequisites = [SubjectCode.From("CS101")]
                }]
            },
            [3] = new Dictionary<int, SaveCurriculumContent.SubjectInCurriculum[]>
            {
                [1] = [new SaveCurriculumContent.SubjectInCurriculum
                {
                    Code = SubjectCode.From("CS301"),
                    Prerequisites = [SubjectCode.From("CS201")]
                }]
            }
        };

        var command = new SaveCurriculumContent.Command(curriculumId, subjectsGrid);

        _curriculumReadRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Curriculum>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Subject>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([subject1, subject2, subject3]);

        var callCount = 0;
        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId))
            .Callback<Curriculum, CancellationToken>((c, ct) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    SimulateDatabaseIdAssignment(c);
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _transactionScopeMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _transactionScopeMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _curriculumRepositoryMock.Verify(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion
}

