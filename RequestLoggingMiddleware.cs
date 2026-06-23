using System.Diagnostics;

namespace TmsApi;

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
        // TODO: Generate correlation ID (8 characters from a GUID)
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        
        // TODO: Add header to response BEFORE calling next
        context.Response.Headers["X-Correlation-Id"] = correlationId;
        
        // TODO: Start stopwatch and log entry
        var stopwatch = Stopwatch.StartNew();
        
        var method = context.Request.Method;
        var path = context.Request.Path;
        
        _logger.LogInformation(
            "Request {CorrelationId}: {Method} {Path} started",
            correlationId, method, path
        );
        
        // TODO: Call the next middleware
        await _next(context);
        
        // TODO: Stop stopwatch and log exit
        stopwatch.Stop();
        var statusCode = context.Response.StatusCode;
        
        _logger.LogInformation(
            "Request {CorrelationId}: {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
            correlationId, method, path, statusCode, stopwatch.ElapsedMilliseconds
        );
    }
}
