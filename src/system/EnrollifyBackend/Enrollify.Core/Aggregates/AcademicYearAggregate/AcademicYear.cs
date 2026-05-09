using Ardalis.GuardClauses;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.ValueObjects;
using Enrollify.SharedKernel;

namespace Enrollify.Core.Aggregates.AcademicYearAggregate;

public class AcademicYear : EntityBase<AcademicYear, AcademicYearId>, IAggregateRoot, IAuditable
{
    private readonly List<AcademicTerm> _academicTerms = new();
    private AcademicYear(){}

    public AcademicYear(AcademicYearStartDate start, AcademicYearEndDate end)
    {
        if (end.Value.Year != start.Value.Year + 1) throw new InvalidAcademicYearRangeException();
        StartDate = Guard.Against.Null(start);
        EndDate = Guard.Against.Null(end);
    }

    public AcademicYearStartDate StartDate { get; private set; }
    public AcademicYearEndDate EndDate { get; private set; }


    public Year StartYear => Year.From(StartDate.Value.Year);
    public Year EndYear => Year.From(EndDate.Value.Year);

    public string AcademicYearTitle => $"AY {StartYear.Value}-{EndYear.Value}";
    public string AcademicYearSlug => $"{StartYear.Value}-{EndYear.Value}";

    public IReadOnlyCollection<AcademicTerm> AcademicTerms => _academicTerms.AsReadOnly();

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


    public AcademicYear UpdateStartAndEndYear(AcademicYearStartDate start, AcademicYearEndDate end)
    {
        if (end.Value.Year != start.Value.Year + 1) throw new InvalidAcademicYearRangeException();
        if (start == StartDate && end == EndDate) return this;
        StartDate = Guard.Against.Null(start);
        EndDate = Guard.Against.Null(end);
        return this;
    }

    public AcademicYear AddTerm(TermNumber termNumber, AcademicTermStartDate startDate, AcademicTermEndDate endDate)
    {
        if (_academicTerms.Any((Func<AcademicTerm, bool>)(t => t.TermNumber == termNumber)))
        {
            throw new InvalidAcademicTermException($"Term number {termNumber.Value} already exists in this academic year.");
        }

        if (startDate.Value < StartDate.Value || endDate.Value > EndDate.Value)
        {
            throw new InvalidAcademicTermException($"Term dates must fall within the academic year range ({AcademicYearTitle}).");
        }

        // validate if the new term overlaps with existing terms
        foreach (var term in _academicTerms)
        {
            if (startDate.Value < term.EndDate.Value && endDate.Value > term.StartDate.Value)
            {
                throw new InvalidAcademicTermException($"The term dates overlap with existing term {term.TermNumber.Value}.");
            }
        }

        var newTerm = new AcademicTerm(termNumber, Id, startDate, endDate);
        _academicTerms.Add(newTerm);
        return this;
    }

    public AcademicYear UpdateTerm(TermNumber termNumber, AcademicTermStartDate startDate, AcademicTermEndDate endDate)
    {
        var term = _academicTerms.FirstOrDefault((Func<AcademicTerm, bool>)(t => t.TermNumber == termNumber));
        if (term == null)
        {
            throw new InvalidAcademicTermException($"No term found with number {termNumber.Value} in this academic year.");
        }

        if (startDate.Value < StartDate.Value || endDate.Value > EndDate.Value)
        {
            throw new InvalidAcademicTermException($"Term dates must fall within the academic year range ({AcademicYearTitle}).");
        }

        foreach (var other in _academicTerms.Where((Func<AcademicTerm, bool>)(t => t.TermNumber != termNumber)))
        {
            if (startDate.Value < other.EndDate.Value && endDate.Value > other.StartDate.Value)
            {
                throw new InvalidAcademicTermException($"The term dates overlap with previous/existing term {other.TermNumber.Value}.");
            }
        }

        term.UpdateTermDates(startDate, endDate);
        return this;
    }
}
