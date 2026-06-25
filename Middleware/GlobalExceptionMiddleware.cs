using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions anywhere in the pipeline and returns a
    /// consistent JSON error response rather than leaking stack traces.
    ///
    /// Copilot helped identify that returning raw exceptions in production
    /// is a security risk; this middleware was added to fix that.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

                var payload = new
                {
                    error   = "An unexpected error occurred.",
                    detail  = _env.IsDevelopment() ? ex.Message : null,
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(payload,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            }
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
