using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class SchemaManagementRouteTests
{
    [Theory]
    [InlineData("/first-login")]
    [InlineData("/register-callback")]
    public void ObsoleteGlobalDatabaseSetupRoutesAreNotExposed(string route)
    {
        Assert.DoesNotContain(typeof(RepositoryAccountClient).Assembly.GetTypes(), type =>
            type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>()
                .Any(attribute => attribute.Template == route));
    }

    [Theory]
    [InlineData("/ppdm39/data-management/schema")]
    [InlineData("/ppdm39/create-database")]
    [InlineData("/ppdm39/create-database/target")]
    [InlineData("/ppdm39/create-database/foundation")]
    [InlineData("/ppdm39/create-database/modules")]
    [InlineData("/ppdm39/create-database/summary")]
    [InlineData("/ppdm39/setup/beep-wizard")]
    public void SchemaManagementUsesTheAdministratorModuleInstallationPage(string path)
    {
        var page = Assert.Single(typeof(RepositoryAccountClient).Assembly.GetTypes().Where(type =>
            type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>()
                .Any(route => route.Template == path)));
        Assert.Contains(page.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            route => route.Template == "/admin/module-databases");
        var authorization = Assert.Single(page.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>());
        Assert.Equal("Administrator", authorization.Roles);
        Assert.Empty(page.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }
}
