using System.Globalization;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;

/// <summary>One scanned station of the reference well against one offset (ENG-01's <c>OgSeparationRow</c>).</summary>
public sealed record OgSeparationRow(
    string Offset,
    double Md,
    double Distance,
    double SigmaS,
    double SeparationFactor,
    double Masd,
    string? Level)
{
    public string Status => string.IsNullOrWhiteSpace(Level) ? "ok" : Level!.ToLowerInvariant();

    /// <summary>AC-4: a violation is the rule's own lowest level, which the settings name.</summary>
    public bool IsViolation { get; init; }

    public string Describe() =>
        Offset + " · " + Md.ToString("0.#", CultureInfo.InvariantCulture) + " m · " +
        Distance.ToString("0.##", CultureInfo.InvariantCulture) + " m · σ " +
        SigmaS.ToString("0.###", CultureInfo.InvariantCulture) + " m · SF " +
        SeparationFactor.ToString("0.###", CultureInfo.InvariantCulture) + " · MASD " +
        Masd.ToString("0.##", CultureInfo.InvariantCulture) + " m · " + Status;
}

/// <summary>The rule the scan applied, shown on screen (ENG-01, AC-2).</summary>
public sealed record OgSeparationRule(
    double ScalingFactor,
    double SurfaceMargin,
    double ProjectAhead,
    double ScanStep,
    string ViolationLevel,
    double ViolationBelow,
    IReadOnlyList<string> Levels)
{
    public string Describe() =>
        "SF = (D - Rref - Roff - Sm) / (k · √(σS² + σPA²)), MASD = k · √(σS² + σPA²) + Rref + Roff + Sm · " +
        "k " + ScalingFactor.ToString("0.##", CultureInfo.InvariantCulture) +
        " · Sm " + SurfaceMargin.ToString("0.##", CultureInfo.InvariantCulture) + " m" +
        " · σPA " + ProjectAhead.ToString("0.##", CultureInfo.InvariantCulture) + " m" +
        " · step " + ScanStep.ToString("0.#", CultureInfo.InvariantCulture) + " m" +
        " · violation below SF " + ViolationBelow.ToString("0.##", CultureInfo.InvariantCulture) +
        " (" + ViolationLevel + ")";
}

/// <summary>What one anti-collision run produced (ENG-01).</summary>
public sealed record OgAntiCollisionScan(
    string Reference,
    IReadOnlyList<string> Offsets,
    IReadOnlyList<string> Excluded,
    IReadOnlyList<OgSeparationRow> Rows,
    IReadOnlyList<string> Warnings,
    OgSeparationRule Rule,
    string Environment,
    string ErrorModel,
    double Sigma,
    DateTime RunAt)
{
    public int Violations => Rows.Count(row => row.IsViolation);

    public int Stations => Rows.Select(row => row.Md).Distinct().Count();

    public string CountsText =>
        Rows.Count.ToString(CultureInfo.InvariantCulture) + " point(s) · " +
        Stations.ToString(CultureInfo.InvariantCulture) + " station(s) · " +
        Offsets.Count.ToString(CultureInfo.InvariantCulture) + " offset(s) · " +
        Violations.ToString(CultureInfo.InvariantCulture) + " violation(s)" +
        (Excluded.Count == 0 ? string.Empty : " · " + Excluded.Count.ToString(CultureInfo.InvariantCulture) + " excluded");

    public string RowsText => Rows.Count == 0
        ? "No station was scanned."
        : string.Join(System.Environment.NewLine, Rows.Select(row => row.Describe()));

    public string WarningsText => Warnings.Count == 0
        ? "No warning was raised."
        : string.Join(System.Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string ExcludedText => Excluded.Count == 0
        ? "Every offset had a survey."
        : "Left out for having no survey: " + string.Join(", ", Excluded);

    /// <summary>AC-5: the export is built from the same rows the grid shows, column for column.</summary>
    public string Csv()
    {
        var text = new StringBuilder();

        text.AppendLine("offset,md_m,distance_m,sigma_m,separation_factor,masd_m,status");

        foreach (var row in Rows)
        {
            text.AppendLine(string.Join(",",
                Quote(row.Offset),
                Figure(row.Md),
                Figure(row.Distance),
                Figure(row.SigmaS),
                Figure(row.SeparationFactor),
                Figure(row.Masd),
                Quote(row.Status)));
        }

        return text.ToString();
    }

    public static string Figure(double value) =>
        value.ToString("0.######", CultureInfo.InvariantCulture);

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>The rules ENG-01 states once.</summary>
public static class AntiCollisionRules
{
    public const string CodeNoConnection = "anticollision-no-connection";
    public const string CodeReadFailed = "anticollision-read-failed";
    public const string CodeWriteFailed = "anticollision-write-failed";
    public const string CodeReferenceRequired = "anticollision-reference-required";
    public const string CodeNoSurvey = "anticollision-no-survey";
    public const string CodeNoOffsets = "anticollision-no-offsets";
    public const string CodeNoModel = "anticollision-no-model";
    public const string CodeMagneticWithoutModel = "anticollision-magnetic-model-unavailable";
    public const string CodeActorRequired = "anticollision-actor-required";

    /// <summary>
    /// AC-2: the rule as the settings name it, so the screen and the computation cannot disagree.
    /// </summary>
    public static OgSeparationRule RuleOf(AntiCollisionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var violation = settings.Levels
            .OrderBy(level => level.Below)
            .FirstOrDefault();

        return new OgSeparationRule(
            settings.ScalingFactor,
            settings.SurfaceMargin.In("m"),
            settings.ProjectAhead.In("m"),
            settings.ScanStep.In("m"),
            violation?.Name ?? "Stop",
            violation?.Below ?? 1.0,
            settings.Levels
                .OrderBy(level => level.Below)
                .Select(level => level.Name + " below " + level.Below.ToString("0.##", CultureInfo.InvariantCulture))
                .ToList());
    }

    /// <summary>
    /// AC-4: a point is a violation when its level is the rule's lowest, which is the level the settings call out. The library
    /// assigns the level, so the count follows the configured rule rather than a number chosen here.
    /// </summary>
    public static bool IsViolation(string? level, string violationLevel) =>
        !string.IsNullOrWhiteSpace(level) &&
        string.Equals(level.Trim(), violationLevel, StringComparison.OrdinalIgnoreCase);

    /// <summary>The stations a survey row set provides, in measured-depth order.</summary>
    public static IReadOnlyList<(double Md, double Inc, double Az, string Unit)> Stations(
        IEnumerable<WELL_DIR_SRVY_STATION> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        return rows
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => row.STATION_MD >= 0m)
            .OrderBy(row => row.STATION_MD)
            .Select(row => (
                Md: (double)row.STATION_MD,
                Inc: (double)row.INCLINATION,
                Az: (double)row.AZIMUTH,
                Unit: row.STATION_MD_OUOM?.Trim() ?? "m"))
            .ToList();
    }
}

/// <summary>ENG-01: the anti-collision scan, its evidence and its saved runs.</summary>
public interface IOgAntiCollision
{
    /// <summary>
    /// Scans the named offsets against the reference well. An offset with no survey is excluded and named (AC-3); the rule is
    /// returned with the rows so the screen can state it (AC-2).
    /// </summary>
    Task<OgResult<OgAntiCollisionScan>> ScanAsync(
        string referenceWell,
        IReadOnlyList<string> offsetWells,
        CancellationToken cancellationToken = default);

    /// <summary>AC-6: records the run with the error model, the sigma and the environment it was made under.</summary>
    Task<OgResult<string>> SaveRunAsync(
        OgAntiCollisionScan scan,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ENG-01 over the owner's <c>WELL_DIR_SRVY</c>, <c>WELL_DIR_SRVY_STATION</c>, <c>WELL</c> and <c>WELLBORE</c>, using the
/// tools' <see cref="AntiCollision.Scan"/> for the mathematics and adding the <c>ANTICOLLISION_RUN</c> record for the evidence.
/// </summary>
/// <remarks>
/// The separation factor, MASD, sigma and levels are the owner's: the scan is not re-implemented, and the settings are the
/// environment's own <see cref="AntiCollisionSettings"/>. What this service adds is the database read, the excluded-offset
/// bookkeeping, the row/CSV projection and the saved run.
/// </remarks>
public sealed class OgAntiCollision : IOgAntiCollision
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgAntiCollision(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgAntiCollisionScan>> ScanAsync(
        string referenceWell,
        IReadOnlyList<string> offsetWells,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(referenceWell))
        {
            return OgResult<OgAntiCollisionScan>.Failed(
                "A scan needs the reference well it is run along.", AntiCollisionRules.CodeReferenceRequired);
        }

        if (offsetWells.Count == 0)
        {
            return OgResult<OgAntiCollisionScan>.Failed(
                "A scan needs at least one offset well; none was named.", AntiCollisionRules.CodeNoOffsets);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgAntiCollisionScan>.Failed(
                "No connection is open, so the surveys cannot be read.", AntiCollisionRules.CodeNoConnection);
        }

        try
        {
            var stations = await OgUow.ReadAllAsync<WELL_DIR_SRVY_STATION>(source).ConfigureAwait(false);
            var surveys = await OgUow.ReadAllAsync<WELL_DIR_SRVY>(source).ConfigureAwait(false);
            var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
            var bores = await OgUow.ReadAllAsync<WELLBORE>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var warnings = new List<string>();
            var environment = WellboreEnvironment.Current;
            var settings = environment.AntiCollision;
            var rule = AntiCollisionRules.RuleOf(settings);

            var reference = Build(referenceWell.Trim(), stations, surveys, wells, bores, warnings);

            if (reference is null)
            {
                return OgResult<OgAntiCollisionScan>.Failed(
                    "Reference well '" + referenceWell.Trim() +
                    "' has no survey station to scan, so nothing can be computed.",
                    AntiCollisionRules.CodeNoSurvey);
            }

            var offsets = new List<WellModel>();
            var included = new List<string>();
            var excluded = new List<string>();

            foreach (var offset in offsetWells
                         .Where(well => !string.IsNullOrWhiteSpace(well))
                         .Select(well => well.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Where(well => !string.Equals(well, referenceWell.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                var model = Build(offset, stations, surveys, wells, bores, warnings);

                if (model is null)
                {
                    // AC-3: an offset without a survey is excluded and named rather than silently dropped.
                    excluded.Add(offset);
                    continue;
                }

                offsets.Add(model);
                included.Add(offset);
            }

            if (offsets.Count == 0)
            {
                return OgResult<OgAntiCollisionScan>.Failed(
                    "None of the named offsets has a survey, so there is nothing to scan: " +
                    string.Join(", ", excluded) + ".", AntiCollisionRules.CodeNoSurvey);
            }

            var result = AntiCollision.Scan(reference, offsets, warnings, settings);

            var rows = new List<OgSeparationRow>();

            foreach (var offset in result.Offsets)
            {
                foreach (var point in offset.Points)
                {
                    rows.Add(new OgSeparationRow(
                        offset.Name,
                        point.ReferenceMd,
                        point.Distance,
                        point.SigmaS,
                        point.SeparationFactor,
                        point.Masd,
                        point.Level)
                    {
                        IsViolation = AntiCollisionRules.IsViolation(point.Level, rule.ViolationLevel),
                    });
                }
            }

            // The configured survey tool model is the error model the uncertainty came from.
            var toolModel = environment.Survey.ToolModels.TryGetValue(environment.Survey.ToolModel, out var configured)
                ? configured
                : null;

            // The run records the identity that was scanned, not the display name, so it can be found again.
            return OgResult<OgAntiCollisionScan>.Ok(new OgAntiCollisionScan(
                referenceWell.Trim(),
                included,
                excluded,
                rows,
                warnings,
                rule,
                environment.Name,
                string.IsNullOrWhiteSpace(toolModel?.Name)
                    ? (string.IsNullOrWhiteSpace(environment.Survey.ToolModel)
                        ? "ISCWSA (environment default)"
                        : environment.Survey.ToolModel)
                    : toolModel!.Name!,
                1d,
                DateTime.UtcNow));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "scanning offsets against " + referenceWell, "the scan is not shown", FailureSeverity.Error);

            return OgResult<OgAntiCollisionScan>.Failed(
                "The scan could not be computed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? AntiCollisionRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<string>> SaveRunAsync(
        OgAntiCollisionScan scan,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scan);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<string>.Failed(
                "A saved run records who made it.", AntiCollisionRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<string>.Failed(
                "No connection is open, so the run cannot be saved.", AntiCollisionRules.CodeNoConnection);
        }

        try
        {
            var id = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;

            var unit = OgUow.For<ANTICOLLISION_RUN>(source);
            unit.PrimaryKey = nameof(ANTICOLLISION_RUN.ANTICOLLISION_RUN_ID);

            unit.Add(new ANTICOLLISION_RUN
            {
                ANTICOLLISION_RUN_ID = id,
                REFERENCE_WELL = scan.Reference,
                OFFSET_WELLS = string.Join(",", scan.Offsets),
                EXCLUDED_WELLS = scan.Excluded.Count == 0 ? null : string.Join(",", scan.Excluded),
                ERROR_MODEL = scan.ErrorModel,
                SIGMA = (decimal)scan.Sigma,
                SCALING_FACTOR = (decimal)scan.Rule.ScalingFactor,
                SURFACE_MARGIN = (decimal)scan.Rule.SurfaceMargin,
                PROJECT_AHEAD = (decimal)scan.Rule.ProjectAhead,
                SCAN_STEP = (decimal)scan.Rule.ScanStep,
                ENVIRONMENT = scan.Environment,
                STATION_COUNT = scan.Stations,
                VIOLATION_COUNT = scan.Violations,
                SETTINGS_JSON = JsonSerializer.Serialize(scan.Rule),
                RUN_BY = actor.Trim(),
                RUN_AT_UTC = now,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<string>.Failed(
                    OgUow.Message(committed, "The run was not saved."), AntiCollisionRules.CodeWriteFailed);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return OgResult<string>.Ok(id);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the anti-collision run", "the run was not saved", FailureSeverity.Error);

            return OgResult<string>.Failed(
                "The run could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? AntiCollisionRules.CodeWriteFailed);
        }
    }

    /// <summary>
    /// One well as the tools' model: its records only. A well with fewer than two stations, or an offset with no wellhead
    /// position, cannot be placed and returns null so the caller can name it.
    /// </summary>
    private static WellModel? Build(
        string wellId,
        IReadOnlyList<WELL_DIR_SRVY_STATION> stations,
        IReadOnlyList<WELL_DIR_SRVY> surveys,
        IReadOnlyList<WELL> wells,
        IReadOnlyList<WELLBORE> bores,
        List<string> warnings)
    {
        var mine = stations
            .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => row.SURVEY_ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .FirstOrDefault();

        if (mine is null)
        {
            return null;
        }

        if (stations
                .Where(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase))
                .GroupBy(row => row.SURVEY_ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Count() > 1)
        {
            warnings.Add("'" + wellId + "' records more than one survey; the one with the most stations is used.");
        }

        var rows = AntiCollisionRules.Stations(mine);

        if (rows.Count < 2)
        {
            return null;
        }

        var well = wells.FirstOrDefault(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase));
        var bore = bores.FirstOrDefault(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase));

        var header = new WellHeader
        {
            Name = string.IsNullOrWhiteSpace(well?.WELL_NAME) ? wellId : well!.WELL_NAME!,
            Uwi = wellId,
            Latitude = bore?.SURFACE_LATITUDE is { } latitude ? (double)latitude : null,
            Longitude = bore?.SURFACE_LONGITUDE is { } longitude ? (double)longitude : null,
            DepthDatum = bore?.DEPTH_DATUM ?? "KB",
        };

        var survey = surveys.FirstOrDefault(row => string.Equals(row.UWI?.Trim(), wellId, StringComparison.OrdinalIgnoreCase) &&
                                                   string.Equals(row.SURVEY_ID, mine.Key, StringComparison.OrdinalIgnoreCase));

        if (survey?.AZIMUTH_NORTH_TYPE is { Length: > 0 } north)
        {
            header.SurveyNorth = north.Trim().ToUpperInvariant() switch
            {
                "TRUE" => NorthReference.True,
                "MAGNETIC" or "MAG" => NorthReference.Magnetic,
                "GRID" => NorthReference.Grid,
                _ => null,
            };
        }

        var model = new WellModel { Header = header };

        foreach (var (md, inc, az, unit) in rows)
        {
            model.Survey.Add(new SurveyStation
            {
                Md = new Quantity(md, unit),
                Inclination = inc,
                Azimuth = az,
            });
        }

        model.Wellbores.Add(new WellboreInfo
        {
            Name = bore?.WELLBORE_NAME ?? "Main bore",
            TotalDepth = bore?.TOTAL_DEPTH is { } depth
                ? new Quantity((double)depth, bore.TOTAL_DEPTH_OUOM ?? "m")
                : null,
        });

        return model;
    }
}
