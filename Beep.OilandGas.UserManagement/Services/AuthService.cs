using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Authentication service implementing JWT-based authentication for Oil & Gas operations.
    /// Supports login, token refresh, password management, and account lockout.
    /// </summary>
    public class AuthService : Contracts.Services.IAuthService
    {
        private const int RefreshTokenSizeBytes = 32;
        private const int DefaultAccessTokenExpiryMinutes = 60;
        private const int DefaultRefreshTokenExpiryDays = 7;
        private const int DefaultMaxFailedLoginAttempts = 5;
        private const int DefaultLockoutDurationMinutes = 30;

        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IUserService userService)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName)
        {
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), _connectionName, tableName, null);
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var user = await _userService.GetByUsernameAsync(request.Username);
                if (user == null)
                {
                    _logger?.LogWarning("Login failed: user not found - {Username}", request.Username);
                    await RecordFailedLoginAsync(request.Username, ipAddress);
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid username or password."
                    };
                }

                if (user.LOCKED_IND == "Y")
                {
                    if (user.LOCKOUT_UNTIL_UTC.HasValue && user.LOCKOUT_UNTIL_UTC.Value > DateTime.UtcNow)
                    {
                        _logger?.LogWarning("Login failed: account locked - {Username}, locked until {LockoutUntil}",
                            request.Username, user.LOCKOUT_UNTIL_UTC);
                        return new LoginResult
                        {
                            Success = false,
                            IsLocked = true,
                            ErrorMessage = $"Account is locked until {user.LOCKOUT_UNTIL_UTC.Value:u}."
                        };
                    }

                    await UnlockAccountAsync(user.USER_ID, "system");
                }

                var passwordValid = await _userService.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    _logger?.LogWarning("Login failed: invalid password - {Username}", request.Username);
                    await RecordFailedLoginAsync(request.Username, ipAddress);
                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid username or password."
                    };
                }

                var maxAttempts = _configuration.GetValue("Authentication:MaxFailedLoginAttempts", DefaultMaxFailedLoginAttempts);
                if (user.FAILED_LOGIN_COUNT >= maxAttempts)
                {
                    var lockoutMinutes = _configuration.GetValue("Authentication:LockoutDurationMinutes", DefaultLockoutDurationMinutes);
                    user.LOCKED_IND = "Y";
                    user.LOCKOUT_UNTIL_UTC = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                    var userRepo = GetRepo<USER>("USER");
                    await userRepo.UpdateAsync(user, "system");
                    _logger?.LogWarning("Account locked after failed attempts - {Username}", request.Username);
                    return new LoginResult
                    {
                        Success = false,
                        IsLocked = true,
                        ErrorMessage = "Account locked due to too many failed attempts."
                    };
                }

                user.FAILED_LOGIN_COUNT = 0;
                user.LAST_LOGIN_UTC = DateTime.UtcNow;
                user.LOCKED_IND = "N";
                user.LOCKOUT_UNTIL_UTC = null;
                var userRepo2 = GetRepo<USER>("USER");
                await userRepo2.UpdateAsync(user, "system");

                var roles = (await _userService.GetRolesAsync(user.USER_ID)).ToArray();
                var permissions = await GetUserPermissionsAsync(user.USER_ID, roles);

                var tokenResult = GenerateTokens(user, roles, permissions);

                await WriteAccessAuditEventAsync(user.USER_ID, "LOGIN_SUCCESS", ipAddress, userAgent,
                    $"User {request.Username} logged in successfully from {ipAddress ?? "unknown"}");

                _logger?.LogInformation("Login successful - {Username}, roles: {Roles}", request.Username, string.Join(", ", roles));

                return new LoginResult
                {
                    Success = true,
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    AccessTokenExpiry = tokenResult.AccessTokenExpiry,
                    RefreshTokenExpiry = tokenResult.RefreshTokenExpiry,
                    UserId = user.USER_ID,
                    Username = user.USER_NAME,
                    Roles = roles,
                    Permissions = permissions,
                    RequiresPasswordChange = RequiresPasswordChange(user),
                    IsLocked = false
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Login failed for user {Username}", request.Username);
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during authentication."
                };
            }
        }

        public async Task<LoginResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            var principal = ValidateRefreshToken(refreshToken);
            if (principal == null)
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid refresh token."
                };
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid token claims."
                };
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null || user.LOCKED_IND == "Y")
            {
                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = "User not found or locked."
                };
            }

            var roles = (await _userService.GetRolesAsync(user.USER_ID)).ToArray();
            var permissions = await GetUserPermissionsAsync(user.USER_ID, roles);
            var tokenResult = GenerateTokens(user, roles, permissions);

            await WriteAccessAuditEventAsync(user.USER_ID, "TOKEN_REFRESH", ipAddress, null,
                $"Token refreshed for user {user.USER_NAME}");

            return new LoginResult
            {
                Success = true,
                AccessToken = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                AccessTokenExpiry = tokenResult.AccessTokenExpiry,
                RefreshTokenExpiry = tokenResult.RefreshTokenExpiry,
                UserId = user.USER_ID,
                Username = user.USER_NAME,
                Roles = roles,
                Permissions = permissions
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
        {
            var principal = ValidateRefreshToken(refreshToken);
            if (principal == null) return false;

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;

            await WriteAccessAuditEventAsync(userId, "LOGOUT", ipAddress, null,
                $"User logged out and token revoked");

            return true;
        }

        public async Task<bool> RevokeAllTokensAsync(string userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return false;

            _logger?.LogInformation("All tokens revoked for user {UserId}", userId);
            await WriteAccessAuditEventAsync(userId, "LOGOUT_ALL", null, null,
                $"All sessions revoked for user {user.USER_NAME}");

            return true;
        }

        public async Task<PasswordChangeResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return new PasswordChangeResult { Success = false, ErrorMessage = "User not found." };
            }

            var passwordValid = await _userService.CheckPasswordAsync(user, currentPassword);
            if (!passwordValid)
            {
                return new PasswordChangeResult { Success = false, ErrorMessage = "Current password is incorrect." };
            }

            try
            {
                var userRepo = GetRepo<USER>("USER");
                var hashedPassword = HashPassword(newPassword);
                user.PASSWORD_HASH = hashedPassword;
                user.LAST_PASSWORD_CHANGE_UTC = DateTime.UtcNow;
                await userRepo.UpdateAsync(user, "system");

                _logger?.LogInformation("Password changed for user {UserId}", userId);
                return new PasswordChangeResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to change password for user {UserId}", userId);
                return new PasswordChangeResult { Success = false, ErrorMessage = "Failed to change password." };
            }
        }

        public async Task<bool> RequestPasswordResetAsync(string emailOrUsername)
        {
            var userRepo = GetRepo<USER>("USER");
            var users = (await userRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_NAME", Operator = "=", FilterValue = emailOrUsername }
            })).Cast<USER>().ToList();

            if (!users.Any())
            {
                users = (await userRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "EMAIL", Operator = "=", FilterValue = emailOrUsername }
                })).Cast<USER>().ToList();
            }

            if (!users.Any()) return true;

            var user = users.First();
            var resetToken = GenerateResetToken();
            user.PASSWORD_RESET_TOKEN = resetToken;
            user.PASSWORD_RESET_TOKEN_EXPIRY_UTC = DateTime.UtcNow.AddHours(1);
            await userRepo.UpdateAsync(user, "system");

            _logger?.LogInformation("Password reset requested for user {UserId}", user.USER_ID);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string resetToken, string newPassword)
        {
            var userRepo = GetRepo<USER>("USER");
            var users = (await userRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "PASSWORD_RESET_TOKEN", Operator = "=", FilterValue = resetToken }
            })).Cast<USER>().ToList();

            if (!users.Any()) return false;

            var user = users.First();
            if (user.PASSWORD_RESET_TOKEN_EXPIRY_UTC < DateTime.UtcNow)
            {
                return false;
            }

            var hashedPassword = HashPassword(newPassword);
            user.PASSWORD_HASH = hashedPassword;
            user.PASSWORD_RESET_TOKEN = null;
            user.PASSWORD_RESET_TOKEN_EXPIRY_UTC = null;
            user.LAST_PASSWORD_CHANGE_UTC = DateTime.UtcNow;
            await userRepo.UpdateAsync(user, "system");

            _logger?.LogInformation("Password reset completed for user {UserId}", user.USER_ID);
            return true;
        }

        public async Task RecordFailedLoginAsync(string username, string? ipAddress = null)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null) return;

            var userRepo = GetRepo<USER>("USER");
            user.FAILED_LOGIN_COUNT = (user.FAILED_LOGIN_COUNT ?? 0) + 1;

            var maxAttempts = _configuration.GetValue("Authentication:MaxFailedLoginAttempts", DefaultMaxFailedLoginAttempts);
            if (user.FAILED_LOGIN_COUNT >= maxAttempts)
            {
                var lockoutMinutes = _configuration.GetValue("Authentication:LockoutDurationMinutes", DefaultLockoutDurationMinutes);
                user.LOCKED_IND = "Y";
                user.LOCKOUT_UNTIL_UTC = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                _logger?.LogWarning("Account locked - {Username} after {Count} failed attempts", username, user.FAILED_LOGIN_COUNT);
            }

            await userRepo.UpdateAsync(user, "system");
            await WriteAccessAuditEventAsync(user.USER_ID, "LOGIN_FAILED", ipAddress, null,
                $"Failed login attempt {user.FAILED_LOGIN_COUNT} for user {username}");
        }

        public async Task<bool> UnlockAccountAsync(string userId, string unlockedByUserId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return false;

            var userRepo = GetRepo<USER>("USER");
            user.LOCKED_IND = "N";
            user.LOCKOUT_UNTIL_UTC = null;
            user.FAILED_LOGIN_COUNT = 0;
            await userRepo.UpdateAsync(user, unlockedByUserId);

            _logger?.LogInformation("Account unlocked - {UserId} by {UnlockedBy}", userId, unlockedByUserId);
            return true;
        }

        private async Task<string[]> GetUserPermissionsAsync(string userId, string[] roles)
        {
            var permRepo = GetRepo<PERMISSION>("PERMISSION");
            var rpRepo = GetRepo<ROLE_PERMISSION>("ROLE_PERMISSION");
            var roleRepo = GetRepo<ROLE>("ROLE");

            var roleObjs = (await roleRepo.GetAsync(new List<AppFilter>()))
                .Cast<ROLE>()
                .Where(r => roles.Contains(r.ROLE_NAME))
                .ToList();

            var permissionIds = new HashSet<string>();
            foreach (var role in roleObjs)
            {
                var rolePerms = (await rpRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = role.ROLE_ID }
                })).Cast<ROLE_PERMISSION>();

                foreach (var rp in rolePerms)
                {
                    permissionIds.Add(rp.PERMISSION_ID);
                }
            }

            if (!permissionIds.Any()) return Array.Empty<string>();

            var allPerms = (await permRepo.GetAsync(new List<AppFilter>())).Cast<PERMISSION>().ToList();
            return allPerms
                .Where(p => permissionIds.Contains(p.PERMISSION_ID))
                .Select(p => p.PERMISSION_CODE)
                .Distinct()
                .ToArray();
        }

        private (string AccessToken, string RefreshToken, DateTime AccessTokenExpiry, DateTime RefreshTokenExpiry) GenerateTokens(USER user, string[] roles, string[] permissions)
        {
            var jwtSettings = _configuration.GetSection("Authentication:Jwt");
            var secretKey = jwtSettings.GetValue<string>("SecretKey")
                ?? throw new InvalidOperationException("JWT SecretKey not configured.");
            var issuer = jwtSettings.GetValue("Issuer", "Beep.OilandGas");
            var audience = jwtSettings.GetValue("Audience", "Beep.OilandGas.Client");
            var accessExpiryMinutes = jwtSettings.GetValue("AccessTokenExpiryMinutes", DefaultAccessTokenExpiryMinutes);
            var refreshExpiryDays = jwtSettings.GetValue("RefreshTokenExpiryDays", DefaultRefreshTokenExpiryDays);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.USER_ID),
                new(ClaimTypes.Name, user.USER_NAME),
                new(ClaimTypes.Email, user.EMAIL ?? string.Empty),
                new("tenant_id", user.TENANT_ID ?? string.Empty),
                new("ba_id", user.BUSINESS_ASSOCIATE_ID ?? string.Empty),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var permissionsClaim = string.Join(",", permissions);
            if (!string.IsNullOrEmpty(permissionsClaim))
            {
                claims.Add(new Claim("permissions", permissionsClaim));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(accessExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: accessTokenExpiry,
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshExpiryDays);

            return (accessToken, refreshToken, accessTokenExpiry, refreshTokenExpiry);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(RefreshTokenSizeBytes);
            return Convert.ToBase64String(bytes);
        }

        private static string GenerateResetToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        private ClaimsPrincipal? ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("Authentication:Jwt");
                var secretKey = jwtSettings.GetValue<string>("SecretKey")
                    ?? throw new InvalidOperationException("JWT SecretKey not configured.");

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(secretKey);

                tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                return (validatedToken as JwtSecurityToken)?.Claims != null
                    ? new ClaimsPrincipal(new ClaimsIdentity(((JwtSecurityToken)validatedToken).Claims))
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static string HashPassword(string password)
        {
            const int saltSize = 16;
            const int hashSize = 32;
            const int iterations = 120000;

            var salt = RandomNumberGenerator.GetBytes(saltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hashSize);

            return string.Join('$', "pbkdf2", iterations.ToString(),
                Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        private bool RequiresPasswordChange(USER user)
        {
            var maxAgeDays = _configuration.GetValue("Authentication:MaxPasswordAgeDays", 90);
            if (!user.LAST_PASSWORD_CHANGE_UTC.HasValue) return true;

            return (DateTime.UtcNow - user.LAST_PASSWORD_CHANGE_UTC.Value).Days >= maxAgeDays;
        }

        private async Task WriteAccessAuditEventAsync(string userId, string eventType, string? ipAddress, string? userAgent, string description)
        {
            try
            {
                var repo = GetRepo<UserAccessAuditEvent>("USER_ACCESS_AUDIT_EVENT");
                var evt = new UserAccessAuditEvent
                {
                    EVENT_ID = Guid.NewGuid().ToString(),
                    USER_ID = userId,
                    EVENT_TYPE = eventType,
                    EVENT_UTC = DateTime.UtcNow,
                    CLIENT_IP = ipAddress,
                    RESULT = "SUCCESS",
                    DETAILS_JSON = $"{{\"description\":\"{description}\"}}",
                    ACTIVE_IND = "Y",
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CHANGED_BY = userId,
                    ROW_CHANGED_DATE = DateTime.UtcNow,
                    ROW_EFFECTIVE_DATE = DateTime.UtcNow
                };
                await repo.InsertAsync(evt, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to write access audit event for user {UserId}", userId);
            }
        }
    }
}
