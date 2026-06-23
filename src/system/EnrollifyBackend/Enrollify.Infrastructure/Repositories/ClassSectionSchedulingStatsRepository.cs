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

  public async Task RefreshHardConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.ConflictHard,
      ClassSectionSchedulingStatsAggregateTypeEnum.HARD_CONFLICTS, cancellationToken);
  }

  public async Task RefreshSoftConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.ConflictSoft,
      ClassSectionSchedulingStatsAggregateTypeEnum.SOFT_CONFLICTS, cancellationToken);
  }

  public async Task RefreshDataIntegrityIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.DataIntegrity,
      ClassSectionSchedulingStatsAggregateTypeEnum.DATA_INTEGRITY, cancellationToken);
  }

  public async Task RefreshInformationalIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.Informational,
      ClassSectionSchedulingStatsAggregateTypeEnum.INFORMATIONAL, cancellationToken);
  }

  public async Task RefreshOfferingCountWithIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId,
    CancellationToken ct)
  {
    try
    {
      string sql = @"MERGE [ClassSectionSchedulingStats] AS [Target]
          USING (
	          SELECT COUNT(1)
	          FROM (
		          SELECT DISTINCT OfferingId
		          FROM [ClassSectionValidationIssues]
		          WHERE ClassSectionId=@ClassSectionId AND OfferingId IS NOT NULL
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
        AggregateType = ClassSectionSchedulingStatsAggregateTypeEnum.OFFERING_WITH_ISSUE_COUNT.Name
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

  private async Task RefreshSectionIssueTierAggregateCounts(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, ClassSectionValidationIssueTierEnum issueTier,
    ClassSectionSchedulingStatsAggregateTypeEnum aggregateType, CancellationToken cancellationToken)
  {
    string[] issueTypes = ClassSectionValidationIssueTypeEnum.List
      .Where(t => t.Tier == issueTier).Select(t => t.Name).ToArray();

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
        "Error refreshing {IssueTier} counts for CourseId {CourseId}, TermId {TermId}, ClassSectionId {ClassSectionId}, AggregateType {AggregateType}",
        issueTier.Name, courseId.Value, termId.Value, classSectionId.Value, aggregateType.Name);
    }
  }

  private async Task RefreshOfferingCountWithSpecificIssueForSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, ClassSectionValidationIssueTypeEnum validationIssueTypeToCount,
    ClassSectionSchedulingStatsAggregateTypeEnum aggregateType, CancellationToken cancellationToken)
  {
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
