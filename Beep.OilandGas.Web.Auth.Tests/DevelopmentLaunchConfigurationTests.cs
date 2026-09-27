using System.Text.Json;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class DevelopmentLaunchConfigurationTests
{
    [Fact]
    public void DevelopmentRepositoryBelongsToApiAndUsesLocalDb()
    {
        var root = FindRepositoryRoot();
        using var api = JsonDocument.Parse(File.ReadAllText(Path.Combine(root,
            "Beep.OilandGas.ApiService", "appsettings.Development.json")));
        var repository = api.RootElement.GetProperty("Repository");
        Assert.Equal("SqlServer", repository.GetProperty("Provider").GetString());
        var connection = new System.Data.Common.DbConnectionStringBuilder
        {
            ConnectionString = repository.GetProperty("ConnectionString").GetString()
        };
        Assert.Equal(@"(localdb)\MSSQLLocalDB", connection["Server"]);
        Assert.Equal("BeepOilGasRepository", connection["Database"]);
        Assert.Equal("true", connection["Integrated Security"].ToString(), ignoreCase: true);

        foreach (var file in new[] { "appsettings.json", "appsettings.Development.json" })
        {
            using var web = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Beep.OilandGas.Web", file)),
                new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            Assert.False(web.RootElement.TryGetProperty("ConnectionStrings", out _));
            Assert.False(web.RootElement.TryGetProperty("Repository", out _));
        }
    }

    [Fact]
    public void DevelopmentSignInAsksForTheOilGasApiAndTheAccountScopesOnly()
    {
        var root = FindRepositoryRoot();
        using var web = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Beep.OilandGas.Web", "appsettings.Development.json")),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        using var api = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Beep.OilandGas.ApiService", "appsettings.Development.json")),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var identity = web.RootElement.GetProperty("IdentityServer");

        // The Web asks for the API's own identifier — the audience the API validates — and nothing shared or role-bearing.
        var apiScopes = identity.GetProperty("ApiScopes").EnumerateArray().Select(value => value.GetString()).ToArray();
        Assert.Equal([api.RootElement.GetProperty("IdentityServer").GetProperty("Audience").GetString()], apiScopes);
        var accountScopes = identity.GetProperty("AccountScopes").EnumerateArray().Select(value => value.GetString()!).ToArray();
        Assert.NotEmpty(accountScopes);
        Assert.All(accountScopes, scope => Assert.StartsWith("account.", scope, StringComparison.Ordinal));
        Assert.DoesNotContain("beep-api", apiScopes.Concat(accountScopes));
        Assert.DoesNotContain("role", apiScopes.Concat(accountScopes));
        Assert.DoesNotContain("roles", apiScopes.Concat(accountScopes));

        // Both hosts name the same identity server; the client id and secret are secrets of the machine (user-secrets).
        Assert.Equal(identity.GetProperty("Authority").GetString(),
            api.RootElement.GetProperty("IdentityServer").GetProperty("Authority").GetString());
        foreach (var file in new[] { "appsettings.json", "appsettings.Development.json" })
        {
            using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Beep.OilandGas.Web", file)),
                new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            if (settings.RootElement.TryGetProperty("IdentityServer", out var section))
            {
                Assert.False(section.TryGetProperty("ClientId", out _), file);
                Assert.False(section.TryGetProperty("ClientSecret", out _), file);
            }
            Assert.False(settings.RootElement.TryGetProperty("Authentication", out _), file);
        }
    }

    private static string FindRepositoryRoot()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.Web")))
            root = root.Parent;
        Assert.NotNull(root);
        return root.FullName;
    }

    [Fact]
    public void DefaultDevelopmentProfilesMatchConfiguredHttpsEndpoints()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.Web")))
            root = root.Parent;
        Assert.NotNull(root);
        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root.FullName,
            "Beep.OilandGas.Web", "appsettings.Development.json")), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        foreach (var (project, section) in new[] { ("Beep.OilandGas.ApiService", "ApiService") })
        {
            using var launch = JsonDocument.Parse(File.ReadAllText(Path.Combine(root.FullName, project,
                "Properties", "launchSettings.json")));
            var first = launch.RootElement.GetProperty("profiles").EnumerateObject().First();
            Assert.Equal("https", first.Name);
            Assert.Equal("Development", first.Value.GetProperty("environmentVariables").GetProperty("ASPNETCORE_ENVIRONMENT").GetString());
            var configured = settings.RootElement.GetProperty(section).GetProperty("BaseUrl").GetString()!;
            Assert.Equal("https", new Uri(configured).Scheme);
            Assert.Contains(first.Value.GetProperty("applicationUrl").GetString()!.Split(';'),
                url => new Uri(url) == new Uri(configured));
        }
    }
}
