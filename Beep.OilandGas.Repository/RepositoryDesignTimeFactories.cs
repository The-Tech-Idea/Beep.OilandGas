using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beep.OilandGas.Repository;

internal static class RepositoryDesignTimeConnection
{
    public static string Get() => Environment.GetEnvironmentVariable("OILGAS_REPOSITORY_CONNECTION")
        ?? throw new InvalidOperationException("Set OILGAS_REPOSITORY_CONNECTION for the selected repository provider.");
}

public sealed class SqlServerRepositoryFactory : IDesignTimeDbContextFactory<SqlServerRepositoryDbContext>
{
    public SqlServerRepositoryDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer(RepositoryDesignTimeConnection.Get()).Options);
}

public sealed class PostgreSqlRepositoryFactory : IDesignTimeDbContextFactory<PostgreSqlRepositoryDbContext>
{
    public PostgreSqlRepositoryDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<PostgreSqlRepositoryDbContext>()
            .UseNpgsql(RepositoryDesignTimeConnection.Get()).Options);
}

public sealed class OracleRepositoryFactory : IDesignTimeDbContextFactory<OracleRepositoryDbContext>
{
    public OracleRepositoryDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<OracleRepositoryDbContext>()
            .UseOracle(RepositoryDesignTimeConnection.Get(), oracle =>
                oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)).Options);
}
