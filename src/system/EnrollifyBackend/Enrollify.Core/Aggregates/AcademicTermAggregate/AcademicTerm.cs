using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.AcademicTermAggregate;

public class AcademicTerm : EntityBase<AcademicTerm, AcademicTermId>, IAggregateRoot, IAuditable
{
    private AcademicTerm() { } // EF Core constructor


    public AcademicTermNumber TermNumber { get; private set; }

    public string TermName { get; private set; }

    public AcademicYearId AcademicYearId { get; private set; }


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


    public AcademicTerm UpdateTermNumber (AcademicTermNumber termNumber)
    {
        if (termNumber == TermNumber) return this;
        TermNumber = termNumber;
        return this;
    }

    public AcademicTerm UpdateTermName(string termName)
    {
        if (termName == TermName) return this;
        TermName = Guard.Against.NullOrWhiteSpace(termName);
        return this;
    }

}
