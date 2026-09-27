using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// S3-06. The API's identity settings: required, per environment, never defaulted — and the shipped files carry no
/// environment's value.
/// </summary>
/// <remarks>
/// The validation rules themselves (https, placeholders, the audience) are the identity server's client library's, tested
/// there; what is pinned here is that the API asks for them and that its files are shaped for it.
/// </remarks>
public class ApiIdentityConfigurationTests
{
    [Theory]
    [InlineData("IdentityServer:Authority", null)]
    [InlineData("IdentityServer:Authority", "http://idp.oilgas.test/")]
    [InlineData("IdentityServer:Authority", "REPLACE_ME")]
    [InlineData("IdentityServer:Audience", null)]
    [InlineData("IdentityServer:Audience", " ")]
    [InlineData("IdentityServer:Audience", "REPLACE_ME")]
    public void A_missing_or_unfilled_identity_setting_stops_the_API_at_startup(string key, string? value)
    {
        var settings = new Dictionary<string, string?>
        {
            ["IdentityServer:Authority"] = "https://idp.oilgas.test/",
            ["IdentityServer:Audience"] = "https://oilgas-api.test/api",
            [key] = value
        };

        Assert.Throws<InvalidOperationException>(() => new ServiceCollection()
            .AddOilGasApiIdentity(new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
    }

    [Fact]
    public void The_shipped_settings_hold_no_environment_s_identity_server_API_identifier_or_origins()
    {
        using var shipped = Read("appsettings.json");

        Assert.False(shipped.RootElement.TryGetProperty("IdentityServer", out _));
        Assert.False(shipped.RootElement.TryGetProperty("Cors", out _));
        Assert.False(shipped.RootElement.TryGetProperty("Repository", out _));
    }

    [Fact]
    public void Development_names_the_local_identity_server_and_this_API_s_own_identifier()
    {
        using var development = Read("appsettings.Development.json");
        var identity = development.RootElement.GetProperty("IdentityServer");

        Assert.Equal("https", new Uri(identity.GetProperty("Authority").GetString()!).Scheme);
        var audience = identity.GetProperty("Audience").GetString();
        Assert.False(string.IsNullOrWhiteSpace(audience));
        Assert.NotEqual("beep-api", audience);
        Assert.False(identity.TryGetProperty("Introspection", out _));
        Assert.False(identity.TryGetProperty("ValidationMode", out _));
    }

    [Fact]
    public void The_production_template_leaves_every_deployment_value_to_the_server()
    {
        using var template = Read("appsettings.Production.template.json");
        var identity = template.RootElement.GetProperty("IdentityServer");

        Assert.Equal("REPLACE_ME", identity.GetProperty("Authority").GetString());
        Assert.Equal("REPLACE_ME", identity.GetProperty("Audience").GetString());
        Assert.Equal("REPLACE_ME", template.RootElement.GetProperty("Repository").GetProperty("ConnectionString").GetString());
        Assert.Equal(["REPLACE_ME"], template.RootElement.GetProperty("Cors").GetProperty("AllowedOrigins")
            .EnumerateArray().Select(origin => origin.GetString()));
    }

    [Fact]
    public void The_log_is_on_under_IIS()
    {
        var webConfig = File.ReadAllText(Path.Combine(ProjectDirectory(), "web.config"));

        Assert.Contains("stdoutLogEnabled=\"true\"", webConfig, StringComparison.Ordinal);
        Assert.DoesNotContain("processPath", webConfig, StringComparison.Ordinal);
        Assert.DoesNotContain("hostingModel", webConfig, StringComparison.Ordinal);
    }

    private static JsonDocument Read(string file) => JsonDocument.Parse(
        File.ReadAllText(Path.Combine(ProjectDirectory(), file)),
        new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });

    private static string ProjectDirectory()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "Beep.OilandGas.ApiService")))
            root = root.Parent;
        Assert.NotNull(root);
        return Path.Combine(root.FullName, "Beep.OilandGas.ApiService");
    }
}
