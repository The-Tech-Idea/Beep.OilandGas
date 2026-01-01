using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.IdentityModel.Tokens;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of IAuthService for authentication operations
    /// Note: JWT generation is basic - can be enhanced to integrate with IdentityServer
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly string? _jwtSecret;
        private readonly string? _jwtIssuer;
        private readonly string? _jwtAudience;

        public AuthService(
            IUserService userService,
            string? jwtSecret = null,
            string? jwtIssuer = null,
            string? jwtAudience = null)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtSecret = jwtSecret ?? "your-secret-key-change-in-production-min-32-chars";
            _jwtIssuer = jwtIssuer ?? "Beep.OilandGas";
            _jwtAudience = jwtAudience ?? "beep-api";
        }

        public async Task<USER?> ValidateCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
                return null;

            if (!user.IS_ACTIVE)
                return null;

            var isValid = await _userService.CheckPasswordAsync(user, password);
            return isValid ? user : null;
        }

        public async Task<string> GenerateJwtAsync(USER user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Get user roles
            var roles = await _userService.GetRolesAsync(user.USER_ID);

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.USER_ID),
                new Claim(ClaimTypes.Name, user.USERNAME),
                new Claim(ClaimTypes.Email, user.EMAIL ?? string.Empty)
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create JWT token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<bool> SignOutAsync(string userId)
        {
            // For now, just return true
            // In a full implementation, you might:
            // - Invalidate token in cache/blacklist
            // - Clear session data
            // - Log sign-out event
            return Task.FromResult(true);
        }
    }
}
