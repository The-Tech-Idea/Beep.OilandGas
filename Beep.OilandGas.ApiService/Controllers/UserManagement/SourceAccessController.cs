using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.UserManagement.Core.DataAccess;

namespace Beep.OilandGas.ApiService.Controllers.UserManagement
{
    [ApiController]
    [Route("api/usermanagement/[controller]")]
    public class SourceAccessController : ControllerBase
    {
        private readonly ISourceAccessControl _sourceAccessControl;

        public SourceAccessController(ISourceAccessControl sourceAccessControl)
        {
            _sourceAccessControl = sourceAccessControl ?? throw new ArgumentNullException(nameof(sourceAccessControl));
        }

        /// <summary>
        /// Get all source systems accessible to a user
        /// </summary>
        [HttpGet("user/{userId}/accessible")]
        public async Task<ActionResult<IEnumerable<string>>> GetAccessibleSources(string userId)
        {
            try
            {
                var sources = await _sourceAccessControl.GetAccessibleSourcesAsync(userId);
                return Ok(sources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user can access data from a specific source
        /// </summary>
        [HttpPost("check-access")]
        public async Task<ActionResult<bool>> CheckSourceAccess([FromBody] CheckSourceAccessRequest request)
        {
            try
            {
                var canAccess = await _sourceAccessControl.CanAccessSourceAsync(request.UserId, request.Source);
                return Ok(canAccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class CheckSourceAccessRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
