using System.Security.Cryptography;
using System.Text;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Security;

/// <summary>One account of the company database, with the roles the RBAC tables grant it.</summary>
public sealed record OgDatabaseUser(
    string UserId,
    string UserName,
    string DisplayName,
    string TenantId,
    string BusinessAssociateId,
    IReadOnlyList<string> Roles,
    bool MustChangePassword = false,
    IReadOnlyList<string>? Permissions = null)
{
    /// <summary>The permission codes granted through the roles, never null.</summary>
    public IReadOnlyList<string> GrantedPermissions => Permissions ?? [];
}

/// <summary>
/// Signs a user in against the company database's own account directory.
/// </summary>
/// <remarks>
/// <para>
/// This is the order the product runs in: the workstation knows <i>which server</i> before anyone signs in, and
/// nothing else. The account directory, the roles and the company settings are all in that database, so the user
/// is authenticated there first, and the settings are read afterwards with that identity — never before it.
/// </para>
/// <para>
/// The password is stored as PBKDF2-SHA256 (210 000 iterations, a 16-byte salt), the same parameters the local
/// credential store uses, in the form <c>pbkdf2-sha256$iterations$salt$hash</c>. A principal the security module
/// seeded carries <c>seeded-no-login</c> and can never verify, which is correct: those are principals for
/// ownership, not login accounts.
/// </para>
/// </remarks>
public interface IOgDatabaseIdentity
{
    /// <summary>True when the database has at least one login account.</summary>
    Task<OgResult<bool>> HasAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The user names of the accounts that can sign in — names only, never a display name, an address or a hash.
    /// The sign-in window offers them so a person on a shared workstation does not have to remember the exact
    /// spelling; a typed name is still accepted.
    /// </summary>
    Task<OgResult<IReadOnlyList<string>>> ListUserNamesAsync(CancellationToken cancellationToken = default);

    Task<OgResult<OgDatabaseUser>> SignInAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the first system administrator in the company database: the USER row with a real password hash and
    /// the administrator role from the RBAC tables the security module seeded.
    /// </summary>
    Task<OgResult<OgDatabaseUser>> CreateSystemAdministratorAsync(
        string userName,
        string displayName,
        string password,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an account and its role grants. Used only to undo a company creation that failed part-way, so the
    /// database is not left with an administrator nobody can finish setting up.
    /// </summary>
    Task<OgResult> DeleteAccountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the account's password and clears the setup's marker, so the password somebody typed during setup
    /// cannot be used again.
    /// </summary>
    Task<OgResult> ChangePasswordAsync(
        string userName,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an account as provisional: it may administer, but its owner must replace the password at the next
    /// sign-in. Written by the setup that creates the account; cleared by <see cref="ChangePasswordAsync"/>.
    /// </summary>
    Task<OgResult> MarkPasswordChangeRequiredAsync(
        string userName,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>The result codes the sign-in screen branches on.</summary>
public static class OgDatabaseIdentityCodes
{
    public const string Refused = "sign-in-refused";

    /// <summary>The account is locked for a stated time; only its owner is told, after proving the password.</summary>
    public const string Locked = "sign-in-locked";
    public const string NoAccounts = "sign-in-no-accounts";
    public const string TablesMissing = "sign-in-security-tables-missing";
    public const string AdministratorExists = "administrator-exists";
    public const string RoleMissing = "administrator-role-missing";
    public const string PasswordRefused = "password-policy";
    public const string Unreachable = "sign-in-database-unreachable";
    public const string WriteFailed = "sign-in-write-failed";
}

/// <inheritdoc />
public sealed class BeepDmDatabaseIdentity : IOgDatabaseIdentity
{
    private const int Iterations = 210_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    /// <summary>The seeded system principal's business associate, reused so the first administrator is not an orphan.</summary>
    private const string SystemBusinessAssociateId = "BEEP_USER_SYSTEM";

    private readonly OgBeepHost _host;
    private readonly IFailureReporter _failures;

    public BeepDmDatabaseIdentity(OgBeepHost host, IFailureReporter failures, string connectionName)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        ConnectionName = connectionName;
    }

    public string ConnectionName { get; }

    public async Task<OgResult<bool>> HasAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = OpenSource();
            if (source is null)
            {
                return OgResult<bool>.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            if (!HasTable(source, nameof(USER)))
            {
                return OgResult<bool>.Failed(
                    "This database has no account tables yet: the system administrator has not prepared its security.",
                    OgDatabaseIdentityCodes.TablesMissing);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);

            return OgResult<bool>.Ok(users.Any(user => CanSignIn(user)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the account directory of the company database",
                "the account directory is unavailable",
                FailureSeverity.Error);

            return OgResult<bool>.Failed(
                $"The account directory could not be read. Reference: {reference}",
                OgDatabaseIdentityCodes.Unreachable);
        }
    }

    public async Task<OgResult<IReadOnlyList<string>>> ListUserNamesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = OpenSource();
            if (source is null)
            {
                return OgResult<IReadOnlyList<string>>.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            if (!HasTable(source, nameof(USER)))
            {
                return OgResult<IReadOnlyList<string>>.Failed(
                    "This database has no account directory yet.",
                    OgDatabaseIdentityCodes.TablesMissing);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            // Names only, and only accounts that could actually sign in: a seeded principal or a disabled account
            // would be an entry that always fails.
            IReadOnlyList<string> names = users
                .Where(CanSignIn)
                .Select(user => string.IsNullOrWhiteSpace(user.USERNAME) ? user.USER_NAME : user.USERNAME)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return OgResult<IReadOnlyList<string>>.Ok(names);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing the user names of the account directory",
                "the sign-in window offers no names",
                FailureSeverity.Degraded);

            return OgResult<IReadOnlyList<string>>.Failed(
                $"The account names could not be listed. Reference: {reference}",
                OgDatabaseIdentityCodes.Unreachable);
        }
    }

    public async Task<OgResult<OgDatabaseUser>> SignInAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = OpenSource();
            if (source is null)
            {
                return OgResult<OgDatabaseUser>.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            if (!HasTable(source, nameof(USER)))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    "This database has no account tables yet, so nobody can sign in to it.",
                    OgDatabaseIdentityCodes.TablesMissing);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);

            var matches = users
                .Where(user => string.Equals(user.USERNAME, userName?.Trim(), StringComparison.OrdinalIgnoreCase)
                               || string.Equals(user.USER_NAME, userName?.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            // One sentence for "no such user", "wrong password" and "not a login account": the screen must not
            // enumerate accounts, and a principal is not an account.
            if (matches.Count != 1 || !CanSignIn(matches[0]))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    "The user name or the password was not accepted.",
                    OgDatabaseIdentityCodes.Refused);
            }

            var candidate = matches[0];
            var now = DateTimeOffset.UtcNow;
            var lockedUntil = candidate.LOCKED_IND == "Y" ? AsUtc(candidate.LOCKOUT_UNTIL_UTC) : null;
            var locked = OgSignInPolicy.IsLocked(lockedUntil, now);
            var verified = Verify(password, candidate.PASSWORD_HASH);

            if (locked)
            {
                if (!verified)
                {
                    // A wrong password on a locked account is refused generically and does not extend the lock: an
                    // attacker must not be able to keep an account locked for ever by guessing at it.
                    return OgResult<OgDatabaseUser>.Failed(
                        "The user name or the password was not accepted.",
                        OgDatabaseIdentityCodes.Refused);
                }

                // The owner proved the password; telling them the account is locked is actionable without
                // revealing anything to somebody who did not.
                return OgResult<OgDatabaseUser>.Failed(
                    OgSignInPolicy.LockedSentence(lockedUntil!.Value, TimeZoneInfo.Local.Id),
                    OgDatabaseIdentityCodes.Locked);
            }

            if (!verified)
            {
                await RecordFailureAsync(source, candidate, now, cancellationToken).ConfigureAwait(false);

                return OgResult<OgDatabaseUser>.Failed(
                    "The user name or the password was not accepted.",
                    OgDatabaseIdentityCodes.Refused);
            }

            // A correct password resets the count and clears an expired lock.
            await RecordSuccessAsync(source, candidate, now, cancellationToken).ConfigureAwait(false);

            var user = matches[0];
            var roles = await ReadRolesAsync(source, user.USER_ID, cancellationToken).ConfigureAwait(false);
            var permissions = await ReadPermissionsAsync(source, user.USER_ID, cancellationToken).ConfigureAwait(false);

            var provisional = await IsProvisionalAsync(source, user, cancellationToken).ConfigureAwait(false);

            return OgResult<OgDatabaseUser>.Ok(new OgDatabaseUser(
                user.USER_ID,
                string.IsNullOrWhiteSpace(user.USERNAME) ? user.USER_NAME : user.USERNAME,
                string.IsNullOrWhiteSpace(user.FULL_NAME) ? user.USER_NAME : user.FULL_NAME,
                user.TENANT_ID ?? string.Empty,
                user.BUSINESS_ASSOCIATE_ID ?? string.Empty,
                roles,
                provisional,
                permissions));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"signing in '{userName}' against the company database",
                "the sign-in did not complete",
                FailureSeverity.Error);

            return OgResult<OgDatabaseUser>.Failed(
                $"Sign-in did not complete. Reference: {reference}",
                OgDatabaseIdentityCodes.Unreachable);
        }
    }

    public async Task<OgResult<OgDatabaseUser>> CreateSystemAdministratorAsync(
        string userName,
        string displayName,
        string password,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    "A user name is required.",
                    OgDatabaseIdentityCodes.Refused);
            }

            if (OgPasswordPolicy.Problems(password).Count > 0)
            {
                return OgResult<OgDatabaseUser>.Failed(
                    string.Join(" ", OgPasswordPolicy.Problems(password)),
                    OgDatabaseIdentityCodes.PasswordRefused);
            }

            var source = OpenSource();
            if (source is null)
            {
                return OgResult<OgDatabaseUser>.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            if (!HasTable(source, nameof(USER)) || !HasTable(source, nameof(ROLE)) || !HasTable(source, nameof(USER_ROLE)))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    "The security tables (USER, ROLE, USER_ROLE) are not in this database, so no administrator can be created in it.",
                    OgDatabaseIdentityCodes.TablesMissing);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);

            if (users.Any(user =>
                    string.Equals(user.USERNAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(user.USER_NAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    $"A user named '{userName.Trim()}' already exists in this database.",
                    OgDatabaseIdentityCodes.AdministratorExists);
            }

            var roles = await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false);

            var administratorRole = roles.FirstOrDefault(role =>
                                        string.Equals(role.ROLE_NAME, "Administrator", StringComparison.OrdinalIgnoreCase))
                                    ?? roles.FirstOrDefault(role =>
                                        string.Equals(role.ROLE_NAME, "Admin", StringComparison.OrdinalIgnoreCase))
                                    ?? roles.FirstOrDefault(role =>
                                        string.Equals(role.ROLE_NAME, OgRoleNames.SystemAdmin, StringComparison.OrdinalIgnoreCase));

            if (administratorRole is null)
            {
                return OgResult<OgDatabaseUser>.Failed(
                    "The database has no administrator role yet. Run the security module first.",
                    OgDatabaseIdentityCodes.RoleMissing);
            }

            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid().ToString("N");

            var userUnit = OgUow.For<USER>(source);
            userUnit.PrimaryKey = nameof(USER.USER_ID);
            userUnit.Add(new USER
            {
                USER_ID = userId,
                USERNAME = userName.Trim(),
                USER_NAME = string.IsNullOrWhiteSpace(displayName) ? userName.Trim() : displayName.Trim(),
                FULL_NAME = displayName.Trim(),
                EMAIL = string.Empty,
                PASSWORD_HASH = Hash(password),
                IS_ACTIVE = true,
                TENANT_ID = string.Empty,
                BUSINESS_ASSOCIATE_ID = SystemBusinessAssociateId,
                FAILED_LOGIN_COUNT = 0,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor,
                ROW_CHANGED_DATE = now,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
            });

            var committed = await userUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    OgUow.Message(committed, "The administrator account was not written."),
                    OgDatabaseIdentityCodes.WriteFailed);
            }

            var roleUnit = OgUow.For<USER_ROLE>(source);
            roleUnit.PrimaryKey = nameof(USER_ROLE.USER_ROLE_ID);
            roleUnit.Add(new USER_ROLE
            {
                USER_ROLE_ID = Guid.NewGuid().ToString("N"),
                USER_ID = userId,
                ROLE_ID = administratorRole.ROLE_ID,
                BUSINESS_ASSOCIATE_ID = SystemBusinessAssociateId,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor,
                ROW_CHANGED_DATE = now,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
            });

            var granted = await roleUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(granted))
            {
                return OgResult<OgDatabaseUser>.Failed(
                    OgUow.Message(granted, "The administrator's role was not written."),
                    OgDatabaseIdentityCodes.WriteFailed);
            }

            return OgResult<OgDatabaseUser>.Ok(new OgDatabaseUser(
                userId,
                userName.Trim(),
                string.IsNullOrWhiteSpace(displayName) ? userName.Trim() : displayName.Trim(),
                string.Empty,
                SystemBusinessAssociateId,
                [administratorRole.ROLE_NAME]));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "creating the system administrator in the company database",
                "no administrator was created",
                FailureSeverity.Error);

            return OgResult<OgDatabaseUser>.Failed(
                $"The administrator could not be created. Reference: {reference}",
                OgDatabaseIdentityCodes.WriteFailed);
        }
    }

    public async Task<OgResult> DeleteAccountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return OgResult.Ok();
            }

            var source = OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            if (HasTable(source, nameof(USER_ROLE)))
            {
                var roleUnit = OgUow.For<USER_ROLE>(source);
                roleUnit.PrimaryKey = nameof(USER_ROLE.USER_ROLE_ID);

                var grants = await OgUow.ReadAllAsync<USER_ROLE>(source).ConfigureAwait(false);

                foreach (var grant in grants.Where(grant =>
                             string.Equals(grant.USER_ID, userId, StringComparison.OrdinalIgnoreCase)))
                {
                    roleUnit.Delete(grant);
                }

                var grantsCommitted = await roleUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(grantsCommitted))
                {
                    return OgResult.Failed(
                        OgUow.Message(grantsCommitted, "The account's roles were not removed."),
                        OgDatabaseIdentityCodes.WriteFailed);
                }
            }

            if (!HasTable(source, nameof(USER)))
            {
                return OgResult.Ok();
            }

            var userUnit = OgUow.For<USER>(source);
            userUnit.PrimaryKey = nameof(USER.USER_ID);

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);
            var row = users.FirstOrDefault(user =>
                string.Equals(user.USER_ID, userId, StringComparison.OrdinalIgnoreCase));

            if (row is null)
            {
                // Already gone: a retried rollback is the same rollback.
                return OgResult.Ok();
            }

            var deleted = userUnit.Delete(row);

            if (!OgUow.IsOk(deleted))
            {
                return OgResult.Failed(
                    OgUow.Message(deleted, "The account was not removed."),
                    OgDatabaseIdentityCodes.WriteFailed);
            }

            var committed = await userUnit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok()
                : OgResult.Failed(
                    OgUow.Message(committed, "The account was not removed."),
                    OgDatabaseIdentityCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "removing the administrator created by a failed setup",
                "the account was left in the company database",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The account could not be removed. Reference: {reference}",
                OgDatabaseIdentityCodes.WriteFailed);
        }
    }

    /// <summary>True when the setup marked this account's password as somebody else's to replace.</summary>
    private async Task<bool> IsProvisionalAsync(
        IDataSource source,
        USER user,
        CancellationToken cancellationToken)
    {
        var marker = await ReadMarkerAsync(source, cancellationToken).ConfigureAwait(false);

        return marker is { MustChangePassword: true }
               && string.Equals(marker.UserName, user.USERNAME, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The setup's marker row, or null when there is none this build can read.</summary>
    internal async Task<OgBootstrapMarker?> ReadMarkerAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        if (!HasTable(source, BEEPSETTINGS.TableName))
        {
            return null;
        }

        var unit = OgUow.For<BEEPSETTINGS>(source);
        unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

        var rows = await OgUow
            .ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, BEEPSETTINGS.Bootstrap)], cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count != 1)
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<OgBootstrapMarker>(rows[0].SETTINGS_JSON);
        }
        catch (System.Text.Json.JsonException ex)
        {
            // A marker this build cannot read is treated as absent: the account signs in normally and the malformed
            // row is recorded rather than blocking the administrator out of the database.
            _failures.ReportHandled(
                ex,
                "reading the setup's password-change marker",
                "the account signs in without being asked to change its password",
                FailureSeverity.Degraded);

            return null;
        }
    }

    public async Task<OgResult> MarkPasswordChangeRequiredAsync(
        string userName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            if (!HasTable(source, BEEPSETTINGS.TableName))
            {
                return OgResult.Failed(
                    $"The database has no {BEEPSETTINGS.TableName} table, so the marker cannot be written.",
                    OgSharedSetupCodes.TableMissing);
            }

            var existing = await ReadMarkerRowAsync(source, cancellationToken).ConfigureAwait(false);

            var marker = new OgBootstrapMarker(userName.Trim(), MustChangePassword: true, DateTimeOffset.UtcNow);

            var unit = OgUow.For<BEEPSETTINGS>(source);
            unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

            var row = existing ?? new BEEPSETTINGS
            {
                SETTINGS_ID = BEEPSETTINGS.Bootstrap,
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = DateTime.UtcNow,
            };

            row.SETTINGS_JSON = System.Text.Json.JsonSerializer.Serialize(marker);
            row.REVISION = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{BEEPSETTINGS.Bootstrap}\u001f{existing?.REVISION}\u001f{DateTime.UtcNow:O}\u001f{row.SETTINGS_JSON}")));
            row.PREVIOUS_REVISION = existing?.REVISION;
            row.INTENT_ID = Guid.NewGuid().ToString("N");
            row.UPDATED_BY = actor;
            row.UPDATED_UTC = DateTime.UtcNow;
            row.ACTIVE_IND = "Y";
            row.ROW_CHANGED_BY = actor;
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            if (existing is null)
            {
                unit.Add(row);
            }
            else
            {
                var updated = unit.Update(row);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult.Failed(
                        OgUow.Message(updated, "The marker was not written."),
                        OgDatabaseIdentityCodes.WriteFailed);
                }
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok()
                : OgResult.Failed(
                    OgUow.Message(committed, "The marker was not written."),
                    OgDatabaseIdentityCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"marking '{userName}' as provisional",
                "the account is not asked to change its password",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The marker could not be written. Reference: {reference}",
                OgDatabaseIdentityCodes.WriteFailed);
        }
    }

    public async Task<OgResult> ChangePasswordAsync(
        string userName,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return OgResult.Failed("An account is required.", OgDatabaseIdentityCodes.Refused);
            }

            var problems = OgPasswordPolicy.Problems(newPassword);

            if (problems.Count > 0)
            {
                return OgResult.Failed(
                    string.Join(" ", problems),
                    OgDatabaseIdentityCodes.PasswordRefused);
            }

            var source = OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);
            var user = users.FirstOrDefault(row =>
                string.Equals(row.USERNAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(row.USER_NAME, userName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                return OgResult.Failed("The account was not found.", OgDatabaseIdentityCodes.Refused);
            }

            var unit = OgUow.For<USER>(source);
            unit.PrimaryKey = nameof(USER.USER_ID);

            // The unit must hold the row it updates: Update() locates the record by primary key through its own
            // buffer, so a row read by another unit is reported as "Object not found" and the write is a no-op.
            var tracked = await OgUow
                .ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, user.USER_ID)], cancellationToken)
                .ConfigureAwait(false);

            if (tracked.Count != 1)
            {
                return OgResult.Failed(
                    "The account could not be read back for the change.",
                    OgDatabaseIdentityCodes.WriteFailed);
            }

            tracked[0].PASSWORD_HASH = Hash(newPassword);
            tracked[0].LAST_PASSWORD_CHANGE_UTC = DateTime.UtcNow;
            tracked[0].ROW_CHANGED_BY = userName.Trim();
            tracked[0].ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(tracked[0]);

            if (!OgUow.IsOk(updated))
            {
                return OgResult.Failed(
                    OgUow.Message(updated, "The password was not changed."),
                    OgDatabaseIdentityCodes.WriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult.Failed(
                    OgUow.Message(committed, "The password was not changed."),
                    OgDatabaseIdentityCodes.WriteFailed);
            }

            // The marker is cleared only after the new hash is committed: a failure here leaves the requirement in
            // place, which is the safe direction (the person is asked again rather than never).
            await ClearMarkerAsync(source, userName, cancellationToken).ConfigureAwait(false);

            return OgResult.Ok("The password was changed.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"changing the password of '{userName}'",
                "the password was not changed",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The password could not be changed. Reference: {reference}",
                OgDatabaseIdentityCodes.WriteFailed);
        }
    }

    /// <summary>The marker row itself, for an update rather than an insert.</summary>
    private static async Task<BEEPSETTINGS?> ReadMarkerRowAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<BEEPSETTINGS>(source);
        unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

        var rows = await OgUow
            .ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, BEEPSETTINGS.Bootstrap)], cancellationToken)
            .ConfigureAwait(false);

        return rows.Count == 1 ? rows[0] : null;
    }

    /// <summary>Removes the marker once the account has a password of its own.</summary>
    private async Task ClearMarkerAsync(
        IDataSource source,
        string userName,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<BEEPSETTINGS>(source);
        unit.PrimaryKey = nameof(BEEPSETTINGS.SETTINGS_ID);

        // Delete() locates the row through the unit's own buffer, so the row must be read by this unit.
        var rows = await OgUow
            .ReadAsync(unit, [OgUow.Equal(unit.PrimaryKey, BEEPSETTINGS.Bootstrap)], cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count != 1)
        {
            return;
        }

        var marker = await ReadMarkerAsync(source, cancellationToken).ConfigureAwait(false);

        if (marker is not null
            && !string.Equals(marker.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (OgUow.IsOk(unit.Delete(rows[0])))
        {
            await unit.Commit().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Records one failed attempt and locks the account when the threshold is reached.
    /// </summary>
    /// <remarks>
    /// The complete row is written back through <see cref="IDataSource.UpdateEntity"/>: the row came from a full read
    /// (so every column is present), whereas a unit-of-work buffer only carries the columns it mapped and would write
    /// defaults for the rest — which is how an earlier attempt at this blanked the account's name.
    /// </remarks>
    private Task RecordFailureAsync(
        IDataSource source,
        USER candidate,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lastAttempt = AsUtc(candidate.ROW_CHANGED_DATE) ?? AsUtc(candidate.LAST_LOGIN_UTC);
        var attempt = OgSignInPolicy.NextFailure(candidate.FAILED_LOGIN_COUNT ?? 0, lastAttempt, now);

        candidate.FAILED_LOGIN_COUNT = attempt.Count;
        candidate.LOCKED_IND = attempt.Locked ? "Y" : "N";
        candidate.LOCKOUT_UNTIL_UTC = attempt.LockedUntil?.UtcDateTime;
        candidate.ROW_CHANGED_BY = "sign-in";
        candidate.ROW_CHANGED_DATE = now.UtcDateTime;

        _ = source.UpdateEntity(nameof(USER), candidate);

        return Task.CompletedTask;
    }

    /// <summary>Clears the failure count and any lock after a successful sign-in.</summary>
    private Task RecordSuccessAsync(
        IDataSource source,
        USER candidate,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        candidate.FAILED_LOGIN_COUNT = 0;
        candidate.LOCKED_IND = "N";
        candidate.LOCKOUT_UNTIL_UTC = null;
        candidate.LAST_LOGIN_UTC = now.UtcDateTime;
        candidate.ROW_CHANGED_BY = string.IsNullOrWhiteSpace(candidate.USERNAME) ? "sign-in" : candidate.USERNAME;
        candidate.ROW_CHANGED_DATE = now.UtcDateTime;

        _ = source.UpdateEntity(nameof(USER), candidate);

        return Task.CompletedTask;
    }

    /// <summary>SQLite stores dates as text; a value without a zone is UTC.</summary>
    private static DateTimeOffset? AsUtc(DateTime? value) =>
        value is { } date
            ? new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc))
            : null;

    /// <summary>
    /// Clears a lock so its owner can sign in again — the "or it is cleared" half of the rule. The SystemAdmin page
    /// calls this; a lock also expires on its own.
    /// </summary>
    public async Task<OgResult> ClearLockoutAsync(
        string userName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The company database '{ConnectionName}' could not be opened.",
                    OgDatabaseIdentityCodes.Unreachable);
            }

            var user = (await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false))
                .FirstOrDefault(row => string.Equals(row.USERNAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(row.USER_NAME, userName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                return OgResult.Failed(
                    $"No account named '{userName}' exists.",
                    OgDatabaseIdentityCodes.Refused);
            }

            user.FAILED_LOGIN_COUNT = 0;
            user.LOCKED_IND = "N";
            user.LOCKOUT_UNTIL_UTC = null;
            user.ROW_CHANGED_BY = actor;
            user.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = source.UpdateEntity(nameof(USER), user);

            return OgUow.IsOk(updated)
                ? OgResult.Ok($"'{userName}' is unlocked.")
                : OgResult.Failed(
                    OgUow.Message(updated, "The lock was not cleared."),
                    OgDatabaseIdentityCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"clearing the lock on '{userName}'",
                "the account is still locked",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The lock could not be cleared. Reference: {reference}",
                OgDatabaseIdentityCodes.WriteFailed);
        }
    }

    /// <summary>
    /// The permission codes this account holds through its roles (<c>USER_ROLE</c> → <c>ROLE_PERMISSION</c> →
    /// <c>PERMISSION</c>). A role is a bundle; this is what the page-level gates read when a role alone is too broad.
    /// </summary>
    private async Task<IReadOnlyList<string>> ReadPermissionsAsync(
        IDataSource source,
        string userId,
        CancellationToken cancellationToken)
    {
        var permissions = new List<string>();

        if (!HasTable(source, nameof(USER_ROLE))
            || !HasTable(source, nameof(ROLE_PERMISSION))
            || !HasTable(source, nameof(PERMISSION)))
        {
            return permissions;
        }

        var grants = await OgUow.ReadAllAsync<USER_ROLE>(source).ConfigureAwait(false);
        var rolePermissions = await OgUow.ReadAllAsync<ROLE_PERMISSION>(source).ConfigureAwait(false);
        var known = await OgUow.ReadAllAsync<PERMISSION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var roleIds = grants
            .Where(grant => string.Equals(grant.USER_ID, userId, StringComparison.OrdinalIgnoreCase))
            .Where(grant => grant.EXPIRY_DATE is null || grant.EXPIRY_DATE >= DateTime.UtcNow)
            .Select(grant => grant.ROLE_ID)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var codes = known
            .Where(permission => !string.IsNullOrWhiteSpace(permission.PERMISSION_CODE))
            .GroupBy(permission => permission.PERMISSION_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().PERMISSION_CODE, StringComparer.OrdinalIgnoreCase);

        foreach (var link in rolePermissions.Where(link => roleIds.Contains(link.ROLE_ID)))
        {
            if (codes.TryGetValue(link.PERMISSION_ID, out var code) && !string.IsNullOrWhiteSpace(code))
            {
                permissions.Add(code);
            }
        }

        return permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>True when the row is an active account with a real hash; a seeded principal is not.</summary>
    private static bool CanSignIn(USER user) =>
        user.IS_ACTIVE
        && !string.IsNullOrWhiteSpace(user.PASSWORD_HASH)
        && !string.Equals(user.PASSWORD_HASH, "seeded-no-login", StringComparison.OrdinalIgnoreCase)
        && user.PASSWORD_HASH.StartsWith("pbkdf2-sha256$", StringComparison.Ordinal);

    private async Task<IReadOnlyList<string>> ReadRolesAsync(
        IDataSource source,
        string userId,
        CancellationToken cancellationToken)
    {
        var roles = new List<string>();

        if (!HasTable(source, nameof(USER_ROLE)) || !HasTable(source, nameof(ROLE)))
        {
            return roles;
        }

        var grants = await OgUow.ReadAllAsync<USER_ROLE>(source).ConfigureAwait(false);
        var known = await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false);

        var roleNames = known.ToDictionary(role => role.ROLE_ID, role => role.ROLE_NAME, StringComparer.OrdinalIgnoreCase);

        foreach (var grant in grants.Where(role => string.Equals(role.USER_ID, userId, StringComparison.OrdinalIgnoreCase)))
        {
            if (grant.EXPIRY_DATE is { } expiry && expiry < DateTime.UtcNow)
            {
                continue;
            }

            if (roleNames.TryGetValue(grant.ROLE_ID, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                roles.Add(name);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        return roles;
    }

    /// <summary>PBKDF2-SHA256, the same parameters the local credential store uses.</summary>
    internal static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashBytes);

        return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Checks a password against a stored hash without leaking which part failed. An unreadable hash is a refusal,
    /// never an exception on the sign-in path.
    /// </summary>
    internal static bool Verify(string? password, string? stored)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(stored))
        {
            return false;
        }

        var parts = stored.Split('$');

        if (parts.Length != 4
            || !string.Equals(parts[0], "pbkdf2-sha256", StringComparison.Ordinal)
            || !int.TryParse(parts[1], out var iterations)
            || iterations <= 0)
        {
            return false;
        }

        // Base64 is decoded with the non-throwing API: a stored hash that is not base64 is a refusal on the
        // sign-in path, not an exception for the caller to handle.
        var saltBuffer = new byte[parts[2].Length];
        var hashBuffer = new byte[parts[3].Length];

        if (!Convert.TryFromBase64String(parts[2], saltBuffer, out var saltLength) || saltLength == 0)
        {
            return false;
        }

        if (!Convert.TryFromBase64String(parts[3], hashBuffer, out var hashLength) || hashLength == 0)
        {
            return false;
        }

        var salt = saltBuffer.AsSpan(0, saltLength).ToArray();
        var expected = hashBuffer.AsSpan(0, hashLength).ToArray();

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    internal IDataSource? OpenSource()
    {
        var editor = _host.Editor;

        var configured = editor.ConfigEditor?.DataConnections?
            .FirstOrDefault(connection =>
                string.Equals(connection.ConnectionName, ConnectionName, StringComparison.OrdinalIgnoreCase));

        if (configured is null)
        {
            return null;
        }

        var source = editor.GetDataSource(ConnectionName);

        if (source is null)
        {
            source = editor.CreateNewDataSourceConnection(configured, ConnectionName);
        }

        if (source?.Openconnection() == System.Data.ConnectionState.Open)
        {
            return source;
        }

        if (source is not null && editor.CheckDataSourceExist(ConnectionName))
        {
            editor.CloseDataSource(ConnectionName);
            editor.RemoveDataDource(ConnectionName);
        }

        source = editor.CreateNewDataSourceConnection(configured, ConnectionName);

        return source?.Openconnection() == System.Data.ConnectionState.Open ? source : null;
    }

    internal bool HasTable(IDataSource source, string table)
    {
        try
        {
            return source.GetEntitesList()
                .Any(name => string.Equals(name, table, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            _failures.ReportHandled(
                ex,
                $"listing the tables of '{ConnectionName}'",
                $"'{table}' cannot be found, so sign-in cannot be checked",
                FailureSeverity.Degraded);

            return false;
        }
    }
}

/// <summary>
/// The marker a setup leaves on the account it created: the password was typed by somebody else and must be replaced
/// at the first sign-in (Keycloak's bootstrap-admin rule; SETUP-03 AC-4).
/// </summary>
public sealed record OgBootstrapMarker(string UserName, bool MustChangePassword, DateTimeOffset CreatedUtc);
