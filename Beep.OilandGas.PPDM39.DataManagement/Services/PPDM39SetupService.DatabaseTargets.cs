using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor.Migration;

namespace Beep.OilandGas.PPDM39.DataManagement.Services;

public partial class PPDM39SetupService
{
    private sealed record DatabaseTargetReviewSession(string Actor, LocalDbDatabaseTargetRequest Target,
        DatabaseTargetMigration Migration, DateTime ExpiresUtc, bool CanCreate);

    private static readonly ConcurrentDictionary<string, DatabaseTargetReviewSession> DatabaseTargetReviews = new(StringComparer.Ordinal);

    /// <summary>Reviews the server/catalog before opening a target connection or writing an ownership claim.</summary>
    public LocalDbDatabaseTargetReview ReviewLocalDbDatabaseTarget(LocalDbDatabaseTargetRequest request, string actor)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireDatabaseTargetActor(actor);
        if (_databaseTargetMigration is null)
            throw RefusalException.Conflict("Database catalog creation is unavailable in this installation. Use an existing database connection.");

        var now = DateTime.UtcNow;
        foreach (var item in DatabaseTargetReviews)
            if (item.Value.ExpiresUtc <= now) DatabaseTargetReviews.TryRemove(item.Key, out _);

        // The host binds the existing native provider, validates the exact target and protects its default repositories.
        var migration = _databaseTargetMigration(request);
        var observed = migration.Inspect();
        var owned = observed.OwnershipConfirmed;
        var unsettled = observed.RequiresReconciliation;
        var canCreate = !observed.DatabaseExists && !unsettled;
        var token = Guid.NewGuid().ToString("N");
        if (!DatabaseTargetReviews.TryAdd(token, new(actor, request, migration, now.AddMinutes(15), canCreate)))
            throw RefusalException.Conflict("The database target review could not be retained. Review it again.");
        var message = owned ? "This installation's database ownership is confirmed. Test the connection before saving." :
            unsettled ? "An earlier database creation needs reconciliation. Preserve its ownership receipt and review the target before continuing." :
            observed.DatabaseExists ? "The database already exists. Use the existing-database connection path; no database will be adopted or replaced." :
            "The database does not exist. Review this target before creating an empty module database.";
        return new(request, token, observed.DatabaseExists, owned, unsettled, message);
    }

    /// <summary>Consumes one exact actor-bound review; BeepDM owns creation, durable ownership and uncertain outcomes.</summary>
    public async Task<LocalDbDatabaseTargetResult> CreateLocalDbDatabaseTargetAsync(LocalDbDatabaseTargetCreate request,
        string actor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireDatabaseTargetActor(actor);
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(request.ReviewToken) ||
            !DatabaseTargetReviews.TryGetValue(request.ReviewToken, out var review) ||
            review.Actor != actor || review.Target != request.Target || !review.CanCreate || review.ExpiresUtc <= DateTime.UtcNow)
            throw RefusalException.Conflict("The database target review changed or expired. Review the exact target again.");
        if (!DatabaseTargetReviews.TryRemove(request.ReviewToken, out _))
            throw RefusalException.Conflict("This database creation was already attempted. Review its outcome before continuing.");

        // No automatic retry: a failed/uncertain attempt remains recorded by the framework's durable target claim.
        await review.Migration.EnsureCreatedAsync(cancellationToken);
        if (!review.Migration.Exists)
            throw RefusalException.Conflict("Database ownership could not be confirmed. Preserve the target for reconciliation.");
        return new(review.Target, request.ReviewToken, OwnershipConfirmed: true);
    }

    private static void RequireDatabaseTargetActor(string actor)
    {
        if (string.IsNullOrWhiteSpace(actor)) throw RefusalException.Forbidden("A resolved repository administrator is required.");
    }
}
