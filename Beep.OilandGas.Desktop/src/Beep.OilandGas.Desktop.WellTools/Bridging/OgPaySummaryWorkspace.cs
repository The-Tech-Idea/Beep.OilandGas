using System.IO;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>The cutoffs a pay run uses. All three must be set: "pay" without a cutoff means nothing (ENG-10 §5).</summary>
public sealed record OgPayCutoffs(double? VshMax, double? PorosityMin, double? SwMax)
{
    public IReadOnlyList<string> Missing =>
        [
            .. VshMax is null ? new[] { "Vsh cutoff" } : [],
            .. PorosityMin is null ? new[] { "porosity cutoff" } : [],
            .. SwMax is null ? new[] { "Sw cutoff" } : [],
        ];

    public override string ToString() =>
        $"Vsh <= {VshMax:0.##}, PHI >= {PorosityMin:0.##}, Sw <= {SwMax:0.##}";
}

/// <summary>The petrophysical parameters of a run, with the tools' defaults for everything not shown on the page.</summary>
public sealed record OgPayParameters(
    OgPayCutoffs Cutoffs,
    string VshMethod = "Linear",
    double GrClean = 20,
    double GrShale = 120,
    double ArchieA = 1.0,
    double ArchieM = 2.0,
    double ArchieN = 2.0,
    double Rw = 0.05,
    double MatrixDensity = 2.65,
    double FluidDensity = 1.0);

/// <summary>One zone the person edits: a name and a top, with the base of the zone above it.</summary>
public sealed class OgPayZoneInput
{
    public OgPayZoneInput(string name, double top, double baseDepth)
    {
        Name = name;
        Top = top;
        Base = baseDepth;
    }

    public string Name { get; set; }

    public double Top { get; set; }

    public double Base { get; set; }
}

/// <summary>One computed zone, as the result grid and the CSV show it.</summary>
public sealed record OgPayZoneRow(
    string Name,
    double Top,
    double Base,
    double Gross,
    double Logged,
    double NetReservoir,
    double NetPay,
    double NetToGross,
    double? Porosity,
    double? Sw,
    double? Vsh,
    double HydrocarbonColumn,
    double? GrossTvd,
    double? NetPayTvd,
    string Cutoffs,
    bool Excluded,
    string? Note);

/// <summary>The result of one run: the zones, the method warnings, and how many log edits preceded it.</summary>
public sealed record OgPaySummaryResult(
    string DepthUnit,
    string LogSetName,
    IReadOnlyList<OgPayZoneRow> Zones,
    IReadOnlyList<string> Warnings,
    OgPayCutoffs Cutoffs)
{
    public double Gross => Zones.Sum(zone => zone.Gross);

    public double NetReservoir => Zones.Sum(zone => zone.NetReservoir);

    public double NetPay => Zones.Sum(zone => zone.NetPay);

    public double NetToGross => Gross > 0 ? NetReservoir / Gross : 0;

    public string Describe() =>
        $"{Zones.Count} zone(s) · gross {Gross:0.##} · net {NetReservoir:0.##} · net pay {NetPay:0.##} · " +
        $"N/G {NetToGross:0.###} · {DepthUnit}";
}

/// <summary>
/// ENG-10: pay summary from logs, with the zone checks the tools leave to the caller.
/// </summary>
/// <remarks>
/// <para>
/// The arithmetic is the tools' <see cref="PaySummary.Compute"/>; this class adds what a desktop needs: cutoffs
/// must be stated, zones must not overlap, a missing input curve excludes the zone and <b>names</b> the curve,
/// and the run reports the method warnings it relied on.
/// </para>
/// <para>
/// Zones are contiguous: each zone ends where the next begins, and the last ends at the log's bottom. That is
/// how <see cref="PaySummary"/> derives zone boundaries from formation tops, and the check makes the constraint
/// explicit instead of silently producing a different gross than the editor shows.
/// </para>
/// </remarks>
public static class OgPaySummaryWorkspace
{
    public const string CodeNoZones = "pay-no-zones";
    public const string CodeOverlap = "pay-zones-overlap";
    public const string CodeCutoffs = "pay-cutoffs-missing";
    public const string CodeGap = "pay-zones-not-contiguous";
    public const string CodeNoLog = "pay-no-log-set";

    private const double Tolerance = 1e-6;

    /// <summary>
    /// The reference well the tools' own test uses: ten 1 m samples with VSH, PHIT and SW curves, and two
    /// formations A (1000–1005) and B (1005–1009). Kept identical so the desktop's numbers can be compared
    /// with the library's.
    /// </summary>
    public static WellModel ReferenceWell()
    {
        var model = new WellModel();
        model.Header.Name = "PAY-REFERENCE";

        model.Tops.Add(new FormationTop { Name = "A", Depth = new Quantity(1000, "m") });
        model.Tops.Add(new FormationTop { Name = "B", Depth = new Quantity(1005, "m") });

        var set = new LogSet
        {
            Name = "main",
            IndexUnit = "m",
            Index = Enumerable.Range(0, 10).Select(i => 1000.0 + i).ToArray(),
        };

        double?[] vsh = [0.20, 0.50, 0.20, 0.10, 0.30, 0.20, 0.60, 0.60, 0.60, 0.60];
        double?[] phi = [0.25, 0.20, 0.05, 0.20, 0.15, 0.20, 0.10, 0.10, 0.10, 0.10];
        double?[] sw = [0.30, 0.30, 0.30, 0.80, 0.40, 0.50, 0.90, 0.90, 0.90, 0.90];

        set.Curves.Add(new LogCurveData { Mnemonic = "VSH", Unit = "v/v", Description = "Shale volume", Values = vsh });
        set.Curves.Add(new LogCurveData { Mnemonic = "PHIT", Unit = "v/v", Description = "Total porosity", Values = phi });
        set.Curves.Add(new LogCurveData { Mnemonic = "SW", Unit = "v/v", Description = "Water saturation", Values = sw });

        model.LogSets.Add(set);
        return model;
    }

    /// <summary>The reference well's zones, as the editor starts them.</summary>
    public static IReadOnlyList<OgPayZoneInput> ReferenceZones() =>
    [
        new("A", 1000, 1005),
        new("B", 1005, 1009),
    ];

    /// <summary>The cutoffs the reference run uses.</summary>
    public static OgPayParameters ReferenceParameters() =>
        new(new OgPayCutoffs(VshMax: 0.4, PorosityMin: 0.1, SwMax: 0.6));

    /// <summary>
    /// Computes the pay summary. Returns a refusal with a named code for the states §5 requires to be refused.
    /// </summary>
    public static OgResult<OgPaySummaryResult> Compute(
        WellModel well,
        IReadOnlyList<OgPayZoneInput> zones,
        OgPayParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(well);
        ArgumentNullException.ThrowIfNull(zones);
        ArgumentNullException.ThrowIfNull(parameters);

        var set = well.LogSets.FirstOrDefault(candidate =>
            candidate.IndexType == LogIndexType.Depth && candidate.Index.Length > 1);

        if (set is null)
        {
            return OgResult<OgPaySummaryResult>.Failed(
                "The well has no depth-indexed log set, so there is nothing to compute pay over.",
                CodeNoLog);
        }

        if (zones.Count == 0)
        {
            return OgResult<OgPaySummaryResult>.Failed("No zones were defined.", CodeNoZones);
        }

        var missing = parameters.Cutoffs.Missing;

        if (missing.Count > 0)
        {
            return OgResult<OgPaySummaryResult>.Failed(
                $"{string.Join(" and ", missing)} must be set: a net sand without a cutoff is not a pay definition.",
                CodeCutoffs);
        }

        var ordered = zones.OrderBy(zone => zone.Top).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            var zone = ordered[i];

            if (string.IsNullOrWhiteSpace(zone.Name))
            {
                return OgResult<OgPaySummaryResult>.Failed("Every zone needs a name.", CodeNoZones);
            }

            if (zone.Base <= zone.Top)
            {
                return OgResult<OgPaySummaryResult>.Failed(
                    $"Zone '{zone.Name}' ends at or above its top ({zone.Top:0.##}–{zone.Base:0.##}).",
                    CodeOverlap);
            }

            if (i == 0)
            {
                continue;
            }

            var previous = ordered[i - 1];

            if (zone.Top < previous.Base - Tolerance)
            {
                return OgResult<OgPaySummaryResult>.Failed(
                    $"Zones '{previous.Name}' and '{zone.Name}' overlap: '{previous.Name}' ends at {previous.Base:0.##} " +
                    $"but '{zone.Name}' starts at {zone.Top:0.##}.",
                    CodeOverlap);
            }

            if (zone.Top > previous.Base + Tolerance)
            {
                return OgResult<OgPaySummaryResult>.Failed(
                    $"Zones '{previous.Name}' and '{zone.Name}' are not contiguous: there is a gap " +
                    $"from {previous.Base:0.##} to {zone.Top:0.##}.",
                    CodeGap);
            }
        }

        // The tools derive a zone from its top to the next top (or the log end), so the editor's bases must
        // agree with that rule or the grid would show a gross the arithmetic did not use.
        var bottom = set.Index[^1];

        if (Math.Abs(ordered[^1].Base - bottom) > 0.5)
        {
            return OgResult<OgPaySummaryResult>.Failed(
                $"The last zone ends at {ordered[^1].Base:0.##} but the log ends at {bottom:0.##} {set.IndexUnit}.",
                CodeGap);
        }

        // A copy with the editor's zones as tops: the caller's model is not mutated by a calculation.
        var model = new WellModel();
        model.Header = well.Header;
        model.LogSets.AddRange(well.LogSets);
        model.Survey.AddRange(well.Survey);
        model.Tops.AddRange(ordered.Select(zone => new FormationTop
        {
            Name = zone.Name,
            Depth = new Quantity(zone.Top, set.IndexUnit),
        }));

        var settings = Settings(parameters);
        var warnings = new List<string>();
        var computed = PaySummary.Compute(model, set, warnings, settings);

        var inputs = new Inputs(set);
        var excluded = inputs.MissingNote();

        var rows = computed.Zones
            .Select(zone => new OgPayZoneRow(
                zone.Name,
                zone.Top,
                zone.Base,
                zone.Gross,
                zone.Logged,
                excluded is null ? zone.NetReservoir : 0,
                excluded is null ? zone.NetPay : 0,
                excluded is null ? zone.NetToGross : 0,
                zone.Porosity,
                zone.Sw,
                zone.Vsh,
                excluded is null ? zone.HydrocarbonColumn : 0,
                zone.GrossTvd,
                zone.NetPayTvd,
                zone.Cutoffs.ToString(),
                Excluded: excluded is not null,
                Note: excluded))
            .ToList();

        if (excluded is not null)
        {
            warnings.Add($"every zone is excluded: {excluded}");
        }

        return OgResult<OgPaySummaryResult>.Ok(new OgPaySummaryResult(
            computed.DepthUnit,
            computed.LogSetName,
            rows,
            warnings,
            parameters.Cutoffs));
    }

    /// <summary>The result as CSV, in the tools' column order.</summary>
    public static string ToCsv(OgPaySummaryResult result, IReadOnlyList<OgPayZoneInput> zones)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(zones);

        // The tools' CSV writer is the definition of the format; feed it the computed zones.
        var toolResult = new PaySummaryResult { DepthUnit = result.DepthUnit, LogSetName = result.LogSetName };

        foreach (var zone in result.Zones)
        {
            toolResult.Zones.Add(new PayZone
            {
                Name = zone.Name,
                Top = zone.Top,
                Base = zone.Base,
                Gross = zone.Gross,
                Logged = zone.Logged,
                NetReservoir = zone.NetReservoir,
                NetPay = zone.NetPay,
                Porosity = zone.Porosity,
                Sw = zone.Sw,
                Vsh = zone.Vsh,
                HydrocarbonColumn = zone.HydrocarbonColumn,
                GrossTvd = zone.GrossTvd,
                NetPayTvd = zone.NetPayTvd,
                Cutoffs = new PayCutoffs
                {
                    VshMax = result.Cutoffs.VshMax,
                    PorosityMin = result.Cutoffs.PorosityMin,
                    SwMax = result.Cutoffs.SwMax,
                },
            });
        }

        return PaySummary.ToCsv(toolResult);
    }

    /// <summary>Writes the CSV, refusing to overwrite unless asked (the same rule as every other export).</summary>
    public static OgResult<string> ExportCsv(
        OgPaySummaryResult result,
        IReadOnlyList<OgPayZoneInput> zones,
        string path,
        bool overwrite = false) =>
        WriteText(path, ToCsv(result, zones), overwrite);

    /// <summary>Writes the XLSX through the tools' writer, so numbers stay numbers.</summary>
    public static OgResult<string> ExportXlsx(
        OgPaySummaryResult result,
        IReadOnlyList<OgPayZoneInput> zones,
        string path,
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(zones);

        if (string.IsNullOrWhiteSpace(path))
        {
            return OgResult<string>.Failed("An export needs a destination path.", "pay-export-path");
        }

        if (File.Exists(path) && !overwrite)
        {
            return OgResult<string>.Failed($"'{path}' already exists. Tick overwrite to replace it.", "pay-export-exists");
        }

        var toolResult = new PaySummaryResult { DepthUnit = result.DepthUnit, LogSetName = result.LogSetName };

        foreach (var zone in result.Zones)
        {
            toolResult.Zones.Add(new PayZone
            {
                Name = zone.Name,
                Top = zone.Top,
                Base = zone.Base,
                Gross = zone.Gross,
                Logged = zone.Logged,
                NetReservoir = zone.NetReservoir,
                NetPay = zone.NetPay,
                Porosity = zone.Porosity,
                Sw = zone.Sw,
                Vsh = zone.Vsh,
                HydrocarbonColumn = zone.HydrocarbonColumn,
                GrossTvd = zone.GrossTvd,
                NetPayTvd = zone.NetPayTvd,
                Cutoffs = new PayCutoffs
                {
                    VshMax = result.Cutoffs.VshMax,
                    PorosityMin = result.Cutoffs.PorosityMin,
                    SwMax = result.Cutoffs.SwMax,
                },
            });
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        using var stream = File.Create(path);
        PaySummaryExport.ToXlsx(toolResult, stream);

        return OgResult<string>.Ok(path);
    }

    private static OgResult<string> WriteText(string path, string text, bool overwrite)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return OgResult<string>.Failed("An export needs a destination path.", "pay-export-path");
        }

        if (File.Exists(path) && !overwrite)
        {
            return OgResult<string>.Failed($"'{path}' already exists. Tick overwrite to replace it.", "pay-export-exists");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, text);

        return OgResult<string>.Ok(path);
    }

    private static PetrophysicsSettings Settings(OgPayParameters parameters) =>
        new()
        {
            Cutoffs = new PayCutoffs
            {
                VshMax = parameters.Cutoffs.VshMax,
                PorosityMin = parameters.Cutoffs.PorosityMin,
                SwMax = parameters.Cutoffs.SwMax,
            },
            VshMethod = parameters.VshMethod,
            GrClean = parameters.GrClean,
            GrShale = parameters.GrShale,
            ArchieA = parameters.ArchieA,
            ArchieM = parameters.ArchieM,
            ArchieN = parameters.ArchieN,
            Rw = parameters.Rw,
            MatrixDensity = new Measure(parameters.MatrixDensity, "g/cm3"),
            FluidDensity = new Measure(parameters.FluidDensity, "g/cm3"),
        };

    private readonly record struct Inputs(bool HasVsh, bool HasPorosity, bool HasSw)
    {
        public Inputs(LogSet set)
            : this(
                Curve(set, "VSH") || Curve(set, "GR"),
                Curve(set, "PHIT") || Curve(set, "PHIE") || Curve(set, "RHOB") || Curve(set, "NPHI"),
                Curve(set, "SW") || Curve(set, "RT"))
        {
        }

        public string? MissingNote()
        {
            var missing = new List<string>();

            if (!HasVsh)
            {
                missing.Add("GR (or VSH)");
            }

            if (!HasPorosity)
            {
                missing.Add("RHOB, NPHI, PHIT or PHIE");
            }

            if (!HasSw)
            {
                missing.Add("RT (or SW)");
            }

            return missing.Count == 0
                ? null
                : $"no curve for {string.Join("; ", missing)}";
        }

        private static bool Curve(LogSet set, string mnemonic) =>
            set.Curves.Any(curve => string.Equals(curve.Mnemonic, mnemonic, StringComparison.OrdinalIgnoreCase));
    }
}
