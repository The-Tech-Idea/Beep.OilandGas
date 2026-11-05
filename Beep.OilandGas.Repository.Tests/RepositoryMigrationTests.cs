using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public class RepositoryMigrationTests
{
    [Theory]
    [InlineData("SqlServer")] [InlineData("PostgreSql")] [InlineData("Oracle")]
    public void Retirement_upgrade_adds_only_the_owned_audit_table_and_generated_constraints(string provider)
    {
        using var context = Create(provider); var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(value => value.Key.EndsWith("_AssetGrantRetirement", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        var table = Assert.Single(migration.UpOperations.OfType<CreateTableOperation>());
        Assert.Equal("APP_ASSET_GRANT_RETIREMENT", table.Name);
        Assert.Equal(3, table.ForeignKeys.Count); Assert.All(table.ForeignKeys, key => Assert.Equal(ReferentialAction.Restrict, key.OnDelete));
        Assert.Equal(1000, table.Columns.Single(column => column.Name == "Reason").MaxLength);
        Assert.DoesNotContain(migration.UpOperations, operation => operation is SqlOperation or InsertDataOperation or UpdateDataOperation);
        Assert.DoesNotContain(migration.DownOperations, operation => operation is SqlOperation);
        Assert.False(context.Database.HasPendingModelChanges());
    }
    [Theory]
    [InlineData("SqlServer")] [InlineData("PostgreSql")] [InlineData("Oracle")]
    public void Facility_qualifier_upgrade_adds_only_a_nullable_column_without_guessing_historical_types(string provider)
    {
        using var context = Create(provider); var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(value => value.Key.EndsWith("_CompositeFacilityGrants", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        var operation = Assert.IsType<AddColumnOperation>(Assert.Single(migration.UpOperations));
        Assert.Equal("APP_USER_ASSET_ACCESS", operation.Table); Assert.Equal("FacilityType", operation.Name); Assert.Equal(128, operation.MaxLength); Assert.True(operation.IsNullable); Assert.Null(operation.DefaultValue);
        Assert.DoesNotContain(migration.DownOperations, value => value is SqlOperation);
        Assert.False(context.Database.HasPendingModelChanges());
    }
    [Theory]
    [InlineData("SqlServer")] [InlineData("PostgreSql")] [InlineData("Oracle")]
    public void Hierarchy_configuration_schema_uses_only_EF_operations_with_company_owner_audit_and_revision(string provider)
    {
        using var context = Create(provider);
        var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(value => value.Key.EndsWith("_CompanyHierarchyConfiguration", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        Assert.DoesNotContain(migration.UpOperations, operation => operation is SqlOperation or DropTableOperation or InsertDataOperation);
        Assert.DoesNotContain(migration.DownOperations, operation => operation is SqlOperation);
        var tables = migration.UpOperations.OfType<CreateTableOperation>().ToArray();
        Assert.Equal(new[] { "APP_HIERARCHY_REVISION", "ORGANIZATION_HIERARCHY_CONFIG" }, tables.Select(table => table.Name).Order());
        var owner = Assert.Single(tables, table => table.Name == "ORGANIZATION_HIERARCHY_CONFIG");
        Assert.Contains(owner.ForeignKeys, key => key.PrincipalTable == "APP_COMPANY" && key.Columns.SequenceEqual(new[] { "TENANT_ID", "ORGANIZATION_ID" }));
        Assert.Contains(owner.Columns, column => column.Name == "REVISION" && column.MaxLength == 36 && !column.IsNullable);
        Assert.Contains(migration.UpOperations.OfType<CreateIndexOperation>(), index => index.IsUnique && index.Columns.SequenceEqual(new[] { "TENANT_ID", "ORGANIZATION_ID" }));
        var mapped = context.Model.FindEntityType(typeof(Beep.OilandGas.Models.Data.Security.ORGANIZATION_HIERARCHY_CONFIG))!;
        Assert.True(mapped.FindProperty("REVISION")!.IsConcurrencyToken);
        Assert.Equal(typeof(TheTechIdea.Data.OilGas.AppTenant).Assembly, mapped.ClrType.Assembly);
        Assert.All(mapped.GetForeignKeys(), key => Assert.Equal(DeleteBehavior.Restrict, key.DeleteBehavior));
    }
    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSql")]
    [InlineData("Oracle")]
    public void Physical_target_upgrade_preserves_unverified_owners_and_enforces_unique_targets(string provider)
    {
        using var context = Create(provider);
        var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(x => x.Key.EndsWith("_PhysicalModuleTargetOwnership", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        Assert.DoesNotContain(migration.UpOperations, x => x is SqlOperation or DropTableOperation or DeleteDataOperation or InsertDataOperation);
        Assert.DoesNotContain(migration.DownOperations, x => x is SqlOperation);
        var columns = migration.UpOperations.OfType<AddColumnOperation>().ToArray();
        Assert.Equal(3, columns.Length);
        Assert.All(columns, column => { Assert.Equal("APP_MODULE_CONNECTION", column.Table); Assert.True(column.IsNullable); Assert.Null(column.DefaultValue); });
        Assert.Equal(64, Assert.Single(columns, column => column.Name == "PhysicalTargetKey").MaxLength);
        var index = Assert.Single(migration.UpOperations.OfType<CreateIndexOperation>());
        Assert.True(index.IsUnique); Assert.Equal(new[] { "PhysicalTargetKey" }, index.Columns);
    }
    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSql")]
    [InlineData("Oracle")]
    public void Tenant_bindings_preserve_unowned_rows_and_use_EF_operations(string provider)
    {
        using var context = Create(provider);
        var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(x => x.Key.EndsWith("_TenantModuleBindings", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        Assert.DoesNotContain(migration.UpOperations, x => x is SqlOperation or DropTableOperation or DeleteDataOperation or InsertDataOperation);
        Assert.DoesNotContain(migration.DownOperations, x => x is SqlOperation);
        var tenant = Assert.Single(migration.UpOperations.OfType<AddColumnOperation>(), x => x.Name == "TenantId");
        Assert.True(tenant.IsNullable);
        Assert.Null(tenant.DefaultValue);
        var primary = Assert.Single(migration.UpOperations.OfType<AddPrimaryKeyOperation>());
        Assert.Equal(new[] { "BindingId" }, primary.Columns);
        Assert.Equal("APP_MODULE_CONNECTION", Assert.Single(migration.UpOperations.OfType<CreateTableOperation>()).Name);
        Assert.Contains(migration.UpOperations.OfType<CreateIndexOperation>(), x => x.IsUnique && x.Columns.SequenceEqual(new[] { "TenantId", "ModuleId" }));
        Assert.Contains(migration.UpOperations.OfType<AddForeignKeyOperation>(), x => x.PrincipalTable == "APP_MODULE_CONNECTION" && x.Columns.SequenceEqual(new[] { "TenantId", "ConnectionKey" }));
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSql")]
    [InlineData("Oracle")]
    public void Company_revocation_audit_uses_only_EF_operations(string provider)
    {
        using var context = Create(provider);
        var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(x => x.Key.EndsWith("_CompanyRoleRevocation", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        Assert.DoesNotContain(migration.UpOperations, operation => operation is SqlOperation);
        Assert.DoesNotContain(migration.DownOperations, operation => operation is SqlOperation);
        var column = Assert.Single(migration.UpOperations.OfType<AddColumnOperation>());
        Assert.Equal("APP_USER_ROLE", column.Table);
        Assert.Equal("REVOKED_BY_USER_ID", column.Name);
        Assert.True(column.IsNullable);
        Assert.Equal(128, column.MaxLength);
    }

    [Theory]
    [InlineData("SqlServer")]
    [InlineData("PostgreSql")]
    [InlineData("Oracle")]
    public void Company_schema_uses_only_EF_migration_operations(string provider)
    {
        using var context = Create(provider);
        var assembly = context.GetService<IMigrationsAssembly>();
        var entry = assembly.Migrations.Single(x => x.Key.EndsWith("_CompanyAccess", StringComparison.Ordinal));
        var migration = assembly.CreateMigration(entry.Value, context.Database.ProviderName!);
        Assert.DoesNotContain(migration.UpOperations, operation => operation is SqlOperation);
        Assert.DoesNotContain(migration.DownOperations, operation => operation is SqlOperation);
        var tables = migration.UpOperations.OfType<CreateTableOperation>().Select(x => x.Name).Order().ToArray();
        Assert.Equal(new[] { "APP_COMPANY", "APP_COMPANY_MEMBERSHIP", "APP_TENANT" }, tables);
    }

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
            "APP_PERSONA", "APP_USER_PERSONA", "APP_PERSONA_PREFERENCE", "APP_PERSONA_AUDIT",
            "APP_TENANT", "APP_COMPANY", "APP_COMPANY_MEMBERSHIP", "APP_MODULE_CONNECTION" })
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
