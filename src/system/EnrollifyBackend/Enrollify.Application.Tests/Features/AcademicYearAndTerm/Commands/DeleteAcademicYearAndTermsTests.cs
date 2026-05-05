using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm;
using Enrollify.Application.Features.AcademicYearAndTerm.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using Moq;

namespace Enrollify.Application.Tests.Features.AcademicYearAndTerm.Commands;

public class DeleteAcademicYearAndTermsTests
{
    private readonly Mock<IAcademicYearAndTermRepository> _repositoryMock = new();
    private readonly Mock<IReadRepository<AcademicYear>> _readRepositoryMock = new();
    private readonly DeleteAcademicYearAndTerms.Handler _handler;

    private static readonly AcademicYearStartDate ValidStart =
        AcademicYearStartDate.From(new DateTime(2024, 6, 1));
    private static readonly AcademicYearEndDate ValidEnd =
        AcademicYearEndDate.From(new DateTime(2025, 5, 31));

    public DeleteAcademicYearAndTermsTests()
    {
        _handler = new DeleteAcademicYearAndTerms.Handler(
            _repositoryMock.Object,
            _readRepositoryMock.Object);
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
            .Setup(r => r.GetByIdAsync(It.IsAny<AcademicYearId>(), It.IsAny<CancellationToken>()))
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
            .Setup(r => r.GetByIdAsync(It.IsAny<AcademicYearId>(), It.IsAny<CancellationToken>()))
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
            .Setup(r => r.GetByIdAsync(It.IsAny<AcademicYearId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingYear);
        _repositoryMock
            .Setup(r => r.Delete(existingYear, It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        var command = new DeleteAcademicYearAndTerms.Command(AcademicYearId.From(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
    }
}
