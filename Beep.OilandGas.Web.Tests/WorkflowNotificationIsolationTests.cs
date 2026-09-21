using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.ApiService.Hubs;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.UserManagement.Contracts.Services;

namespace Beep.OilandGas.Web.Tests;

public sealed class WorkflowNotificationIsolationTests : IAsyncLifetime
{
    private WebApplication _host = null!;
    private string _baseUrl = null!;
    private readonly Mock<IAccessControlService> _access = new();
    private readonly Mock<INotificationPersonaReader> _personas = new();
    private readonly Mock<IProcessService> _processes = new();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        // Test-only authentication. Production JWT configuration is not replaced.
        builder.Services.AddAuthentication("test")
            .AddScheme<AuthenticationSchemeOptions, TestIdentityHandler>("test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSignalR();
        builder.Services.AddWorkflowNotificationAuthorization();
        builder.Services.AddSingleton(_access.Object);
        builder.Services.AddSingleton(_personas.Object);
        builder.Services.AddSingleton(_processes.Object);
        _host = builder.Build();
        _host.UseAuthentication();
        _host.UseAuthorization();
        _host.MapHub<WorkflowNotificationHub>("/hubs/workflow-notifications",
            options => options.CloseOnAuthenticationExpiration = true);
        await _host.StartAsync();
        _baseUrl = _host.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task TwoUsers_ReceiveOnlyOwnMessages_AndHaveSeparateReadState()
    {
        var tokens = new TokenProvider();
        await using var alice = CreateService(new TestAuthenticationState("alice"), tokens);
        await using var bob = CreateService(new TestAuthenticationState("bob"), tokens);
        await alice.StartAsync();
        await bob.StartAsync();
        Assert.True(alice.IsConnected, alice.LastError);
        Assert.True(bob.IsConnected, bob.LastError);

        await DeliverAsync("alice", "alice-only", () => alice.UnreadCount == 1);
        await DeliverAsync("bob", "bob-only", () => bob.UnreadCount == 1);
        Assert.Equal("alice-only", Assert.Single(alice.RecentNotifications).Id);
        Assert.Equal("bob-only", Assert.Single(bob.RecentNotifications).Id);
        alice.MarkAllRead();
        Assert.Equal(0, alice.UnreadCount);
        Assert.Equal(1, bob.UnreadCount);
    }

    [Fact]
    public async Task UserSubscription_CannotBeChosenByCaller()
    {
        await using var connection = Client("alice");
        await connection.StartAsync();
        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SubscribeToUser", "bob"));
    }

    [Fact]
    public async Task PersonaSubscription_RequiresAppRoleAndFieldAccess()
    {
        _personas.Setup(p => p.IsCurrentPersonaAsync(It.IsAny<string>(), "ENGINEER")).ReturnsAsync(true);
        _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string> { "PetroleumEngineer" });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-a", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-b", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = false });
        await using var connection = Client("alice");
        await connection.StartAsync();
        await connection.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-a");
        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-b"));
        _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string>());
        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-a"));
    }

    [Fact]
    public async Task ExternalRoleClaim_DoesNotGrantPersonaAccessWithoutAppAssignment()
    {
        _personas.Setup(p => p.IsCurrentPersonaAsync(It.IsAny<string>(), "ENGINEER")).ReturnsAsync(true);
        _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string>());
        await using var connection = Client("alice");
        await connection.StartAsync();
        // The test authentication ticket carries a PetroleumEngineer role claim.
        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-a"));
    }

    [Fact]
    public async Task UnknownPersona_IsDeniedEvenWithAppRole()
    {
        _personas.Setup(p => p.IsCurrentPersonaAsync(It.IsAny<string>(), "UNKNOWN")).ReturnsAsync(false);
        _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string> { "PetroleumEngineer" });
        await using var connection = Client("alice");
        await connection.StartAsync();
        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SubscribeToPersona", "UNKNOWN", "field-a"));
    }

    [Fact]
    public async Task AccountSwitch_ClearsPreviousUserStateAndReconnectsWithNewToken()
    {
        var auth = new TestAuthenticationState("alice");
        var tokens = new TokenProvider();
        tokens.SetUserToken("bob", "bob");
        await using var service = CreateService(auth, tokens);
        await service.StartAsync();
        await DeliverAsync("alice", "old-user", () => service.UnreadCount == 1);
        auth.SetUser("bob");
        await DeliverAsync("bob", "new-user", () => service.RecentNotifications.Any(n => n.Id == "new-user"));
        Assert.Equal("new-user", Assert.Single(service.RecentNotifications).Id);
    }

    [Fact]
    public async Task PersonaBroadcast_IsIsolatedByField()
    {
        _personas.Setup(p => p.IsCurrentPersonaAsync(It.IsAny<string>(), "ENGINEER")).ReturnsAsync(true);
        _access.Setup(a => a.GetUserRolesAsync(It.IsAny<string>(), null)).ReturnsAsync(new List<string> { "PetroleumEngineer" });
        _access.Setup(a => a.CheckAssetAccessAsync(It.IsAny<string>(), It.IsAny<string>(), "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        await using var alice = Client("alice");
        await using var bob = Client("bob");
        var messages = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var marker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bob.On<NotificationModel>("Notification", notification => messages.Enqueue(notification.Id));
        bob.On<string>("Marker", _ => marker.TrySetResult());
        await alice.StartAsync();
        await bob.StartAsync();
        await alice.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-a");
        await bob.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-b");
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        await hub.NotifyPersonaAsync("ENGINEER", "field-a", "PetroleumEngineer", "Notification", new NotificationModel { Id = "private-a" });
        await hub.NotifyPersonaAsync("ENGINEER", "field-b", "PetroleumEngineer", "Notification", new NotificationModel { Id = "private-b" });
        await hub.NotifyUserAsync("bob", "Marker", "processed");
        await marker.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new[] { "private-b" }, messages.ToArray());
    }

    [Fact]
    public async Task ProcessSubscription_RequiresBothFieldAndEntityAccess()
    {
        _processes.Setup(p => p.GetProcessInstanceAsync("process-1")).ReturnsAsync(new ProcessInstance
        { InstanceId = "process-1", FieldId = "field-a", EntityId = "well-1", EntityType = "WELL" });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-a", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "well-1", "WELL", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = false });
        await using var connection = Client("alice");
        await connection.StartAsync();
        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("SubscribeToProcess", "process-1"));
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "well-1", "WELL", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        await connection.InvokeAsync("SubscribeToProcess", "process-1");
    }

    [Theory]
    [InlineData("different-role")]
    [InlineData("persona-changed")]
    [InlineData("unscoped-payload")]
    public async Task PersonaSelectionDoesNotGrantAnotherTasksAudience(string reason)
    {
        _personas.Setup(p => p.IsCurrentPersonaAsync("alice", "ENGINEER")).ReturnsAsync(true);
        _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string> { "Viewer" });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-a", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        await using var connection = Client("alice");
        var messages = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var marker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<NotificationModel>("Notification", n => messages.Enqueue(n.Id));
        connection.On<string>("Marker", _ => marker.TrySetResult());
        await connection.StartAsync();
        await connection.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-a");
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        if (reason == "persona-changed")
            _personas.Setup(p => p.IsCurrentPersonaAsync("alice", "ENGINEER")).ReturnsAsync(false);
        if (reason == "unscoped-payload")
            await hub.Clients.Group("persona:ENGINEER:field:field-a").SendAsync("Notification", new NotificationModel { Id = "denied" });
        else
            await hub.NotifyPersonaAsync("ENGINEER", "field-a", reason == "different-role" ? "Approver" : "Viewer",
                "Notification", new NotificationModel { Id = "denied" });
        await hub.NotifyUserAsync("alice", "Marker", "processed");
        await marker.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Empty(messages);
    }

    [Theory]
    [InlineData("role")]
    [InlineData("field")]
    [InlineData("lookup-failure")]
    public async Task ExistingPersonaSubscription_RejectsNextDeliveryAfterAccessChanges(string change)
    {
        _personas.Setup(p => p.IsCurrentPersonaAsync(It.IsAny<string>(), "ENGINEER")).ReturnsAsync(true);
        _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string> { "PetroleumEngineer" });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-a", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        await using var connection = Client("alice");
        var messages = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var marker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<NotificationModel>("Notification", n => messages.Enqueue(n.Id));
        connection.On<string>("Marker", _ => marker.TrySetResult());
        await connection.StartAsync();
        await connection.InvokeAsync("SubscribeToPersona", "ENGINEER", "field-a");
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        await hub.NotifyPersonaAsync("ENGINEER", "field-a", "PetroleumEngineer", "Notification", new NotificationModel { Id = "before-change" });
        if (change == "role")
            _access.Setup(a => a.GetUserRolesAsync("alice", null)).ReturnsAsync(new List<string>());
        else if (change == "field")
            _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-a", "FIELD", null))
                .ReturnsAsync(new AccessCheckResponse { HasAccess = false });
        else
            _access.Setup(a => a.GetUserRolesAsync("alice", null)).ThrowsAsync(new InvalidOperationException("RBAC unavailable"));

        // Do not reconnect or resubscribe: a previously accepted group must not retain authority.
        await hub.NotifyPersonaAsync("ENGINEER", "field-a", "PetroleumEngineer", "Notification", new NotificationModel { Id = "after-change" });
        await hub.NotifyUserAsync("alice", "Marker", "processed");
        await marker.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(new[] { "before-change" }, messages.ToArray());
        Assert.Equal(HubConnectionState.Connected, connection.State);
    }

    [Fact]
    public async Task ExistingProcessSubscription_RejectsDeliveryAfterEntityAccessRevoked()
    {
        _processes.Setup(p => p.GetProcessInstanceAsync("process-1")).ReturnsAsync(new ProcessInstance
        { InstanceId = "process-1", FieldId = "field-a", EntityId = "well-1", EntityType = "WELL" });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "field-a", "FIELD", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "well-1", "WELL", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = true });
        await using var connection = Client("alice");
        var messages = new System.Collections.Concurrent.ConcurrentQueue<string>();
        var marker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<NotificationModel>("Notification", n => messages.Enqueue(n.Id));
        connection.On<string>("Marker", _ => marker.TrySetResult());
        await connection.StartAsync();
        await connection.InvokeAsync("SubscribeToProcess", "process-1");
        _access.Setup(a => a.CheckAssetAccessAsync("alice", "well-1", "WELL", null))
            .ReturnsAsync(new AccessCheckResponse { HasAccess = false });
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        await hub.NotifyProcessAsync("process-1", "Notification", new NotificationModel { Id = "restricted" });
        await hub.NotifyUserAsync("alice", "Marker", "processed");
        await marker.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Empty(messages);
    }

    [Fact]
    public async Task UnscopedBroadcast_IsRejected()
    {
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            hub.Clients.All.SendAsync("Notification", new NotificationModel { Id = "unscoped" }));
    }

    [Fact]
    public async Task AnonymousConnection_IsRejected()
    {
        await using var connection = Client(null);
        await Assert.ThrowsAsync<HttpRequestException>(() => connection.StartAsync());
    }

    [Fact]
    public async Task Reconnection_RejoinsOnlyTheAuthenticatedUser()
    {
        await using var connection = Client("alice");
        var received = new TaskCompletionSource<NotificationModel>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<NotificationModel>("Notification", item => received.TrySetResult(item));
        await connection.StartAsync();
        await connection.StopAsync();
        await connection.StartAsync();
        await DeliverAsync("alice", "after-reconnect", () => received.Task.IsCompleted);
        Assert.Equal("after-reconnect", (await received.Task).Id);
    }

    [Fact]
    public async Task Logout_ClearsNotificationsAndStopsConnection()
    {
        var auth = new TestAuthenticationState("alice");
        await using var service = CreateService(auth, new TokenProvider());
        await service.StartAsync();
        await DeliverAsync("alice", "before-logout", () => service.UnreadCount == 1);
        auth.SetUser(null);
        await WaitUntilAsync(() => !service.IsConnected && service.RecentNotifications.Count == 0);
        Assert.Equal(0, service.UnreadCount);
    }

    [Fact]
    public async Task MissingCurrentUserToken_DoesNotUseAnotherUsersToken()
    {
        var tokens = new TokenProvider();
        tokens.SetUserToken("bob", "bob");
        await using var service = CreateService(new TestAuthenticationState("alice"), tokens, seedToken: false);
        await service.StartAsync();
        Assert.False(service.IsConnected);
        Assert.NotNull(service.LastError);
        Assert.Empty(service.RecentNotifications);
    }

    [Fact]
    public async Task NotificationBuffer_IsBounded_AndRepeatedIdsAreDeduplicated()
    {
        await using var service = CreateService(new TestAuthenticationState("alice"), new TokenProvider());
        await service.StartAsync();
        await DeliverAsync("alice", "ready", () => service.UnreadCount == 1);
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        for (var index = 0; index < 110; index++)
            await hub.NotifyUserAsync("alice", "Notification", new NotificationModel { Id = $"item-{index}" });
        await WaitUntilAsync(() => service.RecentNotifications.FirstOrDefault()?.Id == "item-109");
        Assert.Equal(100, service.RecentNotifications.Count);
        Assert.Equal(100, service.UnreadCount);
        await hub.NotifyUserAsync("alice", "Notification", new NotificationModel { Id = "item-109" });
        // An ordered subsequent marker proves the duplicate message was processed.
        await hub.NotifyUserAsync("alice", "Notification", new NotificationModel { Id = "marker" });
        await WaitUntilAsync(() => service.RecentNotifications.FirstOrDefault()?.Id == "marker");
        Assert.Single(service.RecentNotifications, n => n.Id == "item-109");
        Assert.Equal(100, service.UnreadCount);
    }

    private NotificationService CreateService(TestAuthenticationState auth, TokenProvider tokens, bool seedToken = true)
    {
        if (seedToken) tokens.SetUserToken(auth.UserId!, auth.UserId!);
        return new NotificationService(auth, tokens,
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["ApiService:BaseUrl"] = _baseUrl }).Build(), NullLogger<NotificationService>.Instance);
    }

    private HubConnection Client(string? userId) => new HubConnectionBuilder()
        .WithUrl(_baseUrl + "/hubs/workflow-notifications", options =>
            options.AccessTokenProvider = () => Task.FromResult(userId))
        .Build();

    private async Task DeliverAsync(string userId, string id, Func<bool> delivered)
    {
        var hub = _host.Services.GetRequiredService<IHubContext<WorkflowNotificationHub>>();
        // Handshake completion may precede OnConnectedAsync; retry the same ID.
        for (var attempt = 0; attempt < 100 && !delivered(); attempt++)
        {
            await hub.NotifyUserAsync(userId, "Notification", new NotificationModel { Id = id });
            await Task.Delay(25);
        }
        Assert.True(delivered(), "The notification was not delivered to its authenticated recipient.");
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        for (var attempt = 0; attempt < 100 && !predicate(); attempt++) await Task.Delay(25);
        Assert.True(predicate(), "The asynchronous notification state did not settle.");
    }

    private sealed class TestAuthenticationState(string? userId) : AuthenticationStateProvider
    {
        public string? UserId { get; private set; } = userId;
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(
            new AuthenticationState(UserId is null ? new ClaimsPrincipal(new ClaimsIdentity())
                : new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", UserId) }, "test"))));
        public void SetUser(string? id)
        {
            UserId = id;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    private sealed class TestIdentityHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var header = Request.Headers.Authorization.ToString();
            if (!header.StartsWith("Bearer ", StringComparison.Ordinal))
                return Task.FromResult(AuthenticateResult.NoResult());
            var subject = header[7..];
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            { new Claim("sub", "external-" + subject), new Claim(ClaimTypes.NameIdentifier, subject), new Claim(ClaimTypes.Role, "PetroleumEngineer") }, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
