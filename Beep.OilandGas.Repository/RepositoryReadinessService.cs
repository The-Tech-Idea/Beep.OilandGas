using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Repository;

public interface IRepositoryReadinessService
{
    Task<RepositoryReadiness> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed class RepositoryReadinessService(RepositoryDbContext context, IFailureReporter failures)
    : IRepositoryReadinessService
{
    public async Task<RepositoryReadiness> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await context.Database.CanConnectAsync(cancellationToken))
                return RepositoryReadiness.Unavailable;
            if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
                return RepositoryReadiness.MigrationRequired;
            if (!await context.Bootstrap.AnyAsync(x => x.Id == 1, cancellationToken))
                return await context.Users.AnyAsync(cancellationToken)
                    ? RepositoryReadiness.RecoveryRequired : RepositoryReadiness.BootstrapRequired;
            var hasAdministrator = await (from membership in context.UserRoles
                join account in context.Users on membership.UserId equals account.Id
                join role in context.Roles on membership.RoleId equals role.Id
                where account.IsActive && role.NormalizedName == "ADMINISTRATOR"
                select account.Id).AnyAsync(cancellationToken);
            return hasAdministrator ? RepositoryReadiness.Ready : RepositoryReadiness.RecoveryRequired;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        // Broad by design: a readiness check answers "unavailable" for whatever stopped it reading the repository, and the
        // three providers (SQL Server, PostgreSQL, Oracle) and EF's execution strategy each throw their own types.
        // "Unavailable" is this method's answer for exactly that — the setup gate and the health check act on it — and the
        // failure itself is reported (OILGAS-CATCH-01), so its cause is in the store, not only in a log line.
        catch (Exception exception)
        {
            failures.ReportHandled(exception, "checking whether the default repository is ready",
                "readiness answers Unavailable: the setup gate holds requests with 503 and the health check reports the repository down",
                FailureSeverity.Error);
            return RepositoryReadiness.Unavailable;
        }
    }
}
