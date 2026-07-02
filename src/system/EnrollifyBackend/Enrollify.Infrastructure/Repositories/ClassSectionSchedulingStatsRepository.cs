using Dapper;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;
using Enrollify.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionSchedulingStatsRepository : IClassSectionSchedulingStatsRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly ILogger<ClassSectionSchedulingStatsRepository> _logger;

  public ClassSectionSchedulingStatsRepository(IDbConnectionFactory connectionFactory,
    ILogger<ClassSectionSchedulingStatsRepository> logger)
  {
    _connectionFactory = connectionFactory;
    _logger = logger;
  }

  public async Task RefreshDraftSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    await RefreshSectionCountsForCourseAndClassSectionStatus(termId, courseId, ClassSectionStatusEnum.Draft,
      ClassSectionSchedulingStatsAggregateTypeEnum.DRAFT_SECTIONS, cancellationToken);
  }

  public async Task RefreshOpenSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    await RefreshSectionCountsForCourseAndClassSectionStatus(termId, courseId, ClassSectionStatusEnum.Open,
      ClassSectionSchedulingStatsAggregateTypeEnum.OPEN_SECTIONS, cancellationToken);
  }

  public async Task RefreshCancelledSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    await RefreshSectionCountsForCourseAndClassSectionStatus(termId, courseId, ClassSectionStatusEnum.Cancelled,
      ClassSectionSchedulingStatsAggregateTypeEnum.CANCELLED_SECTIONS, cancellationToken);
  }

  public async Task RefreshScheduleConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.SCHEDULE_CONFLICT,
      ClassSectionSchedulingStatsAggregateTypeEnum.SCHEDULE_CONFLICTS, cancellationToken);
  }

  public async Task RefreshSchedulePolicyViolationIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.SCHEDULE_POLICY_VIOLATION,
      ClassSectionSchedulingStatsAggregateTypeEnum.SCHEDULE_POLICY_VIOLATIONS, cancellationToken);
  }

  public async Task RefreshCapacityConstraintIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.CAPACITY_CONSTRAINT,
      ClassSectionSchedulingStatsAggregateTypeEnum.CAPACITY_CONSTRAINTS, cancellationToken);
  }

  public async Task RefreshResourceMisalignmentIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.RESOURCE_MISALIGNMENT,
      ClassSectionSchedulingStatsAggregateTypeEnum.RESOURCE_MISALIGNMENTS, cancellationToken);
  }

  public async Task RefreshMissingRequirementIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT,
      ClassSectionSchedulingStatsAggregateTypeEnum.MISSING_REQUIREMENTS, cancellationToken);
  }

  public async Task RefreshDataInconsistencyIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.DATA_INCONSISTENCY,
      ClassSectionSchedulingStatsAggregateTypeEnum.DATA_INCONSISTENCIES, cancellationToken);
  }

  public async Task RefreshDefaultValueIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueCategoryAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE,
      ClassSectionSchedulingStatsAggregateTypeEnum.DEFAULT_VALUES, cancellationToken);
  }

  public async Task RefreshOfferingCountWithIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId,
    CancellationToken ct)
  {
    var excludedValidationIssueTypes = ClassSectionValidationIssueTypeEnum
      .List.Where(x => x.Severity == DomainValidationErrorSeverityEnum.Info)
      .Select(x => x.Name).ToArray();

    try
    {
      string sql = @"MERGE [ClassSectionSchedulingStats] AS [Target]
          USING (
	          SELECT COUNT(1)
	          FROM (
		          SELECT DISTINCT OfferingId
		          FROM [ClassSectionValidationIssues]
		          WHERE ClassSectionId=@ClassSectionId AND OfferingId IS NOT NULL AND Type NOT IN @ExcludedValidationIssueTypes
		          GROUP BY OfferingId
	          ) AS O
          ) AS [Source] (OfferingsWithIssuesCount)
          ON [Target].[AcademicTermId]=@TermId AND [Target].[CourseId]=@CourseId AND [Target].[ClassSectionId]=@ClassSectionId AND [Target].[AggregateType]=@AggregateType
          WHEN NOT MATCHED THEN
	          INSERT (AcademicTermId, CourseId, ClassSectionId, AggregateType, AggregateCount, ComputedAt)
	          VALUES (@TermId, @CourseId, @ClassSectionId, @AggregateType, [Source].[OfferingsWithIssuesCount], GETUTCDATE())
          WHEN MATCHED THEN
	          UPDATE SET [Target].[AggregateCount]=[Source].[OfferingsWithIssuesCount], [Target].[ComputedAt]=GETUTCDATE();";

      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

      await conn.ExecuteAsync(sql, new
      {
        TermId = termId,
        CourseId = courseId,
        ClassSectionId = classSectionId,
        AggregateType = ClassSectionSchedulingStatsAggregateTypeEnum.OFFERING_WITH_ISSUE_COUNT.Name,
        ExcludedValidationIssueTypes = excludedValidationIssueTypes,
      });
    }
    catch (SqlException ex)
    {
      _logger.LogError(ex,
        "Error refreshing offering with issue counts for CourseId {CourseId}, TermId {TermId}, ClassSectionStatus {ClassSectionStatus}, AggregateType {AggregateType}",
        courseId.Value, termId.Value, classSectionId.Value,
        ClassSectionSchedulingStatsAggregateTypeEnum.OFFERING_WITH_ISSUE_COUNT.Name);
    }
  }

  public async Task RefreshTotalValidationIssuesCountAcrossOfferingsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId,
    CancellationToken ct)
  {
    var excludedValidationIssueTypes = ClassSectionValidationIssueTypeEnum
      .List.Where(x => x.Severity == DomainValidationErrorSeverityEnum.Info)
      .Select(x => x.Name).ToArray();

    try
    {
      string sql = @"
MERGE [ClassSectionSchedulingStats] AS [Target]
	USING (
		SELECT COUNT(1)
		FROM [ClassSectionValidationIssues]
		WHERE ClassSectionId=@ClassSectionId AND Type NOT IN @ExcludedValidationIssueTypes
	) AS [Source] (TotalClassSectionIssuesAcrossOfferingsCount)
ON [Target].[AcademicTermId]=@TermId AND [Target].[CourseId]=@CourseId AND [Target].[ClassSectionId]=@ClassSectionId AND [Target].[AggregateType]=@AggregateType
WHEN NOT MATCHED THEN
	INSERT (AcademicTermId, CourseId, ClassSectionId, AggregateType, AggregateCount, ComputedAt)
	VALUES (@TermId, @CourseId, @ClassSectionId, @AggregateType, [Source].[TotalClassSectionIssuesAcrossOfferingsCount], GETUTCDATE())
WHEN MATCHED THEN
	UPDATE SET [Target].[AggregateCount]=[Source].[TotalClassSectionIssuesAcrossOfferingsCount], [Target].[ComputedAt]=GETUTCDATE();";

      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

      await conn.ExecuteAsync(sql, new
      {
        TermId = termId,
        CourseId = courseId,
        ClassSectionId = classSectionId,
        AggregateType = ClassSectionSchedulingStatsAggregateTypeEnum.TOTAL_VALIDATION_ISSUES_ACROSS_OFFERINGS_COUNT.Name,
        ExcludedValidationIssueTypes = excludedValidationIssueTypes,
      });
    }
    catch (SqlException ex)
    {
      _logger.LogError(ex,
        "Error refreshing offering with issue counts for CourseId {CourseId}, TermId {TermId}, ClassSectionStatus {ClassSectionStatus}, AggregateType {AggregateType}",
        courseId.Value, termId.Value, classSectionId.Value,
        ClassSectionSchedulingStatsAggregateTypeEnum.TOTAL_VALIDATION_ISSUES_ACROSS_OFFERINGS_COUNT.Name);
    }
  }

  public async Task RefreshOfferingsCountForClassSection(ClassSectionId classSectionId,
    CancellationToken ct)
  {
    try
    {
      string sql = @"MERGE [ClassSectionSchedulingStats] AS [Target]
      USING (
	      SELECT O.ClassSectionId, CS.CourseId, CS.AcademicTermId , COUNT(1) AS NumberOfOfferings
	      FROM ClassSectionSubjectOffering O
	      JOIN ClassSections CS ON CS.Id = O.ClassSectionId
	      WHERE CS.IsActive=1 AND O.IsActive=1 AND ClassSectionId=@ClassSectionId
	      GROUP BY O.ClassSectionId, CS.CourseId, CS.AcademicTermId
      ) AS [Source] (ClassSectionId, CourseId, TermId, NumberOfOfferings)
      ON [Target].[AcademicTermId]=[Source].[TermId] AND [Target].[CourseId]=[Source].[CourseId] AND [Target].[ClassSectionId]=@ClassSectionId AND [Target].[AggregateType]=@AggregateType
      WHEN NOT MATCHED THEN
	      INSERT (AcademicTermId, CourseId, ClassSectionId, AggregateType, AggregateCount, ComputedAt)
	      VALUES ([Source].[TermId], [Source].[CourseId], @ClassSectionId, @AggregateType, [Source].[NumberOfOfferings], GETUTCDATE())
      WHEN MATCHED THEN
	      UPDATE SET [Target].[AggregateCount]=[Source].[NumberOfOfferings], [Target].[ComputedAt]=GETUTCDATE();";

      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

      await conn.ExecuteAsync(sql, new
      {
        ClassSectionId = classSectionId,
        AggregateType = ClassSectionSchedulingStatsAggregateTypeEnum.OFFERINGS_COUNT.Name
      });
    }
    catch (SqlException ex)
    {
      _logger.LogError(ex,
        "Error refreshing offering counts for ClassSectionStatus {ClassSectionStatus}, AggregateType {AggregateType}",
        classSectionId.Value,
        ClassSectionSchedulingStatsAggregateTypeEnum.OFFERINGS_COUNT.Name);
    }
  }

  public async Task RefreshOfferingCountWithMissingTeacherIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshOfferingCountWithSpecificIssueForSection(termId, courseId, classSectionId,
      ClassSectionValidationIssueTypeEnum.TEACHER_NOT_ASSIGNED,
      ClassSectionSchedulingStatsAggregateTypeEnum.OFFERING_MISSING_TEACHER_COUNT, cancellationToken);
  }

  public async Task RefreshOfferingCountWithMissingRoomIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshOfferingCountWithSpecificIssueForSection(termId, courseId, classSectionId,
      ClassSectionValidationIssueTypeEnum.ROOM_NOT_ASSIGNED,
      ClassSectionSchedulingStatsAggregateTypeEnum.OFFERING_MISSING_ROOM_COUNT, cancellationToken);
  }

  public async Task RefreshOfferingCountWithNoScheduleIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshOfferingCountWithSpecificIssueForSection(termId, courseId, classSectionId,
      ClassSectionValidationIssueTypeEnum.NO_SCHEDULES,
      ClassSectionSchedulingStatsAggregateTypeEnum.OFFERING_NO_SCHEDULE_COUNT, cancellationToken);
  }

  private async Task RefreshSectionCountsForCourseAndClassSectionStatus(AcademicTermId termId, CourseId courseId,
    ClassSectionStatusEnum classSectionStatus, ClassSectionSchedulingStatsAggregateTypeEnum aggregateType,
    CancellationToken ct)
  {
    try
    {
      string sql = @"MERGE [ClassSectionSchedulingStats] AS [Target]
        USING (
	        SELECT COUNT(1) AggregateCount
	        FROM [dbo].[ClassSections]
	        WHERE CourseId=@CourseId AND AcademicTermId=@TermId AND StatusId=@ClassSectionStatus AND isActive=1
        ) AS [Source] (AggregateCount)
        ON [Target].[AcademicTermId]=@TermId AND [Target].[CourseId]=@CourseId AND [Target].[AggregateType]=@AggregateType
        WHEN NOT MATCHED THEN
	        INSERT (AcademicTermId, CourseId, AggregateType, AggregateCount, ComputedAt)
	        VALUES (@TermId, @CourseId, @AggregateType, [Source].[AggregateCount], GETUTCDATE())
        WHEN MATCHED THEN
	        UPDATE SET [Target].[AggregateCount]=[Source].[AggregateCount], [Target].[ComputedAt]=GETUTCDATE();";

      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(ct);

      await conn.ExecuteAsync(sql, new
      {
        TermId = termId,
        CourseId = courseId,
        ClassSectionStatus = classSectionStatus.Value,
        AggregateType = aggregateType.Name
      });
    }
    catch (SqlException ex)
    {
      _logger.LogError(ex,
        "Error refreshing section counts for CourseId {CourseId}, TermId {TermId}, ClassSectionStatus {ClassSectionStatus}, AggregateType {AggregateType}",
        courseId.Value, termId.Value, classSectionStatus.Value, aggregateType.Value);
    }
  }

  private async Task RefreshSectionIssueCategoryAggregateCounts(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, ClassSectionValidationIssueCategoryEnum issueCategory,
    ClassSectionSchedulingStatsAggregateTypeEnum aggregateType, CancellationToken cancellationToken)
  {
    string[] issueTypes = ClassSectionValidationIssueTypeEnum.List
      .Where(t => t.Category == issueCategory).Select(t => t.Name).ToArray();

    try
    {
      string sql = @"MERGE [ClassSectionSchedulingStats] AS [Target]
        USING (
	        SELECT COUNT(1) AggregateCount
	        FROM ClassSectionValidationIssues
	        WHERE CourseId=@CourseId AND AcademicTermId=@TermId AND ClassSectionId=@ClassSectionId AND Type IN @IssueTypes
        ) AS [Source] (AggregateCount)
        ON [Target].[AcademicTermId]=@TermId AND [Target].[CourseId]=@CourseId AND [Target].[ClassSectionId]=@ClassSectionId AND [Target].[AggregateType]=@AggregateType
        WHEN NOT MATCHED THEN
	        INSERT (AcademicTermId, CourseId, ClassSectionId, AggregateType, AggregateCount, ComputedAt)
	        VALUES (@TermId, @CourseId, @ClassSectionId, @AggregateType, [Source].[AggregateCount], GETUTCDATE())
        WHEN MATCHED THEN
	        UPDATE SET [Target].[AggregateCount]=[Source].[AggregateCount], [Target].[ComputedAt]=GETUTCDATE();";

      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(cancellationToken);

      await conn.ExecuteAsync(sql, new
      {
        TermId = termId,
        CourseId = courseId,
        ClassSectionId = classSectionId,
        AggregateType = aggregateType.Name,
        IssueTypes = issueTypes
      });
    }
    catch (SqlException ex)
    {
      _logger.LogError(ex,
        "Error refreshing {IssueCategory} counts for CourseId {CourseId}, TermId {TermId}, ClassSectionId {ClassSectionId}, AggregateType {AggregateType}",
        issueCategory.Name, courseId.Value, termId.Value, classSectionId.Value, aggregateType.Name);
    }
  }

  private async Task RefreshOfferingCountWithSpecificIssueForSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, ClassSectionValidationIssueTypeEnum validationIssueTypeToCount,
    ClassSectionSchedulingStatsAggregateTypeEnum aggregateType, CancellationToken cancellationToken)
  {

    _logger.LogDebug("Refreshing offering count for ClassSectionId {ClassSectionId} with issue type {ValidationIssueTypeToCount} for aggregate {AggregateType}",
      classSectionId.Value, validationIssueTypeToCount.Name, aggregateType.Name);

    try
    {
      string sql = @"MERGE [ClassSectionSchedulingStats] AS [Target]
          USING (
	          SELECT COUNT(1)
	          FROM (
		          SELECT DISTINCT OfferingId
		          FROM [ClassSectionValidationIssues]
		          WHERE ClassSectionId=@ClassSectionId AND OfferingId IS NOT NULL AND [Type] = @ValidationIssueTypeToCount
		          GROUP BY OfferingId
	          ) AS O
          ) AS [Source] (OfferingsWithIssuesCount)
          ON [Target].[AcademicTermId]=@TermId AND [Target].[CourseId]=@CourseId AND [Target].[ClassSectionId]=@ClassSectionId AND [Target].[AggregateType]=@AggregateType
          WHEN NOT MATCHED THEN
	          INSERT (AcademicTermId, CourseId, ClassSectionId, AggregateType, AggregateCount, ComputedAt)
	          VALUES (@TermId, @CourseId, @ClassSectionId, @AggregateType, [Source].[OfferingsWithIssuesCount], GETUTCDATE())
          WHEN MATCHED THEN
	          UPDATE SET [Target].[AggregateCount]=[Source].[OfferingsWithIssuesCount], [Target].[ComputedAt]=GETUTCDATE();";

      using SqlConnection conn = await _connectionFactory.CreateOpenAsync(cancellationToken);

      await conn.ExecuteAsync(sql, new
      {
        TermId = termId,
        CourseId = courseId,
        ClassSectionId = classSectionId,
        ValidationIssueTypeToCount = validationIssueTypeToCount.Name,
        AggregateType = aggregateType.Name
      });
    }
    catch (SqlException ex)
    {
      _logger.LogError(ex,
        "Error refreshing {AggregateType} counts for CourseId {CourseId}, TermId {TermId}, ClassSectionId {ClassSectionId}, ValidationIssueTypeToCount {ValidationIssueTypeToCount}",
        aggregateType.Name, courseId.Value, termId.Value, classSectionId.Value, validationIssueTypeToCount.Name);
    }
  }
}
