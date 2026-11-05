using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers;

/// <summary>Preview country environments for the currently authorized company. Preview does not save settings.</summary>
[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/company-environment")]
public sealed class CompanyEnvironmentController(RepositoryTenantContext tenant, RepositoryCompanyContextResolver companies,
    ICompanyEnvironmentStore store, ILogger<CompanyEnvironmentController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Read(CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        var selected = tenant.Company!;
        try
        {
            var value = await store.ReadAsync(selected.Scope, () => RequireAdministrator(token));
            await RequireAdministrator(token);
            return Ok(new CompanyEnvironmentReadResult(selected.Scope, value));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException failure)
        { logger.LogWarning(failure, "The company environment could not be reviewed"); return Conflict(new { error = "The company environment or its Beep schema could not be confirmed. Review the system setup repository." }); }
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] CompanyEnvironmentReviewedSaveRequest request, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        var selected = tenant.Company!;
        try
        {
            var value = await store.SaveAsync(selected.Scope, selected.UserId, request, () => RequireAdministrator(token));
            await RequireAdministrator(token);
            return Ok(value);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException or NotSupportedException)
        { return BadRequest(new { error = "The environment settings or save request are invalid." }); }
        catch (InvalidOperationException failure)
        { logger.LogWarning(failure, "The company environment save was not confirmed"); return Conflict(new { error = "The save was not confirmed. Reload the original company settings before retrying." }); }
    }

    [HttpGet("countries")]
    public async Task<IActionResult> Countries(CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        var countries = CompanyEnvironmentPreviewer.Countries();
        return await IsCurrentAdministrator(token) ? Ok(countries) : Forbid();
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromServices] CompanyEnvironmentRuntime runtime, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await runtime.VerifyAsync(tenant.Company!.Scope, () => RequireAdministrator(token))); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException or NotSupportedException or InvalidOperationException)
        { logger.LogWarning(invalid, "The company environment verification could not be confirmed"); return Conflict(new { error = "The saved environment or OilandGasTools build could not be verified. Reload and review the company settings." }); }
    }

    [HttpGet("organization")]
    public async Task<IActionResult> Organization([FromServices] CompanyOrganizationVerifier verifier, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await verifier.ReviewAsync(tenant.Company!.Scope, () => RequireAdministrator(token), token)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException failure)
        { logger.LogWarning(failure, "The selected company organization could not be verified"); return Conflict(new { error = "The PPDM organization or company binding could not be confirmed." }); }
    }

    [HttpGet("sources")]
    public async Task<IActionResult> Sources([FromServices] CompanyEnvironmentSources sources, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await sources.ReviewAsync(tenant.Company!.Scope, () => RequireAdministrator(token), token)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException or NotSupportedException or InvalidOperationException)
        { logger.LogWarning(invalid, "The company environment sources could not be reviewed"); return Conflict(new { error = "The saved sources or their company bindings could not be confirmed." }); }
    }

    [HttpGet("ghg-catalog/{jurisdiction}")]
    public async Task<IActionResult> GhgCatalog(string jurisdiction, [FromServices] CompanyGhgCatalogReader reader, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await reader.ReviewAsync(tenant.Company!.Scope, jurisdiction, () => RequireAdministrator(token))); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException or NotSupportedException or InvalidOperationException)
        { logger.LogWarning(invalid, "The company GHG catalog could not be reviewed"); return Conflict(new { error = "The selected factor catalog or company settings could not be confirmed." }); }
    }

    [HttpGet("ghg-selection")]
    public async Task<IActionResult> GhgSelection([FromServices] CompanyGhgSelectionStatusReader status, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await status.ReadAsync(tenant.Company!.Scope, () => RequireAdministrator(token))); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or InvalidOperationException)
        { logger.LogWarning(invalid, "Company GHG selection could not be read"); return Conflict(new { error = "The company selection or system setup schema could not be confirmed." }); }
    }

    [HttpPut("ghg-selection")]
    public async Task<IActionResult> SaveGhgSelection([FromBody] CompanyGhgSelectionRequest request, [FromServices] ICompanyGhgSelectionStore selections, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await selections.SaveAsync(tenant.Company!.Scope, tenant.Company.UserId, request, () => RequireAdministrator(token))); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (ArgumentException) { return BadRequest(new { error = "The company selection is incomplete or invalid." }); }
        catch (InvalidOperationException invalid)
        { logger.LogWarning(invalid, "Company GHG selection was not confirmed"); return Conflict(new { error = "The catalog, company environment or selection changed. Review the original record." }); }
    }

    [HttpPost("sources/test-well")]
    public async Task<IActionResult> TestWell([FromBody] CompanyEnvironmentWellTestRequest request,
        [FromServices] CompanyEnvironmentWellTestService tests, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        try { return Ok(await tests.TestAsync(tenant.Company!.Scope, request, () => RequireAdministrator(token), token)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException or NotSupportedException or InvalidOperationException)
        { logger.LogWarning(invalid, "The company source well check could not be confirmed"); return Conflict(new { error = "The selected source or well check could not be confirmed. Review the company, binding and saved settings." }); }
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] CompanyEnvironmentPreviewRequest request, [FromServices] CompanyEnvironmentEffectiveResolver effective, CancellationToken token)
    {
        if (!await IsCurrentAdministrator(token)) return Forbid();
        CompanyEnvironmentDraftReview preview;
        try { preview = await effective.PreviewDraftAsync(tenant.Company!.Scope, request, () => RequireAdministrator(token)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException or NotSupportedException)
        { return BadRequest(new { error = "The country or environment overrides are invalid. Review the preset and JSON." }); }
        catch (InvalidOperationException failure)
        {
            logger.LogWarning(failure, "Company environment preview could not confirm shared setup");
            return Conflict(new { error = "The combined settings could not be confirmed. Check shared system and country settings and review the company draft again." });
        }
        return await IsCurrentAdministrator(token) ? Ok(preview) : Forbid();
    }

    private async Task<bool> IsCurrentAdministrator(CancellationToken token)
    {
        var selected = tenant.Company;
        var actor = User.FindActingUserId();
        if (selected is null || actor is null || selected.UserId != actor || !User.IsInRole("Administrator")) return false;
        var current = await companies.ResolveAsync(actor, selected.Scope, token);
        return current is not null && current.Roles.Contains("Administrator", StringComparer.Ordinal);
    }

    private async Task RequireAdministrator(CancellationToken token)
    { if (!await IsCurrentAdministrator(token)) throw new UnauthorizedAccessException("Company administrator access changed."); }
}
