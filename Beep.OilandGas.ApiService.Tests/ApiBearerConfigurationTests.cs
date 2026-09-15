using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ApiBearerConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://localhost:7062/")]
    [InlineData("/relative")]
    [InlineData("https://user:secret@issuer.example/")]
    [InlineData("https://issuer.example/?query=value")]
    [InlineData("https://issuer.example/#fragment")]
    public void InvalidAuthorityFailsDuringRegistrationInBothModes(string? authority)
    {
        foreach (var mode in new[] { "Jwt", "Introspection" })
        {
            var settings = Settings();
            settings["IdentityServer:Authority"] = authority;
            settings["IdentityServer:ValidationMode"] = mode;
            Assert.Throws<InvalidOperationException>(() => ApiBearerAuthentication.Configure(
                new ServiceCollection(), new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
        }
    }

    [Fact]
    public void BlankAudienceFailsBeforeStartup()
    {
        var settings = Settings();
        settings["IdentityServer:Audience"] = " ";
        Assert.Throws<InvalidOperationException>(() => ApiBearerAuthentication.Configure(
            new ServiceCollection(), new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
    }

    [Theory]
    [InlineData(null, "https://issuer.example/")]
    [InlineData("", "https://issuer.example/")]
    [InlineData("https://discovered.example/", "https://discovered.example/")]
    public void DefaultJwtKeepsHttpsAndTokenValidation(string? discovery, string expected)
    {
        var settings = Settings();
        settings["services:identityserver:https:0"] = discovery;
        var services = new ServiceCollection();
        services.AddLogging();
        var scheme = ApiBearerAuthentication.Configure(services,
            new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, scheme);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(scheme);
        Assert.Equal(expected, options.Authority);
        Assert.Equal("beep-api", options.Audience);
        Assert.True(options.RequireHttpsMetadata);
        Assert.Null(options.BackchannelHttpHandler);
        Assert.False(options.MapInboundClaims);
        Assert.False(options.SaveToken);
        Assert.False(options.IncludeErrorDetails);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
        Assert.True(options.TokenValidationParameters.RequireExpirationTime);
    }

    [Fact]
    public void InvalidDiscoveryDoesNotFallBackToConfiguredAuthority()
    {
        var settings = Settings();
        settings["services:identityserver:https:0"] = "http://untrusted.example/";
        Assert.Throws<InvalidOperationException>(() => ApiBearerAuthentication.Configure(
            new ServiceCollection(), new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
    }

    private static Dictionary<string, string?> Settings() => new()
    {
        ["IdentityServer:Authority"] = "https://issuer.example/",
        ["IdentityServer:Introspection:ClientId"] = "fixture",
        ["IdentityServer:Introspection:ClientSecret"] = "fixture-only"
    };
}
