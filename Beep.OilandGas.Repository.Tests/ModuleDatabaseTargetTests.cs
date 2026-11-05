using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class ModuleDatabaseTargetTests
{
    private static ConnectionProperties Connection(string name, string provider, string value) => new()
    { ConnectionName = name, DatabaseType = Enum.Parse<DataSourceType>(provider), ConnectionString = value };

    [Theory]
    [InlineData("SqlServer", "Server=tcp:server.example.,1433;Initial Catalog=production;User ID=one;Password=first", "Data Source=SERVER.EXAMPLE;Database=PRODUCTION;User ID=two;Password=second")]
    [InlineData("Postgre", "Host=server.example;Database=production;Username=one;Password=first", "Server=SERVER.EXAMPLE.;Port=5432;Database=production;Username=two;Password=second;Search Path=other")]
    [InlineData("Oracle", "Data Source=//server.example:1521/production;User Id=one;Password=first", "Data Source=SERVER.EXAMPLE./PRODUCTION;User Id=two;Password=second")]
    [InlineData("SqlLite", "Data Source=./database.db", "Data Source=database.db")]
    [InlineData("SqlServer", "Server=localhost;Database=production", "Server=tcp:127.0.0.1,1433;Database=PRODUCTION")]
    [InlineData("SqlServer", "Server=.;Database=production", "Server=(local);Database=PRODUCTION")]
    public void Credential_schema_and_endpoint_spelling_aliases_share_one_target(string provider, string first, string second)
    {
        using var fixture = new Fixture();
        var a = Connection("first", provider, first); var b = Connection("second", provider, second);
        Assert.Equal(ModuleDatabaseTarget.Key(fixture.Editor.Object, a), ModuleDatabaseTarget.Key(fixture.Editor.Object, b));
    }

    [Fact]
    public void Target_identity_ignores_connection_ids_driver_versions_credentials_and_schema_but_migration_plans_do_not()
    {
        using var fixture = new Fixture(); var connection = fixture.Connections[0];
        var target = ModuleDatabaseTarget.Key(fixture.Editor.Object, connection);
        var plan = Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Fingerprint(connection);
        connection.GuidID = "other-guid"; connection.SchemaName = "other-schema"; connection.DriverName = "other-driver";
        connection.DriverVersion = "other-version"; connection.ConnectionName = "other-name";
        connection.ConnectionString = "Server=server;Database=production;User ID=rotated;Password=literal{secret}";
        Assert.Equal(target, ModuleDatabaseTarget.Key(fixture.Editor.Object, connection));
        Assert.NotEqual(plan, Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Fingerprint(connection));
        Assert.Equal(64, target.Length); Assert.DoesNotContain("production", target, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Beep_template_processing_uses_a_copy_and_matches_concrete_settings()
    {
        using var fixture = new Fixture();
        var template = Connection("template", "SqlServer", "Server={Host},{Port};Database={DataBase};User ID={UserID};Password={Password}");
        template.Host = "server"; template.Port = 1433; template.Database = "production"; template.UserID = "account"; template.Password = "secret";
        var before = template.ConnectionString;
        Assert.Equal(ModuleDatabaseTarget.Key(fixture.Editor.Object, template), ModuleDatabaseTarget.Key(fixture.Editor.Object, fixture.Connections[0]));
        Assert.Equal(before, template.ConnectionString);
    }

    [Fact]
    public void Relative_file_properties_match_the_absolute_file_and_never_change_configuration()
    {
        using var fixture = new Fixture();
        var relative = Connection("relative", "SqlLite", ""); relative.FilePath = "./files"; relative.FileName = "database.db";
        var absolute = Connection("absolute", "SqlLite", new SqliteConnectionStringBuilder
        { DataSource = Path.Combine(Environment.CurrentDirectory, "files", "database.db") }.ConnectionString);
        Assert.Equal(ModuleDatabaseTarget.Key(fixture.Editor.Object, relative), ModuleDatabaseTarget.Key(fixture.Editor.Object, absolute));
        Assert.Equal("./files", relative.FilePath);
    }

    [Fact]
    public void Sqlite_target_rechecks_parent_junction_after_retarget_and_records_regular_path_cost()
    {
        var root = Path.Combine(Path.GetTempPath(), "oilgas-353-target-" + Guid.NewGuid().ToString("N"));
        var first = Path.Combine(root, "first"); var second = Path.Combine(root, "second");
        var alias = Path.Combine(root, "current");
        Directory.CreateDirectory(first); Directory.CreateDirectory(second);
        File.WriteAllBytes(Path.Combine(first, "database.db"), []);
        File.WriteAllBytes(Path.Combine(second, "database.db"), []);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.ExePath).Returns(root);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        string Key(string directory) => ModuleDatabaseTarget.Key(editor.Object, Connection("closure", "SqlLite",
            new SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "database.db") }.ConnectionString));
        void Link(string target)
        {
            if (!OperatingSystem.IsWindows()) { Directory.CreateSymbolicLink(alias, target); return; }
            var start = new System.Diagnostics.ProcessStartInfo("powershell.exe")
                { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("-NoProfile"); start.ArgumentList.Add("-NonInteractive"); start.ArgumentList.Add("-Command");
            start.ArgumentList.Add("New-Item -ItemType Junction -Path '" + alias.Replace("'", "''") +
                "' -Target '" + target.Replace("'", "''") + "' -ErrorAction Stop | Out-Null");
            using var process = System.Diagnostics.Process.Start(start)!;
            var error = process.StandardError.ReadToEnd(); process.WaitForExit();
            Assert.True(process.ExitCode == 0, error);
        }
        var expectedFirst = Key(first); var expectedSecond = Key(second);
        var recordedOrdinaryIdentity = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { Provider = "Sqlite", File = OperatingSystem.IsWindows() ? Path.Combine(first, "database.db").ToUpperInvariant() : Path.Combine(first, "database.db") })));
        Assert.Equal(recordedOrdinaryIdentity, expectedFirst);
        Assert.NotEqual(expectedFirst, expectedSecond);
        if (OperatingSystem.IsWindows()) Assert.Equal(expectedFirst, Key(@"\\?\" + first));
        var future = Path.Combine(root, "not-created", "nested"); var beforeCreation = Key(future);
        Directory.CreateDirectory(future); File.WriteAllBytes(Path.Combine(future, "database.db"), []);
        Assert.Equal(beforeCreation, Key(future));
        Link(first); Assert.Equal(expectedFirst, Key(alias));
        Assert.True(File.GetAttributes(alias).HasFlag(FileAttributes.ReparsePoint));
        // Remove only this generated link; both physical target directories must survive.
        Directory.Delete(alias, recursive: false);
        Assert.True(Directory.Exists(first)); Assert.True(Directory.Exists(second));
        Link(second); Assert.Equal(expectedSecond, Key(alias)); Assert.NotEqual(expectedFirst, Key(alias));
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (var index = 0; index < 128; index++) Assert.Equal(expectedFirst, Key(first));
        watch.Stop();
        File.WriteAllText(Path.Combine(root, "observed.json"), System.Text.Json.JsonSerializer.Serialize(new
            { Iterations = 128, RegularPathMilliseconds = watch.Elapsed.TotalMilliseconds, ParentRetargetRechecked = true }));
        // Retain the isolated evidence directory. No recursive cleanup of junctions or physical targets.
    }

    [Fact]
    public void Unknown_provider_or_ambiguous_configured_name_cannot_claim_a_target()
    {
        using var fixture = new Fixture();
        Assert.Throws<RefusalException>(() => ModuleDatabaseTarget.Key(fixture.Editor.Object, new ConnectionProperties { ConnectionName = "unsupported" }));
        fixture.Connections.Add(Connection("DATABASE", "SqlServer", "Server=another;Database=another"));
        Assert.Throws<RefusalException>(() => ModuleDatabaseTarget.Key(fixture.Editor.Object, "database"));
    }

    [Theory]
    [InlineData("SqlServer", "Server=other;Database=production")]
    [InlineData("SqlServer", "Server=server,1434;Database=production")]
    [InlineData("SqlServer", "Server=server;Database=other")]
    [InlineData("Postgre", "Host=server;Database=production")]
    public void Another_endpoint_database_or_provider_has_another_target(string provider, string value)
    {
        using var fixture = new Fixture(); Assert.NotEqual(ModuleDatabaseTarget.Key(fixture.Editor.Object, fixture.Connections[0]),
            ModuleDatabaseTarget.Key(fixture.Editor.Object, Connection("other", provider, value)));
    }

    [Theory]
    [InlineData("SqlServer", "Server=server;User Id=account;Password=secret")]
    [InlineData("SqlServer", "Server={Unresolved};Database=production")]
    [InlineData("Oracle", "Data Source=tns-alias;User Id=account;Password=secret")]
    [InlineData("Oracle", "Data Source=(DESCRIPTION=(ADDRESS=(HOST=server)));User Id=account;Password=secret")]
    [InlineData("Postgre", "Host=one,two;Database=production")]
    [InlineData("SqlLite", "Data Source=:memory:")]
    [InlineData("SqlLite", "Data Source=memory-name;Mode=Memory")]
    [InlineData("SqlServer", "malformed-secret-password-text")]
    public void Ambiguous_or_implicit_targets_are_refused_without_returning_credentials(string provider, string value)
    {
        using var fixture = new Fixture(); var refusal = Assert.Throws<RefusalException>(() => ModuleDatabaseTarget.Key(fixture.Editor.Object, Connection("other", provider, value)));
        Assert.DoesNotContain("secret", refusal.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("account", refusal.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B")]
    public async Task One_database_has_one_canonical_connection_even_under_another_name_or_schema(string tenant)
    {
        using var fixture = new Fixture();
        await fixture.Bindings.BindAsync("A", "PRODUCTION", new("database", null), "admin");
        fixture.Connections.Add(Connection("alias", "SqlServer", "Data Source=SERVER;Initial Catalog=PRODUCTION;User ID=other;Password=other"));
        fixture.Connections[1].SchemaName = "different-schema";
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Bindings.BindAsync(tenant, "ACCOUNTING", new("alias", null), "admin"));
        Assert.Single(await fixture.Db.Set<AppModuleConnection>().ToArrayAsync());
        Assert.Null(await fixture.Resolver(tenant).FindAsync("ACCOUNTING"));
        await fixture.Bindings.BindAsync("A", "ACCOUNTING", new("DATABASE", null), "admin");
        Assert.Equal("database", await fixture.Resolver("A").ResolveAsync("ACCOUNTING"));
    }

    [Fact]
    public async Task Configuration_redirection_is_refused_by_routing_planning_and_rebinding()
    {
        using var fixture = new Fixture(); var binding = await fixture.Bindings.BindAsync("A", "PRODUCTION", new("database", null), "admin");
        fixture.Connections[0].ConnectionString = "Server=another-tenant;Database=other";
        Assert.Null(await fixture.Resolver("A").FindAsync("PRODUCTION"));
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Resolver("A").GetMigrationBindingFingerprintAsync(["PRODUCTION"], "database"));
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Bindings.BindAsync("A", "PRODUCTION", new("database", binding.ConcurrencyStamp), "admin"));
        fixture.Db.ChangeTracker.Clear(); var owner = await fixture.Db.Set<AppModuleConnection>().SingleAsync();
        fixture.Connections[0].ConnectionString = "Server=server;Database=production;User Id=rotated;Password=rotated";
        Assert.Equal(owner.PhysicalTargetKey, ModuleDatabaseTarget.Key(fixture.Editor.Object, fixture.Connections[0]));
        Assert.Equal("database", await fixture.Resolver("A").ResolveAsync("PRODUCTION"));
    }

    [Fact]
    public async Task Closure_binding_queries_observe_current_owner_stamp_and_tenant_state()
    {
        using var fixture = new Fixture();
        await fixture.Bindings.BindAsync("A", "PPDM_CORE", new("database", null), "admin");
        await fixture.Bindings.BindAsync("A", "LIFECYCLE", new("database", null), "admin");
        var resolver = fixture.Resolver("A");
        var first = await resolver.GetMigrationBindingFingerprintAsync(["ppdm_core", "LIFECYCLE"], "database");
        Assert.Equal("database", await resolver.ResolveAsync("PPDM_CORE"));
        var scope = await resolver.ResolveAssetScopeAsync();
        Assert.Equal("database", scope.ConnectionName);
        Assert.Equal(await resolver.GetMigrationBindingFingerprintAsync(["PPDM_CORE"], "database"), scope.Fingerprint);
        var owner = await fixture.Db.Set<AppModuleConnection>().SingleAsync();
        var target = owner.PhysicalTargetKey; owner.PhysicalTargetKey = null; await fixture.Db.SaveChangesAsync();
        Assert.Null(await resolver.FindAsync("PPDM_CORE"));
        await Assert.ThrowsAsync<RefusalException>(() => resolver.ResolveAssetScopeAsync());
        await Assert.ThrowsAsync<RefusalException>(() => resolver.GetMigrationBindingFingerprintAsync(["PPDM_CORE", "LIFECYCLE"], "database"));
        owner.PhysicalTargetKey = target;
        var binding = await fixture.Db.ModuleDatabases.SingleAsync(x => x.ModuleId == "LIFECYCLE");
        binding.ConcurrencyStamp = Guid.NewGuid().ToString(); await fixture.Db.SaveChangesAsync();
        var current = await resolver.GetMigrationBindingFingerprintAsync(["PPDM_CORE", "LIFECYCLE"], "database");
        Assert.NotEqual(first, current);
        Assert.Equal(scope, await resolver.ResolveAssetScopeAsync());
        (await fixture.Db.ModuleDatabases.SingleAsync(x => x.ModuleId == "PPDM_CORE")).ConcurrencyStamp = Guid.NewGuid().ToString();
        await fixture.Db.SaveChangesAsync();
        Assert.NotEqual(scope.Fingerprint, (await resolver.ResolveAssetScopeAsync()).Fingerprint);
        Assert.Equal("database", await resolver.ResolveAsync("LIFECYCLE"));
        Assert.Null(await fixture.Resolver("B").FindAsync("LIFECYCLE"));
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Resolver("B").ResolveAssetScopeAsync());
        await Assert.ThrowsAsync<RefusalException>(() => fixture.Resolver("B").GetMigrationBindingFingerprintAsync(["PPDM_CORE", "LIFECYCLE"], "database"));
        (await fixture.Db.Set<AppTenant>().SingleAsync(x => x.TenantId == "A")).IsActive = false;
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => resolver.ResolveAsync("LIFECYCLE"));
        await Assert.ThrowsAsync<RefusalException>(() => resolver.ResolveAssetScopeAsync());
        await Assert.ThrowsAsync<RefusalException>(() => resolver.GetMigrationBindingFingerprintAsync(["PPDM_CORE", "LIFECYCLE"], "database"));
    }

    [Fact]
    public async Task Historical_name_only_ownership_requires_explicit_revalidation_and_records_actor()
    {
        using var fixture = new Fixture();
        fixture.Db.Add(new AppModuleConnection { ConnectionKey = "DATABASE", TenantId = "A", AssignedByUserId = "historical" });
        fixture.Db.Add(new ModuleDatabaseBinding { TenantId = "A", ModuleId = "PRODUCTION", ConnectionName = "database", ConnectionKey = "DATABASE" });
        await fixture.Db.SaveChangesAsync(); Assert.Null(await fixture.Resolver("A").FindAsync("PRODUCTION"));
        var binding = await fixture.Db.ModuleDatabases.SingleAsync();
        await fixture.Bindings.BindAsync("A", "PRODUCTION", new("database", binding.ConcurrencyStamp), "reviewer");
        var owner = await fixture.Db.Set<AppModuleConnection>().SingleAsync();
        Assert.Equal("reviewer", owner.TargetVerifiedByUserId); Assert.NotNull(owner.TargetVerifiedUtc); Assert.Equal(64, owner.PhysicalTargetKey!.Length);
        Assert.Equal("historical", owner.AssignedByUserId); Assert.Equal("database", await fixture.Resolver("A").ResolveAsync("PRODUCTION"));
    }

    [Fact]
    public async Task Unique_database_index_arbitrates_alias_claims_even_when_service_checks_are_bypassed()
    {
        using var fixture = new Fixture(); await fixture.Bindings.BindAsync("A", "PRODUCTION", new("database", null), "admin");
        var target = (await fixture.Db.Set<AppModuleConnection>().SingleAsync()).PhysicalTargetKey;
        fixture.Db.Add(new AppModuleConnection { ConnectionKey = "ALIAS", TenantId = "B", PhysicalTargetKey = target, AssignedByUserId = "admin" });
        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Db.SaveChangesAsync());
    }

    [Theory]
    [InlineData("memory")]
    [InlineData("composite")]
    [InlineData("parameters")]
    [InlineData("parameter-list")]
    public void Hidden_runtime_targets_cannot_be_assigned_a_database_identity(string kind)
    {
        using var fixture = new Fixture(); var connection = fixture.Connections[0];
        if (kind == "memory") connection.IsInMemory = true;
        if (kind == "composite") connection.IsComposite = true;
        if (kind == "parameters") connection.Parameters = "Database=other";
        if (kind == "parameter-list") connection.ParameterList = new() { ["Database"] = "other" };
        Assert.Throws<RefusalException>(() => ModuleDatabaseTarget.Key(fixture.Editor.Object, connection));
    }

    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection database = new("Data Source=:memory:");
        public Context Db { get; }
        public Mock<IDMEEditor> Editor { get; } = new();
        public List<ConnectionProperties> Connections { get; } = [Connection("database", "SqlServer", "Server=server;Database=production;User ID=one;Password=first")];
        public ModuleBindingService Bindings { get; }
        public Fixture()
        {
            database.Open(); Db = new(new DbContextOptionsBuilder<Context>().UseSqlite(database).Options);
            Db.Database.EnsureCreated(); Db.AddRange(new AppTenant { TenantId = "A" }, new AppTenant { TenantId = "B" }); Db.SaveChanges();
            var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns(Connections);
            config.SetupGet(x => x.ExePath).Returns(Environment.CurrentDirectory); Editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
            Bindings = new(Db, Editor.Object);
        }
        public ModuleConnectionResolver Resolver(string id)
        { var tenant = new RepositoryTenantContext(); tenant.SelectTenant(id); return new(Db, Editor.Object, tenant); }
        public void Dispose() { Db.Dispose(); database.Dispose(); }
    }
}
