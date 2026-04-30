using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.ValueObjects.Storage;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

public class Teacher : EntityBase<Teacher, TeacherId>, IAggregateRoot, IAuditable
{
    private readonly List<TeacherSubject> _subjects = new();

    private Teacher() { }

    public Teacher(
        string firstName,
        string? middleName,
        string lastName,
        TeacherIdentifier teacherIdentifier,
        TeacherEmail email,
        TeacherPhoneNumber phoneNumber,
        DepartmentId departmentId)
    {
        FirstName = Guard.Against.Null(firstName, message: "First name is required.");
        MiddleName = Guard.Against.Null(middleName, message: "Middle name is required.");
        LastName = Guard.Against.Null(lastName, message: "Last name is required.");
        TeacherIdentifier = Guard.Against.Null(teacherIdentifier, message: "Teacher identifier is required.");
        Email = Guard.Against.Null(email, message: "Email is required.");
        PhoneNumber = Guard.Against.Null(phoneNumber, message: "Phone number is required.");
        DepartmentId = Guard.Against.Null(departmentId, message: "Department ID is required.");
    }

    public string FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; }

    public TeacherIdentifier TeacherIdentifier { get; private set; }
    public TeacherEmail Email { get; private set; }
    public TeacherPhoneNumber PhoneNumber { get; private set; }

    public DepartmentId DepartmentId { get; private set; }
    public Department? Department { get; }

    public string? AcademicTitle { get; private set; }
    public string? Qualification { get; private set; }
    public string? Specialization { get; private set; }
    public string? OfficeLocation { get; private set; }
    public string? OfficeHours { get; private set; }
    public string? Biography { get; private set; }

    public TeacherPhoto? Photo { get; private set; }
    public IReadOnlyCollection<TeacherSubject> Subjects => _subjects.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }
    public UserId CreatedBy { get; private set; }
    public User? CreatedByUser { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public UserId? UpdatedBy { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public UserId? DeletedBy { get; private set; }
    public User? DeletedByUser { get; private set; }
    public bool IsActive { get; private set; }



    public Teacher UpdateFirstName (string firstName)
    {
        if (firstName == FirstName) return this;
        FirstName = Guard.Against.Null(firstName, message: "First name is required.");
        return this;
    }

    public Teacher UpdateMiddleName(string? middleName)
    {
        if (middleName == MiddleName) return this;
        MiddleName = middleName;
        return this;
    }

    public Teacher UpdateLastName(string lastName)
    {
        if (lastName == LastName) return this;
        LastName = Guard.Against.Null(lastName, message: "Last name is required.");
        return this;
    }

    public Teacher UpdateTeacherIdentifier(TeacherIdentifier identifier)
    {
        if (identifier == TeacherIdentifier) return this;
        TeacherIdentifier = Guard.Against.Null(identifier, message: "Teacher identifier is required");
        return this;
    }

    public Teacher UpdateEmail(TeacherEmail email)
    {
        if (email == Email) return this;
        Email = Guard.Against.Null(email, message: "Email is required.");
        return this;
    }

    public Teacher UpdatePhoneNumber(TeacherPhoneNumber phoneNumber)
    {
        if (phoneNumber == PhoneNumber) return this;
        PhoneNumber = Guard.Against.Null(phoneNumber, message: "Phone Number is required.");
        return this;
    }


    public Teacher UpdateDepartmentId(DepartmentId departmentId)
    {
        if (departmentId == DepartmentId) return this;
        DepartmentId = Guard.Against.Null(departmentId, message: "Department ID is required.");
        return this;
    }

    public Teacher UpdateAcademicTitle(string? academicTitle)
    {
        if (academicTitle == AcademicTitle) return this;
        AcademicTitle = academicTitle;
        return this;
    }

    public Teacher UpdateQualification(string? qualification)
    {
        if (qualification == Qualification) return this;
        Qualification = qualification;
        return this;
    }


    public Teacher UpdateSpecialization(string? specialization)
    {
        if (specialization == Specialization) return this;
        Specialization = specialization;
        return this;
    }


    public Teacher UpdateOfficeLocation(string? officeLocations)
    {
        if (officeLocations == OfficeLocation) return this;
        OfficeLocation = officeLocations;
        return this;
    }


    public Teacher UpdateOfficeHours(string? officeHours)
    {
        if (officeHours == OfficeHours) return this;
        OfficeHours = officeHours;
        return this;
    }

    public Teacher UpdateBiography(string? biography)
    {
        if (biography == Biography) return this;
        Biography = biography;
        return this;
    }

    public Teacher UpdatePhoto(FileName fileName, string contentType)
    {
        if (fileName == null || string.IsNullOrEmpty(contentType)) return this;
        Photo = new TeacherPhoto(fileName, contentType);
        return this;
    }


    public Teacher AddSubject(SubjectId subjectId)
    {
        Guard.Against.Null(subjectId, message: "Subject ID is required.");

        var teacherSubject = new TeacherSubject(Id, subjectId);
        _subjects.Add(teacherSubject);
        return this;
    }

    public Teacher RemoveSubject (SubjectId subjectId)
    {
        var teacherSubject = _subjects.FirstOrDefault(ts => ts.SubjectId == subjectId);
        if (teacherSubject != null)
        {
            _subjects.Remove(teacherSubject);
        }
        return this;
    }

    public IEnumerable<TeacherSubject> GetActiveSubjects () { return _subjects.Where(s => s.IsActive); }

}
