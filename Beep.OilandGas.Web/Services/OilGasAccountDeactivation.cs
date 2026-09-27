using Microsoft.AspNetCore.Components.Authorization;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Switches the signed-in person's OilGas account off once the identity server has deleted their account — the Web's half
/// of the account page's deletion (<c>AccountManageTabs.OnAccountDeleted</c>) — and says beforehand when it must not.
/// Their records stay; they no longer sign in.
/// </summary>
/// <remarks>
/// A failure throws: the shared component catches it, logs it, and tells the person this site's records may remain — or,
/// for the question asked beforehand, deletes nothing. Deleting the identity used to leave the OilGas account active, and
/// navigated home without signing out (S3-06 §7).
/// </remarks>
public sealed class OilGasAccountDeactivation(AuthenticationStateProvider authentication, RepositoryAccountClient repository)
{
    /// <summary>Why the person may not delete their account (OilGas's only active administrator), or null.</summary>
    public async Task<string?> RefuseDeletionAsync()
    {
        var state = await authentication.GetAuthenticationStateAsync();
        return await repository.RefusalOfDeletionAsync(state.User);
    }

    public async Task DeactivateCurrentAsync()
    {
        var state = await authentication.GetAuthenticationStateAsync();
        await repository.DeactivateAsync(state.User);
    }
}
