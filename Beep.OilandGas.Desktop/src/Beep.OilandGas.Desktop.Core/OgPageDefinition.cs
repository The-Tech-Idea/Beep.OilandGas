namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>One navigable page: what it is called, which module sells it, and who may open it.</summary>
/// <param name="Route">Stable route key, for example <c>wells</c> or <c>analysis/nodal</c>.</param>
/// <param name="Title">Fallback title; the shell prefers the localisation key <c>Page.&lt;route&gt;</c>.</param>
/// <param name="ModuleKey">The commercial module that sells this page (see <see cref="OgModuleKeys"/>).</param>
/// <param name="RequiredRoles">Roles that may open the page when no navigation grants are configured.</param>
/// <param name="RequiredPermission">An additional permission the data source may grant or revoke.</param>
public sealed record OgPageDefinition(
    string Route,
    string Title,
    string ModuleKey,
    IReadOnlyList<string> RequiredRoles,
    string? RequiredPermission = null)
{
    public bool RequiresEntitlement =>
        !string.Equals(ModuleKey, OgModuleKeys.Core, StringComparison.OrdinalIgnoreCase);
}
