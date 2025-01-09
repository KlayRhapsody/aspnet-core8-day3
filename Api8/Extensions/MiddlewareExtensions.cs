namespace Api8.Extensions;

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseLogRequestType(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LogRequestTypeMiddleware>();
    }
}