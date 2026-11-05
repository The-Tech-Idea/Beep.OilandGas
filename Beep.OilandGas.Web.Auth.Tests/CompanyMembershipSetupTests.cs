using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyMembershipSetupTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Administrator")], "test"))));
    }
    private sealed class Memory : ICompanyMembershipIntentStore, ICompanyRoleIntentStore
    {
        public CompanyMembershipIntent? Value; public CompanyRoleIntent? RoleValue; public CompanyRoleRevocationIntent? Revocation; public CompanyMembershipRetirementIntent? Retirement; public bool Fail;
        public Task<CompanyMembershipIntent?> ReadMembershipAsync(CompanyScope scope, string actor) => Task.FromResult(Value?.Company == scope && Value.Actor == actor ? Value : null);
        public Task RetainMembershipAsync(CompanyMembershipIntent intent) { if (Fail || Value is not null && Value != intent) throw new InvalidOperationException(); Value = intent; return Task.CompletedTask; }
        public Task ClearMembershipAsync(CompanyMembershipIntent intent) { if (Value != intent) throw new InvalidOperationException(); Value = null; return Task.CompletedTask; }
        public Task<CompanyMembershipRetirementIntent?> ReadMembershipRetirementAsync(CompanyScope scope, string actor) => Task.FromResult(Retirement?.Company == scope && Retirement.Actor == actor ? Retirement : null);
        public Task RetainMembershipRetirementAsync(CompanyMembershipRetirementIntent intent) { if (Fail || Retirement is not null && Retirement != intent) throw new InvalidOperationException(); Retirement = intent; return Task.CompletedTask; }
        public Task ClearMembershipRetirementAsync(CompanyMembershipRetirementIntent intent) { if (Retirement != intent) throw new InvalidOperationException(); Retirement = null; return Task.CompletedTask; }
        public Task<CompanyRoleIntent?> ReadRoleAsync(CompanyScope scope, string actor) => Task.FromResult(RoleValue?.Company == scope && RoleValue.Actor == actor ? RoleValue : null);
        public Task RetainRoleAsync(CompanyRoleIntent intent) { if (Fail || RoleValue is not null && RoleValue != intent) throw new InvalidOperationException(); RoleValue = intent; return Task.CompletedTask; }
        public Task ClearRoleAsync(CompanyRoleIntent intent) { if (RoleValue != intent) throw new InvalidOperationException(); RoleValue = null; return Task.CompletedTask; }
        public Task<CompanyRoleRevocationIntent?> ReadRevocationAsync(CompanyScope scope, string actor) => Task.FromResult(Revocation?.Company == scope && Revocation.Actor == actor ? Revocation : null);
        public Task RetainRevocationAsync(CompanyRoleRevocationIntent intent) { if (Fail || Revocation is not null && Revocation != intent) throw new InvalidOperationException(); Revocation = intent; return Task.CompletedTask; }
        public Task ClearRevocationAsync(CompanyRoleRevocationIntent intent) { if (Revocation != intent) throw new InvalidOperationException(); Revocation = null; return Task.CompletedTask; }
    }
    private sealed class Handler : HttpMessageHandler
    {
        public bool LoseResponse; public bool LoseRoleResponse; public bool LoseRevocationResponse; public bool LoseRetirementResponse; public bool ForeignReview; public bool ForeignHistory; public bool ExpiredReview; public int Writes; public int RoleWrites; public int Revocations; public int Retirements; public CompanyMembershipRequest? Original; public CompanyMembershipRetirementRequest? OriginalRetirement;
        public bool Delay; public TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Delete)
            {
                Assert.Equal("/api/identity/roles/tenants/tenant/companies/company/assignments/assignment", request.RequestUri!.AbsolutePath);
                Revocations++;
                if (LoseRevocationResponse) { LoseRevocationResponse = false; throw new HttpRequestException("Response lost"); }
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            if (request.Method == HttpMethod.Post)
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/membership-retirements", StringComparison.Ordinal))
                {
                    var removal = JsonSerializer.Deserialize<CompanyMembershipRetirementRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                    if (OriginalRetirement is null) OriginalRetirement = removal; else Assert.Equal(OriginalRetirement, removal);
                    Retirements++;
                    if (LoseRetirementResponse) { LoseRetirementResponse = false; throw new HttpRequestException("Response lost"); }
                    result = new CompanyMembershipRetirement { IntentId = removal.IntentId, PreviousRevision = removal.ExpectedRevision, ResultRevision = Guid.NewGuid().ToString(), Reason = removal.Reason, ActorUserId = "actor", TenantId = "tenant", BusinessAssociateId = "company", UserId = "member" };
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
                }
                if (request.RequestUri!.AbsolutePath.EndsWith("/assignments", StringComparison.Ordinal))
                {
                    var role = JsonSerializer.Deserialize<AssignCompanyRoleRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                    RoleWrites++;
                    if (LoseRoleResponse) { LoseRoleResponse = false; throw new HttpRequestException("Response lost"); }
                    result = new RepositoryUserRole { TenantId = "tenant", BusinessAssociateId = "company", UserRoleId = "assignment", UserId = "member", RoleId = role.RoleId, GrantedByUserId = "actor", AssignmentReason = role.Reason, ApprovalStatus = "Approved" };
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
                }
                Assert.Equal("/api/identity/roles/tenants/tenant/companies/company/memberships", request.RequestUri!.AbsolutePath);
                var input = JsonSerializer.Deserialize<CompanyMembershipRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                if (Original is not null) Assert.Equal(Original, input); else Original = input;
                Writes++;
                if (LoseResponse) { LoseResponse = false; throw new HttpRequestException("Response lost"); }
                if (Delay) { Started.TrySetResult(); await Release.Task; }
                result = new RepositoryUserRole { TenantId = "tenant", BusinessAssociateId = "company", UserRoleId = input.IntentId, UserId = input.UserId, RoleId = input.RoleId, GrantedByUserId = "actor", AssignmentReason = input.Reason };
            }
            else if (request.RequestUri!.AbsolutePath == "/api/identity/users") result = new[] { new RepositoryUserSummary("member", "Member", null, null, true, "v"), new RepositoryUserSummary("disabled", "Disabled", null, null, false, "v") };
            else if (request.RequestUri.AbsolutePath == "/api/identity/roles") result = new[] { new RepositoryRoleSummary("engineer", "Engineer", null) };
            else if (request.RequestUri.AbsolutePath == "/api/identity/roles/tenants/tenant/companies/company/users/member/access")
                result = new CompanyMemberAccessReview(new("tenant", ForeignReview ? "foreign" : "company"), "member", ExpiredReview ? null : new(new("tenant", "company"), ["Engineer"], ["Wells.Read"])) { Membership = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ExpiredReview ? new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) : null, "actor", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "revision") };
            else if (request.RequestUri.AbsolutePath == "/api/identity/roles/tenants/tenant/companies/company/users/member/assignments")
                result = new[] { new RepositoryUserRole { TenantId = "tenant", BusinessAssociateId = "company", UserRoleId = "assignment", UserId = "member", RoleId = "engineer", ApprovalStatus = "Approved" } };
            else if (request.RequestUri.AbsolutePath == "/api/identity/roles/tenants/tenant/companies/company/users/member/membership-retirements")
                result = new CompanyMembershipRetirementPage(new("tenant", ForeignHistory ? "foreign" : "company"), "member", 0, 50, 1,
                    [new CompanyMembershipRetirement { TenantId = "tenant", BusinessAssociateId = "company", UserId = "member", IntentId = "reference", ActorUserId = "actor", Reason = "Transfer complete", RetiredUtc = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) }]);
            else if (request.RequestUri.AbsolutePath == "/api/identity/roles/tenants/tenant/companies/company/users/disabled/access")
                result = new CompanyMemberAccessReview(new("tenant", "company"), "disabled", null) { Membership = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null, "actor", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "revision") };
            else if (request.RequestUri.AbsolutePath == "/api/identity/roles/tenants/tenant/companies/company/users/disabled/assignments")
                result = new[] { new RepositoryUserRole { TenantId = "tenant", BusinessAssociateId = "company", UserRoleId = "assignment", UserId = "disabled", RoleId = "engineer", ApprovalStatus = "Approved" } };
            else throw new InvalidOperationException("Unexpected request.");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MembershipRemovalHistoryRequiresMatchingCompany(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { ForeignHistory = foreign }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"));
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewRetirements", Flags)!.Invoke(page.Instance, [0])!);
        await page.InvokeAsync(() => page.Render());
        if (foreign) { Assert.Contains("history could not be confirmed", page.Markup); Assert.DoesNotContain("Transfer complete", page.Markup); }
        else { Assert.Contains("Transfer complete", page.Markup); Assert.Contains("By: actor", page.Markup); }
    }

    [Fact]
    public async Task LostMembershipRemovalResponseRetriesTheSameReviewedRequest()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { LoseRetirementResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"));
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewCurrentAccess", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => { typeof(CompanyMembershipSetup).GetField("_retirementConfirmed", Flags)!.SetValue(page.Instance, true); typeof(CompanyMembershipSetup).GetField("_retirementReason", Flags)!.SetValue(page.Instance, "Transfer complete"); });
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("RetireMembership", Flags)!.Invoke(page.Instance, null)!);
        Assert.NotNull(memory.Retirement); Assert.Equal(1, handler.Retirements);
        page.Dispose(); var reloaded = ui.RenderComponent<CompanyMembershipSetup>();
        reloaded.WaitForAssertion(() => Assert.Contains("Membership removal for", reloaded.Markup));
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("RetireMembership", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.Null(memory.Retirement); Assert.Equal(2, handler.Retirements);
    }

    [Fact]
    public async Task InactiveAccountCanBeReviewedAndItsRoleRemovedButCannotBeGrantedMembership()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        var accounts = (List<RepositoryUserSummary>)typeof(CompanyMembershipSetup).GetField("_users", Flags)!.GetValue(page.Instance)!;
        Assert.Contains(accounts, x => x.UserId == "disabled" && !x.IsActive);
        await page.InvokeAsync(() => { typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "disabled"); typeof(CompanyMembershipSetup).GetField("_role", Flags)!.SetValue(page.Instance, "engineer"); typeof(CompanyMembershipSetup).GetField("_reason", Flags)!.SetValue(page.Instance, "Approved"); });
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("Save", Flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Writes); Assert.Null(memory.Value);
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewCurrentAccess", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render()); Assert.Contains("No current company access was confirmed", page.Markup);
        Assert.Contains("/admin/access-control/asset-access?user=disabled", page.Markup);
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewAssignments", Flags)!.Invoke(page.Instance, null)!);
        var assignments = (List<RepositoryUserRole>)typeof(CompanyMembershipSetup).GetField("_assignments", Flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("RevokeRole", Flags)!.Invoke(page.Instance, [Assert.Single(assignments)])!);
        Assert.Equal(1, handler.Revocations);
    }

    [Fact]
    public async Task ReviewedRoleRemovalRequiresConfirmation()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"));
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewAssignments", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        page.FindAll("button").Single(x => x.TextContent.Contains("Select role for removal")).Click();
        Assert.Equal(0, handler.Revocations);
        await page.FindAll("button").Single(x => x.TextContent.Contains("Confirm role removal")).ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        Assert.Equal(1, handler.Revocations); Assert.Null(memory.Revocation);
    }

    [Fact]
    public async Task UnconfirmedRoleRemovalIsRetainedAndNotSentAgain()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { LoseRevocationResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"));
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewAssignments", Flags)!.Invoke(page.Instance, null)!);
        var assignments = (List<RepositoryUserRole>)typeof(CompanyMembershipSetup).GetField("_assignments", Flags)!.GetValue(page.Instance)!;
        var assignment = Assert.Single(assignments);
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("RevokeRole", Flags)!.Invoke(page.Instance, [assignment])!);
        Assert.NotNull(memory.Revocation); Assert.Equal(1, handler.Revocations);
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("RevokeRole", Flags)!.Invoke(page.Instance, [assignment])!);
        Assert.Equal(1, handler.Revocations);
        page.Dispose(); var reloaded = ui.RenderComponent<CompanyMembershipSetup>();
        reloaded.WaitForAssertion(() => Assert.Contains("is unconfirmed", reloaded.Markup));
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ClearRevocationRetained", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.NotNull(memory.Revocation);
        await reloaded.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_revocationClearAcknowledged", Flags)!.SetValue(reloaded.Instance, true));
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ClearRevocationRetained", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.Null(memory.Revocation); Assert.Equal(1, handler.Revocations);
    }

    [Fact]
    public async Task LostResponseRetriesOriginalCompanyMembershipAfterReload()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { LoseResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => { typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"); typeof(CompanyMembershipSetup).GetField("_role", Flags)!.SetValue(page.Instance, "engineer"); typeof(CompanyMembershipSetup).GetField("_reason", Flags)!.SetValue(page.Instance, "Approved"); });
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("Save", Flags)!.Invoke(page.Instance, null)!);
        Assert.NotNull(memory.Value); page.Dispose();
        var reloaded = ui.RenderComponent<CompanyMembershipSetup>(); reloaded.WaitForAssertion(() => Assert.Contains("earlier membership request", reloaded.Markup));
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ClearRetained", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.NotNull(memory.Value);
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("Save", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.Null(memory.Value); Assert.Equal(2, handler.Writes);
    }

    [Fact]
    public async Task CompanyChangeDoesNotApplyPreviousMembershipResponseToNewSelection()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { Delay = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => { typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"); typeof(CompanyMembershipSetup).GetField("_role", Flags)!.SetValue(page.Instance, "engineer"); typeof(CompanyMembershipSetup).GetField("_reason", Flags)!.SetValue(page.Instance, "Approved"); });
        var pending = page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("Save", Flags)!.Invoke(page.Instance, null)!);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "other"), ["Administrator"], [])));
        handler.Release.SetResult(); await pending;
        page.WaitForAssertion(() => Assert.Equal(new CompanyScope("tenant", "other"), typeof(CompanyMembershipSetup).GetField("_scope", Flags)!.GetValue(page.Instance)));
        Assert.Equal(new CompanyScope("tenant", "company"), memory.Value?.Company);
        Assert.DoesNotContain("request was recorded", page.Markup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CurrentAccessReviewRequiresMatchingCompanyAndAccount(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { ForeignReview = foreign }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        var memory = new Memory(); ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"));
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewCurrentAccess", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        if (foreign) { Assert.Contains("could not be confirmed", page.Markup); Assert.DoesNotContain("Current roles: Engineer", page.Markup); }
        else { Assert.Contains("Active company membership", page.Markup); Assert.Contains("Membership starts:", page.Markup); Assert.Contains("Granted by:", page.Markup); Assert.Contains("Engineer", page.Markup); Assert.Contains("Wells.Read", page.Markup); }
    }

    [Fact]
    public async Task ExpiredMembershipShowsRecordedIntervalWithoutClaimingCurrentAccess()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { ExpiredReview = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"));
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewCurrentAccess", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("2026-02-01", page.Markup); Assert.Contains("No current company access was confirmed", page.Markup);
        Assert.DoesNotContain("Active company membership for", page.Markup);
    }

    [Fact]
    public async Task UnconfirmedRoleAssignmentIsRetainedAndCannotBeResentUntilReviewed()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler { LoseRoleResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var memory = new Memory(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company); ui.Services.AddSingleton(api); ui.Services.AddSingleton(new UserAdministrationClient(api));
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth()); ui.Services.AddSingleton<ICompanyMembershipIntentStore>(memory); ui.Services.AddSingleton<ICompanyRoleIntentStore>(memory);
        var page = ui.RenderComponent<CompanyMembershipSetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyMembershipSetup).GetField("_ready", Flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => { typeof(CompanyMembershipSetup).GetField("_user", Flags)!.SetValue(page.Instance, "member"); typeof(CompanyMembershipSetup).GetField("_role", Flags)!.SetValue(page.Instance, "engineer"); typeof(CompanyMembershipSetup).GetField("_reason", Flags)!.SetValue(page.Instance, "Approved"); });
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ReviewCurrentAccess", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("AssignRole", Flags)!.Invoke(page.Instance, null)!);
        Assert.NotNull(memory.RoleValue); Assert.Equal(1, handler.RoleWrites);
        await page.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("AssignRole", Flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(1, handler.RoleWrites);
        page.Dispose(); var reloaded = ui.RenderComponent<CompanyMembershipSetup>();
        reloaded.WaitForAssertion(() => Assert.Contains("role assignment outcome is unconfirmed", reloaded.Markup));
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ClearRoleRetained", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.NotNull(memory.RoleValue);
        await reloaded.InvokeAsync(() => typeof(CompanyMembershipSetup).GetField("_roleClearAcknowledged", Flags)!.SetValue(reloaded.Instance, true));
        await reloaded.InvokeAsync(() => (Task)typeof(CompanyMembershipSetup).GetMethod("ClearRoleRetained", Flags)!.Invoke(reloaded.Instance, null)!);
        Assert.Null(memory.RoleValue); Assert.Equal(1, handler.RoleWrites);
    }
}
#endif
