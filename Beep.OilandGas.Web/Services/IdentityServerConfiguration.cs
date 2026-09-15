using Microsoft.Extensions.Configuration;

namespace Beep.OilandGas.Web.Services;

public static class IdentityServerConfiguration
{
    public static string ResolveAuthority(IConfiguration configuration)
    {
        var value = new[]
        {
            configuration["services:identityserver:https:0"],
            configuration["Authentication:Schemes:OpenIdConnect:Authority"],
            configuration["IdentityServer:Authority"]
        }.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new InvalidOperationException("Configure an absolute HTTPS IdentityServer:Authority without credentials, query, or fragment.");
        }

        return uri.AbsoluteUri.TrimEnd('/') + "/";
    }
}
