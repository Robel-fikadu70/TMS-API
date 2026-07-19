namespace TmsApi.Api.Middlewares;

public class V1DeprecationMiddleware(RequestDelegate next)
{
    // The date V1 will stop working (Dec 31, 2026)
    private static readonly DateTimeOffset SunsetDate = 
        new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    public async Task InvokeAsync(HttpContext context)
    {
        // Response.OnStarting is the "last chance" to change headers 
        // before they are sent to the user's browser
        context.Response.OnStarting(() =>
        {
            // Only add headers if the user is calling a V1 endpoint
            if (context.Request.Path.StartsWithSegments("/api/v1"))
            {
                context.Response.Headers["Deprecation"] = "true";
                context.Response.Headers["Sunset"] = SunsetDate.ToString("R");
                
                // Link header tells them where the new version is
                context.Response.Headers["Link"] = 
                    $"<{context.Request.Scheme}://{context.Request.Host}/api/v2{context.Request.Path.Value?[7..]}>; rel=\"successor-version\"";
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}