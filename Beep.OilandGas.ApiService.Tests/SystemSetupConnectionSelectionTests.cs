using Beep.OilandGas.Models.Data.DataManagement;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SystemSetupConnectionSelectionTests
{
    [Theory]
    [InlineData("system_migration", true)]
    [InlineData("system-migration", false)]
    [InlineData("2systems", false)]
    [InlineData("class", false)]
    [InlineData("", false)]
    public void NamesMustWorkAsBeepGeneratedNamespaces(string name, bool valid) => Assert.Equal(valid, BeepConnectionName.IsValid(name));

    [Fact]
    public async Task SelectsVerifiedRegisteredTargetAndReloadsAfterRestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-system-target-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "system-setup-target.json");
            IConfigurationRoot Configuration() => new ConfigurationBuilder().AddJsonFile(path, optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
            var configuration = Configuration();
            var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "system.db") }.ConnectionString };
            var native = new PersonaTaskRoutingTests.Fixture();
            var source = native.Source; source.SetupGet(x => x.DatasourceName).Returns("system"); source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
            Mock.Get(source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(properties);
            var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
            var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.Setup(x => x.GetDataSource("system")).Returns(source.Object);
            var service = new SystemSetupConnectionSelectionService(configuration, editor.Object);
            var guards = 0; var result = await service.SelectAsync("system", () => { guards++; return Task.CompletedTask; }, default);
            Assert.Equal("system", result.ConnectionName); Assert.Equal(5, guards);
            Assert.Equal(new[] { "begin", "commit" }, native.Transactions);
            Assert.Equal("system", configuration["SystemSetup:ConnectionName"]);
            Assert.Equal("system", Configuration()["SystemSetup:ConnectionName"]);
            Assert.Equal(ModuleDatabaseTarget.Key(editor.Object, properties), Configuration()["SystemSetup:ConnectionTarget"]);
            Assert.DoesNotContain("Data Source", await File.ReadAllTextAsync(path));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SelectAsync("different", () => Task.CompletedTask, default));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RefusesWeakOrBorrowedTransactionsBeforePersistingTarget(bool borrowed)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-system-target-" + Guid.NewGuid().ToString("N"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "system.db") }.ConnectionString };
        var native = new PersonaTaskRoutingTests.Fixture { Borrowed = borrowed, Isolation = borrowed ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted };
        native.Source.SetupGet(x => x.DatasourceName).Returns("system"); native.Source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        Mock.Get(native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(properties);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
        native.Editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new SystemSetupConnectionSelectionService(configuration, native.Editor.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SelectAsync("system", () => Task.CompletedTask, default));
        Assert.False(Directory.Exists(directory));
        Assert.Equal(borrowed ? Array.Empty<string>() : new[] { "begin", "rollback" }, native.Transactions);
    }

    [Fact]
    public async Task RevokedAuthorityRollsBackProbeWithoutSelectingTarget()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-system-target-" + Guid.NewGuid().ToString("N"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "system.db") }.ConnectionString };
        var native = new PersonaTaskRoutingTests.Fixture();
        native.Source.SetupGet(x => x.DatasourceName).Returns("system"); native.Source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        Mock.Get(native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(properties);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]); native.Editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var checks = 0;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new SystemSetupConnectionSelectionService(configuration, native.Editor.Object)
            .SelectAsync("system", () => ++checks == 2 ? Task.FromException(new UnauthorizedAccessException()) : Task.CompletedTask, default));
        Assert.Equal(new[] { "begin", "rollback" }, native.Transactions); Assert.False(Directory.Exists(directory));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RefusesRevocationOrTargetChangeAfterWritingTemporarySelection(bool revoke)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-system-target-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "system-setup-target.json");
        var configuration = new ConfigurationBuilder().AddJsonFile(path, optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "system.db") }.ConnectionString };
        var native = new PersonaTaskRoutingTests.Fixture();
        native.Source.SetupGet(x => x.DatasourceName).Returns("system"); native.Source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        Mock.Get(native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(properties);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]); native.Editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var checks = 0;
        try
        {
            await Assert.ThrowsAnyAsync<Exception>(() => new SystemSetupConnectionSelectionService(configuration, native.Editor.Object).SelectAsync("system", () =>
            {
                if (++checks == 4)
                {
                    if (revoke) throw new UnauthorizedAccessException();
                    properties.ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(directory, "other.db") }.ConnectionString;
                }
                return Task.CompletedTask;
            }, default));
            Assert.Equal(4, checks); Assert.False(File.Exists(path)); Assert.Empty(Directory.GetFiles(directory));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task RefusesUnverifiedOrAmbiguousConnectionsBeforeWriting()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-system-target-" + Guid.NewGuid().ToString("N"));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([new ConnectionProperties { ConnectionName = "duplicate" }, new ConnectionProperties { ConnectionName = "DUPLICATE" }]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new SystemSetupConnectionSelectionService(configuration, editor.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SelectAsync("duplicate", () => Task.CompletedTask, default));
        Assert.False(Directory.Exists(directory)); editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }
}
#endif
