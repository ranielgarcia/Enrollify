using Ardalis.Specification.EntityFrameworkCore;
using Enrollify.SharedKernel;
using System;

namespace Enrollify.Infrastructure.Data;

// inherit from Ardalis.Specification type
public class EfRepository<T>(EnrollifyDbContext dbContext) :
  RepositoryBase<T>(dbContext), IReadRepository<T>, IRepository<T> where T : class, IAggregateRoot
{
}
