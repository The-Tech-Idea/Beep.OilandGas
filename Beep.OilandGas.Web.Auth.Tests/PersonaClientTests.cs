using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class PersonaClientTests
{
    [Fact]
    public async Task ProfileRequestHasNoCallerActorOrRowIdentity()
    {
        using var handler = new Handler("{\"userId\":\"owner\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new PersonaClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        await client.SaveAsync("owner", new("ENGINEER", ConcurrencyStamp: "version"));
        Assert.Equal("/api/personas/users/owner", handler.Path);
        using var document = JsonDocument.Parse(handler.Body!);
        Assert.Equal("version", document.RootElement.GetProperty("concurrencyStamp").GetString());
        Assert.False(document.RootElement.TryGetProperty("userId", out _));
        Assert.False(document.RootElement.TryGetProperty("actorUserId", out _));
        Assert.False(document.RootElement.TryGetProperty("effectiveAccessContextJson", out _));
    }

    [Fact]
    public async Task MissingProfileUsesExplicitNullableEnvelope()
    {
        using var handler = new Handler("{\"profile\":null}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new PersonaClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        Assert.Null(await client.GetAsync("owner"));
    }

    private sealed class Handler(string response) : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? Body { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Path = request.RequestUri!.AbsolutePath;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(token);
            return new(HttpStatusCode.OK) { Content = new StringContent(response) };
        }
    }
}
