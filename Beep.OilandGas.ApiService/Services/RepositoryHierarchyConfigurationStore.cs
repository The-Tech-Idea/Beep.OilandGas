using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryHierarchyConfigurationStore(RepositoryDbContext repository, CompanyModuleConnection company,
    TimeProvider? timeProvider = null) : IHierarchyConfigurationStore
{
    private readonly TimeProvider time = timeProvider ?? TimeProvider.System;

    public async Task<List<HierarchyConfig>> ReadAsync(string organizationId, bool requireAdministrator = false, CancellationToken token = default)
    {
        var actor = await RequireAsync(organizationId, requireAdministrator, token);
        var row = await ReadRowAsync(actor.Scope, token);
        if (actor.Scope != (await RequireAsync(organizationId, requireAdministrator, token)).Scope)
            throw RefusalException.Conflict("The hierarchy company selection changed.");
        if (row is null) return [];
        var configuration = JsonSerializer.Deserialize<List<HierarchyConfig>>(row.LEVELS_JSON)
            ?? throw new InvalidOperationException("The stored hierarchy configuration is missing its levels.");
        if (Validate(organizationId, configuration) is not null || !Guid.TryParseExact(row.REVISION, "D", out _))
            throw new InvalidOperationException("The stored hierarchy configuration is invalid.");
        foreach (var level in configuration) level.Revision = row.REVISION;
        return configuration;
    }

    public async Task<bool> UpdateAsync(string organizationId, List<HierarchyConfig> configuration, CancellationToken token = default)
    {
        var actor = await RequireAsync(organizationId, true, token);
        if (Validate(organizationId, configuration) is { } invalid) throw RefusalException.Invalid(invalid);
        var revisions = configuration.Select(row => row.Revision).Distinct(StringComparer.Ordinal).ToArray();
        if (revisions.Length != 1 || revisions[0] is { } expected && !Guid.TryParseExact(expected, "D", out _))
            throw RefusalException.Invalid("Every configuration level must carry the same expected revision.");
        if (repository.ChangeTracker.HasChanges() || repository.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("A hierarchy command requires its own clean repository transaction.");
        // Authority reads and both persistent writes share this repository transaction. Provider concurrency still needs live acceptance.
        await using var transaction = await repository.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        ORGANIZATION_HIERARCHY_CONFIG? aggregate = null;
        OrganizationHierarchyConfigRevision? audit = null;
        try
        {
            aggregate = await ReadRowAsync(actor.Scope, token);
            if (aggregate?.REVISION != revisions[0])
                throw RefusalException.Conflict("Hierarchy configuration changed. Refresh before saving.");
            var now = time.GetUtcNow().UtcDateTime;
            var previous = aggregate?.REVISION;
            var json = JsonSerializer.Serialize(configuration.OrderBy(row => row.AssetType, StringComparer.Ordinal).Select(row => new
            {
                row.OrganizationId, row.HierarchyLevel, row.LevelName, row.AssetType, row.ParentLevel, row.Active
            }));
            var revision = Guid.NewGuid().ToString("D");
            if (aggregate is null)
            {
                aggregate = new() { CONFIG_ID = Key(actor.Scope), TENANT_ID = actor.Scope.TenantId,
                    ORGANIZATION_ID = actor.Scope.BusinessAssociateId, CREATED_BY = actor.UserId, CREATED_UTC = now };
                repository.Add(aggregate);
            }
            else
            {
                // Commands use an isolated aggregate instance and the client's expected revision for the EF compare-and-swap.
                var tracked = repository.ChangeTracker.Entries<ORGANIZATION_HIERARCHY_CONFIG>().SingleOrDefault(entry => entry.Entity.CONFIG_ID == aggregate.CONFIG_ID);
                if (tracked is not null) tracked.State = EntityState.Detached;
                repository.Attach(aggregate); repository.Entry(aggregate).Property(row => row.REVISION).OriginalValue = previous!;
            }
            aggregate.LEVELS_JSON = json; aggregate.REVISION = revision; aggregate.CHANGED_BY = actor.UserId; aggregate.CHANGED_UTC = now;
            audit = new() { Revision = revision, ConfigId = aggregate.CONFIG_ID, PreviousRevision = previous,
                LevelsJson = json, ActorUserId = actor.UserId, ChangedUtc = now };
            repository.Add(audit);
            var current = await RequireAsync(organizationId, true, token);
            if (current.UserId != actor.UserId || current.Scope != actor.Scope)
                throw RefusalException.Forbidden("The hierarchy command account or company changed.");
            await repository.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw RefusalException.Conflict("Hierarchy configuration changed. Refresh before saving.");
        }
        finally
        {
            // Failed commands must not leave an aggregate/audit queued for an unrelated later SaveChanges.
            if (aggregate is not null) repository.Entry(aggregate).State = EntityState.Detached;
            if (audit is not null) repository.Entry(audit).State = EntityState.Detached;
        }
    }

    private async Task<ORGANIZATION_HIERARCHY_CONFIG?> ReadRowAsync(CompanyScope scope, CancellationToken token)
    {
        var rows = await repository.Set<ORGANIZATION_HIERARCHY_CONFIG>().AsNoTracking()
            .Where(row => row.TENANT_ID == scope.TenantId && row.ORGANIZATION_ID == scope.BusinessAssociateId).ToListAsync(token);
        if (rows.Count > 1 || rows.Any(row => row.TENANT_ID != scope.TenantId || row.ORGANIZATION_ID != scope.BusinessAssociateId || row.CONFIG_ID != Key(scope)))
            throw RefusalException.Conflict("The stored hierarchy configuration has inconsistent company ownership.");
        return rows.SingleOrDefault();
    }

    private async Task<RepositoryCompanyContext> RequireAsync(string organization, bool administrator, CancellationToken token)
    {
        var actor = await company.RefreshCompanyAsync(token);
        if (organization != actor.Scope.BusinessAssociateId)
            throw RefusalException.Forbidden("The hierarchy organization must match the selected company.");
        if (administrator && !actor.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A current company Administrator is required to configure the hierarchy.");
        return actor;
    }

    private static string Key(CompanyScope scope) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new[] { scope.TenantId, scope.BusinessAssociateId })));
    private static string? Validate(string organization, List<HierarchyConfig>? rows) =>
        HierarchyConfigurationRules.Validate(organization, rows);
}
