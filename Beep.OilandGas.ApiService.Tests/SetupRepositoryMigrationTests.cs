#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SetupMigrationContext(DbContextOptions<SetupMigrationContext> options) : RepositoryDbContext(options);

[DbContext(typeof(SetupMigrationContext))]
[Migration("20261028000000_SetupMigrationProbe")]
public sealed class SetupMigrationProbe : Migration
{
    protected override void Up(MigrationBuilder builder) => builder.CreateTable("SETUP_MIGRATION_PROBE", table => new { Id = table.Column<int>(nullable: false) }, constraints: constraints => constraints.PrimaryKey("PK_SETUP_MIGRATION_PROBE", x => x.Id));
    protected override void Down(MigrationBuilder builder) => builder.DropTable("SETUP_MIGRATION_PROBE");
}

public sealed class SetupRepositoryMigrationTests
{
    [Fact]
    public async Task BootstrapHttpRequiresKeyAndExecutesReviewedMigration()
    {
        const string key = "this-is-a-long-setup-test-key-12345";
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new SetupMigrationContext(new DbContextOptionsBuilder<SetupMigrationContext>().UseSqlite(connection).Options);
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RepositoryReadiness.MigrationRequired);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["SetupBootstrap:Enabled"] = "true", ["SetupBootstrap:Key"] = key });
        builder.Services.AddSingleton(readiness.Object);
        builder.Services.AddSingleton<RepositoryDbContext>(db);
        builder.Services.AddScoped<SetupBootstrapAccess>();
        builder.Services.AddScoped<SetupRepositoryMigrationService>();
        builder.Services.AddSingleton<Func<ClaimsPrincipal, string?>>(_ => _ => null);
        builder.Services.AddControllers().AddApplicationPart(typeof(SetupBootstrapRepositoryController).Assembly);
        await using var app = builder.Build();
        app.UseMiddleware<SetupGateMiddleware>(); app.UseMiddleware<CompanyRequestMiddleware>(); app.MapControllers();
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/setup/bootstrap/repository/migration")).StatusCode);
        using var review = new HttpRequestMessage(HttpMethod.Get, "/api/setup/bootstrap/repository/migration"); review.Headers.Add("X-Setup-Key", key);
        using var reviewed = await client.SendAsync(review);
        Assert.Equal(HttpStatusCode.OK, reviewed.StatusCode);
        var plan = (await reviewed.Content.ReadFromJsonAsync<SetupRepositoryMigrationPlan>())!;
        using var apply = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/migration");
        apply.Headers.Add("X-Setup-Key", key); apply.Content = JsonContent.Create(new SetupRepositoryMigrationRequest(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations));
        using var applied = await client.SendAsync(apply);
        Assert.Equal(HttpStatusCode.OK, applied.StatusCode);
        Assert.Equal(plan.PendingMigrations, await db.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task ReviewedPlanMustMatchBeforeEfMigrationRuns()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SetupMigrationContext>().UseSqlite(connection).Options;
        await using var db = new SetupMigrationContext(options);
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RepositoryReadiness.MigrationRequired);
        var service = new SetupRepositoryMigrationService(db, readiness.Object);
        var plan = await service.PlanAsync(() => Task.CompletedTask, default);
        Assert.Single(plan.PendingMigrations);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyAsync(new(plan.Provider, plan.TargetToken, plan.CreatesDatabase, ["changed"]), () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyAsync(new(plan.Provider, "changed", plan.CreatesDatabase, plan.PendingMigrations), () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyAsync(new(plan.Provider, plan.TargetToken, !plan.CreatesDatabase, plan.PendingMigrations), () => Task.CompletedTask, default));
        var checks = 0;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ApplyAsync(new(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations),
            () => ++checks == 3 ? throw new UnauthorizedAccessException() : Task.CompletedTask, default));
        Assert.Empty(await db.Database.GetAppliedMigrationsAsync());
        var result = await service.ApplyAsync(new(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations), () => Task.CompletedTask, default);
        Assert.Equal(RepositoryReadiness.MigrationRequired, result.Readiness); // controlled readiness boundary
        Assert.Equal(plan.PendingMigrations, await db.Database.GetAppliedMigrationsAsync());
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task MissingAuthorityCannotReviewOrApply()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new SetupMigrationContext(new DbContextOptionsBuilder<SetupMigrationContext>().UseSqlite(connection).Options);
        var readiness = new Mock<IRepositoryReadinessService>();
        var service = new SetupRepositoryMigrationService(db, readiness.Object);
        Task Deny() => throw new UnauthorizedAccessException();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.PlanAsync(Deny, default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ApplyAsync(new("provider", "token", false, ["migration"]), Deny, default));
        Assert.Empty(await db.Database.GetAppliedMigrationsAsync());
    }
}
#endif
