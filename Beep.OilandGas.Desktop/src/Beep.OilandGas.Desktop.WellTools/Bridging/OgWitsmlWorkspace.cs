using System.IO;
using System.Net;
using System.Net.Http;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>What a WITSML file contributed, per entity (INT-02 AC-6).</summary>
public sealed record OgWitsmlImport(
    string Source,
    string Version,
    int Stations,
    int Markers,
    int Logs,
    IReadOnlyList<string> Warnings)
{
    /// <summary>One sentence naming every entity, including the ones that contributed nothing.</summary>
    public string Describe() =>
        $"{Source} · {Version} · {Stations} station(s) · {Markers} marker(s) · {Logs} depth log(s)";

    public bool IsPartial => Warnings.Any(warning =>
        warning.Contains("skipped", StringComparison.OrdinalIgnoreCase)
        || warning.Contains("time-based", StringComparison.OrdinalIgnoreCase)
        || warning.Contains("not a depth log", StringComparison.OrdinalIgnoreCase));
}

/// <summary>How a WITSML store check ended (INT-02 AC-5).</summary>
public enum OgWitsmlStoreState
{
    /// <summary>The store answered and named the data-schema versions it supports.</summary>
    Ok,

    /// <summary>No answer at all: DNS, refused connection, timeout, or retries exhausted.</summary>
    Unreachable,

    /// <summary>The store answered and rejected the credentials (401/403).</summary>
    AuthenticationFailed,

    /// <summary>The store answered with a fault or an error code, or the settings were not usable.</summary>
    Refused,
}

/// <summary>One store check, with a sentence that never contains the credentials (AC-4).</summary>
public sealed record OgWitsmlStoreCheck(
    OgWitsmlStoreState State,
    string Endpoint,
    string Detail,
    string? Reference = null)
{
    public bool IsOk => State == OgWitsmlStoreState.Ok;
}

/// <summary>What a store query asks for (INT-02 §3, the Store tab).</summary>
public enum OgWitsmlEntity
{
    Well,
    Wellbore,
    Trajectory,
    FormationMarker,
    Log,
}

/// <summary>The WITSML 1.4.1.1 query a store pull sends, and the wmlType it names (INT-02 §3).</summary>
public static class OgWitsmlQueries
{
    /// <summary>The <c>WMLtypeIn</c> value the store method expects for an entity.</summary>
    public static string WmlType(OgWitsmlEntity entity) => entity switch
    {
        OgWitsmlEntity.Well => "well",
        OgWitsmlEntity.Wellbore => "wellbore",
        OgWitsmlEntity.Trajectory => "trajectory",
        OgWitsmlEntity.FormationMarker => "formationMarker",
        OgWitsmlEntity.Log => "log",
    };

    public static string Name(OgWitsmlEntity entity) => entity switch
    {
        OgWitsmlEntity.Well => "well",
        OgWitsmlEntity.Wellbore => "wellbore",
        OgWitsmlEntity.Trajectory => "trajectory",
        OgWitsmlEntity.FormationMarker => "formation marker",
        OgWitsmlEntity.Log => "log",
    };

    /// <summary>
    /// A 1.4.1.1 query for one entity of one wellbore. The UIDs come from the page, so they are escaped rather than trusted; a query with
    /// no UID asks for everything in the store, which is what a "list what is there" step needs.
    /// </summary>
    public static string For(OgWitsmlEntity entity, string? uidWell, string? uidWellbore, string? uid)
    {
        var well = Escape(uidWell);
        var bore = Escape(uidWellbore);
        var self = Escape(uid);

        var (wrapper, item) = entity switch
        {
            OgWitsmlEntity.Well => ("wells", "well"),
            OgWitsmlEntity.Wellbore => ("wellbores", "wellbore"),
            OgWitsmlEntity.Trajectory => ("trajectorys", "trajectory"),
            OgWitsmlEntity.FormationMarker => ("formationMarkers", "formationMarker"),
            OgWitsmlEntity.Log => ("logs", "log"),
        };

        var attributes = new System.Text.StringBuilder();

        if (well.Length > 0)
        {
            attributes.Append(" uidWell=\"").Append(well).Append('"');
        }

        if (bore.Length > 0 && entity != OgWitsmlEntity.Well)
        {
            attributes.Append(" uidWellbore=\"").Append(bore).Append('"');
        }

        if (self.Length > 0)
        {
            attributes.Append(" uid=\"").Append(self).Append('"');
        }

        return "<" + wrapper + " xmlns=\"http://www.witsml.org/schemas/1series\" version=\"1.4.1.1\">" +
               "<" + item + attributes + "/>" +
               "</" + wrapper + ">";
    }

    private static string Escape(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : System.Security.SecurityElement.Escape(value.Trim()) ?? string.Empty;
}

/// <summary>One store pull: the state, the entity, and what the returned document contributed (INT-02 AC-4, AC-5, AC-6).</summary>
public sealed record OgWitsmlPull(
    OgWitsmlStoreState State,
    string Endpoint,
    string Entity,
    string Detail,
    OgWitsmlImport? Import = null,
    string? Reference = null)
{
    public bool IsOk => State == OgWitsmlStoreState.Ok;

    public int Rows => Import is null ? 0 : Import.Stations + Import.Markers + Import.Logs;

    public string Describe() => Detail + (Import is null ? string.Empty : " · " + Import.Describe());
}

/// <summary>How one 2.1 file stood up to the structural checks (INT-02 AC-2, as far as it goes without the schemas).</summary>
public sealed record OgWitsmlConformance(string File, IReadOnlyList<string> Problems)
{
    public bool Conforms => Problems.Count == 0;

    public string Describe() => File + (Conforms ? " · conforms" : " · " + Problems.Count + " problem(s): " + string.Join("; ", Problems));
}

/// <summary>
/// INT-02: WITSML 1.3.1.1 / 1.4.1.1 / 2.0 read, 1.x write, 2.1 export, a SOAP store check and a store pull.
/// </summary>
/// <remarks>
/// <para>
/// Files: the tools' reader never throws on bad <b>content</b> — an unsupported or unsalvageable object becomes
/// a warning, so a partly understood file still reports what it did contribute. Bad <b>XML</b> is a parse
/// failure and throws; the page reports it by reference.
/// </para>
/// <para>
/// Store credentials are <b>session only</b>: passed to the client, never stored, never audited, and never
/// included in a result sentence. The plan's DPAPI credential store does not exist yet (ADR-014 chose a
/// one-way password derivation for sign-in, which cannot be reused for an outbound password), so persisting a
/// store password is recorded as open rather than invented here.
/// </para>
/// </remarks>
public static class OgWitsmlWorkspace
{
    public const string CodeNoTrajectory = "witsml-no-trajectory";
    public const string CodeNoMarkers = "witsml-no-markers";
    public const string CodeNoLogSet = "witsml-no-log-set";
    public const string CodeWriteFailed = "witsml-write-failed";
    public const string CodeUnsupportedVersion = "witsml-version-unsupported";
    public const string CodeFolder = "witsml-folder";

    /// <summary>The versions this build reads. A document that declares another is refused naming it, not read on unchecked semantics.</summary>
    public static IReadOnlyList<string> SupportedVersions { get; } = ["1.3.1.1", "1.4.1.1", "2.0"];

    public static bool IsSupportedVersion(string? version) =>
        version is not null && SupportedVersions.Contains(version.Trim(), StringComparer.OrdinalIgnoreCase);

    /// <summary>A WITSML 1.4.1.1 trajectory with radians and metres, as public field data commonly is.</summary>
    public const string ExampleTrajectory = """
                                            <?xml version="1.0" encoding="utf-8"?>
                                            <trajectorys xmlns="http://www.witsml.org/schemas/1series" version="1.4.1.1">
                                              <trajectory uidWell="W1" uidWellbore="B1" uid="T1">
                                                <nameWell>15/9-F-4</nameWell><nameWellbore>15/9-F-4</nameWellbore><name>Definitive survey</name>
                                                <trajectoryStation uid="s1"><md uom="m">0</md><incl uom="rad">0</incl><azi uom="rad">0</azi></trajectoryStation>
                                                <trajectoryStation uid="s2"><md uom="m">1500</md><tvd uom="m">1498.1</tvd><incl uom="rad">0.1745329</incl><azi uom="rad">1.5707963</azi></trajectoryStation>
                                                <trajectoryStation uid="s3"><md uom="m">3000</md><incl uom="rad">0.5235988</incl><azi uom="rad">1.5707963</azi></trajectoryStation>
                                              </trajectory>
                                            </trajectorys>
                                            """;

    /// <summary>Formation markers for the same well; the third has no depth and is skipped with a warning.</summary>
    public const string ExampleMarkers = """
                                         <?xml version="1.0" encoding="utf-8"?>
                                         <formationMarkers xmlns="http://www.witsml.org/schemas/1series" version="1.4.1.1">
                                           <formationMarker uidWell="W1" uidWellbore="B1" uid="m1"><name>Top Draupne</name><mdPrognosed uom="m">2600</mdPrognosed><mdTopSample uom="m">2612.5</mdTopSample></formationMarker>
                                           <formationMarker uidWell="W1" uidWellbore="B1" uid="m2"><name>Top Hugin</name><mdPrognosed uom="m">2750</mdPrognosed></formationMarker>
                                           <formationMarker uidWell="W1" uidWellbore="B1" uid="m3"><name>No depth</name></formationMarker>
                                         </formationMarkers>
                                         """;

    /// <summary>A depth log and a time log: the second must be reported, not misread (AC-3).</summary>
    public const string ExampleLogs = """
                                      <?xml version="1.0" encoding="utf-8"?>
                                      <logs xmlns="http://www.witsml.org/schemas/1series" version="1.4.1.1">
                                        <log uidWell="W1" uidWellbore="B1" uid="L1">
                                          <name>MWD depth log</name><indexType>measured depth</indexType><indexCurve>DEPT</indexCurve><nullValue>-999.25</nullValue>
                                          <logCurveInfo uid="DEPT"><mnemonic>DEPT</mnemonic><unit>ft</unit></logCurveInfo>
                                          <logCurveInfo uid="GR"><mnemonic>GR</mnemonic><unit>gAPI</unit><curveDescription>Gamma ray</curveDescription></logCurveInfo>
                                          <logData><mnemonicList>DEPT,GR</mnemonicList><unitList>ft,gAPI</unitList>
                                            <data>9000,45.2</data><data>9000.5,-999.25</data><data>9001,50.1</data>
                                          </logData>
                                        </log>
                                        <log uidWell="W1" uidWellbore="B1" uid="L2"><name>Surface time log</name><indexType>date time</indexType>
                                          <logData><mnemonicList>TIME,HKLD</mnemonicList><data>2026-01-01T00:00:00Z,120</data></logData></log>
                                      </logs>
                                      """;

    /// <summary>Reads WITSML XML. Malformed XML throws; a partly understood file returns warnings.</summary>
    public static OgWitsmlImport Load(string xml, string source = "(text)")
    {
        var result = WitsmlReader.Load(xml);

        return new OgWitsmlImport(
            source,
            Version(xml),
            result.Stations,
            result.Markers,
            result.Logs,
            [.. result.Warnings]);
    }

    public static OgWitsmlImport LoadFile(string path) => Load(File.ReadAllText(path), Path.GetFileName(path));

    /// <summary>The version the document declares, for the preview (1.3.1.1 / 1.4.1.1 / 2.0 / unknown).</summary>
    public static string Version(string xml)
    {
        // The XML declaration is also a version="1.0": strip it before looking at the document's own attributes.
        var body = xml.TrimStart();

        if (body.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
        {
            var end = body.IndexOf("?>", StringComparison.Ordinal);

            if (end > 0)
            {
                body = body[(end + 2)..];
            }
        }

        var declaration = System.Text.RegularExpressions.Regex.Match(
            body,
            "version\\s*=\\s*\"(?<v>[^\"]+)\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (declaration.Success)
        {
            return declaration.Groups["v"].Value;
        }

        // 2.0 documents carry no version attribute; their namespace names the schema.
        return xml.Contains("witsmlv2", StringComparison.OrdinalIgnoreCase) ? "2.0" : "unknown";
    }

    /// <summary>
    /// Reads a document, refusing one whose declared version this build does not read. The refusal names the version found and what is
    /// supported - the feature's "unsupported version" state, rather than reading a document under semantics nobody has checked.
    /// </summary>
    public static OgResult<OgWitsmlImport> LoadChecked(string xml, string source = "(text)")
    {
        ArgumentNullException.ThrowIfNull(xml);

        var version = Version(xml);

        if (!IsSupportedVersion(version))
        {
            return OgResult<OgWitsmlImport>.Failed(
                "'" + source + "' declares WITSML " + version + ", which this build does not read; it reads " +
                string.Join(", ", SupportedVersions) + ".", CodeUnsupportedVersion);
        }

        return OgResult<OgWitsmlImport>.Ok(Load(xml, source));
    }

    /// <summary>
    /// A folder an export can write into, created when it is missing. A folder that cannot be written is refused naming the path before a
    /// single file is attempted - the feature's "export folder not writable" state.
    /// </summary>
    public static OgResult<string> EnsureFolder(string? folder, IFailureReporter failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        if (string.IsNullOrWhiteSpace(folder))
        {
            return OgResult<string>.Failed("An export writes into a folder; none was given.", CodeFolder);
        }

        var full = Path.GetFullPath(folder.Trim());

        try
        {
            Directory.CreateDirectory(full);

            // A directory can exist and still refuse a file (a read-only share, a policy), so writability is asked with a real write.
            var probe = Path.Combine(full, ".write-probe-" + Guid.NewGuid().ToString("n")[..8]);

            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);

            return OgResult<string>.Ok(full);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            failures.ReportHandled(ex, "checking that the WITSML export folder can be written",
                "the export is refused", FailureSeverity.Degraded);

            return OgResult<string>.Failed(
                "The export folder '" + full + "' cannot be written, so no file was written. Choose a folder this account can write to.",
                CodeFolder);
        }
    }

    /// <summary>
    /// Pulls one entity from a store and reads what comes back (INT-02 §3: query, results, pull into the well). The reply is the same
    /// document the file tab reads, so a store pull and a file import report the same counts.
    /// </summary>
    /// <remarks>
    /// The sentence never carries the credentials (AC-4): it is built from the endpoint, the entity and the counts. A failure is classified by
    /// the same function the check uses, so a 401 is an authentication failure here too and never "unreachable" (AC-5).
    /// </remarks>
    public static async Task<OgWitsmlPull> PullAsync(
        WitsmlStoreOptions options,
        OgWitsmlEntity entity,
        string? uidWell,
        string? uidWellbore,
        string? uid,
        IFailureReporter failures,
        HttpMessageHandler? handler = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(failures);

        var endpoint = options.Endpoint?.ToString() ?? "(none)";
        var name = OgWitsmlQueries.Name(entity);

        try
        {
            using var client = new WitsmlStoreClient(options, handler);

            var reply = await client
                .GetFromStoreAsync(OgWitsmlQueries.WmlType(entity), OgWitsmlQueries.For(entity, uidWell, uidWellbore, uid), null,
                    cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(reply.XmlOut))
            {
                return new OgWitsmlPull(OgWitsmlStoreState.Ok, endpoint, name,
                    "The store answered with no " + name + " for that UID.");
            }

            var loaded = LoadChecked(reply.XmlOut, "store:" + OgWitsmlQueries.WmlType(entity));

            if (!loaded.Success || loaded.Value is null)
            {
                return new OgWitsmlPull(OgWitsmlStoreState.Refused, endpoint, name, loaded.Message ?? "The reply could not be read.");
            }

            return new OgWitsmlPull(OgWitsmlStoreState.Ok, endpoint, name,
                "The store returned " + loaded.Value.Describe() + ".", loaded.Value);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var check = Classify(ex, endpoint);

            var reference = failures.ReportHandled(
                ex,
                "pulling WITSML " + name + " from the store at " + endpoint,
                check.Detail,
                check.State == OgWitsmlStoreState.Unreachable ? FailureSeverity.Degraded : FailureSeverity.Error);

            return new OgWitsmlPull(check.State, endpoint, name, check.Detail, null, reference);
        }
    }

    /// <summary>
    /// The structural checks a 2.1 export can be held to without the Energistics schemas (AC-2, as far as it goes): every file parses, its root
    /// is in an Energistics 2 namespace with a uuid and a matching schemaVersion, and every object reference resolves to an object in the set.
    /// </summary>
    /// <remarks>
    /// This is <b>not</b> schema validation and does not close AC-2: an XSD would also check cardinality, enumerations and types. What it does
    /// catch is the class of mistake a hand-built exporter makes - a document that does not parse, a namespace or version that disagrees with
    /// itself, and a Trajectory pointing at a Wellbore that is not in the export.
    /// </remarks>
    public static IReadOnlyList<OgWitsmlConformance> Conform21(
        IReadOnlyList<ExportFile> files, IFailureReporter failures)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(failures);

        const string witsml = "http://www.energistics.org/energyml/data/witsmlv2";
        const string common = "http://www.energistics.org/energyml/data/commonv2";

        var results = new List<OgWitsmlConformance>();
        var defined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var documents = new List<(string File, System.Xml.Linq.XDocument Document, List<string> Problems)>();

        foreach (var file in files)
        {
            var problems = new List<string>();
            System.Xml.Linq.XDocument? document = null;

            try
            {
                document = System.Xml.Linq.XDocument.Parse(file.Content);
            }
            catch (System.Xml.XmlException ex)
            {
                failures.ReportHandled(ex, "reading the exported WITSML 2.1 file " + file.Name,
                    "the file is reported as not well-formed", FailureSeverity.Degraded);

                problems.Add("does not parse as XML");
            }

            if (document?.Root is { } root)
            {
                var ns = root.Name.NamespaceName;

                if (ns is not (witsml or common))
                {
                    problems.Add("root '" + root.Name.LocalName + "' is in '" + ns + "', not an Energistics 2 namespace");
                }

                var uuid = (string?)root.Attribute("uuid");

                if (string.IsNullOrWhiteSpace(uuid))
                {
                    problems.Add("root carries no uuid");
                }
                else
                {
                    defined.Add(uuid);
                }

                var declared = (string?)root.Attribute("schemaVersion");
                var expected = ns == common ? "2.3" : "2.1";

                if (!string.Equals(declared, expected, StringComparison.Ordinal))
                {
                    problems.Add("schemaVersion '" + (declared ?? "(none)") + "' does not match its namespace (expected " + expected + ")");
                }
            }

            documents.Add((file.Name, document!, problems));
        }

        if (files.Select(file => file.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() != files.Count)
        {
            results.Add(new OgWitsmlConformance("(set)", ["two files share a name, so one would overwrite the other"]));
        }

        foreach (var (name, document, problems) in documents)
        {
            if (document?.Root is null)
            {
                results.Add(new OgWitsmlConformance(name, problems));
                continue;
            }

            // An object has a uuid attribute; a reference has an eml:Uuid child and no uuid of its own. Every referenced uuid must exist.
            foreach (var element in document.Descendants())
            {
                if (element.Attribute("uuid") is not null)
                {
                    continue;
                }

                var target = element.Elements()
                    .FirstOrDefault(child => child.Name.LocalName == "Uuid" &&
                                             child.Name.NamespaceName == common);

                if (target is null)
                {
                    continue;
                }

                var value = target.Value.Trim();

                if (value.Length > 0 && !defined.Contains(value))
                {
                    problems.Add("reference in '" + element.Name.LocalName + "' points at uuid " + value +
                                 ", which no file in the set defines");
                }
            }

            results.Add(new OgWitsmlConformance(name, problems.Distinct(StringComparer.Ordinal).ToList()));
        }

        return results;
    }

    /// <summary>The 1.x trajectory XML for a well (AC-1).</summary>
    public static OgResult<string> WriteTrajectory(WellModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.Survey.Count == 0)
        {
            return OgResult<string>.Failed("The well has no survey stations to write.", CodeNoTrajectory);
        }

        return OgResult<string>.Ok(WitsmlWriter.Trajectory(model));
    }

    /// <summary>The 1.x formation-marker XML for a well.</summary>
    public static OgResult<string> WriteMarkers(WellModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.Tops.Count == 0)
        {
            return OgResult<string>.Failed("The well has no formation markers to write.", CodeNoMarkers);
        }

        return OgResult<string>.Ok(WitsmlWriter.FormationMarkers(model));
    }

    /// <summary>The 1.x log XML for a well, when it has a depth log set.</summary>
    public static OgResult<string> WriteLogs(WellModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!model.LogSets.Any(set => set.IndexType == LogIndexType.Depth))
        {
            return OgResult<string>.Failed("The well has no depth-indexed log to write.", CodeNoLogSet);
        }

        return OgResult<string>.Ok(WitsmlWriter.Logs(model));
    }

    /// <summary>The 2.1 file set (Well, Wellbore, Trajectory, LoggingCompany, logs and LAS), with its warnings.</summary>
    public static IReadOnlyList<ExportFile> Export21(WellModel model, out IReadOnlyList<string> warnings)
    {
        ArgumentNullException.ThrowIfNull(model);

        var collected = new List<string>();
        var files = Witsml21Export.Export(model, collected);
        warnings = collected;

        return files;
    }

    /// <summary>
    /// Checks a store: the version it supports, or a state that says why it could not be reached or trusted.
    /// </summary>
    /// <remarks>
    /// Credentials are never placed in <see cref="OgWitsmlStoreCheck.Detail"/>: the detail is built from the
    /// endpoint and the failure kind only, and the failure record carries the same sentence. A 401 is
    /// <b>not</b> reported as unreachable — they need different fixes (credentials vs network).
    /// </remarks>
    public static async Task<OgWitsmlStoreCheck> CheckStoreAsync(
        WitsmlStoreOptions options,
        IFailureReporter failures,
        HttpMessageHandler? handler = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(failures);

        var endpoint = options.Endpoint?.ToString() ?? "(none)";

        try
        {
            using var client = new WitsmlStoreClient(options, handler);
            var version = await client.GetVersionAsync(cancellationToken).ConfigureAwait(false);

            return new OgWitsmlStoreCheck(
                OgWitsmlStoreState.Ok,
                endpoint,
                string.IsNullOrWhiteSpace(version)
                    ? "The store answered but named no data-schema version."
                    : $"The store answered; it supports {version}.");
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var check = Classify(ex, endpoint);

            // Reported so the evidence exists; the sentence carries no credentials either way.
            var reference = failures.ReportHandled(
                ex,
                $"checking the WITSML store at {endpoint}",
                check.Detail,
                check.State == OgWitsmlStoreState.Unreachable
                    ? FailureSeverity.Degraded
                    : FailureSeverity.Error);

            return check with { Reference = reference };
        }
    }

    /// <summary>Maps a store failure to the state a person can act on. Pure, so the mapping is testable.</summary>
    public static OgWitsmlStoreCheck Classify(Exception error, string endpoint)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error is WitsmlStoreException store)
        {
            if (store.Status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return new OgWitsmlStoreCheck(
                    OgWitsmlStoreState.AuthenticationFailed,
                    endpoint,
                    $"The store refused these credentials ({(int)store.Status!.Value}). Check the user and password; nothing is retried.");
            }

            // A SOAP fault or a negative WITSML result: the store answered and said no.
            if (store.Status is not null && (int)store.Status < 500)
            {
                return new OgWitsmlStoreCheck(
                    OgWitsmlStoreState.Refused,
                    endpoint,
                    $"The store answered with HTTP {(int)store.Status.Value}.");
            }

            if (store.Status is not null || store.InnerException is HttpRequestException)
            {
                return new OgWitsmlStoreCheck(
                    OgWitsmlStoreState.Unreachable,
                    endpoint,
                    "The store could not be reached: no answer after the configured retries.");
            }

            return new OgWitsmlStoreCheck(
                OgWitsmlStoreState.Refused,
                endpoint,
                "The store rejected the request.");
        }

        if (error is ArgumentException)
        {
            return new OgWitsmlStoreCheck(
                OgWitsmlStoreState.Refused,
                endpoint,
                "The store settings are not usable: name an https endpoint before adding credentials.");
        }

        return new OgWitsmlStoreCheck(
            OgWitsmlStoreState.Unreachable,
            endpoint,
            "The store could not be reached: no answer.");
    }
}
