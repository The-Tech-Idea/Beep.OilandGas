#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class SetupDriverServiceTests
{
    private sealed class UnsupportedConnection : System.Data.Common.DbConnection
    {
        public UnsupportedConnection() => throw new PlatformNotSupportedException();
        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => string.Empty;
        public override string DataSource => string.Empty;
        public override string ServerVersion => string.Empty;
        public override System.Data.ConnectionState State => System.Data.ConnectionState.Closed;
        public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();
        public override void Close() { }
        public override void Open() => throw new NotSupportedException();
        protected override System.Data.Common.DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) => throw new NotSupportedException();
        protected override System.Data.Common.DbCommand CreateDbCommand() => throw new NotSupportedException();
    }

    private sealed class Fixture
    {
        public Mock<IDMEEditor> Editor = new(); public Mock<IConfigEditor> Config = new(); public Mock<IAssemblyHandler> Loader = new();
        public ConnectionDriversConfig Driver = new() { GuidID = "driver-id", PackageName = "Provider", classHandler = "DriverImpl", dllname = "provider.dll", NuggetVersion = "1.2.3", ADOType = true, DbConnectionType = "Provider.Connection", IsMissing = true, NuggetMissing = true };
        public bool Provider; public bool Connector; public int Downloads; public bool ScanFails;
        public SetupDriverService Service;
        public Fixture(bool configured = true)
        {
            Config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { Driver });
            Config.SetupGet(x => x.DataSourcesClasses).Returns(() => Connector ? new List<AssemblyClassDefinition> { new() { className = "DriverImpl", type = Mock.Of<IDataSource>().GetType() } } : []);
            Editor.SetupGet(x => x.ConfigEditor).Returns(Config.Object); Editor.SetupGet(x => x.assemblyHandler).Returns(Loader.Object);
            Loader.Setup(x => x.GetType("Provider.Connection")).Returns(() => Provider ? typeof(Microsoft.Data.Sqlite.SqliteConnection) : null!);
            Loader.Setup(x => x.HasLocalPackage(It.IsAny<ConnectionDriversConfig>())).Returns(false);
            Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "4.0", null, true, null, false)).ReturnsAsync(() => { Downloads++; if (!ScanFails) Connector = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
            Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Provider", "1.2.3", null, true, null, false)).ReturnsAsync(() => { Downloads++; Provider = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
            var values = configured ? new Dictionary<string, string?> { ["SetupDrivers:Connectors:DriverImpl:Package"] = "Connector", ["SetupDrivers:Connectors:DriverImpl:Version"] = "4.0" } : new();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            Service = new(Editor.Object, configuration, NullLogger<SetupDriverService>.Instance);
        }
    }
#if BEEP_SECURE_CONNECTIONS
    [Fact]
    public void SqlServerCatalogDistinguishesTransactionContractFromUnknownAndMissing()
    {
        var f = new Fixture();
        f.Driver.DatasourceType = DataSourceType.SqlServer;
        Assert.Null(Assert.Single(f.Service.Catalog()).SqlServerOwnedTransactionContract);
        f.Connector = true;
        Assert.False(Assert.Single(f.Service.Catalog()).SqlServerOwnedTransactionContract);

        var capable = new Mock<IDataSource>();
        capable.As<TheTechIdea.Beep.DataBase.IIsolationOwnedTransactionDataSource>();
        f.Config.SetupGet(x => x.DataSourcesClasses).Returns(new List<AssemblyClassDefinition>
        { new() { className = "DriverImpl", type = capable.Object.GetType() } });
        Assert.True(Assert.Single(f.Service.Catalog()).SqlServerOwnedTransactionContract);
        f.Driver.DatasourceType = DataSourceType.SqlLite;
        Assert.Null(Assert.Single(f.Service.Catalog()).SqlServerOwnedTransactionContract);
    }
#endif
    [Fact]
    public void ProviderThatCannotBeConstructedIsNotReportedAsLoaded()
    {
        var f = new Fixture { Connector = true };
        f.Loader.Setup(x => x.GetType("Provider.Connection")).Returns(typeof(UnsupportedConnection));
        var item = Assert.Single(f.Service.Catalog());
        Assert.False(item.ProviderLoaded);
        Assert.False(item.Loaded);
    }

    [Fact]
    public void AbstractProviderIsNotReportedAsLoaded()
    {
        var f = new Fixture { Connector = true };
        f.Loader.Setup(x => x.GetType("Provider.Connection")).Returns(typeof(System.Data.Common.DbConnection));
        var item = Assert.Single(f.Service.Catalog());
        Assert.False(item.ProviderLoaded);
        Assert.False(item.Loaded);
    }

    [Fact]
    public async Task InstallsAndVerifiesBothConnectorAndProvider()
    {
        var f = new Fixture(); var before = Assert.Single(f.Service.Catalog()); Assert.False(before.Loaded);
        Assert.Equal("Connector", before.ConnectorPackage); Assert.Equal("4.0", before.ConnectorVersion);
        var result = await f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default);
        Assert.True(result.Loaded); Assert.True(result.ProviderLoaded); Assert.True(result.ConnectorLoaded); Assert.Equal(2, f.Downloads);
        f.Loader.Verify(x => x.LoadAllAssembly(It.IsAny<IProgress<PassedArgs>>(), It.IsAny<CancellationToken>()), Times.Never);
        f.Config.Verify(x => x.SaveConnectionDriversConfigValues(), Times.Once); f.Config.Verify(x => x.SaveConfigValues(), Times.Once);
        await f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default); Assert.Equal(2, f.Downloads);
    }
    [Fact]
    public async Task StaleReviewedPackageIsRejectedBeforeAnyDownload()
    {
        var f = new Fixture();
        var reviewed = Assert.Single(f.Service.Catalog());
        f.Driver.NuggetVersion = "2.0.0";
        await Assert.ThrowsAnyAsync<Exception>(() => f.Service.InstallAsync("driver-id", reviewed, () => Task.CompletedTask, default));
        Assert.Equal(0, f.Downloads);
        f.Config.Verify(x => x.SaveConnectionDriversConfigValues(), Times.Never);
    }

    [Fact]
    public async Task ChangedPackageConfigurationDuringDownloadIsNotSavedOrConfirmed()
    {
        var f = new Fixture();
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:DriverImpl:Package"] = "Connector", ["SetupDrivers:Connectors:DriverImpl:Version"] = "4.0" }).Build();
        f.Service = new(f.Editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "4.0", null, true, null, false)).ReturnsAsync(() =>
        {
            f.Connector = true;
            settings["SetupDrivers:Connectors:DriverImpl:Version"] = "5.0";
            return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly };
        });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default));
        f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync("Provider", It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        f.Config.Verify(x => x.SaveConnectionDriversConfigValues(), Times.Never);
        Assert.True(f.Driver.IsMissing); Assert.True(f.Driver.NuggetMissing);
    }
    [Fact]
    public async Task ReplacedDriverDuringDownloadIsNotSavedOrConfirmed()
    {
        var f = new Fixture();
        var registered = new List<ConnectionDriversConfig> { f.Driver };
        f.Config.SetupGet(x => x.DataDriversClasses).Returns(registered);
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "4.0", null, true, null, false)).ReturnsAsync(() =>
        {
            f.Connector = true;
            registered[0] = new ConnectionDriversConfig { GuidID = "driver-id", PackageName = "Provider", classHandler = "DriverImpl" };
            return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly };
        });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default));
        f.Config.Verify(x => x.SaveConnectionDriversConfigValues(), Times.Never);
        Assert.True(f.Driver.IsMissing); Assert.True(f.Driver.NuggetMissing);
    }
    [Fact]
    public async Task ServerConfiguredPackageSourcesAreUsedForBothDownloads()
    {
        var f = new Fixture();
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:DriverImpl:Package"] = "Connector", ["SetupDrivers:Connectors:DriverImpl:Version"] = "4.0",
          ["SetupDrivers:PackageSources:0"] = "https://packages.example.test/v3/index.json" }).Build();
        f.Service = new(f.Editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "4.0", It.Is<IEnumerable<string>>(s => s.SequenceEqual(new[] { "https://packages.example.test/v3/index.json" })), true, null, false))
            .ReturnsAsync(() => { f.Connector = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Provider", "1.2.3", It.Is<IEnumerable<string>>(s => s.SequenceEqual(new[] { "https://packages.example.test/v3/index.json" })), true, null, false))
            .ReturnsAsync(() => { f.Provider = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
        Assert.True((await f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default)).Loaded);
    }
    [Fact]
    public async Task ProviderAloneCannotBeMisreportedAsComplete()
    {
        var f = new Fixture(false) { Provider = true };
        var item = Assert.Single(f.Service.Catalog()); Assert.True(item.ProviderLoaded); Assert.False(item.ConnectorLoaded); Assert.False(item.Loaded); Assert.False(item.CanInstall);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default)); Assert.Equal(0, f.Downloads);
    }
    [Fact]
    public async Task ConfiguredProviderPackageReplacesCatalogPackageForInstallAndDisplay()
    {
        var f = new Fixture() { Connector = true };
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Providers:DriverImpl:Package"] = "ActualProvider", ["SetupDrivers:Providers:DriverImpl:Version"] = "9.1" }).Build();
        f.Service = new(f.Editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("ActualProvider", "9.1", null, true, null, false))
            .ReturnsAsync(() => { f.Provider = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
        var catalog = Assert.Single(f.Service.Catalog());
        Assert.Equal("ActualProvider", catalog.Package); Assert.Equal("9.1", catalog.PackageVersion);
        var installed = await f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default);
        Assert.True(installed.Loaded);
        f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync("ActualProvider", "9.1", null, true, null, false), Times.Once);
        f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync("Provider", It.IsAny<string>(), null, true, null, false), Times.Never);
    }
    [Fact]
    public async Task StartupRestoreRejectsInvalidPackagePathsAndNeverDownloads()
    {
        var f = new Fixture();
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:DriverImpl:Package"] = "../outside", ["SetupDrivers:Connectors:DriverImpl:Version"] = "1.0",
          ["SetupDrivers:Providers:DriverImpl:Package"] = "provider", ["SetupDrivers:Providers:DriverImpl:Version"] = "../outside" }).Build();
        f.Service = new(f.Editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        await f.Service.RestoreCachedAsync(default);
        f.Loader.Verify(x => x.LoadNugget(It.IsAny<string>()), Times.Never);
        f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
    [Fact]
    public void StartupRestoreUsesNugetNormalizedVersionDirectory()
    {
        Assert.Equal(Path.Combine("cache", "connector", "4.0.0"), SetupDriverService.CachePath("cache", "Connector", "4.0"));
        Assert.Null(SetupDriverService.CachePath("cache", "Connector", "latest"));
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FailedCachedPackageDoesNotPreventOtherPackagesFromRestoring(bool connectorFails)
    {
        var root = Path.Combine(Path.GetTempPath(), "oilgas-driver-restore-" + Guid.NewGuid().ToString("N"));
        var f = new Fixture();
        var second = new ConnectionDriversConfig { GuidID = "second", PackageName = "OtherProvider", classHandler = "OtherImpl", dllname = "other.dll", NuggetVersion = "2.0.0", ADOType = true, DbConnectionType = "Other.Connection" };
        var classes = new List<AssemblyClassDefinition>();
        f.Config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { f.Driver, second });
        f.Config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        var provider = false; var otherProvider = false;
        f.Loader.Setup(x => x.GetType("Provider.Connection")).Returns(() => provider ? typeof(Microsoft.Data.Sqlite.SqliteConnection) : null!);
        f.Loader.Setup(x => x.GetType("Other.Connection")).Returns(() => otherProvider ? typeof(Microsoft.Data.Sqlite.SqliteConnection) : null!);
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:DriverImpl:Package"] = "Connector", ["SetupDrivers:Connectors:DriverImpl:Version"] = "4.0",
          ["SetupDrivers:Connectors:OtherImpl:Package"] = "OtherConnector", ["SetupDrivers:Connectors:OtherImpl:Version"] = "3.0" }).Build();
        f.Service = new(f.Editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        var connector = SetupDriverService.CachePath(root, "Connector", "4.0")!;
        var firstProvider = SetupDriverService.CachePath(root, "Provider", "1.2.3")!;
        var otherConnector = SetupDriverService.CachePath(root, "OtherConnector", "3.0")!;
        var otherProviderPath = SetupDriverService.CachePath(root, "OtherProvider", "2.0.0")!;
        foreach (var path in new[] { connector, firstProvider, otherConnector, otherProviderPath }) Directory.CreateDirectory(path);
        if (connectorFails) f.Loader.Setup(x => x.LoadNugget(connector)).Throws(new InvalidOperationException("Corrupt connector"));
        else f.Loader.Setup(x => x.LoadNugget(connector)).Returns(() => { classes.Add(new() { className = "DriverImpl", type = Mock.Of<IDataSource>().GetType() }); return true; });
        if (connectorFails) f.Loader.Setup(x => x.LoadNugget(firstProvider)).Returns(() => { provider = true; return true; });
        else f.Loader.Setup(x => x.LoadNugget(firstProvider)).Throws(new InvalidOperationException("Corrupt provider"));
        f.Loader.Setup(x => x.LoadNugget(otherConnector)).Returns(() => { classes.Add(new() { className = "OtherImpl", type = Mock.Of<IDataSource>().GetType() }); return true; });
        f.Loader.Setup(x => x.LoadNugget(otherProviderPath)).Returns(() => { otherProvider = true; return true; });
        try
        {
            await f.Service.RestoreCachedAsync(root, default);
            var catalog = f.Service.Catalog();
            var first = Assert.Single(catalog, x => x.Id == f.Driver.GuidID);
            Assert.Equal(!connectorFails, first.ConnectorLoaded); Assert.Equal(connectorFails, first.ProviderLoaded); Assert.False(first.Loaded);
            Assert.True(Assert.Single(catalog, x => x.Id == second.GuidID).Loaded);
            f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task CancelledCachedRestoreStopsBeforeLoadingTheProvider()
    {
        var root = Path.Combine(Path.GetTempPath(), "oilgas-driver-cancel-" + Guid.NewGuid().ToString("N"));
        var f = new Fixture();
        var connector = SetupDriverService.CachePath(root, "Connector", "4.0")!;
        Directory.CreateDirectory(connector);
        f.Loader.Setup(x => x.LoadNugget(connector)).Throws(new OperationCanceledException());
        try
        {
            await Assert.ThrowsAsync<OperationCanceledException>(() => f.Service.RestoreCachedAsync(root, default));
            f.Loader.Verify(x => x.LoadNugget(It.IsAny<string>()), Times.Once);
        }
        finally { Directory.Delete(root, recursive: true); }
    }
    [Theory]
    [InlineData(null, "1.2.3")]
    [InlineData("../outside", "1.2.3")]
    [InlineData("4.0", null)]
    [InlineData("4.0", "latest")]
    [InlineData("4.0", "[1.0,2.0)")]
    public async Task UnrestorablePackageVersionsAreRejectedBeforeAnyDownload(string? connectorVersion, string? providerVersion)
    {
        var f = new Fixture();
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:DriverImpl:Package"] = "Connector", ["SetupDrivers:Connectors:DriverImpl:Version"] = connectorVersion,
          ["SetupDrivers:Providers:DriverImpl:Package"] = "Provider", ["SetupDrivers:Providers:DriverImpl:Version"] = providerVersion }).Build();
        f.Service = new(f.Editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        Assert.False(Assert.Single(f.Service.Catalog()).CanInstall);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default));
        Assert.Equal(0, f.Downloads);
        f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        f.Config.Verify(x => x.SaveConfigValues(), Times.Never);
    }
    [Fact]
    public async Task UnauthorizedOrUnknownDriverNeverStartsDownload()
    {
        var f = new Fixture();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => throw new UnauthorizedAccessException(), default));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Service.InstallAsync("not-registered", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default));
        Assert.Equal(0, f.Downloads); f.Config.Verify(x => x.SaveConfigValues(), Times.Never);
    }
    [Fact]
    public async Task LostAuthorityDuringProviderLoadingDoesNotPersistDriverState()
    {
        var f = new Fixture(); var revoked = false;
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Provider", "1.2.3", null, true, null, false))
            .ReturnsAsync(() => { f.Provider = true; revoked = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () =>
            revoked ? throw new UnauthorizedAccessException() : Task.CompletedTask, default));
        Assert.True(f.Driver.IsMissing); Assert.True(f.Driver.NuggetMissing);
        f.Config.Verify(x => x.SaveConnectionDriversConfigValues(), Times.Never);
        f.Config.Verify(x => x.SaveConfigValues(), Times.Never);
    }
    [Fact]
    public async Task CancelledConnectorDownloadDoesNotStartProviderDownloadOrPersistDriverState()
    {
        var f = new Fixture();
        using var cancellation = new CancellationTokenSource();
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "4.0", null, true, null, false))
            .ReturnsAsync(() => { f.Connector = true; cancellation.Cancel(); return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, cancellation.Token));

        f.Loader.Verify(x => x.LoadNuggetFromNuGetAsync("Provider", It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        f.Config.Verify(x => x.SaveConnectionDriversConfigValues(), Times.Never);
        f.Config.Verify(x => x.SaveConfigValues(), Times.Never);
        Assert.True(f.Driver.IsMissing);
    }
    [Fact]
    public async Task FailedConnectorScanIsNotReportedOrPersistedAsLoaded()
    {
        var f = new Fixture { ScanFails = true };
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default));
        Assert.True(f.Driver.IsMissing); Assert.False(f.Provider); f.Config.Verify(x => x.SaveConfigValues(), Times.Never);
    }
    [Fact]
    public async Task UnavailableConnectorExplainsPackageFeedFailureAndDoesNotPersist()
    {
        var f = new Fixture();
        f.Loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "4.0", null, true, null, false))
            .ReturnsAsync(new List<System.Reflection.Assembly>());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.InstallAsync("driver-id", Assert.Single(f.Service.Catalog()), () => Task.CompletedTask, default));
        Assert.Contains("connector package", error.Message);
        Assert.Contains("package feeds", error.Message);
        Assert.False(f.Provider);
        Assert.True(f.Driver.IsMissing);
        f.Config.Verify(x => x.SaveConfigValues(), Times.Never);
    }
}
#endif
