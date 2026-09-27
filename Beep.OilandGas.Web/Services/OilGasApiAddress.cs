using Beep.IdentityServer.Contracts;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Where the OilGas API is (<c>ApiService:BaseUrl</c>) — read once, at startup, and handed to everything in the Web that
/// calls it: the typed clients and both hubs.
/// </summary>
/// <remarks>
/// Required and HTTPS. It fell back to <c>https://localhost:7001</c> in two places, so a deployment without the setting
/// started and sent every call — people's access tokens included — to a port on the web server itself.
/// </remarks>
public sealed class OilGasApiAddress(Uri address)
{
    public const string Key = "ApiService:BaseUrl";

    /// <summary>The API's base address, ending in a slash.</summary>
    public Uri Address { get; } = address;

    /// <summary>The address of <paramref name="path"/> on the API.</summary>
    public Uri For(string path) => new(Address, path);

    /// <exception cref="InvalidOperationException">The setting is missing, still a placeholder, or not an https URL.</exception>
    public static OilGasApiAddress Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var value = configuration[Key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{Key} is not configured. Set it to the OilGas API's https address.");
        }

        if (ConfigurationPlaceholder.IsPlaceholder(value))
        {
            throw new InvalidOperationException($"{Key} still holds a configuration-template placeholder. Replace it with the OilGas API's address.");
        }

        if (!Uri.TryCreate(value.TrimEnd('/') + "/", UriKind.Absolute, out var address) || address.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException($"{Key} '{value}' is not an absolute https URL. Set it to the OilGas API's address.");
        }

        return new OilGasApiAddress(address);
    }
}
