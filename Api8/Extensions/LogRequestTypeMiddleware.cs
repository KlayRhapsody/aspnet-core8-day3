namespace Api8.Extensions;

public class LogRequestTypeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogRequestTypeMiddleware> _logger;

    public LogRequestTypeMiddleware(RequestDelegate next, ILogger<LogRequestTypeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestType"] = context.Request.Method,
            ["RequestQueryString"] = context.Request.QueryString
        }))
        {
            await _next(context);
        }
    }
}