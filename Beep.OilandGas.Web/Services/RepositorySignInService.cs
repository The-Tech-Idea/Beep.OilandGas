using Beep.Foundation.IdentityServer.Shared.Authentication;

namespace Beep.OilandGas.Web.Services;

public sealed class RepositorySignInService(RepositoryAccountClient repository, TokenProvider tokens)
{
    public async Task RegisterAsync(string subject, string accessToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        await repository.RegisterAsync(accessToken, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        tokens.SetUserToken(subject, accessToken);
    }
}
