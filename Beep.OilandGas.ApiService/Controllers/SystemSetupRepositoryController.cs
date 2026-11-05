using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Controllers;

/// <summary>System setup schema is planned separately from any company's PPDM database.</summary>
[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/setup/system-repository")]
public sealed class SystemSetupRepositoryController(SystemSetupTarget target, IDMEEditor editor,
    IEnumerable<IModuleSetup> modules, IPPDM39SchemaMigrationService migration, IConfiguration configuration,
    IRepositoryAccessService access) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken token)
    {
        var actor = User.FindActingUserId();
        if (actor is null || !User.IsInRole("Administrator")) return Forbid();
        try
        {
            await RequireAdministrator(actor, token);
            SystemSetupConnectionSelectionService.ReloadPersistedSelectionIfNeeded(configuration);
            if (string.IsNullOrWhiteSpace(configuration["SystemSetup:ConnectionName"]))
            {
                await RequireAdministrator(actor, token);
                if (!string.IsNullOrWhiteSpace(configuration["SystemSetup:ConnectionName"])) throw new InvalidOperationException("The system target changed during review.");
                return Ok(new SystemSetupRepositoryStatus("Unconfigured"));
            }
            var source = target.Resolve();
            var manifest = modules.Single(x => x.ModuleId == "SYSTEM_SETUP").EntityTypes;
            var status = "SchemaReady";
            try { ModuleSchemaVerification.Verify(editor, source.Source, manifest); }
            catch (ModuleSchemaMismatchException) { status = "MigrationRequired"; }
            await RequireAdministrator(actor, token);
            RequireSameTarget(source);
            return Ok(new SystemSetupRepositoryStatus(status));
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException)
        {
            try { await RequireAdministrator(actor, token); }
            catch (UnauthorizedAccessException) { return Forbid(); }
            return Ok(new SystemSetupRepositoryStatus("Unavailable"));
        }
    }

    [HttpPost("plan")]
    public async Task<IActionResult> Plan([FromBody] ModulePlanRequest request, CancellationToken token = default)
    {
        var actor = User.FindActingUserId();
        if (actor is null || !User.IsInRole("Administrator")) return Forbid();
        if (request is null) return BadRequest();
        try
        {
            await RequireAdministrator(actor, token);
            var source = target.Resolve();
            var result = await migration.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
            {
                ConnectionName = source.ConnectionName, ModuleIds = ["SYSTEM_SETUP"], EnvironmentTier = request.EnvironmentTier,
                BackupConfirmed = request.BackupConfirmed, RestoreTestEvidenceProvided = request.RestoreTestEvidenceProvided,
                RestoreTestEvidence = request.RestoreTestEvidence
            });
            await RequireAdministrator(actor, token);
            RequireSameTarget(source);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException) { return Conflict(new { error = "The system setup connection is unavailable or changed." }); }
    }

    private async Task RequireAdministrator(string actor, CancellationToken token)
    {
        var current = await access.GetAccessAsync(actor, token);
        if (current is not { IsActive: true } || !current.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw new UnauthorizedAccessException();
    }

    private void RequireSameTarget((string ConnectionName, TheTechIdea.Beep.IDataSource Source, string Fingerprint) original)
    {
        var current = target.Resolve();
        if (current.ConnectionName != original.ConnectionName || !ReferenceEquals(current.Source, original.Source) || current.Fingerprint != original.Fingerprint)
            throw new InvalidOperationException("The system target changed.");
    }
}
