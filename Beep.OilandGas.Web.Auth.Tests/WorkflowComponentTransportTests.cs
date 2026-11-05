#if WORKFLOW_INBOX_FOCUSED
using System.Net.WebSockets;
using System.Buffers;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkflowComponentTransportTests
{
    public sealed class BudgetProbeHub : Hub
    {
        public int Count(string value) => value.Length;
    }
    private sealed class Host(WebApplication app, Uri address) : IAsyncDisposable
    {
        internal Uri Address => address;
        internal IHubProtocol ComponentProtocol => app.Services.GetServices<IHubProtocol>().Single(protocol => protocol.Name == "blazorpack");
        internal static async Task<Host> Start()
        {
            var builder = WebApplication.CreateBuilder(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddRazorComponents().AddOilGasInteractiveServerComponents();
            var app = builder.Build(); app.MapBlazorHub(); app.MapHub<BudgetProbeHub>("/budget-probe");
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            return new(app, new Uri(address));
        }
        public async ValueTask DisposeAsync() { await app.StopAsync(); await app.DisposeAsync(); }
    }
    private static async Task<ClientWebSocket> Connect(Uri address, string path, CancellationToken token, string protocol = "json")
    {
        var socket = new ClientWebSocket();
        await socket.ConnectAsync(new UriBuilder(address) { Scheme = "ws", Path = path }.Uri, token);
        await Send(socket, JsonSerializer.Serialize(new { protocol, version = 1 }), token);
        Assert.Equal("{}", await Receive(socket, token)); return socket;
    }
    private static Task Send(ClientWebSocket socket, string json, CancellationToken token) =>
        socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json + '\u001e')), WebSocketMessageType.Text, true, token);
    private static async Task<string> Receive(ClientWebSocket socket, CancellationToken token)
    {
        var bytes = new byte[8192]; using var text = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(bytes, token);
            if (result.MessageType == WebSocketMessageType.Close) return "CLOSED";
            text.Write(bytes, 0, result.Count);
            var current = Encoding.UTF8.GetString(text.ToArray()); var end = current.IndexOf('\u001e');
            if (end >= 0) return current[..end];
        }
    }
    private static string Invocation(string target, int characters) => JsonSerializer.Serialize(new
    {
        type = 1, invocationId = "transport-proof", target, arguments = new[] { new string('X', characters) }
    });
    private static async Task RequireClosed(ClientWebSocket socket, CancellationToken token)
    {
        var message = await Receive(socket, token);
        Assert.True(message == "CLOSED" || JsonDocument.Parse(message).RootElement.GetProperty("type").GetInt32() == 7, message);
    }
    private sealed class Binder : IInvocationBinder
    {
        public Type GetReturnType(string invocationId) => typeof(object);
        public IReadOnlyList<Type> GetParameterTypes(string methodName) => [];
        public Type GetStreamItemType(string streamId) => typeof(object);
    }
    private static Task SendPack(ClientWebSocket socket, IHubProtocol protocol, int characters, CancellationToken token) =>
        socket.SendAsync(new ArraySegment<byte>(protocol.GetMessageBytes(new InvocationMessage("transport-proof", "TransportProbeNotAMethod", [new string('X', characters)])).ToArray()), WebSocketMessageType.Binary, true, token);
    private static async Task<HubMessage> ReceivePack(ClientWebSocket socket, IHubProtocol protocol, CancellationToken token)
    {
        var bytes = new byte[8192]; using var buffer = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(bytes, token);
            if (result.MessageType == WebSocketMessageType.Close) return new CloseMessage(null);
            buffer.Write(bytes, 0, result.Count); var input = new ReadOnlySequence<byte>(buffer.ToArray());
            if (protocol.TryParseMessage(ref input, new Binder(), out var message)) return message!;
        }
    }

    [Fact] public async Task ComponentChannelReceivesLargeOriginalButItsFiniteLimitStillClosesOversizedMessages()
    {
        await using var host = await Host.Start(); using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var socket = await Connect(host.Address, "/_blazor", timeout.Token, "blazorpack");
        await SendPack(socket, host.ComponentProtocol, 100000, timeout.Token);
        var accepted = Assert.IsType<CompletionMessage>(await ReceivePack(socket, host.ComponentProtocol, timeout.Token));
        Assert.Equal("transport-proof", accepted.InvocationId);
        Assert.False(string.IsNullOrWhiteSpace(accepted.Error));
        try { await SendPack(socket, host.ComponentProtocol, checked((int)OilGasComponentTransport.MaximumRetainedOriginalMessageBytes + 1), timeout.Token); }
        catch (WebSocketException) { Assert.NotEqual(WebSocketState.Open, socket.State); return; }
        Assert.IsType<CloseMessage>(await ReceivePack(socket, host.ComponentProtocol, timeout.Token));
    }

    [Fact] public async Task ComponentBudgetDoesNotRaiseTheDefaultForOtherApplicationHubs()
    {
        await using var host = await Host.Start(); using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        using var socket = await Connect(host.Address, "/budget-probe", timeout.Token);
        await Send(socket, Invocation("Count", 1024), timeout.Token);
        Assert.Equal(1024, JsonDocument.Parse(await Receive(socket, timeout.Token)).RootElement.GetProperty("result").GetInt32());
        await Send(socket, Invocation("Count", 40000), timeout.Token); await RequireClosed(socket, timeout.Token);
    }
}
#endif
