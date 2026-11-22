using SkiaSharp;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One barrier that has no verification evidence, with the interval it covers.</summary>
public sealed record OgUnverifiedBarrier(string Element, string Role, double Top, double Bottom);

/// <summary>
/// VSB-05 AC-3: marks an unverified barrier on the drawing.
/// </summary>
/// <remarks>
/// <para>
/// The tools' <c>BarrierOverlay</c> draws a cross for a <c>Failed</c> element and nothing at all for an
/// <c>Unverified</c> one, so on the schematic an unverified barrier looks identical to a verified one. That
/// is the distinction this overlay adds: a hatched band over each unverified element's interval, in the
/// warning colour, drawn on top of the schematic in the surface's own coordinates.
/// </para>
/// <para>
/// It is deliberately a desktop overlay rather than a change to the tools: the barrier set is theirs and the
/// geometry is theirs (<see cref="SchematicRenderPlan.DepthToY"/>), so this class only decides that
/// "unverified" deserves to be visible.
/// </para>
/// </remarks>
public static class OgSchematicBarrierOverlay
{
    /// <summary>
    /// The barriers without evidence, resolved to depths. Fallback order matches the tools' own overlay:
    /// the element's own interval, then the string or cement it targets, then the drawn body of the
    /// component it targets.
    /// </summary>
    public static IReadOnlyList<OgUnverifiedBarrier> Unverified(
        WellSchematicDocument document,
        SchematicRenderPlan? plan = null)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Barriers is not { } set)
        {
            return [];
        }

        var marks = new List<OgUnverifiedBarrier>();

        foreach (var envelope in set.Envelopes)
        {
            foreach (var element in envelope.Elements)
            {
                var verified = element.VerifiedDate.HasValue
                               || !string.IsNullOrWhiteSpace(element.VerificationMethod)
                               || !string.IsNullOrWhiteSpace(element.AcceptanceRef);

                if (verified)
                {
                    continue;
                }

                var interval = IntervalOf(document, element, plan);
                if (interval is not { } span || span.Bottom <= span.Top)
                {
                    // No geometry to mark (the element has no interval and no resolvable target, or a
                    // zero-length one). It still appears in the barrier panel, so nothing is hidden.
                    continue;
                }

                marks.Add(new OgUnverifiedBarrier(
                    element.DisplayName,
                    envelope.Role.ToString(),
                    span.Top,
                    span.Bottom));
            }
        }

        return marks;
    }

    /// <summary>Draws the hatch over each unverified interval; returns how many marks were painted.</summary>
    /// <remarks>
    /// <para>
    /// The mark is a depth-aligned band in the drawing's right margin, not a rectangle over the casing
    /// column: the committed <c>SchematicRenderPlan</c> exposes no usable column or depth mapping after the
    /// draw (its depth area collapses), so placing the band from the canvas bounds and the document's own
    /// top and bottom is the only honest option — and it is a margin marker a person can read.
    /// </para>
    /// </remarks>
    public static int Draw(
        SKCanvas canvas,
        WellSchematicDocument document,
        IReadOnlyList<OgUnverifiedBarrier> marks)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(marks);

        if (marks.Count == 0 || document.BottomDepth <= document.TopDepth)
        {
            return 0;
        }

        var bounds = canvas.DeviceClipBounds;

        if (bounds.Height <= 1 || bounds.Width <= 20)
        {
            return 0;
        }

        var right = bounds.Right - 4f;
        var left = right - 10f;
        var painted = 0;

        using var hatch = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.25f,
            Color = new SKColor(0xB2, 0x6A, 0x00, 0xCC),
        };

        using var edge = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f,
            Color = new SKColor(0xB2, 0x6A, 0x00),
        };

        float YOf(double depth) =>
            (float)(bounds.Top
                    + ((depth - document.TopDepth) / (document.BottomDepth - document.TopDepth))
                    * bounds.Height);

        foreach (var mark in marks)
        {
            var y1 = YOf(Math.Min(mark.Top, mark.Bottom));
            var y2 = YOf(Math.Max(mark.Top, mark.Bottom));

            if (float.IsNaN(y1) || float.IsNaN(y2) || Math.Abs(y2 - y1) < 1f)
            {
                continue;
            }

            var rect = new SKRect(left, Math.Min(y1, y2), right, Math.Max(y1, y2));
            canvas.DrawRect(rect, edge);

            // 45° hatch: unmistakable at a glance and distinct from the tools' solid barrier lines.
            canvas.Save();
            canvas.ClipRect(rect);
            for (var x = rect.Left - rect.Height; x < rect.Right; x += 7f)
            {
                canvas.DrawLine(x, rect.Bottom, x + rect.Height, rect.Top, hatch);
            }

            canvas.Restore();
            painted++;
        }

        return painted;
    }

    private static (double Top, double Bottom)? IntervalOf(
        WellSchematicDocument document,
        BarrierElement element,
        SchematicRenderPlan? plan)
    {
        if (element.Top is { } top && element.Bottom is { } bottom)
        {
            return (top.In(DepthUnits.Symbol(document.DepthUnit)), bottom.In(DepthUnits.Symbol(document.DepthUnit)));
        }

        if (element.TargetId is { } targetId)
        {
            if (document.Strings.FirstOrDefault(item => item.Id == targetId) is { } found)
            {
                var topDepth = element.TargetKind == BarrierTargetKind.Cement
                    ? found.CementTop ?? found.TopDepth
                    : found.TopDepth;
                var bottomDepth = element.TargetKind == BarrierTargetKind.Cement
                    ? found.CementBottom ?? found.BottomDepth
                    : found.BottomDepth;

                return (topDepth, bottomDepth);
            }

            if (plan is not null && plan.EquipmentBodies.TryGetValue(targetId, out var body))
            {
                return (plan.YToDepth(body.Top), plan.YToDepth(body.Bottom));
            }
        }

        return null;
    }
}
