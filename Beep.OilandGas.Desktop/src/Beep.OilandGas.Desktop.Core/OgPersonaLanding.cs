namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>The page a persona lands on, and why (FND-07 AC-7).</summary>
/// <param name="Route">The route to open, or an empty string when nothing may be opened.</param>
/// <param name="Persona">The persona the role set resolves to, or a placeholder when it resolves to none.</param>
/// <param name="Because">
/// One sentence for the audit row and the status line. It says when a fallback was used, so a person is never
/// silently shown a page their persona did not choose.
/// </param>
/// <param name="IsFallback">True when the persona's own page was not available and the dashboard was used.</param>
public sealed record OgLanding(string Route, string Persona, string Because, bool IsFallback);

/// <summary>
/// FND-07 AC-7: the RBAC role set decides where the shell opens.
/// </summary>
/// <remarks>
/// <para>
/// The order is the table's, not the role list's: an administrator who is also a drilling engineer lands on the
/// dashboard, because the more privileged persona owns the first screen. That makes the landing deterministic
/// even when the store returns roles in a different order.
/// </para>
/// <para>
/// A persona's page must also be <b>openable</b>. When it is not — a module the licence withholds, a page the
/// role cannot reach, a build without that route — the landing falls back to the dashboard and says so, rather
/// than selecting a route the guard would refuse a moment later.
/// </para>
/// </remarks>
public static class OgPersonaLanding
{
    /// <summary>The default page; every installation can open it.</summary>
    public const string DefaultRoute = "dashboard";

    /// <summary>The placeholder persona when no known role is present.</summary>
    public const string NoPersona = "(no known role)";

    private static readonly (string Role, string Persona, string Route)[] Table =
    [
        (OgRoleNames.SystemAdmin, "Administrator", DefaultRoute),
        (OgRoleNames.TenantAdmin, "Administrator", DefaultRoute),
        (OgRoleNames.ExplorationGeologist, "Exploration geologist", "exploration"),
        (OgRoleNames.DrillingEngineer, "Drilling engineer", "wellpath"),
        (OgRoleNames.ReservoirEngineer, "Reservoir engineer", "correlation"),
        (OgRoleNames.ProductionEngineer, "Production engineer", "logs"),
        (OgRoleNames.HseOfficer, "HSE officer", "hse"),
        (OgRoleNames.DecommissioningCoordinator, "Decommissioning coordinator", "decommissioning"),
        (OgRoleNames.Accountant, "Accountant", "export"),
        (OgRoleNames.Viewer, "Viewer", DefaultRoute),
    ];

    /// <summary>Every persona this build knows, in priority order (Administrator first).</summary>
    public static IReadOnlyList<(string Role, string Persona, string Route)> All => Table;

    /// <summary>
    /// The desktop route a persona's RBAC landing route maps to (EXP-01 AC-4).
    /// </summary>
    /// <remarks>
    /// RBAC landing routes are web paths - <c>/lifecycle/exploration/dashboard</c> for the exploration geologist.
    /// The desktop page is the module's workspace rather than the web dashboard, so the module segment (the one
    /// before a trailing <c>dashboard</c>) is the route. Reading it from the RBAC seed, rather than repeating a
    /// literal, is what keeps the two in step: the guard test compares this mapping with the seed file.
    /// </remarks>
    public static string RouteFor(string? rbacRoute)
    {
        if (string.IsNullOrWhiteSpace(rbacRoute))
        {
            return DefaultRoute;
        }

        var segments = rbacRoute
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0)
        {
            return DefaultRoute;
        }

        if (segments.Length >= 2
            && string.Equals(segments[^1], "dashboard", StringComparison.OrdinalIgnoreCase))
        {
            return segments[^2];
        }

        return segments[^1];
    }

    /// <summary>
    /// The persona a role set resolves to: the highest-priority known role, or <see cref="NoPersona"/>.
    /// </summary>
    public static string PersonaFor(IReadOnlyList<string>? roles)
    {
        if (roles is null)
        {
            return NoPersona;
        }

        foreach (var (role, persona, _) in Table)
        {
            if (roles.Any(candidate => string.Equals(candidate, role, StringComparison.OrdinalIgnoreCase)))
            {
                return persona;
            }
        }

        return NoPersona;
    }

    /// <summary>
    /// The landing for a role set. <paramref name="canOpen"/> is the route guard, so the answer is the page the
    /// person will actually reach.
    /// </summary>
    public static OgLanding Resolve(IReadOnlyList<string>? roles, Func<string, bool> canOpen)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        var personaRoute = (string?)null;
        var persona = NoPersona;

        // The first known role in table order is the persona; its page decides, not a lower-priority role's.
        foreach (var (role, name, route) in Table)
        {
            if (roles is null
                || !roles.Any(candidate => string.Equals(candidate, role, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            persona = name;
            personaRoute = route;

            if (canOpen(route))
            {
                return new OgLanding(
                    route,
                    persona,
                    $"The {persona} persona opens {route}.",
                    IsFallback: false);
            }

            break;
        }

        if (canOpen(DefaultRoute))
        {
            var because = personaRoute is null
                ? $"No known role was found, so the shell opens {DefaultRoute}."
                : $"The {persona} persona's page '{personaRoute}' is not available here, so the shell opens {DefaultRoute}.";

            return new OgLanding(DefaultRoute, persona, because, IsFallback: personaRoute is not null);
        }

        return new OgLanding(
            string.Empty,
            persona,
            "No page is available to this role set; the shell will say so instead of opening a refused route.",
            IsFallback: personaRoute is not null);
    }
}
