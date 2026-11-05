using System.Reflection;
using System.Text.RegularExpressions;
using Beep.OilandGas.Web.Pages.Legal;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>
/// The privacy policy and terms of service the identity server's sign-in page for Beep Oil &amp; Gas links
/// (<c>https://oilandgas.thetechidea.app/privacy</c> and <c>/terms</c>): served at exactly those addresses, to anybody,
/// whether or not the repository is set up, and linked from the footer every page carries.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> no page is rendered here — the anonymity arm reads the pages' attributes (what the router and the
/// authorization middleware read), the gate arm calls the layout's own decision, and the footer arm reads the layout's
/// source. That the text says what the code does is the pages' own comments' and the reviewer's, not a test's.
/// </remarks>
public sealed class LegalPagesTests
{
    [Theory]
    [InlineData(LegalPages.Privacy, "/privacy")]
    [InlineData(LegalPages.Terms, "/terms")]
    public void Each_document_is_served_once_at_the_address_the_identity_server_links(string route, string linked)
    {
        Assert.Equal(linked, route);
        Assert.Single(Pages(), page => page.Routes.Contains(route));
    }

    /// <summary>
    /// Anybody reads them — a person deciding whether to create an account has not signed in. <c>[AllowAnonymous]</c> is
    /// what the router's <c>AuthorizeRouteView</c> and the endpoint's authorization both honour, so a fallback policy added
    /// later still lets them through.
    /// </summary>
    [Theory]
    [InlineData(LegalPages.Privacy)]
    [InlineData(LegalPages.Terms)]
    public void Each_document_is_public(string route)
    {
        var page = Assert.Single(Pages(), candidate => candidate.Routes.Contains(route)).Type;

        Assert.NotEmpty(page.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        Assert.Empty(page.GetCustomAttributes(typeof(AuthorizeAttribute), true));
    }

    /// <summary>
    /// The main layout sends every page but a few to the repository's setup page while the repository is not ready. The
    /// documents read nothing from it and are linked from outside the application, so they are among the few.
    /// </summary>
    [Theory]
    [InlineData("/privacy", true)]
    [InlineData("/terms", true)]
    [InlineData("/Privacy/", true)]
    [InlineData("/dashboard", false)]
    [InlineData("/privacy-settings", false)]
    public void The_repository_setup_gate_lets_the_documents_through(string path, bool exempt)
    {
        var decision = typeof(MainLayout).GetMethod("IsSetupExemptPath", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(decision);

        Assert.Equal(exempt, (bool)decision.Invoke(null, [path])!);
    }

    [Fact]
    public void The_footer_every_page_carries_links_both_documents()
    {
        var layout = Regex.Replace(File.ReadAllText(Path.Combine(WebRoot(), "Shared", "MainLayout.razor")), @"@\*[\s\S]*?\*@", " ");
        var footer = Regex.Match(layout, @"<!-- Footer -->[\s\S]*?</MudPaper>");
        Assert.True(footer.Success, "MainLayout.razor has no footer.");

        Assert.Contains("Href=\"@LegalPages.Privacy\"", footer.Value, StringComparison.Ordinal);
        Assert.Contains("Href=\"@LegalPages.Terms\"", footer.Value, StringComparison.Ordinal);
    }

    private static IEnumerable<(Type Type, string[] Routes)> Pages() =>
        typeof(RepositoryAccountClient).Assembly.GetTypes()
            .Select(type => (Type: type, Routes: type.GetCustomAttributes(typeof(RouteAttribute), true)
                .Cast<RouteAttribute>().Select(route => route.Template).ToArray()))
            .Where(page => page.Routes.Length > 0);

    private static string WebRoot()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.Web")))
            root = root.Parent;
        Assert.NotNull(root);
        return Path.Combine(root.FullName, "Beep.OilandGas.Web");
    }
}
