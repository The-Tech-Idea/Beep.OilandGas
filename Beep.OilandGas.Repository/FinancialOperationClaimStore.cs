using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

/// <summary>
/// Atomic control-plane ownership. No expiry, forced takeover or automatic crash recovery.
/// This alone does not fence writes performed on a separate module database.
/// </summary>
public sealed class FinancialOperationClaimStore(RepositoryDbContext db)
{
    public async Task<FinancialOperationClaim?> ReadAsync(string key, CancellationToken cancellationToken = default)
    {
        Validate(key, nameof(key));
        return await db.Set<FinancialOperationClaim>().AsNoTracking().SingleOrDefaultAsync(x => x.OperationKey == key, cancellationToken);
    }

    public async Task<bool> InitializeAsync(string key, string actor, CancellationToken cancellationToken = default)
    {
        Validate(key, nameof(key)); Validate(actor, nameof(actor));
        if (await ReadAsync(key, cancellationToken) is not null) return false;
        var row = new FinancialOperationClaim { OperationKey = key, ChangedBy = actor, ChangedUtc = DateTime.UtcNow };
        db.Add(row);
        try { await db.SaveChangesAsync(cancellationToken); return true; }
        catch (DbUpdateException)
        {
            // A concurrent initializer can win the primary key. Other failures must propagate.
            db.Entry(row).State = EntityState.Detached;
            if (await ReadAsync(key, cancellationToken) is not null) return false;
            throw;
        }
        finally { db.Entry(row).State = EntityState.Detached; }
    }

    public async Task<FinancialClaimHandle?> TryAcquireAsync(string key, long expectedVersion, string owner,
        CancellationToken cancellationToken = default)
    {
        Validate(key, nameof(key)); Validate(owner, nameof(owner)); ValidateVersion(expectedVersion);
        var token = Guid.NewGuid().ToString();
        var next = checked(expectedVersion + 1);
        var now = DateTime.UtcNow;
        var count = await db.Set<FinancialOperationClaim>()
            .Where(x => x.OperationKey == key && x.Version == expectedVersion && x.Token == null && x.OwnerId == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.Version, next).SetProperty(x => x.Token, token)
                .SetProperty(x => x.OwnerId, owner).SetProperty(x => x.ChangedBy, owner).SetProperty(x => x.ChangedUtc, now), cancellationToken);
        CheckCount(count);
        return count == 1 ? new(key, next, owner, token) : null;
    }

    public async Task<bool> ReleaseAsync(FinancialClaimHandle handle, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(handle);
        Validate(handle.OperationKey, nameof(handle.OperationKey)); Validate(handle.OwnerId, nameof(handle.OwnerId));
        ValidateVersion(handle.Version);
        if (!Guid.TryParseExact(handle.Token, "D", out var token) || token == Guid.Empty)
            throw new ArgumentException("A valid claim token is required.", nameof(handle));
        var next = checked(handle.Version + 1);
        var now = DateTime.UtcNow;
        var count = await db.Set<FinancialOperationClaim>()
            .Where(x => x.OperationKey == handle.OperationKey && x.Version == handle.Version && x.Token == handle.Token && x.OwnerId == handle.OwnerId)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.Version, next).SetProperty(x => x.Token, (string?)null)
                .SetProperty(x => x.OwnerId, (string?)null).SetProperty(x => x.ChangedBy, handle.OwnerId).SetProperty(x => x.ChangedUtc, now), cancellationToken);
        CheckCount(count);
        return count == 1;
    }

    private static void Validate(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        if (value.Length > 128) throw new ArgumentException("Claim keys and actors are limited to 128 characters.", name);
    }
    private static void ValidateVersion(long version)
    {
        if (version < 0 || version == long.MaxValue) throw new ArgumentOutOfRangeException(nameof(version));
    }
    private static void CheckCount(int count)
    {
        if (count is not (0 or 1)) throw new InvalidOperationException("Provider did not confirm the conditional claim update row count.");
    }
}
