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
            using var web = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Beep.OilandGas.Web", file)));
            Assert.False(web.RootElement.TryGetProperty("ConnectionStrings", out _));
            Assert.False(web.RootElement.TryGetProperty("Repository", out _));
        }
    }

    [Fact]
    public void DevelopmentOidcDoesNotRequestExternalRoles()
    {
        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(FindRepositoryRoot(),
            "Beep.OilandGas.Web", "appsettings.Development.json")));
        var scopes = settings.RootElement.GetProperty("Authentication").GetProperty("Schemes")
            .GetProperty("OpenIdConnect").GetProperty("Scope").EnumerateArray()
            .Select(value => value.GetString()).ToArray();
        Assert.DoesNotContain("role", scopes);
        Assert.DoesNotContain("roles", scopes);
        Assert.Contains("openid", scopes);
        Assert.Contains("beep-api", scopes);
        Assert.Contains("offline_access", scopes);
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
            "Beep.OilandGas.Web", "appsettings.Development.json")));
        foreach (var (project, section) in new[] { ("Beep.OilandGas.ApiService", "ApiService"), ("Beep.OilandGas.Web", "WebApp") })
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
