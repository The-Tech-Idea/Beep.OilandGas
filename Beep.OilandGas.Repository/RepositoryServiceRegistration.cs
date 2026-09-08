using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public static class RepositoryServiceRegistration
{
    public static IServiceCollection AddOilGasRepository(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Repository:Provider"];
        var connection = configuration["Repository:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Configure Repository:ConnectionString independently of module connections.");

        switch (provider?.Trim().ToUpperInvariant())
        {
            case "SQLSERVER":
                services.AddDbContext<SqlServerRepositoryDbContext>(options => options.UseSqlServer(connection));
                services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<SqlServerRepositoryDbContext>());
                break;
            case "POSTGRESQL":
                services.AddDbContext<PostgreSqlRepositoryDbContext>(options => options.UseNpgsql(connection));
                services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<PostgreSqlRepositoryDbContext>());
                break;
            case "ORACLE":
                services.AddDbContext<OracleRepositoryDbContext>(options => options.UseOracle(connection,
                    oracle => oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));
                services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<OracleRepositoryDbContext>());
                break;
            default:
                throw new InvalidOperationException("Repository:Provider must be SqlServer, PostgreSql, or Oracle.");
        }

        // Identity stores only: do not replace the API's external bearer authentication.
        services.AddIdentityCore<OilGasUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<RepositoryDbContext>();
        services.AddScoped<RepositoryReadinessService>();
        services.AddScoped<IRepositoryReadinessService>(sp => sp.GetRequiredService<RepositoryReadinessService>());
        services.AddScoped<RepositoryBootstrapService>();
        services.AddScoped<RepositoryRoleCatalogService>();
        services.AddScoped<RepositoryPersonaService>();
        services.AddScoped<IRepositoryAccessService, RepositoryAccessService>();
        return services;
    }
}
