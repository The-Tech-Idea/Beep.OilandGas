using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-03: OSDU records are exported with the envelope the platform requires, a dry run sends nothing, a send
/// reports per-batch outcomes and retries only the failed batches, an expired token is distinguishable, the
/// token never appears in a message, and a reconciliation proposes without merging.
/// </summary>
public sealed class OsduInteropTests
{
    private const string Configured = """
                                      {
                                        "osdu": {
                                          "dataPartition": "opendes",
                                          "viewers": [ "data.default.viewers@opendes.example.com" ],
                                          "owners": [ "data.default.owners@opendes.example.com" ],
                                          "legalTags": [ "opendes-public-usa-dataset-1" ]
                                        },
                                        "toolboxes": {
                                          "dataai": {
                                            "osduStorageUrl": "https://osdu.example.com/api/storage/v2/records",
                                            "osduBatchSize": BATCH
                                          }
                                        }
                                      }
                                      """;

    private const string Token = "eyJ-probe-token-value";

    [Fact]
    public void Export_produces_the_expected_kinds_with_the_required_envelope()
    {
        using var env = Environment(1);

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out var warnings);
        Assert.Empty(warnings);

        // AC-1: one record per kind, and every envelope field the platform refuses without.
        Assert.Equal(3, records.Count);
        Assert.Empty(OgOsduWorkspace.Validate(records));

        var json = OgOsduWorkspace.ExportJson(OgOsduWorkspace.ExampleWell(), out _);
        Assert.True(json.Success, json.Message);

        var array = JsonNode.Parse(json.Value!)!.AsArray();
        Assert.Equal(records.Count, array.Count);

        foreach (var record in array)
        {
            Assert.False(string.IsNullOrWhiteSpace(record!["id"]!.GetValue<string>()));
            Assert.StartsWith("opendes:", record["id"]!.GetValue<string>(), StringComparison.Ordinal);
            Assert.Contains(":", record["kind"]!.GetValue<string>(), StringComparison.Ordinal);
            Assert.NotEmpty(record["acl"]!["viewers"]!.AsArray());
            Assert.NotEmpty(record["legal"]!["legaltags"]!.AsArray());
            Assert.NotNull(record["data"]);
        }
    }

    [Fact]
    public void Validation_names_the_record_that_the_platform_would_refuse()
    {
        using var env = Environment(1);

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out _);
        var first = records[0];
        var id = first["id"]!.GetValue<string>();

        first["acl"] = new JsonObject { ["viewers"] = new JsonArray(), ["owners"] = new JsonArray() };

        var problems = OgOsduWorkspace.Validate(records);

        // §5: the record is named, not the file.
        Assert.Contains(problems, problem =>
            problem.RecordId == id
            && problem.Problem.Contains("acl", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task A_plan_is_pure_and_only_send_touches_the_network()
    {
        using var env = Environment(1);

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out _);
        var handler = new CountingHandler(_ => Ok());

        // AC-5: planning produces counts and sends nothing.
        var plan = OgOsduWorkspace.Plan(records);
        Assert.True(plan.CanSend);
        Assert.Equal(records.Count, plan.RecordCount);
        Assert.Equal(3, plan.BatchCount);
        Assert.Equal(0, handler.Calls);

        // Only the send uses the handler.
        using var http = new HttpClient(handler);
        var sent = await OgOsduWorkspace.SendAsync(records, Token, http);

        Assert.True(sent.Succeeded, sent.Describe());
        Assert.Equal(3, handler.Calls);
    }

    [Fact]
    public async Task A_send_reports_each_batch_and_a_retry_resends_only_the_failed_ones()
    {
        using var env = Environment(1);

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out _);

        // Batch 1 succeeds, batch 2 fails, batch 3 is never attempted.
        var firstRun = new CountingHandler(call => call == 2
            ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
            : Ok());

        using (var http = new HttpClient(firstRun))
        {
            var first = await OgOsduWorkspace.SendAsync(records, Token, http);

            Assert.Equal(1, first.Accepted);
            Assert.Equal(1, first.Rejected);
            Assert.Equal(1, first.NotSent);
            Assert.False(first.Succeeded);
            Assert.Equal(2, firstRun.Calls);

            // AC-2: only batches 2 and 3 are resent — not the accepted batch 1.
            var retryRun = new CountingHandler(_ => Ok());
            using var retryHttp = new HttpClient(retryRun);

            var retried = await OgOsduWorkspace
                .RetryFailedAsync(records, first, Token, retryHttp);

            Assert.True(retried.Succeeded, retried.Describe());
            Assert.Equal(2, retryRun.Calls);
            Assert.Equal([2, 3], retried.Batches.Select(batch => batch.Index));
            Assert.Equal(2, retried.Accepted);
        }
    }

    [Fact]
    public async Task An_expired_token_is_distinguishable_from_an_unreachable_store()
    {
        using var env = Environment(1);

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out _);

        using (var http = new HttpClient(new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized))))
        {
            var rejected = await OgOsduWorkspace.SendAsync(records, Token, http);

            // AC-6: the token is the problem, and the sentence says so.
            Assert.True(rejected.TokenExpired);
            Assert.Contains("token", rejected.Describe(), StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, rejected.Rejected);
        }
    }

    [Fact]
    public async Task The_token_never_appears_in_a_result_a_refusal_or_the_plan()
    {
        using var env = Environment(1);

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out _);

        using (var http = new HttpClient(new CountingHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized))))
        {
            var rejected = await OgOsduWorkspace.SendAsync(records, Token, http);

            // AC-4: not in the summary, not in a batch error, not in a refusal.
            foreach (var text in new[]
                     {
                         rejected.Describe(),
                         string.Join(" ", rejected.Batches.Select(batch => batch.Error ?? string.Empty)),
                     })
            {
                Assert.DoesNotContain(Token, text, StringComparison.Ordinal);
            }
        }

        var plan = OgOsduWorkspace.Plan(records);
        Assert.DoesNotContain(Token, plan.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_plan_with_no_storage_endpoint_is_refused_before_any_request()
    {
        // No dataai.osduStorageUrl in this environment.
        using var env = WellboreServices.Scope(new WellboreServices(WellboreEnvironment.FromJson(
            Configured.Replace("BATCH", "1", StringComparison.Ordinal)
                .Replace("https://osdu.example.com/api/storage/v2/records", string.Empty, StringComparison.Ordinal))));

        var records = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), out _);
        var plan = OgOsduWorkspace.Plan(records);

        Assert.False(plan.CanSend);
        Assert.Contains(plan.Problems, problem =>
            problem.Contains("osduStorageUrl", StringComparison.OrdinalIgnoreCase));

        var handler = new CountingHandler(_ => Ok());
        using var http = new HttpClient(handler);

        var sent = await OgOsduWorkspace.SendAsync(records, Token, http);

        Assert.NotNull(sent.Refusal);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public void Reconciliation_proposes_duplicates_and_merges_nothing()
    {
        using var env = Environment(100);

        var a = OgOsduWorkspace.ExampleWell();
        var b = OgOsduWorkspace.ExampleWell();

        // Same identity seen from two sources: a candidate, not a merge.
        b.Header.Id = "OSDU-DEMO-1-COPY";

        var before = (a.Header.Name, a.Header.Uwi, a.Header.Id, b.Header.Name, b.Header.Uwi, b.Header.Id);

        var candidates = OgOsduWorkspace.Reconcile([a, b], "probe");
        var again = OgOsduWorkspace.Reconcile([a, b], "probe");

        // AC-3: the proposal is deterministic and the inputs are untouched.
        Assert.NotEmpty(candidates);
        Assert.All(candidates, candidate => Assert.True(candidate.Score > 0));
        Assert.Equal(
            candidates.Select(candidate => (candidate.A, candidate.B, candidate.Score)),
            again.Select(candidate => (candidate.A, candidate.B, candidate.Score)));

        Assert.Equal(before, (a.Header.Name, a.Header.Uwi, a.Header.Id, b.Header.Name, b.Header.Uwi, b.Header.Id));
    }

    [Fact]
    public void The_export_carries_only_the_kinds_that_were_selected()
    {
        using var env = Environment(100);

        var model = OgOsduWorkspace.ExampleWell();

        var everything = OgOsduWorkspace.Records(model, OgOsduWorkspace.Kinds, out _);

        Assert.Equal(3, everything.Count);

        // INT-03 §3: the Export tab selects the record kinds.
        var logsOnly = OgOsduWorkspace.Records(model, [OgOsduRecordKind.WellLog], out _);

        Assert.Single(logsOnly);
        Assert.Equal(OgOsduWorkspace.KindId(OgOsduRecordKind.WellLog), logsOnly[0]["kind"]!.GetValue<string>());

        var two = OgOsduWorkspace.Records(model, [OgOsduRecordKind.Wellbore, OgOsduRecordKind.WellboreTrajectory], out _);

        Assert.Equal(2, two.Count);
        Assert.DoesNotContain(two, record =>
            record["kind"]!.GetValue<string>() == OgOsduWorkspace.KindId(OgOsduRecordKind.WellLog));

        // Nothing selected is nothing exported, and the export says so rather than quietly sending everything.
        Assert.Empty(OgOsduWorkspace.Records(model, [], out _));

        var refused = OgOsduWorkspace.Export(model, []);

        Assert.False(refused.Success);
        Assert.Contains("None of the selected record kinds", refused.Message!, StringComparison.Ordinal);

        // The kind ids are the platform's own, one per kind, and each is a legal kind string.
        Assert.Equal(3, OgOsduWorkspace.Kinds.Select(OgOsduWorkspace.KindId).Distinct(StringComparer.Ordinal).Count());
        Assert.All(OgOsduWorkspace.Kinds, kind =>
            Assert.Matches(@"^[\w\-\.]+:[\w\-\.]+:[\w\-\.]+:\d+\.\d+\.\d+$", OgOsduWorkspace.KindId(kind)));

        Assert.Equal(["Wellbore", "WellboreTrajectory", "WellLog"],
            OgOsduWorkspace.Kinds.Select(OgOsduWorkspace.KindLabel));
    }

    [Fact]
    public void An_invalid_record_refuses_the_export_and_names_it()
    {
        // The feature's "schema-invalid record" state: the export refuses and names the record rather than writing a file the platform rejects.
        using var configured = Environment(100);

        var good = OgOsduWorkspace.Export(OgOsduWorkspace.ExampleWell(), OgOsduWorkspace.Kinds);

        Assert.True(good.Success, good.Message);
        Assert.True(good.Value!.Valid);
        Assert.Empty(good.Value.Problems);
        Assert.Contains("osdu:wks", good.Value.Json, StringComparison.Ordinal);

        // An environment with no access or legal metadata: every record breaks the envelope the platform requires.
        using var bare = WellboreServices.Scope(new WellboreServices(WellboreEnvironment.FromJson("{}")));

        var refused = OgOsduWorkspace.Export(OgOsduWorkspace.ExampleWell(), OgOsduWorkspace.Kinds);

        Assert.False(refused.Success);
        Assert.Equal(OgOsduWorkspace.CodeInvalidRecord, refused.Reference);
        Assert.Contains("would be invalid", refused.Message!, StringComparison.Ordinal);

        // The sentence names the record and the rule, so a person can fix the record rather than guess.
        var id = OgOsduWorkspace.Records(OgOsduWorkspace.ExampleWell(), OgOsduWorkspace.Kinds, out _)[0]["id"]!.GetValue<string>();

        Assert.Contains(id, refused.Message!, StringComparison.Ordinal);
        Assert.Contains("legal tags", refused.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_reconciliation_decision_is_only_recorded_when_a_person_makes_it()
    {
        // AC-3 at the page: the accept and reject commands are the only path that records anything, they need a selected candidate, and the
        // crosswalk a merge would write is not invented here.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith("OsduInteropPageView.cs", StringComparison.Ordinal)));

        Assert.Contains("public bool Decide(bool accepted)", page, StringComparison.Ordinal);
        Assert.Contains("if (_candidates.SelectedItem is not OgOsduCandidate candidate)", page, StringComparison.Ordinal);
        Assert.Contains("\"interop.osdu.reconcile\"", page, StringComparison.Ordinal);
        Assert.Contains("Recorded, not merged", page, StringComparison.Ordinal);

        // The decision is only ever made from the two buttons, never from the reconcile run itself.
        Assert.Contains("Button(\"Accept selected\", (_, _) => Decide(accepted: true))", page, StringComparison.Ordinal);
        Assert.Contains("Button(\"Reject selected\", (_, _) => Decide(accepted: false))", page, StringComparison.Ordinal);

        var reconcile = page[page.IndexOf("public bool Reconcile()", StringComparison.Ordinal)..];
        reconcile = reconcile[..reconcile.IndexOf("private bool _lastSentSucceeded", StringComparison.Ordinal)];

        Assert.DoesNotContain("Decide(", reconcile, StringComparison.Ordinal);
        Assert.Contains("nothing is merged", reconcile, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply(", reconcile, StringComparison.Ordinal);
    }

    private static IDisposable Environment(int batch) =>
        WellboreServices.Scope(new WellboreServices(WellboreEnvironment.FromJson(
            Configured.Replace(
                "BATCH",
                batch.ToString(System.Globalization.CultureInfo.InvariantCulture),
                StringComparison.Ordinal))));

    private static HttpResponseMessage Ok() =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent("{ \"recordCount\": 1 }", Encoding.UTF8, "application/json"),
        };

    private sealed class CountingHandler : HttpMessageHandler
    {
        private readonly Func<int, HttpResponseMessage> _respond;

        public CountingHandler(Func<int, HttpResponseMessage> respond) => _respond = respond;

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(_respond(Calls));
        }
    }
}
