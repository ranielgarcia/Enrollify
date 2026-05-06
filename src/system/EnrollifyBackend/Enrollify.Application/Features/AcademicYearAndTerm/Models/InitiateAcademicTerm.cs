using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Models;

public record InitiateAcademicTerm(AcademicTermNumber TermNumber, AcademicTermStartDate StartDate, AcademicTermEndDate EndDate);
