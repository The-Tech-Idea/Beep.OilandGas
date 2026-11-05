using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

public sealed class ModuleConnectionResolver(RepositoryDbContext repository, IDMEEditor editor, RepositoryTenantContext tenant)
{
    // Cache translation delegates per EF model, never binding/ownership/authority values.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Microsoft.EntityFrameworkCore.Metadata.IModel, BindingQueries> Queries = new();
    private BindingQueries CurrentQueries => Queries.GetValue(repository.Model, _ => new BindingQueries());
    private sealed class BindingQueries
    {
        internal readonly Func<RepositoryDbContext, string, string[], IAsyncEnumerable<ModuleDatabaseBinding>> Bindings = EF.CompileAsyncQuery(
            (RepositoryDbContext db, string tenantId, string[] ids) => db.ModuleDatabases.AsNoTracking()
                .Where(x => x.TenantId == tenantId && ids.Contains(x.ModuleId)));
        internal readonly Func<RepositoryDbContext, string, IAsyncEnumerable<AppModuleConnection>> Owners = EF.CompileAsyncQuery(
            (RepositoryDbContext db, string tenantId) => db.Set<AppModuleConnection>().AsNoTracking().Where(x => x.TenantId == tenantId));
        internal readonly Func<RepositoryDbContext, string, string, IAsyncEnumerable<ModuleDatabaseBinding>> Binding = EF.CompileAsyncQuery(
            (RepositoryDbContext db, string tenantId, string id) => db.ModuleDatabases.AsNoTracking().Where(x => x.TenantId == tenantId && x.ModuleId == id));
        internal readonly Func<RepositoryDbContext, string, string?, IAsyncEnumerable<AppModuleConnection>> Owner = EF.CompileAsyncQuery(
            (RepositoryDbContext db, string tenantId, string? key) => db.Set<AppModuleConnection>().AsNoTracking()
                .Where(x => x.ConnectionKey == key && x.TenantId == tenantId));
    }
    private static async Task<List<T>> ReadAsync<T>(IAsyncEnumerable<T> rows, CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        await foreach (var row in rows.WithCancellation(cancellationToken)) result.Add(row);
        return result;
    }

    public async Task<TheTechIdea.Data.OilGas.AssetDatabaseScope> ResolveAssetScopeAsync()
    {
        var current = await ResolveBindingSnapshotAsync(["PPDM_CORE"], null);
        return new(current.ConnectionName, current.Fingerprint);
    }

    public async Task<string> GetMigrationBindingFingerprintAsync(IReadOnlyList<string> moduleIds, string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName)) throw RefusalException.Invalid("Select a migration connection.");
        return (await ResolveBindingSnapshotAsync(moduleIds, connectionName)).Fingerprint;
    }

    // Resolve the connection and proof from the same fresh rows instead of looking up the core binding twice.
    private async Task<(string ConnectionName, string Fingerprint)> ResolveBindingSnapshotAsync(IReadOnlyList<string> moduleIds, string? expectedConnection)
    {
        if (moduleIds.Count == 0) throw RefusalException.Invalid("Select at least one module.");
        var ids = moduleIds.Select(x => x.ToUpperInvariant()).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var tenantId = await RequireTenantAsync();
        var bindings = await ReadAsync(CurrentQueries.Bindings(repository, tenantId, ids));
        var owners = await ReadAsync(CurrentQueries.Owners(repository, tenantId));
        var owned = owners.Where(x => x.PhysicalTargetKey is not null).Select(x => x.ConnectionKey).ToArray();
        var connectionName = expectedConnection ?? bindings.FirstOrDefault()?.ConnectionName;
        if (string.IsNullOrWhiteSpace(connectionName) || ids.Contains("SECURITY") || bindings.Count != ids.Length || bindings.Any(x =>
            x.ConnectionKey is null || !owned.Contains(x.ConnectionKey) ||
            !string.Equals(x.ConnectionKey, x.ConnectionName.ToUpperInvariant(), StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(x.ConcurrencyStamp) || !string.Equals(x.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase)))
            throw RefusalException.Conflict("The selected modules must be bound to the migration connection.");
        var connections = editor.ConfigEditor.DataConnections.Where(x =>
            string.Equals(x.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (connections.Count != 1)
            throw RefusalException.Conflict("The module connection is missing or ambiguous.");
        var connection = connections[0];
        var physicalTarget = ModuleDatabaseTarget.Key(editor, connection);
        if (owners.Any(owner => bindings.Any(binding => binding.ConnectionKey == owner.ConnectionKey) && owner.PhysicalTargetKey != physicalTarget))
            throw RefusalException.Conflict("The configured database target no longer matches its tenant ownership.");
        var snapshot = bindings.OrderBy(x => x.ModuleId, StringComparer.Ordinal)
            .Select(x => new { x.TenantId, x.BindingId, x.ModuleId, x.ConnectionName, x.ConcurrencyStamp });
        // Retain only the digest in the process-local plan, never raw connection credentials.
        var target = Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Fingerprint(connection);
        var fingerprint = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { Bindings = snapshot, PhysicalTarget = physicalTarget, Target = target })));
        return (connection.ConnectionName, fingerprint);
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
        var tenantId = await RequireTenantAsync(cancellationToken);
        var binding = (await ReadAsync(CurrentQueries.Binding(repository, tenantId, id), cancellationToken)).SingleOrDefault();
        if (binding is null || string.IsNullOrWhiteSpace(binding.ConnectionName))
            return (null, $"Configure a database binding for module {id} before accessing its data.");
        var owner = (await ReadAsync(CurrentQueries.Owner(repository, tenantId, binding.ConnectionKey), cancellationToken)).SingleOrDefault();
        if (binding.ConnectionKey is null || binding.ConnectionKey != binding.ConnectionName.ToUpperInvariant() || owner?.PhysicalTargetKey is null)
            return (null, $"The database connection for module {id} has no verified tenant owner.");
        var connections = editor.ConfigEditor.DataConnections.Where(x =>
            string.Equals(x.ConnectionName, binding.ConnectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (connections.Count != 1)
            return (null, $"The database connection for module {id} is missing or ambiguous.");
        if (owner.PhysicalTargetKey != ModuleDatabaseTarget.Key(editor, connections[0]))
            return (null, $"The configured database target for module {id} no longer matches its tenant ownership.");
        return (connections[0].ConnectionName, null);
    }

    private async Task<string> RequireTenantAsync(CancellationToken token = default)
    {
        var id = tenant.TenantId ?? throw RefusalException.Invalid("Select a tenant and company before accessing module data.");
        await new ModuleBindingService(repository, editor).RequireActiveTenantAsync(id, token);
        return id;
    }
}
