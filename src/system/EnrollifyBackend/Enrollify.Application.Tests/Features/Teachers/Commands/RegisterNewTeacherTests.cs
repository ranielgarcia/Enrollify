using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.Subjects.Models;
using Enrollify.Application.Features.Teachers;
using Enrollify.Application.Features.Teachers.Commands;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Application.Features.Teachers.Storage;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.ValueObjects.Storage;
using Enrollify.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.Teachers.Commands;

public class RegisterNewTeacherTests
{
    private readonly Mock<ITeacherRepository> _teacherRepositoryMock = new();
    private readonly Mock<ITeacherPhotoStorageService> _photoStorageMock = new();
    private readonly Mock<IReadRepository<Teacher>> _readRepositoryMock = new();
    private readonly Mock<IReadRepository<Subject>> _subjectReadRepositoryMock = new();
    private readonly RegisterNewTeacher.Handler _handler;
    private readonly FakeLogger<RegisterNewTeacher.Handler> _logger;

    public RegisterNewTeacherTests()
    {
        _logger = new FakeLogger<RegisterNewTeacher.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new RegisterNewTeacher.Handler(
            _teacherRepositoryMock.Object,
            _photoStorageMock.Object,
            _readRepositoryMock.Object,
            _subjectReadRepositoryMock.Object,
            _logger);
    }

    [Fact]
    public async Task Handle_NoSubjects_ReturnsInvalidResult()
    {
        // Arrange
        var command = new RegisterNewTeacher.Command(CreateRegistration([]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.ToLower().Contains("subject"));
    }

    [Fact]
    public async Task Handle_ValidSubjectCodes_AddsAllSubjectsToTeacher()
    {
        // Arrange
        var code1 = SubjectCode.From("CS101");
        var code2 = SubjectCode.From("MATH101");

        SetupSubjectRepo([
            new() { Id = SubjectId.From(1), Code = code1 },
            new() { Id = SubjectId.From(2), Code = code2 }
        ]);

        Teacher? capturedTeacher = null;
        _teacherRepositoryMock
            .Setup(r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .Callback<Teacher, CancellationToken>((t, _) => capturedTeacher = t)
            .ReturnsAsync(Result.Success(TeacherId.From(1)));

        var command = new RegisterNewTeacher.Command(CreateRegistration([code1, code2]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedTeacher);
        Assert.Equal(2, capturedTeacher.Subjects.Count);
        Assert.Contains(capturedTeacher.Subjects, s => s.SubjectId == SubjectId.From(1));
        Assert.Contains(capturedTeacher.Subjects, s => s.SubjectId == SubjectId.From(2));
    }

    [Fact]
    public async Task Handle_SomeSubjectCodesNotFound_ReturnsInvalidResult()
    {
        // Arrange
        var existingCode = SubjectCode.From("CS101");
        var missingCode = SubjectCode.From("CS999");

        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = existingCode }]);

        var command = new RegisterNewTeacher.Command(
            CreateRegistration([existingCode, missingCode]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("CS999"));
    }

    [Fact]
    public async Task Handle_AllSubjectCodesNotFound_ReturnsInvalidResult()
    {
        // Arrange
        SetupSubjectRepo([]);

        var command = new RegisterNewTeacher.Command(
            CreateRegistration([SubjectCode.From("MISS1"), SubjectCode.From("MISS2")]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        var error = Assert.Single(result.ValidationErrors);
        Assert.Contains("MISS1", error.ErrorMessage);
        Assert.Contains("MISS2", error.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WithoutPhoto_DoesNotUploadPhoto()
    {
        // Arrange
        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        _teacherRepositoryMock
            .Setup(r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TeacherId.From(1)));

        var command = new RegisterNewTeacher.Command(CreateRegistration([code1]), Photo: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _photoStorageMock.Verify(
            s => s.UploadPhotoAsync(
                It.IsAny<TeacherIdentifier>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithPhoto_UploadsPhotoAndUpdatesTeacher()
    {
        // Arrange
        var teacherId = TeacherId.From(1);
        var reloadedTeacher = CreateTestTeacher(teacherId);

        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        _teacherRepositoryMock
            .Setup(r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(teacherId));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloadedTeacher);
        _photoStorageMock
            .Setup(s => s.UploadPhotoAsync(
                It.IsAny<TeacherIdentifier>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileName.From("abc_profile.jpg"));
        _teacherRepositoryMock
            .Setup(r => r.Update(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(teacherId));

        var photo = new RegisterNewTeacher.TeacherPhoto([1, 2, 3], "image/jpeg", "photo.jpg");
        var command = new RegisterNewTeacher.Command(CreateRegistration([code1]), Photo: photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _photoStorageMock.Verify(
            s => s.UploadPhotoAsync(
                It.IsAny<TeacherIdentifier>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _teacherRepositoryMock.Verify(r => r.Update(reloadedTeacher, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullSubjects_ReturnsInvalidResult()
    {
        // Arrange
        var command = new RegisterNewTeacher.Command(
            new TeacherForRegistration
            {
                FirstName = "John",
                MiddleName = "M",
                LastName = "Doe",
                TeacherIdentifier = TeacherIdentifier.From("T001"),
                Email = TeacherEmail.From("john@test.com"),
                PhoneNumber = TeacherPhoneNumber.From("09123456789"),
                DepartmentId = DepartmentId.From(1),
                Subjects = null!
            }, Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.ToLower().Contains("subject"));
    }

    [Fact]
    public async Task Handle_NoSubjects_DoesNotCallCreateRepository()
    {
        // Arrange
        var command = new RegisterNewTeacher.Command(CreateRegistration([]), Photo: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _teacherRepositoryMock.Verify(
            r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PhotoProvided_TeacherNotFoundAfterCreate_StillReturnsSuccess()
    {
        // Arrange
        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        _teacherRepositoryMock
            .Setup(r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TeacherId.From(1)));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        var photo = new RegisterNewTeacher.TeacherPhoto([1, 2, 3], "image/jpeg", "photo.jpg");
        var command = new RegisterNewTeacher.Command(CreateRegistration([code1]), Photo: photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: photo step is best-effort; the successful Create result is still returned
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_PhotoProvided_TeacherNotFoundAfterCreate_DoesNotUploadPhoto()
    {
        // Arrange
        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        _teacherRepositoryMock
            .Setup(r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TeacherId.From(1)));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        var photo = new RegisterNewTeacher.TeacherPhoto([1, 2, 3], "image/jpeg", "photo.jpg");
        var command = new RegisterNewTeacher.Command(CreateRegistration([code1]), Photo: photo);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _photoStorageMock.Verify(
            s => s.UploadPhotoAsync(
                It.IsAny<TeacherIdentifier>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PhotoProvided_TeacherNotFoundAfterCreate_LogsError()
    {
        // Arrange
        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        _teacherRepositoryMock
            .Setup(r => r.Create(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TeacherId.From(1)));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        var photo = new RegisterNewTeacher.TeacherPhoto([1, 2, 3], "image/jpeg", "photo.jpg");
        var command = new RegisterNewTeacher.Command(CreateRegistration([code1]), Photo: photo);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var logs = _logger.Collector.GetSnapshot();
        Assert.Contains(logs, l => l.Level == LogLevel.Error);
    }

    private void SetupSubjectRepo(List<MinimumSubjectProjection> projections)
    {
        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<Subject, MinimumSubjectProjection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(projections);
    }

    private static TeacherForRegistration CreateRegistration(SubjectCode[] subjects) =>
        new()
        {
            FirstName = "John",
            MiddleName = "M",
            LastName = "Doe",
            TeacherIdentifier = TeacherIdentifier.From("T001"),
            Email = TeacherEmail.From("john@test.com"),
            PhoneNumber = TeacherPhoneNumber.From("09123456789"),
            DepartmentId = DepartmentId.From(1),
            Subjects = subjects
        };

    private static Teacher CreateTestTeacher(TeacherId teacherId)
    {
        var teacher = new Teacher(
            "John", "M", "Doe",
            TeacherIdentifier.From("T001"),
            TeacherEmail.From("john@test.com"),
            TeacherPhoneNumber.From("09123456789"),
            DepartmentId.From(1));
        SetEntityProperty(teacher, "Id", teacherId);
        SetEntityProperty(teacher, "IsActive", true);
        return teacher;
    }

    private static void SetEntityProperty<T>(T entity, string propertyName, object value) where T : class =>
        typeof(T).GetProperty(propertyName)?.SetValue(entity, value);
}
