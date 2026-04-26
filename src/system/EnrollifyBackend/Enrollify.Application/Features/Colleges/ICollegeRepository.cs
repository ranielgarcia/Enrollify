using Ardalis.Result;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.Application.Features.Colleges;

public interface ICollegeRepository
{
    Task<Result<CollegeId>> Create(College newCollege, CancellationToken cancellationToken);
    Task<Result<CollegeId>> Update(College updatedCollege, CancellationToken cancellationToken);
    Task<Result> Delete(CollegeId collegeId, CancellationToken cancellationToken);
}
