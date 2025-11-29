using Enrollify.Core.Aggregates.PermissionsAggregate;
using Enrollify.Core.Aggregates.RoleAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.UserAggregate;
using Enrollify.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAggregate = Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    public WeatherForecastController(EnrollifyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
    private readonly EnrollifyDbContext _dbContext;

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get()
    {
        //var roomType = new RoomType(RoomTypeName.From("Test7"));
        //roomType.AuditInfo.SetCreatedBy(UserId.From(1));
        //_dbContext.RoomTypes.Add(roomType);
        //_dbContext.SaveChanges();

        //var roomTypes = _dbContext.RoomTypes.ToList();

        //var room = new Room(RoomName.From("Test"), RoomStudentCapacity.From(30), roomType.Id);
        //room.AuditInfo.SetCreatedBy(UserId.From(1));
        //room.UpdateCapacity(RoomStudentCapacity.From(35)).AuditInfo.SetUpdatedBy(UserId.From(1));

        //_dbContext.Rooms.Add(room);
        //_dbContext.SaveChanges();

        //var permission = Permission.Create(new Core.Aggregates.RoleAggregate.Models.PermissionForCreation
        //{
        //    Name = Guid.NewGuid().ToString(),
        //    Resource = Guid.NewGuid().ToString(),
        //    Action = Guid.NewGuid().ToString(),
        //    Description = Guid.NewGuid().ToString()
        //}, UserId.From(1));
        //_dbContext.Permissions.Add(permission);
        //await _dbContext.SaveChangesAsync();

        //var role = new Role(RoleName.From(Guid.NewGuid().ToString()), RoleDescription.From(Guid.NewGuid().ToString()));
        //role.AuditInfo.SetCreatedBy(UserId.From(1));
        //role.AddPermission(permission.Id, UserId.From(1));
        //_dbContext.Roles.Add(role);
        //_dbContext.SaveChanges();

        var user = UserAggregate.User.Create(
            new UserAggregate.Models.UserForCreation { Email = "John2.Doe@gmail.com", FirstName = "John", LastName = "Doe" }, UserAggregate.UserId.From(1));
        user.AssignRole(RoleId.From(1), UserId.From(1));
        user.AuditInfo.SetCreatedBy(UserId.From(1));

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        //var rooms = _dbContext.Rooms.ToList();

        //var roles = _dbContext.Roles.Include(r => r.RolePermissions).ToList();

        var users = _dbContext.Users.Include(u => u.RoleAssignments).ToList();

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
