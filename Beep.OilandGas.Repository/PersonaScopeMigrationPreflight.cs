using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.Repository;

public sealed class PersonaScopeTransitionRequiredException() : InvalidOperationException(
    "Account-wide persona settings exist. Assign their intended companies through a reviewed data transition before applying the company persona migration.");

/// <summary>Checks the old account-wide tables before assigning company-scoped keys.</summary>
public static class PersonaScopeMigrationPreflight
{
    public static async Task RequireEmptyAsync(RepositoryDbContext repository, CancellationToken token)
    {
        var connection = repository.Database.GetDbConnection();
        var options = new DbContextOptionsBuilder<InspectionContext>();
        switch (repository.Database.ProviderName)
        {
            case "Microsoft.EntityFrameworkCore.SqlServer": options.UseSqlServer(connection); break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL": options.UseNpgsql(connection); break;
            case "Oracle.EntityFrameworkCore": options.UseOracle(connection); break;
            default: throw new InvalidOperationException("The repository provider does not support persona migration preflight.");
        }
        await using var inspection = new InspectionContext(options.Options);
        if (await inspection.Set<ProfileRow>().AnyAsync(token) || await inspection.Set<PreferenceRow>().AnyAsync(token))
            throw new PersonaScopeTransitionRequiredException();
    }

    private sealed class ProfileRow { public string UserId { get; set; } = ""; }
    private sealed class PreferenceRow { public string UserId { get; set; } = ""; }
    private sealed class InspectionContext(DbContextOptions<InspectionContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProfileRow>(entity => { entity.HasNoKey(); entity.ToTable("APP_USER_PERSONA"); });
            builder.Entity<PreferenceRow>(entity => { entity.HasNoKey(); entity.ToTable("APP_PERSONA_PREFERENCE"); });
        }
    }
}
