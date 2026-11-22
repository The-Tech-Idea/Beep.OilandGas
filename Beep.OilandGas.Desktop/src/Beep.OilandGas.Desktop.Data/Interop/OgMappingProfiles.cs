using System.Globalization;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;

/// <summary>One profile as the list shows it (INT-04).</summary>
public sealed record OgProfileSummary(
    string Key,
    string Name,
    string System,
    string Version,
    bool IsBuiltIn,
    bool Verify,
    IReadOnlyList<string> Entities,
    int FieldCount,
    IReadOnlyList<string> Issues,
    string? Path,
    bool Valid)
{
    public string Describe() =>
        Key + (string.Equals(Key, Name, StringComparison.Ordinal) ? string.Empty : " (" + Name + ")") +
        " · " + (IsBuiltIn ? "built in" : "custom") + " · " + System + " v" + Version +
        " · " + Entities.Count.ToString(CultureInfo.InvariantCulture) + " entity(ies) · " +
        FieldCount.ToString(CultureInfo.InvariantCulture) + " field(s)" +
        (Verify ? " · verify before production use" : string.Empty) +
        (Valid ? string.Empty : " · " + Issues.Count.ToString(CultureInfo.InvariantCulture) + " problem(s)");
}

/// <summary>What one entity of the profile found in the file (INT-04's per-entity report).</summary>
public sealed record OgEntityReport(string Entity, string Table, bool Present, int Rows, IReadOnlyList<string> Problems)
{
    public string Describe() => Entity + " (" + Table + ") · " +
                                (Present ? Rows.ToString(CultureInfo.InvariantCulture) + " row(s)" : "not in the file") +
                                (Problems.Count == 0 ? string.Empty : " · " + string.Join("; ", Problems));
}

/// <summary>A profile applied to a file, with the resulting model (INT-04's test panel).</summary>
public sealed record OgMappingTest(
    string ProfileName,
    string SourceKind,
    string SourcePath,
    string? WellId,
    bool Passed,
    IReadOnlyList<OgEntityReport> Entities,
    IReadOnlyList<string> Problems,
    IReadOnlyList<string> Warnings,
    string ModelText,
    WellModel? Model)
{
    public int Present => Entities.Count(entity => entity.Present);

    public int Missing => Entities.Count(entity => !entity.Present);

    public string EntitiesText => string.Join(Environment.NewLine, Entities.Select(entity => entity.Describe()));

    public string ProblemsText => Problems.Count == 0
        ? "The profile covers every entity it declares."
        : string.Join(Environment.NewLine, Problems.Select(problem => "✗ " + problem));

    public string WarningsText => Warnings.Count == 0
        ? "The load raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string SummaryText => Passed
        ? "Test passed: " + ProfileName + " read " + Present.ToString(CultureInfo.InvariantCulture) + " entity(ies) from " +
          Path.GetFileName(SourcePath) + "."
        : "Test failed: " + Problems.Count.ToString(CultureInfo.InvariantCulture) + " problem(s), the first being " +
          (Problems.Count == 0 ? "(none)" : Problems[0]);
}

/// <summary>A written-back well (INT-04, AC-5 and AC-6).</summary>
public sealed record OgWriteBack(
    string ProfileName,
    string Folder,
    string? XlsxPath,
    bool Xlsx,
    IReadOnlyList<string> Tables,
    int Rows,
    bool RoundTripOk,
    IReadOnlyList<string> Differences,
    IReadOnlyList<string> Warnings)
{
    public string Describe() =>
        Rows.ToString(CultureInfo.InvariantCulture) + " row(s) over " +
        Tables.Count.ToString(CultureInfo.InvariantCulture) + " table(s) · " +
        (Xlsx ? "workbook " + (XlsxPath ?? "(no path)") : "CSV folder " + Folder) +
        " · " + (RoundTripOk ? "reloads identically" : Differences.Count.ToString(CultureInfo.InvariantCulture) + " difference(s)");

    public string DifferencesText => Differences.Count == 0
        ? "The written data reloads through the same profile with nothing changed."
        : string.Join(Environment.NewLine, Differences.Select(difference => "✗ " + difference));
}

/// <summary>What to write back (INT-04).</summary>
public sealed record OgWriteBackRequest(
    string ProfileName,
    string SourceKind,
    string SourcePath,
    string? WellId,
    string Folder,
    bool Xlsx,
    bool Overwrite);

/// <summary>The mapping-profile rules INT-04 states once.</summary>
public static class OgMappingRules
{
    public const string CodeNoConnection = "mapping-no-connection";
    public const string CodeWriteFailed = "mapping-write-failed";
    public const string CodeActorRequired = "mapping-actor-required";
    public const string CodeProfileRequired = "mapping-profile-required";
    public const string CodeProfileUnknown = "mapping-profile-unknown";
    public const string CodeProfileInvalid = "mapping-profile-invalid";
    public const string CodeBuiltIn = "mapping-profile-built-in";
    public const string CodeRequiredUnmapped = "mapping-required-unmapped";
    public const string CodeSourceRequired = "mapping-source-required";
    public const string CodeSourceKindUnknown = "mapping-source-kind-unknown";
    public const string CodeSourceMissing = "mapping-source-missing";
    public const string CodeWellRequired = "mapping-well-required";
    public const string CodeNoWellRow = "mapping-no-well-row";
    public const string CodeOverwrite = "mapping-overwrite-unconfirmed";
    public const string CodeRoundTrip = "mapping-round-trip";

    public const string Csv = "csv";
    public const string Edm = "edm";
    public const string Xlsx = "xlsx";

    /// <summary>The source kinds the tester reads, and the owner class each one uses.</summary>
    public static IReadOnlyList<string> SourceKinds { get; } = [Csv, Edm, Xlsx];

    public static string KindName(string kind) => kind.ToLowerInvariant() switch
    {
        Csv => "CSV folder",
        Edm => "EDM XML export",
        Xlsx => "Excel workbook",
        _ => kind,
    };

    /// <summary>
    /// AC-3: a required field the mapping does not provide - not in the fields, not through a formula, and not through the vertical
    /// property table - is named, with the entity it belongs to.
    /// </summary>
    public static IReadOnlyList<string> UnmappedRequired(MappingProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var problems = new List<string>();

        foreach (var (entity, map) in profile.Entities)
        {
            if (map.Required.Count == 0)
            {
                continue;
            }

            var provided = map.Fields.Keys
                .Concat(map.Properties is null ? Enumerable.Empty<string>() : map.Properties.Map.Values)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var field in map.Required.Where(field => !provided.Contains(field)))
            {
                problems.Add(entity + ": the required field '" + field +
                             "' is not mapped, so a record would load with it blank. Map it or drop the requirement.");
            }
        }

        return problems;
    }

    /// <summary>
    /// The required fields whose source column the file does not carry: mapped, but absent, which loads a blank where the profile
    /// says a value must exist.
    /// </summary>
    public static IReadOnlyList<string> MissingColumns(
        MappingProfile profile,
        string entity,
        IReadOnlySet<string> columns)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(columns);

        if (!profile.Entities.TryGetValue(entity, out var map) || map.Required.Count == 0)
        {
            return [];
        }

        var problems = new List<string>();

        foreach (var field in map.Required)
        {
            if (!map.Fields.TryGetValue(field, out var column) || column.StartsWith('='))
            {
                continue;
            }

            if (!columns.Contains(column))
            {
                problems.Add(entity + ": the required field '" + field + "' is mapped to column '" + column +
                             "', which this file does not carry.");
            }
        }

        return problems;
    }

    /// <summary>The model as the preview shows it: what the profile actually produced.</summary>
    public static string ModelText(WellModel? model, string unit)
    {
        if (model is null)
        {
            return "No model: the file did not produce one.";
        }

        var text = new StringBuilder();

        text.AppendLine("Well: " + (string.IsNullOrWhiteSpace(model.Header.Name) ? "(no name)" : model.Header.Name) +
                        (string.IsNullOrWhiteSpace(model.Header.Uwi) ? string.Empty : " · UWI " + model.Header.Uwi) +
                        (model.Header.TotalDepth is { } depth ? " · TD " + Figure(depth.In(unit)) + " " + unit : string.Empty));
        text.AppendLine("Wellbores " + model.Wellbores.Count.ToString(CultureInfo.InvariantCulture) +
                        " · hole sections " + model.HoleSections.Count.ToString(CultureInfo.InvariantCulture) +
                        " · strings " + model.Strings.Count.ToString(CultureInfo.InvariantCulture) +
                        " · components " + model.Components.Count.ToString(CultureInfo.InvariantCulture));
        text.AppendLine("Tops " + model.Tops.Count.ToString(CultureInfo.InvariantCulture) +
                        " · survey stations " + model.Survey.Count.ToString(CultureInfo.InvariantCulture) +
                        " · intervals " + model.ContactIntervals.Count.ToString(CultureInfo.InvariantCulture));

        foreach (var section in model.HoleSections.OrderBy(section => section.Bottom.In(unit)).Take(8))
        {
            text.AppendLine("  hole " + Figure(section.Top.In(unit)) + "-" + Figure(section.Bottom.In(unit)) + " " + unit +
                            (section.BitSize is { } bit ? " · bit " + Figure(bit.In("in")) + "\"" : string.Empty));
        }

        foreach (var str in model.Strings.OrderBy(str => str.Bottom.In(unit)).Take(8))
        {
            text.AppendLine("  string " + (string.IsNullOrWhiteSpace(str.Name) ? "(unnamed)" : str.Name) + " · " +
                            Figure(str.Top.In(unit)) + "-" + Figure(str.Bottom.In(unit)) + " " + unit +
                            (str.Attributes.OuterDiameter is { } od ? " · " + Figure(od.In("in")) + "\"" : string.Empty));
        }

        return text.ToString().TrimEnd();
    }

    /// <summary>
    /// AC-5: what a reload of the written data no longer matches. A profile whose write and read disagree is caught here rather than
    /// being discovered downstream.
    /// </summary>
    public static IReadOnlyList<string> Differences(WellModel original, WellModel reloaded, string unit)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(reloaded);

        var differences = new List<string>();

        void Same(string what, string? left, string? right)
        {
            if (!string.Equals(left ?? string.Empty, right ?? string.Empty, StringComparison.Ordinal))
            {
                differences.Add(what + ": written '" + (left ?? "(none)") + "', reloaded '" + (right ?? "(none)") + "'.");
            }
        }

        void Count(string what, int left, int right)
        {
            if (left != right)
            {
                differences.Add(what + ": written " + left.ToString(CultureInfo.InvariantCulture) + ", reloaded " +
                                right.ToString(CultureInfo.InvariantCulture) + ".");
            }
        }

        Same("well name", original.Header.Name, reloaded.Header.Name);
        Same("well uwi", original.Header.Uwi, reloaded.Header.Uwi);
        Count("wellbores", original.Wellbores.Count, reloaded.Wellbores.Count);
        Count("hole sections", original.HoleSections.Count, reloaded.HoleSections.Count);
        Count("strings", original.Strings.Count, reloaded.Strings.Count);
        Count("components", original.Components.Count, reloaded.Components.Count);
        Count("tops", original.Tops.Count, reloaded.Tops.Count);
        Count("survey stations", original.Survey.Count, reloaded.Survey.Count);

        var left = original.HoleSections.OrderBy(section => section.Bottom.In(unit)).ToList();
        var right = reloaded.HoleSections.OrderBy(section => section.Bottom.In(unit)).ToList();

        for (var index = 0; index < Math.Min(left.Count, right.Count); index++)
        {
            if (Math.Abs(left[index].Bottom.In(unit) - right[index].Bottom.In(unit)) > 0.001)
            {
                differences.Add("hole section " + (index + 1).ToString(CultureInfo.InvariantCulture) + " bottom: written " +
                                Figure(left[index].Bottom.In(unit)) + ", reloaded " + Figure(right[index].Bottom.In(unit)) + " " + unit + ".");
            }
        }

        var written = original.Strings.OrderBy(str => str.Bottom.In(unit)).ToList();
        var read = reloaded.Strings.OrderBy(str => str.Bottom.In(unit)).ToList();

        for (var index = 0; index < Math.Min(written.Count, read.Count); index++)
        {
            var a = written[index].Attributes.OuterDiameter?.In("in");
            var b = read[index].Attributes.OuterDiameter?.In("in");

            if (a is { } leftOd && b is { } rightOd && Math.Abs(leftOd - rightOd) > 0.001)
            {
                differences.Add("string " + (index + 1).ToString(CultureInfo.InvariantCulture) + " outside diameter: written " +
                                Figure(leftOd) + "\", reloaded " + Figure(rightOd) + "\".");
            }
        }

        return differences;
    }

    public static string Figure(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>INT-04: mapping profiles, the test panel and the write-back.</summary>
public interface IOgMappingProfiles
{
    /// <summary>The built-in profiles and every custom profile in the profiles folder.</summary>
    Task<OgResult<IReadOnlyList<OgProfileSummary>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>One profile by name, with its entities and any structural problems.</summary>
    Task<OgResult<OgProfileSummary>> LoadAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>AC-1..AC-3: applies a profile to a file and reports every entity and every required field.</summary>
    Task<OgResult<OgMappingTest>> TestAsync(
        string profileName,
        string sourceKind,
        string sourcePath,
        string? wellId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AC-4: saves a profile. A built-in cannot be overwritten in place - the save is refused and the built-in is untouched - and an
    /// invalid profile is refused, so the version already on disk stays.
    /// </summary>
    Task<OgResult<OgProfileSummary>> SaveAsync(
        string name,
        string json,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>AC-5 and AC-6: writes a well through the profile and reloads it to prove the round trip.</summary>
    Task<OgResult<OgWriteBack>> WriteBackAsync(
        OgWriteBackRequest request,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// INT-04 over the tools' <see cref="MappingProfile"/>, <see cref="MappedWellDataProvider"/>, <see cref="TabularWriter"/> and
/// <see cref="XlsxWriter"/>, with the custom profiles kept as files in the application's profiles folder.
/// </summary>
/// <remarks>
/// <para>
/// Nothing is re-implemented: the profile model, the CSV / EDM / Excel sources, the mapping provider and the two writers are the
/// library's. What this service adds is where a customer profile lives, the required-field check that makes a blank identity fail
/// loudly, the per-entity report, the overwrite guard and the round-trip proof.
/// </para>
/// <para>
/// A profile is customer data, so it is a file under the application's profiles folder rather than anything embedded in the build.
/// </para>
/// </remarks>
public sealed class OgMappingProfilesService : IOgMappingProfiles
{
    private readonly IFailureReporter _failures;
    private readonly Func<string> _folder;

    public OgMappingProfilesService(IFailureReporter failures, Func<string> profilesFolder)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _folder = profilesFolder ?? throw new ArgumentNullException(nameof(profilesFolder));
    }

    public Task<OgResult<IReadOnlyList<OgProfileSummary>>> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = new List<OgProfileSummary>();

            foreach (var name in MappingProfile.BuiltInNames)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    list.Add(Summary(name, MappingProfile.LoadBuiltIn(name), builtIn: true, path: null));
                }
                catch (Exception ex) when (ex is ArgumentException or FormatException or JsonException)
                {
                    // A built-in that does not parse is a build defect; it is listed as unusable rather than hidden.
                    var reference = _failures.ReportHandled(
                        ex, "reading the built-in mapping profile " + name, "the profile is listed as unusable",
                        FailureSeverity.Error);

                    list.Add(new OgProfileSummary(name, "(unreadable)", name, "-", true, false, [], 0,
                        ["the built-in profile could not be read (reference " + reference + ")"], null, false));
                }
            }

            foreach (var file in CustomFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));

                try
                {
                    list.Add(Summary(name, MappingProfile.LoadFromFile(file), builtIn: false, path: file));
                }
                catch (Exception ex) when (ex is FormatException or JsonException or IOException)
                {
                    // The state the feature asks for: an invalid profile is refused, and the previous version is what stays on disk -
                    // so it is listed with its problem rather than skipped as if it were not there.
                    var reference = _failures.ReportHandled(
                        ex, "reading the mapping profile " + file, "the profile is listed as invalid", FailureSeverity.Degraded);

                    list.Add(new OgProfileSummary(name, "(invalid)", name, "-", false, false, [], 0,
                        ["'" + file + "' is not a valid profile (reference " + reference + ")"], file, false));
                }
            }

            return Task.FromResult(OgResult<IReadOnlyList<OgProfileSummary>>.Ok(
                (IReadOnlyList<OgProfileSummary>)list.OrderBy(profile => profile.Key, StringComparer.OrdinalIgnoreCase).ToList()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "listing the mapping profiles", "the profiles are not shown", FailureSeverity.Error);

            return Task.FromResult(OgResult<IReadOnlyList<OgProfileSummary>>.Failed(
                "The mapping profiles could not be listed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgMappingRules.CodeProfileInvalid));
        }
    }

    public Task<OgResult<OgProfileSummary>> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Task.FromResult(OgResult<OgProfileSummary>.Failed(
                "A profile is loaded by its name; none was given.", OgMappingRules.CodeProfileRequired));
        }

        try
        {
            var (profile, builtIn, path) = Resolve(name.Trim());

            return Task.FromResult(OgResult<OgProfileSummary>.Ok(Summary(name.Trim(), profile, builtIn, path)));
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or JsonException or IOException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the mapping profile " + name, "the profile is not shown", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgProfileSummary>.Failed(
                "Profile '" + name.Trim() + "' could not be read: it is neither a custom profile in the profiles folder nor a " +
                "built-in. (reference " + reference + ")",
                OgMappingRules.CodeProfileUnknown));
        }
    }

    public async Task<OgResult<OgMappingTest>> TestAsync(
        string profileName,
        string sourceKind,
        string sourcePath,
        string? wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return OgResult<OgMappingTest>.Failed(
                "A test applies one profile; none was named.", OgMappingRules.CodeProfileRequired);
        }

        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            return OgResult<OgMappingTest>.Failed(
                "A test needs a file or folder to read; none was given.", OgMappingRules.CodeSourceRequired);
        }

        var kind = NormaliseKind(sourceKind);

        if (kind is null)
        {
            return OgResult<OgMappingTest>.Failed(
                "'" + sourceKind + "' is not a source this tester reads; use " + string.Join(", ", OgMappingRules.SourceKinds) + ".",
                OgMappingRules.CodeSourceKindUnknown);
        }

        MappingProfile profile;
        string? path = null;

        try
        {
            (profile, _, path) = Resolve(profileName.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or JsonException or IOException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the mapping profile " + profileName, "the test does not run", FailureSeverity.Degraded);

            return OgResult<OgMappingTest>.Failed(
                "Profile '" + profileName.Trim() + "' could not be read. (reference " + reference + ")",
                OgMappingRules.CodeProfileUnknown);
        }

        if (!Exists(kind, sourcePath))
        {
            return OgResult<OgMappingTest>.Failed(
                "No " + OgMappingRules.KindName(kind) + " was found at '" + sourcePath + "', so the profile has nothing to read.",
                OgMappingRules.CodeSourceMissing);
        }

        try
        {
            var source = Open(kind, sourcePath);
            var problems = new List<string>(OgMappingRules.UnmappedRequired(profile));
            var reports = new List<OgEntityReport>();

            foreach (var (entity, map) in profile.Entities)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rows = await source.QueryAsync(map.Table, new Dictionary<string, object?>(), cancellationToken)
                    .ConfigureAwait(false);

                var columns = rows
                    .SelectMany(row => row.Keys)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var entityProblems = new List<string>();

                if (rows.Count > 0)
                {
                    entityProblems.AddRange(OgMappingRules.MissingColumns(profile, entity, columns));
                }

                reports.Add(new OgEntityReport(entity, map.Table, rows.Count > 0, rows.Count, entityProblems));
                problems.AddRange(entityProblems);
            }

            var well = reports.FirstOrDefault(report => string.Equals(report.Entity, "well", StringComparison.OrdinalIgnoreCase));

            if (well is { Present: false })
            {
                problems.Add("well: the profile's well table '" + well.Table +
                             "' has no row in this file, so no well can be loaded from it.");
            }

            var warnings = new List<string>();
            WellModel? model = null;

            if (problems.Count == 0 && well is { Present: true })
            {
                var chosen = string.IsNullOrWhiteSpace(wellId)
                    ? (await new MappedWellDataProvider(source, profile).FindWellsAsync(null, cancellationToken)
                        .ConfigureAwait(false)).FirstOrDefault()?.Id ?? string.Empty
                    : wellId.Trim();

                if (string.IsNullOrWhiteSpace(chosen))
                {
                    return OgResult<OgMappingTest>.Failed(
                        "The profile read the well table but no well id: name the well to load.",
                        OgMappingRules.CodeWellRequired);
                }

                var loaded = await new MappedWellDataProvider(source, profile).LoadWellAsync(chosen, cancellationToken)
                    .ConfigureAwait(false);

                model = loaded.Model;
                warnings.AddRange(loaded.Warnings);
            }

            var test = new OgMappingTest(
                profile.Name,
                kind,
                sourcePath,
                wellId,
                problems.Count == 0 && model is not null,
                reports,
                problems,
                warnings,
                OgMappingRules.ModelText(model, WellboreEnvironment.Current.Units.DepthUnit),
                model);

            return OgResult<OgMappingTest>.Ok(test);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "testing the mapping profile " + profileName + " against " + sourcePath,
                "the test does not run", FailureSeverity.Degraded);

            return OgResult<OgMappingTest>.Failed(
                "The profile could not be tested against that file" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgMappingRules.CodeSourceMissing);
        }
    }

    public async Task<OgResult<OgProfileSummary>> SaveAsync(
        string name,
        string json,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgProfileSummary>.Failed(
                "Saving a profile records who saved it.", OgMappingRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return OgResult<OgProfileSummary>.Failed(
                "A profile is saved under a name; none was given.", OgMappingRules.CodeProfileRequired);
        }

        var clean = name.Trim();

        // AC-4: a built-in is read-only. The refusal says what to do instead, and nothing is written over it.
        if (MappingProfile.BuiltInNames.Contains(clean, StringComparer.OrdinalIgnoreCase))
        {
            return OgResult<OgProfileSummary>.Failed(
                "'" + clean + "' is a profile built into the application, so it cannot be overwritten. Save the change as a copy " +
                "under another name (for example '" + clean + "-company').", OgMappingRules.CodeBuiltIn);
        }

        if (clean.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || clean.Contains("..", StringComparison.Ordinal))
        {
            return OgResult<OgProfileSummary>.Failed(
                "'" + clean + "' is not usable as a profile name: it carries a character a file name cannot.", OgMappingRules.CodeProfileInvalid);
        }

        MappingProfile profile;

        try
        {
            profile = MappingProfile.FromJson(json);
        }
        catch (Exception ex) when (ex is JsonException or FormatException)
        {
            // The state the feature asks for: a profile that does not parse is refused, and the version already on disk stays.
            var reference = _failures.ReportHandled(
                ex, "parsing the mapping profile " + clean, "the profile is not saved and the previous version is kept",
                FailureSeverity.Degraded);

            return OgResult<OgProfileSummary>.Failed(
                "The profile JSON could not be read, so it was not saved and the previous version on disk is unchanged. " +
                "(" + ex.Message.Split('\n')[0] + "; reference " + reference + ")",
                OgMappingRules.CodeProfileInvalid);
        }

        var issues = profile.Validate();

        if (issues.Count > 0)
        {
            return OgResult<OgProfileSummary>.Failed(
                "The profile was not saved because it has " + issues.Count.ToString(CultureInfo.InvariantCulture) +
                " structural problem(s): " + string.Join(" ", issues.Take(4)) +
                (issues.Count > 4 ? " ..." : string.Empty),
                OgMappingRules.CodeProfileInvalid);
        }

        try
        {
            var folder = _folder();
            Directory.CreateDirectory(folder);

            var target = Path.Combine(folder, clean + ".profile.json");
            var temporary = target + ".tmp";

            cancellationToken.ThrowIfCancellationRequested();

            // Written to a temporary file and moved into place, so a failed write cannot leave half a profile behind.
            File.WriteAllText(temporary, profile.ToJson(), new UTF8Encoding(false));
            File.Move(temporary, target, overwrite: true);

            return OgResult<OgProfileSummary>.Ok(Summary(clean, profile, builtIn: false, path: target));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the mapping profile " + clean, "the profile is not saved", FailureSeverity.Error);

            return OgResult<OgProfileSummary>.Failed(
                "The profile could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgMappingRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWriteBack>> WriteBackAsync(
        OgWriteBackRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWriteBack>.Failed(
                "A write-back records who made it.", OgMappingRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(request.Folder))
        {
            return OgResult<OgWriteBack>.Failed(
                "A write-back needs a folder to write into; none was given.", OgMappingRules.CodeWriteFailed);
        }

        var tested = await TestAsync(request.ProfileName, request.SourceKind, request.SourcePath, request.WellId, cancellationToken)
            .ConfigureAwait(false);

        if (!tested.Success || tested.Value is null || tested.Value.Model is null)
        {
            return OgResult<OgWriteBack>.Failed(
                tested.Message ?? "The well could not be loaded through the profile, so nothing was written.",
                tested.Reference ?? OgMappingRules.CodeRoundTrip);
        }

        MappingProfile profile;

        try
        {
            (profile, _, _) = Resolve(request.ProfileName.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or JsonException or IOException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the mapping profile " + request.ProfileName, "nothing is written", FailureSeverity.Degraded);

            return OgResult<OgWriteBack>.Failed(
                "Profile '" + request.ProfileName + "' could not be read. (reference " + reference + ")",
                OgMappingRules.CodeProfileUnknown);
        }

        var model = tested.Value.Model;
        var folder = request.Folder.Trim();
        var wellName = string.IsNullOrWhiteSpace(request.WellId)
            ? (string.IsNullOrWhiteSpace(model.Header.Name) ? "well" : model.Header.Name)
            : request.WellId.Trim();

        try
        {
            var warnings = new List<string>();
            var tables = TabularWriter.ToTables(model, profile, warnings);
            var xlsxPath = request.Xlsx
                ? Path.Combine(folder, Safe(wellName) + ".xlsx")
                : null;

            // The overwrite state: an export that would replace files says so and waits for the confirmation.
            var existing = request.Xlsx
                ? (File.Exists(xlsxPath!) ? [Path.GetFileName(xlsxPath!)] : Array.Empty<string>())
                : tables
                    .Select(table => Path.Combine(folder, table.Name + ".csv"))
                    .Where(File.Exists)
                    .Select(path => Path.GetFileName(path))
                    .ToArray();

            if (existing.Length > 0 && !request.Overwrite)
            {
                return OgResult<OgWriteBack>.Failed(
                    "Writing to '" + folder + "' would overwrite " + existing.Length.ToString(CultureInfo.InvariantCulture) +
                    " existing file(s) - " + string.Join(", ", existing.Take(5)) +
                    (existing.Length > 5 ? ", ..." : string.Empty) +
                    ". Confirm the overwrite to continue; nothing has been written.",
                    OgMappingRules.CodeOverwrite);
            }

            Directory.CreateDirectory(folder);

            if (request.Xlsx)
            {
                var sheets = tables
                    .Select(table => (
                        SheetName: table.Name,
                        Header: (IReadOnlyList<string>)table.Columns.ToList(),
                        Rows: (IReadOnlyList<IReadOnlyList<object?>>)table.Rows
                            .Select(row => (IReadOnlyList<object?>)table.Columns.Select(column => (object?)row.GetValueOrDefault(column)).ToList())
                            .ToList()))
                    .ToList();

                using var file = File.Create(xlsxPath!);
                XlsxWriter.Write(file, sheets);
            }
            else
            {
                TabularWriter.WriteCsvFolder(model, folder, profile, warnings);
            }

            // AC-5: the written data is read back through the same profile, so a write that loses a row is caught here.
            ITabularSource reloadSource = request.Xlsx
                ? new ExcelTabularSource(xlsxPath!)
                : new CsvTabularSource(folder);

            var reloaded = await new MappedWellDataProvider(reloadSource, profile)
                .LoadWellAsync(string.IsNullOrWhiteSpace(request.WellId) ? model.Header.Name : request.WellId.Trim(), cancellationToken)
                .ConfigureAwait(false);

            var differences = OgMappingRules.Differences(model, reloaded.Model, WellboreEnvironment.Current.Units.DepthUnit);

            if (differences.Count > 0)
            {
                return OgResult<OgWriteBack>.Failed(
                    "The data was written to '" + folder + "' but it does not reload identically through '" + profile.Name +
                    "': " + differences[0], OgMappingRules.CodeRoundTrip);
            }

            return OgResult<OgWriteBack>.Ok(new OgWriteBack(
                profile.Name,
                folder,
                xlsxPath,
                request.Xlsx,
                tables.Select(table => table.Name).ToList(),
                tables.Sum(table => table.Rows.Count),
                RoundTripOk: true,
                differences,
                warnings.Distinct(StringComparer.Ordinal).ToList()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "writing the well back through the profile " + request.ProfileName, "nothing is written",
                FailureSeverity.Error);

            return OgResult<OgWriteBack>.Failed(
                "The well could not be written back" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgMappingRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the profile and the source

    private (MappingProfile Profile, bool BuiltIn, string? Path) Resolve(string name)
    {
        var file = Path.Combine(_folder(), name + ".profile.json");

        if (File.Exists(file))
        {
            return (MappingProfile.LoadFromFile(file), false, file);
        }

        if (MappingProfile.BuiltInNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return (MappingProfile.LoadBuiltIn(name), true, null);
        }

        throw new FileNotFoundException("No chart profile at '" + file + "' and no built-in of that name.", file);
    }

    private IEnumerable<string> CustomFiles()
    {
        var folder = _folder();

        return Directory.Exists(folder)
            ? Directory.EnumerateFiles(folder, "*.profile.json", SearchOption.TopDirectoryOnly)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            : [];
    }

    private static OgProfileSummary Summary(string key, MappingProfile profile, bool builtIn, string? path)
    {
        var issues = profile.Validate();

        return new OgProfileSummary(
            key,
            profile.Name,
            profile.System,
            profile.Version,
            builtIn,
            profile.Verify,
            profile.Entities.Keys.OrderBy(entity => entity, StringComparer.OrdinalIgnoreCase).ToList(),
            profile.Entities.Values.Sum(map => map.Fields.Count),
            issues,
            path,
            issues.Count == 0);
    }

    private static string? NormaliseKind(string? kind) =>
        string.IsNullOrWhiteSpace(kind)
            ? null
            : OgMappingRules.SourceKinds.FirstOrDefault(candidate =>
                string.Equals(candidate, kind.Trim(), StringComparison.OrdinalIgnoreCase));

    private static bool Exists(string kind, string path) =>
        string.Equals(kind, OgMappingRules.Csv, StringComparison.OrdinalIgnoreCase)
            ? Directory.Exists(path)
            : File.Exists(path);

    private static ITabularSource Open(string kind, string path) => kind switch
    {
        OgMappingRules.Edm => EdmXmlTabularSource.FromFile(path),
        OgMappingRules.Xlsx => new ExcelTabularSource(path),
        _ => new CsvTabularSource(path),
    };

    private static string Safe(string name)
    {
        var safe = new string(name.Where(character => Path.GetInvalidFileNameChars().IndexOf(character) < 0).ToArray());

        return safe.Length > 0 ? safe : "well";
    }

}
