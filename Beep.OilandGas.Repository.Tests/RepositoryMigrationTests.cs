using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public class RepositoryMigrationTests
{
    [Theory]
    [InlineData("SqlServer", "nvarchar", "NVARCHAR2")]
    [InlineData("PostgreSql", "character varying", "NVARCHAR2")]
    [InlineData("Oracle", "NVARCHAR2", "nvarchar(max)")]
    public void InitialMigrationGeneratesProviderSqlWithoutConnecting(string provider, string expected, string forbidden)
    {
        using var context = Create(provider);
        Assert.Equal(4, context.Database.GetMigrations().Count());
        Assert.False(context.Database.HasPendingModelChanges());
        var script = context.GetService<IMigrator>().GenerateScript();
        Assert.Contains(expected, script);
        Assert.DoesNotContain(forbidden, script);
        foreach (var table in new[] { "AspNetUsers", "AspNetRoles", "AspNetUserRoles", "AspNetUserClaims",
            "AspNetRoleClaims", "AspNetUserLogins", "AspNetUserTokens", "RepositoryBootstrap", "ModuleDatabaseBindings",
            "APP_USER", "APP_ROLE", "APP_PERMISSION", "APP_USER_ROLE", "APP_ROLE_PERMISSION",
            "APP_PERSONA", "APP_USER_PERSONA", "APP_PERSONA_PREFERENCE", "APP_PERSONA_AUDIT" })
            Assert.Contains(table, script);
        Assert.DoesNotContain("CREATE TABLE WELL", script);
        Assert.Contains("__EFMigrationsHistory", script);
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSql")]
    [InlineData("Oracle")]
    public void InitialMigrationCanGenerateIdempotentScript(string provider)
    {
        using var context = Create(provider);
        var script = context.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
        Assert.Contains("InitialRepository", script);
        Assert.Contains("AspNetUsers", script);
    }

    private static RepositoryDbContext Create(string provider) => provider switch
    {
        "SqlServer" => new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer("Server=localhost;Database=Unused;Integrated Security=true").Options),
        "PostgreSql" => new PostgreSqlRepositoryDbContext(new DbContextOptionsBuilder<PostgreSqlRepositoryDbContext>()
            .UseNpgsql("Host=localhost;Database=Unused;Username=unused").Options),
        "Oracle" => new OracleRepositoryDbContext(new DbContextOptionsBuilder<OracleRepositoryDbContext>()
            .UseOracle("Data Source=localhost/FREEPDB1;User Id=unused;Password=unused", oracle =>
                oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)).Options),
        _ => throw new ArgumentOutOfRangeException(nameof(provider))
    };
}
