using Beep.OilandGas.Client.Authentication;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// The Beep Oil &amp; Gas client library's access token, for the Web: the signed-in person's own, from the identity
/// server's client library (refreshed when it is due), on this circuit.
/// </summary>
/// <remarks>
/// The client library signed in with a username and password of its own — a grant the identity server does not offer —
/// and without one sent no token at all, so every call it made was refused (S3-06 §3).
/// </remarks>
public sealed class OilGasUserTokenProvider(AuthenticationStateProvider authentication, IUserTokenManager tokens) : IAuthenticationProvider
{
    public async Task<string> GetAccessTokenAsync()
    {
        var state = await authentication.GetAuthenticationStateAsync();
        var token = await tokens.GetAccessTokenAsync(state.User);
        return token.WasSuccessful(out var user, out var failure)
            ? user.AccessToken.ToString()
            : throw new InvalidOperationException($"The signed-in person's access token is unavailable ({failure.Error}).");
    }

    /// <summary>The token manager refreshes a token when it is due, so asking for one again is the refresh.</summary>
    public async Task<bool> RefreshTokenAsync()
    {
        var state = await authentication.GetAuthenticationStateAsync();
        return (await tokens.GetAccessTokenAsync(state.User)).WasSuccessful(out _);
    }
}
