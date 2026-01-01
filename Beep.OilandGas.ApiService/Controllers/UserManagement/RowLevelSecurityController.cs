using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.UserManagement.Core.DataAccess;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.ApiService.Controllers.UserManagement
{
    [ApiController]
    [Route("api/usermanagement/[controller]")]
    public class RowLevelSecurityController : ControllerBase
    {
        private readonly IRowLevelSecurityProvider _rlsProvider;

        public RowLevelSecurityController(IRowLevelSecurityProvider rlsProvider)
        {
            _rlsProvider = rlsProvider ?? throw new ArgumentNullException(nameof(rlsProvider));
        }

        /// <summary>
        /// Get row-level security filters for a table
        /// </summary>
        [HttpGet("user/{userId}/filters/{tableName}")]
        public async Task<ActionResult<List<AppFilter>>> GetRowFilters(string userId, string tableName)
        {
            try
            {
                var filters = await _rlsProvider.GetRowFiltersAsync(userId, tableName);
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user can access a specific row
        /// </summary>
        [HttpPost("check-row-access")]
        public async Task<ActionResult<bool>> CheckRowAccess([FromBody] CheckRowAccessRequest request)
        {
            try
            {
                // Note: This requires the entity to be passed, which may not be practical via API
                // In practice, you might want to check by entity ID and table name instead
                return BadRequest(new { error = "Row access check requires entity object. Use filters endpoint instead." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Apply row-level security filters to existing filters
        /// </summary>
        [HttpPost("apply-filters")]
        public async Task<ActionResult<List<AppFilter>>> ApplyRowFilters([FromBody] ApplyRowFiltersRequest request)
        {
            try
            {
                var filters = await _rlsProvider.ApplyRowFiltersAsync(request.UserId, request.TableName, request.ExistingFilters);
                return Ok(filters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class CheckRowAccessRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();
    }

    public class ApplyRowFiltersRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public List<AppFilter>? ExistingFilters { get; set; }
    }
}
