using Enrollify.Application.Features.Teachers.Commands;
using Enrollify.Application.Features.Teachers.Models;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.WebAPI.Utilities;
using Enrollify.WebAPI.Validators;

namespace Enrollify.WebAPI.Features.Teachers;

public class UpdateTeacherResponse
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

public class UpdateTeacherRequest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
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

public class UpdateTeacherRequestValidator : Validator<UpdateTeacherRequest>
{
    public UpdateTeacherRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid teacher ID.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Please provide a first name.")
            .MaximumLength(100).WithMessage("First name must be 100 characters or fewer.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Middle name must be 100 characters or fewer.")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

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

[HttpPut("{id:int}")]
[Group<TeacherEndpointGroup>]
[AllowFileUploads]
[Authorize(Policy = PolicyName.HasUpdateTeacherPermission)]
public class UpdateTeacherEndpoint : Endpoint<UpdateTeacherRequest, OkOrNotFoundApiResult<UpdateTeacherResponse>>
{
    private readonly IMediator _mediator;

    public UpdateTeacherEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OkOrNotFoundApiResult<UpdateTeacherResponse>>
        ExecuteAsync(UpdateTeacherRequest request, CancellationToken ct)
    {
        UpdateTeacherDetails.TeacherPhoto? photo = null;

        if (request.Photo is not null)
        {
            var content = await FileUploadUtility.GetFileContentAsync(request.Photo);
            photo = new UpdateTeacherDetails.TeacherPhoto(
                content,
                request.Photo.ContentType,
                request.Photo.FileName);
        }

        var result = await _mediator.Send(new UpdateTeacherDetails.Command(
            new TeacherForUpdate
            {
                Id = TeacherId.From(request.Id),
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
            photo), ct);

        return result.ToUpdateResult(
            id => new UpdateTeacherResponse
            {
                Id = id.Value,
                FirstName = request.FirstName,
                MiddleName = request.MiddleName ?? string.Empty,
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
