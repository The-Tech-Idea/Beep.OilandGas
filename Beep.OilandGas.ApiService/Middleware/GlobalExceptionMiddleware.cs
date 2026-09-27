using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches unhandled exceptions from all downstream middleware and controllers,
    /// returning a consistent JSON error response instead of the default 500 page.
    ///
    /// Reduces the need for duplicated try/catch blocks across 70+ controllers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// DIAG-01. This is the API's app-wide handler, so every exception it takes is reported through the failure service
    /// (<see cref="IFailureReporter"/>): logged, and stored under a reference. It logged past the store, so a failure an
    /// operator was told about by its caller could not be found by anybody, and in development a 500 carried the
    /// exception's own text. A 500 now carries a sentence and the reference, whatever the environment.
    /// </para>
    /// <para>
    /// The 400 and 409 answers still carry the exception's message: they are how the controllers refuse today, and the Web
    /// shows those sentences. Turning those refusals into outcomes the controllers return is OILGAS-CATCH-01.
    /// </para>
    /// </remarks>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFailureReporter _failures;

        public GlobalExceptionMiddleware(RequestDelegate next, IFailureReporter failures)
        {
            _next = next;
            _failures = failures;
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
                var reference = _failures.ReportHandled(ex, Operation(context),
                    consequence: "answered 400 with the exception's message as the refusal", FailureSeverity.Degraded);
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Message, reference);
            }
            catch (InvalidOperationException ex)
            {
                var reference = _failures.ReportHandled(ex, Operation(context),
                    consequence: "answered 409 with the exception's message as the refusal", FailureSeverity.Degraded);
                await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Message, reference);
            }
            catch (UnauthorizedAccessException ex)
            {
                var reference = _failures.ReportHandled(ex, Operation(context),
                    consequence: "answered 403", FailureSeverity.Degraded);
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, "Access denied.", reference);
            }
            catch (Exception ex)
            {
                // Broad on purpose: this is the API's app-wide handler, and whatever reaches it is answered and reported.
                var statusCode = ex is NotImplementedException
                    ? HttpStatusCode.NotImplemented
                    : HttpStatusCode.InternalServerError;

                var reference = _failures.ReportHandled(ex, Operation(context),
                    consequence: $"answered {(int)statusCode}; the request was not completed");

                await WriteErrorResponse(context, statusCode,
                    $"An internal error occurred. If it keeps happening, quote reference {reference}.", reference);
            }
        }

        /// <summary>What was being attempted, for the store: the request, never its query string (it can carry values).</summary>
        private static string Operation(HttpContext context) =>
            $"handling {context.Request.Method} {context.Request.Path.Value}";

        private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string message, string reference)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var error = new
            {
                error = message,
                statusCode = (int)statusCode,
                reference,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(error));
        }
    }
}
