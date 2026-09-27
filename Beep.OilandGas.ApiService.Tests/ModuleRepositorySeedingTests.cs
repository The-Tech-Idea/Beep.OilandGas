using System.Data;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ModuleRepositorySeedingTests
{
    [Fact]
    public async Task CachedTargetMismatchCannotOpenOrSeed()
    {
        using var fixture = new Fixture();
        fixture.ActiveConnection.Database = "stale-database";
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        fixture.Source.Verify(x => x.Openconnection(), Times.Never);
        fixture.Module.Verify(x => x.SeedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingOrUnreadableSchemaCannotSeed(bool throws)
    {
        using var fixture = new Fixture();
        fixture.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true))
            .Returns(() => throws ? throw new InvalidOperationException("Provider details") : new EntityStructure());
        var result = Assert.IsType<ConflictObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        Assert.DoesNotContain("Provider details", System.Text.Json.JsonSerializer.Serialize(result.Value));
        fixture.Module.Verify(x => x.SeedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ClosedDatasourceCannotSeed()
    {
        using var fixture = new Fixture();
        fixture.Source.Setup(x => x.Openconnection()).Returns(ConnectionState.Closed);
        var result = Assert.IsType<ObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        Assert.Equal(503, result.StatusCode);
        fixture.Source.Verify(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true), Times.Never);
        fixture.Module.Verify(x => x.SeedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void UnboundAllModuleSeedingControllersAreNotExposed()
    {
        var assembly = typeof(ModuleRepositoryController).Assembly;
        var routes = assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>())
            .Select(a => a.Template).ToList();
        Assert.DoesNotContain("api/ppdm/modules", routes);
        Assert.DoesNotContain("api/setup/wizard", routes);
        Assert.Contains("api/setup/modules", routes);
        var actions = assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods())
            .SelectMany(m => m.GetCustomAttributes(typeof(HttpPostAttribute), true).Cast<HttpPostAttribute>())
            .Select(a => a.Template).ToList();
        Assert.DoesNotContain("seed/selected-modules", actions);
    }

    [Fact]
    public async Task UnauthenticatedLocalIdCannotSeed()
    {
        using var fixture = new Fixture();
        fixture.Controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("party_id", "forged-local-user")]));
        Assert.IsType<ForbidResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        fixture.Editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RenamedRepositoryModuleCannotSeed()
    {
        using var fixture = new Fixture();
        fixture.Module.SetupGet(x => x.EntityTypes).Returns([typeof(AppRoleExtension)]);
        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        fixture.Editor.VerifyNoOtherCalls();
        fixture.Module.Verify(x => x.SeedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SeedResponseUsesSelectedModuleAndSharedContract()
    {
        using var fixture = new Fixture();
        fixture.Module.Setup(x => x.SeedAsync("module-db", "local-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModuleSetupResult { ModuleId = "wrong", Success = true, RecordsInserted = 3, TablesSeeded = 1 });
        var response = Assert.IsType<OkObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        var summary = Assert.IsType<ModuleSeedSummary>(response.Value);
        Assert.Equal("PRODUCTION", summary.ModuleId);
        Assert.True(summary.Success);
        Assert.Equal(3, summary.RecordsInserted);
        Assert.Equal(1, summary.TablesSeeded);
    }

    [Fact]
    public async Task AmbiguousNamesCannotBeSelectedBoundPlannedOrSeeded()
    {
        using var fixture = new Fixture();
        fixture.Connections.Add(new() { ConnectionName = "MODULE-DB", Database = "different-target" });
        var choices = Assert.IsType<OkObjectResult>(fixture.Controller.Connections());
        Assert.Empty(Assert.IsType<List<string>>(choices.Value));
        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Bind("PRODUCTION", new("module-db", "version"), default));
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Plan("PRODUCTION", new(ConcurrencyStamp: "version"), default));
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
        fixture.Migration.VerifyNoOtherCalls();
        fixture.Module.Verify(x => x.SeedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("host")]
    [InlineData("database")]
    [InlineData("schema")]
    [InlineData("connection-string")]
    [InlineData("password")]
    [InlineData("file")]
    [InlineData("driver")]
    public async Task MigrationFingerprintChangesWhenNamedTargetIsEdited(string setting)
    {
        using var fixture = new Fixture();
        var before = await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db");
        var connection = fixture.Connections[0];
        switch (setting)
        {
            case "host": connection.Host = "new-server"; break;
            case "database": connection.Database = "new-database"; break;
            case "schema": connection.SchemaName = "new-schema"; break;
            case "connection-string": connection.ConnectionString = "Server=new-server;Database=new-database;Password=secret-test"; break;
            case "password": connection.Password = "secret-test"; break;
            case "file": connection.FileName = "new-database.db"; break;
            case "driver": connection.DriverName = "new-driver"; break;
        }
        var after = await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db");
        Assert.NotEqual(before, after);
        Assert.Matches("^[A-F0-9]{64}$", after);
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MigrationFingerprintIgnoresDisplayPreferencesAndParameterOrdering()
    {
        using var fixture = new Fixture();
        var connection = fixture.Connections[0];
        connection.ParameterList = new() { ["a"] = "one", ["b"] = "two" };
        var before = await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db");
        connection.Favourite = !connection.Favourite;
        connection.ParameterList = new() { ["b"] = "two", ["a"] = "one" };
        Assert.Equal(before, await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db"));
        connection.ParameterList["a"] = "changed";
        Assert.NotEqual(before, await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db"));
    }

    [Fact]
    public async Task MigrationFingerprintRejectsAmbiguousConnectionNames()
    {
        using var fixture = new Fixture();
        fixture.Connections.Add(new() { ConnectionName = "MODULE-DB" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db"));
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MigrationFingerprintRejectsUnboundOrMisdirectedModules()
    {
        using var fixture = new Fixture();
        var fingerprint = await fixture.Resolver.GetMigrationBindingFingerprintAsync(["production"], "module-db");
        Assert.Equal(fingerprint, await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION", "production"], "module-db"));
        Assert.IsType<OkObjectResult>(await fixture.Controller.Bind("PRODUCTION", new("module-db", "version"), default));
        Assert.NotEqual(fingerprint, await fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "other-db"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.GetMigrationBindingFingerprintAsync(["UNBOUND"], "module-db"));
        fixture.Connections.Clear();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "module-db"));
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("stale")]
    public async Task PlanningRejectsUnreviewedBindingBeforeMigration(string? stamp)
    {
        using var fixture = new Fixture();
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Plan("PRODUCTION", new(ConcurrencyStamp: stamp), default));
        fixture.Migration.VerifyNoOtherCalls();
        fixture.Editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PlanningUsesVersionedSavedConnectionAndSelectedModule()
    {
        using var fixture = new Fixture();
        fixture.Migration.Setup(x => x.PlanSchemaMigrationAsync(It.Is<SchemaMigrationPlanRequest>(request =>
                request.ConnectionName == "module-db" && request.ModuleIds.Count == 1 &&
                request.ModuleIds[0] == "PRODUCTION" && request.EnvironmentTier == "Production" &&
                request.BackupConfirmed && request.RestoreTestEvidence == "restore-check")))
            .ReturnsAsync(new SchemaMigrationPlanResult { Success = true });

        Assert.IsType<OkObjectResult>(await fixture.Controller.Plan("production",
            new("Production", true, true, "restore-check", "version"), default));

        fixture.Migration.VerifyAll();
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RuntimeResolverUsesSavedBindingAndRejectsMissingConnection()
    {
        using var fixture = new Fixture();
        Assert.Equal("module-db", await fixture.Resolver.ResolveAsync("production"));
        fixture.Connections.Add(fixture.Connections[0]);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.ResolveAsync("PRODUCTION"));
        fixture.Connections.Clear();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.ResolveAsync("PRODUCTION"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.ResolveAsync("UNBOUND"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Resolver.ResolveAsync("SECURITY"));
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UsesSavedConnectionAndLocalActorForOnlySelectedModule()
    {
        using var fixture = new Fixture();
        fixture.Module.Setup(x => x.SeedAsync("module-db", "local-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModuleSetupResult { Success = true });
        Assert.IsType<OkObjectResult>(await fixture.Controller.Seed("production", new("version"), default));
        fixture.Module.Verify(x => x.SeedAsync("module-db", "local-user", It.IsAny<CancellationToken>()), Times.Once);
        fixture.Source.Verify(x => x.Openconnection(), Times.Once);
        fixture.Other.Verify(x => x.SeedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("stale")]
    [InlineData("")]
    public async Task StaleBindingCannotOpenOrSeed(string stamp)
    {
        using var fixture = new Fixture();
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Seed("PRODUCTION", new(stamp), default));
        fixture.Editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MissingConnectionCannotFallBackToDefault()
    {
        using var fixture = new Fixture();
        fixture.Connections.Clear();
        Assert.IsType<ConflictObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
        fixture.Editor.Verify(x => x.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MissingLocalActorCannotSeed()
    {
        using var fixture = new Fixture();
        fixture.Controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "external-subject"), new Claim("sub", "external-subject")], "external"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Controller.Seed("PRODUCTION", new("version"), default));
        fixture.Editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PartialSeedingErrorsAreNotReportedAsSuccess()
    {
        using var fixture = new Fixture();
        fixture.Module.Setup(x => x.SeedAsync("module-db", "local-user", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModuleSetupResult { Success = true, Errors = ["Missing table"] });
        Assert.IsType<BadRequestObjectResult>(await fixture.Controller.Seed("PRODUCTION", new("version"), default));
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);

    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly TestContext db;
        public Mock<IDMEEditor> Editor { get; } = new(MockBehavior.Strict);
        public Mock<IDataSource> Source { get; } = new(MockBehavior.Strict);
        public ConnectionProperties ActiveConnection { get; } = new() { ConnectionName = "module-db" };
        public Mock<IModuleSetup> Module { get; } = new(MockBehavior.Strict);
        public Mock<IModuleSetup> Other { get; } = new(MockBehavior.Strict);
        public Mock<IPPDM39SchemaMigrationService> Migration { get; } = new(MockBehavior.Strict);
        public List<ConnectionProperties> Connections { get; } = [new() { ConnectionName = "module-db" }];
        public ModuleRepositoryController Controller { get; }
        public Beep.OilandGas.ApiService.Services.ModuleConnectionResolver Resolver { get; }

        public Fixture()
        {
            ActiveConnection.GuidID = Connections[0].GuidID;
            connection.Open();
            db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
            db.Database.EnsureCreated();
            db.ModuleDatabases.Add(new() { ModuleId = "PRODUCTION", ConnectionName = "module-db", ConcurrencyStamp = "version" });
            db.SaveChanges();
            Module.SetupGet(x => x.ModuleId).Returns("PRODUCTION");
            Module.SetupGet(x => x.EntityTypes).Returns([typeof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION)]);
            Other.SetupGet(x => x.ModuleId).Returns("EXPLORATION");
            var config = new Mock<IConfigEditor>();
            config.SetupGet(x => x.DataConnections).Returns(Connections);
            Editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
            Editor.Setup(x => x.OpenDataSource("module-db")).Returns(ConnectionState.Open);
            var creator = new ClassCreator(Editor.Object);
            Editor.SetupGet(x => x.classCreator).Returns(creator);
            Editor.Setup(x => x.GetDataSource("module-db")).Returns(Source.Object);
            Source.SetupGet(x => x.DatasourceName).Returns("module-db");
            var dataConnection = new Mock<IDataConnection>();
            dataConnection.SetupGet(x => x.ConnectionProp).Returns(ActiveConnection);
            Source.SetupGet(x => x.Dataconnection).Returns(dataConnection.Object);
            Source.Setup(x => x.Openconnection()).Returns(ConnectionState.Open);
            Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true))
                .Returns(() => creator.ConvertToEntityStructure(typeof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION)));
            Controller = new(db, Editor.Object, [Module.Object, Other.Object], Migration.Object);
            Resolver = new(db, Editor.Object);
            Controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "local-user")], "repository"))
            } };
        }

        public void Dispose() { db.Dispose(); connection.Dispose(); }
    }
}
