using System.IO.Pipelines;
using System.Buffers;
using System.Security.Claims;
using System.Text;
using Beep.OilandGas.ApiService.Hubs;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using OperationStatus = Beep.OilandGas.Models.Data.DataManagement.OperationStatus;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyProgressTests
{
    private static readonly CompanyScope A = new("tenant-a", "company-a");
    private static readonly CompanyScope B = new("tenant-b", "company-a");
    private static readonly CompanyScope SameTenant = new("tenant-a", "company-b");
    private static ClaimsPrincipal User(string id) => new(new ClaimsIdentity([new Claim("account", id)], "test"));
    private static async Task<RepositoryCompanyContext> Company(CompanyScope scope, string user = "alice", string[]? roles = null, string[]? permissions = null)
    {
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(service => service.GetCompanyAccessAsync(user, scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RepositoryCompanyAccess(scope, roles ?? ["Admin"], permissions ?? []));
        return (await new RepositoryCompanyContextResolver(access.Object).ResolveAsync(user, scope))!;
    }
    private static ProgressTrackingService Service(CompanyProgressState state, RepositoryCompanyContext company)
    {
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(company);
        return new(state, tenant, NullLogger<ProgressTrackingService>.Instance);
    }

    [Theory]
    [InlineData("other-tenant")]
    [InlineData("other-company")]
    [InlineData("other-account")]
    public async Task Another_owner_cannot_poll_update_complete_cancel_or_group_an_operation(string other)
    {
        var state = new CompanyProgressState();
        var owner = Service(state, await Company(A));
        var stranger = Service(state, await Company(other == "other-tenant" ? B : other == "other-company" ? SameTenant : A,
            other == "other-account" ? "bob" : "alice"));
        var id = owner.StartOperation("import", "private data");
        Assert.Null(stranger.GetProgress(id));
        stranger.UpdateProgress(id, 99, "tampered"); stranger.CompleteOperation(id, true); stranger.CancelOperation(id);
        stranger.UpdateProgress(new ProgressUpdate { OperationId = id, StatusMessage = "forged registration" });
        Assert.Null(stranger.GetProgress(id));
        Assert.Throws<InvalidOperationException>(() => stranger.RegisterOperationGroup("group", [id]));
        var actual = owner.GetProgress(id)!;
        Assert.False(actual.IsComplete); Assert.Equal(0, actual.ProgressPercentage); Assert.Equal("private data", actual.StatusMessage);
    }

    [Fact]
    public async Task Workflow_steps_and_completion_are_owned_and_terminal()
    {
        var state = new CompanyProgressState(); var owner = Service(state, await Company(A));
        var stranger = Service(state, await Company(A, "bob"));
        var id = owner.StartWorkflow("study", ["one"]); var step = owner.GetWorkflowProgress(id)!.Steps[0].StepId;
        stranger.UpdateWorkflowStep(id, step, "operation", 90, "tampered"); stranger.CompleteWorkflowStep(id, step, true);
        stranger.CompleteWorkflow(id, true); Assert.Null(stranger.GetWorkflowProgress(id));
        Assert.Equal(0, owner.GetWorkflowProgress(id)!.ProgressPercentage);
        owner.CompleteWorkflow(id, true);
        owner.UpdateWorkflowStep(id, step, "operation", 1, "late"); owner.CompleteWorkflow(id, false, "late failure");
        Assert.Equal(WorkflowStatus.Completed, owner.GetWorkflowProgress(id)!.Status);
        Assert.Equal(100, owner.GetWorkflowProgress(id)!.ProgressPercentage);
    }

    [Fact]
    public async Task Reused_group_ids_never_mix_accounts_or_companies_and_envelopes_keep_owner()
    {
        var state = new CompanyProgressState(); var alice = await Company(A); var bob = await Company(A, "bob");
        var first = Service(state, alice); var second = Service(state, bob); var foreign = Service(state, await Company(B));
        var one = first.StartOperation("import", "alice secret"); var two = second.StartOperation("import", "bob secret");
        var three = foreign.StartOperation("import", "foreign secret");
        first.RegisterOperationGroup("reused", [one]); second.RegisterOperationGroup("reused", [two]); foreign.RegisterOperationGroup("reused", [three]);
        Assert.Equal(one, Assert.Single(first.GetOperationGroupProgress("reused")!.Operations).Key);
        Assert.Equal(two, Assert.Single(second.GetOperationGroupProgress("reused")!.Operations).Key);
        Assert.Equal(three, Assert.Single(foreign.GetOperationGroupProgress("reused")!.Operations).Key);
        Assert.Throws<InvalidOperationException>(() => first.RegisterOperationGroup("reused", [one]));
        while (state.Notifications.Reader.TryRead(out var item))
        {
            if (item.UserId == "alice" && item.Resource == "reused" && item.Scope == A)
            {
                var channel = new CompanyHubChannel(A, CompanyChannelKind.Operation, "reused");
                Assert.True(state.CanDeliver(alice, channel, item)); Assert.False(state.CanDeliver(bob, channel, item));
            }
        }
    }

    [Fact]
    public async Task Polling_returns_snapshots_and_unknown_updates_cannot_create_operations()
    {
        var state = new CompanyProgressState(); var owner = Service(state, await Company(A));
        owner.UpdateProgress(new ProgressUpdate { OperationId = "not-started" }); Assert.Null(owner.GetProgress("not-started"));
        var id = owner.StartOperation("test", "original"); var snapshot = owner.GetProgress(id)!; snapshot.StatusMessage = "tampered";
        Assert.Equal("original", owner.GetProgress(id)!.StatusMessage);
        owner.CompleteOperation(id, true); owner.UpdateProgress(id, 1, "late"); owner.CancelOperation(id);
        Assert.False(owner.GetProgress(id)!.HasError); Assert.Equal(100, owner.GetProgress(id)!.ProgressPercentage);
    }

    [Fact]
    public void Tenant_selection_alone_cannot_start_or_read_company_progress()
    {
        var tenant = new RepositoryTenantContext(); tenant.SelectTenant("tenant-a");
        var service = new ProgressTrackingService(new(), tenant, NullLogger<ProgressTrackingService>.Instance);
        Assert.Throws<InvalidOperationException>(() => service.StartOperation("test", "unowned"));
        Assert.Throws<InvalidOperationException>(() => service.GetProgress("id"));
        Assert.Throws<InvalidOperationException>(() => service.StartWorkflow("test", ["step"]));
    }

    [Theory]
    [InlineData("membership")]
    [InlineData("role")]
    [InlineData("wrong-company")]
    [InlineData("lookup-error")]
    public async Task A_worker_refuses_revoked_access_and_never_selects_a_substitute(string denial)
    {
        var captured = await Company(A); var tenant = new RepositoryTenantContext(); var access = new Mock<IRepositoryAccessService>();
        var lookup = access.Setup(service => service.GetCompanyAccessAsync("alice", A, It.IsAny<CancellationToken>()));
        if (denial == "lookup-error") lookup.ThrowsAsync(new InvalidOperationException("provider failed"));
        else lookup.ReturnsAsync(denial == "membership" ? null : new RepositoryCompanyAccess(denial == "wrong-company" ? B : A,
            denial == "role" ? [] : ["Admin"], []));
        await Assert.ThrowsAnyAsync<Exception>(() => CompanyBackgroundScope.RestoreAsync(captured, new(access.Object), tenant, default));
        Assert.Null(tenant.Company); Assert.Null(tenant.TenantId);
    }

    [Fact]
    public async Task Worker_restoration_uses_fresh_grants_in_a_new_scope()
    {
        var captured = await Company(A); var tenant = new RepositoryTenantContext(); var access = new Mock<IRepositoryAccessService>();
        access.Setup(service => service.GetCompanyAccessAsync("alice", A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RepositoryCompanyAccess(A, ["Admin", "Engineer"], ["current"]));
        await CompanyBackgroundScope.RestoreAsync(captured, new(access.Object), tenant, default);
        Assert.Equal(A, tenant.Company!.Scope); Assert.Contains("Engineer", tenant.Company.Roles);
        Assert.NotSame(captured, tenant.Company);
    }

    [Theory]
    [InlineData("role")]
    [InlineData("permission")]
    public async Task Revoked_grants_deny_polling_mutation_and_grouping_even_with_current_membership(string denial)
    {
        var state = new CompanyProgressState(); var captured = await Company(A, permissions: ["ReadProgress"]);
        var creator = Service(state, captured); var id = creator.StartOperation("import", "private");
        var current = await Company(A, roles: denial == "role" ? [] : ["Admin"], permissions: denial == "permission" ? [] : ["ReadProgress"]);
        var revoked = Service(state, current);
        Assert.Null(revoked.GetProgress(id)); revoked.UpdateProgress(id, 100, "tampered"); revoked.CancelOperation(id);
        Assert.Throws<InvalidOperationException>(() => revoked.RegisterOperationGroup("group", [id]));
        Assert.False(creator.GetProgress(id)!.IsComplete);
        Assert.True(state.Notifications.Reader.TryRead(out var notification));
        Assert.False(state.CanDeliver(current, new(A, CompanyChannelKind.Operation, id), notification!));
    }

    [Fact]
    public async Task Permission_revocation_also_prevents_worker_restoration()
    {
        var captured = await Company(A, permissions: ["Import"]); var tenant = new RepositoryTenantContext();
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(service => service.GetCompanyAccessAsync("alice", A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RepositoryCompanyAccess(A, ["Admin"], []));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => CompanyBackgroundScope.RestoreAsync(captured, new(access.Object), tenant, default));
        Assert.Null(tenant.Company);
    }

    [Fact]
    public async Task Workflow_tracking_cancellation_is_terminal_and_account_owned()
    {
        var state = new CompanyProgressState(); var owner = Service(state, await Company(A));
        var other = Service(state, await Company(A, "bob")); var id = owner.StartWorkflow("workflow", ["one", "two"]);
        other.CancelOperation(id); Assert.Equal(WorkflowStatus.Running, owner.GetWorkflowProgress(id)!.Status);
        owner.CancelOperation(id); owner.CompleteWorkflow(id, true);
        var result = owner.GetWorkflowProgress(id)!; Assert.Equal(WorkflowStatus.Cancelled, result.Status);
        Assert.True(result.IsComplete); Assert.All(result.Steps, step => Assert.Equal(OperationStatus.Cancelled, step.Status));
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public async Task Terminal_progress_retention_prunes_authority_and_stops_delivery()
    {
        var clock = new Clock(); var state = new CompanyProgressState(clock); var company = await Company(A);
        var service = Service(state, company); var id = service.StartOperation("test", "private");
        service.RegisterOperationGroup("group", [id]);
        service.CompleteOperation(id, true); var workflow = service.StartWorkflow("study", ["step"]); service.CompleteWorkflow(workflow, true);
        Assert.True(service.GetOperationGroupProgress("group")!.IsComplete);
        clock.Now += TimeSpan.FromMinutes(5); Assert.Null(service.GetProgress(id));
        Assert.Null(service.GetOperationGroupProgress("group"));
        Assert.False(state.CanRead(company, new(A, CompanyChannelKind.Operation, id))); Assert.NotNull(service.GetWorkflowProgress(workflow));
        clock.Now += TimeSpan.FromMinutes(5); Assert.Null(service.GetWorkflowProgress(workflow));
    }

    private sealed class HttpFeature(HttpContext http) : IHttpContextFeature { public HttpContext? HttpContext { get; set; } = http; }
    private sealed class UserFeature(ClaimsPrincipal user) : IConnectionUserFeature { public ClaimsPrincipal? User { get; set; } = user; }
    private sealed record Duplex(PipeReader Input, PipeWriter Output) : IDuplexPipe;
    private static (HubConnectionContext Hub, PipeReader Output) Connection(RepositoryCompanyContext company)
    {
        var input = new Pipe(); var output = new Pipe();
        var native = new DefaultConnectionContext(Guid.NewGuid().ToString()) { Transport = new Duplex(input.Reader, output.Writer) };
        var http = new DefaultHttpContext(); http.Features.Set(company);
        native.Features.Set<IHttpContextFeature>(new HttpFeature(http));
        native.Features.Set<IConnectionUserFeature>(new UserFeature(User(company.UserId)));
        var hub = new HubConnectionContext(native, new HubConnectionContextOptions(), NullLoggerFactory.Instance)
        { Protocol = new JsonHubProtocol() };
        return (hub, output.Reader);
    }

    [Theory]
    [InlineData("membership")]
    [InlineData("role")]
    [InlineData("permission")]
    [InlineData("lookup-error")]
    public async Task Actual_transport_delivers_only_the_envelope_owner_then_removes_revoked_subscriptions(string denial)
    {
        var alice = await Company(A, permissions: ["ReadProgress"]); var bob = await Company(A, "bob"); var state = new CompanyProgressState();
        var a = Service(state, alice); var b = Service(state, bob);
        a.RegisterOperationGroup("reused", []); b.RegisterOperationGroup("reused", []);
        var access = new Mock<IRepositoryAccessService>(); var revoked = false;
        access.Setup(service => service.GetCompanyAccessAsync(It.IsAny<string>(), A, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string user, CompanyScope _, CancellationToken _) =>
            {
                if (revoked && user == "alice")
                {
                    if (denial == "lookup-error") throw new InvalidOperationException("lookup failed");
                    if (denial == "membership") return null;
                    return new(A, denial == "role" ? [] : ["Admin"], denial == "permission" ? [] : ["ReadProgress"]);
                }
                return new(A, ["Admin"], ["ReadProgress"]);
            });
        var services = new ServiceCollection(); services.AddSingleton(state); services.AddSingleton(access.Object);
        services.AddScoped<RepositoryTenantContext>(); services.AddScoped<RepositoryCompanyContextResolver>(); services.AddScoped<CompanyHubScope>();
        services.AddSingleton<Func<ClaimsPrincipal, string?>>(principal => principal.FindFirst("account")?.Value);
        await using var provider = services.BuildServiceProvider();
        var manager = new ProgressHubLifetimeManager(NullLogger<DefaultHubLifetimeManager<ProgressHub>>.Instance,
            provider.GetRequiredService<IServiceScopeFactory>(), (_, _, _) => { });
        var one = Connection(alice); var two = Connection(bob); var group = CompanyHubGroups.Name(new(A, CompanyChannelKind.Operation, "reused"));
        await manager.OnConnectedAsync(one.Hub); await manager.OnConnectedAsync(two.Hub);
        try
        {
            await manager.AddToGroupAsync(one.Hub.ConnectionId, group); await manager.AddToGroupAsync(two.Hub.ConnectionId, group);
            // Exercise the actual hosted publisher and transport together, without recreating diagnostics SDK contracts.
            while (state.Notifications.Reader.TryRead(out _)) { }
            var client = new Mock<IClientProxy>();
            client.Setup(proxy => proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
                .Returns((string method, object?[] args, CancellationToken token) => manager.SendGroupAsync(group, method, args, token));
            var hub = new Mock<IHubContext<ProgressHub>> { DefaultValue = DefaultValue.Mock };
            hub.Setup(context => context.Clients.Group(group)).Returns(client.Object);
            using (var publisher = new ProgressTrackingPublisher(state, hub.Object, (_, _, _) => throw new InvalidOperationException("Unexpected publisher failure")))
            {
                a.UpdateOperationGroup("reused"); await publisher.StartAsync(default);
                try
                {
                    using var publishingTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var initial = await one.Output.ReadAsync(publishingTimeout.Token);
                    var initialJson = Encoding.UTF8.GetString(initial.Buffer.ToArray()); one.Output.AdvanceTo(initial.Buffer.End);
                    Assert.Contains("MultiOperationProgress", initialJson); Assert.DoesNotContain("userId", initialJson);
                    Assert.False(two.Output.TryRead(out _));
                }
                finally { await publisher.StopAsync(default); }
            }
            var notification = new CompanyProgressNotification(alice, CompanyChannelKind.Operation, "reused", System.Text.Json.JsonSerializer.SerializeToElement(new { secret = "alice" }));
            await manager.SendGroupAsync(group, "ProgressUpdate", [notification]);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var read = await one.Output.ReadAsync(timeout.Token); var json = Encoding.UTF8.GetString(read.Buffer.ToArray()); one.Output.AdvanceTo(read.Buffer.End);
            Assert.Contains("alice", json); Assert.DoesNotContain("userId", json); Assert.False(two.Output.TryRead(out _));
            revoked = true; await manager.SendGroupAsync(group, "ProgressUpdate", [notification]);
            Assert.False(one.Output.TryRead(out _)); Assert.False(two.Output.TryRead(out _));
            revoked = false; await manager.SendGroupAsync(group, "ProgressUpdate", [notification]);
            Assert.False(one.Output.TryRead(out _)); // A revoked subscription must be explicitly rejoined.
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.SendGroupAsync(group, "ProgressUpdate", [notification.Payload]));
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.SendAllAsync("ProgressUpdate", [notification]));
            await Assert.ThrowsAsync<HubException>(() => manager.AddToGroupAsync(one.Hub.ConnectionId, CompanyHubGroups.Name(new(B, CompanyChannelKind.Operation, "reused"))));
        }
        finally { await manager.OnDisconnectedAsync(one.Hub); await manager.OnDisconnectedAsync(two.Hub); }
    }

    private static HttpContext Request(RepositoryCompanyContext company)
    { var http = new DefaultHttpContext(); http.Features.Set(company); return http; }

    private static async Task WaitFor(ScopedBackgroundOperationQueue queue, string key, Beep.OilandGas.Models.Core.Interfaces.BackgroundOperationState expected)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (queue.GetStatus(key)?.State != expected) await Task.Delay(10, timeout.Token);
    }

    [Fact]
    public async Task Actual_queue_restores_company_in_a_new_scope_after_request_disposal()
    {
        var captured = await Company(A); var http = new HttpContextAccessor { HttpContext = Request(captured) };
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(service => service.GetCompanyAccessAsync("alice", A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RepositoryCompanyAccess(A, ["Admin", "Engineer"], []));
        var services = new ServiceCollection(); services.AddSingleton(access.Object); services.AddScoped<RepositoryTenantContext>(); services.AddScoped<RepositoryCompanyContextResolver>();
        await using var provider = services.BuildServiceProvider(validateScopes: true);
        using var queue = new ScopedBackgroundOperationQueue(provider.GetRequiredService<IServiceScopeFactory>(), (_, _, _) => "failure-reference", http);
        var completed = new TaskCompletionSource<RepositoryCompanyContext>(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(queue.TryEnqueue<RepositoryTenantContext, TaskCompletionSource<RepositoryCompanyContext>>("job", completed,
            static (tenant, completion, _) => { completion.SetResult(tenant.Company!); return Task.CompletedTask; }));
        http.HttpContext = null; await queue.StartAsync(default);
        try
        {
            var restored = await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.NotSame(captured, restored); Assert.Equal(A, restored.Scope); Assert.Contains("Engineer", restored.Roles);
            Assert.Null(queue.GetStatus("job"));
            http.HttpContext = Request(captured);
            await WaitFor(queue, "job", Beep.OilandGas.Models.Core.Interfaces.BackgroundOperationState.Succeeded);
        }
        finally { await queue.StopAsync(default); }
    }

    [Theory]
    [InlineData("membership")]
    [InlineData("role")]
    [InlineData("permission")]
    public async Task Actual_queue_refuses_revoked_business_execution_and_reports_a_failure_reference(string denial)
    {
        var captured = await Company(A, permissions: ["Import"]); var http = new HttpContextAccessor { HttpContext = Request(captured) };
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(service => service.GetCompanyAccessAsync("alice", A, It.IsAny<CancellationToken>()))
            .ReturnsAsync(denial == "membership" ? null : new RepositoryCompanyAccess(A, denial == "role" ? [] : ["Admin"], denial == "permission" ? [] : ["Import"]));
        var services = new ServiceCollection(); services.AddSingleton(access.Object); services.AddScoped<RepositoryTenantContext>(); services.AddScoped<RepositoryCompanyContextResolver>();
        await using var provider = services.BuildServiceProvider(); var executions = 0; var reports = 0;
        using var queue = new ScopedBackgroundOperationQueue(provider.GetRequiredService<IServiceScopeFactory>(), (_, _, _) => { Interlocked.Increment(ref reports); return "failure-reference"; }, http);
        Assert.True(queue.TryEnqueue<RepositoryTenantContext, int>("job", 0, (_, _, _) => { Interlocked.Increment(ref executions); return Task.CompletedTask; }));
        await queue.StartAsync(default);
        try
        {
            await WaitFor(queue, "job", Beep.OilandGas.Models.Core.Interfaces.BackgroundOperationState.Failed);
            Assert.Equal(0, executions); Assert.Equal(1, reports); Assert.Contains("failure-reference", queue.GetStatus("job")!.Error);
        }
        finally { await queue.StopAsync(default); }
    }

    [Fact]
    public async Task Queue_status_and_admission_keys_are_company_and_account_owned()
    {
        var alice = await Company(A); var http = new HttpContextAccessor { HttpContext = Request(alice) };
        await using var provider = new ServiceCollection().BuildServiceProvider();
        using var queue = new ScopedBackgroundOperationQueue(provider.GetRequiredService<IServiceScopeFactory>(), (_, _, _) => "failure-reference", http);
        Assert.True(queue.TryEnqueue<IServiceProvider, int>("reused", 0, static (_, _, _) => Task.CompletedTask));
        Assert.False(queue.TryEnqueue<IServiceProvider, int>("reused", 0, static (_, _, _) => Task.CompletedTask));
        foreach (var stranger in new[] { await Company(A, "bob"), await Company(B), await Company(SameTenant) })
        {
            http.HttpContext = Request(stranger); Assert.Null(queue.GetStatus("reused"));
            Assert.True(queue.TryEnqueue<IServiceProvider, int>("reused", 0, static (_, _, _) => Task.CompletedTask));
        }
        await queue.StopAsync(default);
    }

    [Fact]
    public async Task Queue_requires_validated_company_feature_and_rejects_headers_alone()
    {
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        http.HttpContext.Request.Headers[CompanyRequestHeaders.Tenant] = A.TenantId;
        http.HttpContext.Request.Headers[CompanyRequestHeaders.Company] = A.BusinessAssociateId;
        await using var provider = new ServiceCollection().BuildServiceProvider();
        using var queue = new ScopedBackgroundOperationQueue(provider.GetRequiredService<IServiceScopeFactory>(), (_, _, _) => "failure-reference", http);
        Assert.False(queue.TryEnqueue<IServiceProvider, int>("job", 0, static (_, _, _) => Task.CompletedTask));
        Assert.Null(queue.GetStatus("job"));
        http.HttpContext = null; Assert.False(queue.TryEnqueue<IServiceProvider, int>("job", 0, static (_, _, _) => Task.CompletedTask));
    }
}
