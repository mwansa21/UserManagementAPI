using System.Diagnostics;

namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware that logs every HTTP request and response, including
    /// method, path, status code, and elapsed time.
    ///
    /// Copilot suggestion: capture elapsed ms and log at Warning level
    /// when a request takes longer than 500 ms.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            _logger.LogInformation(
                "[REQUEST]  {Method} {Path} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await _next(context);

            sw.Stop();

            var level = sw.ElapsedMilliseconds > 500 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(level,
                "[RESPONSE] {Method} {Path} => {StatusCode} ({ElapsedMs} ms) | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                context.TraceIdentifier);
        }
    }

    // Extension method for clean registration in Program.cs
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
            => app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
