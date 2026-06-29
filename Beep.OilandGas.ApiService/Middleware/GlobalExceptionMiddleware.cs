using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches unhandled exceptions from all downstream middleware and controllers,
    /// returning a consistent JSON error response instead of the default 500 page.
    ///
    /// Reduces the need for duplicated try/catch blocks across 70+ controllers.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                // Client disconnected — not a server error. Let it propagate naturally.
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized: {Message}", ex.Message);
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, "Access denied.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

                var statusCode = ex is NotImplementedException
                    ? HttpStatusCode.NotImplemented
                    : HttpStatusCode.InternalServerError;

                await WriteErrorResponse(context, statusCode,
                    context.RequestServices.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>()?.IsDevelopment() == true
                        ? $"{ex.GetType().Name}: {ex.Message}"
                        : "An internal error occurred. Please contact support.");
            }
        }

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var error = new
            {
                error = message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
