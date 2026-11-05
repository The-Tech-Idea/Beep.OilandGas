using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Auth.Tests;

internal static class TestCompany
{
    public static CompanyClientContext Selected(string userId = "test-user")
    {
        var context = new CompanyClientContext();
        if (!context.TrySelect(context.Clear(), userId, new RepositoryCompanyAccess(new("test-tenant", "test-company"), [], [])))
            throw new InvalidOperationException("Test company selection could not be installed.");
        return context;
    }
}
