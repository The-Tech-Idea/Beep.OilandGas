using System.Net.Http.Headers;
using System.Security.Cryptography;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation;

namespace Beep.OilandGas.ApiService.Tests.Infrastructure;

/// <summary>
/// The API's own identity wiring (<see cref="ApiIdentity.AddOilGasApiIdentity"/>) against a test issuer whose signing key
/// the host trusts, so a test drives the real token validation, <c>party_id</c> resolution and roles — never a header or a
/// hand-built principal standing in for them.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> the issuer is a fixed configuration and a test key (the identity server's discovery document and key
/// set are not fetched), as in the identity server's client library's own tests.
/// </remarks>
public static class SignedTokens
{
    public const string Authority = "https://idp.oilgas.test/";
    public const string Audience = "https://oilgas-api.test/api";
    public const string WebClient = "oilgas-web";

    private static readonly RsaSecurityKey SigningKey = new(RSA.Create(2048)) { KeyId = "oilgas-api-tests" };

    /// <summary>The API's identity, as <c>Program.cs</c> registers it, trusting the test issuer.</summary>
    public static WebApplicationBuilder AddApiIdentity(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IdentityServer:Authority"] = Authority,
            ["IdentityServer:Audience"] = Audience
        });
        builder.Services.AddOilGasApiIdentity(builder.Configuration);

        // The API reports through its failure store (AddOilGasDiagnostics), and the identity server's client library does
        // not start without a reporter; a test host has no store, so it records instead.
        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<
            TheTechIdeaWeb.Diagnostics.IFailureReporter, RecordingFailureReporter>(builder.Services);
        builder.Services.Configure<OpenIddictValidationOptions>(options =>
        {
            var configuration = new OpenIddictConfiguration { Issuer = new Uri(Authority) };
            configuration.SigningKeys.Add(SigningKey);
            options.Configuration = configuration;
        });
        return builder;
    }

    /// <summary>A person's access token from the Web, for this API. <paramref name="extra"/> adds or replaces claims.</summary>
    public static string Person(string subject, string? email = null, bool emailVerified = true, string? name = null,
        string audience = Audience, params (string Type, object Value)[] extra)
    {
        var claims = new Dictionary<string, object> { ["sub"] = subject, ["client_id"] = WebClient };
        if (email is not null)
        {
            claims["email"] = email;
            claims["email_verified"] = emailVerified;
        }

        if (name is not null)
        {
            claims["name"] = name;
        }

        foreach (var (type, value) in extra)
        {
            claims[type] = value;
        }

        return Create(audience, claims);
    }

    /// <summary>An application acting for itself (client credentials): its subject is its client id (RFC 9068).</summary>
    public static string Machine(string clientId) =>
        Create(Audience, new Dictionary<string, object> { ["sub"] = clientId, ["client_id"] = clientId });

    /// <summary>Sends <paramref name="token"/> on every later request, or nothing when it is null.</summary>
    public static void SignIn(this HttpClient client, string? token) =>
        client.DefaultRequestHeaders.Authorization = token is null ? null : new AuthenticationHeaderValue("Bearer", token);

    private static string Create(string audience, IDictionary<string, object> claims) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            TokenType = "at+jwt",
            Issuer = Authority,
            Audience = audience,
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.RsaSha256)
        });
}
