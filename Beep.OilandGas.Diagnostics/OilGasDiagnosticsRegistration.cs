using Beep.IdentityServer.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.Diagnostics;

/// <summary>
/// DIAG-01 for OilGas: the log-and-failure service over <see cref="OilGasDiagnosticsDbContext"/>, for the Web and the API.
/// </summary>
/// <remarks>
/// <para>
/// The identity server's client library reports every failure it catches through the application's
/// <c>IFailureReporter</c> and refuses to start without one; this is OilGas's. Every failure is logged through the
/// framework's logging and stored under the reference the person is shown; the Web's administrator reads the list at
/// <c>/admin/failures</c>.
/// </para>
/// <para>
/// Configured the way the repository is — an engine and a connection string, in the host's
/// <c>appsettings.{Environment}.json</c> or the server's own copy. Nothing falls back: a missing or template value stops
/// the host here, naming the setting.
/// </para>
/// </remarks>
public static class OilGasDiagnosticsRegistration
{
    /// <summary>SqlServer, PostgreSql or Oracle.</summary>
    public const string ProviderSetting = "Diagnostics:Provider";

    /// <summary>The failure store's database — the repository's own, or one of its own.</summary>
    public const string ConnectionSetting = "Diagnostics:ConnectionString";

    /// <summary>
    /// Whether the host that owns the schema (the API) applies the store's migrations as it starts: a host with no command
    /// line to run <c>dotnet ef database update</c> sets it.
    /// </summary>
    public const string MigrateOnStartupSetting = "Diagnostics:MigrateOnStartup";

    public static IServiceCollection AddOilGasDiagnostics(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var engine = Engine(configuration);
        var connection = Connection(configuration);

        switch (engine)
        {
            case OilGasDatabaseEngine.SqlServer:
                services.AddDbContext<SqlServerOilGasDiagnosticsDbContext>(options => OilGasDiagnosticsEngines.UseSqlServer(options, connection));
                services.AddScoped<OilGasDiagnosticsDbContext>(sp => sp.GetRequiredService<SqlServerOilGasDiagnosticsDbContext>());
                break;

            case OilGasDatabaseEngine.PostgreSql:
                services.AddDbContext<PostgreSqlOilGasDiagnosticsDbContext>(options => OilGasDiagnosticsEngines.UsePostgreSql(options, connection));
                services.AddScoped<OilGasDiagnosticsDbContext>(sp => sp.GetRequiredService<PostgreSqlOilGasDiagnosticsDbContext>());
                break;

            case OilGasDatabaseEngine.Oracle:
                services.AddDbContext<OracleOilGasDiagnosticsDbContext>(options => OilGasDiagnosticsEngines.UseOracle(options, connection));
                services.AddScoped<OilGasDiagnosticsDbContext>(sp => sp.GetRequiredService<OracleOilGasDiagnosticsDbContext>());
                break;
        }

        return services.AddTheTechIdeaDiagnostics<OilGasDiagnosticsDbContext>();
    }

    /// <summary>
    /// Applies the store's migrations when <see cref="MigrateOnStartupSetting"/> says so. Called by the API only: it owns
    /// the schema, and two hosts migrating one database at once is a race neither can see.
    /// </summary>
    public static async Task ApplyOilGasDiagnosticsSchemaAsync(
        this IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        if (!configuration.GetValue<bool>(MigrateOnStartupSetting))
        {
            return;
        }

        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OilGasDiagnosticsDbContext>().Database.MigrateAsync(cancellationToken);
    }

    /// <summary>The engine <see cref="ProviderSetting"/> names; anything else stops the host, naming the setting.</summary>
    public static OilGasDatabaseEngine Engine(IConfiguration configuration)
    {
        var value = configuration[ProviderSetting]?.Trim();

        // By name only: Enum.TryParse would also take "1" or a comma list.
        var named = Enum.GetValues<OilGasDatabaseEngine>()
            .Where(engine => string.Equals(engine.ToString(), value, StringComparison.OrdinalIgnoreCase))
            .Select(engine => (OilGasDatabaseEngine?)engine)
            .FirstOrDefault();

        return named is { } chosen
            ? chosen
            : throw new InvalidOperationException(
                $"{ProviderSetting} must be SqlServer, PostgreSql or Oracle (the engine OilGas's failure store runs on). "
                + "Set it in appsettings.{Environment}.json.");
    }

    private static string Connection(IConfiguration configuration)
    {
        var value = configuration[ConnectionSetting];

        return !string.IsNullOrWhiteSpace(value) && !ConfigurationPlaceholder.IsPlaceholder(value)
            ? value
            : throw new InvalidOperationException(
                $"{ConnectionSetting} is not configured. Name the database OilGas keeps its failures in — the repository's own, "
                + "or one of its own — in appsettings.{Environment}.json.");
    }
}
