using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Setup;

namespace TheTechIdea.Beep.OilandGas.Desktop.Security;

/// <summary>The person a session was built for: the name they signed in with and the name they are shown as.</summary>
public sealed record OgSignedInUser(string UserName, string DisplayName);

/// <summary>A completed sign-in: the session the shell runs under, and the roles that sign-in granted.</summary>
public sealed record OgSignedInSession(
    OgSession Session,
    IReadOnlyList<string> Roles,
    bool MustChangePassword = false,
    IReadOnlyList<string>? Permissions = null)
{
    /// <summary>The permission codes the roles granted, never null.</summary>
    public IReadOnlyList<string> GrantedPermissions => Permissions ?? [];
}

/// <summary>
/// Sign-in with the audit trail: every attempt — accepted or refused — is recorded, and a successful one
/// produces the session the shell runs under (FND-07 AC-6 and AC-7).
/// </summary>
/// <remarks>
/// The refusal sentence is the identity service's own, so the audit detail never adds information the
/// window withheld.
/// </remarks>
public sealed class OgSignInService
{
    private readonly IOgAuditService _audit;
    private readonly OgRuntimeContext _context;

    public OgSignInService(IOgAuditService audit, OgRuntimeContext context)
    {
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Signs in for the deployment in force.
    /// </summary>
    /// <remarks>
    /// A company database is the account directory: the user is authenticated against that database's USER table
    /// and gets the roles its RBAC tables grant, before any company setting is read. A personal installation
    /// authenticates against this workstation's credential file. The sentence a refusal shows is the same for a
    /// wrong password and an unknown user, so the screen cannot enumerate accounts.
    /// </remarks>
    public async Task<OgResult<OgSignedInSession>> SignInAsync(
        string userName,
        string password,
        Data.Security.IOgDatabaseIdentity database,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        var result = await database.SignInAsync(userName, password, cancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Value is null)
        {
            _audit.Add(
                "sign-in",
                "user",
                string.IsNullOrWhiteSpace(userName) ? "(empty)" : userName,
                "refused",
                "the database did not accept the user name or the password");

            return OgResult<OgSignedInSession>.Failed(
                result.Message ?? "Sign-in failed.",
                result.Reference ?? Data.Security.OgDatabaseIdentityCodes.Refused);
        }

        var user = result.Value;

        // The company values are adopted from the database after this returns; the session starts with the
        // installation's defaults so nothing is invented from a local file.
        var session = BuildSession(new OgSignedInUser(user.UserName, user.DisplayName), setup: null, user.Roles);

        _context.UserId = user.UserId;
        _context.CompanyId = session.CompanyId;
        _context.FieldId = session.FieldId;

        _audit.Add(
            "sign-in",
            "user",
            user.UserName,
            "ok",
            $"database account, roles: {(user.Roles.Count == 0 ? "none" : string.Join(", ", user.Roles))}");

        return OgResult<OgSignedInSession>.Ok(new OgSignedInSession(
            session,
            user.Roles,
            user.MustChangePassword,
            user.GrantedPermissions));
    }

    /// <summary>Records a sign-out. The session itself is discarded by the caller.</summary>
    public void SignOut(OgSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _audit.Add("sign-out", "user", session.UserId, "ok", $"session for {session.CompanyName}");
        _context.UserId = null;
    }

    /// <summary>
    /// Builds the session for the local administrator: the installed company, the default field and an
    /// in-progress period. IdentityServer deployments replace this with the token's claims.
    /// </summary>
    public static OgSession BuildSession(OgSignedInUser user, OgSetupState? setup) =>
        BuildSession(user, setup, roles: null);

    /// <summary>
    /// Builds the session for a signed-in person. <paramref name="roles"/> is the company database's own grant
    /// (null for the local account, which is the system administrator of this workstation).
    /// </summary>
    public static OgSession BuildSession(
        OgSignedInUser user,
        OgSetupState? setup,
        IReadOnlyList<string>? roles) =>
        new(
            connectionName: setup?.ConnectionName ?? OgPaths.ConnectionName,
            companyId: OgPaths.DefaultCompanyId,
            companyName: string.IsNullOrWhiteSpace(setup?.CompanyName)
                ? OgPaths.DefaultCompanyName
                : setup!.CompanyName,
            fieldId: OgPaths.DefaultFieldId,
            fieldName: string.IsNullOrWhiteSpace(setup?.DefaultFieldName)
                ? OgPaths.DefaultFieldName
                : setup!.DefaultFieldName,
            userId: user.UserName,
            userDisplayName: user.DisplayName,
            cultureName: string.IsNullOrWhiteSpace(setup?.CultureName) ? "en-US" : setup!.CultureName,
            timeZoneId: string.IsNullOrWhiteSpace(setup?.TimeZoneId)
                ? TimeZoneInfo.Local.Id
                : setup!.TimeZoneId,
            baseCurrencyCode: string.IsNullOrWhiteSpace(setup?.BaseCurrencyCode)
                ? "USD"
                : setup!.BaseCurrencyCode,
            bookCode: OgPaths.DefaultBookCode,
            periodId: "PERIOD-0001",
            periodName: "Current period",
            mode: setup?.Deployment ?? OgDeploymentMode.Personal,
            personaCode: OgPersonaLanding.PersonaFor(
                roles ?? [OgRoleNames.SystemAdmin]));

    /// <summary>
    /// The same session with the installation's settings re-read: who is signed in does not change, but the
    /// company, field, culture, currency and data path are the values the shared record now holds.
    /// </summary>
    public static OgSession Adopt(OgSession session, OgSetupState? setup)
    {
        ArgumentNullException.ThrowIfNull(session);

        return new OgSession(
            connectionName: setup?.ConnectionName ?? session.ConnectionName,
            companyId: session.CompanyId,
            companyName: string.IsNullOrWhiteSpace(setup?.CompanyName) ? session.CompanyName : setup!.CompanyName,
            fieldId: session.FieldId,
            fieldName: string.IsNullOrWhiteSpace(setup?.DefaultFieldName) ? session.FieldName : setup!.DefaultFieldName,
            userId: session.UserId,
            userDisplayName: session.UserDisplayName,
            cultureName: string.IsNullOrWhiteSpace(setup?.CultureName) ? session.CultureName : setup!.CultureName,
            timeZoneId: string.IsNullOrWhiteSpace(setup?.TimeZoneId) ? session.TimeZoneId : setup!.TimeZoneId,
            baseCurrencyCode: string.IsNullOrWhiteSpace(setup?.BaseCurrencyCode)
                ? session.BaseCurrencyCode
                : setup!.BaseCurrencyCode,
            bookCode: session.BookCode,
            periodId: session.PeriodId,
            periodName: session.PeriodName,
            mode: setup?.Deployment ?? session.Mode,
            personaCode: session.PersonaCode);
    }
}
