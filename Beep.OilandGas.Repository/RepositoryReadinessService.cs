using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public interface IRepositoryReadinessService
{
    Task<RepositoryReadiness> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed class RepositoryReadinessService(RepositoryDbContext context, ILogger<RepositoryReadinessService> logger)
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
        catch (Exception exception)
        {
            logger.LogError(exception, "Default repository readiness check failed");
            return RepositoryReadiness.Unavailable;
        }
    }
}
