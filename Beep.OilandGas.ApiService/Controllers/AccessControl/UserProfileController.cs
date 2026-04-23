using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Controllers.AccessControl
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

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
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                
                if (profile == null)
                    return NotFound(new { message = "User profile not found" });
                
                return Ok(profile);
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
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var roles = await _userProfileService.GetUserRolesAsync(userId, organizationId);
                return Ok(roles);
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
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var layout = await _userProfileService.GetUserDefaultLayoutAsync(userId);
                return Ok(layout ?? "DefaultLayout");
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
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var result = await _userProfileService.UpdateUserPreferencesAsync(userId, request.PreferencesJson);
                return Ok(result);
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
        public async Task<ActionResult<bool>> UpdateUserPrimaryRole(
            string userId,
            [FromBody] UpdatePrimaryRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var result = await _userProfileService.UpdateUserPrimaryRoleAsync(userId, request.PrimaryRole);
                return Ok(result);
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
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var result = await _userProfileService.UpdateUserPreferredLayoutAsync(userId, request.PreferredLayout);
                return Ok(result);
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
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                await _userProfileService.RecordUserLoginAsync(userId);
                return Ok(new { message = "Login recorded" });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}
