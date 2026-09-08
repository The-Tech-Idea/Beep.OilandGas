using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using System.Security.Claims;
using System.Data;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/setup/modules")]
[Authorize(Roles = "Administrator")]
public sealed class ModuleRepositoryController(RepositoryDbContext repository, IDMEEditor editor,
    IEnumerable<IModuleSetup> modules, IPPDM39SchemaMigrationService migration) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var bindings = await repository.ModuleDatabases.AsNoTracking().ToDictionaryAsync(x => x.ModuleId, cancellationToken);
        return Ok(modules.Where(x => !string.Equals(x.ModuleId, "SECURITY", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Order).Select(module => new ModuleDatabaseSummary(module.ModuleId, module.ModuleName,
                bindings.GetValueOrDefault(module.ModuleId)?.ConnectionName,
                bindings.GetValueOrDefault(module.ModuleId)?.ConcurrencyStamp)));
    }

    [HttpGet("connections")]
    public IActionResult Connections() => Ok(editor.ConfigEditor.DataConnections
        .Select(x => x.ConnectionName).Where(x => !string.IsNullOrWhiteSpace(x))
        .GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() == 1)
        .Select(x => x.Key).OrderBy(x => x).ToList());

    [HttpPut("{moduleId}/connection")]
    public async Task<IActionResult> Bind(string moduleId, ModuleBindingRequest request, CancellationToken cancellationToken)
    {
        var module = Find(moduleId);
        if (module is null) return BadRequest(new { Error = "Unknown module or repository-owned security module." });
        if (string.IsNullOrWhiteSpace(request.ConnectionName)) return BadRequest(new { Error = "Select a BeepDM connection." });
        var matches = editor.ConfigEditor.DataConnections.Where(x =>
            string.Equals(x.ConnectionName, request.ConnectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count != 1) return BadRequest(new { Error = "Select an existing, uniquely named BeepDM connection." });
        var connection = matches[0];
        var binding = await repository.ModuleDatabases.FindAsync([module.ModuleId], cancellationToken);
        if (binding is null)
        {
            if (request.ConcurrencyStamp is not null) return Conflict();
            binding = new ModuleDatabaseBinding { ModuleId = module.ModuleId, ConnectionName = connection.ConnectionName };
            repository.ModuleDatabases.Add(binding);
        }
        else
        {
            if (!string.Equals(binding.ConcurrencyStamp, request.ConcurrencyStamp, StringComparison.Ordinal))
                return Conflict(new { Error = "The module binding changed. Reload before saving." });
            binding.ConnectionName = connection.ConnectionName;
            binding.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
        try { await repository.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { return Conflict(new { Error = "The binding could not be saved. Reload before retrying." }); }
        return Ok(binding);
    }

    [HttpPost("{moduleId}/plan")]
    public async Task<IActionResult> Plan(string moduleId, ModulePlanRequest request, CancellationToken cancellationToken)
    {
        var module = Find(moduleId);
        if (module is null) return BadRequest(new { Error = "Unknown module or repository-owned security module." });
        var binding = await repository.ModuleDatabases.AsNoTracking().SingleOrDefaultAsync(x => x.ModuleId == module.ModuleId, cancellationToken);
        if (binding is null || string.IsNullOrWhiteSpace(request.ConcurrencyStamp) ||
            !string.Equals(binding.ConcurrencyStamp, request.ConcurrencyStamp, StringComparison.Ordinal))
            return Conflict(new { Error = "The module binding changed or is missing. Reload before planning." });
        if (editor.ConfigEditor.DataConnections.Count(x =>
            string.Equals(x.ConnectionName, binding.ConnectionName, StringComparison.OrdinalIgnoreCase)) != 1)
            return Conflict(new { Error = "The selected BeepDM connection is missing or ambiguous." });
        var result = await migration.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
        {
            ConnectionName = binding.ConnectionName, ModuleIds = [module.ModuleId],
            EnvironmentTier = request.EnvironmentTier, BackupConfirmed = request.BackupConfirmed,
            RestoreTestEvidenceProvided = request.RestoreTestEvidenceProvided,
            RestoreTestEvidence = request.RestoreTestEvidence
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{moduleId}/seed")]
    public async Task<IActionResult> Seed(string moduleId, ModuleSeedRequest request, CancellationToken cancellationToken)
    {
        var module = Find(moduleId);
        if (module is null) return BadRequest(new { Error = "Unknown module or repository-owned security module." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Forbid();
        var binding = await repository.ModuleDatabases.AsNoTracking()
            .SingleOrDefaultAsync(x => x.ModuleId == module.ModuleId, cancellationToken);
        if (binding is null || string.IsNullOrWhiteSpace(request.ConcurrencyStamp) ||
            !string.Equals(binding.ConcurrencyStamp, request.ConcurrencyStamp, StringComparison.Ordinal))
            return Conflict(new { Error = "The module binding changed or is missing. Reload before seeding." });
        var matches = editor.ConfigEditor.DataConnections.Where(x =>
            string.Equals(x.ConnectionName, binding.ConnectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count != 1)
            return Conflict(new { Error = "The selected BeepDM connection is missing or ambiguous." });
        var connection = matches[0];
        cancellationToken.ThrowIfCancellationRequested();
        if (editor.OpenDataSource(connection.ConnectionName) != ConnectionState.Open)
            return StatusCode(503, new { Error = "The selected module connection could not be opened." });
        var result = await module.SeedAsync(connection.ConnectionName, userId, cancellationToken);
        return result.Success && result.Errors.Count == 0 ? Ok(result) : BadRequest(result);
    }

    private IModuleSetup? Find(string moduleId) => modules.SingleOrDefault(x =>
        !string.Equals(x.ModuleId, "SECURITY", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(x.ModuleId, moduleId, StringComparison.OrdinalIgnoreCase));
}
