using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.App.Services.Field;
using Beep.OilandGas.Models.Constants;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class InvestigatorClientTests
{
    private static readonly RecordedIncidentParticipant Participant = new(17, 4, "person", "WITNESS");
    private static InvestigatorAssignmentReview Review(string incident = "local") => new(
        new(new("tenant", "company"), new("physical", "database"), "actor", "selection-revision", "A"), incident,
        new string('A', 64), [Participant], null);
    private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json") };
    [Fact]
    public async Task SDK_review_uses_the_actual_route_escapes_identity_and_returns_frozen_choices()
    {
        using var f = new Fixture((request, _) => { Assert.Equal(HttpMethod.Get, request.Method); Assert.Equal("/api/field/current/hse/incident/investigator-review", request.RequestUri!.AbsolutePath); Assert.Equal("case/part", Uri.UnescapeDataString(request.RequestUri.Query["?incidentId=".Length..])); return Task.FromResult(Json(Review("case/part"))); });
        var review = await f.Api.ReviewAsync("case/part", "A");
        Assert.Equal(Participant, Assert.Single(review.Participants)); Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyCollection<RecordedIncidentParticipant>>(review.Participants);
        Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("company")] [InlineData("actor")] [InlineData("field")] [InlineData("incident")] [InlineData("revision")] [InlineData("duplicate-key")] [InlineData("current-role")]
    public async Task SDK_refuses_inconsistent_review_evidence(string fault)
    {
        var review = Review();
        review = fault switch {
            "company" => review with { Context = review.Context with { Company = new("tenant", "other") } },
            "actor" => review with { Context = review.Context with { ActorUserId = "other" } },
            "field" => review with { Context = review.Context with { FieldId = "B" } },
            "incident" => review with { IncidentId = "other" }, "revision" => review with { Revision = "" },
            "duplicate-key" => review with { Participants = [Participant, Participant with { BusinessAssociateId = "other" }] },
            _ => review with { CurrentInvestigator = Participant }
        };
        using var f = new Fixture((_, _) => Task.FromResult(Json(review)));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReviewAsync("local", "A")); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task SDK_submission_sends_one_frozen_reviewed_body_and_never_retries_a_lost_write(bool lost)
    {
        using var f = new Fixture(async (request, _) => {
            Assert.Equal(HttpMethod.Put, request.Method); Assert.Equal("/api/field/current/hse/incident/investigator", request.RequestUri!.AbsolutePath); Assert.Equal("?incidentId=local", request.RequestUri.Query);
            var body = JsonSerializer.Deserialize<AssignIncidentInvestigatorRequest>(await request.Content!.ReadAsStringAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            Assert.Equal(new string('A',64), body.ExpectedRevision); Assert.Equal(17, body.PartyObservation); Assert.Equal(4, body.RoleObservation); Assert.Equal("person", body.BusinessAssociateId);
            if (lost) throw new HttpRequestException("lost acknowledgment"); return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        if (lost) await Assert.ThrowsAnyAsync<Exception>(() => f.Api.AssignAsync(Review(), Participant)); else await f.Api.AssignAsync(Review(), Participant);
        Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("company")] [InlineData("actor")] [InlineData("choice")]
    public async Task SDK_submission_cannot_adopt_another_company_actor_or_unreviewed_role(string fault)
    {
        using var f = new Fixture((_, _) => throw new Exception("Must not send")); var review = Review();
        if (fault == "company") review = review with { Context = review.Context with { Company = new("tenant", "other") } };
        if (fault == "actor") review = review with { Context = review.Context with { ActorUserId = "other" } };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.AssignAsync(review, fault == "choice" ? Participant with { RoleObservation = 99 } : Participant)); Assert.Equal(0, f.Calls);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task SDK_discards_review_or_acknowledgment_after_switching_away_and_back(bool write)
    {
        Fixture? fixture = null;
        using var f = fixture = new Fixture((_, _) => { fixture!.Select("other", "other"); fixture.Select("actor", "company"); return Task.FromResult(write ? new HttpResponseMessage(HttpStatusCode.NoContent) : Json(Review())); });
        if (write) await Assert.ThrowsAnyAsync<Exception>(() => f.Api.AssignAsync(Review(), Participant));
        else await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReviewAsync("local", "A")); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("context")] [InlineData("revision")] [InlineData("participants")] [InlineData("currentInvestigator")]
    public async Task SDK_requires_all_review_JSON_members_including_an_explicit_null_current_investigator(string missing)
    {
        var json = JsonSerializer.SerializeToNode(Review(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject(); json.Remove(missing);
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json") }));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReviewAsync("local", "A")); Assert.Equal(1, f.Calls);
    }
    private static CreateCAPlanSubmission PlanSubmission() => new(Review().Context, new(Guid.Parse("53aefbd6-44eb-4c17-9888-05915e520eaa"), "Explicit plan", "CA-017", "recorded-response", 17, 4, "person", new string('A',64)));
    private static CAPlanResolution PlanResult(CreateCAPlanSubmission? submission = null)
    { var intent = submission ?? PlanSubmission(); return new(intent.Context, "local", intent.Intent.PlanId.ToString("N")); }
    [Fact]
    public async Task SDK_plan_creation_posts_the_explicit_review_and_stable_intent_to_the_existing_route()
    {
        using var f = new Fixture(async (request, _) => {
            Assert.Equal(HttpMethod.Post, request.Method); Assert.Equal("/api/field/current/hse/incident/ca-plan", request.RequestUri!.AbsolutePath); Assert.Equal("?incidentId=local", request.RequestUri.Query);
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            var body = JsonSerializer.Deserialize<CreateCAPlanSubmission>(await request.Content!.ReadAsStringAsync(), new JsonSerializerOptions(JsonSerializerDefaults.Web)); Assert.Equal(PlanSubmission(), body);
            return Json(PlanResult());
        });
        Assert.Equal(PlanResult(), await f.Api.CreatePlanAsync("local", PlanSubmission())); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task SDK_plan_creation_never_retries_a_failure_or_lost_acknowledgment(bool lost)
    {
        using var f = new Fixture((_, _) => lost ? throw new HttpRequestException("lost acknowledgment") : Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreatePlanAsync("local", PlanSubmission())); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("company")] [InlineData("actor")] [InlineData("field")] [InlineData("revision")] [InlineData("database")] [InlineData("incident")] [InlineData("plan")]
    public async Task SDK_plan_creation_refuses_substituted_response_context_or_plan_identity(string fault)
    {
        var result = PlanResult(); result = fault switch {
            "company" => result with { Context = result.Context with { Company = new("tenant", "other") } },
            "actor" => result with { Context = result.Context with { ActorUserId = "other" } },
            "field" => result with { Context = result.Context with { FieldId = "B" } },
            "revision" => result with { Context = result.Context with { Revision = "different" } },
            "database" => result with { Context = result.Context with { Database = new("other", "other") } },
            "incident" => result with { IncidentId = "foreign" }, _ => result with { PlanId = "some-other-project" } };
        using var f = new Fixture((_, _) => Task.FromResult(Json(result)));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreatePlanAsync("local", PlanSubmission())); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task SDK_plan_reads_and_creation_acknowledgments_are_discarded_after_switching_away_and_back(bool write)
    {
        Fixture? fixture = null;
        using var f = fixture = new Fixture((_, _) => { fixture!.Select("other", "other"); fixture.Select("actor", "company"); return Task.FromResult(Json(PlanResult())); });
        if (write) await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreatePlanAsync("local", PlanSubmission()));
        else await Assert.ThrowsAnyAsync<Exception>(() => f.Api.GetPlanAsync("local", Review().Context)); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("company")] [InlineData("actor")] [InlineData("field")]
    public async Task SDK_plan_submission_requires_its_selected_company_actor_and_available_FIELD_before_sending(string fault)
    {
        var submission = PlanSubmission(); submission = submission with { Context = fault switch {
            "company" => submission.Context with { Company = new("tenant", "other") }, "actor" => submission.Context with { ActorUserId = "other" }, _ => submission.Context with { FieldId = null } } };
        using var f = new Fixture((_, _) => throw new Exception("Must not send"));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreatePlanAsync("local", submission)); Assert.Equal(0, f.Calls);
    }
    [Theory] [InlineData("context")] [InlineData("incidentId")] [InlineData("planId")]
    public async Task SDK_plan_resolution_requires_each_explicit_JSON_member(string missing)
    {
        var json = JsonSerializer.SerializeToNode(PlanResult(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject(); json.Remove(missing);
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json") }));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.GetPlanAsync("local", Review().Context)); Assert.Equal(1, f.Calls);
    }
    [Fact]
    public async Task SDK_plan_resolution_uses_a_read_and_accepts_an_actual_historical_plan_key_without_mutation()
    {
        var resolution = PlanResult() with { PlanId = "recorded-historical-project" };
        using var f = new Fixture((request, _) => { Assert.Equal(HttpMethod.Get, request.Method); Assert.Null(request.Content); return Task.FromResult(Json(resolution)); });
        Assert.Equal(resolution, await f.Api.GetPlanAsync("local", Review().Context)); Assert.Equal(1, f.Calls);
    }

    private static CAPlanCreationReview CreationReview() => new(Review().Context, "local", new string('A',64), [Participant], ["recorded-response"]);
    [Fact]
    public async Task SDK_creation_review_reads_existing_route_and_freezes_recorded_collections()
    {
        using var f = new Fixture((request, _) => { Assert.Equal(HttpMethod.Get, request.Method); Assert.Equal("/api/field/current/hse/incident/ca-plan-review", request.RequestUri!.AbsolutePath); Assert.Equal("?incidentId=local", request.RequestUri.Query); return Task.FromResult(Json(CreationReview())); });
        var review = await f.Api.ReviewPlanCreationAsync("local", "A");
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyCollection<RecordedIncidentParticipant>>(review.Participants);
        Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyCollection<string>>(review.ResponseTypes); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("company")] [InlineData("actor")] [InlineData("field")] [InlineData("incident")] [InlineData("revision")] [InlineData("participants")] [InlineData("duplicate-party")] [InlineData("responses")] [InlineData("duplicate-response")] [InlineData("status")]
    public async Task SDK_creation_review_refuses_substituted_scope_and_incomplete_or_ambiguous_choices(string fault)
    {
        var review = CreationReview(); review = fault switch {
            "company" => review with { Context = review.Context with { Company = new("tenant", "other") } },
            "actor" => review with { Context = review.Context with { ActorUserId = "other" } },
            "field" => review with { Context = review.Context with { FieldId = "B" } },
            "incident" => review with { IncidentId = "other" }, "revision" => review with { Revision = "bad" },
            "participants" => review with { Participants = [] }, "duplicate-party" => review with { Participants = [Participant, Participant] },
            "responses" => review with { ResponseTypes = [] }, "duplicate-response" => review with { ResponseTypes = ["same", "same"] },
            _ => review with { ResponseTypes = ["STATUS"] } };
        using var f = new Fixture((_, _) => Task.FromResult(Json(review)));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReviewPlanCreationAsync("local", "A")); Assert.Equal(1, f.Calls);
    }
    [Fact]
    public async Task SDK_creation_review_discards_a_late_result_after_company_switch_away_and_back()
    {
        Fixture? fixture = null;
        using var f = fixture = new Fixture((_, _) => { fixture!.Select("other", "other"); fixture.Select("actor", "company"); return Task.FromResult(Json(CreationReview())); });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReviewPlanCreationAsync("local", "A")); Assert.Equal(1, f.Calls);
    }
    [Theory] [InlineData("context")] [InlineData("incidentId")] [InlineData("revision")] [InlineData("participants")] [InlineData("responseTypes")]
    public async Task SDK_creation_review_requires_every_JSON_member(string missing)
    {
        var json = JsonSerializer.SerializeToNode(CreationReview(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!.AsObject(); json.Remove(missing);
        using var f = new Fixture((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json") }));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.ReviewPlanCreationAsync("local", "A")); Assert.Equal(1, f.Calls);
    }

    [Theory] [InlineData("")] [InlineData("short")] [InlineData("lowercase")]
    public async Task SDK_plan_creation_requires_the_canonical_catalog_revision_before_transport(string invalid)
    {
        var submission = PlanSubmission(); submission = submission with { Intent = submission.Intent with { ExpectedRevision = invalid == "lowercase" ? new string('a',64) : invalid } };
        using var f = new Fixture((_, _) => throw new Exception("Must not send"));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Api.CreatePlanAsync("local", submission)); Assert.Equal(0, f.Calls);
    }
    private sealed class Handler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> callback) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => callback(request, token); }
    private sealed class Fixture : IDisposable
    {
        private readonly HttpClient http; public CompanyClientContext Company { get; } = new(); public HseIncidentClient Api { get; } public int Calls { get; private set; }
        public Fixture(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            Select("actor", "company"); http = new(new Handler((request, token) => { Calls++; return handler(request, token); }));
            Api = new(new ClientApiTransport(http, new() { ApiBaseUrl = "https://api.test", MaxRetries = 3, RetryDelay = TimeSpan.Zero }, Company), Company);
        }
        public void Select(string actor, string company) { var version = Company.Clear(); Assert.True(Company.TrySelect(version, actor, new(new("tenant", company), ["Administrator"], []))); }
        public void Dispose() { http.Dispose(); Company.Dispose(); }
    }
}
