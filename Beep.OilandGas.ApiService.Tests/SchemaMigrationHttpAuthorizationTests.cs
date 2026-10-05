using System.Net;
using System.Net.Http.Json;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.ApiService.Controllers.PPDM39;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    [InlineData("unresolved", HttpStatusCode.Forbidden)]
    [InlineData("disabled", HttpStatusCode.Forbidden)]
    [InlineData("unavailable", HttpStatusCode.Forbidden)]
    [InlineData("admin", HttpStatusCode.OK)]
    public async Task MigrationEndpointsUseRepositoryRolesAndLocalAuditActor(string scenario, HttpStatusCode expected)
    {
        var access = new Mock<IRepositoryAccessService>(MockBehavior.Strict);
        var lookup = access.Setup(x => x.GetAccessAsync("local-user", It.IsAny<CancellationToken>()));
        if (scenario == "unavailable") lookup.ThrowsAsync(new InvalidOperationException("Test repository unavailable"));
        else lookup.ReturnsAsync(new RepositoryUserAccess(
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
        var setup = new PPDM39SetupService(editor, NullLogger<PPDM39SetupService>.Instance, common, defaults, metadata, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter());

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(access.Object);
        // The API's own identity over signed tokens; the account store is one known person, or one that cannot answer.
        builder.AddApiIdentity();
        builder.Services.AddSingleton<ICanonicalUserStore<string>>(new OneAccount(resolves: scenario != "unresolved"));
        builder.Services.AddTransient(_ => new PPDM39SetupController(setup, migration.Object, editor,
            NullLogger<PPDM39SetupController>.Instance, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), commonColumnHandler: common, defaults: defaults, metadata: metadata));
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
            // The token carries a role and names another account; neither counts.
            if (scenario != "anonymous")
                client.SignIn(SignedTokens.Person("external-subject", "person@example.test",
                    extra: [("role", "Administrator"), ("party_id", "external-id")]));
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

    /// <summary>The repository's accounts: the token's subject is local-user, or the store cannot answer.</summary>
    private sealed class OneAccount(bool resolves) : ICanonicalUserStore<string>
    {
        public Task<(bool Found, string Key)> TryFindByCurrentSubjectAsync(string subject, CancellationToken cancellationToken) =>
            resolves
                ? Task.FromResult(subject == "external-subject" ? (true, "local-user") : (false, string.Empty))
                : throw new InvalidOperationException("Test repository unavailable");

        public Task<IReadOnlyList<string>> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public Task UpdateCurrentSubjectAsync(string key, string newSubject, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Nothing is relinked in these tests.");

        public Task<string> ProvisionAsync(string subject, string? email, string? displayName, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Nobody is provisioned in these tests.");

        public Task UpdateProfileAsync(string key, string? verifiedEmail, string? displayName, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
