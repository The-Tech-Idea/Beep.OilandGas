using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class UserAdministrationClientTests
{
    [Fact]
    public async Task UpdateUsesCanonicalContractAndReturnsNewVersion()
    {
        using var handler = new Handler(HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new UserAdministrationClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        var saved = await client.UpdateUserAsync("owner", new("Engineer", false, "before"));
        Assert.Equal("after", saved!.ConcurrencyStamp);
        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal("/api/identity/users/owner", handler.Path);
        using var json = JsonDocument.Parse(handler.Body!);
        Assert.Equal(3, json.RootElement.EnumerateObject().Count());
        Assert.Equal("Engineer", json.RootElement.GetProperty("fullName").GetString());
        Assert.False(json.RootElement.GetProperty("isActive").GetBoolean());
        Assert.Equal("before", json.RootElement.GetProperty("concurrencyStamp").GetString());
    }

    [Fact]
    public async Task RejectedUpdateCannotBeReportedAsSaved()
    {
        using var handler = new Handler(HttpStatusCode.Conflict);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new UserAdministrationClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.UpdateUserAsync("owner", new("Engineer", false, "stale")));
    }

    private sealed class Handler(HttpStatusCode status) : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? Body { get; private set; }
        public HttpMethod? Method { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Path = request.RequestUri!.AbsolutePath;
            Method = request.Method;
            Body = await request.Content!.ReadAsStringAsync(token);
            return new(status) { Content = new StringContent("{\"userId\":\"owner\",\"userName\":\"owner\",\"isActive\":false,\"concurrencyStamp\":\"after\"}") };
        }
    }
}
