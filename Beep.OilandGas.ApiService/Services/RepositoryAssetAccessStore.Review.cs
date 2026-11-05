using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class RepositoryAssetAccessStore
{
    public async Task<AssetGrantReviewPage> ReviewAsync(string userId, string databaseScope, int offset = 0, int limit = 50, CancellationToken token = default)
    {
        ValidateUserAndScope(userId, databaseScope); ValidatePage(offset, limit);
        var actor = await RequireReviewAsync(databaseScope, token);
        // No subject membership, active flag, qualifier or PPDM existence gate: administrators must see records that need retirement.
        var query = repository.Set<AppUserAssetAccess>().AsNoTracking().Where(row => row.UserId == userId &&
            row.DatabaseScope == databaseScope && row.OrganizationId == actor.Scope.BusinessAssociateId);
        // Compare ownership before counting/paging; case-insensitive provider collations must not leak aliased counts.
        var headers = await query.Select(row => new { row.Id, row.UserId, row.DatabaseScope, row.OrganizationId }).ToListAsync(token);
        if (headers.Any(row => row.UserId != userId || row.DatabaseScope != databaseScope || row.OrganizationId != actor.Scope.BusinessAssociateId))
            throw RefusalException.Conflict("Stored grants have inconsistent company or subject ownership.");
        var ids = headers.OrderBy(row => row.Id, StringComparer.Ordinal).Skip(offset).Take(limit).Select(row => row.Id).ToArray();
        var rows = await query.Where(row => ids.Contains(row.Id)).ToListAsync(token);
        if (rows.Any(row => row.UserId != userId || !Owned(row, actor.Scope, databaseScope)))
            throw RefusalException.Conflict("Stored grants have inconsistent company or subject ownership.");
        await RequireSameReviewAsync(actor, databaseScope, token);
        return new(actor.Scope, databaseScope, userId, offset, limit, headers.Count, rows.OrderBy(row => row.Id, StringComparer.Ordinal).ToList());
    }

    public async Task<AssetGrantRetirementPage> ReadRetirementsAsync(string grantId, string databaseScope, int offset = 0, int limit = 50, CancellationToken token = default)
    {
        ValidateIdentifier(grantId, nameof(grantId)); ValidatePage(offset, limit);
        var actor = await RequireReviewAsync(databaseScope, token);
        _ = await ReadReviewGrantAsync(grantId, actor.Scope, databaseScope, token);
        var query = repository.Set<AssetGrantRetirement>().AsNoTracking().Where(row => row.GrantId == grantId && row.TenantId == actor.Scope.TenantId &&
            row.OrganizationId == actor.Scope.BusinessAssociateId && row.DatabaseScope == databaseScope);
        var headers = await query.Select(row => new { row.Revision, row.ChangedUtc, row.GrantId, row.TenantId, row.OrganizationId, row.DatabaseScope }).ToListAsync(token);
        if (headers.Any(row => row.GrantId != grantId || row.TenantId != actor.Scope.TenantId || row.OrganizationId != actor.Scope.BusinessAssociateId || row.DatabaseScope != databaseScope))
            throw RefusalException.Conflict("Stored retirement evidence has inconsistent ownership.");
        var revisions = headers.OrderByDescending(row => row.ChangedUtc).ThenBy(row => row.Revision, StringComparer.Ordinal).Skip(offset).Take(limit).Select(row => row.Revision).ToArray();
        var rows = await query.Where(row => revisions.Contains(row.Revision)).ToListAsync(token);
        if (rows.Any(row => row.GrantId != grantId || row.TenantId != actor.Scope.TenantId || row.OrganizationId != actor.Scope.BusinessAssociateId || row.DatabaseScope != databaseScope))
            throw RefusalException.Conflict("Stored retirement evidence has inconsistent ownership.");
        await RequireSameReviewAsync(actor, databaseScope, token);
        return new(actor.Scope, databaseScope, grantId, offset, limit, headers.Count, rows.OrderByDescending(row => row.ChangedUtc).ThenBy(row => row.Revision, StringComparer.Ordinal).ToList());
    }

    public async Task<AssetGrantRetirement> RetireAsync(string grantId, string databaseScope, RetireAssetGrantRequest request, CancellationToken token = default)
    {
        ValidateIdentifier(grantId, nameof(grantId));
        if (request is null || string.IsNullOrWhiteSpace(request.ExpectedRevision) || request.ExpectedRevision.Length > 36 ||
            request.ExpectedRevision != request.ExpectedRevision.Trim() || request.ExpectedRevision.Any(char.IsControl))
            throw RefusalException.Invalid("The reviewed grant revision is required. Refresh before retiring access.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 1000 || request.Reason != request.Reason.Trim() || request.Reason.Any(char.IsControl))
            throw RefusalException.Invalid("Provide a retirement reason of at most 1000 characters without surrounding whitespace or control characters.");
        var actor = await RequireReviewAsync(databaseScope, token);
        if (repository.ChangeTracker.HasChanges() || repository.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("Grant retirement requires its own clean repository transaction.");
        await using var transaction = await repository.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        AppUserAssetAccess? aggregate = null; AssetGrantRetirement? audit = null;
        try
        {
            aggregate = await ReadReviewGrantAsync(grantId, actor.Scope, databaseScope, token);
            if (aggregate.ConcurrencyStamp != request.ExpectedRevision)
                throw RefusalException.Conflict("The stored grant changed. Refresh and review it again before retirement.");
            if (!aggregate.IsActive) throw RefusalException.Conflict("The stored grant is already inactive. Review its existing evidence.");
            var previous = aggregate.ConcurrencyStamp; var now = time.GetUtcNow().UtcDateTime; var revision = Guid.NewGuid().ToString("D");
            audit = new() { Revision = revision, GrantId = grantId, TenantId = actor.Scope.TenantId,
                OrganizationId = actor.Scope.BusinessAssociateId, DatabaseScope = databaseScope, PreviousRevision = previous,
                SnapshotJson = JsonSerializer.Serialize(aggregate), Reason = request.Reason, ActorUserId = actor.UserId, ChangedUtc = now };
            var tracked = repository.ChangeTracker.Entries<AppUserAssetAccess>().SingleOrDefault(entry => entry.Entity.Id == aggregate.Id);
            if (tracked is not null) tracked.State = EntityState.Detached;
            repository.Attach(aggregate);
            repository.Entry(aggregate).Property(row => row.ConcurrencyStamp).OriginalValue = previous;
            aggregate.IsActive = false; aggregate.ChangedBy = actor.UserId; aggregate.ChangedUtc = now; aggregate.ConcurrencyStamp = revision;
            repository.Add(audit);
            await RequireSameReviewAsync(actor, databaseScope, token);
            await repository.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return audit;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw RefusalException.Conflict("The stored grant changed. Refresh and review it again before retirement.");
        }
        finally
        {
            // A failed command must not leave either write queued for a later unrelated SaveChanges.
            if (aggregate is not null) repository.Entry(aggregate).State = EntityState.Detached;
            if (audit is not null) repository.Entry(audit).State = EntityState.Detached;
        }
    }

    private async Task<AppUserAssetAccess> ReadReviewGrantAsync(string id, CompanyScope scope, string database, CancellationToken token)
    {
        var rows = await repository.Set<AppUserAssetAccess>().AsNoTracking().Where(row => row.Id == id && row.OrganizationId == scope.BusinessAssociateId && row.DatabaseScope == database).ToListAsync(token);
        if (rows.Count != 1 || rows[0].Id != id || !Owned(rows[0], scope, database))
            throw RefusalException.Forbidden("The stored grant is unavailable in the selected company's reviewed database scope.");
        return rows[0];
    }

    private async Task<RepositoryCompanyContext> RequireReviewAsync(string database, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await RequireDatabaseScopeAsync(database);
        _ = RequireAdministrator();
        return company.Company;
    }
    private async Task RequireSameReviewAsync(RepositoryCompanyContext captured, string database, CancellationToken token)
    {
        var current = await RequireReviewAsync(database, token);
        if (current.UserId != captured.UserId || current.Scope != captured.Scope)
            throw RefusalException.Forbidden("The review account or selected company changed.");
    }
    private static bool Owned(AppUserAssetAccess row, CompanyScope scope, string database) => row.OrganizationId == scope.BusinessAssociateId && row.DatabaseScope == database;
    private static void ValidatePage(int offset, int limit)
    {
        if (offset < 0 || limit is < 1 or > 100) throw RefusalException.Invalid("Use a nonnegative offset and a page size from 1 to 100.");
    }
}
