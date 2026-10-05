using System.Reflection;
using Beep.OilandGas.Web.Pages.Admin;
using Beep.OilandGas.Web.Pages.PPDM39;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class DatabaseWizardWorkflowTests
{
    [Theory]
    [InlineData("ChangeEnvironment", "Production")]
    [InlineData("ChangeBackup", true)]
    [InlineData("ChangeRestore", true)]
    [InlineData("ChangeEvidence", "new restore evidence")]
    public void ChangingPlanInputsClearsApprovalAndReview(string method, object value)
    {
        var page = new ModuleDatabases();
        Set(page, "_plan", "old plan");
        Set(page, "_approved", true);
        Set(page, "_confirmed", true);
        Set(page, "_riskAcknowledged", true);
        typeof(ModuleDatabases).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, [value]);
        Assert.Null(Get(page, "_plan"));
        Assert.Equal(false, Get(page, "_approved"));
        Assert.Equal(false, Get(page, "_confirmed"));
        Assert.Equal(false, Get(page, "_riskAcknowledged"));
    }

    [Fact]
    public void DatabaseConnectionManagementRequiresAdministrator()
    {
        var attribute = Assert.Single(typeof(DatabaseManagement).GetCustomAttributes<AuthorizeAttribute>());
        Assert.Equal("Administrator", attribute.Roles);
    }

    private static object? Get(object page, string name) =>
        page.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page);
    private static void Set(object page, string name, object value) =>
        page.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, value);
}
