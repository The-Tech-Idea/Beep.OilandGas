using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Validation;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>
/// The OilGas repository's half of the identity server's "the application's key, never the raw <c>sub</c>" resolution:
/// the storage questions the client library's <see cref="PartyIdResolver{TKey}"/> asks, answered from the repository's
/// Identity users and their external logins. The key is the OilGas user's id (<see cref="OilGasUser"/>).
/// </summary>
/// <remarks>
/// <para>
/// An account is found by the identity server's subject under a login whose provider names the one issuer this API
/// trusts (<see cref="RepositoryBootstrapService.ExternalLoginProvider"/>) — <c>iss</c> and <c>sub</c> together identify
/// somebody (OIDC Core §5.7). The library owns the rest: an address the issuer verified and exactly one account holds,
/// relinked to a new subject after the identity server re-mints one; otherwise a new account, the first of which
/// administers (<see cref="RepositoryBootstrapService"/>). The API resolved by issuer and subject alone before, so a
/// re-minted subject arrived as a stranger and its owner's account was unreachable (S3-06 §6).
/// </para>
/// <para>A store that cannot answer throws; the library logs it and leaves the request without a key, which the API's
/// fallback policy refuses.</para>
/// </remarks>
public sealed class RepositoryCanonicalUserStore(
    RepositoryDbContext context,
    UserManager<OilGasUser> users,
    RepositoryBootstrapService bootstrap,
    IOptions<OpenIddictValidationOptions> validation) : ICanonicalUserStore<string>
{
    public async Task<(bool Found, string Key)> TryFindByCurrentSubjectAsync(string subject, CancellationToken cancellationToken)
    {
        var provider = Provider();
        var matches = await context.UserLogins.AsNoTracking()
            .Where(login => login.LoginProvider == provider && login.ProviderKey == subject)
            .Select(login => new { login.UserId, login.LoginProvider, login.ProviderKey })
            .ToListAsync(cancellationToken);

        // External subjects are opaque; database collation and padding must not alias them.
        var exact = matches
            .Where(login => string.Equals(login.LoginProvider, provider, StringComparison.Ordinal)
                            && string.Equals(login.ProviderKey, subject, StringComparison.Ordinal))
            .ToList();

        return exact.Count switch
        {
            0 => (false, string.Empty),
            1 => (true, exact[0].UserId),
            _ => throw new InvalidOperationException(
                $"{exact.Count} OilGas accounts hold the identity server's subject; which one it names cannot be known.")
        };
    }

    public async Task<IReadOnlyList<string>> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var normalized = users.NormalizeEmail(email);
        return await context.Users.AsNoTracking()
            .Where(user => user.NormalizedEmail == normalized)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateCurrentSubjectAsync(string key, string newSubject, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(key)
            ?? throw new InvalidOperationException($"No OilGas account '{key}' to relink.");
        var provider = Provider();

        foreach (var login in (await users.GetLoginsAsync(user)).Where(login => login.LoginProvider == provider).ToList())
        {
            Require(await users.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey), "remove the previous sign-in");
        }

        Require(await users.AddLoginAsync(user, new UserLoginInfo(provider, newSubject, "OIDC")), "link the new sign-in");
    }

    public async Task<string> ProvisionAsync(string subject, string? email, string? displayName, CancellationToken cancellationToken)
    {
        // The library passes only an address the issuer verified (AOR-SDK-12).
        var outcome = await bootstrap.BootstrapAsync(
            Issuer(), subject, cancellationToken, new ExternalRegistrationProfile(displayName, email, EmailVerified: email is not null));

        if (outcome == BootstrapOutcome.NotAllowed)
        {
            throw new InvalidOperationException(
                "The OilGas repository refused to create an account for this sign-in: the account exists and is off, or the "
                + "repository has accounts but no record of its first administrator and needs an operator.");
        }

        var (found, key) = await TryFindByCurrentSubjectAsync(subject, cancellationToken);
        return found
            ? key
            : throw new InvalidOperationException("The OilGas account was created but cannot be found by its sign-in.");
    }

    public async Task UpdateProfileAsync(string key, string? verifiedEmail, string? displayName, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(key)
            ?? throw new InvalidOperationException($"No OilGas account '{key}' to bring up to date.");

        // Only a verified address is kept (AOR-SDK-12): one nobody vouched for is one a re-minted subject could be matched to.
        user.Email = verifiedEmail;
        user.NormalizedEmail = verifiedEmail is null ? null : users.NormalizeEmail(verifiedEmail);
        user.EmailConfirmed = verifiedEmail is not null;
        Require(await users.UpdateAsync(user), "keep the account's address");

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            var extension = await context.Set<AppUserExtension>().SingleOrDefaultAsync(row => row.UserId == key, cancellationToken);
            if (extension is not null)
            {
                extension.FullName = displayName.Trim();
                extension.ChangedUtc = DateTime.UtcNow;
                extension.ChangedBy = key;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// The issuer this API accepts tokens from — the one the identity server's client library set on OpenIddict's
    /// validation (<c>IdentityServer:Authority</c>), so the accounts' logins and the tokens' <c>iss</c> cannot disagree.
    /// </summary>
    private string Issuer() => validation.Value.Issuer?.AbsoluteUri
        ?? throw new InvalidOperationException("Token validation has no issuer; IdentityServer:Authority is not configured.");

    private string Provider() => RepositoryBootstrapService.ExternalLoginProvider(Issuer());

    private static void Require(IdentityResult result, string attempted)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"The OilGas repository could not {attempted}: "
                + string.Join(", ", result.Errors.Select(error => error.Code)));
        }
    }
}
