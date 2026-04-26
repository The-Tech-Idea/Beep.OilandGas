using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Multi-well gas lift injection gas allocation optimization.
    ///
    /// Problem: Given a fixed total gas injection rate Q_total (Mscfd) available to a
    /// field, allocate injection gas among N gas lift wells to maximize total liquid
    /// production or field-level NPV, subject to:
    ///   • Per-well minimum and maximum injection rate constraints.
    ///   • Surface gas supply limit.
    ///   • Separator/pipeline capacity constraints.
    ///
    /// Methods:
    ///  1. <b>Equal incremental production (Equal GLR) allocation</b>:
    ///     Each well receives gas until the marginal production gain per Mscfd is equal
    ///     across all wells — the classic economic allocation criterion.
    ///
    ///  2. <b>Proportional allocation</b>:
    ///     Gas distributed proportional to each well's optimal GLR (useful when
    ///     performance curves are not available).
    ///
    ///  3. <b>Economic optimization (NPV/incremental barrel)</b>:
    ///     Ranks wells by incremental oil revenue per Mscfd of lift gas injected;
    ///     allocates gas greedily from highest to lowest marginal value.
    ///
    ///  4. <b>Sensitivity sweep</b>:
    ///     Sweeps total allocation from minimum to maximum for a set of wells,
    ///     reporting total production at each allocation level.
    ///
    /// Performance curve model used:
    ///   q_oil(Q_inj) = q_max × (1 − e^(−a × Q_inj))  (exponential IPR-GLR saturation)
    ///   where a = curve steepness parameter derived from two known operating points.
    ///
    /// References:
    ///   - Kanu, E.P. et al. (1981) SPE-10253 "Economic Approach to Oil Production and Gas
    ///     Allocation in Continuous Gas Lift"
    ///   - Weiss, W.W. et al. (1975) SPE-5557
    ///   - Nind, T.E.W. (1964) "Principles of Oil Well Production"
    /// </summary>
    public static class GasAllocationOptimization
    {
        // ─────────────────────────────────────────────────────────────────
        //  1. Well performance curve fit
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fits an exponential gas lift performance curve from two known operating points:
        ///   q_oil = q_max × (1 − exp(−a × Q_inj))
        ///
        /// Given (Q₁, q₁) and q_max, solve for 'a':
        ///   a = −ln(1 − q₁/q_max) / Q₁
        /// </summary>
        /// <param name="wellId">Well identifier.</param>
        /// <param name="qMaxOilBopd">Maximum achievable oil rate (bopd) at unlimited injection.</param>
        /// <param name="knownInjectionMscfd">A known injection rate (Mscfd) on the curve.</param>
        /// <param name="knownOilBopd">Oil rate (bopd) at the known injection rate.</param>
        /// <param name="minInjectionMscfd">Minimum injection to keep well alive (Mscfd).</param>
        /// <param name="maxInjectionMscfd">Maximum injection (equipment / wellbore limit, Mscfd).</param>
        public static WellPerformanceCurve FitPerformanceCurve(
            string wellId,
            double qMaxOilBopd,
            double knownInjectionMscfd,
            double knownOilBopd,
            double minInjectionMscfd = 0,
            double maxInjectionMscfd = 5_000)
        {
            if (qMaxOilBopd <= 0)        throw new ArgumentOutOfRangeException(nameof(qMaxOilBopd));
            if (knownInjectionMscfd <= 0) throw new ArgumentOutOfRangeException(nameof(knownInjectionMscfd));

            double fraction = Math.Min(0.999, knownOilBopd / qMaxOilBopd);
            double a = -Math.Log(1.0 - fraction) / knownInjectionMscfd;

            return new WellPerformanceCurve
            {
                WellId             = wellId,
                QMaxOilBopd        = qMaxOilBopd,
                CurveParameter     = a,
                MinInjectionMscfd  = minInjectionMscfd,
                MaxInjectionMscfd  = maxInjectionMscfd,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  2. Evaluate production at a given injection rate
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates oil production rate for a given injection rate using the
        /// fitted exponential performance curve.
        /// </summary>
        public static double EvaluateOilRate(WellPerformanceCurve curve, double injectionMscfd)
        {
            return curve.QMaxOilBopd * (1.0 - Math.Exp(-curve.CurveParameter * injectionMscfd));
        }

        /// <summary>Marginal oil production gain per additional Mscfd (derivative of curve).</summary>
        public static double MarginalOilRate(WellPerformanceCurve curve, double injectionMscfd)
        {
            return curve.QMaxOilBopd * curve.CurveParameter * Math.Exp(-curve.CurveParameter * injectionMscfd);
        }

        // ─────────────────────────────────────────────────────────────────
        //  3. Proportional allocation
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Allocates gas proportionally to each well's optimal GLR (inverse of curve steepness).
        /// Useful as a quick starting point when full optimization is not needed.
        /// </summary>
        public static List<WellAllocation> ProportionalAllocation(
            IList<WellPerformanceCurve> wells,
            double totalAvailableGasMscfd)
        {
            if (wells == null || wells.Count == 0) return new List<WellAllocation>();

            // Weight: inversely proportional to 'a' (flatter curve → needs more gas)
            double totalWeight = wells.Sum(w => 1.0 / w.CurveParameter);
            var allocations    = new List<WellAllocation>();

            foreach (var w in wells)
            {
                double share = totalAvailableGasMscfd * (1.0 / w.CurveParameter) / totalWeight;
                share = Math.Max(w.MinInjectionMscfd, Math.Min(w.MaxInjectionMscfd, share));
                allocations.Add(new WellAllocation
                {
                    WellId              = w.WellId,
                    AllocatedGasMscfd   = share,
                    PredictedOilBopd    = EvaluateOilRate(w, share),
                    MarginalOilBopd_Mscfd = MarginalOilRate(w, share),
                });
            }

            return allocations;
        }

        // ─────────────────────────────────────────────────────────────────
        //  4. Economic greedy allocation (maximize incremental value)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Allocates gas by greedy marginal-value ranking (Kanu 1981 approach).
        /// At each incremental step, additional gas is given to the well with the
        /// highest marginal oil rate gain, until budget is exhausted.
        ///
        /// This approximates the "equal incremental production" criterion.
        /// </summary>
        /// <param name="wells">List of well performance curves.</param>
        /// <param name="totalAvailableGasMscfd">Total field gas budget (Mscfd).</param>
        /// <param name="oilPriceUsdPerBbl">Oil price (USD/bbl) for revenue calculation.</param>
        /// <param name="gasCostUsdPerMscf">Cost of lift gas (USD/Mscf).</param>
        /// <param name="stepSizeMscfd">Incremental step size for allocation iterations (Mscfd).</param>
        public static GasAllocationResult OptimizeEconomicAllocation(
            IList<WellPerformanceCurve> wells,
            double totalAvailableGasMscfd,
            double oilPriceUsdPerBbl = 80.0,
            double gasCostUsdPerMscf = 5.0,
            double stepSizeMscfd = 10.0)
        {
            if (wells == null || wells.Count == 0)
                return new GasAllocationResult();

            // Initialize allocation at minimum injection for each well
            var current = wells.ToDictionary(
                w => w.WellId,
                w => new WellAllocation { WellId = w.WellId, AllocatedGasMscfd = w.MinInjectionMscfd });

            double gasUsed = wells.Sum(w => w.MinInjectionMscfd);
            double gasRemaining = totalAvailableGasMscfd - gasUsed;

            var curveMap = wells.ToDictionary(w => w.WellId);

            // Greedy loop: allocate step by step to the well with best marginal value
            while (gasRemaining >= stepSizeMscfd)
            {
                string? bestWellId = null;
                double bestMarginalValue = 0;

                foreach (var w in wells)
                {
                    double curInj = current[w.WellId].AllocatedGasMscfd;
                    if (curInj + stepSizeMscfd > w.MaxInjectionMscfd) continue;

                    double marginalOil = MarginalOilRate(w, curInj) * stepSizeMscfd;
                    double marginalRevenue = marginalOil * oilPriceUsdPerBbl;
                    double marginalCost    = stepSizeMscfd * gasCostUsdPerMscf;
                    double marginalValue   = marginalRevenue - marginalCost;

                    if (marginalValue > bestMarginalValue)
                    {
                        bestMarginalValue = marginalValue;
                        bestWellId = w.WellId;
                    }
                }

                if (bestWellId == null) break;  // No economically positive allocation remains

                current[bestWellId].AllocatedGasMscfd += stepSizeMscfd;
                gasRemaining -= stepSizeMscfd;
            }

            // Compute final production for each well
            foreach (var a in current.Values)
            {
                var curve = curveMap[a.WellId];
                a.PredictedOilBopd      = EvaluateOilRate(curve, a.AllocatedGasMscfd);
                a.MarginalOilBopd_Mscfd = MarginalOilRate(curve, a.AllocatedGasMscfd);
            }

            double totalOil = current.Values.Sum(a => a.PredictedOilBopd);
            double totalGas = current.Values.Sum(a => a.AllocatedGasMscfd);

            return new GasAllocationResult
            {
                WellAllocations          = current.Values.ToList(),
                TotalAllocatedGasMscfd   = totalGas,
                TotalFieldProductionBopd = totalOil,
                UnallocatedGasMscfd      = gasRemaining,
                EstimatedDailyRevenueUsd = totalOil * oilPriceUsdPerBbl
                                           - totalGas * gasCostUsdPerMscf,
                AllocationMethod         = "EconomicGreedy",
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  5. Sensitivity sweep
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Sweeps total field gas allocation from <paramref name="minTotalGasMscfd"/>
        /// to <paramref name="maxTotalGasMscfd"/> and reports total production at each level.
        /// Useful for planning gas compression capacity or identifying diminishing returns.
        /// </summary>
        public static List<AllocationSensitivityPoint> SweepTotalAllocation(
            IList<WellPerformanceCurve> wells,
            double minTotalGasMscfd,
            double maxTotalGasMscfd,
            double oilPriceUsdPerBbl = 80.0,
            double gasCostUsdPerMscf = 5.0,
            int steps = 10)
        {
            steps = Math.Max(2, steps);
            var results = new List<AllocationSensitivityPoint>(steps);
            double stepSize = (maxTotalGasMscfd - minTotalGasMscfd) / (steps - 1);

            for (int i = 0; i < steps; i++)
            {
                double totalGas = minTotalGasMscfd + i * stepSize;
                var alloc = OptimizeEconomicAllocation(wells, totalGas, oilPriceUsdPerBbl, gasCostUsdPerMscf);
                results.Add(new AllocationSensitivityPoint
                {
                    TotalGasAvailableMscfd   = totalGas,
                    TotalFieldProductionBopd = alloc.TotalFieldProductionBopd,
                    EstimatedDailyRevenueUsd = alloc.EstimatedDailyRevenueUsd,
                });
            }

            return results;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result and data types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Gas lift performance curve for one well (exponential model).</summary>
        public sealed class WellPerformanceCurve
        {
            public string WellId             { get; set; } = string.Empty;
            /// <summary>Maximum oil rate (bopd) at unlimited injection.</summary>
            public double QMaxOilBopd        { get; set; }
            /// <summary>Exponential curve parameter 'a' (1/Mscfd).</summary>
            public double CurveParameter     { get; set; }
            public double MinInjectionMscfd  { get; set; }
            public double MaxInjectionMscfd  { get; set; }
        }

        /// <summary>Gas allocation and predicted performance for one well.</summary>
        public sealed class WellAllocation
        {
            public string WellId                { get; set; } = string.Empty;
            public double AllocatedGasMscfd     { get; set; }
            public double PredictedOilBopd      { get; set; }
            /// <summary>Marginal oil gain (bopd) per additional Mscfd at current injection.</summary>
            public double MarginalOilBopd_Mscfd { get; set; }
        }

        /// <summary>Full field gas allocation optimization result.</summary>
        public sealed class GasAllocationResult
        {
            public List<WellAllocation> WellAllocations          { get; set; } = new();
            public double TotalAllocatedGasMscfd                 { get; set; }
            public double TotalFieldProductionBopd               { get; set; }
            public double UnallocatedGasMscfd                    { get; set; }
            public double EstimatedDailyRevenueUsd               { get; set; }
            public string AllocationMethod                       { get; set; } = string.Empty;
        }

        /// <summary>One point in an allocation sensitivity sweep.</summary>
        public sealed class AllocationSensitivityPoint
        {
            public double TotalGasAvailableMscfd    { get; set; }
            public double TotalFieldProductionBopd  { get; set; }
            public double EstimatedDailyRevenueUsd  { get; set; }
        }
    }
}
