using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.Web.Services;
using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>
/// The Web's view of a person's OilGas account, asked of the API with that person's own access token: the roles a request
/// carries (<see cref="OilGasClaimsTransformation"/>), whether the account is still admitted (<see cref="OilGasAccountAdmission"/>),
/// and switching it off after the identity was deleted.
/// </summary>
public sealed class RepositoryAccountTests
{
    private const string IdentityServerIssuer = "https://identity.example/";

    // ---------------------------------------------------------------- roles on a request

    [Fact]
    public async Task A_request_carries_the_account_and_roles_the_API_reports_and_nothing_a_token_carried()
    {
        var api = new Api(HttpStatusCode.OK, new RepositoryUserAccess("local-user", true, ["Viewer"], ["Wells.Read"]));
        var result = await Transformation(api).TransformAsync(FromToken("subject"));

        Assert.True(result.IsInRole("Viewer"));
        Assert.False(result.IsInRole("Administrator"));
        Assert.Equal(["Wells.Read"], result.FindAll("permission").Select(claim => claim.Value));
        Assert.Null(result.FindFirst("permissions"));
        Assert.Null(result.FindFirst("elevated_permissions"));
        Assert.Null(result.FindFirst(ClaimTypes.NameIdentifier));
        Assert.Equal("local-user", PartyIdClaims.Find(result));
        Assert.Single(result.FindAll(PartyIdClaimsTransformation<string>.ClaimType));
        Assert.True(result.Identity!.IsAuthenticated);

        Assert.Equal("Bearer token-for-subject", api.Authorization);
        Assert.Equal("/api/auth/repository/me", api.Path);
    }

    [Fact]
    public async Task Roles_are_asked_for_once_per_request_and_a_marker_a_token_carried_does_not_skip_the_question()
    {
        var api = new Api(HttpStatusCode.OK, new RepositoryUserAccess("local-user", true, ["Viewer"], []));
        var transformation = Transformation(api);

        var first = await transformation.TransformAsync(FromToken("subject"));
        Assert.Equal(1, api.Calls);
        Assert.Same(first, await transformation.TransformAsync(first));
        Assert.Equal(1, api.Calls);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, RepositoryAccountRefusals.Deactivated)]
    [InlineData(HttpStatusCode.Forbidden, RepositoryAccountRefusals.Removed)]
    [InlineData(HttpStatusCode.Forbidden, RepositoryAccountRefusals.NotAPerson)]
    [InlineData(HttpStatusCode.Forbidden, "some_other_refusal")]
    [InlineData(HttpStatusCode.ServiceUnavailable, RepositoryAccountRefusals.Unresolved)]
    [InlineData(HttpStatusCode.NotFound, null)]
    [InlineData(HttpStatusCode.InternalServerError, null)]
    public async Task An_account_the_API_refuses_or_could_not_answer_about_holds_no_role_and_no_key(HttpStatusCode status, string? title)
    {
        var api = new Api(status, title is null ? null : new ProblemDetails { Title = title, Status = (int)status });
        var result = await Transformation(api).TransformAsync(FromToken("subject"));

        Assert.False(result.IsInRole("Administrator"));
        Assert.Empty(result.FindAll(ClaimTypes.Role));
        Assert.Empty(result.FindAll("permission"));
        Assert.Null(PartyIdClaims.Find(result));
        Assert.Null(result.FindFirst(PartyIdClaimsTransformation<string>.ClaimType));
    }

    [Fact]
    public async Task Without_the_person_s_token_the_API_is_not_asked_and_nothing_is_granted()
    {
        var api = new Api(HttpStatusCode.OK, new RepositoryUserAccess("local-user", true, ["Administrator"], []));
        var result = await Transformation(api, new PersonTokens(except: "subject")).TransformAsync(FromToken("subject"));

        Assert.Equal(0, api.Calls);
        Assert.False(result.IsInRole("Administrator"));
        Assert.Null(PartyIdClaims.Find(result));
    }

    // ---------------------------------------------------------------- admission

    [Theory]
    [InlineData(HttpStatusCode.OK, null, AccountAdmission.Admitted)]
    [InlineData(HttpStatusCode.Forbidden, RepositoryAccountRefusals.Deactivated, AccountAdmission.Refused)]
    [InlineData(HttpStatusCode.Forbidden, RepositoryAccountRefusals.Removed, AccountAdmission.Refused)]
    [InlineData(HttpStatusCode.Forbidden, RepositoryAccountRefusals.NotAPerson, AccountAdmission.Refused)]
    [InlineData(HttpStatusCode.Forbidden, "some_other_refusal", AccountAdmission.Unknown)]
    [InlineData(HttpStatusCode.Forbidden, null, AccountAdmission.Unknown)]
    [InlineData(HttpStatusCode.ServiceUnavailable, RepositoryAccountRefusals.Unresolved, AccountAdmission.Unknown)]
    [InlineData(HttpStatusCode.Unauthorized, null, AccountAdmission.Unknown)]
    [InlineData(HttpStatusCode.BadGateway, null, AccountAdmission.Unknown)]
    public async Task OilGas_admits_an_active_account_refuses_one_it_turned_away_and_asks_again_after_an_outage(
        HttpStatusCode status, string? title, AccountAdmission expected)
    {
        var api = status == HttpStatusCode.OK
            ? new Api(status, new RepositoryUserAccess("local-user", true, ["Viewer"], []))
            : new Api(status, title is null ? null : new ProblemDetails { Title = title, Status = (int)status });

        Assert.Equal(expected, await new OilGasAccountAdmission(Client(api)).EvaluateAsync(FromToken("subject"), default));
    }

    [Fact]
    public async Task An_API_that_cannot_be_reached_is_an_outage_not_a_refusal()
    {
        var api = new Api(HttpStatusCode.OK, null) { Unreachable = true };
        var reporter = new RecordingFailureReporter();
        Assert.Equal(AccountAdmission.Unknown,
            await new OilGasAccountAdmission(Client(api, reporter: reporter)).EvaluateAsync(FromToken("subject"), default));
        // The outage is in the store: it was only logged, so nobody could tell how often admission went unanswered.
        Assert.IsType<HttpRequestException>(Assert.Single(reporter.Reports).Exception);
    }

    [Fact]
    public async Task No_token_for_the_person_is_an_outage_not_a_refusal()
    {
        var api = new Api(HttpStatusCode.OK, new RepositoryUserAccess("local-user", true, [], []));
        Assert.Equal(AccountAdmission.Unknown,
            await new OilGasAccountAdmission(Client(api, new PersonTokens(except: "subject"))).EvaluateAsync(FromToken("subject"), default));
        Assert.Equal(0, api.Calls);
    }

    // ---------------------------------------------------------------- switching the account off

    [Fact]
    public async Task Switching_the_account_off_is_the_person_s_own_request()
    {
        var api = new Api(HttpStatusCode.NoContent, null);
        await Client(api).DeactivateAsync(FromToken("subject"));

        Assert.Equal(HttpMethod.Post, api.Method);
        Assert.Equal("/api/auth/repository/me/deactivate", api.Path);
        Assert.Equal("Bearer token-for-subject", api.Authorization);
    }

    [Fact]
    public async Task A_refused_switch_off_says_why()
    {
        var api = new Api(HttpStatusCode.Conflict, new ProblemDetails { Status = 409, Detail = "the last active administrator" });
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => Client(api).DeactivateAsync(FromToken("subject")));
        Assert.Contains("409", failure.Message);
        Assert.Contains("the last active administrator", failure.Message);
    }

    [Fact]
    public async Task Without_the_person_s_token_nothing_is_switched_off_and_the_caller_is_told()
    {
        var api = new Api(HttpStatusCode.NoContent, null);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Client(api, new PersonTokens(except: "subject")).DeactivateAsync(FromToken("subject")));
        Assert.Equal(0, api.Calls);
    }

    // ---------------------------------------------------------------- asked before the identity is deleted

    [Fact]
    public async Task The_last_administrator_s_refusal_comes_back_in_the_API_s_words()
    {
        var api = new Api(HttpStatusCode.OK, new RepositoryAccountDeletion("You are OilGas's only active administrator."));

        Assert.Equal("You are OilGas's only active administrator.", await Client(api).RefusalOfDeletionAsync(FromToken("subject")));
        Assert.Equal(HttpMethod.Get, api.Method);
        Assert.Equal("/api/auth/repository/me/deletion", api.Path);
        Assert.Equal("Bearer token-for-subject", api.Authorization);
    }

    [Fact]
    public async Task Anybody_else_may_delete()
    {
        var api = new Api(HttpStatusCode.OK, new RepositoryAccountDeletion(null));

        Assert.Null(await Client(api).RefusalOfDeletionAsync(FromToken("subject")));
    }

    /// <summary>An unanswered question is not permission: the shared component deletes nothing when this throws.</summary>
    [Fact]
    public async Task An_API_that_cannot_answer_is_not_permission()
    {
        var api = new Api(HttpStatusCode.ServiceUnavailable, new ProblemDetails { Status = 503, Detail = "the repository is down" });

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => Client(api).RefusalOfDeletionAsync(FromToken("subject")));
        Assert.Contains("503", failure.Message);
    }

    [Fact]
    public void The_account_page_asks_before_deleting()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.Web")))
            root = root.Parent;
        Assert.NotNull(root);

        var page = File.ReadAllText(Path.Combine(root.FullName, "Beep.OilandGas.Web", "Components", "Pages", "Account", "Manage.razor"));
        Assert.Contains("RefuseDeletion=\"@Deactivation.RefuseDeletionAsync\"", page, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- fixtures

    /// <summary>
    /// A person as the identity server's token describes them, carrying everything that must count for nothing here:
    /// its roles and permissions, a NameIdentifier, and a <c>party_id</c> and resolution marker it issued itself.
    /// </summary>
    private static ClaimsPrincipal FromToken(string subject) => new(new ClaimsIdentity(
    [
        new Claim("sub", subject, ClaimValueTypes.String, IdentityServerIssuer),
        new Claim("role", "Administrator", ClaimValueTypes.String, IdentityServerIssuer),
        new Claim(ClaimTypes.Role, "Administrator", ClaimValueTypes.String, IdentityServerIssuer),
        new Claim(ClaimTypes.NameIdentifier, "someone-else", ClaimValueTypes.String, IdentityServerIssuer),
        new Claim("permissions", "Admin.ManageUsers", ClaimValueTypes.String, IdentityServerIssuer),
        new Claim("elevated_permissions", "Admin.AssignRoles", ClaimValueTypes.String, IdentityServerIssuer),
        new Claim(PartyIdClaimsTransformation<string>.ClaimType, "someone-else", ClaimValueTypes.String, IdentityServerIssuer),
        new Claim(OilGasClaimsTransformation.ResolvedMarker, "true", ClaimValueTypes.String, IdentityServerIssuer)
    ], "Cookies", "name", ClaimTypes.Role));

    private static OilGasClaimsTransformation Transformation(Api api, PersonTokens? tokens = null) => new(Client(api, tokens));

    private static RepositoryAccountClient Client(Api api, PersonTokens? tokens = null, RecordingFailureReporter? reporter = null) =>
        new(new HttpClient(api) { BaseAddress = new Uri("https://api.example/") }, tokens ?? new PersonTokens(),
            NullLogger<RepositoryAccountClient>.Instance, reporter ?? new RecordingFailureReporter());

    /// <summary>The identity server's client library: each person's own token, or none for <c>except</c>.</summary>
    private sealed class PersonTokens(string? except = null) : IUserTokenManager
    {
        public Task<TokenResult<UserToken>> GetAccessTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null, CancellationToken ct = default)
        {
            var subject = user.FindFirst("sub")?.Value;
            return Task.FromResult<TokenResult<UserToken>>(subject is null || subject == except
                ? new FailedResult("session_tokens_missing", "no tokens for this session")
                : new UserToken
                {
                    AccessToken = AccessToken.Parse("token-for-" + subject),
                    AccessTokenType = null,
                    ClientId = ClientId.Parse("oilgas-web"),
                    Expiration = DateTimeOffset.UtcNow.AddHours(1)
                });
        }

        public Task RevokeRefreshTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    /// <summary>The OilGas API, answering every request with one status and body.</summary>
    private sealed class Api(HttpStatusCode status, object? body) : HttpMessageHandler
    {
        public bool Unreachable { get; init; }
        public int Calls { get; private set; }
        public string? Path { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Authorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Path = request.RequestUri!.AbsolutePath;
            Method = request.Method;
            Authorization = request.Headers.Authorization?.ToString();
            if (Unreachable)
                throw new HttpRequestException("No connection could be made.");
            var response = new HttpResponseMessage(status);
            if (body is ProblemDetails problem)
                response.Content = JsonContent.Create(problem, mediaType: new("application/problem+json"));
            else if (body is not null)
                response.Content = JsonContent.Create(body);
            return Task.FromResult(response);
        }
    }
}
