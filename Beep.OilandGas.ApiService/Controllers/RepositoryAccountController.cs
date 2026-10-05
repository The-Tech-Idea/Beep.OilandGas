using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers;

/// <summary>
/// The caller's OilGas account: its id, whether it is active, its roles and permissions — what the Web reads to put the
/// repository's roles on its own sign-in and to refuse an account the repository has switched off.
/// </summary>
/// <remarks>
/// The account is the one the identity server's client library resolved for the token (provisioned at first sight; the
/// first administers). It looked the account up by the token's issuer and subject and answered 403 for an inactive account
/// — the same answer a caller the API could not resolve got, so "off" and "broken" could not be told apart. Now: an account
/// that is off or gone is 403 naming why; one that could not be resolved is 503.
/// </remarks>
[ApiController]
[Route("api/auth/repository")]
[Authorize(Policy = RepositoryAuthorization.SignedIn)]
public sealed class RepositoryAccountController(IRepositoryAccessService access, IFailureReporter failures) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<RepositoryUserAccess>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (User.IsMachine())
            return Problem("An application acting for itself has no OilGas account.",
                statusCode: StatusCodes.Status403Forbidden, title: RepositoryAccountRefusals.NotAPerson);

        var userId = User.FindActingUserId();
        if (userId is null)
            return Problem("Your OilGas account could not be resolved just now. Try again shortly.",
                statusCode: StatusCodes.Status503ServiceUnavailable, title: RepositoryAccountRefusals.Unresolved);

        var result = await access.GetAccessAsync(userId, cancellationToken);
        if (result is null)
            return Problem("This account no longer exists in OilGas.",
                statusCode: StatusCodes.Status403Forbidden, title: RepositoryAccountRefusals.Removed);
        if (!result.IsActive)
            return Problem("This account has been switched off in OilGas.",
                statusCode: StatusCodes.Status403Forbidden, title: RepositoryAccountRefusals.Deactivated);

        return Ok(result);
    }

    /// <summary>What the last active administrator is told when they try to delete their account.</summary>
    public const string LastAdministratorMayNotDelete =
        "You are OilGas's only active administrator. Make somebody else an administrator before you delete your account: " +
        "once OilGas has had an administrator, nobody becomes one by signing in.";

    /// <summary>
    /// Whether the caller may delete their account — asked by the Web's account page before the identity server deletes
    /// anything, since the identity goes first and OilGas could afterwards only refuse to switch its own account off.
    /// The last active administrator may not; <see cref="DeactivateMe"/> refuses them for the same reason.
    /// </summary>
    [HttpGet("me/deletion")]
    [Authorize]
    [ProducesResponseType<RepositoryAccountDeletion>(StatusCodes.Status200OK)]
    public async Task<IActionResult> MyDeletion([FromServices] RepositoryUserService users) =>
        Ok(new RepositoryAccountDeletion(
            await users.IsLastActiveAdministratorAsync(User.ActingUserId()) ? LastAdministratorMayNotDelete : null));

    /// <summary>
    /// Switches the caller's own OilGas account off — what the Web asks once the person has deleted their account at the
    /// identity server. Their records stay (engineering and financial history must); they no longer sign in. The last
    /// active administrator is refused (409), as an administrator switching them off would be.
    /// </summary>
    /// <remarks>
    /// Deleting the identity used to leave the OilGas account active (S3-06 §7). The action requires an active account —
    /// the API's default policy, on top of this controller's signed-in one.
    /// </remarks>
    [HttpPost("me/deactivate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivateMe([FromServices] RepositoryUserService users)
    {
        var userId = User.ActingUserId();
        var account = await users.GetByIdAsync(userId);
        if (account is null)
            return Problem("This account no longer exists in OilGas.",
                statusCode: StatusCodes.Status403Forbidden, title: RepositoryAccountRefusals.Removed);

        // The user service refuses the last active administrator, and a version changed since the read, as refusals the API's
        // handler answers (409) in its own words; the save itself losing a race is answered here.
        try
        {
            await users.UpdateAsync(userId, new RepositoryUserUpdate(account.FullName, false, account.ConcurrencyStamp));
            return NoContent();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException changed)
        {
            failures.ReportHandled(changed, "switching the caller's own account off",
                "the account stays active; the person is asked to try again", FailureSeverity.Degraded);
            return Problem("The account changed while it was being switched off. Try again.", statusCode: StatusCodes.Status409Conflict);
        }
    }
}
