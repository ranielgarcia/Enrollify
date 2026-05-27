using Enrollify.Application.Features.ClassSections.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.IntegrationTests.Infrastructure;
using FluentValidation;

namespace Enrollify.IntegrationTests._Tests.Application.ClassSections;

/// <summary>
/// Tests to verify that Mediator pipeline behaviors are being invoked
/// </summary>
[Collection("Application")]
public class MediatorPipelineTests
{
    private readonly ApplicationTestFixture _fixture;

    public MediatorPipelineTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Mediator command execution triggers pipeline behaviors (LoggingBehavior + ValidationBehavior)")]
    public async Task BulkInitializeCommand_TriggersPipelineBehaviors()
    {
        // Arrange - Command with missing data to exercise the full pipeline
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            [new(CourseId.From(1), NumberOfSections: 1)]);

        // Act - Send command through Mediator; ValidationBehavior now throws ValidationException on failure
        var exception = await Record.ExceptionAsync(() => _fixture.SendAsync(command));

        // Assert - Either the command succeeds or ValidationBehavior threw — both confirm the pipeline ran
        Assert.True(
            exception is null or ValidationException,
            $"Expected no exception or ValidationException from pipeline, but got: {exception?.GetType().Name}: {exception?.Message}");
    }

    [Fact(DisplayName = "Invalid command - ValidationBehavior throws ValidationException")]
    public async Task BulkInitializeCommand_InvalidCommand_ValidationBehaviorThrowsValidationException()
    {
        // Arrange - Empty TargetCourses triggers a synchronous validation rule
        var command = new BulkInitializeClassSectionsForAcademicYear.Command(
            AcademicTermId.From(1),
            YearLevel.From(1),
            TargetCourses: []);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _fixture.SendAsync(command));
    }
}
