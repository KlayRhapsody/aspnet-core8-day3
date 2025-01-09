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

        
        _logger.LogTrace("Trace log");
        _logger.LogDebug("Debug log");
        _logger.LogInformation("✅ Information log: {SmtpIp}, {SmtpPort}", 
            _appSettings.Value.SmtpIp,
            _appSettings.Value.SmtpPort);
        _logger.LogWarning("Warning log");
        _logger.LogError("Error log");
        _logger.LogCritical("Critical log");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)],
            Config = _config.GetValue<string>("AppSettings:SomeKey"),
            // ConnectionStrings = _config.GetConnectionString("DefaultConnection"),
            ConnectionStrings = _appSettings.Value.SmtpIp + ":" + _appSettings.Value.SmtpPort
        })
        .ToArray();
    }
}
