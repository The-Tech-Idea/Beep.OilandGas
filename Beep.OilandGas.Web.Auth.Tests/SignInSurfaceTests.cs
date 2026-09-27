using System.Text.Json;
using System.Text.RegularExpressions;
using Beep.Foundation.IdentityServer.Shared.Extensions;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>
/// S3-06. Sign-in, registration and sign-out are the identity server's client library's; the Web's account pages are its
/// own, at the library's addresses; the API's address and the deployment files are shaped for each environment.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> the link and route arms read source and routes. A link assembled from pieces, or a route added at run
/// time, passes.
/// </remarks>
public sealed class SignInSurfaceTests
{
    private static readonly string WebRoot = FindWebRoot();

    /// <summary>
    /// The Web linked to <c>/login</c>, <c>/register</c> and <c>/authentication/logout</c>, none of which exists since sign-in
    /// moved to the library — a page asking a signed-out person to sign in sent them to "not found".
    /// </summary>
    [Fact]
    public void No_sign_in_or_account_route_is_written_as_text()
    {
        var literal = new Regex(
            "\"/(login|register|logout|signin|signout|clientapp|authentication/|Account/)|\"/account/(login|logout|register|access-denied|link|manage|signin)",
            RegexOptions.IgnoreCase);

        var offenders = Directory.EnumerateFiles(WebRoot, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".razor", StringComparison.Ordinal) || path.EndsWith(".cs", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, number: index + 1)))
            .Where(entry => literal.IsMatch(entry.line) && !entry.line.TrimStart().StartsWith("@page", StringComparison.Ordinal))
            .Select(entry => $"{Path.GetRelativePath(WebRoot, entry.path)}:{entry.number}")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>The library maps its sign-in routes; a page on one of them is an ambiguous route at the first request.</summary>
    [Fact]
    public void No_page_claims_a_route_the_library_maps()
    {
        var claimed = Routes().Where(route => BeepClientEndpoints.SdkPaths.Contains(route, StringComparer.OrdinalIgnoreCase)).ToList();

        Assert.Empty(claimed);
    }

    /// <summary>The account page and the refusal page are the Web's, at the addresses the library sends people to.</summary>
    [Fact]
    public void The_account_and_refusal_pages_are_served_at_the_library_s_addresses()
    {
        var routes = Routes().ToList();

        Assert.Single(routes, route => route == BeepClientEndpoints.Manage);
        Assert.Single(routes, route => route == BeepClientEndpoints.AccessDenied);
    }

    /// <summary>
    /// The menu holding sign-in, the account page and sign-out opens. Since MudBlazor 9 custom activator content calls the
    /// menu's context itself; this one took the click and never opened, so none of the three could be reached.
    /// </summary>
    /// <remarks>
    /// Only menus carrying the library's entries are held to it — the Web's own navigation menus are its own. A menu is
    /// read up to its first closing tag, so a sign-in entry inside a nested menu is judged by the inner one.
    /// </remarks>
    [Fact]
    public void The_sign_in_and_account_menu_opens()
    {
        var menus = Directory.EnumerateFiles(WebRoot, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => Regex.Matches(Regex.Replace(File.ReadAllText(path), @"@\*[\s\S]*?\*@", " "), @"<MudMenu\b[\s\S]*?</MudMenu>")
                .Select(match => (File: Path.GetFileName(path), Body: match.Value)))
            .Where(menu => menu.Body.Contains("BeepClientEndpoints.", StringComparison.Ordinal)
                || menu.Body.Contains("<SignOutButton", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(menus);
        Assert.Empty(menus
            .Where(menu => Regex.Match(menu.Body, @"<ActivatorContent\b[^>]*>(?<body>[\s\S]*?)</ActivatorContent>") is { Success: true } activator
                && !Regex.IsMatch(activator.Groups["body"].Value, @"\.(ToggleAsync|OpenAsync)\b"))
            .Select(menu => menu.File));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("https://REPLACE_ME.example")]
    [InlineData("http://api.oilgas.test")]
    [InlineData("api.oilgas.test")]
    public void The_API_s_address_is_required_and_https(string? value)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [OilGasApiAddress.Key] = value })
            .Build();

        var refused = Assert.Throws<InvalidOperationException>(() => OilGasApiAddress.Resolve(configuration));
        Assert.Contains(OilGasApiAddress.Key, refused.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void The_API_s_address_is_the_configured_one()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [OilGasApiAddress.Key] = "https://api.oilgas.test/base" })
            .Build();

        var api = OilGasApiAddress.Resolve(configuration);

        Assert.Equal(new Uri("https://api.oilgas.test/base/"), api.Address);
        Assert.Equal(new Uri("https://api.oilgas.test/base/progressHub"), api.For("progressHub"));
    }

    [Fact]
    public void The_production_template_leaves_every_deployment_value_to_the_server_and_the_log_is_on()
    {
        using var template = JsonDocument.Parse(File.ReadAllText(Path.Combine(WebRoot, "appsettings.Production.template.json")),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var identity = template.RootElement.GetProperty("IdentityServer");
        foreach (var key in new[] { "Authority", "ClientId", "ClientSecret" })
            Assert.Equal("REPLACE_ME", identity.GetProperty(key).GetString());
        Assert.Equal("REPLACE_ME", template.RootElement.GetProperty("ApiService").GetProperty("BaseUrl").GetString());

        using var shipped = JsonDocument.Parse(File.ReadAllText(Path.Combine(WebRoot, "appsettings.json")),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        Assert.False(shipped.RootElement.TryGetProperty("IdentityServer", out _));
        Assert.False(shipped.RootElement.TryGetProperty("ApiService", out _));

        var webConfig = File.ReadAllText(Path.Combine(WebRoot, "web.config"));
        Assert.Contains("stdoutLogEnabled=\"true\"", webConfig, StringComparison.Ordinal);
        Assert.DoesNotContain("processPath", webConfig, StringComparison.Ordinal);
    }

    private static IEnumerable<string> Routes() =>
        typeof(RepositoryAccountClient).Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>())
            .Select(route => route.Template);

    private static string FindWebRoot()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.Web")))
            root = root.Parent;
        Assert.NotNull(root);
        return Path.Combine(root.FullName, "Beep.OilandGas.Web");
    }
}
