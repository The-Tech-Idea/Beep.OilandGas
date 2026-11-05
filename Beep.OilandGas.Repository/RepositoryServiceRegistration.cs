using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public static class RepositoryServiceRegistration
{
    public static IServiceCollection AddOilGasRepository(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IRepositoryConnectionSource>(_ => new ConfigurationRepositoryConnectionSource(configuration));
        services.AddScoped(sp => RepositoryConnectionSnapshot.Capture(sp.GetRequiredService<IRepositoryConnectionSource>().Resolve()));
        services.AddDbContext<SqlServerRepositoryDbContext>((sp, options) => options.UseSqlServer(sp.GetRequiredService<RepositoryConnectionSnapshot>().Require("SQLSERVER")));
        services.AddDbContext<PostgreSqlRepositoryDbContext>((sp, options) => options.UseNpgsql(sp.GetRequiredService<RepositoryConnectionSnapshot>().Require("POSTGRESQL")));
        services.AddDbContext<OracleRepositoryDbContext>((sp, options) => options.UseOracle(sp.GetRequiredService<RepositoryConnectionSnapshot>().Require("ORACLE"),
            oracle => oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));
        services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<RepositoryConnectionSnapshot>().Settings?.Provider switch
        {
            "SQLSERVER" => sp.GetRequiredService<SqlServerRepositoryDbContext>(),
            "POSTGRESQL" => sp.GetRequiredService<PostgreSqlRepositoryDbContext>(),
            "ORACLE" => sp.GetRequiredService<OracleRepositoryDbContext>(),
            _ => throw new InvalidOperationException("Configure a supported identity repository connection.")
        });

        // Identity stores only: do not replace the API's external bearer authentication.
        services.AddIdentityCore<OilGasUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<RepositoryDbContext>();
        services.AddScoped<RepositoryReadinessService>();
        services.AddScoped<IRepositoryReadinessService>(sp => sp.GetRequiredService<RepositoryConnectionSnapshot>().Settings is null
            ? new UnconfiguredRepositoryReadinessService() : sp.GetRequiredService<RepositoryReadinessService>());
        services.AddScoped<RepositoryBootstrapService>();
        services.AddScoped<RepositoryRoleCatalogService>();
        services.AddScoped<RepositoryPersonaService>();
        services.AddScoped<IRepositoryAccessService, RepositoryAccessService>();
        services.AddScoped<RepositoryCompanyContextResolver>();
        services.AddScoped<RepositoryTenantContext>();
        return services;
    }
}

/// <summary>Allows the setup gate to report a missing repository configuration without creating a database context.</summary>
public sealed class UnconfiguredRepositoryReadinessService : IRepositoryReadinessService
{
    public Task<RepositoryReadiness> CheckAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(RepositoryReadiness.Unavailable);
    }
}
