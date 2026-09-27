using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// S3-06. Who an API request is from, end to end: a signed token validated for this API's audience, the person resolved to
/// their OilGas account by the identity server's client library over the repository (<see cref="RepositoryCanonicalUserStore"/>),
/// the repository's roles on the request, and <c>/api/auth/repository/me</c>'s answer — on a repository database.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> SQLite stands in for the repository's providers (<c>LocalDbInstallationTests</c> and
/// <c>RepositoryRegistrationHttpTests</c> drive SQL Server); the issuer is a test key (<see cref="SignedTokens"/>).
/// </remarks>
public sealed class ApiIdentityPipelineTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private WebApplication _app = null!;
    private HttpClient _http = null!;

    public async Task InitializeAsync()
    {
        _connection.Open();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = "SqlServer",
            ["Repository:ConnectionString"] = "Server=replaced-by-the-test;Database=unused"
        });
        builder.Services.AddOilGasRepository(builder.Configuration);
        builder.Services.RemoveAll<RepositoryDbContext>();
        builder.Services.AddDbContext<SqliteRepository>(options => options.UseSqlite(_connection));
        builder.Services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<SqliteRepository>());
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<RepositoryRoleAssignmentService>();
        builder.Services.AddScoped<RepositoryUserService>();
        builder.AddApiIdentity();
        builder.Services.AddControllers().AddApplicationPart(typeof(RepositoryAccountController).Assembly)
            .ConfigureApplicationPartManager(parts => parts.FeatureProviders.Add(new OnlyAccountController()));

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
        // Any business endpoint: no attribute, so the API's fallback policy (an active account) decides.
        _app.MapGet("/api/probe", (ClaimsPrincipal user) => Results.Json(new Probe(
            PartyIdClaims.Find(user), user.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray(),
            user.FindAll(PartyIdClaimsTransformation<string>.ClaimType).Count())));

        await using (var scope = _app.Services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<RepositoryDbContext>().Database.EnsureCreatedAsync();
        await _app.StartAsync();
        _http = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    public async Task DisposeAsync()
    {
        _http.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
        _connection.Dispose();
    }

    [Fact]
    public async Task The_first_person_to_sign_in_administers_and_the_next_does_not()
    {
        var first = await MeAsync(SignedTokens.Person("first", "first@example.test", name: "First Person"));
        var second = await MeAsync(SignedTokens.Person("second", "second@example.test"));

        Assert.Equal(["Administrator"], first.Roles);
        Assert.Empty(second.Roles);
        Assert.NotEqual(first.UserId, second.UserId);
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        Assert.Equal(first.UserId, (await db.Bootstrap.SingleAsync()).AdministratorUserId);
        Assert.Equal("First Person", (await db.Set<AppUserExtension>().SingleAsync(row => row.UserId == first.UserId)).FullName);
    }

    [Fact]
    public async Task Roles_and_an_account_a_token_names_count_for_nothing()
    {
        var admin = await MeAsync(SignedTokens.Person("first", "first@example.test"));

        _http.SignIn(SignedTokens.Person("second", "second@example.test", extra:
        [
            ("role", "Administrator"),
            (ClaimTypes.Role, "Administrator"),
            ("party_id", admin.UserId),
            (ClaimTypes.NameIdentifier, admin.UserId),
            (RepositoryRolesClaimsTransformation.ActiveAccount, "true"),
            (RepositoryRolesClaimsTransformation.ResolvedMarker, "true")
        ]));
        var probe = (await _http.GetFromJsonAsync<Probe>("/api/probe"))!;

        Assert.NotEqual(admin.UserId, probe.PartyId);
        Assert.Equal(1, probe.PartyIdClaims);
        Assert.Empty(probe.Roles);
    }

    [Fact]
    public async Task A_re_minted_subject_with_the_verified_address_of_one_account_is_that_account()
    {
        var before = await MeAsync(SignedTokens.Person("first", "first@example.test"));

        var after = await MeAsync(SignedTokens.Person("first-reseeded", "first@example.test"));

        Assert.Equal(before.UserId, after.UserId);
        Assert.Equal(["Administrator"], after.Roles);
        await using var scope = _app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        var login = await db.UserLogins.SingleAsync(row => row.UserId == before.UserId);
        Assert.Equal("first-reseeded", login.ProviderKey);
    }

    [Fact]
    public async Task An_address_the_issuer_did_not_verify_is_a_different_person()
    {
        var owner = await MeAsync(SignedTokens.Person("first", "first@example.test"));

        var stranger = await MeAsync(SignedTokens.Person("stranger", "first@example.test", emailVerified: false));

        Assert.NotEqual(owner.UserId, stranger.UserId);
        Assert.Empty(stranger.Roles);
    }

    [Fact]
    public async Task An_application_acting_for_itself_has_no_account_and_is_not_provisioned()
    {
        await MeAsync(SignedTokens.Person("first", "first@example.test"));
        _http.SignIn(SignedTokens.Machine("reporting-job"));

        using var me = await _http.GetAsync("/api/auth/repository/me");
        Assert.Equal(HttpStatusCode.Forbidden, me.StatusCode);
        Assert.Equal(RepositoryAccountRefusals.NotAPerson, (await me.Content.ReadFromJsonAsync<ProblemDetails>())!.Title);
        using var probe = await _http.GetAsync("/api/probe");
        Assert.Equal(HttpStatusCode.Forbidden, probe.StatusCode);

        await using var scope = _app.Services.CreateAsyncScope();
        Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<RepositoryDbContext>().Users.CountAsync());
    }

    [Fact]
    public async Task A_token_for_another_API_or_none_at_all_is_not_signed_in()
    {
        _http.SignIn(SignedTokens.Person("first", "first@example.test", audience: "https://another-api.test/api"));
        using var foreign = await _http.GetAsync("/api/auth/repository/me");
        Assert.Equal(HttpStatusCode.Unauthorized, foreign.StatusCode);

        _http.SignIn(null);
        using var anonymous = await _http.GetAsync("/api/probe");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        await using var scope = _app.Services.CreateAsyncScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<RepositoryDbContext>().Users.CountAsync());
    }

    [Fact]
    public async Task An_account_OilGas_switched_off_is_told_so_and_admitted_nowhere()
    {
        await MeAsync(SignedTokens.Person("first", "first@example.test"));
        var member = await MeAsync(SignedTokens.Person("second", "second@example.test"));
        await using (var scope = _app.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
            var account = (await users.FindByIdAsync(member.UserId))!;
            account.IsActive = false;
            Assert.True((await users.UpdateAsync(account)).Succeeded);
        }

        using var me = await _http.GetAsync("/api/auth/repository/me");
        Assert.Equal(HttpStatusCode.Forbidden, me.StatusCode);
        Assert.Equal(RepositoryAccountRefusals.Deactivated, (await me.Content.ReadFromJsonAsync<ProblemDetails>())!.Title);
        using var probe = await _http.GetAsync("/api/probe");
        Assert.Equal(HttpStatusCode.Forbidden, probe.StatusCode);
    }

    [Fact]
    public async Task A_person_switches_their_own_account_off_but_not_the_last_administrator_s()
    {
        var adminToken = SignedTokens.Person("first", "first@example.test");
        await MeAsync(adminToken);
        var member = await MeAsync(SignedTokens.Person("second", "second@example.test"));

        using (var off = await _http.PostAsync("/api/auth/repository/me/deactivate", null))
            Assert.Equal(HttpStatusCode.NoContent, off.StatusCode);
        using (var me = await _http.GetAsync("/api/auth/repository/me"))
            Assert.Equal(RepositoryAccountRefusals.Deactivated, (await me.Content.ReadFromJsonAsync<ProblemDetails>())!.Title);

        _http.SignIn(adminToken);
        using (var lastAdministrator = await _http.PostAsync("/api/auth/repository/me/deactivate", null))
            Assert.Equal(HttpStatusCode.Conflict, lastAdministrator.StatusCode);
        Assert.Equal(["Administrator"], (await _http.GetFromJsonAsync<RepositoryUserAccess>("/api/auth/repository/me"))!.Roles);

        await using var scope = _app.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
        Assert.False((await users.FindByIdAsync(member.UserId))!.IsActive);
    }

    /// <summary>
    /// Asked before the identity server deletes anything: the last active administrator is told why not, anybody else
    /// may — the rule <c>me/deactivate</c> applies afterwards, answered while it can still stop the deletion.
    /// </summary>
    [Fact]
    public async Task The_last_administrator_is_told_before_deleting_and_anybody_else_may_delete()
    {
        var adminToken = SignedTokens.Person("first", "first@example.test");
        await MeAsync(adminToken);
        var memberToken = SignedTokens.Person("second", "second@example.test");
        await MeAsync(memberToken);

        _http.SignIn(adminToken);
        var admin = await _http.GetFromJsonAsync<RepositoryAccountDeletion>("/api/auth/repository/me/deletion");
        Assert.Equal(Beep.OilandGas.ApiService.Controllers.RepositoryAccountController.LastAdministratorMayNotDelete, admin!.Refusal);

        _http.SignIn(memberToken);
        var member = await _http.GetFromJsonAsync<RepositoryAccountDeletion>("/api/auth/repository/me/deletion");
        Assert.Null(member!.Refusal);
    }

    private async Task<RepositoryUserAccess> MeAsync(string token)
    {
        _http.SignIn(token);
        using var response = await _http.GetAsync("/api/auth/repository/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<RepositoryUserAccess>())!;
    }

    private sealed record Probe(string? PartyId, string[] Roles, int PartyIdClaims);

    private sealed class SqliteRepository(DbContextOptions<SqliteRepository> options) : RepositoryDbContext(options);

    private sealed class OnlyAccountController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in feature.Controllers.Where(x => x.AsType() != typeof(RepositoryAccountController)).ToArray())
                feature.Controllers.Remove(controller);
        }
    }
}
