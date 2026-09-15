using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ApiIntrospectionTests
{
    [Theory]
    [InlineData("active", HttpStatusCode.OK)]
    [InlineData("missing-issuer", HttpStatusCode.OK)]
    [InlineData("null-issuer", HttpStatusCode.InternalServerError)]
    [InlineData("expired", HttpStatusCode.Unauthorized)]
    [InlineData("malformed", HttpStatusCode.InternalServerError)]
    [InlineData("inactive", HttpStatusCode.Unauthorized)]
    [InlineData("wrong-audience", HttpStatusCode.Unauthorized)]
    [InlineData("wrong-issuer", HttpStatusCode.InternalServerError)]
    public async Task OpaqueTokensUseIntrospectionAndOnlyRepositoryRoles(string scenario, HttpStatusCode expected)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(Settings());
        var scheme = ApiBearerAuthentication.Configure(builder.Services, builder.Configuration);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = scheme;
            options.DefaultChallengeScheme = scheme;
        });
        builder.Services.AddAuthorization(RepositoryAuthorization.Configure);
        var reader = new Reader();
        builder.Services.AddSingleton<IRepositoryAccessService>(reader);
        builder.Services.AddScoped<IClaimsTransformation, RepositoryClaimsTransformation>();
        var metadata = new OpenIddictConfiguration
        {
            Issuer = new("https://issuer.example/"),
            IntrospectionEndpoint = new("https://issuer.example/connect/introspect")
        };
        metadata.IntrospectionEndpointAuthMethodsSupported.Add("client_secret_post");
        builder.Services.AddOpenIddict().AddValidation(options => options.SetConfiguration(metadata));
        var transport = new Factory(scenario);
        builder.Services.AddSingleton<IHttpClientFactory>(transport);
        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/claims", (ClaimsPrincipal user) => new
        {
            LocalId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            Reader = user.IsInRole("Reader"), Admin = user.IsInRole("Administrator")
        }).RequireAuthorization();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new(app.Urls.Single()) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "opaque-test-token");
            using var response = await client.GetAsync("/claims");
            Assert.Equal(expected, response.StatusCode);
            Assert.True(transport.Called);
            if (expected == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                Assert.Equal("local-user", result.GetProperty("localId").GetString());
                Assert.True(result.GetProperty("reader").GetBoolean());
                Assert.False(result.GetProperty("admin").GetBoolean());
                Assert.True(reader.Calls > 0);
            }
            else Assert.Equal(0, reader.Calls);
        }
        finally { await app.StopAsync(); }
    }

    [Theory]
    [InlineData("IdentityServer:ValidationMode", "unknown")]
    [InlineData("IdentityServer:Authority", "http://issuer.example/")]
    [InlineData("IdentityServer:Introspection:ClientId", "")]
    [InlineData("IdentityServer:Introspection:ClientSecret", "")]
    [InlineData("IdentityServer:Audience", "")]
    public void InvalidConfigurationDoesNotFallBackToJwt(string key, string value)
    {
        var settings = Settings();
        settings[key] = value;
        Assert.Throws<InvalidOperationException>(() => ApiBearerAuthentication.Configure(new ServiceCollection(),
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
    }

    private static Dictionary<string, string?> Settings() => new()
    {
        ["IdentityServer:ValidationMode"] = "Introspection",
        ["IdentityServer:Authority"] = "https://issuer.example/",
        ["IdentityServer:Audience"] = "beep-api",
        ["IdentityServer:Introspection:ClientId"] = "resource-client",
        ["IdentityServer:Introspection:ClientSecret"] = "fixture-only-secret"
    };

    private sealed class Reader : IRepositoryAccessService
    {
        public int Calls { get; private set; }
        public Task<RepositoryUserAccess?> GetAccessAsync(string issuer, string subject, CancellationToken cancellationToken = default)
        {
            Calls++;
            Assert.Equal("https://issuer.example/", issuer);
            Assert.Equal("external-user", subject);
            return Task.FromResult<RepositoryUserAccess?>(new("local-user", true, ["Reader"], []));
        }
    }

    private sealed class Factory(string scenario) : IHttpClientFactory
    {
        public bool Called { get; private set; }
        public HttpClient CreateClient(string name) => new(new Handler(async request =>
        {
            Called = true;
            Assert.Equal("https://issuer.example/connect/introspect", request.RequestUri!.AbsoluteUri);
            var form = await request.Content!.ReadAsStringAsync();
            Assert.Contains("token=opaque-test-token", form);
            Assert.Contains("client_id=resource-client", form);
            Assert.Contains("client_secret=fixture-only-secret", form);
            if (scenario == "malformed")
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { active = "not-a-boolean" }) };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new
            {
                active = scenario != "inactive", sub = "external-user",
                iss = scenario is "missing-issuer" or "null-issuer" ? null : scenario == "wrong-issuer" ? "https://other.example/" : "https://issuer.example/",
                aud = scenario == "wrong-audience" ? "other-api" : "beep-api",
                exp = DateTimeOffset.UtcNow.AddMinutes(scenario == "expired" ? -10 : 10).ToUnixTimeSeconds(),
                token_type = "Bearer", role = "Administrator"
            }, options: new System.Text.Json.JsonSerializerOptions
            {
                DefaultIgnoreCondition = scenario == "missing-issuer"
                    ? System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    : System.Text.Json.Serialization.JsonIgnoreCondition.Never
            }) };
        }));
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }
}
