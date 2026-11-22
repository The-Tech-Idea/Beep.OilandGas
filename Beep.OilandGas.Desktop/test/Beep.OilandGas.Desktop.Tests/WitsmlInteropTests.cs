using System.Net;
using System.Net.Http;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-02: WITSML 1.4.1.1 trajectory and markers round-trip through the writer, a time log is reported as
/// unsupported, a partial import names each entity, the 2.1 export is well-formed, and a store check tells
/// "no answer" from "credentials refused" without ever printing the password.
/// </summary>
public sealed class WitsmlInteropTests
{
    private const string Password = "S3cret!Store-42";
    private const string User = "svc_witsml";

    [Fact]
    public void A_141_trajectory_and_markers_load_producing_both_entities()
    {
        UseDefaultEnvironment();

        var trajectory = OgWitsmlWorkspace.Load(OgWitsmlWorkspace.ExampleTrajectory, "trajectory.xml");
        Assert.Equal("1.4.1.1", trajectory.Version);
        Assert.Equal(3, trajectory.Stations);
        Assert.Equal(0, trajectory.Markers);

        var markers = OgWitsmlWorkspace.Load(OgWitsmlWorkspace.ExampleMarkers, "markers.xml");
        Assert.Equal(2, markers.Markers);

        // The third marker has no depth: it is skipped with a warning, not silently dropped.
        Assert.Contains(markers.Warnings, warning =>
            warning.Contains("no measured depth", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_141_trajectory_and_markers_round_trip_through_the_writer()
    {
        UseDefaultEnvironment();

        var loaded = OgWitsmlWorkspace.Load(OgWitsmlWorkspace.ExampleTrajectory);

        var written = OgWitsmlWorkspace.WriteTrajectory(LoadModel(OgWitsmlWorkspace.ExampleTrajectory));
        Assert.True(written.Success, written.Message);

        var reloaded = OgWitsmlWorkspace.Load(written.Value!);

        // AC-1: same stations, same depths, same angles (writer uses degrees; reader converts back).
        Assert.Equal(loaded.Stations, reloaded.Stations);

        var original = LoadModel(OgWitsmlWorkspace.ExampleTrajectory);
        var again = LoadModel(written.Value!);

        Assert.Equal(
            original.Survey.Select(station => station.Md.In("m")),
            again.Survey.Select(station => station.Md.In("m")));
        Assert.Equal(original.Survey[1].Inclination, again.Survey[1].Inclination, 3);
        Assert.Equal(original.Survey[2].Azimuth, again.Survey[2].Azimuth, 3);

        // Markers round-trip with their names and depths.
        var markerModel = LoadModel(OgWitsmlWorkspace.ExampleMarkers);
        var markerXml = OgWitsmlWorkspace.WriteMarkers(markerModel);
        Assert.True(markerXml.Success, markerXml.Message);

        var markerAgain = LoadModel(markerXml.Value!);
        Assert.Equal(2, markerAgain.Tops.Count);
        Assert.Equal("Top Draupne", markerAgain.Tops[0].Name);

        // The reader prefers the sampled depth over the prognosed one; the writer carries that same depth.
        Assert.Equal(2612.5, markerAgain.Tops[0].Depth.In("m"), 3);
    }

    [Fact]
    public void A_time_indexed_log_is_reported_as_unsupported()
    {
        UseDefaultEnvironment();

        var logs = OgWitsmlWorkspace.Load(OgWitsmlWorkspace.ExampleLogs);

        // AC-3: the depth log is read; the time log is named as unsupported rather than misread.
        Assert.Equal(1, logs.Logs);
        Assert.Contains(logs.Warnings, warning =>
            warning.Contains("time-based", StringComparison.OrdinalIgnoreCase)
            && warning.Contains("Surface time log", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_partial_import_reports_each_entity()
    {
        UseDefaultEnvironment();

        var logs = OgWitsmlWorkspace.Load(OgWitsmlWorkspace.ExampleLogs, "logs.xml");

        // AC-6: counts per entity, and the problem is visible in the same sentence set.
        Assert.True(logs.IsPartial);
        Assert.Contains("0 station(s)", logs.Describe(), StringComparison.Ordinal);
        Assert.Contains("1 depth log(s)", logs.Describe(), StringComparison.Ordinal);
        Assert.NotEmpty(logs.Warnings);
    }

    [Fact]
    public void A_witsml_20_document_is_recognised_by_its_namespace()
    {
        UseDefaultEnvironment();

        const string v2 = """
                          <Trajectory xmlns="http://www.energistics.org/energyml/data/witsmlv2" uid="T2">
                            <TrajectoryStation uid="a"><Md uom="ft">0</Md><Incl uom="dega">0</Incl><Azi uom="dega">0</Azi></TrajectoryStation>
                            <TrajectoryStation uid="b"><Md uom="ft">5000</Md><Incl uom="dega">30</Incl><Azi uom="dega">90</Azi></TrajectoryStation>
                          </Trajectory>
                          """;

        var loaded = OgWitsmlWorkspace.Load(v2);

        Assert.Equal("2.0", loaded.Version);
        Assert.Equal(2, loaded.Stations);
    }

    [Fact]
    public void A_21_export_produces_the_expected_files_and_wellformed_xml()
    {
        UseDefaultEnvironment();

        var model = LoadModel(OgWitsmlWorkspace.ExampleTrajectory);
        var files = OgWitsmlWorkspace.Export21(model, out _);

        Assert.NotEmpty(files);
        Assert.Contains(files, file => file.Name == "Well.xml");
        Assert.Contains(files, file => file.Name == "Trajectory.xml");

        foreach (var file in files.Where(file => file.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
        {
            var document = System.Xml.Linq.XDocument.Parse(file.Content);

            // AC-2's in-process half: the files are well-formed and in the 2.1 namespace. Schema validation
            // against the Energistics XSDs is not shipped in this repository (see the evidence limits).
            Assert.Contains(
                "energistics.org/energyml/data/witsmlv2",
                document.Root!.Name.NamespaceName,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Store_unreachable_and_authentication_failure_are_distinguishable()
    {
        UseDefaultEnvironment();

        var unreachable = await OgWitsmlWorkspace.CheckStoreAsync(
            Options(),
            new QuietFailures(),
            new ThrowingHandler());

        var refused = await OgWitsmlWorkspace.CheckStoreAsync(
            Options(),
            new QuietFailures(),
            new StatusHandler(HttpStatusCode.Unauthorized));

        var notFound = await OgWitsmlWorkspace.CheckStoreAsync(
            Options(),
            new QuietFailures(),
            new StatusHandler(HttpStatusCode.NotFound));

        // AC-5: three different answers, each with the next step in its sentence.
        Assert.Equal(OgWitsmlStoreState.Unreachable, unreachable.State);
        Assert.Equal(OgWitsmlStoreState.AuthenticationFailed, refused.State);
        Assert.Equal(OgWitsmlStoreState.Refused, notFound.State);

        Assert.Contains("could not be reached", unreachable.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("credentials", refused.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Store_credentials_never_appear_in_the_result_or_the_failure_record()
    {
        UseDefaultEnvironment();

        var failures = new QuietFailures();

        var unreachable = await OgWitsmlWorkspace.CheckStoreAsync(
            Options(),
            failures,
            new ThrowingHandler());

        var refused = await OgWitsmlWorkspace.CheckStoreAsync(
            Options(),
            failures,
            new StatusHandler(HttpStatusCode.Unauthorized));

        // AC-4: neither the sentence the person reads nor the record the administrator reads carries them.
        foreach (var text in new[] { unreachable.Detail, refused.Detail, unreachable.Reference ?? string.Empty })
        {
            Assert.DoesNotContain(Password, text, StringComparison.Ordinal);
            Assert.DoesNotContain(User, text, StringComparison.Ordinal);
        }

        Assert.NotEmpty(failures.Reported);
        Assert.All(failures.Reported, row =>
        {
            Assert.DoesNotContain(Password, row, StringComparison.Ordinal);
            Assert.DoesNotContain(User, row, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task Credentials_over_plain_http_are_refused_before_a_request_is_sent()
    {
        UseDefaultEnvironment();

        var options = Options();
        options.Endpoint = new Uri("http://store.example.test/Store");

        var check = await OgWitsmlWorkspace.CheckStoreAsync(options, new QuietFailures());

        Assert.Equal(OgWitsmlStoreState.Refused, check.State);
        Assert.Contains("https", check.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Password, check.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void A_document_whose_version_is_not_read_is_refused_naming_it()
    {
        // INT-02's unsupported-version state: the version found is in the sentence, and nothing is read under semantics nobody checked.
        const string old =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <trajectorys xmlns="http://www.witsml.org/schemas/1series" version="1.2">
              <trajectory uidWell="W1" uidWellbore="B1" uid="T1"><name>a</name></trajectory>
            </trajectorys>
            """;

        var refused = OgWitsmlWorkspace.LoadChecked(old, "old.xml");

        Assert.False(refused.Success);
        Assert.Equal(OgWitsmlWorkspace.CodeUnsupportedVersion, refused.Reference);
        Assert.Contains("1.2", refused.Message!, StringComparison.Ordinal);
        Assert.Contains("1.4.1.1", refused.Message!, StringComparison.Ordinal);

        // The versions the build reads are read, and the list is the list the refusal prints.
        Assert.Equal(["1.3.1.1", "1.4.1.1", "2.0"], OgWitsmlWorkspace.SupportedVersions);
        Assert.True(OgWitsmlWorkspace.LoadChecked(OgWitsmlWorkspace.ExampleTrajectory, "now.xml").Success);
        Assert.True(OgWitsmlWorkspace.IsSupportedVersion("2.0"));
        Assert.False(OgWitsmlWorkspace.IsSupportedVersion("1.2"));
        Assert.False(OgWitsmlWorkspace.IsSupportedVersion(null));
    }

    [Fact]
    public void An_export_folder_that_cannot_be_written_is_refused_naming_the_path()
    {
        // The feature's "export folder not writable" state: refused with the path, before a single file is attempted.
        var root = Path.Combine(Path.GetTempPath(), "witsml-folder-" + Guid.NewGuid().ToString("n")[..8]);

        Directory.CreateDirectory(root);

        try
        {
            var written = OgWitsmlWorkspace.EnsureFolder(Path.Combine(root, "exports"), new QuietFailures());

            Assert.True(written.Success, written.Message);
            Assert.True(Directory.Exists(written.Value!));

            // A file where the folder should be is the folder that cannot be written.
            var blocked = Path.Combine(root, "blocked");

            File.WriteAllText(blocked, "not a folder");

            var refused = OgWitsmlWorkspace.EnsureFolder(blocked, new QuietFailures());

            Assert.False(refused.Success);
            Assert.Equal(OgWitsmlWorkspace.CodeFolder, refused.Reference);
            Assert.Contains(blocked, refused.Message!, StringComparison.Ordinal);

            Assert.Equal(OgWitsmlWorkspace.CodeFolder, OgWitsmlWorkspace.EnsureFolder(" ", new QuietFailures()).Reference);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task A_store_pull_reads_the_returned_document_into_the_same_counts()
    {
        UseDefaultEnvironment();

        var failures = new QuietFailures();

        var pull = await OgWitsmlWorkspace.PullAsync(
            Options(),
            OgWitsmlEntity.Trajectory,
            "W1",
            "B1",
            "T1",
            failures,
            new SoapReplyHandler(OgWitsmlWorkspace.ExampleTrajectory));

        Assert.Equal(OgWitsmlStoreState.Ok, pull.State);
        Assert.NotNull(pull.Import);

        // The reply is the same document the file tab reads, so the counts and the entity sentence are the same.
        Assert.Equal(3, pull.Import!.Stations);
        Assert.Equal(3, pull.Rows);
        Assert.Equal("trajectory", pull.Entity);
        Assert.Contains("3 station(s)", pull.Detail, StringComparison.Ordinal);
        Assert.Contains("3 station(s)", pull.Describe(), StringComparison.Ordinal);

        // AC-4: the sentence a person reads carries no credentials, and nothing was reported as a failure.
        Assert.DoesNotContain(Password, pull.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain(User, pull.Detail, StringComparison.Ordinal);
        Assert.Empty(failures.Reported);

        // An empty answer is an answer, not a failure: the store said there is nothing under that UID.
        var empty = await OgWitsmlWorkspace.PullAsync(
            Options(), OgWitsmlEntity.Trajectory, "W1", "B1", "NOPE", failures,
            new SoapReplyHandler(string.Empty));

        Assert.Equal(OgWitsmlStoreState.Ok, empty.State);
        Assert.Null(empty.Import);
        Assert.Contains("no trajectory", empty.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_store_pull_that_fails_is_classified_the_same_way_as_a_check()
    {
        UseDefaultEnvironment();

        var failures = new QuietFailures();

        // AC-5, on the pull as well as the check: a 401 is an authentication failure, never "unreachable".
        var refused = await OgWitsmlWorkspace.PullAsync(
            Options(), OgWitsmlEntity.Log, "W1", "B1", "L1", failures, new StatusHandler(HttpStatusCode.Unauthorized));

        var unreachable = await OgWitsmlWorkspace.PullAsync(
            Options(), OgWitsmlEntity.Log, "W1", "B1", "L1", failures, new ThrowingHandler());

        Assert.Equal(OgWitsmlStoreState.AuthenticationFailed, refused.State);
        Assert.Equal(OgWitsmlStoreState.Unreachable, unreachable.State);
        Assert.Equal("log", refused.Entity);

        // AC-4: every sentence and every reported row is free of the credentials, on both paths.
        foreach (var text in new[] { refused.Detail, unreachable.Detail, refused.Reference ?? string.Empty })
        {
            Assert.DoesNotContain(Password, text, StringComparison.Ordinal);
            Assert.DoesNotContain(User, text, StringComparison.Ordinal);
        }

        Assert.NotEmpty(failures.Reported);
        Assert.All(failures.Reported, row =>
        {
            Assert.DoesNotContain(Password, row, StringComparison.Ordinal);
            Assert.DoesNotContain(User, row, StringComparison.Ordinal);
        });

        // The query that was sent names the entity and the UIDs, and nothing else.
        var query = OgWitsmlQueries.For(OgWitsmlEntity.Log, "W1", "B1", "L1");

        Assert.Contains("1.4.1.1", query, StringComparison.Ordinal);
        Assert.Contains("uidWell=\"W1\"", query, StringComparison.Ordinal);
        Assert.Contains("uidWellbore=\"B1\"", query, StringComparison.Ordinal);
        Assert.Contains("uid=\"L1\"", query, StringComparison.Ordinal);
        Assert.Equal("log", OgWitsmlQueries.WmlType(OgWitsmlEntity.Log));

        // A UID that carries XML is escaped rather than injected into the query.
        var hostile = OgWitsmlQueries.For(OgWitsmlEntity.Well, "\"/><x", null, null);

        Assert.Contains("&quot;", hostile, StringComparison.Ordinal);
        Assert.DoesNotContain("/><x", hostile, StringComparison.Ordinal);
    }

    [Fact]
    public void The_21_export_conforms_structurally_and_a_dangling_reference_is_caught()
    {
        UseDefaultEnvironment();

        var model = LoadModel(OgWitsmlWorkspace.ExampleTrajectory);
        var files = OgWitsmlWorkspace.Export21(model, out _);
        var failures = new QuietFailures();

        var conformance = OgWitsmlWorkspace.Conform21(files, failures);

        // AC-2's in-process half: nothing here is schema validation, and the sentence the page prints says so.
        Assert.Equal(files.Count, conformance.Count);
        Assert.All(conformance, item => Assert.True(item.Conforms, item.Describe()));
        Assert.Empty(failures.Reported);

        // A reference that resolves to nothing is caught and named - the class of mistake a hand-built exporter makes. The trajectory
        // points at the wellbore, whose uuid is the Wellbore file's own, so replacing that value in the trajectory orphans the reference.
        var trajectory = files.Single(file => file.Name == "Trajectory.xml");
        var wellbore = files.Single(file => file.Name == "Wellbore.xml");
        var wellboreId = System.Xml.Linq.XDocument.Parse(wellbore.Content).Root!.Attribute("uuid")!.Value;

        Assert.Contains(wellboreId, trajectory.Content, StringComparison.Ordinal);

        var orphan = "00000000-0000-0000-0000-000000000000";
        var tampered = files
            .Select(file => file.Name == trajectory.Name
                ? new ExportFile(file.Name, file.Content.Replace(wellboreId, orphan, StringComparison.Ordinal))
                : file)
            .ToList();

        var broken = OgWitsmlWorkspace.Conform21(tampered, failures).Single(item => item.File == trajectory.Name);

        Assert.False(broken.Conforms);
        Assert.Contains(orphan, broken.Describe(), StringComparison.Ordinal);

        // A file that is not XML at all, and a namespace that disagrees with its own schemaVersion, are both reported.
        var garbage = OgWitsmlWorkspace.Conform21([new ExportFile("Bad.xml", "not xml")], failures);

        Assert.False(garbage.Single().Conforms);
        Assert.Contains("does not parse", garbage.Single().Describe(), StringComparison.Ordinal);

        var mismatched = OgWitsmlWorkspace.Conform21(
            [new ExportFile("Wrong.xml",
                """
                <Well xmlns="http://www.energistics.org/energyml/data/witsmlv2" uuid="11111111-1111-1111-1111-111111111111" schemaVersion="2.3"/>
                """)],
            failures);

        Assert.False(mismatched.Single().Conforms);
        Assert.Contains("does not match its namespace", mismatched.Single().Describe(), StringComparison.Ordinal);

        // Two files with one name would overwrite each other on disk.
        var duplicate = OgWitsmlWorkspace.Conform21([files[0], files[0]], failures);

        Assert.Contains(duplicate, item => !item.Conforms && item.File == "(set)");
    }

    /// <summary>A store that answers one SOAP method with the document a test hands it.</summary>
    private sealed class SoapReplyHandler : HttpMessageHandler
    {
        private readonly string _xmlOut;

        public SoapReplyHandler(string xmlOut) => _xmlOut = xmlOut;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body =
                "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>" +
                "<WMLS_GetFromStoreResponse xmlns=\"http://www.witsml.org/message/120\">" +
                "<Result>1</Result><SuppMsgOut/>" +
                (_xmlOut.Length == 0 ? "<XMLout/>" : "<XMLout><![CDATA[" + _xmlOut + "]]></XMLout>") +
                "</WMLS_GetFromStoreResponse></soap:Body></soap:Envelope>";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "text/xml"),
            });
        }
    }

    private static WitsmlStoreOptions Options() => new()
    {
        Endpoint = new Uri("https://store.example.test/Store"),
        UserName = User,
        Password = Password,
        Timeout = TimeSpan.FromSeconds(5),
        Retries = 0,
    };

    private static WellModel LoadModel(string xml) => WitsmlReader.Load(xml).Model;

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("probe: the store did not answer");
    }

    private sealed class StatusHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;

        public StatusHandler(HttpStatusCode status) => _status = status;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status));
    }

    /// <summary>Records what would have gone to the store, so a test can inspect it for secrets.</summary>
    private sealed class QuietFailures : IFailureReporter
    {
        public List<string> Reported { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Reported.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
