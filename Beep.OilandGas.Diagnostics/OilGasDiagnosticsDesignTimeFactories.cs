using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beep.OilandGas.Diagnostics;

/// <summary>The connection <c>dotnet ef</c> works against, named for the chosen engine — never a default.</summary>
internal static class OilGasDiagnosticsDesignTimeConnection
{
    public const string Variable = "OILGAS_DIAGNOSTICS_CONNECTION";

    public static string Get() => Environment.GetEnvironmentVariable(Variable)
        ?? throw new InvalidOperationException($"Set {Variable} for the engine whose migrations you are working on.");
}

public sealed class SqlServerOilGasDiagnosticsFactory : IDesignTimeDbContextFactory<SqlServerOilGasDiagnosticsDbContext>
{
    public SqlServerOilGasDiagnosticsDbContext CreateDbContext(string[] args) => new(
        (DbContextOptions<SqlServerOilGasDiagnosticsDbContext>)OilGasDiagnosticsEngines.UseSqlServer(
            new DbContextOptionsBuilder<SqlServerOilGasDiagnosticsDbContext>(), OilGasDiagnosticsDesignTimeConnection.Get()).Options);
}

public sealed class PostgreSqlOilGasDiagnosticsFactory : IDesignTimeDbContextFactory<PostgreSqlOilGasDiagnosticsDbContext>
{
    public PostgreSqlOilGasDiagnosticsDbContext CreateDbContext(string[] args) => new(
        (DbContextOptions<PostgreSqlOilGasDiagnosticsDbContext>)OilGasDiagnosticsEngines.UsePostgreSql(
            new DbContextOptionsBuilder<PostgreSqlOilGasDiagnosticsDbContext>(), OilGasDiagnosticsDesignTimeConnection.Get()).Options);
}

public sealed class OracleOilGasDiagnosticsFactory : IDesignTimeDbContextFactory<OracleOilGasDiagnosticsDbContext>
{
    public OracleOilGasDiagnosticsDbContext CreateDbContext(string[] args) => new(
        (DbContextOptions<OracleOilGasDiagnosticsDbContext>)OilGasDiagnosticsEngines.UseOracle(
            new DbContextOptionsBuilder<OracleOilGasDiagnosticsDbContext>(), OilGasDiagnosticsDesignTimeConnection.Get()).Options);
}
