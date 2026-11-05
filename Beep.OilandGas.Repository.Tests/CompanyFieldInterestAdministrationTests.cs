using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ReviewClient = Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService;
using AccessApi = Beep.OilandGas.Client.App.Services.AccessControl.AccessControlService;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyFieldInterestAdministrationTests
{
    private static readonly CompanyScope Scope = new("tenant", "company");
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
    private static void Select(CompanyClientContext company, string user = "actor", bool admin = true, CompanyScope? scope = null) =>
        Assert.True(company.TrySelect(company.Clear(), user, new(scope ?? Scope, admin ? ["Administrator"] : [], [])));
    private static AssetGrantAdministrationState State(CompanyClientContext company, ReviewClient client,
        Func<Exception, string, string, string>? explain = null) => new(company, (_, _) => Task.FromResult<List<Beep.OilandGas.Models.Data.AssetAccess>?>([]),
            (_, _, _) => throw new InvalidOperationException("Interest review cannot grant access."),
            (_, _, _) => throw new InvalidOperationException("Interest review cannot revoke access."),
            explain ?? ((_, _, sentence) => sentence + ". Refresh company selection or verify your FIELD access."), client);
    private static FieldInterestReview Result() => new(Scope, new string('A', 64), "A", Now, "UTC", [
        new(new INTEREST_SET { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1, INTEREST_SET_TYPE = "test-interest", ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) },
            [new INT_SET_COMPONENT { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1, COMPONENT_OBS_NO = 7, COMPONENT_TYPE = "test-field", FIELD_ID = "A", ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) }],
            [new INT_SET_PARTNER { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1, PARTNER_BA_ID = "company", PARTNER_OBS_NO = 2, INTEREST_SET_ROLE = "test-recorded-role", ACTIVE_IND = "Y", GROSS_PERCENT_INTEREST = 37.5m, NET_PERCENT_INTEREST = 34.125m, EFFECTIVE_DATE = Now.AddYears(-1) }])]);

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Review_is_for_the_actor_and_exact_FIELD_without_a_grant_subject_or_new_authority(bool empty)
    {
        using var company = new CompanyClientContext(); Select(company); var result = Result(); if (empty) result.Sets.Clear();
        var client = new Mock<ReviewClient>(MockBehavior.Strict); CancellationToken token = default;
        client.Setup(value => value.GetFieldInterestReviewAsync("A", It.IsAny<CancellationToken>())).Returns((string _, CancellationToken sent) => { token = sent; return Task.FromResult(result); });
        using var state = State(company, client.Object); state.SelectInterestField("A"); Assert.True(state.CanReviewInterests); Assert.False(state.CanLoad);
        await state.LoadInterestReviewAsync(); Assert.Same(result, state.InterestReview); Assert.Equal(empty ? 0 : 1, state.InterestReview!.Sets.Count);
        Assert.Equal("UTC", state.InterestReview.DateZoneId); Assert.False(state.Busy); Assert.False(state.CanEdit); Assert.False(state.Loaded); Assert.Empty(state.Grants); Assert.Null(state.Error);
        Assert.Equal("", state.UserId); Assert.True(token.CanBeCanceled); client.VerifyAll(); client.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("company")] [InlineData("administrator")] [InlineData("empty")] [InlineData("padded")] [InlineData("control")] [InlineData("long")]
    public async Task Missing_company_authority_or_exact_FIELD_sends_no_review_request(string missing)
    {
        using var company = new CompanyClientContext(); if (missing != "company") Select(company, admin: missing != "administrator");
        var client = new Mock<ReviewClient>(MockBehavior.Strict); using var state = State(company, client.Object);
        state.SelectInterestField(missing switch { "empty" => "", "padded" => " A ", "control" => "A\nB", "long" => new string('A', 129), _ => "A" });
        Assert.False(state.CanReviewInterests); await state.LoadInterestReviewAsync(); Assert.NotNull(state.Error); Assert.Null(state.InterestReview); Assert.False(state.Busy);
        if (missing is "padded" or "control" or "long") Assert.NotNull(state.InterestFieldError); client.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("null")] [InlineData("company")] [InlineData("field")] [InlineData("database")] [InlineData("timestamp")] [InlineData("date-zone")]
    [InlineData("null-set")] [InlineData("duplicate-set")] [InlineData("set-type")] [InlineData("component-type")] [InlineData("component-field")]
    [InlineData("component-sequence")] [InlineData("duplicate-component")] [InlineData("foreign-partner")] [InlineData("partner-sequence")]
    [InlineData("duplicate-partner")] [InlineData("partner-role")] [InlineData("fractional-observation")] [InlineData("inactive")] [InlineData("limit")]
    public async Task Malformed_or_foreign_typed_evidence_cannot_be_displayed(string fault)
    {
        using var company = new CompanyClientContext(); Select(company); FieldInterestReview? result = Result(); var set = result.Sets[0];
        switch (fault)
        {
            case "null": result = null; break;
            case "company": result = result with { Company = new("tenant", "other") }; break;
            case "field": result = result with { FieldId = "B" }; break;
            case "database": result = result with { DatabaseScope = "PPDM39" }; break;
            case "timestamp": result = result with { ReviewedUtc = DateTime.SpecifyKind(Now, DateTimeKind.Unspecified) }; break;
            case "date-zone": result = result with { DateZoneId = "" }; break;
            case "null-set": result.Sets.Add(null!); break;
            case "duplicate-set": result.Sets.Add(set); break;
            case "set-type": set.InterestSet.INTEREST_SET_TYPE = ""; break;
            case "component-type": set.Components[0].COMPONENT_TYPE = ""; break;
            case "component-field": set.Components[0].FIELD_ID = "B"; break;
            case "component-sequence": set.Components[0].INTEREST_SET_SEQ_NO = 2; break;
            case "duplicate-component": set.Components.Add(set.Components[0]); break;
            case "foreign-partner": set.CompanyPartners[0].PARTNER_BA_ID = "other"; break;
            case "partner-sequence": set.CompanyPartners[0].INTEREST_SET_SEQ_NO = 2; break;
            case "duplicate-partner": set.CompanyPartners.Add(set.CompanyPartners[0]); break;
            case "partner-role": set.CompanyPartners[0].INTEREST_SET_ROLE = " recorded "; break;
            case "fractional-observation": set.CompanyPartners[0].PARTNER_OBS_NO = 2.5m; break;
            case "inactive": set.CompanyPartners[0].ACTIVE_IND = "N"; break;
            default: result.Sets.AddRange(Enumerable.Repeat(set, 256)); break;
        }
        var client = new Mock<ReviewClient>(); client.Setup(value => value.GetFieldInterestReviewAsync("A", It.IsAny<CancellationToken>())).ReturnsAsync(result!);
        using var state = State(company, client.Object); state.SelectInterestField("A"); await state.LoadInterestReviewAsync();
        Assert.Null(state.InterestReview); Assert.NotNull(state.Error); Assert.False(state.Busy); Assert.False(state.CanEdit);
    }

    [Theory]
    [InlineData("field", false)] [InlineData("field", true)] [InlineData("company", false)] [InlineData("company", true)]
    [InlineData("account", false)] [InlineData("account", true)] [InlineData("roles", false)] [InlineData("roles", true)]
    [InlineData("subject", false)] [InlineData("subject", true)] [InlineData("dispose", false)] [InlineData("dispose", true)]
    [InlineData("grants", false)] [InlineData("grants", true)]
    public async Task Context_changes_cancel_review_and_discard_late_success_or_failure(string change, bool failure)
    {
        using var company = new CompanyClientContext(); Select(company); var response = new TaskCompletionSource<FieldInterestReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new Mock<ReviewClient>(); CancellationToken captured = default;
        client.Setup(value => value.GetFieldInterestReviewAsync("A", It.IsAny<CancellationToken>())).Returns((string _, CancellationToken token) => { captured = token; return response.Task; });
        using var state = State(company, client.Object); state.SelectInterestField("A"); var operation = state.LoadInterestReviewAsync(); Assert.True(state.Busy); Assert.False(state.CanReviewInterests);
        switch (change)
        {
            case "field": state.SelectInterestField("B"); break;
            case "company": Select(company, scope: new("other-tenant", "other")); break;
            case "account": Select(company, user: "new-actor"); break;
            case "roles": Select(company, admin: false); break;
            case "subject": state.SelectUser("subject"); break;
            case "dispose": state.Dispose(); break;
            default: state.SelectUser("subject"); await state.LoadAsync(); break;
        }
        Assert.True(captured.IsCancellationRequested);
        if (failure) response.SetException(new ApiClientException("Late review failure.")); else response.SetResult(Result()); await operation;
        Assert.Null(state.InterestReview); Assert.Null(state.Error); Assert.False(state.Busy); Assert.Equal(change == "grants", state.Loaded);
        if (change == "field") Assert.Equal("B", state.InterestFieldId);
        if (change is "company" or "account" or "roles" or "subject") Assert.Equal("", state.InterestFieldId);
    }

    [Fact]
    public async Task Superseded_same_FIELD_read_cannot_replace_the_newer_result_or_clear_its_busy_state()
    {
        using var company = new CompanyClientContext(); Select(company);
        var first = new TaskCompletionSource<FieldInterestReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<FieldInterestReview>(TaskCreationOptions.RunContinuationsAsynchronously); var reads = 0;
        var client = new Mock<ReviewClient>(); client.Setup(value => value.GetFieldInterestReviewAsync("A", It.IsAny<CancellationToken>())).Returns(() => ++reads == 1 ? first.Task : second.Task);
        using var state = State(company, client.Object); state.SelectInterestField("A"); var old = state.LoadInterestReviewAsync(); var current = state.LoadInterestReviewAsync();
        first.SetResult(Result()); await old; Assert.True(state.Busy); Assert.Null(state.InterestReview);
        var fresh = Result() with { ReviewedUtc = Now.AddMinutes(1) }; second.SetResult(fresh); await current;
        Assert.Same(fresh, state.InterestReview); Assert.False(state.Busy); Assert.Null(state.Error);
    }

    [Fact]
    public async Task A_failed_refresh_removes_previous_evidence_and_allows_an_explicit_retry()
    {
        using var company = new CompanyClientContext(); Select(company); var client = new Mock<ReviewClient>();
        client.SetupSequence(value => value.GetFieldInterestReviewAsync("A", It.IsAny<CancellationToken>())).ReturnsAsync(Result()).ThrowsAsync(new ApiClientException("Forbidden FIELD.")).ReturnsAsync(Result());
        using var state = State(company, client.Object); state.SelectInterestField("A"); await state.LoadInterestReviewAsync(); Assert.NotNull(state.InterestReview);
        await state.LoadInterestReviewAsync(); Assert.Null(state.InterestReview); Assert.NotNull(state.Error); Assert.True(state.CanReviewInterests);
        await state.LoadInterestReviewAsync(); Assert.NotNull(state.InterestReview); Assert.Null(state.Error);
    }

    [Fact]
    public async Task Company_change_during_error_explanation_cannot_publish_old_error_text()
    {
        using var company = new CompanyClientContext(); Select(company); var client = new Mock<ReviewClient>();
        client.Setup(value => value.GetFieldInterestReviewAsync("A", It.IsAny<CancellationToken>())).ThrowsAsync(new ApiClientException("Denied."));
        using var state = State(company, client.Object, (_, _, _) => { Select(company, user: "new-actor"); return "Old actor error."; });
        state.SelectInterestField("A"); await state.LoadInterestReviewAsync(); Assert.Null(state.Error); Assert.Null(state.InterestReview); Assert.False(state.Busy);
    }

    [Fact]
    public async Task Disposal_unsubscribes_and_prevents_new_review_work()
    {
        using var company = new CompanyClientContext(); Select(company); var client = new Mock<ReviewClient>(MockBehavior.Strict);
        var state = State(company, client.Object); var notifications = 0; state.Changed += () => notifications++;
        state.SelectInterestField("A"); state.Dispose(); var before = notifications; Select(company, user: "new-actor"); state.SelectInterestField("B"); await state.LoadInterestReviewAsync();
        Assert.Equal(before, notifications); Assert.False(state.CanReviewInterests); client.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Actual_API_facade_SDK_and_administration_state_preserve_recorded_values_date_zone_and_current_authority()
    {
        using var f = await CompanyFieldInterestReviewTests.Ready(); ((ClaimsIdentity)f.Request.User.Identity!).AddClaim(new(ClaimTypes.Role, "Administrator"));
        var controller = new AccessControlController(CompanyAssetFacadeTests.Service(f), principal => principal.FindFirst("party_id")?.Value)
            { ControllerContext = new() { HttpContext = f.Request } };
        using var company = new CompanyClientContext(); Select(company, "account", scope: f.Gate.Company.Scope);
        using var http = new HttpClient(new Handler(async request => {
            Assert.Equal("/api/AccessControl/fields/A/interest-review", request.RequestUri!.AbsolutePath);
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            var result = Assert.IsType<OkObjectResult>((await controller.GetFieldInterestReview("A")).Result);
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web))) };
        }));
        var sdk = new AccessApi(new ClientApiTransport(http, new() { ApiBaseUrl = "https://api.test" }, company));
        using var state = State(company, sdk); state.SelectInterestField("A"); await state.LoadInterestReviewAsync();
        Assert.Null(state.Error); var evidence = Assert.Single(state.InterestReview!.Sets); Assert.Equal("UTC", state.InterestReview.DateZoneId);
        Assert.Equal(7, Assert.Single(evidence.Components).COMPONENT_OBS_NO); Assert.Equal(2, Assert.Single(evidence.CompanyPartners).PARTNER_OBS_NO);
        Assert.Equal(37.5m, evidence.CompanyPartners[0].GROSS_PERCENT_INTEREST); Assert.Equal(34.125m, evidence.CompanyPartners[0].NET_PERCENT_INTEREST);
        Assert.False(state.CanEdit); Assert.Empty(f.Writes);
    }
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request); }
}
