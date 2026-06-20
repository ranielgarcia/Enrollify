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
      ClassSectionSchedulingStatsAggregateTypeEnum.DraftSections, cancellationToken);
  }

  public async Task RefreshOpenSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    await RefreshSectionCountsForCourseAndClassSectionStatus(termId, courseId, ClassSectionStatusEnum.Open,
      ClassSectionSchedulingStatsAggregateTypeEnum.OpenSections, cancellationToken);
  }

  public async Task RefreshCancelledSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    await RefreshSectionCountsForCourseAndClassSectionStatus(termId, courseId, ClassSectionStatusEnum.Cancelled,
      ClassSectionSchedulingStatsAggregateTypeEnum.CancelledSections, cancellationToken);
  }

  public async Task RefreshHardConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.ConflictHard,
      ClassSectionSchedulingStatsAggregateTypeEnum.HardConflictIssues, cancellationToken);
  }

  public async Task RefreshSoftConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.ConflictSoft,
      ClassSectionSchedulingStatsAggregateTypeEnum.SoftConflictIssues, cancellationToken);
  }

  public async Task RefreshDataIntegrityIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.DataIntegrity,
      ClassSectionSchedulingStatsAggregateTypeEnum.DataIntegrityIssue, cancellationToken);
  }

  public async Task RefreshInformationalIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    await RefreshSectionIssueTierAggregateCounts(termId, courseId, classSectionId,
      ClassSectionValidationIssueTierEnum.Informational,
      ClassSectionSchedulingStatsAggregateTypeEnum.Informational, cancellationToken);
  }

  private async Task RefreshSectionCountsForCourseAndClassSectionStatus(AcademicTermId termId, CourseId courseId,
    ClassSectionStatusEnum classSectionStatus, ClassSectionSchedulingStatsAggregateTypeEnum aggregateType,
    CancellationToken ct)
  {
    try
    {
      string sql = @"
        MERGE [ClassSectionSchedulingStats] AS [Target]
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
    catch (Exception ex)
    {
      _logger.LogError(ex,
        "Error refreshing section counts for CourseId {CourseId}, TermId {TermId}, ClassSectionStatus {ClassSectionStatus}, AggregateType {AggregateType}",
        courseId.Value, termId.Value, classSectionStatus.Value, aggregateType.Value);
      throw;
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
      string sql = @"
        MERGE [ClassSectionSchedulingStats] AS [Target]
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
	        UPDATE SET [Target].[AggregateCount]=[Source].[AggregateCount], [Target].[ComputedAt]=GETUTCDATE();
        ";

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
    catch (Exception ex)
    {
      _logger.LogError(ex,
        "Error refreshing {IssueTier} counts for CourseId {CourseId}, TermId {TermId}, ClassSectionId {ClassSectionId}, AggregateType {AggregateType}",
        issueTier.Name, courseId.Value, termId.Value, classSectionId.Value, aggregateType.Name);
      throw;
    }
  }
}
