using System.ComponentModel.DataAnnotations;
using Enrollify.Core.Constants;
using Microsoft.Extensions.Options;

namespace Enrollify.Core.Models;

public class CurriculaSettings
{
    public static readonly string Key = nameof(CurriculaSettings);

    [Required(ErrorMessage = "AcademicSystem is required.")]
    [ValidAcademicSystem]
    public AcademicSystemEnum AcademicSystem { get; set; } = AcademicSystemEnum.Semester;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ValidAcademicSystemAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return new ValidationResult("AcademicSystem cannot be null.");
        }

        if (value is not AcademicSystemEnum academicSystem)
        {
            return new ValidationResult($"Invalid AcademicSystem type. Expected {nameof(AcademicSystemEnum)}.");
        }

        if (!AcademicSystemEnum.TryFromValue(academicSystem.Value, out _))
        {
            var validValues = string.Join(", ", AcademicSystemEnum.List.Select(e => e.Name));
            return new ValidationResult($"Invalid AcademicSystem value '{academicSystem.Name}'. Valid values are: {validValues}.");
        }

        return ValidationResult.Success;
    }
}

[OptionsValidator]
public partial class CurriculaSettingsValidator : IValidateOptions<CurriculaSettings>;
