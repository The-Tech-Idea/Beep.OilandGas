using Beep.OilandGas.Repository;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>Checks application installation, independently of named module datasources.</summary>
    public class SetupGateMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] ExemptPaths = ["/health", "/api/setup/repository", "/swagger", "/api/auth", "/.well-known"];

        public SetupGateMiddleware(RequestDelegate next){_next=next;}

        public async Task InvokeAsync(HttpContext context, IRepositoryReadinessService readiness)
        {
            if (ExemptPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }
            var status = await readiness.CheckAsync(context.RequestAborted);
            if (status != RepositoryReadiness.Ready)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Default repository setup is required.",
                    status = status.ToString(),
                    setupUrl = "/health/repository"
                }, context.RequestAborted);
                return;
            }
            await _next(context);
        }
    }
}
