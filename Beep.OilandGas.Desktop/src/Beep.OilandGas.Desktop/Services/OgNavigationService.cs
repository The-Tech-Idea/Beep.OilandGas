using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>One row in the shell navigation.</summary>
public sealed record OgNavigationItem(
    string Title,
    string Route,
    string ModuleKey,
    string Group,
    string Glyph);

/// <summary>
/// The commercial gate: what this installation is entitled to (REL-02). It is always the <b>second</b> gate
/// after roles, so an entitled module still needs the right role, and a permitted role still needs the module.
/// </summary>
public interface IOgEntitlementService
{
    /// <summary>The rights in force now. Re-read after <c>Import</c>, so a licence change needs no restart.</summary>
    OgEntitlement Current { get; }

    bool Allows(string moduleKey);

    bool AllowsWriting(string moduleKey);
}

/// <summary>
/// The permissive entitlement: everything allowed, and <see cref="Current"/> says so rather than pretending a
/// licence exists. Used when an installation deliberately runs with enforcement off and no file.
/// </summary>
public sealed class AllowAllEntitlements : IOgEntitlementService
{
    public OgEntitlement Current { get; } =
        OgEntitlement.Unlicensed(OgLicenceEnforcement.Permissive);

    public bool Allows(string moduleKey) => true;

    public bool AllowsWriting(string moduleKey) => true;
}

/// <summary>
/// Builds the shell navigation and answers route-access questions from two independent facts: the
/// commercial entitlement and the signed-in roles. A page must pass both.
/// </summary>
public sealed class OgNavigationService
{
    private static readonly HashSet<string> AdministrationRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "settings", "diagnostics", "failures", "audit",
    };

    private static readonly HashSet<string> DataRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "connections",
    };

    private readonly IOgEntitlementService _entitlements;
    private readonly ILocProvider _loc;

    public OgNavigationService(IOgEntitlementService entitlements, ILocProvider loc)
    {
        _entitlements = entitlements;
        _loc = loc;
    }

    public IReadOnlyList<OgNavigationItem> Build(OgPermissions? permissions)
    {
        var allowed = new List<OgNavigationItem>();

        foreach (var page in OgModuleCatalog.Pages)
        {
            if (!CanAccess(page, permissions))
            {
                continue;
            }

            var (groupKey, glyph) = Presentation(page.Route);
            allowed.Add(new OgNavigationItem(
                _loc.Get($"Page.{page.Route}"),
                page.Route,
                page.ModuleKey,
                _loc.Get(groupKey),
                glyph));
        }

        return allowed;
    }

    public bool CanAccess(string route, OgPermissions? permissions)
    {
        var page = OgModuleCatalog.Find(route);
        return page is not null && CanAccess(page, permissions);
    }

    private bool CanAccess(OgPageDefinition page, OgPermissions? permissions)
    {
        if (page.RequiresEntitlement && !_entitlements.Allows(page.ModuleKey))
        {
            return false;
        }

        if (permissions is null)
        {
            return false;
        }

        if (permissions.IsSystemAdmin)
        {
            return true;
        }

        if (page.RequiredRoles.Count == 0)
        {
            return true;
        }

        return page.RequiredRoles.Any(permissions.HasRole);
    }

    private static (string GroupKey, string Glyph) Presentation(string route) => route.ToLowerInvariant() switch
    {
        "dashboard" => ("Shell.Nav.Overview", "\uE80F"),
        "tasks" => ("Shell.Nav.Overview", "\uE9D5"),
        "wells" => ("Page.wells", "\uE7B8"),
        "connections" => ("Page.connections", "\uE968"),
        _ => ("Shell.Nav.Administration", "\uE713"),
    };

    public static bool IsAdministrationRoute(string route) => AdministrationRoutes.Contains(route);

    public static bool IsDataRoute(string route) => DataRoutes.Contains(route);
}
