using Ardalis.Result;
using Enrollify.Application.Features.Teachers.DTOs;
using Mediator;

namespace Enrollify.Application.Features.Teachers.Queries;

public record SearchTeachersPaginatedQuery(string? searchTerm, int page = 1, int pageSize = 10) : IQuery<Result<PagedResult<TeacherDto>>>;
