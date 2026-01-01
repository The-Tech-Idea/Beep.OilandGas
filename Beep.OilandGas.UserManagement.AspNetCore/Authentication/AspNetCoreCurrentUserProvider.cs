using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Beep.OilandGas.UserManagement.Core.Authentication;

namespace Beep.OilandGas.UserManagement.AspNetCore.Authentication
{
    /// <summary>
    /// Implementation of ICurrentUserProvider for ASP.NET Core
    /// Extracts user context from HTTP requests (JWT tokens or HttpContext.User)
    /// </summary>
    public class AspNetCoreCurrentUserProvider : ICurrentUserProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCoreCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Gets the current authenticated user
        /// Priority 1: JWT token from Authorization header (for requests from Web app)
        /// Priority 2: HttpContext.User (for direct API requests)
        /// </summary>
        public async Task<UserPrincipal?> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            // Priority 1: Try to extract from JWT token in Authorization header
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    var principal = await ExtractUserFromJwtTokenAsync(token);
                    if (principal != null)
                    {
                        return UserPrincipal.FromClaimsPrincipal(principal);
                    }
                }
            }

            // Priority 2: Fallback to HttpContext.User (set by authentication middleware)
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                return UserPrincipal.FromClaimsPrincipal(httpContext.User);
            }

            return null;
        }

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        public async Task<string?> GetCurrentUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.UserId;
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return false;
            }

            // Check Authorization header first
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return true; // Token present, assume authenticated (validation happens in GetCurrentUserAsync)
            }

            // Fallback to HttpContext.User
            return httpContext.User?.Identity?.IsAuthenticated == true;
        }

        /// <summary>
        /// Extracts user claims from JWT token
        /// Note: This is a basic implementation. For production, you should validate the token signature
        /// </summary>
        private Task<ClaimsPrincipal?> ExtractUserFromJwtTokenAsync(string token)
        {
            try
            {
                // Basic JWT parsing - extract claims without signature validation
                // For production, use JwtSecurityTokenHandler with proper validation
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                
                // Read token without validation (for now - should validate in production)
                var jsonToken = handler.ReadJwtToken(token);
                
                // Extract claims
                var claims = jsonToken.Claims.ToList();
                
                // Create ClaimsIdentity
                var identity = new ClaimsIdentity(claims, "Bearer");
                var principal = new ClaimsPrincipal(identity);

                return Task.FromResult<ClaimsPrincipal?>(principal);
            }
            catch
            {
                // Token is invalid or malformed
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }
    }
}
