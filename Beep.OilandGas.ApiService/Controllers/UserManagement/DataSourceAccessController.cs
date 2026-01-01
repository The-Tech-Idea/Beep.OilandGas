using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.UserManagement.Core.DataAccess;

namespace Beep.OilandGas.ApiService.Controllers.UserManagement
{
    [ApiController]
    [Route("api/usermanagement/[controller]")]
    public class DataSourceAccessController : ControllerBase
    {
        private readonly IDataSourceAccessControl _dataSourceAccessControl;

        public DataSourceAccessController(IDataSourceAccessControl dataSourceAccessControl)
        {
            _dataSourceAccessControl = dataSourceAccessControl ?? throw new ArgumentNullException(nameof(dataSourceAccessControl));
        }

        /// <summary>
        /// Get all data sources accessible to a user
        /// </summary>
        [HttpGet("user/{userId}/accessible")]
        public async Task<ActionResult<IEnumerable<string>>> GetAccessibleDataSources(string userId)
        {
            try
            {
                var dataSources = await _dataSourceAccessControl.GetAccessibleDataSourcesAsync(userId);
                return Ok(dataSources);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user can access a specific data source
        /// </summary>
        [HttpPost("check-access")]
        public async Task<ActionResult<bool>> CheckDataSourceAccess([FromBody] CheckDataSourceAccessRequest request)
        {
            try
            {
                var canAccess = await _dataSourceAccessControl.CanAccessDataSourceAsync(request.UserId, request.DataSourceName);
                return Ok(canAccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all databases accessible to a user
        /// </summary>
        [HttpGet("user/{userId}/databases")]
        public async Task<ActionResult<IEnumerable<string>>> GetAccessibleDatabases(string userId)
        {
            try
            {
                var databases = await _dataSourceAccessControl.GetAccessibleDatabasesAsync(userId);
                return Ok(databases);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user can access a specific database
        /// </summary>
        [HttpPost("database/check-access")]
        public async Task<ActionResult<bool>> CheckDatabaseAccess([FromBody] CheckDatabaseAccessRequest request)
        {
            try
            {
                var canAccess = await _dataSourceAccessControl.CanAccessDatabaseAsync(request.UserId, request.DatabaseName);
                return Ok(canAccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class CheckDataSourceAccessRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string DataSourceName { get; set; } = string.Empty;
    }

    public class CheckDatabaseAccessRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
