using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.AcademicYearAndTerm.Queries;

public class GetActiveAcademicYearQueryTests
{
    private readonly Mock<IReadRepository<AcademicYear>> _readRepositoryMock = new();
    private readonly GetActiveAcademicYearQueryHandler _handler;

    private static readonly AcademicYearStartDate ValidStart =
        AcademicYearStartDate.From(new DateTime(2024, 6, 1));
    private static readonly AcademicYearEndDate ValidEnd =
        AcademicYearEndDate.From(new DateTime(2025, 5, 31));

    public GetActiveAcademicYearQueryTests()
    {
        _handler = new GetActiveAcademicYearQueryHandler(_readRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_NoActiveAcademicYear_ReturnsNotFoundResult()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearExists_ReturnsSuccessResult()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearExists_ReturnsMappedDtoWithCorrectDates()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.Equal(ValidStart, result.Value.StartDate);
        Assert.Equal(ValidEnd, result.Value.EndDate);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearExists_ReturnsMappedDtoWithCorrectYears()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.Equal(2024, result.Value.StartYear.Value);
        Assert.Equal(2025, result.Value.EndYear.Value);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearExists_ReturnsMappedDtoWithCorrectTitle()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.Equal("AY 2024-2025", result.Value.AcademicYearTitle);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearWithNoTerms_ReturnsDtoWithEmptyTermsArray()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.Empty(result.Value.AcademicTerms);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearWithTerms_ReturnsDtoWithMappedTerms()
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

        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(year);

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.Equal(3, result.Value.AcademicTerms.Length);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearWithTerms_TermsHaveCorrectTermNumbers()
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

        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(year);

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        var termNumbers = result.Value.AcademicTerms.Select(t => t.TermNumber.Value).ToArray();
        Assert.Contains(1, termNumbers);
        Assert.Contains(2, termNumbers);
        Assert.Contains(3, termNumbers);
    }

    [Fact]
    public async Task Handle_ActiveAcademicYearExists_ReturnsAcademicYearDtoType()
    {
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<GetActiveAcademicYearSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AcademicYear(ValidStart, ValidEnd));

        var result = await _handler.Handle(new GetActiveAcademicYearQuery(), CancellationToken.None);

        Assert.IsType<AcademicYearDto>(result.Value);
    }
}
