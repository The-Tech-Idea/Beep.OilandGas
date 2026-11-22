using System.Globalization;
using SkiaSharp;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>
/// ENG-07: the axial load against measured depth, with each station's string limit drawn beside it, so a run that exceeds its
/// pipe is visible rather than read out of a grid.
/// </summary>
/// <remarks>
/// The axis conventions follow the tools' own depth views: measured depth increases downward and the load runs across. Every colour
/// comes from the wellbore theme, so the chart follows the application's palette instead of carrying its own.
/// </remarks>
public sealed class OgTorqueDragChart : ViewSurface
{
    /// <summary>One plotted station: measured depth, axial load and the limit of the component covering it.</summary>
    public List<(double Md, double Load, double Limit)> Points { get; } = new();

    /// <summary>The depth unit the stations are in.</summary>
    public string DepthUnit { get; set; } = "m";

    /// <summary>The load unit the values are in.</summary>
    public string LoadUnit { get; set; } = "kN";

    /// <summary>A line above the plot: the scenario and the run's headline.</summary>
    public string Caption { get; set; } = string.Empty;

    public OgTorqueDragChart()
    {
        Name = UiText.Get("view.torqueDrag", "Load vs depth");
        Width = 820;
        Height = 420;
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

        var plot = new SKRect(X + 80, Y + 34, X + Width - 24, Y + Height - 46);
        canvas.DrawRect(plot, background);

        if (!string.IsNullOrWhiteSpace(Caption))
        {
            canvas.DrawText(Caption, plot.Left, Y + 18, SKTextAlign.Left, f11, text);
        }

        if (Points.Count < 2)
        {
            canvas.DrawText("No run: design and run a torque-and-drag case", plot.MidX, plot.MidY, SKTextAlign.Center, f11, text);
            canvas.DrawRect(plot, frame);
            return;
        }

        var mdMin = Points.Min(point => point.Md);
        var mdMax = Math.Max(mdMin + 1e-6, Points.Max(point => point.Md));
        var loadMax = Math.Max(1e-6, Points.Max(point => Math.Max(point.Load, point.Limit)) * 1.05);

        float Xload(double load) => plot.Left + (float)(load / loadMax) * plot.Width;
        float Ymd(double md) => plot.Top + (float)((md - mdMin) / (mdMax - mdMin)) * plot.Height;

        for (var step = 0; step <= 4; step++)
        {
            var load = loadMax * step / 4;
            var md = mdMin + (mdMax - mdMin) * step / 4;

            canvas.DrawLine(Xload(load), plot.Top, Xload(load), plot.Bottom, grid);
            canvas.DrawLine(plot.Left, Ymd(md), plot.Right, Ymd(md), grid);

            canvas.DrawText(load.ToString("0", culture), Xload(load), plot.Bottom + 13, SKTextAlign.Center, f10, text);
            canvas.DrawText(md.ToString("0", culture), plot.Left - 4, Ymd(md) + 3, SKTextAlign.Right, f10, text);
        }

        canvas.DrawText(LoadUnit, plot.MidX, plot.Bottom + 30, SKTextAlign.Center, f10, text);
        canvas.DrawText(DepthUnit, plot.Left - 60, plot.Top - 4, SKTextAlign.Left, f10, text);

        // the limit of the component covering each station, dashed
        using (var limit = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f,
            Color = theme.Text.WithAlpha(150),
            PathEffect = SKPathEffect.CreateDash([6f, 4f], 0),
        })
        using (var path = Paths.Polyline(Points.Select(point => new SKPoint(Xload(point.Limit), Ymd(point.Md))).ToList()))
        {
            canvas.DrawPath(path, limit);
        }

        // the load itself, and the stations that are outside their component's limit marked
        using (var line = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            Color = theme.Frame,
        })
        using (var path = Paths.Polyline(Points.Select(point => new SKPoint(Xload(point.Load), Ymd(point.Md))).ToList()))
        {
            canvas.DrawPath(path, line);
        }

        using (var over = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = theme.Frame })
        {
            foreach (var point in Points.Where(point => point.Load > point.Limit))
            {
                canvas.DrawCircle(new SKPoint(Xload(point.Load), Ymd(point.Md)), 3f, over);
            }
        }

        canvas.DrawRect(plot, frame);
    }
}
