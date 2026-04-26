using Enrollify.Application.Features.Teachers.Commands;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.WebAPI.Utilities;
using Enrollify.WebAPI.Validators;

namespace Enrollify.WebAPI.Features.Teachers;

public class RegisterTeacherResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TeacherIdentifier { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string? AcademicTitle { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public string? OfficeLocation { get; set; }
    public string? OfficeHours { get; set; }
    public string? Biography { get; set; }
}

public class RegisterTeacherRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TeacherIdentifier { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string? AcademicTitle { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public string? OfficeLocation { get; set; }
    public string? OfficeHours { get; set; }
    public string? Biography { get; set; }
    public IFormFile? Photo { get; set; }
}

public class RegisterTeacherRequestValidator : Validator<RegisterTeacherRequest>
{
    public RegisterTeacherRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Please provide a first name.")
            .MaximumLength(100).WithMessage("First name must be 100 characters or fewer.");
        RuleFor(x => x.MiddleName)
            .NotEmpty().WithMessage("Please provide a middle name.")
            .MaximumLength(100).WithMessage("Middle name must be 100 characters or fewer.");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Please provide a last name.")
            .MaximumLength(100).WithMessage("Last name must be 100 characters or fewer.");
        RuleFor(x => x.TeacherIdentifier)
            .NotEmpty().WithMessage("Please provide a teacher identifier.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Please provide an email address.")
            .EmailAddress().WithMessage("Please provide a valid email address.");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Please provide a phone number.");
        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Please provide a valid department ID.");
        RuleFor(x => x.Photo)
            .Must(photo =>
            {
                if (photo is null) return true;
                var (errorOccurred, _) = ImageValidator.ValidateFile(photo);
                return !errorOccurred;
            }).WithMessage("Photo must be a valid image file (PNG or JPEG, max 5 MB).")
            .When(x => x.Photo is not null);
    }
}

[HttpPost("")]
[Group<TeacherEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateTeacherPermission)]
public class RegisterTeacherEndpoint : Endpoint<RegisterTeacherRequest, CreatedApiResult<RegisterTeacherResponse>>
{
    private readonly IMediator _mediator;

    public RegisterTeacherEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<RegisterTeacherResponse>>
        ExecuteAsync(RegisterTeacherRequest request, CancellationToken cancellationToken)
    {
        RegisterNewTeacher.TeacherPhoto? photo = null;

        if (request.Photo is not null)
        {
            var content = await FileUploadUtility.GetFileContentAsync(request.Photo);
            photo = new RegisterNewTeacher.TeacherPhoto(
                content,
                request.Photo.ContentType,
                request.Photo.FileName);
        }

        var result = await _mediator.Send(new RegisterNewTeacher.Command(
            new TeacherForCreation
            {
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                TeacherIdentifier = TeacherIdentifier.From(request.TeacherIdentifier),
                Email = TeacherEmail.From(request.Email),
                PhoneNumber = TeacherPhoneNumber.From(request.PhoneNumber),
                DepartmentId = DepartmentId.From(request.DepartmentId),
                AcademicTitle = request.AcademicTitle,
                Qualification = request.Qualification,
                Specialization = request.Specialization,
                OfficeLocation = request.OfficeLocation,
                OfficeHours = request.OfficeHours,
                Biography = request.Biography
            },
            photo), cancellationToken);

        return result.ToCreatedResult(
            id => $"/teachers/{id}",
            id => new RegisterTeacherResponse
            {
                Id = id.Value,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                TeacherIdentifier = request.TeacherIdentifier,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                DepartmentId = request.DepartmentId,
                AcademicTitle = request.AcademicTitle,
                Qualification = request.Qualification,
                Specialization = request.Specialization,
                OfficeLocation = request.OfficeLocation,
                OfficeHours = request.OfficeHours,
                Biography = request.Biography
            });
    }
}
