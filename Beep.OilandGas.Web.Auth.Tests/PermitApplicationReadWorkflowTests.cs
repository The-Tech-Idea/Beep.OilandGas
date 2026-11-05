#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.ApiService.Controllers.Permits;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.Web.Pages.PPDM39.Permits;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Detail = TheTechIdea.Data.OilGas.PermitApplicationDetail;
using Summary = TheTechIdea.Data.OilGas.PermitApplicationSummary;
using NativeCheck = Beep.OilandGas.Models.Data.PermitsAndApplications.PermitComplianceResult;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class PermitApplicationReadWorkflowTests
{
    private const string Id = "permit/شركة?one#2";
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Safety")], "test"))));
    }
    private static Detail Source(string id = Id) => new() { Application = new Summary { PermitApplicationId = id, FieldId = "field", ApplicantName = "Recorded applicant", ApplicationType = "Drilling", RegulatoryAuthority = "Authority", Status = "Submitted" }, StatusHistory = [new() { HistoryId = "history", ApplicationId = id, Status = "Submitted", UpdatedBy = "issuer" }], ComplianceResult = new() { ApplicationId = id, FieldId = "field", CheckedAtUtc = DateTimeOffset.UtcNow, IsCompliant = true, ComplianceScore = 100 } };
    [Theory]
    [InlineData("valid")]
    [InlineData("foreign-history")]
    [InlineData("duplicate-history")]
    [InlineData("changed-field")]
    [InlineData("missing-check")]
    [InlineData("invalid-check")]
    public async Task PermitApiMapsCanonicalHistoryAndCheckerWithoutRawNativePayload(string mode)
    {
        var fieldId = "field"; var lifecycle = new Mock<IPermitApplicationLifecycleService>(); var workflow = new Mock<IPermitApplicationWorkflowService>(); var history = new Mock<IPermitStatusHistoryService>(); var checks = new Mock<IPermitComplianceCheckService>(); var fields = new Mock<IFieldOrchestrator>(); fields.SetupGet(x => x.CurrentFieldId).Returns(() => fieldId);
        lifecycle.Setup(x => x.GetByIdAsync(Id)).ReturnsAsync(new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = Id, FIELD_ID = "field", APPLICANT_ID = "Recorded applicant", STATUS = PermitApplicationStatus.Submitted });
        var row = new PERMIT_STATUS_HISTORY { PERMIT_STATUS_HISTORY_ID = "history", PERMIT_APPLICATION_ID = mode == "foreign-history" ? "foreign" : Id, STATUS = "Submitted", STATUS_REMARKS = "Recorded application received", UPDATED_BY = "issuer" };
        history.Setup(x => x.GetHistoryAsync(Id)).ReturnsAsync(mode == "duplicate-history" ? [row, row] : [row]);
        checks.Setup(x => x.CheckComplianceAsync(Id, It.IsAny<string?>())).ReturnsAsync(() => { if (mode == "changed-field") fieldId = "other"; return mode == "missing-check" ? null! : new NativeCheck { IsCompliant = true, ComplianceScore = 100, Violations = mode == "invalid-check" ? ["Required evidence is missing"] : [] }; });
        var controller = new PermitsController(lifecycle.Object, workflow.Object, checks.Object, history.Object, fields.Object, Mock.Of<ICompanyPermitApplicationDataAccess>());
        if (mode != "valid") { await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetByIdAsync(Id)); if (mode.Contains("history")) checks.VerifyNoOtherCalls(); return; }
        var response = await controller.GetByIdAsync(Id); var detail = Assert.IsType<Detail>(Assert.IsType<OkObjectResult>(response.Result).Value);
        PermitApplicationReadValidator.Require(detail, Id, "field"); Assert.Equal("Recorded applicant", detail.Application.ApplicantName); Assert.Equal("issuer", Assert.Single(detail.StatusHistory).UpdatedBy); Assert.Null(detail.StatusHistory[0].StatusDate);
        var json = JsonSerializer.Serialize(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web)); Assert.DoesNotContain("PERMIT_APPLICATION_ID", json); Assert.Equal(Id, JsonSerializer.Deserialize<Detail>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Application.PermitApplicationId);
        Assert.Equal(typeof(PermitApplicationCreationProposal).Assembly, typeof(Detail).Assembly);
    }
    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> send) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(send(request)); }
    [Theory][InlineData("valid")][InlineData("foreign-application")][InlineData("foreign-history")][InlineData("foreign-check")]
    public async Task PermitTypedReadUsesExactQueryAndRefusesUnboundWireEvidence(string mode)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ViewPermits"])));
        var detail = Source(); if (mode == "foreign-application") detail.Application.PermitApplicationId = "foreign"; if (mode == "foreign-history") detail.StatusHistory[0].ApplicationId = "foreign"; if (mode == "foreign-check") detail.ComplianceResult!.FieldId = "other";
        using var http = new HttpClient(new Handler(request => { Assert.Equal("/api/field/current/permits/application?applicationId=" + Uri.EscapeDataString(Id), request.RequestUri!.PathAndQuery); return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(detail, new JsonSerializerOptions(JsonSerializerDefaults.Web)), System.Text.Encoding.UTF8, "application/json") }; })) { BaseAddress = new("https://permit.test") };
        var client = new PermitServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        if (mode == "valid") Assert.Equal(Id, (await client.GetByIdAsync(Id))!.Application.PermitApplicationId); else await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetByIdAsync(Id));
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("missing")]
    public async Task PermitCheckerReadRequiresExactRecordedApplicationEvidence(string mode)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ViewPermits"])));
        var check = Source().ComplianceResult!; if (mode == "foreign") check.ApplicationId = "other";
        using var http = new HttpClient(new Handler(request => { Assert.Equal("/api/field/current/permits/application-check?applicationId=" + Uri.EscapeDataString(Id), request.RequestUri!.PathAndQuery); return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(mode == "missing" ? null : check, new JsonSerializerOptions(JsonSerializerDefaults.Web)), System.Text.Encoding.UTF8, "application/json") }; })) { BaseAddress = new("https://permit.test") };
        var client = new PermitServiceClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        if (mode == "valid") Assert.Equal(Id, (await client.CheckComplianceAsync(Id)).ApplicationId); else await Assert.ThrowsAsync<InvalidOperationException>(() => client.CheckComplianceAsync(Id));
    }
    [Theory][InlineData("valid")][InlineData("foreign-field")][InlineData("foreign-history")][InlineData("foreign-check")][InlineData("denied")][InlineData("missing")]
    public void PermitDetailPublishesOnlyCurrentPermittedCanonicalEvidence(string mode)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], mode == "denied" ? [] : ["HSE.ViewPermits"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field"); var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var detail = Source(); if (mode == "foreign-field") detail.Application.FieldId = "other"; if (mode == "foreign-history") detail.StatusHistory[0].ApplicationId = "other"; if (mode == "foreign-check") detail.ComplianceResult!.ApplicationId = "other";
        var client = new Mock<IPermitServiceClient>(); client.Setup(x => x.GetByIdAsync(Id, It.IsAny<CancellationToken>())).ReturnsAsync(mode == "missing" ? null : detail);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<IPermitApplicationIntentStore>(Mock.Of<IPermitApplicationIntentStore>()); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>())); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/ppdm39/permits/detail?applicationId=" + Uri.EscapeDataString(Id)); var page = ui.RenderComponent<PermitDetail>();
        if (mode == "valid") { page.WaitForAssertion(() => Assert.Contains("Recorded applicant", page.Markup)); Assert.Contains("issuer", page.Markup); Assert.Contains("Automated checker result", page.Markup); Assert.Contains("Not recorded", page.Markup); Assert.DoesNotContain("Submit for Review", page.Markup); Assert.Empty(page.FindAll("button").Where(b => b.TextContent.Trim() is "Approve" or "Reject")); company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("Recorded applicant", page.Markup)); }
        else { page.WaitForAssertion(() => Assert.Contains("could not be confirmed", page.Markup)); Assert.DoesNotContain("Recorded applicant", page.Markup); if (mode == "denied") client.Verify(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never); }
    }
    [Fact]
    public async Task PermitDetailDiscardsLateCompanyResponseAndReloadsChangedQuery()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ViewPermits"])));
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field"); var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "field", SelectionAvailable = true, SelectionContext = context });
        var pending = new TaskCompletionSource<Detail?>(TaskCreationOptions.RunContinuationsAsynchronously); var client = new Mock<IPermitServiceClient>(); client.Setup(x => x.GetByIdAsync(Id, It.IsAny<CancellationToken>())).Returns(pending.Task); client.Setup(x => x.GetByIdAsync("second", It.IsAny<CancellationToken>())).ReturnsAsync(Source("second"));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object); ui.Services.AddSingleton<IPermitApplicationIntentStore>(Mock.Of<IPermitApplicationIntentStore>()); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>())); ui.Services.AddSingleton<AuthenticationStateProvider>(new Auth());
        var navigation = ui.Services.GetRequiredService<NavigationManager>(); navigation.NavigateTo("/ppdm39/permits/detail?applicationId=" + Uri.EscapeDataString(Id)); var page = ui.RenderComponent<PermitDetail>(); company.Clear(); pending.SetResult(Source()); await page.InvokeAsync(() => Task.CompletedTask); Assert.DoesNotContain("Recorded applicant", page.Markup);
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Safety"], ["HSE.ViewPermits"]))); navigation.NavigateTo("/ppdm39/permits/detail?applicationId=second"); page.SetParametersAndRender(); page.WaitForAssertion(() => Assert.Contains("application second", page.Markup)); client.Verify(x => x.GetByIdAsync("second", It.IsAny<CancellationToken>()), Times.Once);
    }
}
#endif
