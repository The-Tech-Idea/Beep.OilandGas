using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
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
using FileImportResult = Beep.OilandGas.PPDM39.Core.FileImportResult;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyModuleConnectionTests
{
    private static readonly CompanyScope A = new("A", "company-a");

    [Fact]
    public async Task Each_company_uses_its_tenants_core_binding_with_no_startup_default()
    {
        using var f = new Fixture(); await f.Select(A);
        Assert.Equal("db-a", (await f.Gate.ResolveAsync()).ConnectionName);
        await f.Select(new("B", "company-b")); Assert.Equal("db-b", (await f.Gate.ResolveAsync()).ConnectionName);
        f.Editor.Verify(editor => editor.GetDataSource(It.IsAny<string>()), Times.Never);
        f.Editor.Verify(editor => editor.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("foreign-query")]
    [InlineData("foreign-body")]
    [InlineData("conflicting-selectors")]
    [InlineData("empty-query")]
    [InlineData("empty-body")]
    public async Task Connection_arguments_are_assertions_and_can_never_select_another_target(string choice)
    {
        using var f = new Fixture(); await f.Select(A);
        var query = choice switch { "foreign-query" or "conflicting-selectors" => "db-b", "empty-query" => " ", _ => null };
        var body = choice switch { "foreign-body" => "db-b", "conflicting-selectors" => "db-a", "empty-body" => "", _ => null };
        await Assert.ThrowsAsync<RefusalException>(() => f.Gate.ResolveAsync(query, body));
        Assert.Equal("db-a", (await f.Gate.ResolveAsync("DB-A", "db-a")).ConnectionName);
        f.Editor.Verify(editor => editor.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("no-selection")]
    [InlineData("membership")]
    [InlineData("role")]
    [InlineData("permission")]
    [InlineData("disabled-tenant")]
    public async Task Missing_or_revoked_company_authority_refuses_business_resolution(string reason)
    {
        using var f = new Fixture();
        if (reason != "no-selection") await f.Select(A);
        else f.Tenant.SelectTenant("A");
        if (reason == "membership") f.Available = false;
        if (reason == "role") f.Roles = [];
        if (reason == "permission") f.Permissions = [];
        if (reason == "disabled-tenant") { (await f.Db.Set<AppTenant>().FindAsync("A"))!.IsActive = false; await f.Db.SaveChangesAsync(); }
        await Assert.ThrowsAsync<RefusalException>(() => f.Gate.ResolveAsync());
        f.Editor.Verify(editor => editor.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Cancellation_and_an_unavailable_binding_never_reach_a_business_datasource()
    {
        using var f = new Fixture(); await f.Select(A);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.Gate.ResolveAsync(token: new CancellationToken(true)));
        f.Db.ModuleDatabases.Remove(await f.Db.ModuleDatabases.SingleAsync(binding => binding.TenantId == "A")); await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Gate.ResolveAsync());
        f.Editor.Verify(editor => editor.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Writes_require_the_authoritative_selected_account()
    {
        using var f = new Fixture(); await f.Select(A);
        f.Gate.RequireActor("actor"); Assert.Throws<RefusalException>(() => f.Gate.RequireActor("forged"));
    }

    [Fact]
    public async Task A_request_never_switches_bindings_between_rows_or_operations()
    {
        using var f = new Fixture(); await f.Select(A); var original = await f.Gate.ResolveAsync();
        (await f.Db.ModuleDatabases.SingleAsync(binding => binding.TenantId == "A")).ConcurrencyStamp = "changed";
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Gate.ResolveAsync());
        await f.Select(A); Assert.NotEqual(original, await f.Gate.ResolveAsync());
    }

    [Theory]
    [InlineData("tenant")]
    [InlineData("company")]
    [InlineData("account")]
    [InlineData("binding")]
    [InlineData("target")]
    [InlineData("credentials")]
    public async Task A_queued_import_cannot_run_in_a_substituted_scope_or_changed_binding(string change)
    {
        using var f = new Fixture(); await f.Select(A); var job = await f.Job();
        if (change == "tenant") await f.Select(new("B", "company-a"));
        if (change == "company") await f.Select(new("A", "another-company"));
        if (change == "account") await f.Select(A, "other-account");
        if (change == "binding")
        { (await f.Db.ModuleDatabases.SingleAsync(binding => binding.TenantId == "A")).ConcurrencyStamp = "rebound"; await f.Db.SaveChangesAsync(); }
        if (change == "target") f.Connections[0].Database = "another-database";
        if (change == "credentials") f.Connections[0].Password = "rotated";
        await Assert.ThrowsAsync<RefusalException>(() => f.Gate.RequireJobAsync(job, default));
        f.Editor.Verify(editor => editor.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Queued_import_captures_the_shared_immutable_database_scope_and_revalidates_it()
    {
        using var f = new Fixture(); await f.Select(A); var job = await f.Job();
        Assert.Equal(A, job.Company); Assert.Equal("db-a", job.ConnectionName); Assert.Equal(64, job.Database.Fingerprint.Length);
        await f.Gate.RequireJobAsync(job, default);
        f.Roles = ["Admin", "Engineer"]; await f.Gate.RequireJobAsync(job, default);
        Assert.Contains("Engineer", f.Gate.Company.Roles);
    }

    [Theory]
    [InlineData("company")]
    [InlineData("binding")]
    [InlineData("membership")]
    public async Task The_actual_import_runner_refuses_changed_authority_before_creating_input_or_invoking_execution(string change)
    {
        using var f = new Fixture(); await f.Select(A); var job = await f.Job();
        if (change == "company") await f.Select(new("A", "another-company"));
        if (change == "membership") f.Available = false;
        if (change == "binding")
        { (await f.Db.ModuleDatabases.SingleAsync(binding => binding.TenantId == "A")).ConcurrencyStamp = "changed"; await f.Db.SaveChangesAsync(); }
        var executor = new Mock<ICsvImportExecutor>(MockBehavior.Strict); var progress = new Mock<IProgressTrackingService>();
        var runner = new CsvImportJobRunner(executor.Object, progress.Object, f.Gate);
        await Assert.ThrowsAsync<RefusalException>(() => runner.RunAsync(job, default));
        executor.VerifyNoOtherCalls(); progress.Verify(tracker => tracker.CompleteOperation(job.OperationId, false, null, It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("success")]
    [InlineData("rows-failed")]
    [InlineData("executor-error")]
    public async Task The_actual_runner_keeps_the_captured_target_owns_its_file_and_reports_real_completion(string outcome)
    {
        using var f = new Fixture(); await f.Select(A); var job = await f.Job(); string? path = null;
        var executor = new Mock<ICsvImportExecutor>(); var progress = new Mock<IProgressTrackingService>();
        executor.Setup(service => service.ExecuteAsync(job, It.IsAny<string>())).Returns(async (CsvImportJob captured, string file) =>
        {
            path = file; Assert.Equal(job.Database, captured.Database); Assert.Equal(job.Content, await File.ReadAllBytesAsync(file));
            if (outcome == "executor-error") throw new InvalidOperationException("provider failed");
            return new FileImportResult { SuccessCount = 2, Errors = outcome == "rows-failed" ? [new()] : [] };
        });
        var runner = new CsvImportJobRunner(executor.Object, progress.Object, f.Gate);
        if (outcome == "success") await runner.RunAsync(job, default);
        else await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(job, default));
        Assert.NotNull(path); Assert.False(File.Exists(path));
        progress.Verify(tracker => tracker.CompleteOperation(job.OperationId, outcome == "success", It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
    }

    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection database = new("Data Source=:memory:");
        public Context Db { get; }
        public Mock<IDMEEditor> Editor { get; } = new();
        public List<ConnectionProperties> Connections { get; } = [
            new() { ConnectionName = "db-a", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "database-a" },
            new() { ConnectionName = "db-b", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "database-b" }];
        public bool Available = true;
        public string[] Roles = ["Admin"];
        public string[] Permissions = ["read"];
        public RepositoryTenantContext Tenant { get; private set; } = new();
        private readonly RepositoryCompanyContextResolver companies;
        public CompanyModuleConnection Gate { get; private set; }

        public Fixture()
        {
            database.Open(); Db = new(new DbContextOptionsBuilder<Context>().UseSqlite(database).Options); Db.Database.EnsureCreated();
            var config = new Mock<IConfigEditor>(); config.SetupGet(editor => editor.DataConnections).Returns(Connections); Editor.SetupGet(editor => editor.ConfigEditor).Returns(config.Object);
            Db.AddRange(new AppTenant { TenantId = "A" }, new AppTenant { TenantId = "B" });
            for (var index = 0; index < 2; index++)
            {
                var tenant = index == 0 ? "A" : "B"; var connection = Connections[index];
                Db.Add(new AppModuleConnection { TenantId = tenant, ConnectionKey = connection.ConnectionName.ToUpperInvariant(), AssignedByUserId = "admin", PhysicalTargetKey = ModuleDatabaseTarget.Key(Editor.Object, connection) });
                Db.Add(new ModuleDatabaseBinding { TenantId = tenant, ModuleId = "PPDM_CORE", ConnectionName = connection.ConnectionName, ConnectionKey = connection.ConnectionName.ToUpperInvariant(), ConcurrencyStamp = "v1" });
            }
            Db.SaveChanges();
            var access = new Mock<IRepositoryAccessService>();
            access.Setup(service => service.GetCompanyAccessAsync(It.IsAny<string>(), It.IsAny<CompanyScope>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string _, CompanyScope scope, CancellationToken _) => Available ? new(scope, Roles, Permissions) : null);
            companies = new(access.Object); Gate = new(new(Db, Editor.Object, Tenant), Tenant, companies);
        }
        public async Task Select(CompanyScope scope, string user = "actor")
        { Tenant = new(); Tenant.SelectCompany((await companies.ResolveAsync(user, scope))!); Gate = new(new(Db, Editor.Object, Tenant), Tenant, companies); }
        public async Task<CsvImportJob> Job() => new("operation", "WELL", Gate.Company.Scope, await Gate.ResolveAsync(), Gate.Company.UserId, true, [1, 2, 3]);
        public void Dispose() { Db.Dispose(); database.Dispose(); }
    }
}
