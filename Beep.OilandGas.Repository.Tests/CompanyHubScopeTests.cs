using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Hubs;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyHubScopeTests
{
    private static readonly CompanyScope A = new("tenant-a", "company-a");
    private static readonly CompanyScope SameTenant = new("tenant-a", "company-b");
    private static readonly CompanyScope B = new("tenant-b", "company-a");
    private static ClaimsPrincipal User(string account) => new(new ClaimsIdentity([new Claim("resolved-account", account)], "test"));
    private static string? Account(ClaimsPrincipal principal) => principal.FindFirst("resolved-account")?.Value;

    [Theory]
    [InlineData(CompanyChannelKind.User)]
    [InlineData(CompanyChannelKind.Persona)]
    [InlineData(CompanyChannelKind.Process)]
    [InlineData(CompanyChannelKind.Operation)]
    [InlineData(CompanyChannelKind.Workflow)]
    public void Every_channel_has_distinct_tenant_company_and_resource_keys(CompanyChannelKind kind)
    {
        var source = new CompanyHubChannel(A, kind, "reused:id", kind == CompanyChannelKind.Persona ? "field:id" : null);
        var first = CompanyHubGroups.Name(source);
        Assert.Equal(source, CompanyHubGroups.Parse(first));
        Assert.NotEqual(first, CompanyHubGroups.Name(source with { Scope = B }));
        Assert.NotEqual(first, CompanyHubGroups.Name(source with { Scope = SameTenant }));
        Assert.DoesNotContain("reused:id", first);
    }

    [Theory]
    [InlineData("user:alice")]
    [InlineData("process:one")]
    [InlineData("persona:ENGINEER:field:one")]
    [InlineData("tenant:a:company:b:user:")]
    [InlineData("tenant:a:company:b:unknown:one")]
    [InlineData("tenant:a:company:b:persona:one")]
    [InlineData("tenant:a:company:b:user:%")]
    [InlineData("tenant:a:company:b:user:%61lice")]
    public void Unscoped_malformed_or_noncanonical_channels_are_refused(string group) => Assert.Null(CompanyHubGroups.Parse(group));

    [Fact]
    public async Task A_delivery_never_uses_the_same_user_or_resource_in_another_company()
    {
        using var f = new Fixture();
        var snapshot = await f.Handshake(A);
        foreach (var scope in new[] { SameTenant, B })
            Assert.False(await f.Gate.CanDeliverAsync(User("alice"), snapshot,
                CompanyHubGroups.Name(new(scope, CompanyChannelKind.User, "alice")), null, (_, _) => throw new InvalidOperationException()));
        Assert.Equal(1, f.Reads); // Handshake only: no cross-company lookup was attempted.
    }

    [Fact]
    public async Task User_delivery_rechecks_membership_and_removal_denies_the_next_delivery()
    {
        using var f = new Fixture(); var snapshot = await f.Handshake(A);
        var group = CompanyHubGroups.Name(new(A, CompanyChannelKind.User, "alice"));
        Assert.True(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, null, (_, _) => throw new InvalidOperationException()));
        f.Available = false;
        Assert.False(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, null, (_, _) => throw new InvalidOperationException()));
        Assert.Equal(3, f.Reads);
    }

    [Fact]
    public async Task Fresh_company_roles_replace_handshake_roles_before_delivery()
    {
        using var f = new Fixture(); f.Roles = ["Engineer", "Administrator"];
        var snapshot = await f.Handshake(A);
        f.Roles = ["Engineer"];
        var group = CompanyHubGroups.Name(new(A, CompanyChannelKind.Persona, "ENGINEER", "field-1"));
        Assert.False(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, "Administrator", (_, _) => throw new InvalidOperationException()));
        var invoked = false;
        Assert.True(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, "Engineer", (channel, current) =>
        {
            invoked = true;
            Assert.Equal(A, channel.Scope); Assert.Equal("field-1", channel.Field);
            Assert.Equal(A, f.Tenant.Company!.Scope); Assert.DoesNotContain("Administrator", current.Roles);
            return Task.FromResult(true);
        }));
        Assert.True(invoked);
    }

    [Fact]
    public async Task Resource_denial_is_not_overridden_by_membership()
    {
        using var f = new Fixture(); var snapshot = await f.Handshake(A);
        Assert.False(await f.Gate.CanDeliverAsync(User("alice"), snapshot,
            CompanyHubGroups.Name(new(A, CompanyChannelKind.Process, "restricted")), null, (_, _) => Task.FromResult(false)));
    }

    [Fact]
    public async Task Role_specific_user_notifications_stop_after_role_revocation_while_generic_membership_messages_remain_possible()
    {
        using var f = new Fixture(); var snapshot = await f.Handshake(A);
        var group = CompanyHubGroups.Name(new(A, CompanyChannelKind.User, "alice"));
        Assert.True(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, "Engineer", (_, _) => Task.FromResult(false)));
        f.Roles = [];
        Assert.False(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, "Engineer", (_, _) => Task.FromResult(true)));
        Assert.True(await f.Gate.CanDeliverAsync(User("alice"), snapshot, group, null, (_, _) => Task.FromResult(false)));
    }

    [Fact]
    public void Fields_cannot_alias_nonpersona_channels()
    {
        Assert.Throws<ArgumentException>(() => CompanyHubGroups.Name(new(A, CompanyChannelKind.User, "alice", "unexpected-field")));
    }

    [Fact]
    public async Task Missing_handshake_wrong_account_and_another_user_channel_are_refused()
    {
        using var f = new Fixture(); var snapshot = await f.Handshake(A);
        Assert.Null(await f.Gate.RestoreAsync(User("alice"), null));
        Assert.Null(await f.Gate.RestoreAsync(User("bob"), snapshot));
        Assert.Null(await f.Gate.RestoreAsync(new ClaimsPrincipal(new ClaimsIdentity()), snapshot));
        Assert.False(await f.Gate.CanDeliverAsync(User("alice"), snapshot,
            CompanyHubGroups.Name(new(A, CompanyChannelKind.User, "bob")), null, (_, _) => Task.FromResult(true)));
    }

    [Fact]
    public async Task Membership_provider_failure_propagates_and_never_reuses_handshake_grants()
    {
        using var f = new Fixture(); var snapshot = await f.Handshake(A);
        f.Access.Setup(x => x.GetCompanyAccessAsync("alice", A, It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Unavailable"));
        await Assert.ThrowsAsync<HttpRequestException>(() => f.Gate.CanDeliverAsync(User("alice"), snapshot,
            CompanyHubGroups.Name(new(A, CompanyChannelKind.User, "alice")), null, (_, _) => Task.FromResult(true)));
    }

    [Fact]
    public async Task Invocation_filter_rebinds_scope_and_aborts_when_membership_is_revoked()
    {
        using var f = new Fixture(); var snapshot = await f.Handshake(A);
        var caller = new Caller(User("alice"), snapshot);
        var method = typeof(TestHub).GetMethod(nameof(TestHub.Echo))!;
        var invocation = new HubInvocationContext(caller, f.Services, new TestHub(), method, []);
        var filter = new CompanyHubFilter(); var invoked = 0;
        await filter.InvokeMethodAsync(invocation, _ => { invoked++; Assert.Equal(A, f.Tenant.Company!.Scope); return ValueTask.FromResult<object?>("ok"); });
        f.Available = false;
        await Assert.ThrowsAsync<HubException>(async () => await filter.InvokeMethodAsync(invocation, _ => { invoked++; return ValueTask.FromResult<object?>(null); }));
        Assert.True(caller.Aborted); Assert.Equal(1, invoked);
    }

    [Fact]
    public async Task Headers_alone_cannot_supply_a_hub_handshake_feature()
    {
        using var f = new Fixture();
        var caller = new Caller(User("alice"), null);
        caller.Http.Request.Headers[CompanyRequestHeaders.Tenant] = A.TenantId;
        caller.Http.Request.Headers[CompanyRequestHeaders.Company] = A.BusinessAssociateId;
        var context = new HubLifetimeContext(caller, f.Services, new TestHub());
        await Assert.ThrowsAsync<HubException>(() => new CompanyHubFilter().OnConnectedAsync(context, _ => throw new InvalidOperationException()));
        Assert.True(caller.Aborted); Assert.Equal(0, f.Reads);
    }

    [Fact]
    public async Task Validated_HTTP_handshake_feature_rebinds_a_fresh_hub_scope_with_current_roles()
    {
        using var f = new Fixture(); f.Roles = ["Administrator", "Engineer"];
        var http = new DefaultHttpContext { User = User("alice") };
        http.Request.Path = "/hubs/workflow-notifications";
        http.Request.Headers[CompanyRequestHeaders.Tenant] = A.TenantId;
        http.Request.Headers[CompanyRequestHeaders.Company] = A.BusinessAssociateId;
        var requestTenant = new RepositoryTenantContext();
        await CompanyRequestTestInvoker.InvokeAsync(new CompanyRequestMiddleware(_ => Task.CompletedTask, Account), http, requestTenant,
            f.Resolver, f.Access.Object, new ModuleBindingService(null!, null!)); // Business handshake never uses control-plane binding operations.
        var handshake = http.Features.Get<RepositoryCompanyContext>();
        Assert.NotNull(handshake); Assert.Equal(A, handshake.Scope);
        Assert.True(http.User.IsInRole("Administrator")); Assert.Null(f.Tenant.Company);
        f.Roles = ["Engineer"];
        var caller = new Caller(http.User, handshake);
        await new CompanyHubFilter().OnConnectedAsync(new(caller, f.Services, new TestHub()), _ =>
        {
            Assert.Equal(A, f.Tenant.Company!.Scope);
            Assert.Equal(new[] { "Engineer" }, f.Tenant.Company.Roles);
            return Task.CompletedTask;
        });
        Assert.Equal(2, f.Reads);
    }

    private sealed class Fixture : IDisposable
    {
        public bool Available = true;
        public string[] Roles = ["Engineer"];
        public int Reads;
        public readonly RepositoryTenantContext Tenant = new();
        public readonly Mock<IRepositoryAccessService> Access = new();
        public readonly RepositoryCompanyContextResolver Resolver;
        public readonly CompanyHubScope Gate;
        public readonly ServiceProvider Services;
        public Fixture()
        {
            Access.Setup(x => x.GetCompanyAccessAsync(It.IsAny<string>(), It.IsAny<CompanyScope>(), It.IsAny<CancellationToken>()))
                .Returns((string _, CompanyScope scope, CancellationToken _) => { Reads++; return Task.FromResult<RepositoryCompanyAccess?>(Available ? new(scope, Roles, []) : null); });
            Resolver = new(Access.Object); Gate = new(Tenant, Resolver, Account);
            Services = new ServiceCollection().AddSingleton(Gate).BuildServiceProvider();
        }
        public async Task<RepositoryCompanyContext> Handshake(CompanyScope scope) => (await Resolver.ResolveAsync("alice", scope))!;
        public void Dispose() => Services.Dispose();
    }
    private sealed class TestHub : Hub { public string Echo() => "ok"; }
    private sealed class Caller : HubCallerContext
    {
        public readonly DefaultHttpContext Http = new();
        private readonly ClaimsPrincipal user;
        public bool Aborted;
        public Caller(ClaimsPrincipal user, RepositoryCompanyContext? company)
        {
            this.user = user;
            if (company is not null) Http.Features.Set(company);
            Features.Set<IHttpContextFeature>(new HttpFeature { HttpContext = Http });
        }
        public override string ConnectionId => "test-connection";
        public override string? UserIdentifier => Account(user);
        public override ClaimsPrincipal User => user;
        public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override CancellationToken ConnectionAborted => default;
        public override void Abort() => Aborted = true;
    }
    private sealed class HttpFeature : IHttpContextFeature { public HttpContext? HttpContext { get; set; } }
}
