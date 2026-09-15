using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class IdentityServerConfigurationTests
{
    [Theory]
    [InlineData(null, null, "https://identity.example/auth", "https://identity.example/auth/")]
    [InlineData(null, "https://oidc.example/", "https://identity.example/", "https://oidc.example/")]
    [InlineData("https://discovery.example/", "https://oidc.example/", "https://identity.example/", "https://discovery.example/")]
    [InlineData(" ", "", "https://identity.example/", "https://identity.example/")]
    public void ResolvesConfiguredAuthority(string? discovery, string? scheme, string? authority, string expected)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["services:identityserver:https:0"] = discovery,
            ["Authentication:Schemes:OpenIdConnect:Authority"] = scheme,
            ["IdentityServer:Authority"] = authority
        }).Build();
        Assert.Equal(expected, IdentityServerConfiguration.ResolveAuthority(configuration));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/relative")]
    [InlineData("http://identity.example")]
    [InlineData("https://user:password@identity.example")]
    [InlineData("https://identity.example/?query=value")]
    [InlineData("https://identity.example/#fragment")]
    public void InvalidAuthorityFailsInsteadOfUsingLocalhost(string? authority)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IdentityServer:Authority"] = authority
        }).Build();
        Assert.Throws<InvalidOperationException>(() => IdentityServerConfiguration.ResolveAuthority(configuration));
    }
}
