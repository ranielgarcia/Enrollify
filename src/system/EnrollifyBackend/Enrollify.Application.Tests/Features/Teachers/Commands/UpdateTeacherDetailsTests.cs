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
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.Teachers.Commands;

public class UpdateTeacherDetailsTests
{
    private readonly Mock<ITeacherRepository> _teacherRepositoryMock = new();
    private readonly Mock<ITeacherPhotoStorageService> _photoStorageMock = new();
    private readonly Mock<IReadRepository<Teacher>> _readRepositoryMock = new();
    private readonly Mock<IReadRepository<Subject>> _subjectReadRepositoryMock = new();
    private readonly UpdateTeacherDetails.Handler _handler;

    public UpdateTeacherDetailsTests()
    {
        var logger = new FakeLogger<UpdateTeacherDetails.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new UpdateTeacherDetails.Handler(
            _teacherRepositoryMock.Object,
            _photoStorageMock.Object,
            _readRepositoryMock.Object,
            _subjectReadRepositoryMock.Object,
            logger);
    }

    [Fact]
    public async Task Handle_TeacherNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Teacher?)null);

        var command = new UpdateTeacherDetails.Command(CreateUpdate(TeacherId.From(99), []), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_SubjectCodeNotFound_ReturnsInvalidResult()
    {
        // Arrange
        var teacher = CreateTestTeacher(TeacherId.From(1));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        SetupSubjectRepo([]);

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [SubjectCode.From("MISSING")]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("MISSING"));
    }

    [Fact]
    public async Task Handle_NoExistingSubjects_AddsNewSubjects()
    {
        // Arrange
        var teacher = CreateTestTeacher(TeacherId.From(1));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code1 = SubjectCode.From("CS101");
        var code2 = SubjectCode.From("CS102");
        SetupSubjectRepo([
            new() { Id = SubjectId.From(1), Code = code1 },
            new() { Id = SubjectId.From(2), Code = code2 }
        ]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(CreateUpdate(TeacherId.From(1), [code1, code2]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, teacher.Subjects.Count);
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(1));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(2));
    }

    [Fact]
    public async Task Handle_ActiveSubjectNotInNewList_IsRemovedFromTeacher()
    {
        // Arrange
        var teacher = CreateTestTeacher(TeacherId.From(1));
        AddActiveSubject(teacher, SubjectId.From(1));

        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code2 = SubjectCode.From("CS102");
        SetupSubjectRepo([new() { Id = SubjectId.From(2), Code = code2 }]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [code2]), Photo: null); // S1 omitted from new list

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(teacher.Subjects, s => s.SubjectId == SubjectId.From(1));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(2));
    }

    /// <summary>
    /// Verifies the bug fix: an inactive (soft-deleted) subject that the caller re-includes
    /// in the update list must be re-added, because GetActiveSubjects() correctly excludes
    /// it from existingSubjectIds and therefore it surfaces as a new subject to add.
    /// </summary>
    [Fact]
    public async Task Handle_InactiveSubjectIncludedInNewList_IsReAdded()
    {
        // Arrange: teacher already has an inactive record for SubjectId(1)
        var teacher = CreateTestTeacher(TeacherId.From(1));
        AddInactiveSubject(teacher, SubjectId.From(1));
        Assert.Single(teacher.Subjects); // sanity: one inactive subject before update

        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [code1]), Photo: null); // re-request the inactive subject

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: AddSubject was called because GetActiveSubjects() excluded the inactive record
        // from existingSubjectIds, so SubjectId(1) appeared in newSubjectsToAdd.
        Assert.True(result.IsSuccess);
        Assert.Equal(2, teacher.Subjects.Count); // original inactive(1) + newly added(1)
    }

    /// <summary>
    /// Verifies the bug fix: an inactive subject that is NOT in the new list must NOT be
    /// pushed through RemoveSubject, preserving the soft-deleted audit record in the collection.
    /// </summary>
    [Fact]
    public async Task Handle_InactiveSubjectNotInNewList_IsPreservedInSubjectsCollection()
    {
        // Arrange: teacher has an inactive record for SubjectId(2), not requested in new list
        var teacher = CreateTestTeacher(TeacherId.From(1));
        AddInactiveSubject(teacher, SubjectId.From(2));

        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code1 = SubjectCode.From("CS101");
        SetupSubjectRepo([new() { Id = SubjectId.From(1), Code = code1 }]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [code1]), Photo: null); // only S1, inactive S2 not included

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: RemoveSubject was NOT called for inactive S2 — it stays in the collection.
        // If the bug were present, S2 would be removed and Subjects.Count would be 1.
        Assert.True(result.IsSuccess);
        Assert.Equal(2, teacher.Subjects.Count); // inactive(2) preserved + new S1 added
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(2));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(1));
    }

    [Fact]
    public async Task Handle_MultipleActiveSubjects_SomeRemovedAndNewOnesAdded()
    {
        // Arrange: teacher has S1, S2, S3 active — new list keeps S3, removes S1+S2, adds S4+S5
        var teacher = CreateTestTeacher(TeacherId.From(1));
        AddActiveSubject(teacher, SubjectId.From(1));
        AddActiveSubject(teacher, SubjectId.From(2));
        AddActiveSubject(teacher, SubjectId.From(3));

        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code3 = SubjectCode.From("CS103");
        var code4 = SubjectCode.From("CS104");
        var code5 = SubjectCode.From("CS105");
        SetupSubjectRepo([
            new() { Id = SubjectId.From(3), Code = code3 },
            new() { Id = SubjectId.From(4), Code = code4 },
            new() { Id = SubjectId.From(5), Code = code5 }
        ]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [code3, code4, code5]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: S1 and S2 removed, S3 retained, S4 and S5 added
        Assert.True(result.IsSuccess);
        Assert.Equal(3, teacher.Subjects.Count);
        Assert.DoesNotContain(teacher.Subjects, s => s.SubjectId == SubjectId.From(1));
        Assert.DoesNotContain(teacher.Subjects, s => s.SubjectId == SubjectId.From(2));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(3));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(4));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(5));
    }

    [Fact]
    public async Task Handle_AllActiveSubjectsReplaced_RemovesAllAndAddsNew()
    {
        // Arrange: teacher has S1, S2 active — new list has S3, S4 (full replacement)
        var teacher = CreateTestTeacher(TeacherId.From(1));
        AddActiveSubject(teacher, SubjectId.From(1));
        AddActiveSubject(teacher, SubjectId.From(2));

        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code3 = SubjectCode.From("CS103");
        var code4 = SubjectCode.From("CS104");
        SetupSubjectRepo([
            new() { Id = SubjectId.From(3), Code = code3 },
            new() { Id = SubjectId.From(4), Code = code4 }
        ]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [code3, code4]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: S1 and S2 both removed, S3 and S4 both added
        Assert.True(result.IsSuccess);
        Assert.Equal(2, teacher.Subjects.Count);
        Assert.DoesNotContain(teacher.Subjects, s => s.SubjectId == SubjectId.From(1));
        Assert.DoesNotContain(teacher.Subjects, s => s.SubjectId == SubjectId.From(2));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(3));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(4));
    }

    /// <summary>
    /// Verifies that simultaneous removal of an active subject and addition of a new subject
    /// works correctly when an unrelated inactive subject is also present. The inactive subject
    /// must be preserved (not removed), while the active removal and new addition both happen.
    /// </summary>
    [Fact]
    public async Task Handle_ActiveRemovedAndNewAddedWhileInactiveSubjectPreserved()
    {
        // Arrange: teacher has S1 active, S2 active, S3 inactive
        // New list = [S1, S4]: keep S1, remove S2, add S4, preserve S3 (inactive)
        var teacher = CreateTestTeacher(TeacherId.From(1));
        AddActiveSubject(teacher, SubjectId.From(1));
        AddActiveSubject(teacher, SubjectId.From(2));
        AddInactiveSubject(teacher, SubjectId.From(3));

        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);

        var code1 = SubjectCode.From("CS101");
        var code4 = SubjectCode.From("CS104");
        SetupSubjectRepo([
            new() { Id = SubjectId.From(1), Code = code1 },
            new() { Id = SubjectId.From(4), Code = code4 }
        ]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(
            CreateUpdate(TeacherId.From(1), [code1, code4]), Photo: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert: S1 kept, S2 removed, S4 added, S3 (inactive) preserved
        Assert.True(result.IsSuccess);
        Assert.Equal(3, teacher.Subjects.Count); // S1(kept) + S3(inactive preserved) + S4(added)
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(1));
        Assert.DoesNotContain(teacher.Subjects, s => s.SubjectId == SubjectId.From(2));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(3));
        Assert.Contains(teacher.Subjects, s => s.SubjectId == SubjectId.From(4));
    }

    [Fact]
    public async Task Handle_WithoutPhoto_DoesNotUploadPhoto()
    {
        // Arrange
        var teacher = CreateTestTeacher(TeacherId.From(1));
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        SetupSubjectRepo([]);
        SetupSuccessfulUpdate(TeacherId.From(1));

        var command = new UpdateTeacherDetails.Command(CreateUpdate(TeacherId.From(1), []), Photo: null);

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
    public async Task Handle_WithPhoto_UploadsAndUpdatesTeacher()
    {
        // Arrange
        var teacherId = TeacherId.From(1);
        var teacher = CreateTestTeacher(teacherId);
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacher);
        SetupSubjectRepo([]);
        SetupSuccessfulUpdate(teacherId);
        _photoStorageMock
            .Setup(s => s.UploadPhotoAsync(
                It.IsAny<TeacherIdentifier>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(FileName.From("abc_profile.jpg"));

        var photo = new UpdateTeacherDetails.TeacherPhoto([1, 2, 3], "image/jpeg", "photo.jpg");
        var command = new UpdateTeacherDetails.Command(CreateUpdate(teacherId, []), Photo: photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _photoStorageMock.Verify(
            s => s.UploadPhotoAsync(
                It.IsAny<TeacherIdentifier>(), It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupSubjectRepo(List<MinimumSubjectProjection> projections)
    {
        _subjectReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<Subject, MinimumSubjectProjection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(projections);
    }

    private void SetupSuccessfulUpdate(TeacherId teacherId)
    {
        _teacherRepositoryMock
            .Setup(r => r.Update(It.IsAny<Teacher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(teacherId));
    }

    private static TeacherForUpdate CreateUpdate(TeacherId id, SubjectCode[] subjects) =>
        new()
        {
            Id = id,
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

    private static void AddActiveSubject(Teacher teacher, SubjectId subjectId)
    {
        teacher.AddSubject(subjectId);
        var ts = teacher.Subjects.First(s => s.SubjectId == subjectId);
        SetEntityProperty(ts, "IsActive", true);
    }

    private static void AddInactiveSubject(Teacher teacher, SubjectId subjectId)
    {
        teacher.AddSubject(subjectId);
        // IsActive defaults to false, simulating a soft-deleted TeacherSubject record
    }

    private static void SetEntityProperty<T>(T entity, string propertyName, object value) where T : class =>
        typeof(T).GetProperty(propertyName)?.SetValue(entity, value);
}
