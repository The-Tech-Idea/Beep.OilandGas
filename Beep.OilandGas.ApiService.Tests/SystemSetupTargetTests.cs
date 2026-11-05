#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Beep.OilandGas.Repository;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SystemSetupTargetTests
{
    [Fact]
    public void SystemTargetIsIndependentOfTenantAndPinsTheConfiguredBeepTarget()
    {
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
        var connection = new Mock<IDataConnection>(); connection.SetupGet(x => x.ConnectionProp).Returns(properties);
        var source = new Mock<IDataSource>(); source.SetupGet(x => x.DatasourceName).Returns("system"); source.SetupGet(x => x.Dataconnection).Returns(connection.Object); source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.Setup(x => x.GetDataSource("system")).Returns(source.Object);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "system", ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(editor.Object, properties) }).Build();
        var target = new SystemSetupTarget(configuration, editor.Object);
        var first = target.Resolve(); Assert.Equal("system", first.ConnectionName); Assert.Equal(64, first.Fingerprint.Length);
        Assert.Equal(first.Fingerprint, target.FingerprintForMigration(["SYSTEM_SETUP"], "system"));
        Assert.Throws<InvalidOperationException>(() => target.FingerprintForMigration(["SYSTEM_SETUP", "PPDM_CORE"], "system"));
        Assert.Throws<InvalidOperationException>(() => target.FingerprintForMigration(["SYSTEM_SETUP"], "other"));
        properties.Password = "rotated-secret";
        Assert.Equal(first.ConnectionName, target.Resolve().ConnectionName);
        properties.ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "changed.db") }.ConnectionString;
        Assert.Throws<InvalidOperationException>(() => target.Resolve());
    }

    [Fact]
    public void MissingSystemTargetPinFailsBeforeOpeningADatasource()
    {
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite,
            ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "system" }).Build();
        Assert.Throws<InvalidOperationException>(() => new SystemSetupTarget(settings, editor.Object).Resolve());
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void SystemSchemaOwnsSharedRecordsAndCompanyEnvironment()
    {
        var module = new SystemSetupModule(new ModuleSetupContext());
        Assert.Equal("SYSTEM_SETUP", module.ModuleId);
        Assert.Contains(typeof(SYSTEM_SETUP_RECORD), module.EntityTypes);
        Assert.Contains(typeof(COMPANY_OILGAS_ENVIRONMENT), module.EntityTypes);
        Assert.Contains(typeof(COMPANY_GHG_CATALOG_SELECTION), module.EntityTypes);
        Assert.Equal(3, module.EntityTypes.Count);
    }

    [Fact]
    public void UnconfiguredSystemTargetDoesNotOpenAnyDatasource()
    {
        var editor = new Mock<IDMEEditor>();
        var target = new SystemSetupTarget(new ConfigurationBuilder().Build(), editor.Object);
        Assert.Throws<InvalidOperationException>(() => target.Resolve());
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PlanUsesOnlyTheConfiguredSystemTargetAndSystemManifest()
    {
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
        var connection = new Mock<IDataConnection>(); connection.SetupGet(x => x.ConnectionProp).Returns(properties);
        var source = new Mock<IDataSource>(); source.SetupGet(x => x.DatasourceName).Returns("system"); source.SetupGet(x => x.Dataconnection).Returns(connection.Object); source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.Setup(x => x.GetDataSource("system")).Returns(source.Object);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "system", ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(editor.Object, properties) }).Build();
        var migration = new Mock<IPPDM39SchemaMigrationService>();
        migration.Setup(x => x.PlanSchemaMigrationAsync(It.IsAny<SchemaMigrationPlanRequest>())).ReturnsAsync(new SchemaMigrationPlanResult { Success = true, PlanId = "plan" });
        var module = new SystemSetupModule(new ModuleSetupContext());
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("admin", true, ["Administrator"], []));
        var controller = new SystemSetupRepositoryController(new SystemSetupTarget(configuration, editor.Object), editor.Object, [module], migration.Object, configuration, access.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "admin"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
        Assert.IsType<OkObjectResult>(await controller.Plan(new ModulePlanRequest("Test")));
        migration.Verify(x => x.PlanSchemaMigrationAsync(It.Is<SchemaMigrationPlanRequest>(p => p.ConnectionName == "system" && p.ModuleIds!.SequenceEqual(new[] { "SYSTEM_SETUP" }) && p.EnvironmentTier == "Test")), Times.Once);
        Assert.Equal("Administrator", Assert.Single(typeof(SystemSetupRepositoryController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Roles);
    }

    [Theory]
    [InlineData("revoked")]
    [InlineData("target")]
    public async Task PlanCannotPublishAfterAuthorityOrTargetChanges(string change)
    {
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
        var connection = new Mock<IDataConnection>(); connection.SetupGet(x => x.ConnectionProp).Returns(properties);
        var source = new Mock<IDataSource>(); source.SetupGet(x => x.DatasourceName).Returns("system"); source.SetupGet(x => x.Dataconnection).Returns(connection.Object); source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.Setup(x => x.GetDataSource("system")).Returns(source.Object);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "system", ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(editor.Object, properties) }).Build();
        var active = true; var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(() => new RepositoryUserAccess("admin", active, ["Administrator"], []));
        var migration = new Mock<IPPDM39SchemaMigrationService>();
        migration.Setup(x => x.PlanSchemaMigrationAsync(It.IsAny<SchemaMigrationPlanRequest>())).ReturnsAsync(() =>
        { if (change == "revoked") active = false; else properties.ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "changed.db") }.ConnectionString; return new SchemaMigrationPlanResult { Success = true, PlanId = "plan" }; });
        var controller = new SystemSetupRepositoryController(new SystemSetupTarget(configuration, editor.Object), editor.Object,
            [new SystemSetupModule(new ModuleSetupContext())], migration.Object, configuration, access.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "admin"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
        var result = await controller.Plan(new ModulePlanRequest("Test"));
        if (change == "revoked") Assert.IsType<ForbidResult>(result); else Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task StatusSeparatesConfirmedMissingSchemaFromMetadataFailure()
    {
        var properties = new ConnectionProperties { ConnectionName = "system", DatabaseType = DataSourceType.SqlLite,
            ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
        var connection = new Mock<IDataConnection>(); connection.SetupGet(x => x.ConnectionProp).Returns(properties);
        var source = new Mock<IDataSource>(); source.SetupGet(x => x.DatasourceName).Returns("system");
        source.SetupGet(x => x.Dataconnection).Returns(connection.Object); source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
        source.SetupGet(x => x.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Ok });
        var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([properties]);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.Setup(x => x.GetDataSource("system")).Returns(source.Object);
        editor.SetupGet(x => x.classCreator).Returns(new TheTechIdea.Beep.Tools.ClassCreator(editor.Object));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "system", ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(editor.Object, properties) }).Build();
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("admin", true, ["Administrator"], []));
        var controller = new SystemSetupRepositoryController(new SystemSetupTarget(configuration, editor.Object), editor.Object,
            [new SystemSetupModule(new ModuleSetupContext())], Mock.Of<IPPDM39SchemaMigrationService>(), configuration, access.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "admin"), new(ClaimTypes.Role, "Administrator")], "test")) } } };

        source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Throws(new InvalidOperationException("metadata unavailable"));
        Assert.Equal("Unavailable", Assert.IsType<SystemSetupRepositoryStatus>(Assert.IsType<OkObjectResult>(await controller.Status(default)).Value).Status);
        source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns(new EntityStructure { Fields = [] });
        source.Setup(x => x.CheckEntityExist(It.IsAny<string>())).Returns(false);
        Assert.Equal("MigrationRequired", Assert.IsType<SystemSetupRepositoryStatus>(Assert.IsType<OkObjectResult>(await controller.Status(default)).Value).Status);
        source.SetupGet(x => x.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Failed });
        Assert.Equal("Unavailable", Assert.IsType<SystemSetupRepositoryStatus>(Assert.IsType<OkObjectResult>(await controller.Status(default)).Value).Status);
    }

    [Fact]
    public async Task StatusReloadsASelectionSavedAfterThisProcessStarted()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-status-reload-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "system-setup-target.json");
        try
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(path, optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
            var editor = new Mock<IDMEEditor>();
            var access = new Mock<IRepositoryAccessService>();
            access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryUserAccess("admin", true, ["Administrator"], []));
            var controller = new SystemSetupRepositoryController(new SystemSetupTarget(configuration, editor.Object), editor.Object, [],
                Mock.Of<IPPDM39SchemaMigrationService>(), configuration, access.Object)
            { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "admin"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
            Assert.Equal("Unconfigured", Assert.IsType<SystemSetupRepositoryStatus>(Assert.IsType<OkObjectResult>(await controller.Status(default)).Value).Status);
            await File.WriteAllTextAsync(path, "{\"SystemSetup\":{\"ConnectionName\":\"system\",\"ConnectionTarget\":\"saved-target\"}}");
            Assert.Equal("Unavailable", Assert.IsType<SystemSetupRepositoryStatus>(Assert.IsType<OkObjectResult>(await controller.Status(default)).Value).Status);
            Assert.Equal("system", configuration["SystemSetup:ConnectionName"]);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task UnconfiguredStatusRequiresCurrentAdministratorThroughoutRead()
    {
        var editor = new Mock<IDMEEditor>(); var configuration = new ConfigurationBuilder().Build();
        var access = new Mock<IRepositoryAccessService>(); var calls = 0;
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            new RepositoryUserAccess("admin", ++calls == 1, ["Administrator"], []));
        var controller = new SystemSetupRepositoryController(new SystemSetupTarget(configuration, editor.Object), editor.Object, [],
            Mock.Of<IPPDM39SchemaMigrationService>(), configuration, access.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "admin"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
        Assert.IsType<ForbidResult>(await controller.Status(default));
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }
}
#endif
