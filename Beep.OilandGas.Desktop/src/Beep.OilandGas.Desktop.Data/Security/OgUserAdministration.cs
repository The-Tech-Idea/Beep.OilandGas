using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Security;

/// <summary>One account as the administration page shows it.</summary>
public sealed record OgManagedUser(
    string UserId,
    string UserName,
    string DisplayName,
    bool IsActive,
    IReadOnlyList<string> Roles,
    bool MustChangePassword,
    DateTimeOffset? LockedUntil = null)
{
    /// <summary>True when a sign-in lock still applies.</summary>
    public bool IsLocked => OgSignInPolicy.IsLocked(LockedUntil, DateTimeOffset.UtcNow);
}

/// <summary>What the system administrator may do to the account directory of the installation's database.</summary>
/// <remarks>
/// Every operation here is the SystemAdmin's (SETUP-05 AC-9, SETUP-10 AC-6). The page that calls it is gated on the
/// role; this type adds the rules that make an accidental lock-out impossible: the last active administrator cannot
/// be disabled or lose its role, and a reset password is provisional again, so the administrator never knows the
/// password a person will actually use.
/// </remarks>
public interface IOgUserAdministration
{
    Task<OgResult<IReadOnlyList<OgManagedUser>>> ListAsync(CancellationToken cancellationToken = default);

    Task<OgResult<OgManagedUser>> CreateAsync(
        string userName,
        string displayName,
        string password,
        IReadOnlyList<string> roles,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a new password on somebody's account. The account is marked provisional again: the administrator who
    /// reset it must not be the person who keeps using it.
    /// </summary>
    Task<OgResult> ResetPasswordAsync(
        string userName,
        string newPassword,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult> SetActiveAsync(
        string userName,
        bool active,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult> GrantRoleAsync(
        string userName,
        string roleName,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult> RevokeRoleAsync(
        string userName,
        string roleName,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The roles this database offers, for the picker.</summary>
    Task<OgResult<IReadOnlyList<string>>> ListRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears a sign-in lock so its owner can try again before the lock expires on its own.</summary>
    Task<OgResult> ClearLockoutAsync(
        string userName,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>The result codes the page branches on.</summary>
public static class OgUserAdministrationCodes
{
    public const string UserExists = "user-exists";
    public const string UserNotFound = "user-not-found";
    public const string RoleNotFound = "role-not-found";
    public const string TablesMissing = "security-tables-missing";
    public const string LastAdministrator = "last-administrator";
    public const string RoleAlreadyGranted = "role-already-granted";
    public const string PasswordRefused = OgDatabaseIdentityCodes.PasswordRefused;
    public const string Unreachable = OgDatabaseIdentityCodes.Unreachable;
    public const string WriteFailed = OgDatabaseIdentityCodes.WriteFailed;
}

/// <inheritdoc />
public sealed class BeepDmUserAdministration : IOgUserAdministration
{
    private const string AdministratorRoleNames = "Administrator,Admin,SystemAdmin";

    private readonly BeepDmDatabaseIdentity _identity;
    private readonly IFailureReporter _failures;

    public BeepDmUserAdministration(BeepDmDatabaseIdentity identity, IFailureReporter failures)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public string ConnectionName => _identity.ConnectionName;

    public async Task<OgResult<IReadOnlyList<OgManagedUser>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = _identity.OpenSource();
            if (source is null)
            {
                return OgResult<IReadOnlyList<OgManagedUser>>.Failed(
                    $"The database '{ConnectionName}' could not be opened.",
                    OgUserAdministrationCodes.Unreachable);
            }

            if (!HasSecurityTables(source))
            {
                return OgResult<IReadOnlyList<OgManagedUser>>.Failed(
                    "This database has no account tables yet.",
                    OgUserAdministrationCodes.TablesMissing);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);
            var grants = await OgUow.ReadAllAsync<USER_ROLE>(source).ConfigureAwait(false);
            var roles = await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false);
            var marker = await _identity.ReadMarkerAsync(source, cancellationToken).ConfigureAwait(false);

            var roleNames = roles
                .Where(role => !string.IsNullOrWhiteSpace(role.ROLE_ID))
                .GroupBy(role => role.ROLE_ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().ROLE_NAME ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            IReadOnlyList<OgManagedUser> rows = users
                .Select(user =>
                {
                    var granted = grants
                        .Where(grant => string.Equals(grant.USER_ID, user.USER_ID, StringComparison.OrdinalIgnoreCase))
                        .Select(grant => roleNames.TryGetValue(grant.ROLE_ID, out var name) ? name : string.Empty)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    return new OgManagedUser(
                        user.USER_ID,
                        string.IsNullOrWhiteSpace(user.USERNAME) ? user.USER_NAME ?? string.Empty : user.USERNAME,
                        string.IsNullOrWhiteSpace(user.FULL_NAME) ? user.USER_NAME ?? string.Empty : user.FULL_NAME,
                        user.IS_ACTIVE,
                        granted,
                        marker is not null
                        && marker.MustChangePassword
                        && string.Equals(marker.UserName, user.USERNAME, StringComparison.OrdinalIgnoreCase),
                        user.LOCKED_IND == "Y" && user.LOCKOUT_UNTIL_UTC is { } until
                            ? new DateTimeOffset(DateTime.SpecifyKind(until, DateTimeKind.Utc))
                            : null);
                })
                .OrderBy(user => user.UserName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return OgResult<IReadOnlyList<OgManagedUser>>.Ok(rows);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing the accounts of the database",
                "no accounts are shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<OgManagedUser>>.Failed(
                $"The accounts could not be listed. Reference: {reference}",
                OgUserAdministrationCodes.Unreachable);
        }
    }

    public async Task<OgResult<OgManagedUser>> CreateAsync(
        string userName,
        string displayName,
        string password,
        IReadOnlyList<string> roles,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(userName))
            {
                return OgResult<OgManagedUser>.Failed(
                    "A user name is required.",
                    OgUserAdministrationCodes.UserNotFound);
            }

            var problems = OgPasswordPolicy.Problems(password);

            if (problems.Count > 0)
            {
                return OgResult<OgManagedUser>.Failed(
                    string.Join(" ", problems),
                    OgUserAdministrationCodes.PasswordRefused);
            }

            var source = _identity.OpenSource();
            if (source is null)
            {
                return OgResult<OgManagedUser>.Failed(
                    $"The database '{ConnectionName}' could not be opened.",
                    OgUserAdministrationCodes.Unreachable);
            }

            if (!HasSecurityTables(source))
            {
                return OgResult<OgManagedUser>.Failed(
                    "This database has no account tables yet.",
                    OgUserAdministrationCodes.TablesMissing);
            }

            var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);

            if (users.Any(user =>
                    string.Equals(user.USERNAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(user.USER_NAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgManagedUser>.Failed(
                    $"A user named '{userName.Trim()}' already exists.",
                    OgUserAdministrationCodes.UserExists);
            }

            var now = DateTime.UtcNow;
            var userId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<USER>(source);
            unit.PrimaryKey = nameof(USER.USER_ID);
            unit.Add(new USER
            {
                USER_ID = userId,
                USERNAME = userName.Trim(),
                USER_NAME = string.IsNullOrWhiteSpace(displayName) ? userName.Trim() : displayName.Trim(),
                FULL_NAME = displayName.Trim(),
                EMAIL = string.Empty,
                PASSWORD_HASH = BeepDmDatabaseIdentity.Hash(password),
                IS_ACTIVE = true,
                TENANT_ID = string.Empty,
                BUSINESS_ASSOCIATE_ID = string.Empty,
                FAILED_LOGIN_COUNT = 0,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor,
                ROW_CHANGED_DATE = now,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgManagedUser>.Failed(
                    OgUow.Message(committed, "The account was not created."),
                    OgUserAdministrationCodes.WriteFailed);
            }

            // Provisional until its owner chooses a password: the administrator typed this one.
            var marked = await _identity
                .MarkPasswordChangeRequiredAsync(userName, actor, cancellationToken)
                .ConfigureAwait(false);

            if (!marked.Success)
            {
                return OgResult<OgManagedUser>.Failed(
                    marked.Message ?? "The account was created but is not marked for a password change.",
                    marked.Reference ?? OgUserAdministrationCodes.WriteFailed);
            }

            var grantedRoles = new List<string>();

            foreach (var role in roles.Where(role => !string.IsNullOrWhiteSpace(role)))
            {
                var granted = await GrantRoleAsync(userName, role, actor, cancellationToken).ConfigureAwait(false);

                if (!granted.Success)
                {
                    return OgResult<OgManagedUser>.Failed(
                        granted.Message ?? $"The account was created but the role '{role}' was not granted.",
                        granted.Reference ?? OgUserAdministrationCodes.WriteFailed);
                }

                grantedRoles.Add(role.Trim());
            }

            return OgResult<OgManagedUser>.Ok(new OgManagedUser(
                userId,
                userName.Trim(),
                string.IsNullOrWhiteSpace(displayName) ? userName.Trim() : displayName.Trim(),
                IsActive: true,
                grantedRoles,
                MustChangePassword: true));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating the account '{userName}'",
                "no account was created",
                FailureSeverity.Error);

            return OgResult<OgManagedUser>.Failed(
                $"The account could not be created. Reference: {reference}",
                OgUserAdministrationCodes.WriteFailed);
        }
    }

    public async Task<OgResult> ResetPasswordAsync(
        string userName,
        string newPassword,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var changed = await _identity
            .ChangePasswordAsync(userName, newPassword, cancellationToken)
            .ConfigureAwait(false);

        if (!changed.Success)
        {
            return changed;
        }

        // Re-marked: the administrator typed this password, so the person must choose their own before using it.
        var marked = await _identity
            .MarkPasswordChangeRequiredAsync(userName, actor, cancellationToken)
            .ConfigureAwait(false);

        return marked.Success
            ? OgResult.Ok("The password was reset; the person must choose a new one at the next sign-in.")
            : marked;
    }

    public async Task<OgResult> SetActiveAsync(
        string userName,
        bool active,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = _identity.OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The database '{ConnectionName}' could not be opened.",
                    OgUserAdministrationCodes.Unreachable);
            }

            if (!HasSecurityTables(source))
            {
                return OgResult.Failed("This database has no account tables yet.", OgUserAdministrationCodes.TablesMissing);
            }

            var unit = OgUow.For<USER>(source);
            unit.PrimaryKey = nameof(USER.USER_ID);

            var rows = await OgUow
                .ReadAsync(unit, [OgUow.Equal(nameof(USER.USERNAME), userName.Trim())], cancellationToken)
                .ConfigureAwait(false);

            var user = rows.FirstOrDefault();

            if (user is null)
            {
                return OgResult.Failed($"No account named '{userName}' exists.", OgUserAdministrationCodes.UserNotFound);
            }

            // Disabling the last active administrator would lock everybody out of the installation.
            if (!active && await IsLastActiveAdministratorAsync(source, user, cancellationToken).ConfigureAwait(false))
            {
                return OgResult.Failed(
                    "This is the last active administrator; disabling it would leave nobody able to administer the database.",
                    OgUserAdministrationCodes.LastAdministrator);
            }

            user.IS_ACTIVE = active;
            user.ROW_CHANGED_BY = actor;
            user.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(user);

            if (!OgUow.IsOk(updated))
            {
                return OgResult.Failed(
                    OgUow.Message(updated, "The account was not changed."),
                    OgUserAdministrationCodes.WriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok(active ? "The account is active." : "The account is disabled.")
                : OgResult.Failed(
                    OgUow.Message(committed, "The account was not changed."),
                    OgUserAdministrationCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"changing whether '{userName}' is active",
                "the account was not changed",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The account could not be changed. Reference: {reference}",
                OgUserAdministrationCodes.WriteFailed);
        }
    }

    public async Task<OgResult> GrantRoleAsync(
        string userName,
        string roleName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = _identity.OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The database '{ConnectionName}' could not be opened.",
                    OgUserAdministrationCodes.Unreachable);
            }

            if (!HasSecurityTables(source))
            {
                return OgResult.Failed("This database has no account tables yet.", OgUserAdministrationCodes.TablesMissing);
            }

            var user = (await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false))
                .FirstOrDefault(row => string.Equals(row.USERNAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(row.USER_NAME, userName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                return OgResult.Failed($"No account named '{userName}' exists.", OgUserAdministrationCodes.UserNotFound);
            }

            var role = (await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false))
                .FirstOrDefault(row => string.Equals(row.ROLE_NAME, roleName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (role is null)
            {
                return OgResult.Failed($"No role named '{roleName}' exists.", OgUserAdministrationCodes.RoleNotFound);
            }

            var grants = await OgUow.ReadAllAsync<USER_ROLE>(source).ConfigureAwait(false);

            if (grants.Any(grant =>
                    string.Equals(grant.USER_ID, user.USER_ID, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(grant.ROLE_ID, role.ROLE_ID, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult.Failed(
                    $"'{userName}' already holds the role '{role.ROLE_NAME}'.",
                    OgUserAdministrationCodes.RoleAlreadyGranted);
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<USER_ROLE>(source);
            unit.PrimaryKey = nameof(USER_ROLE.USER_ROLE_ID);
            unit.Add(new USER_ROLE
            {
                USER_ROLE_ID = Guid.NewGuid().ToString("N"),
                USER_ID = user.USER_ID,
                ROLE_ID = role.ROLE_ID,
                BUSINESS_ASSOCIATE_ID = user.BUSINESS_ASSOCIATE_ID ?? string.Empty,
                EFFECTIVE_DATE = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor,
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor,
                ROW_CHANGED_DATE = now,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok($"'{userName}' now holds '{role.ROLE_NAME}'.")
                : OgResult.Failed(
                    OgUow.Message(committed, "The role was not granted."),
                    OgUserAdministrationCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"granting '{roleName}' to '{userName}'",
                "the role was not granted",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The role could not be granted. Reference: {reference}",
                OgUserAdministrationCodes.WriteFailed);
        }
    }

    public async Task<OgResult> RevokeRoleAsync(
        string userName,
        string roleName,
        string actor,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = _identity.OpenSource();
            if (source is null)
            {
                return OgResult.Failed(
                    $"The database '{ConnectionName}' could not be opened.",
                    OgUserAdministrationCodes.Unreachable);
            }

            if (!HasSecurityTables(source))
            {
                return OgResult.Failed("This database has no account tables yet.", OgUserAdministrationCodes.TablesMissing);
            }

            var user = (await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false))
                .FirstOrDefault(row => string.Equals(row.USERNAME, userName.Trim(), StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(row.USER_NAME, userName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                return OgResult.Failed($"No account named '{userName}' exists.", OgUserAdministrationCodes.UserNotFound);
            }

            var role = (await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false))
                .FirstOrDefault(row => string.Equals(row.ROLE_NAME, roleName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (role is null)
            {
                return OgResult.Failed($"No role named '{roleName}' exists.", OgUserAdministrationCodes.RoleNotFound);
            }

            if (IsAdministratorName(role.ROLE_NAME) && await IsLastActiveAdministratorAsync(source, user, cancellationToken).ConfigureAwait(false))
            {
                return OgResult.Failed(
                    "This is the last active administrator; revoking the role would leave nobody able to administer the database.",
                    OgUserAdministrationCodes.LastAdministrator);
            }

            // The unit must hold the row it deletes.
            var unit = OgUow.For<USER_ROLE>(source);
            unit.PrimaryKey = nameof(USER_ROLE.USER_ROLE_ID);

            var rows = await OgUow
                .ReadAsync(unit, [OgUow.Equal(nameof(USER_ROLE.USER_ID), user.USER_ID)], cancellationToken)
                .ConfigureAwait(false);

            var grant = rows.FirstOrDefault(row =>
                string.Equals(row.ROLE_ID, role.ROLE_ID, StringComparison.OrdinalIgnoreCase));

            if (grant is null)
            {
                return OgResult.Failed(
                    $"'{userName}' does not hold the role '{role.ROLE_NAME}'.",
                    OgUserAdministrationCodes.RoleNotFound);
            }

            var deleted = unit.Delete(grant);

            if (!OgUow.IsOk(deleted))
            {
                return OgResult.Failed(
                    OgUow.Message(deleted, "The role was not revoked."),
                    OgUserAdministrationCodes.WriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committed)
                ? OgResult.Ok($"'{userName}' no longer holds '{role.ROLE_NAME}'.")
                : OgResult.Failed(
                    OgUow.Message(committed, "The role was not revoked."),
                    OgUserAdministrationCodes.WriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"revoking '{roleName}' from '{userName}'",
                "the role was not revoked",
                FailureSeverity.Error);

            return OgResult.Failed(
                $"The role could not be revoked. Reference: {reference}",
                OgUserAdministrationCodes.WriteFailed);
        }
    }

    public Task<OgResult> ClearLockoutAsync(
        string userName,
        string actor,
        CancellationToken cancellationToken = default) =>
        _identity.ClearLockoutAsync(userName, actor, cancellationToken);

    public async Task<OgResult<IReadOnlyList<string>>> ListRolesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = _identity.OpenSource();
            if (source is null)
            {
                return OgResult<IReadOnlyList<string>>.Failed(
                    $"The database '{ConnectionName}' could not be opened.",
                    OgUserAdministrationCodes.Unreachable);
            }

            if (!HasSecurityTables(source))
            {
                return OgResult<IReadOnlyList<string>>.Failed(
                    "This database has no role table yet.",
                    OgUserAdministrationCodes.TablesMissing);
            }

            IReadOnlyList<string> names = (await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false))
                .Select(role => role.ROLE_NAME ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return OgResult<IReadOnlyList<string>>.Ok(names);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing the roles of the database",
                "no roles are shown",
                FailureSeverity.Error);

            return OgResult<IReadOnlyList<string>>.Failed(
                $"The roles could not be listed. Reference: {reference}",
                OgUserAdministrationCodes.Unreachable);
        }
    }

    private bool HasSecurityTables(IDataSource source) =>
        _identity.HasTable(source, nameof(USER))
        && _identity.HasTable(source, nameof(ROLE))
        && _identity.HasTable(source, nameof(USER_ROLE));

    /// <summary>True when the account is an administrator and the only active one.</summary>
    private async Task<bool> IsLastActiveAdministratorAsync(
        IDataSource source,
        USER user,
        CancellationToken cancellationToken)
    {
        var roles = await OgUow.ReadAllAsync<ROLE>(source).ConfigureAwait(false);
        var grants = await OgUow.ReadAllAsync<USER_ROLE>(source).ConfigureAwait(false);
        var users = await OgUow.ReadAllAsync<USER>(source).ConfigureAwait(false);

        var administratorRoleIds = roles
            .Where(role => IsAdministratorName(role.ROLE_NAME))
            .Select(role => role.ROLE_ID)
            .ToList();

        var administrators = users
            .Where(candidate => candidate.IS_ACTIVE)
            .Where(candidate => grants.Any(grant =>
                string.Equals(grant.USER_ID, candidate.USER_ID, StringComparison.OrdinalIgnoreCase)
                && administratorRoleIds.Contains(grant.ROLE_ID, StringComparer.OrdinalIgnoreCase)))
            .Select(candidate => candidate.USER_ID)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        return administrators.Count == 1
               && string.Equals(administrators[0], user.USER_ID, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAdministratorName(string? roleName) =>
        !string.IsNullOrWhiteSpace(roleName)
        && AdministratorRoleNames.Split(',').Contains(roleName, StringComparer.OrdinalIgnoreCase);
}
