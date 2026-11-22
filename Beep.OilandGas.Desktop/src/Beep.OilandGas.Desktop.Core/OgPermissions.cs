namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The permission codes this application gates on, from the owning module's own catalogue so the string is never a
/// local invention: <c>Beep.OilandGas.Models.Data.Security.PermissionConstants.Admin</c>.
/// </summary>
public static class OgPermissionNames
{
    /// <summary>Create, reset, disable and grant roles on accounts.</summary>
    public const string ManageUsers = "Admin.ManageUsers";

    /// <summary>Change the installation's company settings, module set and environment.</summary>
    public const string ConfigureSystem = "Admin.ConfigureSystem";
}

/// <summary>
/// What the signed-in person may do. Roles come from the client application's own RBAC store; the
/// permission set is whatever the data source granted, and may be unavailable when the lookup failed —
/// which is different from "granted nothing".
/// </summary>
public sealed class OgPermissions
{
    public static OgPermissions None { get; } = new([], [], permissionSourceAvailable: false);

    public OgPermissions(
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        bool permissionSourceAvailable)
    {
        Roles = roles ?? [];
        Permissions = permissions ?? [];
        PermissionSourceAvailable = permissionSourceAvailable;
    }

    public IReadOnlyList<string> Roles { get; }

    public IReadOnlyList<string> Permissions { get; }

    /// <summary>False when the grant lookup itself failed, so an empty set means "unknown", not "denied".</summary>
    public bool PermissionSourceAvailable { get; }

    public bool IsSystemAdmin =>
        Roles.Any(role => string.Equals(role, OgRoleNames.SystemAdmin, StringComparison.OrdinalIgnoreCase));

    public bool HasRole(string role) =>
        Roles.Any(item => string.Equals(item, role, StringComparison.OrdinalIgnoreCase));

    public bool HasPermission(string permission) =>
        Permissions.Any(item => string.Equals(item, permission, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// May this person administer the account directory? A system administrator may; otherwise the database must have
    /// granted the permission through one of the roles (<c>Admin.ManageUsers</c>).
    /// </summary>
    public bool CanManageUsers =>
        IsSystemAdmin || HasPermission(OgPermissionNames.ManageUsers);

    /// <summary>
    /// May this person change the installation's company settings? A system administrator may; otherwise the database
    /// must have granted <c>Admin.ConfigureSystem</c>.
    /// </summary>
    public bool CanConfigureSystem =>
        IsSystemAdmin || HasPermission(OgPermissionNames.ConfigureSystem);

    /// <summary>The permission the write is allowed by, for a refusal sentence.</summary>
    public string NameOfPermissionFor(string capability) => capability switch
    {
        OgPermissionNames.ManageUsers => "Admin.ManageUsers",
        OgPermissionNames.ConfigureSystem => "Admin.ConfigureSystem",
        _ => capability,
    };
}
