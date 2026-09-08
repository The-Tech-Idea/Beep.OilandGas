using Microsoft.AspNetCore.Http;
using TheTechIdea.Beep.Services.Telemetry.Context;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>
    /// Starts a BeepActivityScope for every HTTP request, providing W3C TraceContext-
    /// compatible correlation IDs (trace-id, span-id) that flow through the BeepDM
    /// TelemetryPipeline for structured log enrichment and audit chain grouping.
    ///
    /// Phase 1C of BeepDM framework integration.
    /// </summary>
    public class BeepTracingMiddleware
    {
        private readonly RequestDelegate _next;

        public BeepTracingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Start a Beep activity scope for this request.
            // The scope propagates trace-id and span-id to all downstream
            // BeepLog / BeepAudit calls via AsyncLocal.
            using (BeepActivityScope.Begin("http-request", new Dictionary<string, object>
            {
                ["method"] = context.Request.Method,
                ["path"] = context.Request.Path.Value ?? "/",
                ["traceparent"] = context.Request.Headers["traceparent"].FirstOrDefault() ?? ""
            }))
            {
                await _next(context);
            }
        }
    }
}
