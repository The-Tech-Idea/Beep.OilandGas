using System.Net;
using System.Reflection;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Data.OilGas;
using Xunit;
using UsersPage = Beep.OilandGas.Web.Pages.Admin.AccessControl.Users;

namespace Beep.OilandGas.Web.Auth.Tests;

public class UserAdministrationStateTests
{
    [Theory]
    [InlineData("AssignRole")]
    [InlineData("RemoveRole")]
    public async Task RoleMutationsDoNotRunBeforeMembershipsAreLoaded(string method)
    {
        var page = new Beep.OilandGas.Web.Pages.Admin.AccessControl.UserRoles();
        var type = page.GetType();
        type.GetField("_selectedUserId", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, "user");
        type.GetField("_selectedRole", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, "Administrator");
        // No API client is injected: reaching a mutation would fail this test.
        await (Task)type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, method == "RemoveRole" ? new object[] { "Administrator" } : null)!;
        Assert.Equal(false, type.GetField("_busy", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task FailedReloadClearsPreviouslyVisibleUsersAndSelection(HttpStatusCode status)
    {
        using var handler = new FailureHandler(status);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example") };
        var page = new UsersPage();
        typeof(UsersPage).GetProperty("Accounts", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(page, new UserAdministrationClient(new ApiClient(http, new RecordingFailureReporter())));
        var user = new RepositoryUserSummary("user", "user", null, "Name", true, "version");
        Field("_users").SetValue(page, new List<RepositoryUserSummary> { user });
        Field("_selected").SetValue(page, user);

        await (Task)typeof(UsersPage).GetMethod("ReloadAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(page, null)!;

        Assert.Empty((List<RepositoryUserSummary>)Field("_users").GetValue(page)!);
        Assert.Null(Field("_selected").GetValue(page));
        Assert.NotNull(Field("_error").GetValue(page));
        Assert.Equal(false, Field("_busy").GetValue(page));
    }

    private static FieldInfo Field(string name) =>
        typeof(UsersPage).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    private sealed class FailureHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent("{}") });
    }
}
