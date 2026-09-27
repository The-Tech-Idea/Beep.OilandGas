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
        // Every provider carries the same migrations, in order (a count pinned here went stale on the next one added).
        using var reference = Create("SqlServer");
        Assert.Equal(Names(reference), Names(context));
        Assert.False(context.Database.HasPendingModelChanges());
        var script = context.GetService<IMigrator>().GenerateScript();
        Assert.Contains(expected, script);
        Assert.DoesNotContain(forbidden, script);
        foreach (var table in new[] { "AspNetUsers", "AspNetRoles", "AspNetUserRoles", "AspNetUserClaims",
            "AspNetRoleClaims", "AspNetUserLogins", "AspNetUserTokens", "RepositoryBootstrap", "ModuleDatabaseBindings",
            "APP_USER", "APP_USER_ASSET_ACCESS", "APP_ROLE", "APP_PERMISSION", "APP_USER_ROLE", "APP_ROLE_PERMISSION",
            "APP_PERSONA", "APP_USER_PERSONA", "APP_PERSONA_PREFERENCE", "APP_PERSONA_AUDIT" })
            Assert.Contains(table, script);
        Assert.DoesNotContain("CREATE TABLE WELL", script);
        Assert.Contains("__EFMigrationsHistory", script);
        var asset = context.Model.FindEntityType(typeof(TheTechIdea.Data.OilGas.AppUserAssetAccess))!;
        Assert.Equal("APP_USER_ASSET_ACCESS", asset.GetTableName());
        Assert.True(asset.FindProperty("ConcurrencyStamp")!.IsConcurrencyToken);
        Assert.Equal(64, asset.FindProperty("DatabaseScope")!.GetMaxLength());
        Assert.Contains(asset.GetForeignKeys(), key => key.PrincipalEntityType.ClrType == typeof(TheTechIdea.Data.OilGas.OilGasUser)
            && key.DeleteBehavior == DeleteBehavior.Restrict);
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
        Assert.Contains("AssetAccessExtensions", script);
    }

    /// <summary>A migration's name without its timestamp, which each provider's generation stamps differently.</summary>
    private static string[] Names(RepositoryDbContext context) =>
        context.Database.GetMigrations().Select(id => id[(id.IndexOf('_') + 1)..]).ToArray();

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
