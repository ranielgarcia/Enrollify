using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Models;

public record InitiateAcademicTerm(TermNumber TermNumber, AcademicTermStartDate StartDate, AcademicTermEndDate EndDate);
