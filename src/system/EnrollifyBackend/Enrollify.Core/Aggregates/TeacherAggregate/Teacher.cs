using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate.Models;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

public class Teacher : EntityBase<Teacher, TeacherId>, IAggregateRoot, IAuditable
{
    private Teacher() { }

    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }

    public TeacherEmail Email { get; private set; }
    public TeacherPhoneNumber PhoneNumber { get; private set; }

    public DepartmentId DepartmentId { get; private set; }
    public Department? Department { get; }

    public string AcademicTitle { get; private set; }

    public TeacherPhoto Photo { get; private set; }

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


    public static Teacher Create(TeacherForCreation teacherForCreation)
    {
        var teacher = new Teacher();

        teacher
            .UpdateFirstName(teacherForCreation.FirstName)
            .UpdateMiddleName(teacherForCreation.MiddleName)
            .UpdateLastName(teacherForCreation.LastName)
            .UpdateEmail(teacherForCreation.Email)
            .UpdateDepartmentId(teacherForCreation.DepartmentId);

        return teacher;
    }

    public Teacher UpdateFirstName (string firstName)
    {
        if (firstName == FirstName) return this;
        FirstName = Guard.Against.Null(firstName, message: "First name is required.");
        return this;
    }

    public Teacher UpdateMiddleName(string middleName)
    {
        if (middleName == MiddleName) return this;
        MiddleName = Guard.Against.Null(middleName, message: "Middle name is required.");
        return this;
    }

    public Teacher UpdateLastName(string lastName)
    {
        if (lastName == LastName) return this;
        LastName = Guard.Against.Null(lastName, message: "Last name is required.");
        return this;
    }

    public Teacher UpdateAcademicTitle(string academicTitle)
    {
        if (academicTitle == AcademicTitle) return this;
        AcademicTitle = Guard.Against.Null(academicTitle, message: "Academic title is required.");
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

    public Teacher UpdatePhoto(string locationPath, string fileName)
    {
        if (locationPath == null || fileName == null) return this;
        Photo = new TeacherPhoto(locationPath, fileName);
        return this;
    }
}
