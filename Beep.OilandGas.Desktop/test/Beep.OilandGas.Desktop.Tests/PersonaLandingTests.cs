using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Security;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-07 AC-7: the RBAC role set decides where the shell opens, the more privileged persona wins, a page that
/// cannot be opened falls back to the dashboard with a stated reason, and the session's persona is no longer a
/// placeholder.
/// </summary>
public sealed class PersonaLandingTests
{
    [Theory]
    [InlineData(OgRoleNames.SystemAdmin, "dashboard", "Administrator")]
    [InlineData(OgRoleNames.TenantAdmin, "dashboard", "Administrator")]
    [InlineData(OgRoleNames.DrillingEngineer, "wellpath", "Drilling engineer")]
    [InlineData(OgRoleNames.ReservoirEngineer, "correlation", "Reservoir engineer")]
    [InlineData(OgRoleNames.ProductionEngineer, "logs", "Production engineer")]
    [InlineData(OgRoleNames.HseOfficer, "hse", "HSE officer")]
    [InlineData(OgRoleNames.Accountant, "export", "Accountant")]
    [InlineData(OgRoleNames.Viewer, "dashboard", "Viewer")]
    public void Each_persona_lands_on_its_own_page(string role, string route, string persona)
    {
        var landing = OgPersonaLanding.Resolve([role], _ => true);

        Assert.Equal(route, landing.Route);
        Assert.Equal(persona, landing.Persona);
        Assert.False(landing.IsFallback);
        Assert.Contains(route, landing.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void The_more_privileged_persona_wins_whatever_order_the_roles_arrive_in()
    {
        var forward = OgPersonaLanding.Resolve([OgRoleNames.DrillingEngineer, OgRoleNames.SystemAdmin], _ => true);
        var reverse = OgPersonaLanding.Resolve([OgRoleNames.SystemAdmin, OgRoleNames.DrillingEngineer], _ => true);

        Assert.Equal("dashboard", forward.Route);
        Assert.Equal(forward.Route, reverse.Route);
        Assert.Equal("Administrator", reverse.Persona);
    }

    [Fact]
    public void A_persona_page_that_cannot_be_opened_falls_back_to_the_dashboard_and_says_so()
    {
        // Only the dashboard is openable: the drilling engineer's page is refused.
        var landing = OgPersonaLanding.Resolve(
            [OgRoleNames.DrillingEngineer],
            route => route == "dashboard");

        Assert.Equal("dashboard", landing.Route);
        Assert.True(landing.IsFallback);
        Assert.Equal("Drilling engineer", landing.Persona);
        Assert.Contains("wellpath", landing.Because, StringComparison.Ordinal);
        Assert.Contains("not available", landing.Because, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_lower_priority_role_does_not_rescue_a_denied_persona_page()
    {
        // The drilling engineer is the persona; the viewer role must not silently change the landing.
        var landing = OgPersonaLanding.Resolve(
            [OgRoleNames.Viewer, OgRoleNames.DrillingEngineer],
            route => route == "dashboard");

        Assert.Equal("dashboard", landing.Route);
        Assert.Equal("Drilling engineer", landing.Persona);
        Assert.True(landing.IsFallback);
    }

    [Fact]
    public void No_known_role_lands_on_the_dashboard_and_is_named_as_such()
    {
        var landing = OgPersonaLanding.Resolve([], _ => true);

        Assert.Equal("dashboard", landing.Route);
        Assert.Equal(OgPersonaLanding.NoPersona, landing.Persona);
        Assert.False(landing.IsFallback);
        Assert.Contains("No known role", landing.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void Nothing_openable_produces_an_empty_route_rather_than_a_refused_one()
    {
        var landing = OgPersonaLanding.Resolve([OgRoleNames.SystemAdmin], _ => false);

        Assert.Equal(string.Empty, landing.Route);
        Assert.Contains("No page is available", landing.Because, StringComparison.Ordinal);
    }

    [Fact]
    public void PersonaFor_reports_the_highest_priority_role_and_a_placeholder_otherwise()
    {
        Assert.Equal("Administrator", OgPersonaLanding.PersonaFor([OgRoleNames.Viewer, OgRoleNames.SystemAdmin]));
        Assert.Equal("HSE officer", OgPersonaLanding.PersonaFor([OgRoleNames.HseOfficer]));
        Assert.Equal(OgPersonaLanding.NoPersona, OgPersonaLanding.PersonaFor([]));
        Assert.Equal(OgPersonaLanding.NoPersona, OgPersonaLanding.PersonaFor(null));
        Assert.Equal(OgPersonaLanding.NoPersona, OgPersonaLanding.PersonaFor(["NOT-A-ROLE"]));
    }

    [Fact]
    public void Every_persona_route_exists_in_the_page_catalog()
    {
        // A mapping to a route nobody declared would land the shell on nothing.
        foreach (var (_, persona, route) in OgPersonaLanding.All)
        {
            Assert.True(
                OgModuleCatalog.Find(route) is not null,
                $"The {persona} persona maps to '{route}', which is not a declared page.");
        }
    }

    [Fact]
    public void The_session_persona_is_resolved_from_the_roles_not_hard_coded()
    {
        // The bootstrap grants the administrator role, so the session must say Administrator.
        using var application = new OgBootstrapper().Create();

        Assert.Contains(OgRoleNames.SystemAdmin, application.Permissions.Roles);
        Assert.Equal("Administrator", application.Session.PersonaCode);
        Assert.NotEqual("ADMINISTRATOR", application.Session.PersonaCode);
    }

    [Fact]
    public void A_local_sign_in_produces_an_administrator_persona()
    {
        var session = OgSignInService.BuildSession(
            new OgSignedInUser("admin", "Local Administrator"),
            setup: null);

        Assert.Equal("Administrator", session.PersonaCode);
    }
}
