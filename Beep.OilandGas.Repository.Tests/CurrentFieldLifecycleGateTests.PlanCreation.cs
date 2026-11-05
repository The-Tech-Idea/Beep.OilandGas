using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.HSE;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.App.Services.Field;
using Beep.OilandGas.LifeCycle.Services.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CurrentFieldLifecycleGateTests
{
    private static PPDMHSEService PlanFacade(Fixture f, RepositoryUserProfileService owner) => new("A", f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, owner, f.Policy,
        new PPDMCompanyCorrectiveActionCommands(f.Gate, owner, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy), CompanyIncidentCommandTests.Commands(f));
    [Fact]
    public async Task Actual_HSE_plan_HTTP_and_SDK_create_retry_read_and_later_action_use_the_same_canonical_owner()
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!; var intent = CompanyCorrectiveActionPlanCommandTests.Request();
        using var boundary = new Boundary(f, owner); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        using var company = new CompanyClientContext(); var version = company.Clear(); Assert.True(company.TrySelect(version, "actor", new(context.Company, ["Administrator"], [])));
        var sdk = new HseIncidentClient(new ClientApiTransport(host.Http, new() { ApiBaseUrl = host.Url, MaxRetries = 3, RetryDelay = TimeSpan.Zero }, company), company);
        var catalog = await sdk.ReviewPlanCreationAsync("local", "A");
        intent = intent with { ExpectedRevision = catalog.Revision };
        var submission = new CreateCAPlanSubmission(context, intent);
        var created = await sdk.CreatePlanAsync("local", submission); Assert.Equal(intent.PlanId.ToString("N"), created.PlanId); Assert.Equal(context, created.Context); Assert.Equal(4, f.Writes.Count);
        Assert.Equal(created, await sdk.CreatePlanAsync("local", submission)); Assert.Equal(4, f.Writes.Count);
        Assert.Equal(created, await sdk.GetPlanAsync("local", context)); Assert.Equal(4, f.Writes.Count);
        using var added = await host.Http.PostAsJsonAsync(host.Url + "/api/field/current/hse/incidents/local/cas", CompanyCorrectiveActionCommandTests.Addition() with { ResponsibleBaId = intent.ResponsibleBaId });
        Assert.Equal(HttpStatusCode.OK, added.StatusCode); Assert.Equal(7, f.Writes.Count);
        Assert.Equal(created, await sdk.GetPlanAsync("local", context));
        await Assert.ThrowsAnyAsync<Exception>(() => sdk.CreatePlanAsync("local", submission)); Assert.Equal(7, f.Writes.Count);
        Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("foreign", 404)] [InlineData("missing-manage", 403)] [InlineData("stale-field", 409)] [InlineData("stale-revision", 409)]
    public async Task Actual_HSE_plan_HTTP_creation_enforces_account_case_write_and_submitted_FIELD_review(string fault, int expected)
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready();
        if (fault == "missing-manage") { f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ManageIncidents").ToArrayAsync()); await f.Db.SaveChangesAsync(); await f.Select("company"); }
        var owner = CompanyCorrectiveActionAuthorityTests.Profile(f); var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!;
        if (fault == "stale-field") context = context with { FieldId = "B" };
        if (fault == "stale-revision") context = context with { Revision = "old-review" };
        using var boundary = new Boundary(f, owner, fault != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, fault != "anonymous", typeof(HSEController));
        var url = host.Url + "/api/field/current/hse/incident/ca-plan?incidentId=" + (fault == "foreign" ? "foreign" : "local");
        using var response = await host.Http.PostAsJsonAsync(url, new CreateCAPlanSubmission(context, CompanyCorrectiveActionPlanCommandTests.Request()));
        Assert.True(expected == (int)response.StatusCode, $"Expected {expected}; actual {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}"); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("context")] [InlineData("intent")] [InlineData("planId")] [InlineData("planName")] [InlineData("planNumber")] [InlineData("responseType")] [InlineData("partyObservation")] [InlineData("roleObservation")] [InlineData("responsibleBaId")] [InlineData("expectedRevision")]
    public async Task Actual_HSE_plan_HTTP_creation_requires_every_explicit_JSON_member(string missing)
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f); var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var json = JsonSerializer.SerializeToNode(new CreateCAPlanSubmission(context, CompanyCorrectiveActionPlanCommandTests.Request()), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();
        if (missing is "context" or "intent") json.Remove(missing); else json["intent"]!.AsObject().Remove(missing);
        using var boundary = new Boundary(f, owner); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        using var response = await host.Http.PostAsync(host.Url + "/api/field/current/hse/incident/ca-plan?incidentId=local", new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData(false, 415)] [InlineData(true, 400)]
    public async Task Actual_HSE_plan_HTTP_creation_never_turns_a_bodyless_or_empty_call_into_resolution_or_creation(bool emptyObject, int expected)
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        using var boundary = new Boundary(f, owner); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        using var response = await host.Http.PostAsync(host.Url + "/api/field/current/hse/incident/ca-plan?incidentId=local", emptyObject ? new StringContent("{}", Encoding.UTF8, "application/json") : null);
        Assert.True(expected == (int)response.StatusCode, $"Expected {expected}; actual {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}"); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("allowed", 200)] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("foreign", 404)] [InlineData("missing-plan", 409)]
    public async Task Actual_HSE_plan_HTTP_resolution_is_a_read_with_independent_current_case_authority(string fault, int expected)
    {
        using var f = await CompanyCorrectiveActionAuthorityTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        if (fault == "missing-plan") f.Rows.RemoveAll(row => row is Beep.OilandGas.PPDM39.Models.HSE_INCIDENT_RESPONSE link && link.PROJECT_ID == CompanyCorrectiveActionAuthorityTests.Plan);
        using var boundary = new Boundary(f, owner, fault != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, fault != "anonymous", typeof(HSEController));
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incident/ca-plan?incidentId=" + (fault == "foreign" ? "foreign" : "local"));
        Assert.Equal(expected, (int)response.StatusCode);
        if (fault == "allowed") { var result = await response.Content.ReadFromJsonAsync<CAPlanResolution>(); Assert.Equal(CompanyCorrectiveActionAuthorityTests.Plan, result!.PlanId); Assert.Equal("local", result.IncidentId); Assert.Equal("actor", result.Context.ActorUserId); }
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task Actual_HSE_plan_creation_review_HTTP_and_SDK_return_recorded_choices_and_changed_reference_revision_without_writes()
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!;
        using var boundary = new Boundary(f, owner); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        using var company = new CompanyClientContext(); var version = company.Clear(); Assert.True(company.TrySelect(version, "actor", new(context.Company, ["Administrator"], [])));
        var sdk = new HseIncidentClient(new ClientApiTransport(host.Http, new() { ApiBaseUrl = host.Url }, company), company);
        f.Rows.AddRange(new object[] {
            new Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE { ACTION_TYPE = "retired-response", ACTIVE_IND = "N" },
            new Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE { ACTION_TYPE = "expired-response", ACTIVE_IND = "Y", EXPIRY_DATE = DateTime.UtcNow.AddDays(-1) },
            new Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE { ACTION_TYPE = "future-response", ACTIVE_IND = "Y", EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1) }
        });
        var review = await sdk.ReviewPlanCreationAsync("local", "A");
        Assert.Equal(context, review.Context); Assert.Equal("local", review.IncidentId); Assert.Equal(64, review.Revision.Length);
        Assert.Contains(review.Participants, row => row.PartyObservation == 17 && row.RoleObservation == 4 && row.BusinessAssociateId == "local-person");
        Assert.Equal(new[] { "recorded-action-response" }, review.ResponseTypes); Assert.DoesNotContain("STATUS", review.ResponseTypes);
        Assert.Equal(review.Revision, (await sdk.ReviewPlanCreationAsync("local", "A")).Revision);
        f.Rows.OfType<Beep.OilandGas.PPDM39.Models.R_PROJECT_BA_ROLE>().Single().REMARK = "Changed recorded role interpretation";
        Assert.NotEqual(review.Revision, (await sdk.ReviewPlanCreationAsync("local", "A")).Revision);
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("anonymous", 401)] [InlineData("denied-field", 403)] [InlineData("permission", 403)] [InlineData("foreign", 404)]
    [InlineData("existing-plan", 409)] [InlineData("missing-response", 409)] [InlineData("duplicate-response", 409)] [InlineData("non-utc-response", 409)] [InlineData("missing-participant", 409)]
    public async Task Actual_HSE_plan_creation_review_HTTP_requires_current_write_case_and_unambiguous_recorded_creation_evidence(string fault, int expected)
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        if (fault == "permission") { f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents).ToArrayAsync()); await f.Db.SaveChangesAsync(); }
        if (fault == "existing-plan") f.Rows.Add(new Beep.OilandGas.PPDM39.Models.HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 20, PROJECT_ID = "recorded-project", ACTIVE_IND = "N" });
        if (fault == "missing-response") f.Rows.RemoveAll(row => row is Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE reference && reference.ACTION_TYPE != "STATUS");
        if (fault == "duplicate-response") f.Rows.Add(new Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE { ACTION_TYPE = "recorded-action-response", ACTIVE_IND = "N" });
        if (fault == "non-utc-response") f.Rows.OfType<Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE>().Single(row => row.ACTION_TYPE != "STATUS").EFFECTIVE_DATE = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        if (fault == "missing-participant") f.Rows.RemoveAll(row => row is Beep.OilandGas.PPDM39.Models.HSE_INCIDENT_BA);
        using var boundary = new Boundary(f, owner, fault != "denied-field"); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, fault != "anonymous", typeof(HSEController));
        using var response = await host.Http.GetAsync(host.Url + "/api/field/current/hse/incident/ca-plan-review?incidentId=" + (fault == "foreign" ? "foreign" : "local"));
        Assert.True(expected == (int)response.StatusCode, $"Expected {expected}; actual {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}"); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Actual_HSE_plan_HTTP_creation_requires_the_original_catalog_revision_and_never_refreshes_it(bool changedCatalog)
    {
        using var f = await CompanyCorrectiveActionPlanCommandTests.Ready(); var owner = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var context = (await owner.GetCurrentFieldSelectionAsync()).SelectionContext!;
        using var boundary = new Boundary(f, owner); boundary.Orchestrator.Setup(value => value.GetHSEService()).Returns(() => PlanFacade(f, owner));
        await using var host = await HttpHost.Start(f, owner, boundary, true, typeof(HSEController));
        using var company = new CompanyClientContext(); var version = company.Clear(); Assert.True(company.TrySelect(version, "actor", new(context.Company, ["Administrator"], [])));
        var sdk = new HseIncidentClient(new ClientApiTransport(host.Http, new() { ApiBaseUrl = host.Url, MaxRetries = 3, RetryDelay = TimeSpan.Zero }, company), company);
        var catalog = await sdk.ReviewPlanCreationAsync("local", "A");
        var request = CompanyCorrectiveActionPlanCommandTests.Request() with { ExpectedRevision = changedCatalog ? catalog.Revision : new string('B',64) };
        if (changedCatalog) f.Rows.Add(new Beep.OilandGas.PPDM39.Models.R_HSE_RESPONSE_TYPE { ACTION_TYPE = "retired-added-after-review", ACTIVE_IND = "N" });
        var exception = await Assert.ThrowsAsync<Beep.OilandGas.Client.Exceptions.ApiException>(() => sdk.CreatePlanAsync("local", new(catalog.Context, request)));
        Assert.Equal(409, (int)exception.StatusCode); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }

}
