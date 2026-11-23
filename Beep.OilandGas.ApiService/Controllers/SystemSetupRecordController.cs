using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers;

/// <summary>All active users can read shared setup; only current application administrators can change it.</summary>
[ApiController]
[Authorize]
[Route("api/setup/system-repository/records")]
public sealed class SystemSetupRecordController(SystemSetupRecordStore store, IRepositoryAccessService access,
    TheTechIdeaWeb.Diagnostics.IFailureReporter failures) : ControllerBase
{
    [HttpGet("tools-default-draft")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ToolsDefaultDraft(CancellationToken token)
    {
        var actor = User.FindActingUserId();
        if (actor is null || !User.IsInRole("Administrator")) return Forbid();
        async Task Guard()
        {
            var current = await access.GetAccessAsync(actor, token);
            if (current is not { IsActive: true } || !current.Roles.Contains("Administrator", StringComparer.Ordinal)) throw new UnauthorizedAccessException();
        }
        try { await Guard(); var draft = SystemSetupToolsDefaults.Draft(); await Guard(); return Ok(draft); }
        catch (UnauthorizedAccessException denied) { failures.ReportHandled(denied, "authorizing system setup access", "request forbidden"); return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or InvalidOperationException or JsonException)
        { failures.ReportHandled(invalid, "Tool default draft is unavailable", "setup record or outcome unconfirmed; request refused"); return Conflict(new { error = "The installed tool defaults could not be prepared safely." }); }
    }

    [HttpGet("{category}/{key}")]
    public async Task<IActionResult> Read(string category, string key, CancellationToken token)
    {
        var actor = User.FindActingUserId(); if (actor is null) return Forbid();
        try
        {
            async Task Guard() { if (!await access.IsAccountActiveAsync(actor, token)) throw new UnauthorizedAccessException(); }
            var value = await store.ReadAsync(category, key, Guard); await Guard();
            return Ok(new SystemSetupReadResult(category, key, value));
        }
        catch (UnauthorizedAccessException denied) { failures.ReportHandled(denied, "authorizing system setup access", "request forbidden"); return Forbid(); }
        catch (ArgumentException invalid) { failures.ReportHandled(invalid, "validating system setup record", "request rejected"); return BadRequest(new { error = "Select a valid setup category and key." }); }
        catch (InvalidOperationException failure)
        { failures.ReportHandled(failure, "The system setup record could not be confirmed", "setup record or outcome unconfirmed; request refused"); return Conflict(new { error = "The system setup repository or record could not be confirmed." }); }
    }

    [HttpPut("{category}/{key}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Save(string category, string key, [FromBody] SystemSetupRequest request, CancellationToken token)
    {
        var actor = User.FindActingUserId();
        if (actor is null || !User.IsInRole("Administrator")) return Forbid();
        if (request is null || request.Category != category || request.Key != key) return BadRequest(new { error = "The route and setup record must match." });
        try
        {
            async Task Guard()
            {
                var current = await access.GetAccessAsync(actor, token);
                if (current is not { IsActive: true } || !current.Roles.Contains("Administrator", StringComparer.Ordinal)) throw new UnauthorizedAccessException();
            }
            var value = await store.SaveAsync(request, actor, Guard); await Guard(); return Ok(value);
        }
        catch (UnauthorizedAccessException denied) { failures.ReportHandled(denied, "authorizing system setup access", "request forbidden"); return Forbid(); }
        catch (Exception invalid) when (invalid is ArgumentException or JsonException or FormatException)
        { failures.ReportHandled(invalid, "validating system setup settings", "request rejected"); return BadRequest(new { error = "The system setup settings or revision are invalid." }); }
        catch (InvalidOperationException failure)
        { failures.ReportHandled(failure, "The system setup save outcome is unconfirmed", "setup record or outcome unconfirmed; request refused"); return Conflict(new { error = "The system setup save was not confirmed. Reload the original record before retrying." }); }
    }
}
