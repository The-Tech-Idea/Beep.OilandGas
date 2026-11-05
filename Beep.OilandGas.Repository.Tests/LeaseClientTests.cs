using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Data.OilGas;
using Xunit;
using LeaseApi = Beep.OilandGas.Client.App.Services.Lease.LeaseService;

namespace Beep.OilandGas.Repository.Tests;

public sealed class LeaseClientTests
{
    private static readonly LeaseCreationContext Context = new(new("tenant", "company"), "reviewed-database", "actor", "A", "lease", "company-data", "operational", "UTC");
    private static CreateLeaseAcquisition Intent() => new() { CommandId = Guid.NewGuid(), Context = Context, PropertyId = "A", LeaseName = "Recorded", LeaseNumber = "number", LeaseType = "reviewed", EffectiveDate = new(2020, 1, 1), CalculatedInterest = 0 };
    private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json") };

    [Fact]
    public async Task FIELD_and_lease_identity_are_escaped_in_the_resource_route_and_filters_are_copied()
    {
        using var f = new Fixture((request, _) => {
            Assert.Equal("/api/fields/A%2FB/lease-acquisition/available", request.RequestUri!.AbsolutePath);
            Assert.Equal("?filters%5BLeaseName%5D=Recorded%20name", request.RequestUri.Query); return Task.FromResult(Json(Array.Empty<LeaseSummary>()));
        });
        var filters = new Dictionary<string, string> { ["PropertyId"] = "A/B", ["LeaseName"] = "Recorded name" };
        Assert.Empty(await f.Api.GetAvailableLeasesAsync(filters)); Assert.Equal("A/B", filters["PropertyId"]); Assert.Equal(2, filters.Count);
    }
    [Theory] [InlineData("missing")] [InlineData("lease-only")] [InlineData("conflict")]
    public async Task A_lease_ID_alone_or_conflicting_FIELD_does_not_send_business_reads(string fault)
    {
        using var f = new Fixture((_, _) => throw new Exception("Must not send"));
        var filters = fault switch { "lease-only" => new Dictionary<string, string> { ["LeaseId"] = "lease" }, "conflict" => new() { ["FieldId"] = "A", ["PropertyId"] = "B" }, _ => new() };
        await Assert.ThrowsAsync<ArgumentException>(() => f.Api.GetAvailableLeasesAsync(filters)); Assert.Equal(0, f.Calls);
    }
    [Theory] [InlineData("field")] [InlineData("id")]
    public async Task Evaluation_cannot_adopt_a_record_from_another_resource(string fault)
    {
        var row = new LeaseSummary { LeaseId = fault == "id" ? "other" : "lease", FieldId = fault == "field" ? "B" : "A", LandRightSubtype = "lease", DateZoneId = "UTC", CalculatedInterest = .25m };
        using var f = new Fixture((request, _) => { Assert.Equal("/api/fields/A/lease-acquisition/evaluate/lease", request.RequestUri!.AbsolutePath); return Task.FromResult(Json(row)); });
        await Assert.ThrowsAsync<ApiClientException>(() => f.Api.EvaluateLeaseAsync("lease", "A")); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task A_status_write_is_bound_to_the_FIELD_and_not_automatically_retried(bool lost)
    {
        using var f = new Fixture(async (request, _) => {
            Assert.Equal(HttpMethod.Put, request.Method); Assert.Equal("/api/fields/A/lease-acquisition/lease/status", request.RequestUri!.AbsolutePath);
            Assert.Contains("ACTIVE", await request.Content!.ReadAsStringAsync());
            if (lost) throw new HttpRequestException("lost"); return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        });
        if (lost) await Assert.ThrowsAnyAsync<ApiClientException>(() => f.Api.UpdateLeaseStatusAsync("lease", "ACTIVE", "A"));
        else await f.Api.UpdateLeaseStatusAsync("lease", "ACTIVE", "A"); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public async Task Context_creation_and_reconciliation_use_the_real_routes_and_same_frozen_id()
    {
        var intent = Intent(); var paths = new List<string>();
        using var f = new Fixture(async (request, _) => {
            paths.Add(request.RequestUri!.AbsolutePath);
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            if (request.Method == HttpMethod.Get) return Json(Context);
            var body = JsonSerializer.Deserialize<CreateLeaseAcquisition>(await request.Content!.ReadAsStringAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            Assert.Equal(intent.CommandId, body.CommandId); Assert.Equal(Context, body.Context); Assert.Equal(0, body.CalculatedInterest);
            return Json(new LeaseCreationReceipt(body.CommandId, "Recorded", body.CommandId.ToString("N"), Context));
        });
        Assert.Equal(Context, await f.Api.GetCreationContextAsync("A"));
        Assert.Equal(intent.CommandId.ToString("N"), (await f.Api.CreateLeaseAcquisitionAsync(intent)).LeaseId);
        Assert.Equal("Recorded", (await f.Api.ReconcileLeaseCreationAsync(intent)).State);
        Assert.Equal(new[] { "/api/fields/A/lease-acquisition/creation-context", "/api/fields/A/lease-acquisition", "/api/fields/A/lease-acquisition/creation-reconciliation" }, paths);
    }

    [Theory]
    [InlineData("company")] [InlineData("actor")] [InlineData("field")] [InlineData("zone")] [InlineData("database")] [InlineData("subtype")] [InlineData("component")] [InlineData("status")]
    public async Task Missing_or_mismatching_review_context_is_refused(string fault)
    {
        var context = fault switch {
            "company" => Context with { Company = new("other", "other") }, "actor" => Context with { ActorUserId = "other" },
            "field" => Context with { FieldId = "B" }, "zone" => Context with { DateZoneId = "not-a-zone" },
            "database" => Context with { DatabaseScopeFingerprint = "" }, "subtype" => Context with { LandRightSubtype = "" },
            "component" => Context with { CompanyComponentType = "" }, _ => Context with { StatusType = "" } };
        using var f = new Fixture((_, _) => Task.FromResult(Json(context)));
        await Assert.ThrowsAsync<ApiClientException>(() => f.Api.GetCreationContextAsync("A")); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData("command")] [InlineData("context")] [InlineData("field")] [InlineData("account")] [InlineData("company")]
    public async Task A_draft_cannot_adopt_an_unreviewed_company_or_account_before_sending(string fault)
    {
        using var f = new Fixture((_, _) => throw new Exception("Must not send")); var request = Intent();
        if (fault == "command") request.CommandId = Guid.Empty;
        if (fault == "context") request.Context = null;
        if (fault == "field") request.PropertyId = "B";
        if (fault == "account") f.Select("other", "company");
        if (fault == "company") f.Select("actor", "other");
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreateLeaseAcquisitionAsync(request)); Assert.Equal(0, f.Calls);
    }

    [Theory]
    [InlineData("command")] [InlineData("state")] [InlineData("id")] [InlineData("context")] [InlineData("absence")] [InlineData("empty")]
    public async Task Creation_needs_a_complete_matching_recorded_receipt(string fault)
    {
        var intent = Intent(); var receipt = new LeaseCreationReceipt(intent.CommandId, "Recorded", intent.CommandId.ToString("N"), Context);
        receipt = fault switch { "command" => receipt with { CommandId = Guid.NewGuid() }, "state" => receipt with { State = "" },
            "id" => receipt with { LeaseId = "other" }, "context" => receipt with { Context = Context with { FieldId = "B" } },
            "absence" => receipt with { State = "NotRecorded", LeaseId = null }, _ => receipt };
        using var f = new Fixture((_, _) => Task.FromResult(fault == "empty" ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") } : Json(receipt)));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreateLeaseAcquisitionAsync(intent)); Assert.Equal(1, f.Calls);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Uncertain_POST_is_never_automatically_retried(bool timeout)
    {
        using var f = new Fixture((_, _) => timeout ? throw new TaskCanceledException("timeout", new TimeoutException()) : throw new HttpRequestException("lost"));
        await Assert.ThrowsAnyAsync<ApiClientException>(() => f.Api.CreateLeaseAcquisitionAsync(Intent())); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public async Task Explicit_NotRecorded_reconciliation_is_a_read_only_single_POST()
    {
        var intent = Intent(); using var f = new Fixture((request, _) => {
            Assert.Equal("/api/fields/A/lease-acquisition/creation-reconciliation", request.RequestUri!.AbsolutePath);
            return Task.FromResult(Json(new LeaseCreationReceipt(intent.CommandId, "NotRecorded", null, Context)));
        });
        var receipt = await f.Api.ReconcileLeaseCreationAsync(intent); Assert.Equal("NotRecorded", receipt.State); Assert.Null(receipt.LeaseId); Assert.Equal(1, f.Calls);
    }

    [Fact]
    public async Task Lease_read_rejects_a_missing_recorded_number_instead_of_inventing_zero()
    {
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"leaseId\":\"lease\",\"fieldId\":\"A\",\"landRightSubtype\":\"lease\",\"dateZoneId\":\"UTC\"}", Encoding.UTF8, "application/json") }));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.EvaluateLeaseAsync("lease", "A"));
    }

    [Fact]
    public async Task Valid_zero_is_preserved_and_empty_reads_are_explicit()
    {
        var row = new LeaseSummary { LeaseId = "lease", FieldId = "A", LandRightSubtype = "lease", DateZoneId = "UTC", CalculatedInterest = 0 };
        using var f = new Fixture((request, _) => Task.FromResult(request.RequestUri!.AbsolutePath.Contains("evaluate") ? Json(row) : Json(Array.Empty<LeaseSummary>())));
        Assert.Equal(0, (await f.Api.EvaluateLeaseAsync("lease", "A")).CalculatedInterest);
        Assert.Empty(await f.Api.GetAvailableLeasesAsync(new() { ["FieldId"] = "A" }));
    }

    [Fact]
    public async Task Broad_lease_reads_do_not_send_a_request()
    {
        using var f = new Fixture((_, _) => throw new Exception("Must not send"));
        await Assert.ThrowsAsync<ArgumentException>(() => f.Api.GetAvailableLeasesAsync()); Assert.Equal(0, f.Calls);
    }

    [Fact]
    public async Task Read_only_summary_preserves_a_company_recorded_status_without_inventing_a_standard_status()
    {
        var row = new LeaseSummary { LeaseId = "lease", FieldId = "A", LandRightSubtype = "lease", DateZoneId = "UTC", CalculatedInterest = .25m, Status = "COMPANY_REVIEW" };
        using var f = new Fixture((_, _) => Task.FromResult(Json(row)));
        Assert.Equal("COMPANY_REVIEW", (await f.Api.EvaluateLeaseAsync("lease", "A")).Status);
    }

    [Fact]
    public async Task A_context_response_from_a_previous_company_generation_is_discarded()
    {
        Fixture? fixture = null;
        using var f = fixture = new Fixture((_, _) => { fixture!.Select("actor", "company"); return Task.FromResult(Json(Context)); });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.GetCreationContextAsync("A"));
    }

    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => callback(request, token); }

    [Theory]
    [InlineData("commandId")] [InlineData("state")] [InlineData("leaseId")] [InlineData("context")]
    public async Task NotRecorded_requires_an_explicit_complete_receipt_including_the_null_id(string missing)
    {
        var intent = Intent(); var properties = JsonSerializer.SerializeToNode(new LeaseCreationReceipt(intent.CommandId, "NotRecorded", null, Context), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject();
        properties.Remove(missing);
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(properties.ToJsonString(), Encoding.UTF8, "application/json") }));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReconcileLeaseCreationAsync(intent)); Assert.Equal(1, f.Calls);
    }
    private sealed class Fixture : IDisposable
    {
        private readonly HttpClient http;
        public CompanyClientContext Company { get; } = new();
        public LeaseApi Api { get; }
        public int Calls { get; private set; }
        public Fixture(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            Select("actor", "company"); http = new(new Handler((request, token) => { Calls++; return handler(request, token); }));
            Api = new(new ClientApiTransport(http, new() { ApiBaseUrl = "https://api.test", MaxRetries = 3, RetryDelay = TimeSpan.Zero }, Company), Company);
        }
        public void Select(string actor, string company) { var version = Company.Clear(); Assert.True(Company.TrySelect(version, actor, new(new("tenant", company), ["Administrator"], []))); }
        public void Dispose() { http.Dispose(); Company.Dispose(); }
    }
}
