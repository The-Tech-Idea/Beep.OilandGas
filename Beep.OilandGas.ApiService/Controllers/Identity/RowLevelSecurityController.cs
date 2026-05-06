using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

/// <summary>
/// Row-level security controller for enforcing data access policies.
/// Provides endpoints for checking row access, applying filters, and verifying source/database access.
/// </summary>
[ApiController]
[Route("api/identity/security")]
[Authorize]
public class RowLevelSecurityController : ControllerBase
{
    private readonly IRowLevelSecurityService _securityService;
    private readonly ILogger<RowLevelSecurityController> _logger;

    public RowLevelSecurityController(
        IRowLevelSecurityService securityService,
        ILogger<RowLevelSecurityController> logger)
    {
        _securityService = securityService;
        _logger = logger;
    }

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    // ── Row Access ──────────────────────────────────────────────────────────

    /// <summary>Check if the current user has access to a specific row.</summary>
    [HttpPost("check-row-access")]
    public async Task<IActionResult> CheckRowAccess([FromBody] CheckRowAccessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.USER_ID) || string.IsNullOrWhiteSpace(request.TABLE_NAME))
        {
            return BadRequest(new { error = "USER_ID and TABLE_NAME are required." });
        }

        var result = await _securityService.CheckRowAccessAsync(request);
        return Ok(result);
    }

    /// <summary>Apply row-level filters based on the user's scope.</summary>
    [HttpPost("apply-row-filters")]
    public async Task<IActionResult> ApplyRowFilters([FromBody] ApplyRowFiltersRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.USER_ID) || string.IsNullOrWhiteSpace(request.TABLE_NAME))
        {
            return BadRequest(new { error = "USER_ID and TABLE_NAME are required." });
        }

        var result = await _securityService.ApplyRowFiltersAsync(request);
        return Ok(result);
    }

    // ── Source/Database Access ──────────────────────────────────────────────

    /// <summary>Check if the user has access to a specific data source.</summary>
    [HttpPost("check-source-access")]
    public async Task<IActionResult> CheckSourceAccess([FromBody] CheckSourceAccessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.USER_ID) || string.IsNullOrWhiteSpace(request.TARGET_SOURCE))
        {
            return BadRequest(new { error = "USER_ID and TARGET_SOURCE are required." });
        }

        var result = await _securityService.CheckSourceAccessAsync(request);
        return Ok(result);
    }

    /// <summary>Check if the user has access to a specific database.</summary>
    [HttpPost("check-database-access")]
    public async Task<IActionResult> CheckDatabaseAccess([FromBody] CheckDatabaseAccessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.USER_ID) || string.IsNullOrWhiteSpace(request.DATABASE_NAME))
        {
            return BadRequest(new { error = "USER_ID and DATABASE_NAME are required." });
        }

        var result = await _securityService.CheckDatabaseAccessAsync(request);
        return Ok(result);
    }

    /// <summary>Check if the user has access to a specific data source connection.</summary>
    [HttpPost("check-datasource-access")]
    public async Task<IActionResult> CheckDataSourceAccess([FromBody] CheckDataSourceAccessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.USER_ID) || string.IsNullOrWhiteSpace(request.DATASOURCE_NAME))
        {
            return BadRequest(new { error = "USER_ID and DATASOURCE_NAME are required." });
        }

        var result = await _securityService.CheckDataSourceAccessAsync(request);
        return Ok(result);
    }

    // ── Scope Queries ───────────────────────────────────────────────────────

    /// <summary>Get the user's accessible field IDs.</summary>
    [HttpGet("accessible-fields")]
    public async Task<IActionResult> GetAccessibleFields([FromQuery] string? userId = null)
    {
        var targetUserId = userId ?? CurrentUserId;
        var fields = await _securityService.GetUserAccessibleFieldsAsync(targetUserId);
        return Ok(fields);
    }

    /// <summary>Get the user's accessible asset IDs.</summary>
    [HttpGet("accessible-assets")]
    public async Task<IActionResult> GetAccessibleAssets([FromQuery] string? userId = null)
    {
        var targetUserId = userId ?? CurrentUserId;
        var assets = await _securityService.GetUserAccessibleAssetsAsync(targetUserId);
        return Ok(assets);
    }

    /// <summary>Get the user's accessible organization IDs.</summary>
    [HttpGet("accessible-organizations")]
    public async Task<IActionResult> GetAccessibleOrganizations([FromQuery] string? userId = null)
    {
        var targetUserId = userId ?? CurrentUserId;
        var orgs = await _securityService.GetUserAccessibleOrganizationsAsync(targetUserId);
        return Ok(orgs);
    }
}
