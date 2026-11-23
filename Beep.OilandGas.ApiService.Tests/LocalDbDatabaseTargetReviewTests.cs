using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Editor.SchemaMigration;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>Review guards use a recording native provider and real receipts. The marked LocalDB case uses the production factory.</summary>
public sealed class LocalDbDatabaseTargetReviewTests(ITestOutputHelper output)
{
    private static readonly LocalDbDatabaseTargetRequest Intent = new("ModuleTest", @"(localdb)\MSSQLLocalDB", "ModuleTest");
    private static PPDM39SetupService Setup(Func<LocalDbDatabaseTargetRequest, DatabaseTargetMigration>? factory) => new(
        Mock.Of<IDMEEditor>(), NullLogger<PPDM39SetupService>.Instance,
        Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),
        Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),
        Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),
        new Infrastructure.RecordingFailureReporter(), databaseTargetMigration: factory);

    [Fact]
    public async Task Review_is_read_only_and_creation_requires_exact_actor_target_and_one_use_token()
    {
        var provider = new Provider();
        var receipt = Path.Combine(Path.GetTempPath(), "OilGasTargetReview", Guid.NewGuid().ToString("N"), "owner.json");
        var setup = Setup(_ => MigrationManager.BindDatabaseTarget(provider, receipt));
        var review = setup.ReviewLocalDbDatabaseTarget(Intent, "admin");
        Assert.True(review.CanCreate); Assert.False(Directory.Exists(Path.GetDirectoryName(receipt)));
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent, review.ReviewToken), "other"));
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent with { Database = "Changed" }, review.ReviewToken), "admin"));
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent, "unknown"), "admin"));
        Assert.Equal(0, provider.Creates);
        var created = await setup.CreateLocalDbDatabaseTargetAsync(new(Intent, review.ReviewToken), "admin");
        Assert.True(created.OwnershipConfirmed); Assert.Equal(Intent, created.Target);
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent, review.ReviewToken), "admin"));
        Assert.Equal(1, provider.Creates);
        var recovered = Setup(_ => MigrationManager.BindDatabaseTarget(provider, receipt)).ReviewLocalDbDatabaseTarget(Intent, "admin");
        Assert.True(recovered.OwnershipConfirmed); Assert.False(recovered.CanCreate);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Uncertain_creation_is_not_retried_or_offered_as_a_fresh_database(bool existsAfterFailure)
    {
        var provider = new Provider { Fail = true, ExistsAfterFailure = existsAfterFailure };
        var receipt = Path.Combine(Path.GetTempPath(), "OilGasTargetReview", Guid.NewGuid().ToString("N"), "owner.json");
        var setup = Setup(_ => MigrationManager.BindDatabaseTarget(provider, receipt));
        var review = setup.ReviewLocalDbDatabaseTarget(Intent, "admin");
        await Assert.ThrowsAsync<IOException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent, review.ReviewToken), "admin"));
        var recovered = setup.ReviewLocalDbDatabaseTarget(Intent, "admin");
        Assert.True(recovered.RequiresReconciliation); Assert.False(recovered.CanCreate);
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent, recovered.ReviewToken), "admin"));
        Assert.Equal(1, provider.Creates);
    }

    [Fact]
    public async Task Existing_unowned_database_cannot_be_created_or_adopted()
    {
        var provider = new Provider { Exists = true };
        var receipt = Path.Combine(Path.GetTempPath(), "OilGasTargetReview", Guid.NewGuid().ToString("N"), "owner.json");
        var setup = Setup(_ => MigrationManager.BindDatabaseTarget(provider, receipt));
        var review = setup.ReviewLocalDbDatabaseTarget(Intent, "admin");
        Assert.True(review.DatabaseExists); Assert.False(review.CanCreate); Assert.False(review.OwnershipConfirmed);
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(Intent, review.ReviewToken), "admin"));
        Assert.Equal(0, provider.Creates); Assert.False(Directory.Exists(Path.GetDirectoryName(receipt)));
    }

    [Fact]
    public void Missing_capability_and_missing_actor_refuse_before_review()
    {
        Assert.Throws<RefusalException>(() => Setup(null).ReviewLocalDbDatabaseTarget(Intent, "admin"));
        Assert.Throws<RefusalException>(() => Setup(_ => throw new Exception("Must not bind")).ReviewLocalDbDatabaseTarget(Intent, ""));
    }

    [Theory]
    [InlineData("identityserver", "(localdb)\\MSSQLLocalDB")]
    [InlineData("BeepOilGasRepository", "(localdb)\\MSSQLLocalDB")]
    [InlineData("Diagnostics", "(localdb)\\MSSQLLocalDB")]
    [InlineData("NewModule", "localhost")]
    [InlineData("master", "(localdb)\\MSSQLLocalDB")]
    public void Protected_or_unsupported_targets_refuse_before_native_creation(string database, string server)
    {
        using var repo = Repository();
        Assert.Throws<RefusalException>(() => ModuleDatabaseTarget.BindLocalDbCreation(Intent with { Database = database, Server = server },
            Mock.Of<IDMEEditor>(), repo, Settings()));
    }

    [Fact]
    public void LocalDb_connection_mapping_uses_integrated_security_and_no_tcp_port()
    {
        var properties = new ConnectionConfig { DatabaseType = "SQL Server", ConnectionName = "ModuleTest", Host = Intent.Server, Database = Intent.Database, Port = 1433 }.ToConnectionProperties();
        var parsed = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(properties.ConnectionString);
        Assert.True(properties.IntegratedSecurity); Assert.Equal(0, properties.Port);
        Assert.True(parsed.IntegratedSecurity); Assert.Equal(Intent.Server, parsed.DataSource); Assert.Equal(Intent.Database, parsed.InitialCatalog);
    }

    [LocalDbModuleFact]
    public async Task Production_factory_reviews_creates_and_recovers_a_real_retained_LocalDb_database()
    {
        using var repo = Repository();
        var root = Path.Combine(Path.GetTempPath(), "OilGasWizardTarget", Guid.NewGuid().ToString("N"));
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.ExePath).Returns(root);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var target = Intent with { ConnectionName = "WizardModule", Database = "OilGasWizard_" + Guid.NewGuid().ToString("N") };
        output.WriteLine($"Retained database: {target.Database}; receipts: {root}");
        var setup = Setup(request => ModuleDatabaseTarget.BindLocalDbCreation(request, editor.Object, repo, Settings()));
        var review = setup.ReviewLocalDbDatabaseTarget(target, "admin");
        Assert.True(review.CanCreate); Assert.False(Directory.Exists(root));
        var created = await setup.CreateLocalDbDatabaseTargetAsync(new(target, review.ReviewToken), "admin");
        Assert.True(created.OwnershipConfirmed);
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(new ConnectionConfig
        { DatabaseType = "SQL Server", ConnectionName = target.ConnectionName, Host = target.Server, Database = target.Database }.ToConnectionProperties().ConnectionString);
        await connection.OpenAsync(); // Open only: no SQL. BeepDM owns catalog DDL.
        Assert.Equal(target.Database, connection.Database);
        var recovered = setup.ReviewLocalDbDatabaseTarget(target, "admin");
        Assert.True(recovered.OwnershipConfirmed); Assert.False(recovered.CanCreate);
        await Assert.ThrowsAsync<RefusalException>(() => setup.CreateLocalDbDatabaseTargetAsync(new(target, review.ReviewToken), "admin"));
    }

    private static SqlServerRepositoryDbContext Repository() => new(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
        .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=BeepOilGasRepository;Integrated Security=true").Options);
    private static IConfiguration Settings() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    { ["Diagnostics:Provider"] = "SqlServer", ["Diagnostics:ConnectionString"] = @"Server=(localdb)\MSSQLLocalDB;Database=Diagnostics;Integrated Security=true" }).Build();

    private sealed class Provider : IDatabaseTargetMigrationProvider
    {
        public DatabaseTargetMigrationIdentity Identity { get; } = new("Recording", "server", "database", "review-test");
        public bool Exists { get; set; }
        public bool Fail { get; init; }
        public bool ExistsAfterFailure { get; init; }
        public int Creates { get; private set; }
        private Guid _owner;
        public bool DatabaseExists() => Exists;
        public bool OwnerMatches(Guid owner) => Exists && _owner == owner;
        public Task CreateDatabaseAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); Creates++;
            if (Fail) { Exists = ExistsAfterFailure; throw new IOException("Unacknowledged creation"); }
            Exists = true; return Task.CompletedTask;
        }
        public Task StampOwnerAsync(Guid owner, CancellationToken token) { _owner = owner; return Task.CompletedTask; }
    }
}
