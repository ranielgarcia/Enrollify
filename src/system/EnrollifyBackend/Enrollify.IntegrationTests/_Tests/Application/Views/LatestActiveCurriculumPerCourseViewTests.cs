using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate.Models;
using Enrollify.Core.Constants;
using Enrollify.Core.Models.Views;
using Enrollify.Infrastructure.Data;
using Enrollify.IntegrationTests.Helpers;
using Enrollify.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Enrollify.IntegrationTests._Tests.Application.Views;

/// <summary>
/// Integration tests for vw_LatestActiveCurriculumPerCourse database view.
/// Tests verify the view returns only the single latest active curriculum per course.
/// </summary>
[Collection("Application")]
public class LatestActiveCurriculumPerCourseViewTests
{
    private readonly ApplicationTestFixture _fixture;

    public LatestActiveCurriculumPerCourseViewTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "View returns latest active curriculum when course has multiple active curriculums with different years")]
    public async Task View_Returns_Latest_Active_Curriculum_When_Multiple_Active_Exist()
    {
        // Arrange
        var result = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var (college, course) = await CreateCollegeAndCourseAsync(db);

            // Create three active curriculums with different effective years
            var curriculum2020 = CreateActiveCurriculum(course.Id, 2020, "2020-A", "Curriculum 2020");
            var curriculum2022 = CreateActiveCurriculum(course.Id, 2022, "2022-A", "Curriculum 2022");
            var curriculum2024 = CreateActiveCurriculum(course.Id, 2024, "2024-A", "Curriculum 2024");

            db.Curriculums.AddRange(curriculum2020, curriculum2022, curriculum2024);
            await db.SaveChangesAsync();

            // Act
            return await db.LatestActiveCurriculumPerCourse
                .Where(c => c.CourseId == course.Id)
                .ToListAsync();
        });

        // Assert
        Assert.Single(result);
        var latestCurriculum = result.First();
        Assert.Equal(2024, latestCurriculum.EffectiveYear);
        Assert.Equal("2024-A", latestCurriculum.Version);
        Assert.Equal(CurriculumStatusEnum.Active, latestCurriculum.StatusId);
    }

    [Fact(DisplayName = "View excludes inactive curriculums")]
    public async Task View_Excludes_Inactive_Curriculums()
    {
        // Arrange
        var (result, activeCurriculumId) = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var (college, course) = await CreateCollegeAndCourseAsync(db);

            // Create three curriculums with unique versions
            var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6);
            var activeCurriculum = CreateActiveCurriculum(course.Id, 2024, $"A{uniqueSuffix}", "Active Curriculum");
            var inactiveCurriculum = CreateActiveCurriculum(course.Id, 2025, $"B{uniqueSuffix}", "Will be inactive"); // Start as active
            var draftCurriculum = CreateDraftCurriculum(course.Id, 2026, $"C{uniqueSuffix}", "Draft Curriculum");

            db.Curriculums.AddRange(activeCurriculum, inactiveCurriculum, draftCurriculum);
            await db.SaveChangesAsync();

            // Now set the second curriculum to inactive AFTER saving
            // This avoids the ApplySoftDelete interceptor overriding it
            inactiveCurriculum.GetType().GetProperty(nameof(Curriculum.IsActive))!
                .SetValue(inactiveCurriculum, false);
            await db.SaveChangesAsync();

            // Act
            var result = await db.LatestActiveCurriculumPerCourse
                .Where(c => c.CourseId == course.Id)
                .ToListAsync();

            return (result, activeCurriculum.Id);
        });

        // Assert
        Assert.Single(result);
        Assert.Equal(activeCurriculumId, result.First().Id);
        Assert.Equal(CurriculumStatusEnum.Active, result.First().StatusId);
    }

    [Fact(DisplayName = "View excludes non-active status curriculums (Draft, PhaseOut, Archived)")]
    public async Task View_Excludes_Non_Active_Status_Curriculums()
    {
        // Arrange
        var (result, activeCurriculumId) = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var (college, course) = await CreateCollegeAndCourseAsync(db);

            // Create curriculums with different statuses, all with short unique versions (max 20 chars)
            var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8); // 8 chars
            var activeCurriculum = CreateCurriculumWithStatus(course.Id, 2023, $"V1-{uniqueSuffix}", $"Active curriculum {uniqueSuffix}", CurriculumStatusEnum.Active, true);
            var draftCurriculum = CreateCurriculumWithStatus(course.Id, 2024, $"V2-{uniqueSuffix}", $"Draft curriculum {uniqueSuffix}", CurriculumStatusEnum.Draft, true);
            var phaseOutCurriculum = CreateCurriculumWithStatus(course.Id, 2025, $"V3-{uniqueSuffix}", $"PhaseOut curriculum {uniqueSuffix}", CurriculumStatusEnum.PhaseOut, true);
            var archivedCurriculum = CreateCurriculumWithStatus(course.Id, 2026, $"V4-{uniqueSuffix}", $"Archived curriculum {uniqueSuffix}", CurriculumStatusEnum.Archived, true);

            db.Curriculums.AddRange(activeCurriculum, draftCurriculum, phaseOutCurriculum, archivedCurriculum);
            await db.SaveChangesAsync();

            // Act
            var result = await db.LatestActiveCurriculumPerCourse
                .Where(c => c.CourseId == course.Id)
                .ToListAsync();

            return (result, activeCurriculum.Id);
        });

        // Assert
        Assert.Single(result);
        Assert.Equal(activeCurriculumId, result.First().Id);
        Assert.Equal(CurriculumStatusEnum.Active, result.First().StatusId);
    }

    [Fact(DisplayName = "View returns different curriculums for different courses")]
    public async Task View_Returns_Different_Curriculums_For_Different_Courses()
    {
        // Arrange
        var testData = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var college = TestDataBuilder.CreateCollege("ENG", "Engineering College");
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            var course1 = CreateCourse(college.Id, "CS", "Computer Science", 4);
            var course2 = CreateCourse(college.Id, "IT", "Information Technology", 4);
            db.Courses.AddRange(course1, course2);
            await db.SaveChangesAsync();

            // Create curriculums for both courses
            var course1Curriculum2023 = CreateActiveCurriculum(course1.Id, 2023, "2023-A", "CS Curriculum 2023");
            var course1Curriculum2024 = CreateActiveCurriculum(course1.Id, 2024, "2024-A", "CS Curriculum 2024");
            var course2Curriculum2022 = CreateActiveCurriculum(course2.Id, 2022, "2022-A", "IT Curriculum 2022");
            var course2Curriculum2024 = CreateActiveCurriculum(course2.Id, 2024, "2024-A", "IT Curriculum 2024");

            db.Curriculums.AddRange(course1Curriculum2023, course1Curriculum2024, course2Curriculum2022, course2Curriculum2024);
            await db.SaveChangesAsync();

            // Act
            var result = await db.LatestActiveCurriculumPerCourse.ToListAsync();

            return (
                result,
                course1.Id,
                course2.Id,
                course1Curriculum2024.Id,
                course2Curriculum2024.Id
            );
        });

        // Assert - should have exactly 2 records (one per course)
        var course1Result = testData.result.Where(c => c.CourseId == testData.Item2).ToList();
        var course2Result = testData.result.Where(c => c.CourseId == testData.Item3).ToList();

        Assert.Single(course1Result);
        Assert.Single(course2Result);
        Assert.Equal(testData.Item4, course1Result.First().Id);
        Assert.Equal(testData.Item5, course2Result.First().Id);
    }

    [Fact(DisplayName = "View returns empty when course has no active curriculums")]
    public async Task View_Returns_Empty_When_Course_Has_No_Active_Curriculums()
    {
        // Arrange
        var result = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var (college, course) = await CreateCollegeAndCourseAsync(db);

            // Create draft and active-status curriculums, with short unique versions
            var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            var draftCurriculum = CreateCurriculumWithStatus(course.Id, 2024, $"V1-{uniqueSuffix}", $"Draft curriculum {uniqueSuffix}", CurriculumStatusEnum.Draft, true);
            var activeStatusCurriculum = CreateCurriculumWithStatus(course.Id, 2023, $"V2-{uniqueSuffix}", $"Active status but will be inactive {uniqueSuffix}", CurriculumStatusEnum.Active, true);

            db.Curriculums.AddRange(draftCurriculum, activeStatusCurriculum);
            await db.SaveChangesAsync();

            // Now set the active-status curriculum to IsActive = false AFTER saving
            // This avoids the ApplySoftDelete interceptor overriding it
            activeStatusCurriculum.GetType().GetProperty(nameof(Curriculum.IsActive))!
                .SetValue(activeStatusCurriculum, false);
            await db.SaveChangesAsync();

            // Act
            return await db.LatestActiveCurriculumPerCourse
                .Where(c => c.CourseId == course.Id)
                .ToListAsync();
        });

        // Assert
        Assert.Empty(result);
    }

    [Fact(DisplayName = "View returns curriculum with all expected fields populated")]
    public async Task View_Returns_Curriculum_With_All_Expected_Fields()
    {
        // Arrange
        var approvedDate = DateTimeOffset.UtcNow.AddMonths(-2);
        var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8); // 8 chars
        var uniqueVersion = $"V-{uniqueSuffix}"; // e.g., "V-a1b2c3d4" (11 chars, well under 20)
        var testDescription = $"Test Desc {uniqueSuffix}";
        
        var (result, curriculumId, courseId) = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var (college, course) = await CreateCollegeAndCourseAsync(db);

            var curriculum = CreateActiveCurriculum(course.Id, 2024, uniqueVersion, testDescription);
            
            // Use reflection to set ApprovedDate since there's no public setter
            typeof(Curriculum).GetProperty(nameof(Curriculum.ApprovedDate))!
                .SetValue(curriculum, approvedDate);

            db.Curriculums.Add(curriculum);
            await db.SaveChangesAsync();

            // Act
            var result = await db.LatestActiveCurriculumPerCourse
                .Where(c => c.CourseId == course.Id)
                .SingleAsync();

            return (result, curriculum.Id, course.Id);
        });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(curriculumId, result.Id);
        Assert.Equal(courseId, result.CourseId);
        Assert.Equal(2024, result.EffectiveYear);
        Assert.Equal(uniqueVersion, result.Version);
        Assert.Equal(CurriculumStatusEnum.Active, result.StatusId);
        Assert.Equal(testDescription, result.Description);
        Assert.NotNull(result.ApprovedDate);
        Assert.Equal(approvedDate, result.ApprovedDate.Value, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "View correctly orders by EffectiveYear DESC when years are not sequential")]
    public async Task View_Orders_By_EffectiveYear_DESC_When_Years_Not_Sequential()
    {
        // Arrange
        var (result, curriculum2025Id) = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var (college, course) = await CreateCollegeAndCourseAsync(db);

            // Create curriculums with non-sequential years
            var curriculum2020 = CreateActiveCurriculum(course.Id, 2020, "2020-A", "Curriculum 2020");
            var curriculum2025 = CreateActiveCurriculum(course.Id, 2025, "2025-A", "Curriculum 2025");
            var curriculum2018 = CreateActiveCurriculum(course.Id, 2018, "2018-A", "Curriculum 2018");
            var curriculum2023 = CreateActiveCurriculum(course.Id, 2023, "2023-A", "Curriculum 2023");

            db.Curriculums.AddRange(curriculum2020, curriculum2025, curriculum2018, curriculum2023);
            await db.SaveChangesAsync();

            // Act
            var result = await db.LatestActiveCurriculumPerCourse
                .Where(c => c.CourseId == course.Id)
                .SingleAsync();

            return (result, curriculum2025.Id);
        });

        // Assert
        Assert.Equal(curriculum2025Id, result.Id);
        Assert.Equal(2025, result.EffectiveYear);
    }

    [Fact(DisplayName = "View handles multiple courses with different latest curriculum years")]
    public async Task View_Handles_Multiple_Courses_With_Different_Latest_Years()
    {
        // Arrange
        var testData = await _fixture.ExecuteDbContextAsync(async db =>
        {
            var college = TestDataBuilder.CreateCollege("SCI", "College of Science");
            db.Colleges.Add(college);
            await db.SaveChangesAsync();

            // Create three different courses
            var courseBio = CreateCourse(college.Id, "BIO", "Biology", 4);
            var courseChem = CreateCourse(college.Id, "CHEM", "Chemistry", 4);
            var coursePhys = CreateCourse(college.Id, "PHYS", "Physics", 4);
            db.Courses.AddRange(courseBio, courseChem, coursePhys);
            await db.SaveChangesAsync();

            // Bio: latest is 2024
            db.Curriculums.Add(CreateActiveCurriculum(courseBio.Id, 2020, "2020-A", "Bio 2020"));
            db.Curriculums.Add(CreateActiveCurriculum(courseBio.Id, 2024, "2024-A", "Bio 2024"));

            // Chem: latest is 2022
            db.Curriculums.Add(CreateActiveCurriculum(courseChem.Id, 2022, "2022-A", "Chem 2022"));
            db.Curriculums.Add(CreateActiveCurriculum(courseChem.Id, 2021, "2021-A", "Chem 2021"));

            // Phys: latest is 2025
            db.Curriculums.Add(CreateActiveCurriculum(coursePhys.Id, 2025, "2025-A", "Phys 2025"));
            db.Curriculums.Add(CreateActiveCurriculum(coursePhys.Id, 2023, "2023-A", "Phys 2023"));

            await db.SaveChangesAsync();

            // Act
            var results = await db.LatestActiveCurriculumPerCourse.ToListAsync();

            return (results, courseBio.Id, courseChem.Id, coursePhys.Id);
        });

        // Assert
        var bioResult = testData.results.Single(c => c.CourseId == testData.Item2);
        var chemResult = testData.results.Single(c => c.CourseId == testData.Item3);
        var physResult = testData.results.Single(c => c.CourseId == testData.Item4);

        Assert.Equal(2024, bioResult.EffectiveYear);
        Assert.Equal(2022, chemResult.EffectiveYear);
        Assert.Equal(2025, physResult.EffectiveYear);
    }

    #region Helper Methods

    private async Task<(College college, Course course)> CreateCollegeAndCourseAsync(EnrollifyDbContext db)
    {
        var uniqueCode = TestDataBuilder.GenerateUniqueString("TST").Substring(0, 10);
        var college = TestDataBuilder.CreateCollege(uniqueCode, $"Test College {uniqueCode}");
        db.Colleges.Add(college);
        await db.SaveChangesAsync();

        var courseCode = TestDataBuilder.GenerateUniqueString("CRS").Substring(0, 10);
        var course = CreateCourse(college.Id, courseCode, $"Test Course {courseCode}", 4);
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        return (college, course);
    }

    private Course CreateCourse(CollegeId collegeId, string code, string name, int durationYears)
    {
        return new Course(
            CourseCode.From(code),
            name,
            durationYears,
            $"Description for {name}",
            collegeId
        );
    }

    private Curriculum CreateActiveCurriculum(CourseId courseId, int effectiveYear, string version, string description)
    {
        return CreateCurriculumWithStatus(courseId, effectiveYear, version, description, CurriculumStatusEnum.Active, true);
    }

    private Curriculum CreateInactiveCurriculum(CourseId courseId, int effectiveYear, string version, string description)
    {
        return CreateCurriculumWithStatus(courseId, effectiveYear, version, description, CurriculumStatusEnum.Active, false);
    }

    private Curriculum CreateDraftCurriculum(CourseId courseId, int effectiveYear, string version, string description)
    {
        return CreateCurriculumWithStatus(courseId, effectiveYear, version, description, CurriculumStatusEnum.Draft, true);
    }

    private Curriculum CreateCurriculumWithStatus(CourseId courseId, int effectiveYear, string version, string description, CurriculumStatusEnum status, bool isActive)
    {
        var curriculum = Curriculum.CreateDraftCurriculum(new DraftCurriculumForCreation
        {
            CourseId = courseId,
            EffectiveYear = effectiveYear,
            Version = version,
            Description = description
        });

        // Update status if not Draft
        if (status != CurriculumStatusEnum.Draft)
        {
            curriculum.UpdateStatus(status);
        }

        // Set IsActive using reflection (no public setter)
        typeof(Curriculum).GetProperty(nameof(Curriculum.IsActive))!
            .SetValue(curriculum, isActive);

        return curriculum;
    }

    #endregion
}
