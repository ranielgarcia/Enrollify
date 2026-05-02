using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.AcademicYearAggregate;

public class AcademicYear : EntityBase<AcademicYear, AcademicYearId>, IAggregateRoot, IAuditable
{
    private AcademicYear(){}

    public AcademicStartDate StartDate { get; private set; }
    public AcademicEndDate EndDate { get; private set; }


    public Year StartYear => Year.From(StartDate.Value.Year);
    public Year EndYear => Year.From(EndDate.Value.Year);

    public string AcademicYearTitle => $"AY {StartYear.Value}-{EndYear.Value}";


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


    public AcademicYear UpdateStartAndEndYear(AcademicStartDate start, AcademicEndDate end)
    {
        if (end.Value.Year != start.Value.Year + 1) throw new InvalidAcademicYearRangeException();
        if (start == StartDate && end == EndDate) return this;
        StartDate = start;
        EndDate = end;
        return this;
    }
}
