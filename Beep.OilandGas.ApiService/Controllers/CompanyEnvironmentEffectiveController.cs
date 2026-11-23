using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers;

/// <summary>Active company members can read their current effective, non-secret environment settings.</summary>
[ApiController]
[Authorize]
[Route("api/company-environment/effective")]
public sealed class CompanyEnvironmentEffectiveController(RepositoryTenantContext tenant, RepositoryCompanyContextResolver companies,
    CompanyEnvironmentEffectiveResolver environments, TheTechIdeaWeb.Diagnostics.IFailureReporter failures) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var selected = tenant.Company;
        var actor = User.FindActingUserId();
        if (selected is null || actor is null || selected.UserId != actor) return Forbid();
        async Task Guard()
        {
            var current = await companies.ResolveAsync(actor, selected.Scope, token);
            if (current is null || !current.Roles.Order(StringComparer.Ordinal).SequenceEqual(selected.Roles.Order(StringComparer.Ordinal)) ||
                !current.Permissions.Order(StringComparer.Ordinal).SequenceEqual(selected.Permissions.Order(StringComparer.Ordinal)))
                throw new UnauthorizedAccessException("Company access changed.");
        }
        try { await Guard(); return Ok(await environments.ResolveAsync(selected.Scope, Guard)); }
        catch (UnauthorizedAccessException denied) { failures.ReportHandled(denied,"authorizing company or setup request","request refused"); return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or System.Text.Json.JsonException or FormatException or NotSupportedException or InvalidOperationException)
        { failures.ReportHandled(invalid,"The effective company environment could not be confirmed","original company evidence or outcome unconfirmed; request refused"); return Conflict(new { error = "The shared, country or company setup could not be confirmed. Ask an administrator to review system setup." }); }
    }
}
