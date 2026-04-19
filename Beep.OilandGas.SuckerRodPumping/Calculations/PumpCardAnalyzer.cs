using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.SuckerRodPumping;

namespace Beep.OilandGas.SuckerRodPumping.Calculations
{
    /// <summary>
    /// Advanced dynamometer pump-card analysis.
    ///
    /// A dynamometer card is a closed loop on a (position, load) plane acquired at the
    /// polished rod.  The shape of the loop is a fingerprint of the down-hole pump
    /// condition.  Classic pattern-recognition features are:
    ///
    ///  • Card area    — proportional to work done per stroke; used to compute pump efficiency
    ///  • Load range   — PPRL − MPRL; indicates fluid loading
    ///  • Upstroke shape — ideally a broad plateau; deviations indicate gas or valve problems
    ///  • Downstroke shape — load should drop sharply at the start then remain flat
    ///  • Bump-in / bump-out characteristics — where the standing and travelling valves open/close
    ///
    /// Pattern classification thresholds follow the logic in:
    ///   - Gibbs, S.G. (1963) "Predicting the Behavior of Sucker-Rod Pumping Systems"
    ///     JPT, July 1963, pp 769-778.
    ///   - Dempsey, J.C. (1965) API Paper 801-40B; API production dept.
    ///   - XSPOC Well Optimisation Guide (generic) for modern interpretive heuristics.
    ///
    /// Unit conventions in this class:
    ///   - Position: normalised 0–1 (0 = bottom of stroke, 1 = top)
    ///   - Load: pounds-force (lbf)
    /// </summary>
    public static class PumpCardAnalyzer
    {
        // ─────────────────────────────────────────────────────────────────
        //  Pump condition patterns (enumeration)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Diagnosed pump condition from dynamometer card pattern analysis.</summary>
        public enum PumpCondition
        {
            /// <summary>Normal operation — pump filling adequately, both valves seating properly.</summary>
            Normal,
            /// <summary>Fluid pound — pump barrel partially filled; rapid load drop early in downstroke.</summary>
            FluidPound,
            /// <summary>Gas interference — gaseous fluid reaching pump; upstroke plateau lost.</summary>
            GasInterference,
            /// <summary>Pump worn or barrel leak — net area reduced due to fluid slippage.</summary>
            WornPump,
            /// <summary>Stuck or leaking travelling valve — load does not rise at start of upstroke.</summary>
            StuckTravellingValve,
            /// <summary>Stuck or leaking standing valve — load drops slowly during downstroke instead of sharply.</summary>
            StuckStandingValve,
            /// <summary>Over-pumping (pump-off) — fluid level below pump intake; card collapses.</summary>
            Overpumping,
            /// <summary>Pump tagging (plunger hitting barrel bottom) — characteristic sharp load spike.</summary>
            PumpTagging,
            /// <summary>Rod part (rod string failure) — load profile consistent with missing lower section.</summary>
            RodPart,
            /// <summary>Insufficient data to diagnose.</summary>
            Indeterminate
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Pump card analysis result.</summary>
        public sealed class CardAnalysisResult
        {
            /// <summary>Computed card area using the shoelace formula (lbf · stroke fraction).</summary>
            public double CardArea { get; set; }

            /// <summary>Theoretical maximum card area if pump were perfect (lbf · stroke fraction).</summary>
            public double TheoreticalMaxArea { get; set; }

            /// <summary>Pump fillage efficiency = CardArea / TheoreticalMaxArea (0–1).</summary>
            public double FillageFraction { get; set; }

            /// <summary>Pump volumetric efficiency estimated from fillage (0–100 %).</summary>
            public double VolumetricEfficiencyPercent { get; set; }

            /// <summary>Daily liquid production estimate based on pump geometry and efficiency (bbl/day).</summary>
            public double EstimatedLiquidBbld { get; set; }

            /// <summary>Primary diagnosed pump condition.</summary>
            public PumpCondition PrimaryCondition { get; set; }

            /// <summary>Secondary (co-existing) condition if detected.</summary>
            public PumpCondition? SecondaryCondition { get; set; }

            /// <summary>Qualitative description of the card shape.</summary>
            public string ShapeDescription { get; set; } = string.Empty;

            /// <summary>Recommended operational action.</summary>
            public string Recommendation { get; set; } = string.Empty;

            /// <summary>Feature metrics used for classification.</summary>
            public CardFeatures Features { get; set; } = new();
        }

        /// <summary>Extracted geometric/statistical features from the card loop.</summary>
        public sealed class CardFeatures
        {
            /// <summary>Peak load (lbf).</summary>
            public double PeakLoad { get; set; }

            /// <summary>Minimum load (lbf).</summary>
            public double MinLoad { get; set; }

            /// <summary>Load range = PeakLoad − MinLoad (lbf).</summary>
            public double LoadRange { get; set; }

            /// <summary>Upstroke average gradient (lbf per stroke fraction).</summary>
            public double UpstrokeGradient { get; set; }

            /// <summary>Downstroke average gradient (lbf per stroke fraction).</summary>
            public double DownstrokeGradient { get; set; }

            /// <summary>Position where upstroke load first plateaus (fraction 0–1).</summary>
            public double PlateauStartPosition { get; set; }

            /// <summary>Position where downstroke load begins its drop (fraction 0–1).</summary>
            public double DropStartPosition { get; set; }

            /// <summary>Ratio of achieved load range to theoretical (fluid) load.</summary>
            public double LoadRangeRatio { get; set; }

            /// <summary>Standard deviation of load on the upstroke plateau — high value indicates gas.</summary>
            public double UpstrokePlateauNoise { get; set; }

            /// <summary>Whether a sharp load spike at the end of the downstroke was detected (pump tagging).</summary>
            public bool TaggingDetected { get; set; }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Main analysis entry point
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Analyses a dynamometer card and returns pump condition diagnosis and efficiency.
        /// </summary>
        /// <param name="card">Pump card from <see cref="SuckerRodLoadCalculator.GeneratePumpCard"/> or measured data.</param>
        /// <param name="systemProperties">System properties for efficiency and production estimate.</param>
        /// <param name="theoreticalFluidLoad">
        ///   Expected fluid load on the pump plunger (lbf) = pump_area × ΔP.
        ///   If 0, it is estimated from the card's load range.
        /// </param>
        public static CardAnalysisResult Analyze(
            PumpCard card,
            SUCKER_ROD_SYSTEM_PROPERTIES systemProperties,
            double theoreticalFluidLoad = 0.0)
        {
            if (card?.Points == null || card.Points.Count < 4)
                return new CardAnalysisResult { PrimaryCondition = PumpCondition.Indeterminate };

            var pts = card.Points
                .OrderBy(p => p.Position)
                .Select(p => ((double)p.Position, (double)p.Load))
                .ToList();

            // ── Features ────────────────────────────────────────────────
            var features = ExtractFeatures(pts, theoreticalFluidLoad);
            double cardArea = ComputeCardArea(pts);
            double maxArea = features.LoadRange * 1.0; // unit position range = 1

            double fillage = maxArea > 1.0 ? Math.Min(1.0, cardArea / maxArea) : 0.0;
            double volEff = fillage * 100.0;

            // ── Production estimate ──────────────────────────────────────
            double spm    = (double)systemProperties.STROKES_PER_MINUTE;
            double stroke = (double)systemProperties.STROKE_LENGTH;         // inches
            double dPump  = (double)systemProperties.PUMP_DIAMETER;         // inches
            double pump_area_in2 = Math.PI * dPump * dPump / 4.0;
            // Volume per stroke (in³) = pump_area × stroke_length × fillage
            double volumePerStroke_in3 = pump_area_in2 * stroke * fillage;
            // Convert to bbl/day: 1 bbl = 9702 in³; 1 day = 1440 min
            double bbld = volumePerStroke_in3 * spm * 1440.0 / 9702.0;

            // ── Condition diagnosis ──────────────────────────────────────
            (var primary, var secondary, string desc, string rec) = Diagnose(features, fillage);

            return new CardAnalysisResult
            {
                CardArea                   = Math.Round(cardArea, 2),
                TheoreticalMaxArea         = Math.Round(maxArea, 2),
                FillageFraction            = Math.Round(fillage, 3),
                VolumetricEfficiencyPercent= Math.Round(volEff, 1),
                EstimatedLiquidBbld        = Math.Round(bbld, 1),
                PrimaryCondition           = primary,
                SecondaryCondition         = secondary,
                ShapeDescription           = desc,
                Recommendation             = rec,
                Features                   = features
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Card area calculation (shoelace / trapezoidal integration)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the enclosed area of the card loop using the shoelace formula.
        ///
        /// The card must form a closed loop.  The area equals the work per stroke
        /// (lbf × stroke fraction).  Multiply by stroke_length (in) to get lbf·in,
        /// then convert to ft·lbf or horsepower as needed.
        /// </summary>
        public static double ComputeCardArea(IReadOnlyList<(double Position, double Load)> pts)
        {
            if (pts == null || pts.Count < 3) return 0.0;
            double area = 0.0;
            int n = pts.Count;
            for (int i = 0; i < n; i++)
            {
                var (x1, y1) = pts[i];
                var (x2, y2) = pts[(i + 1) % n];
                area += (x1 * y2) - (x2 * y1);
            }
            return Math.Abs(area) / 2.0;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Feature extraction
        // ─────────────────────────────────────────────────────────────────

        private static CardFeatures ExtractFeatures(
            List<(double Position, double Load)> pts,
            double theoreticalFluidLoad)
        {
            double peak = pts.Max(p => p.Load);
            double min  = pts.Min(p => p.Load);
            double range = peak - min;

            // Split into upstroke (position increasing) and downstroke (position decreasing)
            // The sorted list by position approximates the upstroke; the downstroke is the
            // reverse path back from position 1 to 0.  In practice the card is a loop
            // with duplicate x-values, so we use the first-half and second-half split.
            int half = pts.Count / 2;
            var upstroke   = pts.Take(half + 1).ToList();
            var downstroke = pts.Skip(half).ToList();

            // Upstroke gradient: load change from pos 0.1 to 0.4 (loading section)
            double upGrad = ComputeRegionGradient(upstroke, 0.05, 0.40);
            // Downstroke gradient: load change from pos 0.9 to 0.6 (unloading section)
            double dnGrad = ComputeRegionGradient(downstroke, 0.95, 0.60);

            // Plateau start: position where upstroke load first exceeds 90% of peak
            double plateauPos = PositionAtLoad(upstroke, 0.90 * peak, 0.5);

            // Drop start: position on downstroke where load first drops below 90% of peak
            double dropPos = PositionAtLoad(downstroke, 0.90 * peak, 0.9);

            // Plateau noise: std-dev of load between position 0.3 and 0.8 on upstroke
            double plateauNoise = LoadStdDev(upstroke, 0.30, 0.80);

            // Pump tagging: load spike in the last 5% of downstroke exceeds 105% of mean load
            double meanLoad = pts.Average(p => p.Load);
            var lastSection = pts.Where(p => p.Position > 0.92).ToList();
            bool tagging = lastSection.Count > 0 && lastSection.Max(p => p.Load) > 1.05 * meanLoad;

            double lrRatio = theoreticalFluidLoad > 1.0 ? range / theoreticalFluidLoad : 1.0;

            return new CardFeatures
            {
                PeakLoad              = Math.Round(peak, 1),
                MinLoad               = Math.Round(min, 1),
                LoadRange             = Math.Round(range, 1),
                UpstrokeGradient      = Math.Round(upGrad, 1),
                DownstrokeGradient    = Math.Round(dnGrad, 1),
                PlateauStartPosition  = Math.Round(plateauPos, 3),
                DropStartPosition     = Math.Round(dropPos, 3),
                LoadRangeRatio        = Math.Round(lrRatio, 3),
                UpstrokePlateauNoise  = Math.Round(plateauNoise, 1),
                TaggingDetected       = tagging
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Condition diagnosis
        // ─────────────────────────────────────────────────────────────────

        private static (PumpCondition primary, PumpCondition? secondary, string desc, string rec)
            Diagnose(CardFeatures f, double fillage)
        {
            // Pump tagging: load spike near bottom — check first
            if (f.TaggingDetected)
                return (PumpCondition.PumpTagging, null,
                    "Load spike detected at bottom of stroke — plunger contacting barrel bottom.",
                    "Reduce stroke length or pump setting depth to eliminate tagging. Risk of pump failure.");

            // Overpumping / pump-off: very low fillage, card collapses
            if (fillage < 0.30)
                return (PumpCondition.Overpumping, null,
                    "Card area severely reduced — pump is running dry (pump-off).",
                    "Reduce pump speed (SPM) or install a pump-off controller. Increase fluid inflow if possible.");

            // Fluid pound: moderate fillage (30–70%) with high downstroke gradient (sharp load drop)
            // The downstroke drop occurs early — DropStartPosition > 0.7 means late drop is normal;
            // a value < 0.55 indicates early / abrupt drop (fluid pound signature).
            if (fillage < 0.75 && f.DropStartPosition < 0.55)
                return (PumpCondition.FluidPound, fillage < 0.50 ? PumpCondition.Overpumping : (PumpCondition?)null,
                    $"Fluid pound detected (fillage {fillage:P0}). Abrupt downstroke load drop at position {f.DropStartPosition:F2}.",
                    "Reduce pump speed or install time-delay pump-off controller. Monitor for barrel and valve wear.");

            // Gas interference: upstroke plateau is noisy / irregular with reduced load range
            if (f.UpstrokePlateauNoise > 0.08 * f.LoadRange && f.LoadRangeRatio < 0.80)
                return (PumpCondition.GasInterference, null,
                    "Noisy upstroke plateau and reduced load range indicate gas interference at pump intake.",
                    "Increase pump submergence, consider gas anchor or down-hole gas separator. Slow pump speed.");

            // Stuck / leaking travelling valve: upstroke load rises very slowly (flat near min load)
            if (f.PlateauStartPosition > 0.70)
                return (PumpCondition.StuckTravellingValve, null,
                    $"Upstroke load plateaus late (position {f.PlateauStartPosition:F2}) — travelling valve may be leaking or stuck.",
                    "Pull pump and inspect/replace travelling valve ball and seat.");

            // Stuck / leaking standing valve: downstroke load drops very slowly
            if (f.DropStartPosition > 0.75 && f.DownstrokeGradient > -0.2 * f.LoadRange)
                return (PumpCondition.StuckStandingValve, null,
                    "Slow downstroke unloading — standing valve leaking or stuck open.",
                    "Pull pump and inspect/replace standing valve ball and seat.");

            // Worn pump: reduced card area with otherwise normal shape
            if (fillage < 0.65 && fillage >= 0.30 && f.LoadRangeRatio < 0.70)
                return (PumpCondition.WornPump, null,
                    $"Normal card shape but low fillage ({fillage:P0}) and reduced load range — worn barrel or plunger.",
                    "Pull pump and measure barrel/plunger clearance. Replace pump if clearance exceeds API limits.");

            // Normal
            return (PumpCondition.Normal, null,
                $"Normal card shape — pump filling adequately ({fillage:P0} fillage). Both valves seating properly.",
                "Continue normal operation. Schedule next inspection per standard interval.");
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helper methods
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Compute average load gradient (Δload / Δposition) over a position sub-range.</summary>
        private static double ComputeRegionGradient(
            List<(double Position, double Load)> pts,
            double startPos, double endPos)
        {
            double lo = Math.Min(startPos, endPos);
            double hi = Math.Max(startPos, endPos);
            var region = pts.Where(p => p.Position >= lo && p.Position <= hi).ToList();
            if (region.Count < 2) return 0.0;
            double dPos  = region.Last().Position - region.First().Position;
            double dLoad = region.Last().Load - region.First().Load;
            return dPos > 1e-9 ? dLoad / dPos : 0.0;
        }

        /// <summary>Find the position where load first crosses a threshold in the point sequence.</summary>
        private static double PositionAtLoad(
            List<(double Position, double Load)> pts,
            double targetLoad,
            double defaultPos)
        {
            for (int i = 1; i < pts.Count; i++)
            {
                if ((pts[i - 1].Load < targetLoad && pts[i].Load >= targetLoad) ||
                    (pts[i - 1].Load > targetLoad && pts[i].Load <= targetLoad))
                {
                    // Linear interpolation
                    double frac = (targetLoad - pts[i - 1].Load) /
                                  (pts[i].Load - pts[i - 1].Load + 1e-12);
                    return pts[i - 1].Position + frac * (pts[i].Position - pts[i - 1].Position);
                }
            }
            return defaultPos;
        }

        /// <summary>Standard deviation of load values within a position sub-range.</summary>
        private static double LoadStdDev(
            List<(double Position, double Load)> pts,
            double loPos, double hiPos)
        {
            var region = pts
                .Where(p => p.Position >= loPos && p.Position <= hiPos)
                .Select(p => p.Load)
                .ToList();

            if (region.Count < 2) return 0.0;
            double mean = region.Average();
            double variance = region.Sum(x => (x - mean) * (x - mean)) / region.Count;
            return Math.Sqrt(variance);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Batch comparison: compare current card against a reference
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Compares a current dynamometer card against a reference (baseline) card and
        /// quantifies the change in pump condition.
        ///
        /// Returns a signed percent change in card area (negative = pump deteriorating).
        /// </summary>
        /// <param name="referenceCard">Baseline card (healthy pump).</param>
        /// <param name="currentCard">Current card to compare.</param>
        public static (double AreaChangePct, string TrendDescription) CompareCards(
            PumpCard referenceCard, PumpCard currentCard)
        {
            if (referenceCard?.Points == null || currentCard?.Points == null)
                return (0, "Insufficient data.");

            var refPts = referenceCard.Points
                .OrderBy(p => p.Position)
                .Select(p => ((double)p.Position, (double)p.Load))
                .ToList();

            var curPts = currentCard.Points
                .OrderBy(p => p.Position)
                .Select(p => ((double)p.Position, (double)p.Load))
                .ToList();

            double refArea = ComputeCardArea(refPts);
            double curArea = ComputeCardArea(curPts);

            double changePct = refArea > 1.0 ? (curArea - refArea) / refArea * 100.0 : 0.0;
            string trend = changePct >= 0  ? $"Card area improved by {changePct:F1}%."
                         : changePct > -10 ? $"Minor decline: card area reduced by {-changePct:F1}%."
                         : changePct > -25 ? $"Moderate decline: card area reduced by {-changePct:F1}% — monitor closely."
                                           : $"Significant decline: card area reduced by {-changePct:F1}% — pump investigation required.";

            return (Math.Round(changePct, 2), trend);
        }
    }
}
