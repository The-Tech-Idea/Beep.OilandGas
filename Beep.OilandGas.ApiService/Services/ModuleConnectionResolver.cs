using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

public sealed class ModuleConnectionResolver(RepositoryDbContext repository, IDMEEditor editor)
{
    public async Task<TheTechIdea.Data.OilGas.AssetDatabaseScope> ResolveAssetScopeAsync()
    {
        var connection = await ResolveAsync("PPDM_CORE");
        var fingerprint = await GetMigrationBindingFingerprintAsync(new[] { "PPDM_CORE" }, connection);
        return new(connection, fingerprint);
    }

    public async Task<string> GetMigrationBindingFingerprintAsync(IReadOnlyList<string> moduleIds, string connectionName)
    {
        if (moduleIds.Count == 0) throw RefusalException.Invalid("Select at least one module.");
        var ids = moduleIds.Select(x => x.ToUpperInvariant()).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var bindings = await repository.ModuleDatabases.AsNoTracking().Where(x => ids.Contains(x.ModuleId)).ToListAsync();
        if (ids.Contains("SECURITY") || bindings.Count != ids.Length || bindings.Any(x =>
            string.IsNullOrWhiteSpace(x.ConcurrencyStamp) || !string.Equals(x.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase)))
            throw RefusalException.Conflict("The selected modules must be bound to the migration connection.");
        var connections = editor.ConfigEditor.DataConnections.Where(x =>
            string.Equals(x.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (connections.Count != 1)
            throw RefusalException.Conflict("The module connection is missing or ambiguous.");
        var connection = connections[0];
        var snapshot = bindings.OrderBy(x => x.ModuleId, StringComparer.Ordinal)
            .Select(x => new { x.ModuleId, x.ConnectionName, x.ConcurrencyStamp });
        // Retain only the digest in the process-local plan, never raw connection credentials.
        var target = Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Fingerprint(connection);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { Bindings = snapshot, Target = target })));
    }

    /// <summary>
    /// The connection a module's data lives on. A module with no binding yet, or whose bound connection is missing or
    /// ambiguous, is refused (409): the administrator binds it first.
    /// </summary>
    public async Task<string> ResolveAsync(string moduleId, CancellationToken cancellationToken = default)
    {
        var (connection, refusal) = await LookupAsync(moduleId, cancellationToken);
        return connection ?? throw RefusalException.Conflict(refusal!);
    }

    /// <summary>
    /// The connection a module's data lives on, or null while it has none to use — what a background worker asks on each
    /// run, where a module not yet bound means "wait", not a failure (OILGAS-CATCH-01: it asked <see cref="ResolveAsync"/>
    /// and caught its exception).
    /// </summary>
    public async Task<string?> FindAsync(string moduleId, CancellationToken cancellationToken = default) =>
        (await LookupAsync(moduleId, cancellationToken)).Connection;

    private async Task<(string? Connection, string? Refusal)> LookupAsync(string moduleId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        var id = moduleId.ToUpperInvariant();
        if (id == "SECURITY") throw new InvalidOperationException("Security belongs to the default repository.");
        var binding = await repository.ModuleDatabases.AsNoTracking()
            .SingleOrDefaultAsync(x => x.ModuleId == id, cancellationToken);
        if (binding is null || string.IsNullOrWhiteSpace(binding.ConnectionName))
            return (null, $"Configure a database binding for module {id} before accessing its data.");
        var connections = editor.ConfigEditor.DataConnections.Where(x =>
            string.Equals(x.ConnectionName, binding.ConnectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (connections.Count != 1)
            return (null, $"The database connection for module {id} is missing or ambiguous.");
        return (connections[0].ConnectionName, null);
    }
}
