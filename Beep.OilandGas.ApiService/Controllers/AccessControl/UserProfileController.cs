using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
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

        private readonly RepositoryRequestAccount _account;

        // The route ID names the subject; current repository authority determines an administrator override.
        public UserProfileController(IUserProfileService userProfileService, RepositoryRequestAccount account)
        {
            _userProfileService = userProfileService;
            _account = account;
        }

        /// <summary>
        /// Get a user's profile
        /// </summary>
        [HttpGet("{targetUserId}")]
        public async Task<ActionResult<UserProfile>> GetUserProfile(string targetUserId)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            await _account.RequireSelfOrAdministratorAsync(targetUserId);
            var profile = await _userProfileService.GetUserProfileAsync(targetUserId);
            
            if (profile == null)
                return NotFound(new { message = "User profile not found" });
            
            return Ok(profile);
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("{targetUserId}/roles")]
        public async Task<ActionResult<List<string>>> GetUserRoles(
            string targetUserId,
            [FromQuery] string? organizationId = null)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            await _account.RequireSelfOrAdministratorAsync(targetUserId);
            var roles = await _userProfileService.GetUserRolesAsync(targetUserId, organizationId);
            return Ok(roles);
        }

        /// <summary>
        /// Get the default layout for a user based on their primary role
        /// </summary>
        [HttpGet("{targetUserId}/default-layout")]
        public async Task<ActionResult<string>> GetUserDefaultLayout(string targetUserId)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            await _account.RequireSelfOrAdministratorAsync(targetUserId);
            var layout = await _userProfileService.GetUserDefaultLayoutAsync(targetUserId);
            return Ok(layout ?? "DefaultLayout");
        }

        /// <summary>
        /// Update user preferences
        /// </summary>
        [HttpPut("{targetUserId}/preferences")]
        public async Task<ActionResult<bool>> UpdateUserPreferences(
            string targetUserId,
            [FromBody] UpdatePreferencesRequest request)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            await _account.RequireSelfOrAdministratorAsync(targetUserId);
            var result = await _userProfileService.UpdateUserPreferencesAsync(targetUserId, request.PreferencesJson);
            return Ok(result);
        }

        /// <summary>
        /// Update user's primary role
        /// </summary>
        [HttpPut("{targetUserId}/primary-role")]
        public async Task<ActionResult<bool>> UpdateUserPrimaryRole(
            string targetUserId,
            [FromBody] UpdatePrimaryRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            await _account.RequireSelfOrAdministratorAsync(targetUserId);
            var result = await _userProfileService.UpdateUserPrimaryRoleAsync(targetUserId, request.PrimaryRole);
            return Ok(result);
        }

        /// <summary>
        /// Update user's preferred layout
        /// </summary>
        [HttpPut("{targetUserId}/preferred-layout")]
        public async Task<ActionResult<bool>> UpdateUserPreferredLayout(
            string targetUserId,
            [FromBody] UpdatePreferredLayoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            await _account.RequireSelfOrAdministratorAsync(targetUserId);
            var result = await _userProfileService.UpdateUserPreferredLayoutAsync(targetUserId, request.PreferredLayout);
            return Ok(result);
        }

        /// <summary>
        /// Record the signed-in user's own login. Only the account that signed in is recorded; a login is never recorded
        /// for somebody else.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult> RecordUserLogin()
        {
            var userId = await _account.RequireAsync();
            await _userProfileService.RecordUserLoginAsync(userId);
            return Ok(new { message = "Login recorded" });
        }
    }
}
