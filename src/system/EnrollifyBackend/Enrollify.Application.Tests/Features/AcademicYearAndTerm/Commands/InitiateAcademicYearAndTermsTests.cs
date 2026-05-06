using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.AcademicYearAndTerm.Commands;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Models;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;

namespace Enrollify.Application.Tests.Features.AcademicYearAndTerm.Commands;

public class InitiateAcademicYearAndTermsTests
{
    private readonly Mock<IAcademicYearAndTermRepository> _repositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _readRepositoryMock = new();
    private readonly FakeLogger<InitiateAcademicYearAndTerms.Handler> _logger;
    private readonly InitiateAcademicYearAndTerms.Handler _handler;

    private static readonly AcademicYearStartDate ValidStart =
        AcademicYearStartDate.From(new DateTime(2024, 6, 1));
    private static readonly AcademicYearEndDate ValidEnd =
        AcademicYearEndDate.From(new DateTime(2025, 5, 31));

    public InitiateAcademicYearAndTermsTests()
    {
        _logger = new FakeLogger<InitiateAcademicYearAndTerms.Handler>(
            FakeLogCollector.Create(new FakeLogCollectorOptions()));

        _handler = new InitiateAcademicYearAndTerms.Handler(
            _repositoryMock.Object,
            _readRepositoryMock.Object,
            _logger);
    }

    [Fact]
    public async Task Handle_TermsIsNull_ReturnsInvalidResult()
    {
        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("At least one academic term"));
    }

    [Fact]
    public async Task Handle_TermsIsEmpty_ReturnsInvalidResult()
    {
        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, []);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("At least one academic term"));
    }

    [Fact]
    public async Task Handle_TermCountLessThanRequired_ReturnsInvalidResult()
    {
        // AcademicSystem is Trimester (3); providing only 1 term
        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd,
        [
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30))
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains(result.ValidationErrors, e =>
            e.ErrorMessage.Contains("3") && e.ErrorMessage.Contains("1"));
    }

    [Fact]
    public async Task Handle_TermCountMoreThanRequired_ReturnsInvalidResult()
    {
        // AcademicSystem is Trimester (3); providing 4 terms (duplicate term number used to stay within valid values)
        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd,
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
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    [Fact]
    public async Task Handle_OverlappingAcademicYearExists_LogsWarning()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        Assert.Contains(_logger.Collector.GetSnapshot(), l => l.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Handle_InvalidYearRange_EndNotExactlyOneYearAfterStart_ReturnsInvalidResult()
    {
        // End year 2026 is two years after start year 2024 (must be exactly one year apart)
        var invalidEnd = AcademicYearEndDate.From(new DateTime(2026, 5, 31));
        SetupNoOverlap();

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, invalidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_TermStartDateBeforeYearStart_ReturnsInvalidResult()
    {
        SetupNoOverlap();

        var terms = new[]
        {
            CreateTerm(1, new DateTime(2023, 1, 1), new DateTime(2024, 9, 30)), // starts before year
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31))
        };

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_TermEndDateAfterYearEnd_ReturnsInvalidResult()
    {
        SetupNoOverlap();

        var terms = new[]
        {
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30)),
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 7, 31)) // ends after year
        };

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_DuplicateTermNumbers_ReturnsInvalidResult()
    {
        SetupNoOverlap();

        // Terms 1 and 2 share the same term number
        var terms = new[]
        {
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 9, 30)),
            CreateTerm(1, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31))
        };

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_OverlappingTermDates_ReturnsInvalidResult()
    {
        SetupNoOverlap();

        // Term 1 ends 2024-10-31, Term 2 starts 2024-10-01 — they overlap
        var terms = new[]
        {
            CreateTerm(1, new DateTime(2024, 6, 1), new DateTime(2024, 10, 31)),
            CreateTerm(2, new DateTime(2024, 10, 1), new DateTime(2025, 1, 31)),
            CreateTerm(3, new DateTime(2025, 2, 1), new DateTime(2025, 5, 31))
        };

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, terms);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithAcademicYearDto()
    {
        SetupNoOverlap();
        var academicYear = new AcademicYear(ValidStart, ValidEnd);
        _repositoryMock
            .Setup(r => r.Create(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(academicYear));

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, CreateValidTerms());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.IsType<AcademicYearDto>(result.Value);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsCreateOnRepositoryOnce()
    {
        SetupNoOverlap();
        _repositoryMock
            .Setup(r => r.Create(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Create(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAcademicYearWithAllTerms()
    {
        SetupNoOverlap();
        AcademicYear? capturedYear = null;
        _repositoryMock
            .Setup(r => r.Create(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .Callback<AcademicYear, CancellationToken>((ay, _) => capturedYear = ay)
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(capturedYear);
        Assert.Equal(3, capturedYear.AcademicTerms.Count);
    }

    [Fact]
    public async Task Handle_ValidCommand_DoesNotLogWarning()
    {
        SetupNoOverlap();
        _repositoryMock
            .Setup(r => r.Create(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new AcademicYear(ValidStart, ValidEnd)));

        var command = new InitiateAcademicYearAndTerms.Command(ValidStart, ValidEnd, CreateValidTerms());

        await _handler.Handle(command, CancellationToken.None);

        Assert.DoesNotContain(_logger.Collector.GetSnapshot(), l => l.Level == LogLevel.Warning);
    }

    private void SetupNoOverlap() =>
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<ISpecification<AcademicYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);

    private static InitiateAcademicTerm CreateTerm(int termNumber, DateTime start, DateTime end) =>
        new(AcademicTermNumber.From(termNumber),
            AcademicTermStartDate.From(start),
            AcademicTermEndDate.From(end));

    private static InitiateAcademicTerm[] CreateValidTerms() =>
    [
        new(AcademicTermNumber.From(1),
            AcademicTermStartDate.From(new DateTime(2024, 6, 1)),
            AcademicTermEndDate.From(new DateTime(2024, 9, 30))),
        new(AcademicTermNumber.From(2),
            AcademicTermStartDate.From(new DateTime(2024, 10, 1)),
            AcademicTermEndDate.From(new DateTime(2025, 1, 31))),
        new(AcademicTermNumber.From(3),
            AcademicTermStartDate.From(new DateTime(2025, 2, 1)),
            AcademicTermEndDate.From(new DateTime(2025, 5, 31)))
    ];
}
