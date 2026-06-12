using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Generate a short Correlation ID (e.g., first 8 chars of a GUID)
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        // 2. Add the header BEFORE processing the request
        // This ensures the ID is present even if the request fails later
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        // 3. Start timing
        var stopwatch = Stopwatch.StartNew();

        // 4. Log Entry
        _logger.LogInformation(
            "Request Started: {Method} {Path} | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId
        );

        try
        {
            // 5. Pass control to the next middleware (the actual endpoint)
            await _next(context);
        }
        finally
        {
            // 6. Stop timing
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // 7. Log Exit with status code and time
            _logger.LogInformation(
                "Request Completed: {Method} {Path} | Status: {StatusCode} | Time: {ElapsedMs}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsedMs,
                correlationId
            );
        }
    }
}
