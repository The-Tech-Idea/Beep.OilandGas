using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Controllers.Identity;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Repository;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryRegistrationHttpTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task FirstSightProvisioningAndUserAdministrationUseLocalDbIdentityOverHttp()
    {
        var database = $"BeepOilGas_Http_{Guid.NewGuid():N}";
        output.WriteLine($"Retained HTTP repository test database: {database}");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = "SqlServer",
            ["Repository:ConnectionString"] = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true"
        });
        builder.Services.AddOilGasRepository(builder.Configuration);
        var taskRouter = new Mock<ICrossPersonaTaskRouter>(MockBehavior.Strict);
        taskRouter.Setup(x => x.GetTasksForPersonaAsync("ENGINEER")).ReturnsAsync(new List<CROSS_PERSONA_TASK>
        {
            new() { CROSS_TASK_ID = "reader-task", TARGET_PERSONA_CODE = "ENGINEER", ASSIGNED_ROLE = "Reader", TASK_TYPE = "APPROVAL", PRIORITY = 1 },
            new() { CROSS_TASK_ID = "admin-task", TARGET_PERSONA_CODE = "ENGINEER", ASSIGNED_ROLE = "Administrator", TASK_TYPE = "APPROVAL", PRIORITY = 1 },
            new() { CROSS_TASK_ID = "foreign-persona", TARGET_PERSONA_CODE = "OTHER", ASSIGNED_ROLE = "Reader" }
        });
        builder.Services.AddSingleton(taskRouter.Object);
        var assetAccess = new Mock<Beep.OilandGas.Models.Core.Interfaces.IAccessControlService>(MockBehavior.Strict);
        builder.Services.AddSingleton(assetAccess.Object);
        var hierarchy = new Mock<Beep.OilandGas.Models.Core.Interfaces.IAssetHierarchyService>(MockBehavior.Strict);
        builder.Services.AddSingleton(hierarchy.Object);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<RepositoryUserService>();
        builder.Services.AddScoped<RepositoryRoleAssignmentService>();
        builder.Services.AddScoped<RepositoryRoleCatalogService>();
        // The API's own identity over signed tokens: each person is provisioned at first sight, the first administers.
        builder.AddApiIdentity();
        builder.Services.AddControllers().AddApplicationPart(typeof(RepositoryAccountController).Assembly)
            .ConfigureApplicationPartManager(parts => parts.FeatureProviders.Add(new AccountControllers()));
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await using (var setup = app.Services.CreateAsyncScope())
            await setup.ServiceProvider.GetRequiredService<RepositoryDbContext>().Database.MigrateAsync();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            using (var anonymous = await client.GetAsync("/api/auth/repository/me"))
                Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
            using (var anonymousInbox = await client.GetAsync("/api/workflow/tasks/inbox"))
                Assert.Equal(HttpStatusCode.Unauthorized, anonymousInbox.StatusCode);

            // The first person to sign in administers; what their token says about roles or accounts counts for nothing.
            SetSubject(client, "first");
            var admin = await client.GetFromJsonAsync<RepositoryUserAccess>("/api/auth/repository/me");
            Assert.NotNull(admin);
            Assert.Equal(new[] { "Administrator" }, admin.Roles);
            Assert.NotEqual("forged-local-id", admin.UserId);

            // A subject differing only in case is not the first person's (OIDC subjects are case-sensitive), and SQL
            // Server's collation will not hold it beside theirs: it is refused, and no account is made for it.
            SetSubject(client, "FIRST");
            using (var aliasedAccount = await client.GetAsync("/api/auth/repository/me"))
                Assert.NotEqual(HttpStatusCode.OK, aliasedAccount.StatusCode);
            using (var aliasedAdmin = await client.GetAsync("/api/identity/users"))
                Assert.Equal(HttpStatusCode.Forbidden, aliasedAdmin.StatusCode);

            // The next person is a member with no role.
            SetSubject(client, "second");
            var member = await client.GetFromJsonAsync<RepositoryUserAccess>("/api/auth/repository/me");
            Assert.NotNull(member);
            Assert.Empty(member.Roles);
            using (var denied = await client.GetAsync("/api/identity/users"))
                Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
            using (var selfPromotion = await client.PostAsJsonAsync($"/api/identity/users/{member.UserId}/roles",
                new { RoleName = "Administrator" }))
                Assert.Equal(HttpStatusCode.Forbidden, selfPromotion.StatusCode);
            using (var removeAdmin = await client.DeleteAsync($"/api/identity/users/{admin.UserId}/roles/Administrator"))
                Assert.Equal(HttpStatusCode.Forbidden, removeAdmin.StatusCode);
            using (var grantAsset = await client.PostAsJsonAsync("/api/AccessControl/grant-access",
                new { UserId = member.UserId, AssetId = "field", AssetType = "FIELD", AccessLevel = "WRITE" }))
                Assert.Equal(HttpStatusCode.Forbidden, grantAsset.StatusCode);
            using (var revokeAsset = await client.PostAsJsonAsync("/api/AccessControl/revoke-access",
                new { UserId = admin.UserId, AssetId = "field", AssetType = "FIELD" }))
                Assert.Equal(HttpStatusCode.Forbidden, revokeAsset.StatusCode);
            using (var grantPermission = await client.PostAsJsonAsync("/api/AccessControl/role/reader/permission/write", new { }))
                Assert.Equal(HttpStatusCode.Forbidden, grantPermission.StatusCode);
            using (var revokePermission = await client.DeleteAsync("/api/AccessControl/role/reader/permission/read"))
                Assert.Equal(HttpStatusCode.Forbidden, revokePermission.StatusCode);
            assetAccess.VerifyNoOtherCalls();
            foreach (var route in new[] { "/api/AssetHierarchy/organization/organization",
                "/api/AssetHierarchy/organization/organization/config", "/api/AssetHierarchy/asset/field/FIELD/children",
                "/api/AssetHierarchy/asset/well/WELL/path", $"/api/AssetHierarchy/user/{admin.UserId}" })
            {
                using var hierarchyRead = await client.GetAsync(route);
                Assert.Equal(HttpStatusCode.Forbidden, hierarchyRead.StatusCode);
            }
            using (var hierarchyWrite = await client.PutAsJsonAsync("/api/AssetHierarchy/organization/organization/config", new object[0]))
                Assert.Equal(HttpStatusCode.Forbidden, hierarchyWrite.StatusCode);
            using (var hierarchyValidation = await client.PostAsJsonAsync("/api/AssetHierarchy/validate-access",
                new { UserId = admin.UserId, AssetPath = new object[0] }))
                Assert.Equal(HttpStatusCode.Forbidden, hierarchyValidation.StatusCode);
            hierarchy.VerifyNoOtherCalls();

            SetSubject(client, "first");
            var accounts = await client.GetFromJsonAsync<List<RepositoryUserSummary>>("/api/identity/users");
            Assert.NotNull(accounts);
            Assert.Equal(2, accounts.Count);
            Assert.Equal("External first", accounts.Single(x => x.UserId == admin.UserId).FullName);
            Assert.Equal("External second", accounts.Single(x => x.UserId == member.UserId).FullName);
            // Only an address the identity server verified is kept.
            Assert.Equal("shared@example.invalid", accounts.Single(x => x.UserId == admin.UserId).Email);
            Assert.Null(accounts.Single(x => x.UserId == member.UserId).Email);
            var memberSummary = accounts.Single(x => x.UserId == member.UserId);
            using (var invalidUser = await client.PutAsJsonAsync($"/api/identity/users/{member.UserId}",
                new RepositoryUserUpdate(new string('x', 1001), false, memberSummary.ConcurrencyStamp)))
                Assert.Equal(HttpStatusCode.BadRequest, invalidUser.StatusCode);
            using (var invalidRole = await client.PostAsJsonAsync("/api/identity/roles", new RepositoryRoleRequest("", null)))
                Assert.Equal(HttpStatusCode.BadRequest, invalidRole.StatusCode);
            using (var validRole = await client.PostAsJsonAsync("/api/identity/roles", new RepositoryRoleRequest("Reader", "Read access")))
                Assert.Equal(HttpStatusCode.OK, validRole.StatusCode);
            using (var invalidCatalog = await client.PutAsJsonAsync("/api/personas/ENGINEER", new PersonaCatalogUpdate("")))
                Assert.Equal(HttpStatusCode.BadRequest, invalidCatalog.StatusCode);
            using (var catalog = await client.PutAsJsonAsync("/api/personas/ENGINEER", new PersonaCatalogUpdate("Engineer")))
                Assert.Equal(HttpStatusCode.OK, catalog.StatusCode);
            using (var invalidProfile = await client.PutAsJsonAsync($"/api/personas/users/{member.UserId}",
                new PersonaProfileUpdate("ENGINEER", Locale: new string('x', 33))))
                Assert.Equal(HttpStatusCode.BadRequest, invalidProfile.StatusCode);
            using (var profile = await client.PutAsJsonAsync($"/api/personas/users/{member.UserId}",
                new PersonaProfileUpdate("ENGINEER", Locale: "en")))
                Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
            using (var invalidPreference = await client.PutAsJsonAsync($"/api/personas/users/{member.UserId}/preferences/ENGINEER/layout",
                new PersonaPreferenceUpdate(new string('x', 4001))))
                Assert.Equal(HttpStatusCode.BadRequest, invalidPreference.StatusCode);
            using (var preference = await client.PutAsJsonAsync($"/api/personas/users/{member.UserId}/preferences/ENGINEER/layout",
                new PersonaPreferenceUpdate("compact")))
                Assert.Equal(HttpStatusCode.OK, preference.StatusCode);
            using (var grant = await client.PostAsJsonAsync($"/api/identity/users/{member.UserId}/roles",
                new { RoleName = "Reader" }))
                Assert.Equal(HttpStatusCode.NoContent, grant.StatusCode);
            SetSubject(client, "second");
            var grantedAccess = await client.GetFromJsonAsync<RepositoryUserAccess>("/api/auth/repository/me");
            Assert.Equal(new[] { "Reader" }, grantedAccess!.Roles);
            var inbox = await client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox");
            Assert.Equal("reader-task", Assert.Single(inbox!.CriticalTasks).TaskId);
            Assert.Equal(1, inbox.Counts.TotalPending);
            var counts = await client.GetFromJsonAsync<InboxCounts>("/api/workflow/tasks/counts?personaCode=ENGINEER");
            Assert.Equal(1, counts!.Approvals);
            var filtered = await client.GetFromJsonAsync<List<UnifiedTask>>("/api/workflow/tasks?taskType=APPROVAL&pageSize=1&page=0");
            Assert.Equal("reader-task", Assert.Single(filtered!).TaskId);
            using (var foreignPersona = await client.GetAsync("/api/workflow/tasks/inbox?personaCode=OTHER"))
                Assert.Equal(HttpStatusCode.Forbidden, foreignPersona.StatusCode);
            using (var invalidPage = await client.GetAsync("/api/workflow/tasks?pageSize=101"))
                Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
            using (var invalidSort = await client.GetAsync("/api/workflow/tasks?sortBy=invalid"))
                Assert.Equal(HttpStatusCode.BadRequest, invalidSort.StatusCode);
            SetSubject(client, "first");
            using (var revoke = await client.DeleteAsync($"/api/identity/users/{member.UserId}/roles/Reader"))
                Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
            using (var removeLastAdmin = await client.DeleteAsync($"/api/identity/users/{admin.UserId}/roles/Administrator"))
                Assert.Equal(HttpStatusCode.Conflict, removeLastAdmin.StatusCode);
            SetSubject(client, "second");
            var revokedAccess = await client.GetFromJsonAsync<RepositoryUserAccess>("/api/auth/repository/me");
            Assert.Empty(revokedAccess!.Roles);
            var revokedInbox = await client.GetFromJsonAsync<UnifiedInbox>("/api/workflow/tasks/inbox");
            Assert.Equal(0, revokedInbox!.Counts.TotalPending);
            Assert.Empty(revokedInbox.CriticalTasks);
            SetSubject(client, "first");
            using (var staleUpdate = await client.PutAsJsonAsync($"/api/identity/users/{member.UserId}",
                new RepositoryUserUpdate(memberSummary.FullName, false, memberSummary.ConcurrencyStamp)))
                Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
            memberSummary = (await client.GetFromJsonAsync<RepositoryUserSummary>($"/api/identity/users/{member.UserId}"))!;
            Assert.True(memberSummary.IsActive);
            using (var deactivated = await client.PutAsJsonAsync($"/api/identity/users/{member.UserId}",
                new RepositoryUserUpdate(memberSummary.FullName, false, memberSummary.ConcurrencyStamp)))
                Assert.Equal(HttpStatusCode.OK, deactivated.StatusCode);
            var administrator = accounts.Single(x => x.UserId == admin.UserId);
            using (var lastAdmin = await client.PutAsJsonAsync($"/api/identity/users/{admin.UserId}",
                new RepositoryUserUpdate(administrator.FullName, false, administrator.ConcurrencyStamp)))
                Assert.Equal(HttpStatusCode.Conflict, lastAdmin.StatusCode);
            SetSubject(client, "second");
            using (var disabled = await client.GetAsync("/api/auth/repository/me"))
                Assert.Equal(HttpStatusCode.Forbidden, disabled.StatusCode);
            using (var disabledInbox = await client.GetAsync("/api/workflow/tasks/inbox"))
                Assert.Equal(HttpStatusCode.Forbidden, disabledInbox.StatusCode);
            taskRouter.Verify(x => x.GetTasksForPersonaAsync("ENGINEER"), Times.Exactly(3));
            taskRouter.VerifyNoOtherCalls();

            await using var verify = app.Services.CreateAsyncScope();
            var db = verify.ServiceProvider.GetRequiredService<RepositoryDbContext>();
            Assert.Equal(2, await db.Users.CountAsync());
            Assert.Equal(2, await db.UserLogins.CountAsync());
            Assert.Equal(1, await db.UserRoles.CountAsync());
            Assert.Equal(admin.UserId, (await db.Bootstrap.SingleAsync()).AdministratorUserId);
            Assert.True((await db.Users.SingleAsync(x => x.Id == admin.UserId)).EmailConfirmed);
            Assert.False((await db.Users.SingleAsync(x => x.Id == member.UserId)).EmailConfirmed);
        }
        finally { await app.StopAsync(); }
    }

    /// <summary>
    /// Signs <paramref name="subject"/> in with a token that also claims the Administrator role and another account — both
    /// ignored. Only "first" has an address the identity server verified.
    /// </summary>
    private static void SetSubject(HttpClient client, string subject) =>
        client.SignIn(SignedTokens.Person(subject, "shared@example.invalid", emailVerified: subject == "first",
            name: $"External {subject}", extra: [("role", "Administrator"), (ClaimTypes.NameIdentifier, "forged-local-id")]));

    private sealed class AccountControllers : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var allowed = new[] { typeof(RepositoryAccountController),
                typeof(UserManagementController), typeof(RepositoryRolesController), typeof(PersonasController),
                typeof(WorkflowTasksController), typeof(Beep.OilandGas.ApiService.Controllers.AccessControl.AccessControlController),
                typeof(Beep.OilandGas.ApiService.Controllers.AccessControl.AssetHierarchyController) };
            foreach (var controller in feature.Controllers.Where(x => !allowed.Contains(x.AsType())).ToArray())
                feature.Controllers.Remove(controller);
        }
    }
}
