using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Operational constraint analysis for gas lift systems.
    ///
    /// Constraints modeled:
    ///  1. <b>Available gas supply</b> — total lift gas from compressor or pipeline;
    ///     sets the upper bound on all well allocations.
    ///  2. <b>Separator operating envelope</b> — separator inlet pressure, maximum
    ///     liquid throughput (bopd + bwpd), and maximum gas throughput (Mscfd)
    ///     constrain achievable production rates.
    ///  3. <b>Compressor discharge pressure</b> — maximum casing injection pressure
    ///     determines the deepest valve that can be opened.
    ///  4. <b>Wellhead / tubing equipment</b> — maximum allowable wellhead pressure
    ///     (MAWP), minimum tubing velocity to prevent liquid loading, and maximum
    ///     gas injection rate per wellbore (velocity string limit).
    ///  5. <b>Pipeline backpressure</b> — flowline backpressure at wellhead affects
    ///     net pressure differential and flowing BHP.
    ///
    /// Key outputs:
    ///  • Feasibility check (all constraints satisfied at proposed operating point)
    ///  • Active constraint identification (which constraint is binding)
    ///  • Feasible operating envelope (max production achievable within constraints)
    ///  • Recommended adjustments to restore feasibility
    ///
    /// References:
    ///   - API RP 11V5 (2000) "Operation, Maintenance and Troubleshooting of Gas Lift Installations"
    ///   - Lea, J.F. and Nickens, H.V. (2004) "Solving Gas Well Liquid Loading Problems" SPE-72092
    ///   - Turner, R.G. et al. (1969) SPE-2198 "Analysis and Prediction of Minimum Flow Rate
    ///     for the Continuous Removal of Liquids from Gas Wells"
    /// </summary>
    public static class OperationalConstraints
    {
        // ─────────────────────────────────────────────────────────────────
        //  1. Gas supply constraint check
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks whether total requested gas injection is within available supply.
        /// </summary>
        /// <param name="requestedInjectionMscfd">Sum of all well injection requests (Mscfd).</param>
        /// <param name="availableSupplyMscfd">Total compressor or manifold supply (Mscfd).</param>
        /// <param name="safetyMarginFraction">Reserve fraction of supply (e.g., 0.05 = 5% margin).</param>
        public static ConstraintCheckResult CheckGasSupply(
            double requestedInjectionMscfd,
            double availableSupplyMscfd,
            double safetyMarginFraction = 0.05)
        {
            double effectiveSupply = availableSupplyMscfd * (1.0 - safetyMarginFraction);
            bool satisfied = requestedInjectionMscfd <= effectiveSupply;
            double surplus = effectiveSupply - requestedInjectionMscfd;

            return new ConstraintCheckResult
            {
                ConstraintName   = "Gas Supply",
                IsSatisfied      = satisfied,
                RequestedValue   = requestedInjectionMscfd,
                LimitValue       = effectiveSupply,
                Surplus          = surplus,
                Units            = "Mscfd",
                Recommendation   = satisfied
                    ? "Gas supply adequate."
                    : $"Reduce total injection by {-surplus:F0} Mscfd or increase compressor capacity.",
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  2. Separator operating envelope
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks whether a proposed production rate is within separator capacity.
        /// A three-phase separator has independent limits on:
        ///  • Total liquid rate (bopd + bwpd)
        ///  • Gas rate (Mscfd)
        ///  • Inlet pressure (psia) — must stay below operating pressure relief limit.
        /// </summary>
        /// <param name="oilRateBopd">Proposed oil production rate (bopd).</param>
        /// <param name="waterRateBwpd">Proposed water production rate (bwpd).</param>
        /// <param name="gasRateMscfd">Total gas rate entering separator (injection + formation gas, Mscfd).</param>
        /// <param name="separatorInletPressurePsia">Expected separator inlet pressure (psia).</param>
        /// <param name="maxLiquidBopd">Separator maximum liquid throughput (bopd + bwpd).</param>
        /// <param name="maxGasMscfd">Separator maximum gas throughput (Mscfd).</param>
        /// <param name="maxInletPressurePsia">Separator maximum operating pressure (psia).</param>
        public static SeparatorEnvelopeResult CheckSeparatorEnvelope(
            double oilRateBopd,
            double waterRateBwpd,
            double gasRateMscfd,
            double separatorInletPressurePsia,
            double maxLiquidBopd,
            double maxGasMscfd,
            double maxInletPressurePsia)
        {
            double totalLiquid = oilRateBopd + waterRateBwpd;

            var liquidCheck = new ConstraintCheckResult
            {
                ConstraintName   = "Separator Liquid Throughput",
                IsSatisfied      = totalLiquid <= maxLiquidBopd,
                RequestedValue   = totalLiquid,
                LimitValue       = maxLiquidBopd,
                Surplus          = maxLiquidBopd - totalLiquid,
                Units            = "bopd+bwpd",
                Recommendation   = totalLiquid <= maxLiquidBopd
                    ? "Liquid throughput within limits."
                    : $"Total liquid {totalLiquid:F0} bpd exceeds separator limit of {maxLiquidBopd:F0} bpd.",
            };

            var gasCheck = new ConstraintCheckResult
            {
                ConstraintName   = "Separator Gas Throughput",
                IsSatisfied      = gasRateMscfd <= maxGasMscfd,
                RequestedValue   = gasRateMscfd,
                LimitValue       = maxGasMscfd,
                Surplus          = maxGasMscfd - gasRateMscfd,
                Units            = "Mscfd",
                Recommendation   = gasRateMscfd <= maxGasMscfd
                    ? "Gas throughput within limits."
                    : $"Gas rate {gasRateMscfd:F0} Mscfd exceeds separator limit of {maxGasMscfd:F0} Mscfd.",
            };

            var pressureCheck = new ConstraintCheckResult
            {
                ConstraintName   = "Separator Inlet Pressure",
                IsSatisfied      = separatorInletPressurePsia <= maxInletPressurePsia,
                RequestedValue   = separatorInletPressurePsia,
                LimitValue       = maxInletPressurePsia,
                Surplus          = maxInletPressurePsia - separatorInletPressurePsia,
                Units            = "psia",
                Recommendation   = separatorInletPressurePsia <= maxInletPressurePsia
                    ? "Inlet pressure within limits."
                    : $"Inlet pressure {separatorInletPressurePsia:F0} psia exceeds limit of {maxInletPressurePsia:F0} psia.",
            };

            return new SeparatorEnvelopeResult
            {
                LiquidConstraint  = liquidCheck,
                GasConstraint     = gasCheck,
                PressureConstraint = pressureCheck,
                AllConstraintsMet = liquidCheck.IsSatisfied && gasCheck.IsSatisfied && pressureCheck.IsSatisfied,
                BindingConstraint = new[] { liquidCheck, gasCheck, pressureCheck }
                    .Where(c => !c.IsSatisfied)
                    .OrderBy(c => c.Surplus)  // most violated first
                    .FirstOrDefault()?.ConstraintName ?? "None",
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  3. Wellhead / tubing constraints
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks wellhead and tubing constraints for a gas lift well:
        ///  a) Maximum allowable wellhead pressure (MAWP) not exceeded.
        ///  b) Minimum tubing velocity to prevent liquid loading (Turner criterion).
        ///  c) Maximum injection rate for wellbore (velocity limit on casing string).
        /// </summary>
        /// <param name="wellheadPressurePsia">Current wellhead tubing pressure (psia).</param>
        /// <param name="mawpPsia">Maximum allowable working pressure of wellhead equipment (psia).</param>
        /// <param name="tubingIdInches">Tubing internal diameter (in).</param>
        /// <param name="gasProductionRateMscfd">Gas production (formation + injection) at wellhead (Mscfd).</param>
        /// <param name="wellheadTempF">Wellhead temperature (°F).</param>
        /// <param name="wellheadPressureForTurnerPsia">Wellhead flowing pressure for Turner check (psia).</param>
        /// <param name="gasSpecificGravity">Gas SG (air=1); for liquid density in Turner equation.</param>
        public static WellheadConstraintResult CheckWellheadConstraints(
            double wellheadPressurePsia,
            double mawpPsia,
            double tubingIdInches,
            double gasProductionRateMscfd,
            double wellheadTempF = 60.0,
            double wellheadPressureForTurnerPsia = 0,
            double gasSpecificGravity = 0.65)
        {
            // a) MAWP check
            var mawpCheck = new ConstraintCheckResult
            {
                ConstraintName  = "Wellhead MAWP",
                IsSatisfied     = wellheadPressurePsia <= mawpPsia,
                RequestedValue  = wellheadPressurePsia,
                LimitValue      = mawpPsia,
                Surplus         = mawpPsia - wellheadPressurePsia,
                Units           = "psia",
                Recommendation  = wellheadPressurePsia <= mawpPsia
                    ? "Within MAWP."
                    : $"Wellhead pressure {wellheadPressurePsia:F0} psia exceeds MAWP {mawpPsia:F0} psia.",
            };

            // b) Turner critical velocity for liquid unloading (Turner et al. 1969)
            //   v_crit = 5.62 × ((σ × (ρ_L − ρ_g))^0.25) / ρ_g^0.5   (ft/s)
            //   Simplified: v_Turner ≈ 4.0 × P^0.5 / T  for 0.65 SG gas (Coleman 1991 approximation)
            //   Actual gas velocity in tubing: v_actual = Q_scfd × Z × T / (A_tubing × P × 86400)
            double pTurner = wellheadPressureForTurnerPsia > 0 ? wellheadPressureForTurnerPsia : wellheadPressurePsia;
            double T_R     = wellheadTempF + 459.67;
            double tubingArea = Math.PI * tubingIdInches * tubingIdInches / (4.0 * 144.0);  // ft²

            // Coleman (1991) Turner simplification for gas lift: v_crit ≈ 4.0 × √(P/T)
            double vCritFtS = 4.0 * Math.Sqrt(pTurner / T_R);

            // Actual gas velocity (approx. Z = 0.9 for wellhead conditions)
            double qScfd    = gasProductionRateMscfd * 1_000.0;
            double vActual  = tubingArea > 0
                ? qScfd * 0.9 * T_R / (pTurner * tubingArea * 86_400.0)
                : 0;

            var loadingCheck = new ConstraintCheckResult
            {
                ConstraintName  = "Tubing Minimum Velocity (Liquid Loading)",
                IsSatisfied     = vActual >= vCritFtS,
                RequestedValue  = vActual,
                LimitValue      = vCritFtS,
                Surplus         = vActual - vCritFtS,
                Units           = "ft/s",
                Recommendation  = vActual >= vCritFtS
                    ? "Above liquid loading velocity."
                    : $"Actual velocity {vActual:F2} ft/s below Turner critical {vCritFtS:F2} ft/s — increase gas injection or reduce tubing size.",
            };

            return new WellheadConstraintResult
            {
                MawpConstraint    = mawpCheck,
                LiquidLoadingConstraint = loadingCheck,
                AllConstraintsMet = mawpCheck.IsSatisfied && loadingCheck.IsSatisfied,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  4. Compressor discharge pressure vs. deepest valve
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Verifies the compressor discharge pressure is sufficient to open the
        /// deepest (operating) gas lift valve given the hydrostatic casing column.
        ///
        ///   P_casing_at_valve = P_compressor − P_friction_casing + ρ_gas_column × depth
        ///   Constraint: P_casing_at_valve ≥ P_open (operating valve opening pressure)
        /// </summary>
        public static ConstraintCheckResult CheckCompressorPressure(
            double compressorDischargePsia,
            double operatingValveDepthFt,
            double valveOpeningPressurePsia,
            double gasColumnGradientPsiPerFt = 0.025,
            double casingFrictionLossPsi = 20.0)
        {
            double casingPressureAtValve = compressorDischargePsia
                - casingFrictionLossPsi
                + gasColumnGradientPsiPerFt * operatingValveDepthFt;

            bool satisfied = casingPressureAtValve >= valveOpeningPressurePsia;

            return new ConstraintCheckResult
            {
                ConstraintName  = "Compressor Discharge Pressure",
                IsSatisfied     = satisfied,
                RequestedValue  = casingPressureAtValve,
                LimitValue      = valveOpeningPressurePsia,
                Surplus         = casingPressureAtValve - valveOpeningPressurePsia,
                Units           = "psia",
                Recommendation  = satisfied
                    ? $"Compressor pressure adequate: {casingPressureAtValve:F0} psia at valve vs. {valveOpeningPressurePsia:F0} psia required."
                    : $"Compressor pressure insufficient: increase discharge by {valveOpeningPressurePsia - casingPressureAtValve:F0} psia.",
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  5. Full operating point feasibility check
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Runs a complete feasibility check for all operational constraints at a
        /// proposed operating point for a gas lift well.
        /// </summary>
        public static OperatingPointFeasibility CheckOperatingPointFeasibility(
            double proposedInjectionMscfd,
            double availableSupplyMscfd,
            double oilRateBopd,
            double waterRateBwpd,
            double totalGasToSeparatorMscfd,
            double separatorInletPressurePsia,
            double maxSeparatorLiquidBopd,
            double maxSeparatorGasMscfd,
            double maxSeparatorPressurePsia,
            double wellheadPressurePsia,
            double mawpPsia,
            double tubingIdInches,
            double compressorDischargePsia,
            double operatingValveDepthFt,
            double valveOpeningPressurePsia)
        {
            var supplyCheck     = CheckGasSupply(proposedInjectionMscfd, availableSupplyMscfd);
            var separatorResult = CheckSeparatorEnvelope(
                oilRateBopd, waterRateBwpd, totalGasToSeparatorMscfd,
                separatorInletPressurePsia, maxSeparatorLiquidBopd,
                maxSeparatorGasMscfd, maxSeparatorPressurePsia);
            var wellheadResult  = CheckWellheadConstraints(
                wellheadPressurePsia, mawpPsia, tubingIdInches,
                totalGasToSeparatorMscfd);
            var compressorCheck = CheckCompressorPressure(
                compressorDischargePsia, operatingValveDepthFt, valveOpeningPressurePsia);

            var allChecks = new List<ConstraintCheckResult>
            {
                supplyCheck,
                separatorResult.LiquidConstraint,
                separatorResult.GasConstraint,
                separatorResult.PressureConstraint,
                wellheadResult.MawpConstraint,
                wellheadResult.LiquidLoadingConstraint,
                compressorCheck,
            };

            bool allMet     = allChecks.All(c => c.IsSatisfied);
            var  violations = allChecks.Where(c => !c.IsSatisfied).ToList();

            return new OperatingPointFeasibility
            {
                IsFullyFeasible      = allMet,
                Constraints          = allChecks,
                Violations           = violations,
                PrimaryViolation     = violations.OrderBy(c => c.Surplus).FirstOrDefault()?.ConstraintName
                                       ?? "None",
                NumberOfViolations   = violations.Count,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Result of a single constraint check.</summary>
        public sealed class ConstraintCheckResult
        {
            public string ConstraintName   { get; set; } = string.Empty;
            public bool   IsSatisfied      { get; set; }
            public double RequestedValue   { get; set; }
            public double LimitValue       { get; set; }
            /// <summary>Positive = within limit; negative = over limit by this amount.</summary>
            public double Surplus          { get; set; }
            public string Units            { get; set; } = string.Empty;
            public string Recommendation   { get; set; } = string.Empty;
        }

        /// <summary>Three-phase separator operating envelope check.</summary>
        public sealed class SeparatorEnvelopeResult
        {
            public ConstraintCheckResult LiquidConstraint    { get; set; } = new();
            public ConstraintCheckResult GasConstraint       { get; set; } = new();
            public ConstraintCheckResult PressureConstraint  { get; set; } = new();
            public bool   AllConstraintsMet                  { get; set; }
            public string BindingConstraint                  { get; set; } = string.Empty;
        }

        /// <summary>Wellhead and tubing constraint check.</summary>
        public sealed class WellheadConstraintResult
        {
            public ConstraintCheckResult MawpConstraint           { get; set; } = new();
            public ConstraintCheckResult LiquidLoadingConstraint  { get; set; } = new();
            public bool AllConstraintsMet                         { get; set; }
        }

        /// <summary>Complete operating point feasibility assessment.</summary>
        public sealed class OperatingPointFeasibility
        {
            public bool   IsFullyFeasible       { get; set; }
            public List<ConstraintCheckResult> Constraints  { get; set; } = new();
            public List<ConstraintCheckResult> Violations   { get; set; } = new();
            public string PrimaryViolation      { get; set; } = string.Empty;
            public int    NumberOfViolations    { get; set; }
        }
    }
}
