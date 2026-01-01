using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Beep.OilandGas.UserManagement.Core.Audit;
using Beep.OilandGas.UserManagement.Core.Authentication;

namespace Beep.OilandGas.UserManagement.AspNetCore.Middleware
{
    /// <summary>
    /// Middleware for auditing authorization decisions in ASP.NET Core
    /// </summary>
    public class AuthorizationAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuditLogger? _auditLogger;

        public AuthorizationAuditMiddleware(RequestDelegate next, IAuditLogger? auditLogger = null)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _auditLogger = auditLogger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log authorization attempts if audit logger is available
            if (_auditLogger != null && context.User?.Identity?.IsAuthenticated == true)
            {
                var userPrincipal = UserPrincipal.FromClaimsPrincipal(context.User);
                
                // Log the request
                await _auditLogger.LogDataAccessAsync(
                    userPrincipal.UserId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString.ToString(),
                    true);
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods for registering the audit middleware
    /// </summary>
    public static class AuthorizationAuditMiddlewareExtensions
    {
        /// <summary>
        /// Adds authorization audit middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseAuthorizationAudit(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationAuditMiddleware>();
        }
    }
}
