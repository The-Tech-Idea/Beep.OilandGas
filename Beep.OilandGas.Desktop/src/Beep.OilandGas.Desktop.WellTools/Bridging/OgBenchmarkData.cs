using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>What a log render will do to a curve of a given length (REL-04 AC-2).</summary>
/// <param name="Samples">Samples in the curve.</param>
/// <param name="Threshold">The configured sample count above which drawing is thinned.</param>
/// <param name="Bound">The most points the renderer will draw.</param>
/// <param name="Ratio">Samples per drawn point.</param>
/// <param name="Applied">True when the curve is above the threshold.</param>
/// <param name="Note">One sentence for the view's badge, or null when nothing was thinned.</param>
public sealed record OgDecimationPlan(
    int Samples,
    int Threshold,
    int Bound,
    double Ratio,
    bool Applied,
    string? Note);

/// <summary>
/// REL-04: a deterministic large log and the decimation arithmetic the view states.
/// </summary>
/// <remarks>
/// Thinning is the tools' <see cref="CurveDecimation"/>: points are bucketed by pixel row and four points per
/// row are kept (first, leftmost, rightmost, last), so the drawn polyline covers the same pixels and spikes
/// survive. The plan's bound is therefore four points per pixel row, which is what
/// <see cref="OgDecimationPlan.Bound"/> reports before anything is drawn.
/// </remarks>
public static class OgBenchmarkData
{
    /// <summary>The sample count above which the log view thins a curve, and says so.</summary>
    public const int DisplaySampleThreshold = 20_000;

    /// <summary>A deterministic depth log with <paramref name="samples"/> GR samples, for timing and tests.</summary>
    public static WellModel SyntheticLog(int samples = 100_000)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(samples, 2);

        var model = new WellModel();
        model.Header.Id = $"BENCH-{samples}";
        model.Header.Name = $"Benchmark log {samples:N0} samples";
        model.Header.Uwi = $"BENCH-{samples}-UWI";

        var set = new LogSet
        {
            Name = $"synthetic {samples}",
            IndexType = LogIndexType.Depth,
            IndexUnit = "m",
            IndexMnemonic = "DEPT",
        };

        var index = new double[samples];
        var values = new double?[samples];

        for (var i = 0; i < samples; i++)
        {
            var depth = i * 0.5;
            index[i] = depth;

            // Two wavelengths so the curve has features at both scales; a straight line would not show whether
            // extrema survive decimation.
            values[i] = 60 + (18 * Math.Sin(depth / 40d)) + (6 * Math.Sin(depth / 3.5d));
        }

        set.Index = index;
        set.Curves.Add(new LogCurveData
        {
            Mnemonic = "GR",
            Unit = "gAPI",
            Description = "Gamma ray (synthetic)",
            Values = values,
        });

        model.LogSets.Add(set);
        return model;
    }

    /// <summary>The plan for a curve of <paramref name="samples"/> over a plot of <paramref name="plotHeight"/> pixels.</summary>
    public static OgDecimationPlan Plan(
        int samples,
        int threshold = DisplaySampleThreshold,
        int plotHeight = 600)
    {
        plotHeight = Math.Max(1, plotHeight);
        threshold = Math.Max(1, threshold);

        var bound = Math.Min(samples, plotHeight * 4);
        var applied = samples > threshold;
        var ratio = bound <= 0 ? 1 : (double)samples / bound;

        return new OgDecimationPlan(
            samples,
            threshold,
            bound,
            ratio,
            applied,
            applied
                ? $"Decimated: {samples:N0} samples drawn as at most {bound:N0} points ({ratio:0.#}:1) at {plotHeight} px."
                : null);
    }

    /// <summary>Thins a polyline with the tools' renderer; used by the tests to prove the bound is real.</summary>
    public static IReadOnlyList<(float x, float y)> Decimate(IReadOnlyList<(float x, float y)> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        return CurveDecimation.Decimate(points);
    }

    /// <summary>The GR curve's points as (value, depth), which is how the renderer buckets them.</summary>
    public static IReadOnlyList<(float x, float y)> Points(WellModel model, int plotHeight = 600)
    {
        ArgumentNullException.ThrowIfNull(model);

        var set = model.LogSets.FirstOrDefault();
        var curve = set?.Curves.FirstOrDefault();

        if (set is null || curve is null)
        {
            return [];
        }

        var points = new List<(float x, float y)>(curve.Values.Length);
        var top = set.Index.Length > 0 ? set.Index[0] : 0;
        var bottom = set.Index.Length > 0 ? set.Index[^1] : 1;
        var span = Math.Max(1e-9, bottom - top);

        for (var i = 0; i < curve.Values.Length; i++)
        {
            var depth = set.Index[i];
            var y = (float)((depth - top) / span * plotHeight);
            points.Add(((float)(curve.Values[i] ?? double.NaN), y));
        }

        return points;
    }
}
