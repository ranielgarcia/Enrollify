using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.Extensions;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.AcademicYearAggregate;

public class AcademicTerm : IAuditable
{
    private AcademicTerm() { } // EF Core constructor

    public AcademicTerm(TermNumber termNumber, AcademicYearId academicYearId, AcademicTermStartDate startDate, AcademicTermEndDate endDate)
    {
        TermNumber = Guard.Against.Null(termNumber);
        AcademicYearId = Guard.Against.Null(academicYearId);
        StartDate = Guard.Against.Null(startDate);
        EndDate = Guard.Against.Null(endDate);
    }

    public AcademicTermId Id { get; set; }

    public TermNumber TermNumber { get; private set; }

    public string TermName => $"{TermNumber.Value.ToOrdinal()} term";

    public AcademicYearId AcademicYearId { get; private set; }

    public AcademicTermStartDate StartDate { get; private set; }
    public AcademicTermEndDate EndDate { get; private set; }


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


    public AcademicTerm UpdateTermNumber (TermNumber termNumber)
    {
        if (termNumber == TermNumber) return this;
        TermNumber = termNumber;
        return this;
    }

    public AcademicTerm UpdateTermDates(AcademicTermStartDate startDate, AcademicTermEndDate endDate)
    {
        if (startDate.Value >= endDate.Value)
        {
            throw new InvalidAcademicTermException("Start date must be before end date.");
        }
        StartDate = startDate;
        EndDate = endDate;
        return this;
    }
}
