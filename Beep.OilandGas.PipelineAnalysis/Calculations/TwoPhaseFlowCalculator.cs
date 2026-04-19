using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Two-phase (gas–liquid) flow analysis in pipelines.
    ///
    /// Supported methods:
    ///  1. Beggs and Brill (1973)  — general inclined two-phase flow; industry standard.
    ///  2. Dukler (1964)           — horizontal two-phase flow; classic correlation.
    ///  3. Mukherjee and Brill (1985) — inclined/directional pipelines.
    ///
    /// Flow-pattern map (Beggs-Brill):
    ///  Segregated (stratified / wavy), Intermittent (slug / plug), Distributed (bubble / mist)
    ///  are determined from the Froude number and input liquid volume fraction.
    ///
    /// Key outputs per method:
    ///  • Pressure gradient (psi/ft)
    ///  • Liquid holdup fraction (H_L)
    ///  • Flow regime
    ///  • Erosional velocity check (API RP 14E)
    ///
    /// Units used throughout:
    ///   Diameter (in), Length (ft), Pressure (psia), Temperature (°R unless noted),
    ///   Flow rate (Mscf/d for gas; bbl/d for liquid), Velocity (ft/s).
    ///
    /// References:
    ///   - Beggs, H.D. and Brill, J.P. (1973) JPT, pp 607-617.
    ///   - Dukler, A.E. et al. (1964) AIChE J. 10(1), pp 44-51.
    ///   - Mukherjee, H. and Brill, J.P. (1985) JPT, pp 1206-1218.
    ///   - API RP 14E (2007) for erosional velocity.
    /// </summary>
    public static class TwoPhaseFlowCalculator
    {
        // ─────────────────────────────────────────────────────────────────
        //  Constants
        // ─────────────────────────────────────────────────────────────────

        private const double G_FT_S2 = 32.174;   // ft/s²
        private const double PSI_PER_FT_WATER = 0.433; // psi/ft for fresh water
        private const double MSCF_TO_SCFD = 1000.0;    // Mscf → scf/d

        // ─────────────────────────────────────────────────────────────────
        //  Flow-pattern enumeration
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Two-phase flow pattern in the pipeline.</summary>
        public enum FlowPattern
        {
            /// <summary>Stratified smooth or wavy flow — liquid and gas separated.</summary>
            Segregated,
            /// <summary>Slug or plug flow — alternating liquid slugs and gas pockets.</summary>
            Intermittent,
            /// <summary>Bubble or mist flow — one phase dispersed in the other.</summary>
            Distributed,
            /// <summary>Transition between segregated and intermittent.</summary>
            Transition
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Two-phase flow analysis result for a single pipeline segment.</summary>
        public sealed class TwoPhaseResult
        {
            /// <summary>Detected flow pattern.</summary>
            public FlowPattern Pattern { get; set; }

            /// <summary>Liquid holdup fraction H_L (0–1); volume fraction of the pipe occupied by liquid.</summary>
            public double LiquidHoldup { get; set; }

            /// <summary>Mixture density (lb/ft³).</summary>
            public double MixtureDensity { get; set; }

            /// <summary>Mixture velocity (ft/s) = v_sg + v_sl.</summary>
            public double MixtureVelocity { get; set; }

            /// <summary>Superficial gas velocity v_sg (ft/s).</summary>
            public double SuperficialGasVelocity { get; set; }

            /// <summary>Superficial liquid velocity v_sl (ft/s).</summary>
            public double SuperficialLiquidVelocity { get; set; }

            /// <summary>Input liquid volume fraction (no-slip holdup) C_L = v_sl / v_m.</summary>
            public double InputLiquidFraction { get; set; }

            /// <summary>Froude number of the mixture.</summary>
            public double FroudeNumber { get; set; }

            /// <summary>Two-phase friction factor.</summary>
            public double FrictionFactor { get; set; }

            /// <summary>Total pressure gradient (psi/ft)  — positive = pressure loss in flow direction.</summary>
            public double TotalPressureGradientPsiPerFt { get; set; }

            /// <summary>Total pressure drop across the segment (psi).</summary>
            public double TotalPressureDropPsi { get; set; }

            /// <summary>Erosional velocity limit per API RP 14E (ft/s).</summary>
            public double ErosionalVelocityLimit { get; set; }

            /// <summary>Whether mixture velocity exceeds the erosional limit.</summary>
            public bool ErosionalVelocityExceeded { get; set; }

            /// <summary>Method used for calculation.</summary>
            public string Method { get; set; } = string.Empty;

            /// <summary>Operational notes or warnings.</summary>
            public List<string> Notes { get; set; } = new();
        }

        // ─────────────────────────────────────────────────────────────────
        //  Beggs-Brill method
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates two-phase pressure drop using the Beggs-Brill (1973) correlation.
        ///
        /// The method applies to inclined pipes including horizontal and vertical.
        /// It determines:
        ///  1. Flow pattern (Segregated, Intermittent, Distributed, Transition)
        ///  2. Liquid holdup H_L (corrected for inclination)
        ///  3. Friction multiplier Φ
        ///  4. Pressure gradient (friction + elevation components)
        ///
        /// Elevation component:  (dP/dL)_el = ρ_m × g × sin(θ) / 144   [psi/ft]
        /// Friction component:   (dP/dL)_f  = f_tp × ρ_n × v_m² / (2 × g × D)
        /// where ρ_n is the no-slip mixture density.
        /// </summary>
        /// <param name="gasMscfd">Gas flow rate (Mscf/d at standard conditions).</param>
        /// <param name="liquidBbld">Liquid flow rate (bbl/d at flowing conditions).</param>
        /// <param name="diameterIn">Pipe inside diameter (in).</param>
        /// <param name="lengthFt">Pipe segment length (ft).</param>
        /// <param name="inclinationDeg">Pipe inclination from horizontal (degrees; positive = uphill).</param>
        /// <param name="pressurePsia">Average operating pressure (psia).</param>
        /// <param name="temperatureF">Average operating temperature (°F).</param>
        /// <param name="gasSg">Gas specific gravity (air = 1).</param>
        /// <param name="liquidDensityLbFt3">Liquid density (lb/ft³); ~50 for crude oil, 62.4 for water.</param>
        /// <param name="zFactor">Gas compressibility factor Z.</param>
        public static TwoPhaseResult BeggsBrill(
            double gasMscfd,
            double liquidBbld,
            double diameterIn,
            double lengthFt,
            double inclinationDeg,
            double pressurePsia,
            double temperatureF,
            double gasSg = 0.65,
            double liquidDensityLbFt3 = 52.0,
            double zFactor = 0.9)
        {
            var result = new TwoPhaseResult { Method = "Beggs-Brill (1973)" };

            double dFt    = diameterIn / 12.0;
            double area   = Math.PI * dFt * dFt / 4.0;  // ft²
            double tR     = temperatureF + 459.67;
            double theta  = inclinationDeg * Math.PI / 180.0;

            // ── Volumetric flow rates at flowing conditions ────────────────
            // Gas: Qg (ft³/s)  using real-gas law
            double qgScfd  = gasMscfd * MSCF_TO_SCFD;
            double qgFt3s  = qgScfd * zFactor * (tR / 520.0) * (14.7 / pressurePsia) / 86400.0;

            // Liquid: Ql (ft³/s)  — 1 bbl = 5.615 ft³
            double qlFt3s  = liquidBbld * 5.615 / 86400.0;

            if (qgFt3s < 1e-15 && qlFt3s < 1e-15)
            {
                result.Notes.Add("Zero flow — no pressure drop.");
                return result;
            }

            // ── Superficial velocities ──────────────────────────────────────
            double vSg = area > 1e-15 ? qgFt3s / area : 0.0;   // ft/s
            double vSl = area > 1e-15 ? qlFt3s / area : 0.0;   // ft/s
            double vM  = vSg + vSl;                              // mixture velocity

            // ── Gas density at flowing conditions ───────────────────────────
            double gasDensity = gasSg * 28.97 * pressurePsia / (zFactor * 10.7316 * tR); // lb/ft³
            // (Ideal gas: ρ = PM/ZRT, M_air=28.97, R=10.7316 psia·ft³/lb-mol·°R)

            // ── No-slip (input) liquid volume fraction C_L ──────────────────
            double cL = vM > 1e-15 ? vSl / vM : 0.0;

            // ── Froude number ───────────────────────────────────────────────
            double froudeM = vM * vM / (G_FT_S2 * dFt);

            result.SuperficialGasVelocity   = Math.Round(vSg, 3);
            result.SuperficialLiquidVelocity = Math.Round(vSl, 3);
            result.MixtureVelocity          = Math.Round(vM, 3);
            result.InputLiquidFraction      = Math.Round(cL, 4);
            result.FroudeNumber             = Math.Round(froudeM, 4);

            // ── Flow pattern determination (Beggs-Brill boundaries) ─────────
            // Pattern limits based on C_L and Froude number
            double L1 = 316.0 * Math.Pow(cL, 0.302);
            double L2 = 0.0009252 * Math.Pow(cL, -2.4684);
            double L3 = 0.10 * Math.Pow(cL, -1.4516);
            double L4 = 0.5 * Math.Pow(cL, -6.738);

            FlowPattern fp;
            if ((cL < 0.01 && froudeM < L1) || (cL >= 0.01 && froudeM < L2))
                fp = FlowPattern.Segregated;
            else if (froudeM >= L2 && froudeM < L3)
                fp = FlowPattern.Transition;
            else if ((cL >= 0.01 && cL < 0.4 && froudeM >= L3 && froudeM < L1) ||
                     (cL >= 0.4 && froudeM >= L3 && froudeM < L4))
                fp = FlowPattern.Intermittent;
            else
                fp = FlowPattern.Distributed;

            result.Pattern = fp;

            // ── Liquid holdup (horizontal, H_L_0) ────────────────────────────
            // Beggs-Brill holdup constants by flow pattern
            double a, b, c;
            switch (fp)
            {
                case FlowPattern.Segregated:
                    a = 0.98; b = 0.4846; c = 0.0868; break;
                case FlowPattern.Intermittent:
                    a = 0.845; b = 0.5351; c = 0.0173; break;
                case FlowPattern.Distributed:
                    a = 1.065; b = 0.5824; c = 0.0609; break;
                default: // Transition — linear interpolation between Segregated and Intermittent
                    a = 0.91; b = 0.51; c = 0.05; break;
            }

            double hL0 = Math.Min(1.0, a * Math.Pow(cL, b) / Math.Pow(froudeM > 1e-12 ? froudeM : 1e-12, c));
            hL0 = Math.Max(cL, hL0);  // Holdup must be ≥ no-slip fraction

            // ── Inclination correction (Beggs-Brill) ─────────────────────────
            double sinTheta = Math.Sin(theta);
            double C = 0.0;
            if (sinTheta > 0 && fp != FlowPattern.Distributed)  // uphill, not mist
            {
                double d1, e1, f1, g1;
                switch (fp)
                {
                    case FlowPattern.Segregated:
                        d1 = 0.011; e1 = -3.768; f1 = 3.539; g1 = -1.614; break;
                    case FlowPattern.Intermittent:
                        d1 = 2.96; e1 = 0.305; f1 = -0.4473; g1 = 0.0978; break;
                    default:
                        d1 = 0.011; e1 = -3.768; f1 = 3.539; g1 = -1.614; break;
                }
                double nfvL = vSl * Math.Pow(liquidDensityLbFt3 / (G_FT_S2 * 62.4), 0.25);
                C = Math.Max(0, (1 - cL) * Math.Log(d1 * Math.Pow(nfvL, e1) * Math.Pow(cL, f1) * Math.Pow(froudeM, g1)));
            }
            else if (sinTheta < 0)  // downhill
            {
                C = (1 - cL) * Math.Log(4.70 * Math.Pow(froudeM, 0.3692) * Math.Pow(Math.Abs(cL), 0.1244));
                C = -Math.Abs(C);
            }

            double psi = 1 + C * (Math.Sin(1.8 * theta) - (1.0 / 3.0) * Math.Sin(1.8 * theta) * Math.Sin(1.8 * theta) * Math.Sin(1.8 * theta));
            double hL = Math.Min(1.0, Math.Max(cL, hL0 * psi));
            result.LiquidHoldup = Math.Round(hL, 4);

            // ── Mixture density ───────────────────────────────────────────────
            double rhoM = hL * liquidDensityLbFt3 + (1 - hL) * gasDensity;
            double rhoN = cL  * liquidDensityLbFt3 + (1 - cL)  * gasDensity;  // no-slip
            result.MixtureDensity = Math.Round(rhoM, 3);

            // ── Moody friction factor (single-phase Colebrook-White) ─────────
            double roughnessFt = 0.00015;  // commercial steel default (0.0018 in)
            double relRoughness = roughnessFt / dFt;
            double viscosityMix = 0.02;  // cp — approximate gas-dominated
            double re = rhoN * vM * dFt / (viscosityMix * 6.72e-4);
            double fN = ColebrookFriction(re, relRoughness);

            // ── Two-phase friction factor multiplier (Beggs-Brill) ───────────
            double y = cL > 0 && hL > 0 ? Math.Log(cL / (hL * hL)) : 0;
            double s;
            if (y >= -1.0 && y <= 1.2)
                s = Math.Log(2.2 * y - 1.2);
            else
            {
                double denom = -0.0523 + 3.182 * y - 0.8725 * y * y + 0.01853 * y * y * y * y;
                s = Math.Abs(denom) < 1e-10 ? 0 : y / denom;
            }
            double fTp = fN * Math.Exp(s);
            result.FrictionFactor = Math.Round(fTp, 6);

            // ── Pressure gradients ───────────────────────────────────────────
            // Friction: (dP/dL)_f = fTp * ρ_n * v_m² / (2 g_c D)   [lbf/ft² per ft]
            double dpFrictionPsfFt = fTp * rhoN * vM * vM / (2.0 * G_FT_S2 * dFt);
            double dpFrictionPsiPerFt = dpFrictionPsfFt / 144.0;

            // Elevation: (dP/dL)_el = ρ_m g sin(θ) / g_c   [lbf/ft² per ft]
            double dpElevPsfFt = rhoM * G_FT_S2 * sinTheta / G_FT_S2;  // = ρ_m sin(θ)
            double dpElevPsiPerFt = dpElevPsfFt / 144.0;

            double totalGrad = dpFrictionPsiPerFt + dpElevPsiPerFt;
            result.TotalPressureGradientPsiPerFt = Math.Round(totalGrad, 6);
            result.TotalPressureDropPsi = Math.Round(totalGrad * lengthFt, 3);

            // ── Erosional velocity check (API RP 14E) ────────────────────────
            double Ce = 100.0;  // conservative; 150–300 for clean/inhibited service
            double erosVel = Ce / Math.Sqrt(rhoM);
            result.ErosionalVelocityLimit = Math.Round(erosVel, 2);
            result.ErosionalVelocityExceeded = vM > erosVel;
            if (result.ErosionalVelocityExceeded)
                result.Notes.Add($"WARNING: Mixture velocity {vM:F1} ft/s exceeds erosional limit {erosVel:F1} ft/s (API RP 14E). Consider larger diameter or flow rate reduction.");

            if (hL > 0.70 && fp == FlowPattern.Intermittent)
                result.Notes.Add("High liquid holdup in slug/intermittent regime — risk of severe slugging. Consider slug catcher sizing.");

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Multi-segment pipeline
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates two-phase flow along a multi-segment pipeline with varying
        /// inclination and diameter, marching from inlet to outlet.
        ///
        /// Pressure at each segment inlet is the previous segment's outlet pressure.
        /// Gas volumetric flow rate is recomputed per segment using the local pressure.
        /// </summary>
        /// <param name="gasMscfd">Gas rate (Mscfd).</param>
        /// <param name="liquidBbld">Liquid rate (bbl/d).</param>
        /// <param name="segments">Ordered pipeline segment properties from inlet to outlet.</param>
        /// <param name="inletPressurePsia">Inlet pressure (psia).</param>
        /// <param name="temperatureF">Average temperature (°F; assumed uniform).</param>
        /// <param name="gasSg">Gas specific gravity.</param>
        /// <param name="liquidDensityLbFt3">Liquid density (lb/ft³).</param>
        /// <param name="zFactor">Gas Z-factor (assumed constant).</param>
        public static List<(PIPELINE_PROPERTIES Segment, TwoPhaseResult Result, double InletPressure, double OutletPressure)>
            AnalyzeMultiSegment(
                double gasMscfd,
                double liquidBbld,
                IEnumerable<PIPELINE_PROPERTIES> segments,
                double inletPressurePsia,
                double temperatureF = 100.0,
                double gasSg = 0.65,
                double liquidDensityLbFt3 = 52.0,
                double zFactor = 0.9)
        {
            var results = new List<(PIPELINE_PROPERTIES, TwoPhaseResult, double, double)>();
            double pIn = inletPressurePsia;

            foreach (var seg in segments)
            {
                double dIn    = (double)(seg.DIAMETER > 0 ? seg.DIAMETER : 6m);   // inches
                double lenFt  = (double)(seg.LENGTH > 0 ? seg.LENGTH : 0m);       // feet
                double elevFt = (double)seg.ELEVATION_CHANGE;
                double inclDeg = lenFt > 0
                    ? Math.Asin(Math.Max(-1.0, Math.Min(1.0, elevFt / lenFt))) * 180.0 / Math.PI
                    : 0.0;

                var segResult = BeggsBrill(gasMscfd, liquidBbld, dIn, lenFt, inclDeg,
                    pIn, temperatureF, gasSg, liquidDensityLbFt3, zFactor);

                double pOut = Math.Max(14.7, pIn - segResult.TotalPressureDropPsi);
                results.Add((seg, segResult, Math.Round(pIn, 2), Math.Round(pOut, 2)));
                pIn = pOut;
            }

            return results;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Dukler horizontal two-phase
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates pressure drop for horizontal two-phase flow using the Dukler (1964) method.
        ///
        /// Dukler uses the concept of a "friction factor ratio" based on the Lockhart-Martinelli
        /// parameter X_tt to correlate two-phase pressure drop to single-phase values.
        ///
        /// X_tt = (q_l/q_g)^0.9 × (ρ_g/ρ_l)^0.5 × (μ_l/μ_g)^0.1  [Lockhart-Martinelli]
        /// φ_g² = 1 + C/X + 1/X²   (gas-phase multiplier, C = 20 for turbulent-turbulent)
        /// </summary>
        /// <param name="gasMscfd">Gas rate (Mscfd).</param>
        /// <param name="liquidBbld">Liquid rate (bbl/d).</param>
        /// <param name="diameterIn">Pipe ID (in).</param>
        /// <param name="lengthFt">Segment length (ft).</param>
        /// <param name="pressurePsia">Average pressure (psia).</param>
        /// <param name="temperatureF">Temperature (°F).</param>
        /// <param name="gasSg">Gas SG.</param>
        /// <param name="liquidDensityLbFt3">Liquid density (lb/ft³).</param>
        /// <param name="liquidViscosityCp">Liquid viscosity (cp).</param>
        /// <param name="gasViscosityCp">Gas viscosity (cp).</param>
        /// <param name="zFactor">Z-factor.</param>
        public static TwoPhaseResult Dukler(
            double gasMscfd,
            double liquidBbld,
            double diameterIn,
            double lengthFt,
            double pressurePsia,
            double temperatureF = 100.0,
            double gasSg = 0.65,
            double liquidDensityLbFt3 = 52.0,
            double liquidViscosityCp = 2.0,
            double gasViscosityCp = 0.015,
            double zFactor = 0.9)
        {
            var result = new TwoPhaseResult { Method = "Dukler (1964)" };

            double dFt   = diameterIn / 12.0;
            double area  = Math.PI * dFt * dFt / 4.0;
            double tR    = temperatureF + 459.67;

            double qgFt3s = gasMscfd * MSCF_TO_SCFD * zFactor * (tR / 520.0) * (14.7 / pressurePsia) / 86400.0;
            double qlFt3s = liquidBbld * 5.615 / 86400.0;

            double gasDensity = gasSg * 28.97 * pressurePsia / (zFactor * 10.7316 * tR);

            double vSg = area > 0 ? qgFt3s / area : 0;
            double vSl = area > 0 ? qlFt3s / area : 0;
            double vM  = vSg + vSl;
            double cL  = vM > 1e-15 ? vSl / vM : 0;

            // Lockhart-Martinelli parameter X_tt
            double flowRatio = qlFt3s > 1e-15 ? qlFt3s / Math.Max(qgFt3s, 1e-15) : 0;
            double xtt = Math.Pow(flowRatio, 0.9) *
                         Math.Pow(gasDensity / liquidDensityLbFt3, 0.5) *
                         Math.Pow(liquidViscosityCp / Math.Max(gasViscosityCp, 1e-6), 0.1);

            // Gas-phase friction multiplier φ_g²  (C=20 turbulent-turbulent)
            double phiG2 = 1 + 20.0 / xtt + 1.0 / (xtt * xtt);

            // Single-phase gas pressure drop (Darcy-Weisbach)
            double reG = gasDensity * vSg * dFt / (gasViscosityCp * 6.72e-4);
            double fG  = ColebrookFriction(reG, 0.0018 / (diameterIn)); // relative roughness
            double dpGPsiPerFt = fG * gasDensity * vSg * vSg / (2.0 * G_FT_S2 * dFt) / 144.0;

            double totalGrad = dpGPsiPerFt * phiG2;
            double hL = Math.Max(cL, 1.0 / (1 + 1.0 / Math.Max(xtt, 1e-6)));  // approximate

            result.SuperficialGasVelocity    = Math.Round(vSg, 3);
            result.SuperficialLiquidVelocity = Math.Round(vSl, 3);
            result.MixtureVelocity           = Math.Round(vM, 3);
            result.InputLiquidFraction       = Math.Round(cL, 4);
            result.LiquidHoldup              = Math.Round(hL, 4);
            result.FrictionFactor            = Math.Round(fG * phiG2, 6);
            result.MixtureDensity            = Math.Round(hL * liquidDensityLbFt3 + (1 - hL) * gasDensity, 3);
            result.TotalPressureGradientPsiPerFt = Math.Round(totalGrad, 6);
            result.TotalPressureDropPsi      = Math.Round(totalGrad * lengthFt, 3);

            double erosVel = 100.0 / Math.Sqrt(result.MixtureDensity);
            result.ErosionalVelocityLimit    = Math.Round(erosVel, 2);
            result.ErosionalVelocityExceeded = vM > erosVel;

            result.Pattern = xtt > 10 ? FlowPattern.Segregated
                           : xtt > 1  ? FlowPattern.Intermittent
                                       : FlowPattern.Distributed;
            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Colebrook-White friction factor (private helper)
        // ─────────────────────────────────────────────────────────────────

        private static double ColebrookFriction(double re, double relRoughness)
        {
            if (re < 2300) return 64.0 / Math.Max(re, 1.0);  // laminar
            // Initial estimate: Swamee-Jain
            double f = 0.25 / Math.Pow(Math.Log10(relRoughness / 3.7 + 5.74 / Math.Pow(re, 0.9)), 2);
            // Iterate Colebrook
            for (int i = 0; i < 50; i++)
            {
                double rhs = -2.0 * Math.Log10(relRoughness / 3.7 + 2.51 / (re * Math.Sqrt(f)));
                double fNew = 1.0 / (rhs * rhs);
                if (Math.Abs(fNew - f) < 1e-10) break;
                f = fNew;
            }
            return f;
        }
    }
}
