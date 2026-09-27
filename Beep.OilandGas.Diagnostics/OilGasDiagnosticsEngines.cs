using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.Diagnostics;

/// <summary>
/// How the failure store is opened on each engine — one definition, used by the running hosts and by <c>dotnet ef</c>
/// alike, so the migrations are generated against the same options they run with.
/// </summary>
public static class OilGasDiagnosticsEngines
{
    public static DbContextOptionsBuilder UseSqlServer(DbContextOptionsBuilder options, string connection) =>
        options.UseSqlServer(connection, sql => sql.MigrationsHistoryTable(OilGasDiagnosticsDbContext.MigrationsHistoryTable));

    public static DbContextOptionsBuilder UsePostgreSql(DbContextOptionsBuilder options, string connection) =>
        options.UseNpgsql(connection, npgsql => npgsql.MigrationsHistoryTable(OilGasDiagnosticsDbContext.MigrationsHistoryTable));

    public static DbContextOptionsBuilder UseOracle(DbContextOptionsBuilder options, string connection) =>
        options.UseOracle(connection, oracle =>
        {
            // The repository's compatibility level, so both stores accept the same servers.
            oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
            oracle.MigrationsHistoryTable(OilGasDiagnosticsDbContext.MigrationsHistoryTable);
        });
}
