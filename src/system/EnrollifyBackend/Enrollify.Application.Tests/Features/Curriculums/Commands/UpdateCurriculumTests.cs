using Ardalis.Result;
using Enrollify.Application.Features.Curriculums;
using Enrollify.Application.Features.Curriculums.Commands;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Moq;
using System.Reflection;

namespace Enrollify.Application.Tests.Features.Curriculums.Commands;

public class UpdateCurriculumTests
{
    private readonly Mock<ICurriculumRepository> _curriculumRepositoryMock;
    private readonly Mock<IReadRepository<Curriculum>> _curriculumReadRepositoryMock;
    private readonly Mock<IReadRepository<Course>> _courseReadRepositoryMock;
    private readonly UpdateCurriculum.Handler _handler;

    public UpdateCurriculumTests()
    {
        _curriculumRepositoryMock = new Mock<ICurriculumRepository>();
        _curriculumReadRepositoryMock = new Mock<IReadRepository<Curriculum>>();
        _courseReadRepositoryMock = new Mock<IReadRepository<Course>>();

        _handler = new UpdateCurriculum.Handler(
            _curriculumRepositoryMock.Object,
            _curriculumReadRepositoryMock.Object,
            _courseReadRepositoryMock.Object);
    }

    [Fact(DisplayName = "Course not found returns NotFound")]
    public async Task Handle_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = CourseId.From(999);
        var command = new UpdateCurriculum.Command(
            CurriculumId.From(1),
            courseId,
            2024,
            "2024-A",
            "Test Description");

        _courseReadRepositoryMock
            .Setup(r => r.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Contains("Course", result.Errors.First());
    }

    [Fact(DisplayName = "Curriculum not found returns NotFound")]
    public async Task Handle_CurriculumNotFound_ReturnsNotFound()
    {
        // Arrange
        var curriculumId = CurriculumId.From(999);
        var command = new UpdateCurriculum.Command(
            curriculumId,
            CourseId.From(1),
            2024,
            "2024-A",
            "Test Description");

        var course = CreateTestCourse();
        _courseReadRepositoryMock
            .Setup(r => r.GetByIdAsync(command.courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _curriculumReadRepositoryMock
            .Setup(r => r.GetByIdAsync(curriculumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Curriculum?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Contains("Curriculum", result.Errors.First());
    }

    [Fact(DisplayName = "Active curriculum cannot be modified - returns Forbidden")]
    public async Task Handle_ActiveCurriculum_ReturnsForbidden()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        
        // Set curriculum status to Active
        SetEntityProperty(curriculum, "StatusId", CurriculumStatusEnum.Active);

        var command = new UpdateCurriculum.Command(
            curriculumId,
            CourseId.From(1),
            2024,
            "2024-B",
            "Updated Description");

        var course = CreateTestCourse();
        _courseReadRepositoryMock
            .Setup(r => r.GetByIdAsync(command.courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _curriculumReadRepositoryMock
            .Setup(r => r.GetByIdAsync(curriculumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
        Assert.Contains("Active curriculums cannot be modified", result.Errors.First());
    }

    [Fact(DisplayName = "Draft curriculum can be updated successfully")]
    public async Task Handle_DraftCurriculum_SuccessfullyUpdates()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        
        // Set curriculum status to Draft (default)
        SetEntityProperty(curriculum, "StatusId", CurriculumStatusEnum.Draft);

        var command = new UpdateCurriculum.Command(
            curriculumId,
            CourseId.From(1),
            2025,
            "2025-A",
            "Updated Description");

        var course = CreateTestCourse();
        _courseReadRepositoryMock
            .Setup(r => r.GetByIdAsync(command.courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _curriculumReadRepositoryMock
            .Setup(r => r.GetByIdAsync(curriculumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(curriculumId, result.Value);
        _curriculumRepositoryMock.Verify(
            r => r.UpdateCurriculum(curriculum, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact(DisplayName = "Archived curriculum can be updated successfully")]
    public async Task Handle_ArchivedCurriculum_SuccessfullyUpdates()
    {
        // Arrange
        var curriculumId = CurriculumId.From(1);
        var curriculum = CreateTestCurriculum(curriculumId);
        
        // Set curriculum status to Archived
        SetEntityProperty(curriculum, "StatusId", CurriculumStatusEnum.Archived);

        var command = new UpdateCurriculum.Command(
            curriculumId,
            CourseId.From(1),
            2023,
            "2023-LEGACY",
            "Archived curriculum update");

        var course = CreateTestCourse();
        _courseReadRepositoryMock
            .Setup(r => r.GetByIdAsync(command.courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        _curriculumReadRepositoryMock
            .Setup(r => r.GetByIdAsync(curriculumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(curriculum);

        _curriculumRepositoryMock
            .Setup(r => r.UpdateCurriculum(It.IsAny<Curriculum>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(curriculumId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(curriculumId, result.Value);
        _curriculumRepositoryMock.Verify(
            r => r.UpdateCurriculum(curriculum, It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    #region Helper Methods

    private static Curriculum CreateTestCurriculum(CurriculumId curriculumId)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = CourseId.From(1),
            EffectiveYear = Year.From(2024),
            Version = "2024-A",
            Description = "Test Curriculum"
        });

        SetEntityProperty(curriculum, "Id", curriculumId);
        SetEntityProperty(curriculum, "IsActive", true);
        SetEntityProperty(curriculum, "CreatedBy", Core.Aggregates.UserAggregate.UserId.From(1));

        return curriculum;
    }

    private static Course CreateTestCourse()
    {
        var course = new Course(
            CourseCode.From("BSCS"),
            "Bachelor of Science in Computer Science",
            4,
            "Test Course Description",
            Core.Aggregates.CollegeAggregate.CollegeId.From(1));

        SetEntityProperty(course, "Id", CourseId.From(1));
        SetEntityProperty(course, "IsActive", true);

        return course;
    }

    private static void SetEntityProperty<T>(T entity, string propertyName, object value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, value);
        }
        else
        {
            var field = typeof(T).GetField($"<{propertyName}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(entity, value);
            }
        }
    }

    #endregion
}
