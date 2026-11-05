using Beep.OilandGas.Repository.Models;
#if MIGRATION_CONTROLLER_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class MigrationDispatchHttpTests
{
    private sealed class Auth(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }

    private sealed class OnlyMigrationController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in feature.Controllers.Where(x => x.AsType() != typeof(PPDM39SetupController)).ToArray())
                feature.Controllers.Remove(controller);
        }
    }

    [Fact]
    public async Task HttpDispatchChecksCurrentRoleAndUsesResolvedActor()
    {
        var administrator = false;
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("actor", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new RepositoryUserAccess("actor", true, administrator ? ["Administrator"] : [], []));
        var migration = new Mock<IPPDM39SchemaMigrationService>(MockBehavior.Strict);
        migration.Setup(x => x.ApproveSchemaMigrationPlanAsync(It.Is<SchemaMigrationApprovalRequest>(r => r.ApprovedBy == "actor")))
            .ReturnsAsync(new SchemaMigrationApprovalResult { Success = true });
        migration.Setup(x => x.ExecuteSchemaMigrationPlanAsync(It.Is<SchemaMigrationExecuteRequest>(r => r.ExecutedBy == "actor")))
            .ReturnsAsync(new SchemaMigrationExecuteResult { Success = true });
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(access.Object); builder.Services.AddSingleton(migration.Object);
        builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, Auth>("test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddControllers().AddApplicationPart(typeof(PPDM39SetupController).Assembly)
            .ConfigureApplicationPartManager(parts => parts.FeatureProviders.Add(new OnlyMigrationController()));
        await using var app = builder.Build();
        app.Use(async (context, next) => { try { await next(); } catch (RefusalException) { context.Response.StatusCode = StatusCodes.Status403Forbidden; } });
        app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        var approval = new SchemaMigrationApprovalRequest { PlanId = "plan", ExpectedPlanHash = "hash", ExpectedManifestHash = "manifest", ApprovedBy = "forged" };
        var execution = new SchemaMigrationExecuteRequest { PlanId = "plan", ExpectedPlanHash = "hash", ExpectedManifestHash = "manifest", ExecutedBy = "forged" };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/ppdm39/setup/schema/approve", approval)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/ppdm39/setup/schema/execute", execution)).StatusCode);
        migration.VerifyNoOtherCalls();
        administrator = true;
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/ppdm39/setup/schema/approve", approval)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/ppdm39/setup/schema/execute", execution)).StatusCode);
        migration.VerifyAll();
        administrator = false;
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/ppdm39/setup/schema/execute", execution)).StatusCode);
        migration.Verify(x => x.ExecuteSchemaMigrationPlanAsync(It.IsAny<SchemaMigrationExecuteRequest>()), Times.Once);
    }
}
#endif
