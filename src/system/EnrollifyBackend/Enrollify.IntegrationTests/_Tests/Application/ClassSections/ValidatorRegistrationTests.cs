using Enrollify.Application.Behaviors;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.Validators;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.IntegrationTests.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Enrollify.IntegrationTests._Tests.Application.ClassSections;

/// <summary>
/// Tests to verify that FluentValidation validators are properly registered in DI
/// </summary>
[Collection("Application")]
public class ValidatorRegistrationTests
{
    private readonly ApplicationTestFixture _fixture;

    public ValidatorRegistrationTests(ApplicationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "BulkInitializeClassSectionsForAcademicYearValidator is registered in DI")]
    public async Task BulkInitializeValidator_IsRegisteredInDI()
    {
        await _fixture.ExecuteInScopeAsync(sp =>
        {
            // Try to resolve the validator
            var validator = sp.GetService<IValidator<BulkInitializeClassSectionsForAcademicYear.Command>>();

            Assert.NotNull(validator);
            Assert.IsType<BulkInitializeClassSectionsForAcademicYearValidator>(validator);

            return Task.CompletedTask;
        });
    }

    [Fact(DisplayName = "IEnumerable<IValidator<Command>> resolves at least one validator")]
    public async Task BulkInitializeValidator_EnumerableResolvesValidator()
    {
        await _fixture.ExecuteInScopeAsync(sp =>
        {
            // Try to resolve as IEnumerable (this is what ValidationBehavior uses)
            var validators = sp.GetServices<IValidator<BulkInitializeClassSectionsForAcademicYear.Command>>().ToList();

            Assert.NotEmpty(validators);
            Assert.Single(validators);
            Assert.IsType<BulkInitializeClassSectionsForAcademicYearValidator>(validators[0]);

            return Task.CompletedTask;
        });
    }

    [Fact(DisplayName = "Validator can be manually invoked and throws ValidationException")]
    public async Task BulkInitializeValidator_ManualInvocation_ThrowsValidationException()
    {
        await _fixture.ExecuteInScopeAsync(async sp =>
        {
            // Get the validator
            var validator = sp.GetRequiredService<IValidator<BulkInitializeClassSectionsForAcademicYear.Command>>();

            // Create invalid command
            var command = new BulkInitializeClassSectionsForAcademicYear.Command(
                AcademicTermId.From(1),
                YearLevel.From(1),
                new List<BulkInitializeClassSectionsForAcademicYear.TargetCourse>());

            // Validate
            var result = await validator.ValidateAsync(command);

            // Assert validation failed
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("At least one course is required"));
        });
    }

    [Fact(DisplayName = "ValidationBehavior receives validators through constructor injection")]
    public async Task ValidationBehavior_ReceivesValidators()
    {
        await _fixture.ExecuteInScopeAsync(sp =>
        {
            // Verify validators can be resolved for injection into ValidationBehavior
            var validators = sp.GetServices<IValidator<BulkInitializeClassSectionsForAcademicYear.Command>>().ToList();

            Assert.NotEmpty(validators);
            Assert.Single(validators);

            return Task.CompletedTask;
        });
    }
}
