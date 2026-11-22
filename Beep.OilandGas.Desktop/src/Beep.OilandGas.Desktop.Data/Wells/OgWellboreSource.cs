using System.Globalization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;

/// <summary>One track of the drawn document (VSB-03).</summary>
public sealed record OgWellboreTrack(string Name, string Kind, int Entries)
{
    public string Describe() => Name + " (" + Kind + ", " + Entries.ToString(CultureInfo.InvariantCulture) + ")";
}

/// <summary>
/// A well drawn from its own records (VSB-03, AC-1): the document, the depth range it covers and what the records did not
/// provide.
/// </summary>
public sealed record OgWellboreView(
    string WellId,
    string WellName,
    float Top,
    float Bottom,
    string Unit,
    IReadOnlyList<OgWellboreTrack> Tracks,
    IReadOnlyList<string> Warnings,
    WellboreDocument Document)
{
    /// <summary>
    /// AC-1: a depth track plus at least one data track. The depth track alone is not enough - a view with only an axis cannot
    /// show anything about the well.
    /// </summary>
    public bool HasDepthAndDataTrack =>
        Tracks.Any(track => string.Equals(track.Kind, "depth", StringComparison.Ordinal)) &&
        Tracks.Any(track => !string.Equals(track.Kind, "depth", StringComparison.Ordinal));

    public string RangeText =>
        Top.ToString("0.#", CultureInfo.InvariantCulture) + " to " +
        Bottom.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit;

    public string TrackText => Tracks.Count == 0
        ? "no track"
        : string.Join(" · ", Tracks.Select(track => track.Describe()));

    public string WarningsText => Warnings.Count == 0
        ? "The records supplied every track shown."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    /// <summary>
    /// What is drawn at a depth: the interval recorded at that depth, or a sentence saying the depth carries nothing.
    /// </summary>
    public string ReadoutAt(float depth)
    {
        foreach (var track in Document.Tracks.OfType<IntervalTrack>())
        {
            foreach (var interval in track.Intervals)
            {
                if (depth >= Math.Min(interval.Top, interval.Bottom) && depth <= Math.Max(interval.Top, interval.Bottom))
                {
                    return depth.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " · " +
                           track.Name + " · " + interval.Text;
                }
            }
        }

        return depth.ToString("0.#", CultureInfo.InvariantCulture) + " " + Unit + " · nothing is recorded at this depth";
    }
}

/// <summary>The rules VSB-03 states once for a database well.</summary>
public static class WellboreSourceRules
{
    public const string CodeNoConnection = "wellbore-source-no-connection";
    public const string CodeReadFailed = "wellbore-source-read-failed";
    public const string CodeWellRequired = "wellbore-source-well-required";
    public const string CodeWellUnknown = "wellbore-source-well-unknown";
    public const string CodeNoDepthData = "wellbore-source-no-depth-data";

    /// <summary>The unit a recorded depth is in, from the owner's own UOM column.</summary>
    public static DepthUnit UnitOf(string? uom) =>
        (uom ?? string.Empty).Trim().ToUpperInvariant() is "F" or "FT" or "FOT" or "FEET"
            ? DepthUnit.Feet
            : DepthUnit.Meters;

    public static string Symbol(DepthUnit unit) => unit == DepthUnit.Feet ? "ft" : "m";

    /// <summary>
    /// AC-1's geometry: the axis covers the well from zero to its recorded total depth, or - when no total depth is recorded -
    /// from the shallowest to the deepest interval, which is stated as a warning rather than passed off as the well's depth.
    /// </summary>
    public static (float Top, float Bottom, string? Warning) AxisOf(
        decimal? totalDepth,
        IEnumerable<(decimal Top, decimal Bottom)> intervals)
    {
        var deepest = intervals.Select(interval => Math.Max(interval.Top, interval.Bottom)).DefaultIfEmpty(0m).Max();

        if (totalDepth is > 0m)
        {
            return (0f, (float)totalDepth.Value, null);
        }

        return deepest <= 0m
            ? (0f, 0f, "The well records no total depth and no interval, so there is no depth to draw.")
            : (0f, (float)deepest,
                "The wellbore records no total depth, so the depth axis ends at the deepest recorded interval (" +
                deepest.ToString("0.#", CultureInfo.InvariantCulture) + ") rather than at the well's depth.");
    }

    /// <summary>
    /// The document for a well: a depth track, an interval track from its recorded completions, and the warning that no curve
    /// values exist to plot.
    /// </summary>
    public static (WellboreDocument Document, IReadOnlyList<string> Warnings, IReadOnlyList<OgWellboreTrack> Tracks) Build(
        string uwi,
        string name,
        decimal? totalDepth,
        string? depthUom,
        IReadOnlyList<(decimal Top, decimal Bottom, string? Type, string? Uom)> intervals)
    {
        var unit = UnitOf(depthUom ?? intervals.Select(interval => interval.Uom).FirstOrDefault(uom => uom is not null));
        var document = new WellboreDocument { Name = name, WellName = name };
        var warnings = new List<string>();
        var tracks = new List<OgWellboreTrack>();

        var (top, bottom, axisWarning) = AxisOf(totalDepth, intervals.Select(interval => (interval.Top, interval.Bottom)));

        if (axisWarning is not null)
        {
            warnings.Add(axisWarning);
        }

        document.Axis.Top = top;
        document.Axis.Bottom = bottom;
        document.Axis.Unit = unit;

        document.Tracks.Add(new DepthTrack { WidthRatio = 0.6f, ShowGrid = true });
        tracks.Add(new OgWellboreTrack("Depth", "depth", 0));

        var completions = new IntervalTrack { Name = "Completion", WidthRatio = 1f };

        foreach (var interval in intervals)
        {
            if (interval.Bottom <= interval.Top)
            {
                warnings.Add("A completion interval with top " +
                             interval.Top.ToString("0.#", CultureInfo.InvariantCulture) + " and base " +
                             interval.Bottom.ToString("0.#", CultureInfo.InvariantCulture) +
                             " has no thickness and is not drawn.");
                continue;
            }

            if (UnitOf(interval.Uom) != unit && !string.IsNullOrWhiteSpace(interval.Uom))
            {
                warnings.Add("A completion interval is recorded in '" + interval.Uom +
                             "', which differs from the well's '" + Symbol(unit) +
                             "'; it is drawn with the well's unit as recorded.");
            }

            completions.Intervals.Add(new DepthInterval
            {
                Top = (float)interval.Top,
                Bottom = (float)interval.Bottom,
                Text = string.IsNullOrWhiteSpace(interval.Type) ? "completion" : interval.Type!.Trim(),
            });
        }

        if (completions.Intervals.Count > 0)
        {
            document.Tracks.Add(completions);
            tracks.Add(new OgWellboreTrack("Completion", "intervals", completions.Intervals.Count));
        }
        else
        {
            warnings.Add("No completion interval is recorded for " + uwi +
                         ", so the view has a depth track and no data track.");
        }

        // The honest gap: this schema records log headers and index coordinates, not measured curve values.
        warnings.Add(
            "The recorded log tables hold log headers and index coordinates but no measured curve values, so no curve track " +
            "is drawn; a curve needs a value store the owner does not have.");

        return (document, warnings, tracks);
    }
}

/// <summary>VSB-03: a well from the database as a wellbore document.</summary>
public interface IOgWellboreSource
{
    Task<OgResult<OgWellboreView>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// VSB-03 over the owner's <c>WELL</c>, <c>WELLBORE</c> and <c>WELL_COMPLETION</c> records.
/// </summary>
/// <remarks>
/// <para>
/// AC-1 asks for a real well from the database with a depth track and at least one data track. The well's identity and total
/// depth come from <c>WELL</c>/<c>WELLBORE</c>, and its recorded <b>completion intervals</b> are the depth-indexed data that
/// renders as an interval track with a real range, a real readout and real text - so the view is drawn from records rather
/// than from the tools' demo.
/// </para>
/// <para>
/// The one thing this schema cannot supply is a curve track: <c>WELL_LOG</c>, <c>WELL_LOG_CURVE</c> and
/// <c>WELL_LOG_AXIS_COORD</c> record headers and index coordinates but no measured values. That is stated as a warning on the
/// view instead of being hidden behind a demo curve, which is why VSB-03 was recorded as blocked until now.
/// </para>
/// </remarks>
public sealed class OgWellboreSource : IOgWellboreSource
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWellboreSource(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWellboreView>> LoadAsync(
        string wellId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
        {
            return OgResult<OgWellboreView>.Failed(
                "A well is drawn by its identity; none is named.", WellboreSourceRules.CodeWellRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWellboreView>.Failed(
                "No connection is open, so the well cannot be drawn.", WellboreSourceRules.CodeNoConnection);
        }

        try
        {
            var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);
            var bores = await OgUow.ReadAllAsync<WELLBORE>(source).ConfigureAwait(false);
            var completions = await OgUow.ReadAllAsync<WELL_COMPLETION>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var key = wellId.Trim();

            var well = wells.FirstOrDefault(row => string.Equals(row.UWI?.Trim(), key, StringComparison.OrdinalIgnoreCase));

            var bore = bores.FirstOrDefault(row => string.Equals(row.UWI?.Trim(), key, StringComparison.OrdinalIgnoreCase));

            if (well is null && bore is null)
            {
                return OgResult<OgWellboreView>.Failed(
                    "Well '" + key + "' is not recorded.", WellboreSourceRules.CodeWellUnknown);
            }

            var name = well?.WELL_NAME ?? bore?.WELLBORE_NAME ?? key;

            var intervals = completions
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.UWI?.Trim(), key, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.TOP_DEPTH)
                .Select(row => (Top: row.TOP_DEPTH, Bottom: row.BASE_DEPTH, Type: row.COMPLETION_TYPE,
                    Uom: row.TOP_DEPTH_OUOM))
                .ToList();

            var (document, warnings, tracks) = WellboreSourceRules.Build(
                key,
                name,
                bore?.TOTAL_DEPTH ?? well?.BASE_DEPTH,
                bore?.TOTAL_DEPTH_OUOM ?? well?.BASE_DEPTH_OUOM,
                intervals);

            if (!tracks.Any(track => !string.Equals(track.Kind, "depth", StringComparison.Ordinal)))
            {
                return OgResult<OgWellboreView>.Failed(
                    "No depth-indexed data is recorded for " + key +
                    ", so there is nothing to draw beside the depth axis.",
                    WellboreSourceRules.CodeNoDepthData);
            }

            return OgResult<OgWellboreView>.Ok(new OgWellboreView(
                key,
                name,
                document.Axis.Top,
                document.Axis.Bottom,
                WellboreSourceRules.Symbol(document.Axis.Unit),
                tracks,
                warnings,
                document));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "drawing well " + wellId, "the well is not drawn", FailureSeverity.Error);

            return OgResult<OgWellboreView>.Failed(
                "The well could not be drawn" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WellboreSourceRules.CodeReadFailed);
        }
    }
}
