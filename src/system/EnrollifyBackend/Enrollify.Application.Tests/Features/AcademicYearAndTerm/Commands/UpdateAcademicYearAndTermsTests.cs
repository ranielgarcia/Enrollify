using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.AcademicYearAndTerm.Commands;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Models;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Models;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.AcademicYearAndTerm.Commands;

public class UpdateAcademicYearAndTermsTests
{
    private readonly Mock<IAcademicYearAndTermRepository> _repositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _readRepositoryMock = new();
    private readonly Mock<IReadRepository<ClassSection>> _classSectionReadRepositoryMock = new();
    private readonly FakeLogger<UpdateAcademicYearAndTerms.Handler> _logger;
    private readonly UpdateAcademicYearAndTerms.Handler _handler;

    private static readonly AcademicYearId ValidId = AcademicYearId.From(1);
    private static readonly AcademicYearStartDate ValidStart =
        AcademicYearStartDate.From(new DateTime(2024, 6, 1));
    private static readonly AcademicYearEndDate ValidEnd =
        AcademicYearEndDate.From(new DateTime(2025, 5, 31));

    public UpdateAcademicYearAndTermsTests()
    {
        _logger = new FakeLogger<UpdateAcademicYearAndTerms.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new UpdateAcademicYearAndTerms.Handler(
            _repositoryMock.Object,
            _readRepositoryMock.Object,
            _classSectionReadRepositoryMock.Object,
            _logger);
    }

    [Fact]
    public async Task Handle_TermsIsNull_ReturnsInvalidResult()
    {
        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("At least one academic term"));
    }

    [Fact]
    public async Task Handle_TermsIsEmpty_ReturnsInvalidResult()
    {
        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("At least one academic term"));
    }

    [Fact]
    public async Task Handle_TermCountLessThanRequired_ReturnsInvalidResult()
    {
        // AcademicTermSystem is Trimester (3); providing only 2 terms
        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd,
        [
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30)),
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31))
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e =>
            e.ErrorMessage.Contains("3") && e.ErrorMessage.Contains("2"));
    }

    [Fact]
    public async Task Handle_TermCountMoreThanRequired_ReturnsInvalidResult()
    {
        // AcademicTermSystem is Trimester (3); providing 4 terms
        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd,
        [
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30)),
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31)),
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30))
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e =>
            e.ErrorMessage.Contains("3") && e.ErrorMessage.Contains("4"));
    }

    [Fact]
    public async Task Handle_OverlappingAcademicYearExists_ReturnsConflict()
    {
        var overlappingYear = new AcademicYear(ValidStart, ValidEnd);
        _readRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlappingYear]);

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Handle_OverlappingAcademicYearExists_LogsWarning()
    {
        var overlappingYear = new AcademicYear(ValidStart, ValidEnd);
        _readRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlappingYear]);

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        Assert.Contains(_logger.Collector.GetSnapshot(), l => l.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_AcademicYearNotFound_ReturnsNotFoundResult()
    {
        SetupNoOverlap();
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_InvalidYearRange_EndNotExactlyOneYearAfterStart_ReturnsInvalidResult()
    {
        // End year 2026 is two years after start 2024 (must be exactly one year apart)
        var invalidEnd = AcademicYearEndDate.From(new DateTime(2026, 5, 31));
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, invalidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_TermStartDateBeforeYearStart_ReturnsInvalidResult()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());

        var terms = new[]
        {
            CreateTerm(1, new DateTime(2023, 1, 1), new DateTime(2024, 9, 30)), // before year
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31))
        };

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_DuplicateTermNumbers_ReturnsInvalidResult()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());

        // Two terms share the same term number
        var terms = new[]
        {
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30)),
            CreateTerm(1, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31))
        };

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_OverlappingTermDates_ReturnsInvalidResult()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());

        // Term 1 ends 2024-10-31, Term 2 starts 2024-10-01 — they overlap
        var terms = new[]
        {
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 10, 31)),
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31))
        };

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithAcademicYearDto()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());
        _repositoryMock
            .Setup(r => r.Update(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.IsType<AcademicYearDto>(result.Value);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsUpdateOnRepositoryOnce()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());
        _repositoryMock
            .Setup(r => r.Update(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Update(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReplacesAllTermsWithNewOnes()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());
        AcademicYear? capturedYear = null;
        _repositoryMock
            .Setup(r => r.Update(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .Callback<AcademicYear, CancellationToken>((ay, _) => capturedYear = ay)
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(capturedYear);
        Assert.Equal(3, capturedYear.AcademicTerms.Count);
    }

    [Fact]
    public async Task Handle_ValidCommand_DoesNotCallCreateRepository()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());
        _repositoryMock
            .Setup(r => r.Update(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Create(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupNoOverlap() =>
        _readRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

    private void SetupExistingYear(AcademicYear year)
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(year);

        // Default: no active/completed sections blocking the update
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<ClassSection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    [Fact(DisplayName = "Handle returns Forbidden when Active sections exist for the academic year terms")]
    public async Task Handle_HasActiveClassSection_ReturnsForbidden()
    {
        SetupNoOverlap();
        SetupExistingYear(CreateYearWithTerms());
        SetupActiveSections(new List<ClassSection> { CreateActiveSectionWithFakeIds() });

        var command = new UpdateAcademicYearAndTerms.Command(ValidId, ValidStart, ValidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Forbidden, result.Status);
    }

    private void SetupActiveSections(List<ClassSection> sections) =>
        _classSectionReadRepositoryMock
            .Setup(r => r.ListAsync(
                It.IsAny<ISpecification<ClassSection>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sections);

    private static ClassSection CreateActiveSectionWithFakeIds()
    {
        var creation = new ClassSectionForCreation
        {
            Name = "Test Section",
            IntendedYearLevel = YearLevel.From(1),
            CourseId = CourseId.From(1),
            CurriculumId = CurriculumId.From(1),
            AcademicTermId = AcademicTermId.From(1),
            CohortAcademicYearId = AcademicYearId.From(1),
            SectionCode = SectionCode.From('A'),
        };
        return new ClassSection(creation)
            .OpenForEnrollment()
            .LockEnrollment()
            .Activate();
    }

    private static AcademicYear CreateYearWithTerms()
    {
        var year = new AcademicYear(ValidStart, ValidEnd);
        year.AddTerm(
            TermNumber.From(1),
            AcademicTermStartDate.From(new DateTime(2024, 6, 1)),
            AcademicTermEndDate.From(new DateTime(2024, 9, 30)));
        year.AddTerm(
            TermNumber.From(2),
            AcademicTermStartDate.From(new DateTime(2024, 10, 1)),
            AcademicTermEndDate.From(new DateTime(2025, 1, 31)));
        year.AddTerm(
            TermNumber.From(3),
            AcademicTermStartDate.From(new DateTime(2025, 2, 1)),
            AcademicTermEndDate.From(new DateTime(2025, 5, 31)));
        return year;
    }

    private static InitiateAcademicTerm CreateTerm(int termNumber, DateTime start, DateTime end) =>
        new(TermNumber.From(termNumber),
            AcademicTermStartDate.From(start),
            AcademicTermEndDate.From(end));

    private static InitiateAcademicTerm[] CreateValidTerms() =>
    [
        new(TermNumber.From(1),
            AcademicTermStartDate.From(new DateTime(2024, 6, 1)),
            AcademicTermEndDate.From(new DateTime(2024, 9, 30))),
        new(TermNumber.From(2),
            AcademicTermStartDate.From(new DateTime(2024, 10, 1)),
            AcademicTermEndDate.From(new DateTime(2025, 1, 31))),
        new(TermNumber.From(3),
            AcademicTermStartDate.From(new DateTime(2025, 2, 1)),
            AcademicTermEndDate.From(new DateTime(2025, 5, 31)))
    ];
}
