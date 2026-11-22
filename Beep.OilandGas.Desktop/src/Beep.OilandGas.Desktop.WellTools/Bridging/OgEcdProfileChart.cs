using System.Globalization;
using SkiaSharp;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>
/// ENG-08: the equivalent circulating density against measured depth, with the recorded fracture gradient drawn as a threshold when
/// one exists.
/// </summary>
/// <remarks>
/// The conventions are the tools' own depth views - measured depth downward, the same theme, fonts and axis placement - so the chart
/// sits beside the schematic and the load chart rather than looking like a different application. When no fracture gradient is
/// recorded nothing is drawn: a threshold is not invented.
/// </remarks>
public sealed class OgEcdProfileChart : ViewSurface
{
    /// <summary>The profile: depth and ECD in ppg.</summary>
    public List<(double Depth, double Ecd)> Profile { get; } = new();

    /// <summary>The recorded fracture gradients, as depth and equivalent mud weight in ppg.</summary>
    public List<(double Depth, double Gradient)> Fracture { get; } = new();

    /// <summary>The mud weight, drawn as the static line the ECD can never fall below.</summary>
    public double? StaticPpg { get; set; }

    /// <summary>The depth unit the profile is in.</summary>
    public string DepthUnit { get; set; } = "m";

    /// <summary>A line above the plot: the rate and the headline.</summary>
    public string Caption { get; set; } = string.Empty;

    public OgEcdProfileChart()
    {
        Name = UiText.Get("view.ecdProfile", "ECD vs depth");
        Width = 820;
        Height = 400;
    }

    protected override void DrawContent(SKCanvas canvas)
    {
        var theme = WellboreTheme.Default;
        var culture = DisplayFormat.Culture;

        using var text = new SKPaint { IsAntialias = true, Color = theme.Text };
        using var grid = new SKPaint { Color = theme.Grid };
        using var frame = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = theme.Frame };
        using var background = new SKPaint { Color = theme.TrackBackground };
        using var f10 = new SKFont { Size = 10f };
        using var f11 = new SKFont { Size = 11f };

        var plot = new SKRect(X + 80, Y + 34, X + Width - 90, Y + Height - 46);
        canvas.DrawRect(plot, background);

        if (!string.IsNullOrWhiteSpace(Caption))
        {
            canvas.DrawText(Caption, plot.Left, Y + 18, SKTextAlign.Left, f11, text);
        }

        if (Profile.Count < 2)
        {
            canvas.DrawText("No profile: run the circulating system", plot.MidX, plot.MidY, SKTextAlign.Center, f11, text);
            canvas.DrawRect(plot, frame);
            canvas.DrawText("ppg", plot.Right + 8, plot.Bottom + 13, SKTextAlign.Left, f10, text);
            return;
        }

        var depthMin = Math.Min(Profile.Min(point => point.Depth),
            Fracture.Count == 0 ? double.MaxValue : Fracture.Min(point => point.Depth));
        var depthMax = Math.Max(Profile.Max(point => point.Depth),
            Fracture.Count == 0 ? double.MinValue : Fracture.Max(point => point.Depth));

        if (depthMax - depthMin < 1e-9)
        {
            depthMax = depthMin + 1e-6;
        }

        var values = Profile.Select(point => point.Ecd)
            .Concat(Fracture.Select(point => point.Gradient))
            .Concat(StaticPpg is { } stat ? [stat] : Array.Empty<double>())
            .ToList();

        var valueMin = Math.Max(0d, values.Min() - 0.2d);
        var valueMax = values.Max() + 0.2d;

        if (valueMax - valueMin < 1e-6)
        {
            valueMax = valueMin + 1e-6;
        }

        float Xvalue(double value) => plot.Left + (float)((value - valueMin) / (valueMax - valueMin)) * plot.Width;
        float Ydepth(double depth) => plot.Top + (float)((depth - depthMin) / (depthMax - depthMin)) * plot.Height;

        for (var step = 0; step <= 4; step++)
        {
            var value = valueMin + (valueMax - valueMin) * step / 4d;
            var depth = depthMin + (depthMax - depthMin) * step / 4d;

            canvas.DrawLine(Xvalue(value), plot.Top, Xvalue(value), plot.Bottom, grid);
            canvas.DrawLine(plot.Left, Ydepth(depth), plot.Right, Ydepth(depth), grid);

            canvas.DrawText(value.ToString("0.0", culture), Xvalue(value), plot.Bottom + 13, SKTextAlign.Center, f10, text);
            canvas.DrawText(depth.ToString("0", culture), plot.Left - 4, Ydepth(depth) + 3, SKTextAlign.Right, f10, text);
        }

        canvas.DrawText("ppg", plot.Right + 30, plot.Bottom + 30, SKTextAlign.Center, f10, text);
        canvas.DrawText(DepthUnit, plot.Left - 60, plot.Top - 4, SKTextAlign.Left, f10, text);

        // the static mud weight, which no circulating ECD can fall below
        if (StaticPpg is { } staticWeight)
        {
            using var statPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                Color = theme.Grid,
                PathEffect = SKPathEffect.CreateDash([3f, 3f], 0),
            };

            canvas.DrawLine(Xvalue(staticWeight), plot.Top, Xvalue(staticWeight), plot.Bottom, statPaint);
        }

        // the recorded fracture gradient, when there is one
        if (Fracture.Count > 0)
        {
            using var fracturePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.4f,
                Color = theme.Text.WithAlpha(170),
                PathEffect = SKPathEffect.CreateDash([6f, 4f], 0),
            };

            using var fracturePath = Paths.Polyline(
                Fracture.OrderBy(point => point.Depth)
                    .Select(point => new SKPoint(Xvalue(point.Gradient), Ydepth(point.Depth)))
                    .ToList());

            canvas.DrawPath(fracturePath, fracturePaint);
        }

        // the profile
        using var profilePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            Color = theme.Frame,
        };

        using var profilePath = Paths.Polyline(
            Profile.OrderBy(point => point.Depth)
                .Select(point => new SKPoint(Xvalue(point.Ecd), Ydepth(point.Depth)))
                .ToList());

        canvas.DrawPath(profilePath, profilePaint);

        canvas.DrawRect(plot, frame);
    }
}
