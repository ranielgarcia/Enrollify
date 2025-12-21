using Ardalis.Result;
using Enrollify.Application.Colleges.DTOs;
using Mediator;

namespace Enrollify.Application.Colleges.Features;

public class ListCollegesQuery : IQuery<Result<List<CollegeDTO>>>
{
}

public class ListCollegesQueryHandler : IQueryHandler<ListCollegesQuery, Result<List<CollegeDTO>>>
{
    private readonly ICollegeRepository _repository;
    public ListCollegesQueryHandler(ICollegeRepository repository)
    {
        _repository = repository;
    }
    public async ValueTask<Result<List<CollegeDTO>>> Handle(ListCollegesQuery request, CancellationToken cancellationToken)
    {
        var colleges = await _repository.ListColleges(cancellationToken);
        var toReturn = colleges
            .Select(c => new CollegeDTO
            {
                Id = c.Id,
                Code = c.Code.Value,
                Name = c.Name,
                Description = c.Description,
                Dean = c.Dean,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                CreatedByUser = BaseUserDTO.FromUser(c.CreatedByUser),
                UpdatedByUser = BaseUserDTO.FromUser(c.UpdatedByUser),
                UpdatedAt = c.UpdatedAt,
                UpdatedBy = c.UpdatedBy,
                DeletedAt = c.DeletedAt,
                DeletedBy = c.DeletedBy,
                IsActive = c.IsActive
            }).ToList();
        return Result.Success(toReturn);
    }
}