using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>The built-in track sets the well view can show.</summary>
public enum OgTrackTemplate
{
    /// <summary>The tools' triple-combo demo log (GR / resistivity / porosity).</summary>
    TripleComboDemo,

    /// <summary>Tracks built from the canonical model's first depth log set.</summary>
    ModelLogSet,

    /// <summary>A bare depth track — the contrast case for the selector.</summary>
    DepthOnly,
}

/// <summary>What a page needs to know about the document it is showing.</summary>
public sealed record OgDocumentSummary(    string Name,
    string WellName,
    int TrackCount,
    int MarkerCount,
    float Top,
    float Bottom,
    string DepthUnit,
    string? Source);

/// <summary>
/// Turns well data into a <see cref="WellboreDocument"/> the host can draw.
/// </summary>
/// <remarks>
/// <para>
/// The canonical model is <see cref="WellModel"/>: the loaders produce it, and PPDM will map into it
/// (DAT-02/VSB-03). This class therefore has two entry points — one from a real model and one demo
/// document — so the rendering path can be proven before the PPDM schema is available.
/// </para>
/// <para>
/// Nothing here invents well data for a real well: a model with no depth log set yields a document with
/// no tracks, and the page says so rather than drawing an empty plot as if it were the well.
/// </para>
/// </remarks>
public static class OgDocumentMapper
{
    /// <summary>
    /// The tools' own triple-combo demo document. Used to prove the host end to end while PPDM module
    /// setup is blocked; it is never presented as a real well.
    /// </summary>
    public static WellboreDocument TripleComboDemo() => WellLogTemplates.CreateTripleComboDemo();

    /// <summary>The document shapes the page offers.</summary>
    public static IReadOnlyList<OgTrackTemplate> Templates { get; } = Enum.GetValues<OgTrackTemplate>();

    /// <summary>Builds the document for a chosen track set.</summary>
    public static WellboreDocument Create(OgTrackTemplate template) => template switch
    {
        OgTrackTemplate.TripleComboDemo => TripleComboDemo(),
        OgTrackTemplate.ModelLogSet => FromWellModel(BuildDemoWell()),
        OgTrackTemplate.DepthOnly => DepthOnlyDocument(),
    };

    /// <summary>A bare depth track with an axis: the smallest useful document, and a deliberate contrast
    /// to the populated ones so the template choice is visible.</summary>
    public static WellboreDocument DepthOnlyDocument()
    {
        var document = new WellboreDocument { Name = "Depth only", WellName = "DEMO-1" };
        document.Axis.Top = 2000f;
        document.Axis.Bottom = 2400f;
        document.Axis.Unit = DepthUnit.Meters;
        document.Tracks.Add(new DepthTrack { WidthRatio = 0.6f });
        return document;
    }

    /// <summary>
    /// A document from the canonical model. The model's first depth log set supplies the axis and the
    /// curves; formation tops become markers. A model without a depth log set produces a document with
    /// no tracks, which the caller must report as "no depth data" rather than as an empty well.
    /// </summary>
    public static WellboreDocument FromWellModel(WellModel well, string? standard = null)
    {
        ArgumentNullException.ThrowIfNull(well);
        return WellLogDocuments.FromModel(well, standard);
    }

    /// <summary>
    /// A small, deterministic <see cref="WellModel"/> with one depth log set (GR and RHOB) and two tops.
    /// This is the shape a PPDM read will produce, so mapping it now proves the path.
    /// </summary>
    public static WellModel BuildDemoWell(string wellName = "DEMO-1")
    {
        var model = new WellModel();
        model.Header.Name = wellName;
        model.Header.Uwi = "DEMO-1-UWI";

        var logSet = new LogSet
        {
            Name = "Demo triple combo",
            IndexType = LogIndexType.Depth,
            IndexMnemonic = "DEPT",
            IndexUnit = "m",
        };

        const int samples = 401;
        var index = new double[samples];
        var gamma = new double?[samples];
        var density = new double?[samples];

        for (var i = 0; i < samples; i++)
        {
            var depth = 2000d + i;
            index[i] = depth;

            // A deterministic, recognisable log shape: a sand/shale alternation with a density track.
            gamma[i] = 45 + (30 * Math.Sin(i / 12d)) + (i % 40 < 20 ? 18 : -6);
            density[i] = 2.35 + (0.12 * Math.Cos(i / 18d));
        }

        logSet.Index = index;
        logSet.Curves.Add(new LogCurveData { Mnemonic = "GR", Unit = "API", Description = "Gamma ray", Values = gamma });
        logSet.Curves.Add(new LogCurveData { Mnemonic = "RHOB", Unit = "g/cm3", Description = "Bulk density", Values = density });

        model.LogSets.Add(logSet);
        return model;
    }

    /// <summary>
    /// What the mapper could not do. A document with no tracks or no depth range is still returned so the
    /// page can show it with a notice, rather than refusing to open.
    /// </summary>
    public static IReadOnlyList<string> Warnings(WellboreDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var warnings = new List<string>();

        if (document.Tracks.Count == 0)
        {
            warnings.Add("No depth-indexed data was found; the view has no tracks.");
        }

        if (document.Axis.Bottom <= document.Axis.Top)
        {
            warnings.Add("The document has no depth range.");
        }

        return warnings;
    }

    /// <summary>The facts a page or the smoke run reports about a document.</summary>
    public static OgDocumentSummary Summarise(WellboreDocument document)    {
        ArgumentNullException.ThrowIfNull(document);

        return new OgDocumentSummary(
            document.Name,
            document.WellName,
            document.Tracks.Count,
            document.Markers.Count,
            document.Axis.Top,
            document.Axis.Bottom,
            DepthUnits.Symbol(document.Axis.Unit),
            document.Properties.TryGetValue("source", out var source) ? source : null);
    }
}
