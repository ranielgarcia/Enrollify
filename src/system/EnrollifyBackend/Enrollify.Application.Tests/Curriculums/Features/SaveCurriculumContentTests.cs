using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Curriculums;
using Enrollify.Application.Curriculums.Features;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Curriculums.Features;

public class SaveCurriculumContentTests
{
    private readonly Mock<IReadRepository<Curriculum>> _curriculumReadRepositoryMock;
    private readonly Mock<IReadRepository<Subject>> _subjectReadRepositoryMock;
    private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
    private readonly SaveCurriculumContent.Handler _handler;

    public SaveCurriculumContentTests()
    {
        _curriculumReadRepositoryMock = new Mock<IReadRepository<Curriculum>>();
        _subjectReadRepositoryMock = new Mock<IReadRepository<Subject>>();
        _curriculumRepositoryMock = new Mock<ICurriculumRepository>();

        _handler = new SaveCurriculumContent.Handler(
            _curriculumReadRepositoryMock.Object,
            _subjectReadRepositoryMock.Object,
            _curriculumRepositoryMock.Object);
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
        Assert.Equal(2, curriculumSubject.YearLevel);
        Assert.Equal(2, curriculumSubject.TermNumber);
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
    public async Task Handle_PrerequisiteNotInCurriculum_ReturnsInvalidResult()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);

        var mainSubjectCode = SubjectCode.From("CS102");
        var prereqCode = SubjectCode.From("CS101");

        var mainSubject = CreateTestSubject(SubjectId.From(2), mainSubjectCode);
        var prereqSubject = CreateTestSubject(SubjectId.From(1), prereqCode);

        // Only add CS102 to the grid, but CS101 is listed as a prerequisite
        // CS101 is not in the grid itself, so it won't be added to the curriculum
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

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("CS101") && e.ErrorMessage.Contains("not found in the curriculum"));
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
            EffectiveYear = 2024,
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
        CurriculumSubjectId curriculumSubjectId)
    {
        var curriculumSubject = curriculum.AddSubject(subjectId, yearLevel, semester, isElective: false, electiveGroupName: null);
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
}
