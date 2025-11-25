using Enrollify.Core.RoomAggregate;
using Enrollify.Core.RoomTypeAggregate;
using Enrollify.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Enrollify.SharedKernel;

namespace Enrollify.WebAPI.Controllers;

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
    public IEnumerable<WeatherForecast> Get()
    {
        var roomType = new RoomType(RoomTypeName.From("Test3"));
        _dbContext.RoomTypes.Add(roomType);
        _dbContext.SaveChanges();

        var roomTypes = _dbContext.RoomTypes.ToList();

        var room = new Room(RoomName.From("Test"), 30, RoomTypeId.From(1));
        room.UpdateCapacity(10).AuditInfo.SetUpdatedBy()

        //_dbContext.Rooms.Add();

        var rooms = _dbContext.Rooms.ToList();

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
