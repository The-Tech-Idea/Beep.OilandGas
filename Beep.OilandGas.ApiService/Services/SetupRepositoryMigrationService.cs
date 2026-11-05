using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TheTechIdea.Data.OilGas;
using System.Security.Cryptography;
using System.Text;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reviews and applies EF migrations only to the identity repository configured on this server.</summary>
public sealed class SetupRepositoryMigrationService(RepositoryDbContext repository, IRepositoryReadinessService readiness)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly byte[] TargetKey = RandomNumberGenerator.GetBytes(32);

    public async Task<SetupRepositoryMigrationPlan> PlanAsync(Func<Task> authority, CancellationToken token)
    {
        await authority();
        var status = await readiness.CheckAsync(token);
        var createsDatabase = status == RepositoryReadiness.Unavailable &&
            !await repository.GetService<IRelationalDatabaseCreator>().ExistsAsync(token);
        if (status != RepositoryReadiness.MigrationRequired && !createsDatabase)
            throw new InvalidOperationException("The configured identity repository is not awaiting migration.");
        var provider = repository.Database.ProviderName ?? throw new InvalidOperationException("The identity repository provider is unavailable.");
        var pending = createsDatabase ? repository.Database.GetMigrations().ToArray() :
            (await repository.Database.GetPendingMigrationsAsync(token)).ToArray();
        if (pending.Length == 0) throw new InvalidOperationException("No pending identity repository migrations were found.");
        if (!createsDatabase && pending.Any(x => x.EndsWith("_CompanyPersonaScope", StringComparison.Ordinal)))
        {
            var applied = await repository.Database.GetAppliedMigrationsAsync(token);
            if (applied.Any(x => x.EndsWith("_PersonaExtensions", StringComparison.Ordinal)))
                await PersonaScopeMigrationPreflight.RequireEmptyAsync(repository, token);
        }
        await authority();
        var target = Convert.ToHexString(HMACSHA256.HashData(TargetKey, Encoding.UTF8.GetBytes(provider + "\0" + repository.Database.GetDbConnection().ConnectionString)));
        return new(provider, target, createsDatabase, pending);
    }

    public async Task<SetupRepositoryMigrationResult> ApplyAsync(SetupRepositoryMigrationRequest request, Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.PendingMigrations is null || request.PendingMigrations.Count == 0 || request.PendingMigrations.Count > 1000)
            throw new InvalidOperationException("Review the pending migrations before applying them.");
        await Gate.WaitAsync(token);
        try
        {
            var current = await PlanAsync(authority, token);
            if (!string.Equals(current.Provider, request.Provider, StringComparison.Ordinal) ||
                !string.Equals(current.TargetToken, request.TargetToken, StringComparison.Ordinal) ||
                current.CreatesDatabase != request.CreatesDatabase ||
                !current.PendingMigrations.SequenceEqual(request.PendingMigrations, StringComparer.Ordinal))
                throw new InvalidOperationException("The identity repository migration plan changed. Review it again.");
            await authority();
            token.ThrowIfCancellationRequested();
            await repository.Database.MigrateAsync(token);
            var result = await readiness.CheckAsync(token);
            return new(result);
        }
        finally { Gate.Release(); }
    }
}
