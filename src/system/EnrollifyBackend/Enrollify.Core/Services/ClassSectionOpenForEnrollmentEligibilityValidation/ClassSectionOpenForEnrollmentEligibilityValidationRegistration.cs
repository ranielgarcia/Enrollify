using Microsoft.Extensions.DependencyInjection;

namespace Enrollify.Core.Services.ClassSectionOpenForEnrollmentEligibilityValidation;

public static class ClassSectionOpenForEnrollmentEligibilityValidationRegistration
{
  public static void RegisterClassSectionOpenForEnrollmentEligibilityValidationServices(
    this IServiceCollection services)
  {
    services.AddScoped<ClassSectionOpenForEnrollmentEligibilityValidationPipeline>();
    services
      .AddTransient<IClassSectionOpenForEnrollmentEligibilityValidationRule, Rules.ClassSectionAdviserIsRequiredRule>();
    services
      .AddTransient<IClassSectionOpenForEnrollmentEligibilityValidationRule,
        Rules.ClassSectionSubjectOfferingTeacherIsRequiredRule>();
  }
}
