using System.Net;
using System.Text;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.Web.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class RepositorySignInTests
{
    [Theory]
    [InlineData("Created")]
    [InlineData("Registered")]
    [InlineData("AlreadyCompleted")]
    public async Task TokenIsCachedOnlyAfterConfirmedRegistration(string status)
    {
        var subject = Guid.NewGuid().ToString();
        var tokens = new TokenProvider();
        tokens.SetUserToken(subject, "previous-token");
        using var handler = new Handler(200, "{\"status\":\"" + status + "\"}",
            () => Assert.Equal("previous-token", tokens.GetUserToken(subject)));
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example/") };
        await new RepositorySignInService(new RepositoryAccountClient(http), tokens).RegisterAsync(subject, "new-token");
        Assert.Equal("new-token", tokens.GetUserToken(subject));
        Assert.Equal("Bearer new-token", handler.Authorization);
        Assert.Equal("/api/setup/repository/register", handler.Path);
    }

    [Theory]
    [InlineData(200, "{}")]
    [InlineData(200, "null")]
    [InlineData(200, "{\"status\":\"NotAllowed\"}")]
    [InlineData(200, "{\"status\":\"created\"}")]
    [InlineData(200, "not-json")]
    [InlineData(204, "")]
    [InlineData(401, "{}")]
    [InlineData(403, "{}")]
    [InlineData(409, "{}")]
    [InlineData(500, "{}")]
    public async Task UnconfirmedRegistrationCannotReplaceCachedToken(int code, string body)
    {
        var subject = Guid.NewGuid().ToString();
        var tokens = new TokenProvider();
        tokens.SetUserToken(subject, "previous-token");
        using var handler = new Handler(code, body);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example/") };
        await Assert.ThrowsAnyAsync<Exception>(() => new RepositorySignInService(new RepositoryAccountClient(http), tokens)
            .RegisterAsync(subject, "rejected-token"));
        Assert.Equal("previous-token", tokens.GetUserToken(subject));
        Assert.Equal("/api/setup/repository/register", handler.Path);
    }

    [Theory]
    [InlineData("", "token")]
    [InlineData("subject", " ")]
    public async Task MissingIdentityOrTokenDoesNotCallTheApi(string subject, string token)
    {
        using var handler = new Handler(200, "{\"status\":\"Created\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example/") };
        await Assert.ThrowsAsync<ArgumentException>(() => new RepositorySignInService(new RepositoryAccountClient(http), new TokenProvider())
            .RegisterAsync(subject, token));
        Assert.Null(handler.Path);
    }

    private sealed class Handler(int code, string body, Action? beforeResponse = null) : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? Authorization { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Path = request.RequestUri!.AbsolutePath;
            Authorization = request.Headers.Authorization?.ToString();
            beforeResponse?.Invoke();
            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)code)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
