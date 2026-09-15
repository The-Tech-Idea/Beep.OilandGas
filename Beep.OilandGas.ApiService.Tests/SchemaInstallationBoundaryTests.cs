using Beep.OilandGas.ApiService.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class SchemaInstallationBoundaryTests
{
    [Fact]
    public void UnboundSchemaInstallationControllerIsNotExposed()
    {
        var controllers = typeof(ModuleRepositoryController).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(ControllerBase).IsAssignableFrom(x));
        foreach (var controller in controllers)
        {
            var routes = controller.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>();
            Assert.DoesNotContain(routes, route => route.Template.Trim('/') == "api/ppdm39/schema");
        }
    }

    [Fact]
    public void BoundModuleInstallationRequiresAdministratorRole()
    {
        var authorization = Assert.Single(typeof(ModuleRepositoryController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Administrator", authorization.Roles);
        Assert.Empty(typeof(ModuleRepositoryController).GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        foreach (var method in typeof(ModuleRepositoryController).GetMethods())
            Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }
}
