using System.Reflection;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class WebStartupDependencyTests
{
    [Fact]
    public void SharedAuthenticationCanLoadItsRequiredOidcRuntime()
    {
        var required = Assert.Single(typeof(PartyIdClaims).Assembly.GetReferencedAssemblies(),
            assembly => assembly.Name == "Microsoft.AspNetCore.Authentication.OpenIdConnect");
        var actual = Assembly.Load(required).GetName();
        Assert.True(actual.Version >= required.Version,
            $"Shared authentication requires {required.Version}, but the host resolved {actual.Version}.");
    }
}
