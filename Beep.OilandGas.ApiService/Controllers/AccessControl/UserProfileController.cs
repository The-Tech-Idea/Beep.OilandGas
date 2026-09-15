using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Controllers.AccessControl
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        private bool CanAccess(string userId) => User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(User.FindFirstValue(ClaimTypes.NameIdentifier))
            && (User.FindFirstValue(ClaimTypes.NameIdentifier) == userId || User.IsInRole("Administrator"));

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Get a user's profile
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserProfile>> GetUserProfile(string userId)
        {
            if (!CanAccess(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                
                if (profile == null)
                    return NotFound(new { message = "User profile not found" });
                
                return Ok(profile);
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("{userId}/roles")]
        public async Task<ActionResult<List<string>>> GetUserRoles(
            string userId,
            [FromQuery] string? organizationId = null)
        {
            if (!CanAccess(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var roles = await _userProfileService.GetUserRolesAsync(userId, organizationId);
                return Ok(roles);
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get the default layout for a user based on their primary role
        /// </summary>
        [HttpGet("{userId}/default-layout")]
        public async Task<ActionResult<string>> GetUserDefaultLayout(string userId)
        {
            if (!CanAccess(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var layout = await _userProfileService.GetUserDefaultLayoutAsync(userId);
                return Ok(layout ?? "DefaultLayout");
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Update user preferences
        /// </summary>
        [HttpPut("{userId}/preferences")]
        public async Task<ActionResult<bool>> UpdateUserPreferences(
            string userId,
            [FromBody] UpdatePreferencesRequest request)
        {
            if (!CanAccess(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var result = await _userProfileService.UpdateUserPreferencesAsync(userId, request.PreferencesJson);
                return Ok(result);
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Update user's primary role
        /// </summary>
        [HttpPut("{userId}/primary-role")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<bool>> UpdateUserPrimaryRole(
            string userId,
            [FromBody] UpdatePrimaryRoleRequest request)
        {
            if (!CanAccess(userId) || !User.IsInRole("Administrator")) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var result = await _userProfileService.UpdateUserPrimaryRoleAsync(userId, request.PrimaryRole);
                return Ok(result);
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Update user's preferred layout
        /// </summary>
        [HttpPut("{userId}/preferred-layout")]
        public async Task<ActionResult<bool>> UpdateUserPreferredLayout(
            string userId,
            [FromBody] UpdatePreferredLayoutRequest request)
        {
            if (!CanAccess(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var result = await _userProfileService.UpdateUserPreferredLayoutAsync(userId, request.PreferredLayout);
                return Ok(result);
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Record user login
        /// </summary>
        [HttpPost("{userId}/login")]
        public async Task<ActionResult> RecordUserLogin(string userId)
        {
            if (!CanAccess(userId)) return Forbid();
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                await _userProfileService.RecordUserLoginAsync(userId);
                return Ok(new { message = "Login recorded" });
            }
            catch (System.Exception exception) when (exception is System.ArgumentException or System.Text.Json.JsonException or System.NotSupportedException)
            {
                return BadRequest(new { error = "Invalid profile request or unsupported organization role scope." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}
