using Ardalis.Result;
using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Constants;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate.Models;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate.Models;
using Enrollify.Core.ValueObjects;
using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using Enrollify.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Enrollify.IntegrationTests._Tests.Application.ClassSections;

/// <summary>
/// Integration tests verifying ClassSection status lifecycle transitions via Mediator commands.
/// Entities are inserted directly into the database to keep each test focused on its transition logic.
/// </summary>
[Collection("Application")]
public class ClassSectionStatusTransitionTests
{
    private readonly ApplicationTestFixture _fixture;

    public ClassSectionStatusTransitionTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    // =========================================================
    // OPEN TRANSITION
    // =========================================================

    [Fact(DisplayName = "Open class section - draft with subject offerings succeeds")]
    public async Task OpenClassSection_DraftWithOfferings_Succeeds()
    {
        // Arrange
        var (sectionId, _, _) = await CreateDraftSectionWithOfferingAsync();

        // Act
        var result = await _fixture.SendAsync(new OpenClassSectionForEnrollment.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Open, saved!.StatusId);
    }

    [Fact(DisplayName = "Open class section - draft with no offerings returns invalid")]
    public async Task OpenClassSection_DraftWithNoOfferings_ReturnsInvalid()
    {
        // Arrange
        var sectionId = await CreateDraftSectionAsync();

        // Act
        var result = await _fixture.SendAsync(new OpenClassSectionForEnrollment.Command(sectionId));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact(DisplayName = "Open class section - non-existent section returns not found")]
    public async Task OpenClassSection_NotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = ClassSectionId.From(999999);

        // Act
        var result = await _fixture.SendAsync(new OpenClassSectionForEnrollment.Command(nonExistentId));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    // =========================================================
    // LOCK TRANSITION
    // =========================================================

    [Fact(DisplayName = "Lock class section enrollment - open section succeeds")]
    public async Task LockClassSection_OpenSection_Succeeds()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Open);

        // Act
        var result = await _fixture.SendAsync(new LockClassSectionEnrollment.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Locked, saved!.StatusId);
    }

    [Fact(DisplayName = "Lock class section enrollment - draft section returns invalid")]
    public async Task LockClassSection_DraftSection_ReturnsInvalid()
    {
        // Arrange — a brand-new Draft section (no transition needed)
        var sectionId = await CreateDraftSectionAsync();

        // Act
        var result = await _fixture.SendAsync(new LockClassSectionEnrollment.Command(sectionId));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    // =========================================================
    // ACTIVATE TRANSITION
    // =========================================================

    [Fact(DisplayName = "Activate class section - locked section succeeds")]
    public async Task ActivateClassSection_LockedSection_Succeeds()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Locked);

        // Act
        var result = await _fixture.SendAsync(new ActivateClassSection.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Active, saved!.StatusId);
    }

    [Fact(DisplayName = "Activate class section - open section returns invalid")]
    public async Task ActivateClassSection_OpenSection_ReturnsInvalid()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Open);

        // Act
        var result = await _fixture.SendAsync(new ActivateClassSection.Command(sectionId));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    // =========================================================
    // COMPLETE TRANSITION
    // =========================================================

    [Fact(DisplayName = "Complete class section - active section succeeds")]
    public async Task CompleteClassSection_ActiveSection_Succeeds()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Active);

        // Act
        var result = await _fixture.SendAsync(new CompleteClassSection.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Completed, saved!.StatusId);
    }

    [Fact(DisplayName = "Complete class section - locked section returns invalid")]
    public async Task CompleteClassSection_LockedSection_ReturnsInvalid()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Locked);

        // Act
        var result = await _fixture.SendAsync(new CompleteClassSection.Command(sectionId));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    // =========================================================
    // CANCEL TRANSITION
    // =========================================================

    [Fact(DisplayName = "Cancel class section - draft section succeeds")]
    public async Task CancelClassSection_DraftSection_Succeeds()
    {
        // Arrange
        var sectionId = await CreateDraftSectionAsync();

        // Act
        var result = await _fixture.SendAsync(new CancelClassSection.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Cancelled, saved!.StatusId);
    }

    [Fact(DisplayName = "Cancel class section - open section succeeds")]
    public async Task CancelClassSection_OpenSection_Succeeds()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Open);

        // Act
        var result = await _fixture.SendAsync(new CancelClassSection.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Cancelled, saved!.StatusId);
    }

    [Fact(DisplayName = "Cancel class section - locked section succeeds")]
    public async Task CancelClassSection_LockedSection_Succeeds()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Locked);

        // Act
        var result = await _fixture.SendAsync(new CancelClassSection.Command(sectionId));

        // Assert
        Assert.True(result.IsSuccess);
        var saved = await LoadSectionAsync(sectionId);
        Assert.Equal(ClassSectionStatusEnum.Cancelled, saved!.StatusId);
    }

    [Fact(DisplayName = "Cancel class section - completed section returns invalid")]
    public async Task CancelClassSection_CompletedSection_ReturnsInvalid()
    {
        // Arrange
        var sectionId = await CreateSectionInStatusAsync(targetStatus: ClassSectionStatusEnum.Completed);

        // Act
        var result = await _fixture.SendAsync(new CancelClassSection.Command(sectionId));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    /// <summary>
    /// Creates a ClassSection at the requested status by chaining domain transitions in-memory
    /// before persisting (no subject offerings — use CreateDraftSectionWithOfferingAsync for open tests).
    /// </summary>
    private async Task<ClassSectionId> CreateSectionInStatusAsync(ClassSectionStatusEnum targetStatus)
    {
        var (sectionId, section, _) = await CreateDraftSectionCoreAsync(withOffering: false);

        if (targetStatus == ClassSectionStatusEnum.Draft)
            return sectionId;

        // To transition past Draft we need at least one offering for Open
        // Add a minimal offering before transitioning
        var (sectionIdWithOffering, _, offeringSubjectId) = await CreateDraftSectionWithOfferingCoreAsync();

        var transitioned = await _fixture.ExecuteDbContextAsync(async db =>
        {
            // Load fresh from DB
            var s = await db.ClassSections.FindAsync(sectionIdWithOffering);
            s!.OpenForEnrollment(); // Draft → Open

            if (targetStatus == ClassSectionStatusEnum.Open)
            {
                await db.SaveChangesAsync();
                return sectionIdWithOffering;
            }

            s.LockEnrollment(); // Open → Locked

            if (targetStatus == ClassSectionStatusEnum.Locked)
            {
                await db.SaveChangesAsync();
                return sectionIdWithOffering;
            }

            s.Activate(); // Locked → Active

            if (targetStatus == ClassSectionStatusEnum.Active)
            {
                await db.SaveChangesAsync();
                return sectionIdWithOffering;
            }

            s.Complete(); // Active → Completed
            await db.SaveChangesAsync();
            return sectionIdWithOffering;
        });

        return transitioned;
    }

    /// <summary>Creates a Draft section (no offerings).</summary>
    private async Task<ClassSectionId> CreateDraftSectionAsync()
    {
        var (id, _, _) = await CreateDraftSectionCoreAsync(withOffering: false);
        return id;
    }

    /// <summary>Creates a Draft section and one subject offering (needed for the Open transition).</summary>
    private async Task<(ClassSectionId SectionId, ClassSectionId _, SubjectId SubjectId)> CreateDraftSectionWithOfferingAsync()
        => await CreateDraftSectionWithOfferingCoreAsync();

    private async Task<(ClassSectionId SectionId, ClassSection Section, SubjectId SubjectId)> CreateDraftSectionCoreAsync(bool withOffering)
    {
        var (collegeId, courseId, ayId, termId, curriculumId, curriculumSubjectId, subjectId) = await CreateMinimalSetupAsync();

        return await _fixture.ExecuteDbContextAsync(async db =>
        {
            var section = new ClassSection(new ClassSectionForCreation
            {
                Name = TestDataBuilder.GenerateUniqueString("SEC"),
                IntendedYearLevel = YearLevel.From(1),
                CourseId = courseId,
                CurriculumId = curriculumId,
                AcademicTermId = termId,
                CohortAcademicYearId = ayId,
                SectionCode = SectionCode.From('A')
            });

            db.ClassSections.Add(section);
            await db.SaveChangesAsync();

            if (withOffering)
            {
                var offering = new ClassSectionSubjectOffering(new ClassSectionSubjectOfferingForCreation
                {
                    ClassSectionId = section.Id,
                    SubjectId = subjectId,
                    CurriculumSubjectId = curriculumSubjectId,
                    SnapshotSubjectCode = SubjectCode.From("TST-SNAP"),
                    SnapshotSubjectTitle = "Test Snapshot Subject",
                    SnapshotUnits = 3m,
                    SnapshotIsElective = false
                });
                db.ClassSectionSubjectOfferings.Add(offering);
                await db.SaveChangesAsync();
            }

            return (section.Id, section, subjectId);
        });
    }

    private async Task<(ClassSectionId SectionId, ClassSectionId _, SubjectId SubjectId)> CreateDraftSectionWithOfferingCoreAsync()
    {
        var (id, section, subjectId) = await CreateDraftSectionCoreAsync(withOffering: true);
        return (id, id, subjectId);
    }

    private async Task<(CollegeId CollegeId, CourseId CourseId, AcademicYearId AyId, AcademicTermId TermId,
        CurriculumId CurriculumId, CurriculumSubjectId CurriculumSubjectId, SubjectId SubjectId)>
        CreateMinimalSetupAsync()
    {
        return await _fixture.ExecuteDbContextAsync(async db =>
        {
            // College
            var college = TestDataBuilder.CreateCollege(
                TestDataBuilder.GenerateUniqueString("TST").Substring(0, 8),
                TestDataBuilder.GenerateUniqueString("College"));
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            // Course
            var course = new Course(
                CourseCode.From(TestDataBuilder.GenerateUniqueString("CR").Substring(0, 8)),
                TestDataBuilder.GenerateUniqueString("Course"),
                4,
                "Integration test course",
                college.Id);
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            // RoomType + Subject
            var roomType = TestDataBuilder.CreateRoomType(TestDataBuilder.GenerateUniqueString("RT"));
            db.RoomTypes.Add(roomType);
            await db.SaveChangesAsync();

            var subject = new Subject(new SubjectForCreation
            {
                Code = SubjectCode.From(TestDataBuilder.GenerateUniqueString("SJ").Substring(0, 8)),
                Title = TestDataBuilder.GenerateUniqueString("Subject"),
                Units = 3m,
                Description = "Integration test subject",
                PreferRoomTypeId = roomType.Id
            });
            db.Subjects.Add(subject);
            await db.SaveChangesAsync();

            // AcademicYear — shared global counter ensures unique start year across all Application tests
            int year = TestDataBuilder.NextAyStartYear();
            var ayStart = new DateTime(year, 8, 1);
            var ayEnd = new DateTime(year + 1, 6, 30);
            var ay = new AcademicYear(
                AcademicYearStartDate.From(ayStart),
                AcademicYearEndDate.From(ayEnd));
            db.AcademicYears.Add(ay);
            await db.SaveChangesAsync();

            // AcademicTerm (within AY date range)
            ay.AddTerm(
                TermNumber.From(1),
                AcademicTermStartDate.From(new DateTime(year, 8, 1)),
                AcademicTermEndDate.From(new DateTime(year, 11, 30)));
            await db.SaveChangesAsync();

            var termId = ay.AcademicTerms.First().Id;

            // Curriculum — add subject BEFORE saving so EF Core tracks it as Added
            var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
            {
                CourseId = course.Id,
                EffectiveYear = Year.From(year),
                Version = TestDataBuilder.GenerateUniqueString("V").Substring(0, 10),
                Description = "Integration test curriculum"
            });
            curriculum.UpdateStatus(CurriculumStatusEnum.Active);
            curriculum.AddSubject(subject.Id, 1, 1, isElective: false, electiveGroupName: null, subjectUnitsOverride: null);
            db.Curriculums.Add(curriculum);
            await db.SaveChangesAsync();

            var curriculumSubject = curriculum.CurriculumSubjects.First(cs => cs.SubjectId == subject.Id);

            return (college.Id, course.Id, ay.Id, termId, curriculum.Id, curriculumSubject.Id, subject.Id);
        });
    }

    private async Task<ClassSection?> LoadSectionAsync(ClassSectionId id)
    {
        return await _fixture.ExecuteDbContextAsync(async db =>
        {
            return await db.ClassSections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        });
    }
}
