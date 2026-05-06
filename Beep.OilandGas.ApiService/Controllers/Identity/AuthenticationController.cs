using Beep.OilandGas.UserManagement.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

/// <summary>
/// Authentication controller for login, logout, token management, and password operations.
/// Implements Oil & Gas industry best practices for secure authentication.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthService authService,
        ILogger<AuthenticationController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    private string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? UserAgent => HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    // ── Authentication ──────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates a user and returns JWT access and refresh tokens.
    /// Records login event in audit trail.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Username and password are required." });
        }

        var result = await _authService.LoginAsync(request, IpAddress, UserAgent);

        if (!result.Success)
        {
            var statusCode = result.IsLocked ? StatusCodes.Status423Locked : StatusCodes.Status401Unauthorized;
            return StatusCode(statusCode, new
            {
                error = result.ErrorMessage,
                isLocked = result.IsLocked,
                lockedUntil = result.IsLocked ? DateTime.UtcNow.AddMinutes(30) : (DateTime?)null
            });
        }

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            accessTokenExpiry = result.AccessTokenExpiry,
            refreshTokenExpiry = result.RefreshTokenExpiry,
            userId = result.UserId,
            username = result.Username,
            roles = result.Roles,
            permissions = result.Permissions,
            requiresPasswordChange = result.RequiresPasswordChange
        });
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required." });
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken, IpAddress);

        if (!result.Success)
        {
            return Unauthorized(new { error = result.ErrorMessage });
        }

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            accessTokenExpiry = result.AccessTokenExpiry,
            refreshTokenExpiry = result.RefreshTokenExpiry
        });
    }

    /// <summary>
    /// Logs out the current user by revoking the refresh token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        if (!string.IsNullOrWhiteSpace(request?.RefreshToken))
        {
            await _authService.RevokeTokenAsync(request.RefreshToken, IpAddress);
        }

        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Logs out from all devices by revoking all refresh tokens.
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        await _authService.RevokeAllTokensAsync(CurrentUserId);
        return Ok(new { message = "Logged out from all devices." });
    }

    // ── Password Management ─────────────────────────────────────────────────

    /// <summary>
    /// Changes the current user's password. Requires current password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { error = "Current and new passwords are required." });
        }

        var result = await _authService.ChangePasswordAsync(CurrentUserId, request.CurrentPassword, request.NewPassword);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(new { message = "Password changed successfully." });
    }

    /// <summary>
    /// Initiates password reset. Sends reset token to user's email.
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
        {
            return BadRequest(new { error = "Email or username is required." });
        }

        await _authService.RequestPasswordResetAsync(request.EmailOrUsername);

        return Ok(new { message = "If the account exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Completes password reset using a valid reset token.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { error = "Reset token and new password are required." });
        }

        var result = await _authService.ResetPasswordAsync(request.ResetToken, request.NewPassword);

        if (!result)
        {
            return BadRequest(new { error = "Invalid or expired reset token." });
        }

        return Ok(new { message = "Password reset successfully." });
    }

    // ── Account Management (Admin) ──────────────────────────────────────────

    /// <summary>
    /// Unlocks a locked user account. Requires admin privileges.
    /// </summary>
    [HttpPost("unlock/{userId}")]
    [Authorize(Policy = "Admin.ManageUsers")]
    public async Task<IActionResult> UnlockAccount(string userId)
    {
        var result = await _authService.UnlockAccountAsync(userId, CurrentUserId);

        if (!result)
        {
            return NotFound(new { error = "User not found." });
        }

        return Ok(new { message = "Account unlocked successfully." });
    }

    /// <summary>
    /// Records a failed login attempt (for external auth systems).
    /// </summary>
    [HttpPost("failed-login")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordFailedLogin([FromBody] FailedLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { error = "Username is required." });
        }

        await _authService.RecordFailedLoginAsync(request.Username, IpAddress);
        return Ok();
    }
}

// ── Request/Response DTOs ────────────────────────────────────────────────────

public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string? RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ForgotPasswordRequest(string EmailOrUsername);
public record ResetPasswordRequest(string ResetToken, string NewPassword);
public record FailedLoginRequest(string Username);
