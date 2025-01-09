using Api8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api8.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const string CategoryName = "WFCon";
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _config;
    private readonly IOptionsSnapshot<AppSettingsOptions> _appSettings;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IOptionsSnapshot<AppSettingsOptions> appSettings)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _config = configuration;
        _appSettings = appSettings;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var logger = _loggerFactory.CreateLogger(CategoryName);
        var random = new Random();
        int eventIdValue = random.Next(1, int.MaxValue);
        var eventId = new EventId(eventIdValue, CategoryName);

        logger.LogTrace(eventId, "Trace log");
        logger.LogDebug(eventId, "Debug log");
        logger.LogInformation(eventId, "✅ Information log: {SmtpIp}, {SmtpPort}", 
            _appSettings.Value.SmtpIp,
            _appSettings.Value.SmtpPort);
        logger.LogWarning(eventId, "Warning log");
        logger.LogError(eventId, "Error log");
        logger.LogCritical(eventId, "Critical log");

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
