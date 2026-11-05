using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyClientScopeTests
{
    private static readonly CompanyScope A = new("tenant-a", "company-a");
    private static readonly CompanyScope B = new("tenant-b", "company-b");
    private static RepositoryCompanyAccess Access(CompanyScope scope, params string[] roles) => new(scope, roles, ["study.read"]);
    private static void Select(CompanyClientContext context, CompanyScope scope, string user = "alice", params string[] roles) =>
        Assert.True(context.TrySelect(context.Clear(), user, Access(scope, roles)));
    private static string? Account(ClaimsPrincipal principal) => principal.FindFirst("resolved-account")?.Value;

    [Fact]
    public async Task Independent_sessions_send_only_their_own_scope_without_shared_default_headers()
    {
        using var a = new CompanyClientContext(); using var b = new CompanyClientContext();
        Select(a, A); Select(b, B);
        var seen = new List<CompanyScope>();
        using var http = new HttpClient(new Handler((request, _) =>
        {
            seen.Add(new(request.Headers.GetValues(CompanyRequestHeaders.Tenant).Single(),
                request.Headers.GetValues(CompanyRequestHeaders.Company).Single()));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }));
        using var first = new HttpRequestMessage(HttpMethod.Post, "https://api.test/api/studies");
        using var second = new HttpRequestMessage(HttpMethod.Get, "https://api.test/api/studies");
        using var one = await a.SendAsync(http, first); using var two = await b.SendAsync(http, second);
        Assert.Equal(new[] { A, B }, seen);
        Assert.Empty(http.DefaultRequestHeaders);
    }

    [Fact]
    public async Task Unselected_business_request_never_reaches_transport()
    {
        using var context = new CompanyClientContext();
        var calls = 0;
        using var http = new HttpClient(new Handler((_, _) => { calls++; return Task.FromResult(new HttpResponseMessage()); }));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/api/field/current");
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SendAsync(http, request));
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("/api/auth/repository/me/companies")]
    [InlineData("/api/identity/roles")]
    [InlineData("/api/setup/modules?tenantId=tenant-a")]
    [InlineData("/api/ppdm39/setup/schema/execute?tenantId=tenant-a")]
    public async Task Account_and_control_plane_calls_require_no_business_selection(string path)
    {
        using var context = new CompanyClientContext();
        using var http = new HttpClient(new Handler((request, _) =>
        {
            Assert.False(request.Headers.Contains(CompanyRequestHeaders.Tenant));
            Assert.False(request.Headers.Contains(CompanyRequestHeaders.Company));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        })) { BaseAddress = new("https://api.test") };
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add(CompanyRequestHeaders.Tenant, "forged");
        using var response = await context.SendAsync(http, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/setup-other")]
    [InlineData("/api/identity-other")]
    [InlineData("/hubs/workflow-notifications/negotiate")]
    [InlineData("/progressHub")]
    public void Similar_prefixes_and_hubs_require_selection(string path)
    {
        using var context = new CompanyClientContext();
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        Assert.Throws<InvalidOperationException>(() => context.Prepare(request));
    }

    [Fact]
    public async Task Scope_captured_before_token_lookup_cannot_adopt_a_new_company()
    {
        using var context = new CompanyClientContext(); Select(context, A);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/api/studies");
        context.Prepare(request);
        Select(context, B);
        using var http = new HttpClient(new Handler((_, _) => throw new InvalidOperationException("Transport must not run.")));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.SendAsync(http, request));
        Assert.Equal("tenant-a", request.Headers.GetValues(CompanyRequestHeaders.Tenant).Single());
    }

    [Fact]
    public async Task Company_change_cancels_inflight_work_and_refuses_a_late_response_even_if_transport_ignores_cancellation()
    {
        using var context = new CompanyClientContext(); Select(context, A);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var complete = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = false;
        using var http = new HttpClient(new Handler(async (_, token) =>
        {
            using var registration = token.Register(() => cancelled = true);
            entered.SetResult();
            return await complete.Task;
        }));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/api/studies");
        var pending = context.SendAsync(http, request);
        await entered.Task;
        Select(context, B);
        Assert.True(cancelled);
        complete.SetResult(new(HttpStatusCode.OK));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Fact]
    public async Task Response_and_download_become_unreadable_after_company_switch()
    {
        using var context = new CompanyClientContext(); Select(context, A);
        using var http = new HttpClient(new Handler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("private") })));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/api/export");
        var response = await context.SendAsync(http, request);
        using var stream = new CompanyResponseStream(await response.Content.ReadAsStreamAsync(), response, context);
        Select(context, B);
        Assert.ThrowsAny<OperationCanceledException>(() => context.EnsureCurrent(response));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => { var read = await stream.ReadAsync(new byte[20]); Assert.Equal(0, read); });
    }

    [Fact]
    public void Selection_is_immutable_and_a_stale_commit_cannot_restore_previous_access()
    {
        using var context = new CompanyClientContext();
        var roles = new[] { "Accountant" };
        var version = context.Clear();
        Assert.True(context.TrySelect(version, "alice", new(A, roles, ["read"])));
        roles[0] = "Administrator";
        Assert.Equal("Accountant", context.Selection!.Roles.Single());
        context.Clear();
        Assert.False(context.TrySelect(version, "alice", Access(A, "Administrator")));
        Assert.Null(context.Selection);
    }

    [Fact]
    public async Task Request_prepared_in_another_session_is_refused()
    {
        using var a = new CompanyClientContext(); using var b = new CompanyClientContext(); Select(a, A); Select(b, B);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/api/studies"); a.Prepare(request);
        using var http = new HttpClient(new Handler((_, _) => throw new InvalidOperationException()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => b.SendAsync(http, request));
    }

    [Fact]
    public async Task Selection_gateway_requires_an_exact_response_and_leaves_old_data_cleared_on_failure()
    {
        using var context = new CompanyClientContext(); Select(context, A);
        var api = new AccessClient { Resolve = _ => Task.FromResult(Access(A)) };
        var select = new CompanySelectionService(api, new AccountProvider("alice"), context, Account);
        await Assert.ThrowsAsync<InvalidOperationException>(() => select.SelectAsync(B));
        Assert.Null(context.Selection);
        api.Resolve = _ => throw new HttpRequestException("Unavailable");
        await Assert.ThrowsAsync<HttpRequestException>(() => select.SelectAsync(A));
        Assert.Null(context.Selection);
    }

    [Fact]
    public async Task Concurrent_selections_accept_only_the_latest_gateway_answer()
    {
        using var context = new CompanyClientContext();
        var late = new TaskCompletionSource<RepositoryCompanyAccess>(TaskCreationOptions.RunContinuationsAsynchronously);
        var api = new AccessClient { Resolve = scope => scope == A ? late.Task : Task.FromResult(Access(B, "Engineer")) };
        var select = new CompanySelectionService(api, new AccountProvider("alice"), context, Account);
        var old = select.SelectAsync(A);
        await select.SelectAsync(B);
        late.SetResult(Access(A, "Administrator"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => old);
        Assert.Equal(B, context.Selection!.Scope);
    }

    [Fact]
    public async Task Account_change_during_gateway_lookup_cannot_install_company_roles()
    {
        using var context = new CompanyClientContext();
        var account = new AccountProvider("alice");
        var late = new TaskCompletionSource<RepositoryCompanyAccess>(TaskCreationOptions.RunContinuationsAsynchronously);
        var select = new CompanySelectionService(new AccessClient { Resolve = _ => late.Task }, account, context, Account);
        var pending = select.SelectAsync(A);
        account.Change("bob"); late.SetResult(Access(A, "Administrator"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Null(context.Selection);
    }

    [Fact]
    public async Task Blazor_uses_standard_company_roles_and_preserves_independent_repository_roles()
    {
        using var company = new CompanyClientContext();
        var navigation = new TestNavigation("/production");
        using var provider = new CompanyAuthenticationStateProvider(new AccountProvider("alice", "Administrator"), company, navigation, Account);
        Assert.False((await provider.GetAuthenticationStateAsync()).User.IsInRole("Administrator"));
        Select(company, A, "alice", "Accountant");
        var user = (await provider.GetAuthenticationStateAsync()).User;
        Assert.True(user.IsInRole("Accountant")); Assert.False(user.IsInRole("Administrator"));
        using var services = new ServiceCollection().AddLogging().AddAuthorizationCore().BuildServiceProvider();
        Assert.True((await services.GetRequiredService<IAuthorizationService>().AuthorizeAsync(user, null,
            new AuthorizationPolicyBuilder().RequireRole("Accountant").Build())).Succeeded);
        navigation.Go("/admin/module-databases");
        user = (await provider.GetAuthenticationStateAsync()).User;
        Assert.True(user.IsInRole("Administrator")); Assert.False(user.IsInRole("Accountant"));
        navigation.Go("/production");
        Assert.True((await provider.GetAuthenticationStateAsync()).User.IsInRole("Accountant"));
    }

    [Fact]
    public async Task Company_administrator_does_not_gain_repository_administration_and_account_change_clears_scope()
    {
        using var company = new CompanyClientContext();
        var account = new AccountProvider("alice"); var navigation = new TestNavigation("/production");
        using var provider = new CompanyAuthenticationStateProvider(account, company, navigation, Account);
        Select(company, A, "alice", "Administrator");
        Assert.True((await provider.GetAuthenticationStateAsync()).User.IsInRole("Administrator"));
        navigation.Go("/admin/module-databases");
        Assert.False((await provider.GetAuthenticationStateAsync()).User.IsInRole("Administrator"));
        account.Change("bob"); Assert.Null(company.Selection);
        Assert.Equal("bob", Account((await provider.GetAuthenticationStateAsync()).User));
    }

    [Fact]
    public async Task Roles_and_permissions_are_removed_from_every_identity_when_no_company_is_available()
    {
        using var context = new CompanyClientContext();
        var account = new AccountProvider("alice", "Administrator");
        account.User.AddIdentity(new ClaimsIdentity([new Claim("custom-role", "Forged"), new Claim("permission", "all")], "other", "name", "custom-role"));
        using var provider = new CompanyAuthenticationStateProvider(account, context, new TestNavigation("/production"), Account);
        var user = (await provider.GetAuthenticationStateAsync()).User;
        Assert.DoesNotContain(user.Claims, x => x.Type == "permission" || x.Type == "custom-role" || x.Type == ClaimTypes.Role);
        Assert.False(user.IsInRole("Forged"));
    }

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request, cancellationToken); }

    [Fact]
    public async Task A_late_original_authentication_state_cannot_replace_the_current_account()
    {
        using var context = new CompanyClientContext();
        var late = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var provider = new CompanyAuthenticationStateProvider(new DeferredAccount(late.Task), context,
            new TestNavigation("/admin/module-databases"), Account);
        var pending = provider.GetAuthenticationStateAsync();
        provider.SetAuthenticationState(new AccountProvider("bob").GetAuthenticationStateAsync());
        late.SetResult(await new AccountProvider("alice", "Administrator").GetAuthenticationStateAsync());
        var result = await pending;
        Assert.Equal("bob", Account(result.User));
        Assert.False(result.User.IsInRole("Administrator"));
    }

    private sealed class DeferredAccount(Task<AuthenticationState> state) : AuthenticationStateProvider
    { public override Task<AuthenticationState> GetAuthenticationStateAsync() => state; }
    private sealed class AccessClient : ICompanyAccessClient
    {
        public Func<CompanyScope, Task<RepositoryCompanyAccess>> Resolve { get; set; } = scope => Task.FromResult(Access(scope));
        public Task<RepositoryCompanyAccess> GetAccessAsync(CompanyScope scope, CancellationToken token = default) => Resolve(scope);
        public Task<CompanyScope[]> ListAsync(CancellationToken token = default) => Task.FromResult(new[] { A, B });
    }
    private sealed class AccountProvider : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider
    {
        public ClaimsPrincipal User { get; private set; }
        public AccountProvider(string account, params string[] roles) => User = Principal(account, roles);
        private static ClaimsPrincipal Principal(string account, string[] roles) => new(new ClaimsIdentity(
            new[] { new Claim("resolved-account", account) }.Concat(roles.Select(x => new Claim(ClaimTypes.Role, x))), "test"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(User));
        public void Change(string account) { User = Principal(account, []); NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
        public void SetAuthenticationState(Task<AuthenticationState> state) => NotifyAuthenticationStateChanged(state);
    }
    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation(string path) => Initialize("https://web.test/", "https://web.test" + path);
        public void Go(string path) { Uri = ToAbsoluteUri(path).ToString(); NotifyLocationChanged(false); }
        protected override void NavigateToCore(string uri, bool forceLoad) => Go(uri);
    }
}
