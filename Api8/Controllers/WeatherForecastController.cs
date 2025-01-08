using Api8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api8.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _config;
    private readonly IOptionsSnapshot<AppSettingsOptions> _appSettings;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        IConfiguration configuration,
        IOptionsSnapshot<AppSettingsOptions> appSettings)
    {
        _logger = logger;
        _config = configuration;
        _appSettings = appSettings;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            Config = _config.GetValue<string>("AppSettings:SomeKey"),
            // ConnectionStrings = _config.GetConnectionString("DefaultConnection"),
            ConnectionStrings = _appSettings.Value.Smtp.Ip + ":" + _appSettings.Value.Smtp.Port
        })
        .ToArray();
    }
}
