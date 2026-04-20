using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.DepartmentAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate.Models;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

public class Teacher : EntityBase<Teacher, TeacherId>, IAggregateRoot, IAuditable
{
    private Teacher() { }


    public static Teacher Create(TeacherForCreation teacherForCreation)
    {
        var teacher = new Teacher();

        teacher
            .UpdateFirstName(teacherForCreation.FirstName)
            .UpdateMiddleName(teacherForCreation.MiddleName)
            .UpdateLastName(teacherForCreation.LastName)
            .UpdateTeacherIdentifier(teacherForCreation.TeacherIdentifier)
            .UpdateEmail(teacherForCreation.Email)
            .UpdatePhoneNumber(teacherForCreation.PhoneNumber)
            .UpdateDepartmentId(teacherForCreation.DepartmentId)
            .UpdateAcademicTitle(teacherForCreation.AcademicTitle)
            .UpdateQualification(teacherForCreation.Qualification)
            .UpdateSpecialization(teacherForCreation.Specialization)
            .UpdateOfficeLocations(teacherForCreation.OfficeLocation)
            .UpdateOfficeHours(teacherForCreation.OfficeHours)
            .UpdateBiography(teacherForCreation.Biography);

        return teacher;
    }

    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }

    public TeacherIdentifier TeacherIdentifier { get; private set; }
    public TeacherEmail Email { get; private set; }
    public TeacherPhoneNumber PhoneNumber { get; private set; }

    public DepartmentId DepartmentId { get; private set; }
    public Department? Department { get; }

    public string AcademicTitle { get; private set; }
    public string Qualification { get; private set; }
    public string Specialization { get; private set; }
    public string OfficeLocation { get; private set; }
    public string OfficeHours { get; private set; }
    public string Biography { get; private set; }

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

    public Teacher UpdateAcademicTitle(string academicTitle)
    {
        if (academicTitle == AcademicTitle) return this;
        AcademicTitle = Guard.Against.Null(academicTitle, message: "Academic title is required.");
        return this;
    }

    public Teacher UpdateQualification(string qualification)
    {
        if (qualification == Qualification) return this;
        Qualification = Guard.Against.Null(qualification, message: "Qualification is required.");
        return this;
    }


    public Teacher UpdateSpecialization(string specialization)
    {
        if (specialization == Specialization) return this;
        Specialization = Guard.Against.Null(specialization, message: "Specialization is required.");
        return this;
    }


    public Teacher UpdateOfficeLocations(string officeLocations)
    {
        if (officeLocations == OfficeLocation) return this;
        OfficeLocation = Guard.Against.Null(officeLocations, message: "Office location is required.");
        return this;
    }


    public Teacher UpdateOfficeHours(string officeHours)
    {
        if (officeHours == OfficeHours) return this;
        OfficeHours = Guard.Against.Null(officeHours, message: "Office hours is required.");
        return this;
    }

    public Teacher UpdateBiography(string biography)
    {
        if (biography == Biography) return this;
        Biography = Guard.Against.Null(biography, message: "Biography is required.");
        return this;
    }

    public Teacher UpdatePhoto(string locationPath, string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(locationPath) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(contentType)) return this;
        Photo = new TeacherPhoto(locationPath, fileName, contentType);
        return this;
    }
}
