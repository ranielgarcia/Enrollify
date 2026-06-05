using Ardalis.Result;
using Ardalis.Specification;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.AcademicYearAndTerm.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.AcademicYearAndTerm.Commands;

public class DeleteAcademicYearAndTermsTests
{
    private readonly Mock<IAcademicYearAndTermRepository> _repositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _readRepositoryMock = new();
    private readonly Mock<IReadRepository<ClassSection>> _classSectionReadRepositoryMock = new();
    private readonly DeleteAcademicYearAndTerms.Handler _handler;

    private static readonly AcademicYearStartDate ValidStart =
        AcademicYearStartDate.From(DateTime.UtcNow.AddMonths(1));
    private static readonly AcademicYearEndDate ValidEnd =
        AcademicYearEndDate.From(DateTime.UtcNow.AddYears(1));

    public DeleteAcademicYearAndTermsTests()
    {
        _handler = new DeleteAcademicYearAndTerms.Handler(
            _repositoryMock.Object,
            _readRepositoryMock.Object,
            _classSectionReadRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_AcademicYearNotFound_ReturnsNotFoundResult()
    {
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AcademicYearId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(99));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_AcademicYearNotFound_DoesNotCallDelete()
    {
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AcademicYearId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AcademicYear?)null);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(99));

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Delete(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AcademicYearFound_CallsDeleteWithCorrectEntity()
    {
        var existingYear = new AcademicYear(ValidStart, ValidEnd);
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingYear);
        _repositoryMock
            .Setup(r => r.Delete(existingYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.Delete(existingYear, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AcademicYearFound_DeleteSucceeds_ReturnsSuccess()
    {
        var existingYear = new AcademicYear(ValidStart, ValidEnd);
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingYear);
        _repositoryMock
            .Setup(r => r.Delete(existingYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_AcademicYearFound_DeleteFails_PropagatesErrorResult()
    {
        var existingYear = new AcademicYear(ValidStart, ValidEnd);
        var errorResult = Result.Error("Database failure");
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingYear);
        _repositoryMock
            .Setup(r => r.Delete(existingYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
    }

    [Fact(DisplayName = "Handle should return Invalid when attempting to delete past academic year")]
    public async Task Handle_PastAcademicYear_ReturnsInvalidResult()
    {
        var pastStart = AcademicYearStartDate.From(DateTime.UtcNow.AddYears(-2));
        var pastEnd = AcademicYearEndDate.From(DateTime.UtcNow.AddYears(-1));
        var pastYear = new AcademicYear(pastStart, pastEnd);
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastYear);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Contains("past academic year", result.ValidationErrors.First().ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Handle should not call Delete when academic year is in the past")]
    public async Task Handle_PastAcademicYear_DoesNotCallDelete()
    {
        var pastStart = AcademicYearStartDate.From(DateTime.UtcNow.AddYears(-2));
        var pastEnd = AcademicYearEndDate.From(DateTime.UtcNow.AddYears(-1));
        var pastYear = new AcademicYear(pastStart, pastEnd);
        _readRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AcademicYearId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pastYear);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Delete(It.IsAny<AcademicYear>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "Handle should return Invalid when academic year ended yesterday")]
    public async Task Handle_AcademicYearEndedYesterday_ReturnsInvalidResult()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var startDate = AcademicYearStartDate.From(yesterday.AddYears(-1));
        var endDate = AcademicYearEndDate.From(yesterday);
        var expiredYear = new AcademicYear(startDate, endDate);
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredYear);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact(DisplayName = "Handle should allow delete when academic year ends today")]
    public async Task Handle_AcademicYearEndsToday_AllowsDelete()
    {
        var today = DateTime.UtcNow.Date;
        var startDate = AcademicYearStartDate.From(today.AddYears(-1));
        var endDate = AcademicYearEndDate.From(today);
        var currentYear = new AcademicYear(startDate, endDate);
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentYear);
        _repositoryMock
            .Setup(r => r.Delete(currentYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(
            r => r.Delete(currentYear, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handle should allow delete when academic year ends tomorrow")]
    public async Task Handle_AcademicYearEndsTomorrow_AllowsDelete()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var startDate = AcademicYearStartDate.From(tomorrow.AddYears(-1));
        var endDate = AcademicYearEndDate.From(tomorrow);
        var futureYear = new AcademicYear(startDate, endDate);
        _readRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<AcademicYear>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(futureYear);
        _repositoryMock
            .Setup(r => r.Delete(futureYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(
            r => r.Delete(futureYear, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
