using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

/// <summary>
/// Row-level security controller for enforcing data access policies.
/// Provides endpoints for checking row access, applying filters, and verifying source/database access.
/// </summary>
/// <remarks>
/// Every check is for the signed-in caller (<see cref="ActingUser.ActingUserId"/>, the OilGas account's <c>party_id</c>).
/// The checks took the user from the request body, and the scope queries from <c>?userId=</c> with a
/// <c>NameIdentifier ?? "system"</c> fallback, so any signed-in caller could read another person's scope.
/// </remarks>
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

    // ── Row Access ──────────────────────────────────────────────────────────

    /// <summary>Check if the current user has access to a specific row.</summary>
    [HttpPost("check-row-access")]
    public async Task<IActionResult> CheckRowAccess([FromBody] CheckRowAccessRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(request.TABLE_NAME))
        {
            return BadRequest(new { error = "TABLE_NAME is required." });
        }

        var result = await _securityService.CheckRowAccessAsync(request with { USER_ID = actor });
        return Ok(result);
    }

    /// <summary>Apply row-level filters based on the user's scope.</summary>
    [HttpPost("apply-row-filters")]
    public async Task<IActionResult> ApplyRowFilters([FromBody] ApplyRowFiltersRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(request.TABLE_NAME))
        {
            return BadRequest(new { error = "TABLE_NAME is required." });
        }

        var result = await _securityService.ApplyRowFiltersAsync(request with { USER_ID = actor });
        return Ok(result);
    }

    // ── Source/Database Access ──────────────────────────────────────────────

    /// <summary>Check if the user has access to a specific data source.</summary>
    [HttpPost("check-source-access")]
    public async Task<IActionResult> CheckSourceAccess([FromBody] CheckSourceAccessRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(request.TARGET_SOURCE))
        {
            return BadRequest(new { error = "TARGET_SOURCE is required." });
        }

        var result = await _securityService.CheckSourceAccessAsync(request with { USER_ID = actor });
        return Ok(result);
    }

    /// <summary>Check if the user has access to a specific database.</summary>
    [HttpPost("check-database-access")]
    public async Task<IActionResult> CheckDatabaseAccess([FromBody] CheckDatabaseAccessRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(request.DATABASE_NAME))
        {
            return BadRequest(new { error = "DATABASE_NAME is required." });
        }

        var result = await _securityService.CheckDatabaseAccessAsync(request with { USER_ID = actor });
        return Ok(result);
    }

    /// <summary>Check if the user has access to a specific data source connection.</summary>
    [HttpPost("check-datasource-access")]
    public async Task<IActionResult> CheckDataSourceAccess([FromBody] CheckDataSourceAccessRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(request.DATASOURCE_NAME))
        {
            return BadRequest(new { error = "DATASOURCE_NAME is required." });
        }

        var result = await _securityService.CheckDataSourceAccessAsync(request with { USER_ID = actor });
        return Ok(result);
    }

    // ── Scope Queries ───────────────────────────────────────────────────────

    /// <summary>Get the signed-in user's accessible field IDs.</summary>
    [HttpGet("accessible-fields")]
    public async Task<IActionResult> GetAccessibleFields()
    {
        var fields = await _securityService.GetUserAccessibleFieldsAsync(User.ActingUserId());
        return Ok(fields);
    }

    /// <summary>Get the signed-in user's accessible asset IDs.</summary>
    [HttpGet("accessible-assets")]
    public async Task<IActionResult> GetAccessibleAssets()
    {
        var assets = await _securityService.GetUserAccessibleAssetsAsync(User.ActingUserId());
        return Ok(assets);
    }

    /// <summary>Get the signed-in user's accessible organization IDs.</summary>
    [HttpGet("accessible-organizations")]
    public async Task<IActionResult> GetAccessibleOrganizations()
    {
        var orgs = await _securityService.GetUserAccessibleOrganizationsAsync(User.ActingUserId());
        return Ok(orgs);
    }
}
