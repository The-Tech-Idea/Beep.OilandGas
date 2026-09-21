using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class ProductionEngineerWorkspaceTests
{
    [Theory]
    [InlineData("/production/engineer-workspace")]
    [InlineData("/production/operations")]
    public void WorkspaceRequiresAuthentication(string destination)
    {
        var page = Assert.Single(typeof(ApiClient).Assembly.GetTypes(), type =>
            type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>()
                .Any(route => route.Template == destination));
        Assert.NotEmpty(page.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Empty(page.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }

    [Fact]
    public void EngineerToolAndMonitoringDestinationsExist()
    {
        var routes = typeof(ApiClient).Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>())
            .Select(route => route.Template).ToHashSet(StringComparer.OrdinalIgnoreCase);
        string[] destinations = [
            "/ppdm39/calculations/nodalanalysis", "/ppdm39/calculations/gaslift",
            "/ppdm39/pumps/suckerrodpumping", "/ppdm39/pumps/plungerlift",
            "/ppdm39/calculations/pipelineanalysis", "/ppdm39/calculations/compressor-analysis",
            "/ppdm39/calculations/choke-analysis", "/ppdm39/calculations/well-test-analysis",
            "/production", "/production/well-performance", "/production/intervention",
            "/production/forecasting", "/ppdm39/production/well-tests",
            "/production/allocation", "/ppdm39/production/reporting",
            "/ppdm39/production/reserves", "/ppdm39/operations/enhancedrecovery"
        ];
        foreach (var destination in destinations) Assert.Contains(destination, routes);
    }
}
