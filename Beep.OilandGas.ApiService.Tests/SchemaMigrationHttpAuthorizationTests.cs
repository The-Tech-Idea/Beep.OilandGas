using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class SchemaMigrationHttpAuthorizationTests
{
    [Theory]
    [InlineData("anonymous", HttpStatusCode.Unauthorized)]
    [InlineData("member", HttpStatusCode.Forbidden)]
    [InlineData("unregistered", HttpStatusCode.Forbidden)]
    [InlineData("disabled", HttpStatusCode.Forbidden)]
    [InlineData("unavailable", HttpStatusCode.Forbidden)]
    [InlineData("admin", HttpStatusCode.OK)]
    public async Task MigrationEndpointsUseRepositoryRolesAndLocalAuditActor(string scenario, HttpStatusCode expected)
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        var lookup = access.Setup(x => x.GetAccessAsync("https://test-issuer.invalid", "external-subject", It.IsAny<CancellationToken>()));
        if (scenario == "unavailable") lookup.ThrowsAsync(new InvalidOperationException("Test repository unavailable"));
        else lookup.ReturnsAsync(scenario == "unregistered" ? null : new RepositoryUserAccess(
            "local-user", scenario != "disabled", scenario == "admin" ? ["Administrator"] : [], []));
        var migration = new Mock<IPPDM39SchemaMigrationService>(MockBehavior.Strict);
        migration.Setup(x => x.ApproveSchemaMigrationPlanAsync(It.IsAny<SchemaMigrationApprovalRequest>()))
            .ReturnsAsync(new SchemaMigrationApprovalResult { Success = true });
        migration.Setup(x => x.ExecuteSchemaMigrationPlanAsync(It.IsAny<SchemaMigrationExecuteRequest>()))
            .ReturnsAsync(new SchemaMigrationExecuteResult { Success = true });
        var editor = Mock.Of<IDMEEditor>();
        var common = Mock.Of<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var metadata = Mock.Of<IPPDMMetadataRepository>();
        var setup = new PPDM39SetupService(editor, NullLogger<PPDM39SetupService>.Instance, common, defaults, metadata);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(access.Object);
        builder.Services.AddScoped<IClaimsTransformation, RepositoryClaimsTransformation>();
        builder.Services.AddAuthentication("TestExternal")
            .AddScheme<AuthenticationSchemeOptions, TestExternalHandler>("TestExternal", _ => { });
        builder.Services.AddAuthorization(RepositoryAuthorization.Configure);
        builder.Services.AddTransient(_ => new PPDM39SetupController(setup, migration.Object, editor,
            NullLogger<PPDM39SetupController>.Instance, commonColumnHandler: common, defaults: defaults, metadata: metadata));
        builder.Services.AddControllers().AddApplicationPart(typeof(PPDM39SetupController).Assembly)
            .ConfigureApplicationPartManager(parts => parts.FeatureProviders.Add(new OnlySetupController()))
            .AddControllersAsServices();
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            if (scenario != "anonymous") client.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
            using var approved = await client.PostAsJsonAsync("/api/ppdm39/setup/schema/approve",
                new SchemaMigrationApprovalRequest { PlanId = "plan", ApprovedBy = "forged-actor" });
            Assert.Equal(expected, approved.StatusCode);
            using var executed = await client.PostAsJsonAsync("/api/ppdm39/setup/schema/execute",
                new SchemaMigrationExecuteRequest { PlanId = "plan", ExecutedBy = "forged-actor",
                    ExpectedPlanHash = "hash", ExpectedManifestHash = "manifest" });
            Assert.Equal(expected, executed.StatusCode);
            if (scenario == "admin")
            {
                lookup.ReturnsAsync(new RepositoryUserAccess("local-user", true, [], []));
                using var revoked = await client.PostAsJsonAsync("/api/ppdm39/setup/schema/execute",
                    new SchemaMigrationExecuteRequest { PlanId = "plan", ExpectedPlanHash = "hash",
                        ExpectedManifestHash = "manifest" });
                Assert.Equal(HttpStatusCode.Forbidden, revoked.StatusCode);
                migration.Verify(x => x.ApproveSchemaMigrationPlanAsync(It.Is<SchemaMigrationApprovalRequest>(
                    request => request.ApprovedBy == "local-user")), Times.Once);
                migration.Verify(x => x.ExecuteSchemaMigrationPlanAsync(It.Is<SchemaMigrationExecuteRequest>(
                    request => request.ExecutedBy == "local-user")), Times.Once);
            }
            else migration.VerifyNoOtherCalls();
        }
        finally { await app.StopAsync(); }
    }

    private sealed class OnlySetupController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in feature.Controllers.Where(x => x.AsType() != typeof(PPDM39SetupController)).ToArray())
                feature.Controllers.Remove(controller);
        }
    }

    // This scheme is registered only in the isolated test host, never in the application.
    private sealed class TestExternalHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("X-Test-Authenticated"))
                return Task.FromResult(AuthenticateResult.NoResult());
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("iss", "https://test-issuer.invalid"), new Claim("sub", "external-subject"),
                new Claim(ClaimTypes.Role, "Administrator"), new Claim(ClaimTypes.NameIdentifier, "external-id")
            }, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
