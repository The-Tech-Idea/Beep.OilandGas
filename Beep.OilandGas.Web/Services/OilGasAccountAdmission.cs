using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Whether OilGas still admits a signed-in person — the API's answer about their account. The identity server's client
/// library asks it at sign-in, on the cookie and on an open circuit, and signs a refused person out.
/// </summary>
/// <remarks>
/// It replaced a revalidating state provider of the Web's own, which read tokens from a dictionary the library no longer
/// has; a refused account there only failed the circuit, and nothing refused its sign-in or its cookie. An API that could
/// not answer is <see cref="AccountAdmission.Unknown"/>: admitted for now and asked again, never refused on a blip.
/// </remarks>
public sealed class OilGasAccountAdmission(RepositoryAccountClient repository) : IAccountAdmission
{
    public async Task<AccountAdmission> EvaluateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken) =>
        (await repository.GetAccountAsync(principal, cancellationToken)).State switch
        {
            RepositoryAccountState.Active => AccountAdmission.Admitted,
            RepositoryAccountState.Refused => AccountAdmission.Refused,
            _ => AccountAdmission.Unknown
        };
}
