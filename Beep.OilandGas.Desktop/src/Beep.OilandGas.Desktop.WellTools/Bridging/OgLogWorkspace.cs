using System.IO;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One row of the curve list.</summary>
public sealed record OgLogCurveRow(
    string Mnemonic,
    string Unit,
    string Description,
    float Minimum,
    float Maximum,
    string Scale,
    int Samples,
    bool UnitKnown)
{
    /// <summary>VSB-04 §5: a curve without a unit is plotted but flagged.</summary>
    public string UnitLabel => UnitKnown ? Unit : "unit unknown";
}

/// <summary>What is on screen for a well log, and what the page needs to say about it.</summary>
public sealed record OgLogSummary(
    string Name,
    string? Standard,
    int TrackCount,
    int CurveCount,
    float Top,
    float Bottom,
    string DepthUnit,
    string ImageData,
    string CementData);

/// <summary>Which edit a history entry records.</summary>
public enum OgLogEditKind
{
    Shift,
    Splice,

    /// <summary>A component's symbol changed (VSB-08).</summary>
    Symbol,
}

/// <summary>
/// One recorded edit. A splice records both intervals, because an audit entry that says only "spliced" cannot
/// be checked against the data afterwards.
/// </summary>
public sealed record OgLogEdit(
    OgLogEditKind Kind,
    string Description,
    string? SourceInterval,
    string? TargetInterval)
{
    public string Detail => TargetInterval is null
        ? $"{Kind}: {SourceInterval}"
        : $"{Kind}: source {SourceInterval} → target {TargetInterval}";
}

/// <summary>
/// The edits made on this screen, newest last, each with the action that reverses it.
/// </summary>
/// <remarks>
/// The page surfaces undo rather than hiding it (VSB-04 §4): an edit to a curve changes what a person reads,
/// so reversing it must be one action and must leave a trace of what was reversed.
/// </remarks>
public sealed class OgLogEditHistory
{
    private readonly List<(OgLogEdit Edit, Action Undo)> _entries = [];

    public IReadOnlyList<OgLogEdit> Entries => _entries.Select(entry => entry.Edit).ToList();

    public bool CanUndo => _entries.Count > 0;

    /// <summary>The last edit that was undone, for the page to state.</summary>
    public OgLogEdit? LastUndone { get; private set; }

    public void Record(OgLogEdit edit, Action undo)
    {
        ArgumentNullException.ThrowIfNull(edit);
        ArgumentNullException.ThrowIfNull(undo);

        _entries.Add((edit, undo));
    }

    public bool Undo()
    {
        if (_entries.Count == 0)
        {
            return false;
        }

        var last = _entries[^1];
        _entries.RemoveAt(_entries.Count - 1);
        last.Undo();
        LastUndone = last.Edit;

        return true;
    }
}

/// <summary>
/// Curve editing: depth shift and splice through the tools' own <see cref="CurveEditing"/>, with an undo
/// history and a refusal when a shift would move samples outside the log's range.
/// </summary>
public static class OgLogEditing
{
    public const string CodeZeroShift = "edit-shift-zero";
    public const string CodeOutsideRange = "edit-shift-outside-range";
    public const string CodeSpliceOutside = "edit-splice-outside-range";

    /// <summary>
    /// Shifts a curve's depths. Refused when the shift is zero or would move a sample outside the axis range,
    /// because either makes the log show something the data does not say.
    /// </summary>
    public static OgResult<OgLogEdit> Shift(
        WellLogCurve curve,
        float delta,
        string unit,
        float axisTop,
        float axisBottom,
        OgLogEditHistory history,
        string reason = "depth shift")
    {
        ArgumentNullException.ThrowIfNull(curve);
        ArgumentNullException.ThrowIfNull(history);

        if (delta == 0f)
        {
            return OgResult<OgLogEdit>.Failed(
                "A depth shift of zero changes nothing.",
                CodeZeroShift);
        }

        var low = Math.Min(axisTop, axisBottom);
        var high = Math.Max(axisTop, axisBottom);

        foreach (var sample in curve.Samples)
        {
            var moved = sample.Depth + delta;
            if (moved < low || moved > high)
            {
                return OgResult<OgLogEdit>.Failed(
                    $"The shift would move a sample to {moved:0.###}, outside the log range {low:0.###}–{high:0.###} {unit}.",
                    CodeOutsideRange);
            }
        }

        var before = curve.Samples.Select(sample => sample.Depth).ToArray();

        CurveEditing.Shift(curve, CurveDepthShift.Bulk(curve.Mnemonic, new Quantity(delta, unit), reason), unit);

        var summary = $"{curve.Mnemonic} shifted {delta:+0.###;-0.###;0} {unit}";
        var edit = new OgLogEdit(
            OgLogEditKind.Shift,
            summary,
            $"{curve.Mnemonic} {before.Min():0.###}–{before.Max():0.###} {unit}",
            $"{curve.Mnemonic} {curve.Samples.Min(s => s.Depth):0.###}–{curve.Samples.Max(s => s.Depth):0.###} {unit}");

        history.Record(edit, () =>
        {
            for (var i = 0; i < before.Length && i < curve.Samples.Count; i++)
            {
                curve.Samples[i].Depth = before[i];
            }

            curve.Samples.Sort((a, b) => a.Depth.CompareTo(b.Depth));
        });

        return OgResult<OgLogEdit>.Ok(edit);
    }

    /// <summary>
    /// Splices <paramref name="lower"/> into <paramref name="upper"/> at a depth, replacing both in the list
    /// with the result. Undo restores the two original curves in their places.
    /// </summary>
    public static OgResult<OgLogEdit> Splice(
        IList<WellLogCurve> curves,
        int upperIndex,
        int lowerIndex,
        float spliceDepth,
        string? mnemonic,
        OgLogEditHistory history)
    {
        ArgumentNullException.ThrowIfNull(curves);
        ArgumentNullException.ThrowIfNull(history);

        if (upperIndex < 0 || lowerIndex < 0 || upperIndex >= curves.Count || lowerIndex >= curves.Count)
        {
            return OgResult<OgLogEdit>.Failed(
                "The curves to splice are not in the list.",
                CodeSpliceOutside);
        }

        var upper = curves[upperIndex];
        var lower = curves[lowerIndex];

        if (spliceDepth <= upper.Samples.Min(s => s.Depth) || spliceDepth >= lower.Samples.Max(s => s.Depth))
        {
            return OgResult<OgLogEdit>.Failed(
                $"The splice depth {spliceDepth:0.###} is outside the two curves' range.",
                CodeSpliceOutside);
        }

        var spliced = CurveEditing.Splice(upper, lower, spliceDepth, mnemonic);

        var source = $"{upper.Mnemonic} {upper.Samples.Min(s => s.Depth):0.###}–{spliceDepth:0.###} {upper.Unit}";
        var target = $"{lower.Mnemonic} {spliceDepth:0.###}–{lower.Samples.Max(s => s.Depth):0.###} {lower.Unit}";

        var edit = new OgLogEdit(OgLogEditKind.Splice, $"{spliced.Mnemonic} spliced at {spliceDepth:0.###}", source, target);

        // The higher index first, so removing it cannot move the lower one.
        var first = Math.Min(upperIndex, lowerIndex);
        var second = Math.Max(upperIndex, lowerIndex);
        var originalFirst = curves[first];
        var originalSecond = curves[second];

        curves.RemoveAt(second);
        curves.RemoveAt(first);
        curves.Insert(first, spliced);

        history.Record(edit, () =>
        {
            curves.Remove(spliced);
            curves.Insert(first, originalFirst);
            curves.Insert(second, originalSecond);
        });

        return OgResult<OgLogEdit>.Ok(edit);
    }
}

/// <summary>
/// Crossplots, produced by the tools' <see cref="Crossplots"/> rather than re-implemented (VSB-04 AC-5).
/// </summary>
public static class OgLogCrossplots
{
    public static (CrossplotPanel Panel, IReadOnlyList<string> Warnings) DensityNeutron(LogSet set)
    {
        var warnings = new List<string>();
        var panel = Crossplots.DensityNeutron(set, warnings: warnings);
        return (panel, warnings);
    }

    public static (CrossplotPanel Panel, IReadOnlyList<string> Warnings) Pickett(LogSet set)
    {
        var warnings = new List<string>();
        var panel = Crossplots.Pickett(set, warnings: warnings);
        return (panel, warnings);
    }
}

/// <summary>Which optional log displays have data, and the sentence shown when they have none.</summary>
public static class OgLogAvailability
{
    public const string NoImageData = "No image log data for this wellbore.";
    public const string NoCementData = "No cement bond data for this wellbore.";

    public static string Image(WellboreDocument document) =>
        document.Tracks.OfType<ImageLogTrack>().Any(track => track.Image.Rows.Count > 0)
            ? "Image log track present."
            : NoImageData;

    /// <summary>
    /// Cement is present when the cement presentation produced tracks: the CBL curve, the derived bond index,
    /// or the bond flag track. Looking for a track *named* "cement" would miss all three.
    /// </summary>
    public static string Cement(WellboreDocument document)
    {
        var hasCbl = document.Tracks.OfType<WellLogTrack>()
            .SelectMany(track => track.Curves)
            .Any(curve => curve.Mnemonic is "CBL" or "BI" or "BOND");

        var hasFlags = document.Tracks.OfType<FlagTrack>().Any(track =>
            track.Name.Contains("bond", StringComparison.OrdinalIgnoreCase));

        return hasCbl || hasFlags ? "Cement bond track present." : NoCementData;
    }

    /// <summary>The number of cement presentation tracks (CBL, bond index, flags, VDL) in the document.</summary>
    public static int CementTrackCount(WellboreDocument document) =>
        document.Tracks.OfType<WellLogTrack>().Count(track =>
            track.Curves.Any(curve => curve.Mnemonic is "CBL" or "BI" or "BOND"))
        + document.Tracks.OfType<FlagTrack>().Count(track =>
            track.Name.Contains("bond", StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// A demo log set with the four curves a triple combo and both crossplots need: GR, resistivity, density and
/// neutron. Deterministic, so a test can assert counts rather than "something happened".
/// </summary>
public static class OgLogDemo
{
    /// <summary>
    /// The same demo log plus an image log and a cement bond presentation, so the two "no data" states can be
    /// contrasted with real tracks (VSB-04 AC-6). Deterministic values, no files.
    /// </summary>
    public static WellboreDocument WithImageAndCement(float top = 8200f, float bottom = 8300f)
    {
        var document = WellLogTemplates.CreateTripleComboDemo(top, bottom, DepthUnit.Feet);

        // Image log: 8 azimuthal columns over the interval, a deterministic pattern.
        var image = new ImageLogTrack { Name = "Image" };
        var rows = 60;

        for (var row = 0; row < rows; row++)
        {
            var depth = top + ((bottom - top) * row / (rows - 1f));
            var pixels = new float[8];

            for (var column = 0; column < pixels.Length; column++)
            {
                pixels[column] = (float)((Math.Sin((row / 6.0) + (column * 0.7)) + 1.0) / 2.0);
            }

            image.Image.Depths.Add(depth);
            image.Image.Rows.Add(pixels);
        }

        document.Tracks.Add(image);

        // Cement bond: a CBL curve that improves down-hole, through the tools' own presentation.
        var cbl = new WellLogCurve { Mnemonic = "CBL", Unit = "mV", Name = "CBL amplitude" };
        var samples = 120;

        for (var i = 0; i < samples; i++)
        {
            var depth = top + ((bottom - top) * i / (samples - 1f));
            cbl.Samples.Add(new WellLogCurveSample
            {
                Depth = depth,
                Value = (float)(90 - (70 * (i / (double)samples))),
            });
        }

        cbl.MinimumValue = 0;
        cbl.MaximumValue = 100;

        var calibration = new CblCalibration();

        foreach (var track in CementBondTracks.Build(cbl, vdl: null, calibration))
        {
            document.Tracks.Add(track);
        }

        return document;
    }

    public static WellModel BuildWell(string wellName = "LOG-DEMO-1", int samples = 401)
    {
        var model = new WellModel();
        model.Header.Name = wellName;
        model.Header.Uwi = wellName + "-UWI";

        var set = new LogSet
        {
            Name = "Demo triple combo",
            IndexType = LogIndexType.Depth,
            IndexMnemonic = "DEPT",
            IndexUnit = "m",
        };

        var index = new double[samples];
        var gr = new double?[samples];
        var rt = new double?[samples];
        var rhob = new double?[samples];
        var nphi = new double?[samples];

        for (var i = 0; i < samples; i++)
        {
            var depth = 2000d + i;
            index[i] = depth;

            // A sand/shale alternation with a hydrocarbon leg: the numbers are made up but stable, so the
            // crossplots have a real cloud to draw and the same cloud every run.
            gr[i] = 45 + (30 * Math.Sin(i / 12d)) + (i % 40 < 20 ? 18 : -6);
            rt[i] = 2 + (18 * Math.Pow(Math.Sin(i / 30d) + 1.2, 2));
            rhob[i] = 2.35 + (0.12 * Math.Cos(i / 18d));
            nphi[i] = 0.22 + (0.08 * Math.Sin(i / 22d));
        }

        set.Index = index;
        set.Curves.Add(new LogCurveData { Mnemonic = "GR", Unit = "API", Description = "Gamma ray", Values = gr });
        set.Curves.Add(new LogCurveData { Mnemonic = "RT", Unit = "ohm.m", Description = "Deep resistivity", Values = rt });
        set.Curves.Add(new LogCurveData { Mnemonic = "RHOB", Unit = "g/cm3", Description = "Bulk density", Values = rhob });
        set.Curves.Add(new LogCurveData { Mnemonic = "NPHI", Unit = "v/v", Description = "Neutron porosity", Values = nphi });

        model.LogSets.Add(set);
        return model;
    }
}

/// <summary>The lithology standards a log can be drawn against.</summary>
public static class OgLogStandards
{
    public const string Fgdc = "FGDC";
    public const string Gost = "GOST 21.302-2013";
    public const string SampleCompany = "Sample company standard (demo)";

    public const string GostNotice =
        "GOST 21.302-2013 supplies its numbers and names; the shipped file carries no drawings (they are added as " +
        "overrides), so the built-in patterns remain.";

    /// <summary>The lithology standards this build offers. FGDC is the built-in library; the other two are layers.</summary>
    public static IReadOnlyList<string> Available { get; } = [Fgdc, Gost, SampleCompany];

    /// <summary>The standard in force, as the environment states it.</summary>
    public static string Active
    {
        get
        {
            var layers = WellboreEnvironment.Current.Resources.LithologyStandards;

            return layers.Count == 0
                ? Fgdc
                : string.Join(", ", layers.Select(layer => Path.GetFileName(layer)));
        }
    }

    /// <summary>
    /// Resolves a requested standard. An unknown name falls back to FGDC with a notice rather than leaving the
    /// log drawn against nothing.
    /// </summary>
    public static (string? Standard, string? Notice) Resolve(string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested)
            || requested.Equals(Fgdc, StringComparison.OrdinalIgnoreCase))
        {
            return (Fgdc, null);
        }

        if (requested.Equals(Gost, StringComparison.OrdinalIgnoreCase)
            || requested.Equals("GOST 21.302", StringComparison.OrdinalIgnoreCase))
        {
            return (Gost, GostNotice);
        }

        if (requested.Equals(SampleCompany, StringComparison.OrdinalIgnoreCase)
            || requested.Equals("Sample company", StringComparison.OrdinalIgnoreCase))
        {
            return (SampleCompany, null);
        }

        return (Fgdc, $"'{requested}' is not a standard this build offers; FGDC is used.");
    }

    /// <summary>
    /// Puts a standard in force by setting `resources.lithologyStandards`. The library is rebuilt on the next
    /// draw, so fills change without reloading the document.
    /// </summary>
    public static void Apply(string? requested, string? folder = null)
    {
        var (standard, _) = Resolve(requested);
        var layers = WellboreEnvironment.Current.Resources.LithologyStandards;

        layers.Clear();

        if (standard == Gost)
        {
            layers.Add(Gost);
        }
        else if (standard == SampleCompany)
        {
            layers.Add(SampleStandard(folder));
        }
    }

    /// <summary>The drawing the active standard gives a lithology code, for the page and its tests.</summary>
    public static string Drawing(string codeOrAlias)
    {
        var definition = LithologyLibrary.Current.Resolve(codeOrAlias);
        return $"{definition.Pattern} · {definition.Background} · {definition.Foreground}";
    }

    /// <summary>
    /// A company standard sample: it overrides four common lithologies so the fill change is visible. Clearly a
    /// demo - a real company supplies its own file (entries, overrides and .pat hatches).
    /// </summary>
    private const string SampleJson = """
                                      {
                                        "name": "Sample company standard (demo)",
                                        "aliasesOfName": [ "Sample company" ],
                                        "source": "Demo only: shows how a company's lithology drawings override the built-in library. Not a published standard.",
                                        "entries": [],
                                        "overrides": [
                                          { "code": "sand", "name": "Sand", "pattern": "Bricks", "background": "#e8f0ff", "foreground": "#204080", "spacing": 6, "size": 1.2 },
                                          { "code": "shale", "name": "Shale", "pattern": "Dots", "background": "#fdf0e0", "foreground": "#804020", "spacing": 8 },
                                          { "code": "limestone", "name": "Limestone", "pattern": "Dashes", "background": "#eafbe7", "foreground": "#206030", "spacing": 7 },
                                          { "code": "sandstone", "name": "Sandstone", "pattern": "CrossHatch", "background": "#f2e8ff", "foreground": "#403070", "spacing": 6 }
                                        ]
                                      }
                                      """;

    private static string SampleStandard(string? folder)
    {
        var directory = string.IsNullOrWhiteSpace(folder)
            ? Path.Combine(Path.GetTempPath(), "TheTechIdea", "BeepOilGas", "Standards")
            : folder;

        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "sample-company.lithology.json");
        File.WriteAllText(path, SampleJson);
        return path;
    }
}
