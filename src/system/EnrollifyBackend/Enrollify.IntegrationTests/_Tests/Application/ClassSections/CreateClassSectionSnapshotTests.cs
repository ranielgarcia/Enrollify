using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate.Models;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;
using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Enrollify.IntegrationTests._Tests.Application.ClassSections;

/// <summary>
/// Integration tests verifying that snapshot fields on ClassSectionSubjectOffering are
/// correctly populated from the CurriculumSubject at creation time.
/// </summary>
[Collection("Application")]
public class CreateClassSectionSnapshotTests
{
    private readonly ApplicationTestFixture _fixture;

    public CreateClassSectionSnapshotTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Create class section - snapshot fields are copied from curriculum subject data")]
    public async Task CreateClassSection_ValidData_SnapshotFieldsMatchCurriculumSubjectData()
    {
        // Arrange
        var setup = await CreateFullSetupAsync(subjectUnitsOverride: null);

        var command = new CreateClassSection.Command(
            YearLevel: YearLevel.From(1),
            CourseId: setup.CourseId,
            AcademicTermId: setup.AcademicTermId,
            AdviserId: setup.TeacherId,
            StudentCapacity: 30);

        // Act
        var result = await _fixture.SendAsync(command);

        // Assert
        Assert.True(result.IsSuccess, $"CreateClassSection failed: {string.Join(", ", result.Errors)}");

        var offerings = await _fixture.ExecuteDbContextAsync(async db =>
            await db.ClassSectionSubjectOfferings
                .AsNoTracking()
                .Where(o => (int)o.ClassSectionId == (int)result.Value)
                .ToListAsync());

        Assert.Single(offerings);
        var offering = offerings[0];
        Assert.Equal(setup.SubjectCode, (string)offering.SnapshotSubjectCode);
        Assert.Equal(setup.SubjectTitle, offering.SnapshotSubjectTitle);
        Assert.Equal(setup.SubjectUnits, offering.SnapshotUnits);
        Assert.False(offering.SnapshotIsElective);
        Assert.Null(offering.SnapshotElectiveGroupName);
    }

    [Fact(DisplayName = "Create class section - snapshot units use curriculum subject override when set")]
    public async Task CreateClassSection_WithUnitsOverride_SnapshotUnitsUsesOverride()
    {
        // Arrange — curriculum has a units override of 4 while the subject itself has 3 units
        const decimal subjectUnits = 3m;
        const decimal overrideUnits = 4m;
        var setup = await CreateFullSetupAsync(subjectUnitsOverride: overrideUnits, subjectUnits: subjectUnits);

        var command = new CreateClassSection.Command(
            YearLevel: YearLevel.From(1),
            CourseId: setup.CourseId,
            AcademicTermId: setup.AcademicTermId,
            AdviserId: setup.TeacherId,
            StudentCapacity: 30);

        // Act
        var result = await _fixture.SendAsync(command);

        // Assert
        Assert.True(result.IsSuccess, $"CreateClassSection failed: {string.Join(", ", result.Errors)}");

        var offerings = await _fixture.ExecuteDbContextAsync(async db =>
            await db.ClassSectionSubjectOfferings
                .AsNoTracking()
                .Where(o => (int)o.ClassSectionId == (int)result.Value)
                .ToListAsync());

        Assert.Single(offerings);
        var offering = offerings[0];
        Assert.Equal(overrideUnits, offering.SnapshotUnits);
        Assert.NotEqual(subjectUnits, offering.SnapshotUnits);
    }

    // =========================================================
    // HELPER
    // =========================================================

    private record FullSetup(
        CourseId CourseId,
        AcademicTermId AcademicTermId,
        TeacherId TeacherId,
        string SubjectCode,
        string SubjectTitle,
        decimal SubjectUnits);

    private async Task<FullSetup> CreateFullSetupAsync(
        decimal? subjectUnitsOverride,
        decimal subjectUnits = 3m)
    {
        return await _fixture.ExecuteDbContextAsync(async db =>
        {
            // College
            var college = TestDataBuilder.CreateCollege(
                TestDataBuilder.GenerateUniqueString("TST").Substring(0, 8),
                TestDataBuilder.GenerateUniqueString("College"));
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            // Department (required for Teacher)
            var department = new Department(
                DepartmentCode.From(TestDataBuilder.GenerateUniqueString("DP").Substring(0, 8)),
                TestDataBuilder.GenerateUniqueString("Department"),
                "Test Chairperson",
                "Integration test department",
                college.Id);
            db.Departments.Add(department);
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

            // Teacher
            var uniqueId = TestDataBuilder.GenerateUniqueString("T").Substring(0, 10);
            var teacher = new Teacher(
                "Test",
                null,
                "Teacher",
                TeacherIdentifier.From(uniqueId),
                TeacherEmail.From($"{uniqueId}@test.enrollify.local"),
                TeacherPhoneNumber.From(TestDataBuilder.NextPhoneNumber()),
                department.Id);
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();

            // RoomType + Subject
            var roomType = TestDataBuilder.CreateRoomType(TestDataBuilder.GenerateUniqueString("RT"));
            db.RoomTypes.Add(roomType);
            await db.SaveChangesAsync();

            var subjectCode = TestDataBuilder.GenerateUniqueString("SJ").Substring(0, 8);
            var subjectTitle = TestDataBuilder.GenerateUniqueString("Subject Title");
            var subject = new Subject(new SubjectForCreation
            {
                Code = SubjectCode.From(subjectCode),
                Title = subjectTitle,
                Units = subjectUnits,
                Description = "Integration test subject",
                PreferRoomTypeId = roomType.Id
            });
            db.Subjects.Add(subject);
            await db.SaveChangesAsync();

            // AcademicYear — shared global counter ensures unique start year across all Application tests
            int year = TestDataBuilder.NextAyStartYear();
            var ay = new AcademicYear(
                AcademicYearStartDate.From(new DateTime(year, 8, 1)),
                AcademicYearEndDate.From(new DateTime(year + 1, 6, 30)));
            db.AcademicYears.Add(ay);
            await db.SaveChangesAsync();

            // AcademicTerm 1 within the AY
            ay.AddTerm(
                TermNumber.From(1),
                AcademicTermStartDate.From(new DateTime(year, 8, 1)),
                AcademicTermEndDate.From(new DateTime(year, 11, 30)));
            await db.SaveChangesAsync();

            var academicTerm = ay.AcademicTerms.First();

            // Curriculum — add subject BEFORE saving so EF Core tracks it as Added
            var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
            {
                CourseId = course.Id,
                EffectiveYear = Year.From(year),
                Version = TestDataBuilder.GenerateUniqueString("V").Substring(0, 10),
                Description = "Integration test curriculum"
            });
            curriculum.UpdateStatus(CurriculumStatusEnum.Active);
            curriculum.AddSubject(subject.Id, 1, 1, isElective: false, electiveGroupName: null, subjectUnitsOverride: subjectUnitsOverride);
            db.Curriculums.Add(curriculum);
            await db.SaveChangesAsync();

            // CourseCurriculumAssignment — links Course + cohort AY (= ay for Year 1) + Curriculum
            var assignment = new CourseCurriculumAssignment(course.Id, ay.Id, curriculum.Id);
            db.CourseCurriculumAssignments.Add(assignment);
            await db.SaveChangesAsync();

            return new FullSetup(
                CourseId: course.Id,
                AcademicTermId: academicTerm.Id,
                TeacherId: teacher.Id,
                SubjectCode: subjectCode,
                SubjectTitle: subjectTitle,
                SubjectUnits: subjectUnits);
        });
    }
}
