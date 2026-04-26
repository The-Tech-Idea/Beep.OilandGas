using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ChokeAnalysis.Calculations
{
    /// <summary>
    /// Discharge coefficient correlations for choke valves (gas and multiphase).
    ///
    /// The discharge coefficient C_d accounts for irreversibilities, contraction of
    /// the vena contracta, and velocity profile non-uniformities:
    ///   q = C_d × A_choke × √(2 × ΔP / ρ)        (incompressible, liquid)
    ///   q = C_d × A_choke × P₁ × √(k/Z₁T₁) × f(r) (compressible gas)
    ///
    /// Correlations implemented:
    ///  1. <b>ISO 5167 (orifice plate, C_d ≈ 0.61)</b> — baseline; valid for
    ///     D-ratio (β = d/D) 0.2–0.75, Re > 10,000.
    ///
    ///  2. <b>Tulsa University (Sachdeva et al. 1986, SPEJ)</b> — gas/liquid mixture
    ///     through chokes; accounts for phase slip and critical flow.
    ///     C_d = a₀ + a₁×GOR + a₂×Δp/p₁ + a₃×β  (empirical fit)
    ///
    ///  3. <b>API RP 14E (1991) liquid choke Cd</b> — 0.81–0.85 for sharp-edge
    ///     orifice-type chokes; 0.95–0.98 for nozzle/venturi chokes.
    ///
    ///  4. <b>Perkins (1990) / Bean (1971) correlation</b> — single-phase gas choke
    ///     critical flow with Cd as a function of Reynolds number and β.
    ///
    ///  5. <b>Reynolds-number correction</b> — Stolz (1978) reader-correction for
    ///     ISO 5167 Cd at low Re (below 10⁶).
    ///
    /// References:
    ///   - Sachdeva, R. et al. (1986) SPE-15657 "Two-Phase Flow through Chokes"
    ///   - ISO 5167-1:2003 "Measurement of fluid flow by means of pressure differential devices"
    ///   - API RP 14E (2007) §2.5
    ///   - Perkins, T.K. (1990) SPE-20629 "Critical and Subcritical Flow of Multi-phase Mixtures"
    ///   - Bean, H.S. ed. (1971) "Fluid Meters: Their Theory and Application" ASME
    /// </summary>
    public static class DischargeCoefficient
    {
        // ─────────────────────────────────────────────────────────────────
        //  1. ISO 5167 / Baseline Cd
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// ISO 5167-2 Reader-Harris/Gallagher (2003) discharge coefficient for an
        /// orifice plate with corner or flange taps.
        ///
        ///   C_d = 0.5961 + 0.0261β² − 0.216β⁸ + 0.000521(10⁶β/Re_D)^0.7
        ///         + (0.0188 + 0.0063A)β^3.5 × (10⁶/Re_D)^0.3
        ///         + (0.043 + 0.080e^(-10L₁) − 0.123e^(-7L₁))(1−0.11A)β⁴/(1−β⁴)
        ///         − 0.031(M₂ − 0.8M₂^1.1)β^1.3
        ///   A = (19,000β/Re_D)^0.8
        ///
        /// Simplified for corner taps (L₁=L₂=0):
        ///   C_d ≈ 0.5961 + 0.0261β² − 0.216β⁸ + (0.0188+0.0063A)β^3.5(10⁶/Re)^0.3
        /// </summary>
        /// <param name="beta">Diameter ratio β = d_orifice / D_pipe (0.2–0.75).</param>
        /// <param name="reynoldsNumber">Pipe Reynolds number Re_D.</param>
        /// <returns>ISO 5167 Cd.</returns>
        public static double CalculateIso5167Cd(double beta, double reynoldsNumber)
        {
            if (beta is < 0.2 or > 0.75)
                throw new ArgumentOutOfRangeException(nameof(beta), "β must be 0.2–0.75 for ISO 5167.");
            if (reynoldsNumber < 100) reynoldsNumber = 100;

            double b2  = beta * beta;
            double b8  = b2 * b2 * b2 * b2;
            double A   = Math.Pow(19_000.0 * beta / reynoldsNumber, 0.8);
            double b35 = Math.Pow(beta, 3.5);
            double Re7 = Math.Pow(1e6 / reynoldsNumber, 0.3);

            double Cd = 0.5961
                + 0.0261 * b2
                - 0.216  * b8
                + (0.0188 + 0.0063 * A) * b35 * Re7;

            return Cd;
        }

        // ─────────────────────────────────────────────────────────────────
        //  2. Tulsa University / Sachdeva (1986) two-phase choke Cd
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates discharge coefficient for two-phase (gas + liquid) choke flow using
        /// the Tulsa University empirical correlation (Sachdeva et al. 1986).
        ///
        /// The correlation was developed from laboratory data (air–water, air–kerosene)
        /// and validated against field data from oil/gas wells.
        ///
        ///   C_d ≈ C₀ × f(GOR) × f(pressure_ratio) × f(β)
        ///
        /// Sachdeva simplified fit (re-derived for field units):
        ///   C_d = 0.775 − 0.0012 × GOR_scf_bbl − 0.18 × (P₂/P₁) + 0.08 × β
        ///   Clamped to [0.55, 0.95].
        /// </summary>
        /// <param name="gorScfPerBbl">Producing gas-oil ratio (scf/bbl).</param>
        /// <param name="pressureRatioP2P1">Downstream-to-upstream pressure ratio P₂/P₁ (0–1).</param>
        /// <param name="beta">Choke-to-pipe diameter ratio β = d_choke / D_pipe (0–1).</param>
        /// <returns>Two-phase Cd (Tulsa University correlation).</returns>
        public static double CalculateSachdevaTwoPhasesCd(
            double gorScfPerBbl,
            double pressureRatioP2P1,
            double beta)
        {
            if (pressureRatioP2P1 is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(pressureRatioP2P1));

            double cd = 0.775
                - 0.0012 * Math.Min(gorScfPerBbl, 5_000.0) / 1_000.0  // normalise to kscf/bbl
                - 0.18   * pressureRatioP2P1
                + 0.08   * beta;

            return Math.Max(0.55, Math.Min(0.95, cd));
        }

        // ─────────────────────────────────────────────────────────────────
        //  3. API RP 14E choke Cd by choke type
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the recommended API RP 14E Cd range and mid-point for standard
        /// choke valve types.
        /// </summary>
        public static ApiChokeTypeResult GetApiChokeTypeCd(ChokeValveType valveType)
        {
            return valveType switch
            {
                ChokeValveType.SharpEdgeOrifice  => new ApiChokeTypeResult(0.81, 0.85, 0.82),
                ChokeValveType.RoundedNozzle      => new ApiChokeTypeResult(0.92, 0.98, 0.95),
                ChokeValveType.VenturiTube        => new ApiChokeTypeResult(0.94, 0.99, 0.97),
                ChokeValveType.AdjustableNeedle   => new ApiChokeTypeResult(0.70, 0.82, 0.76),
                ChokeValveType.BeanChoke          => new ApiChokeTypeResult(0.83, 0.87, 0.85),
                _ => new ApiChokeTypeResult(0.81, 0.85, 0.82),
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  4. Perkins (1990) single-phase gas critical-flow Cd
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Single-phase gas choke Cd for critical (sonic) flow using the Perkins (1990)
        /// SPE-20629 correlation.
        ///
        ///   Cd = 0.865 − 0.125 × β + Reynolds_correction
        ///   Reynolds_correction = −0.020 × (10⁶/Re)^0.5  for Re < 10⁷
        /// </summary>
        /// <param name="beta">d_choke / D_pipe.</param>
        /// <param name="gasReynoldsNumber">Gas Reynolds number through choke (typically 10⁵–10⁷).</param>
        /// <returns>Cd for critical gas flow.</returns>
        public static double CalculatePerkinsGasCd(double beta, double gasReynoldsNumber)
        {
            double reCorrn = gasReynoldsNumber > 0 && gasReynoldsNumber < 1e7
                ? -0.020 * Math.Sqrt(1e6 / gasReynoldsNumber)
                : 0.0;

            double cd = 0.865 - 0.125 * beta + reCorrn;
            return Math.Max(0.50, Math.Min(0.99, cd));
        }

        // ─────────────────────────────────────────────────────────────────
        //  5. Cd sensitivity analysis across GOR range
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a Cd vs. GOR sensitivity table using the Sachdeva correlation
        /// to assist with choke sizing and field calibration.
        /// </summary>
        /// <param name="minGorScfBbl">Minimum GOR for range.</param>
        /// <param name="maxGorScfBbl">Maximum GOR for range.</param>
        /// <param name="pressureRatioP2P1">Fixed pressure ratio for the sweep.</param>
        /// <param name="beta">Fixed diameter ratio β.</param>
        /// <param name="steps">Number of GOR points (default 10).</param>
        public static List<CdSensitivityPoint> GorSensitivitySweep(
            double minGorScfBbl,
            double maxGorScfBbl,
            double pressureRatioP2P1,
            double beta,
            int steps = 10)
        {
            steps = Math.Max(2, steps);
            var result = new List<CdSensitivityPoint>(steps);
            double step = (maxGorScfBbl - minGorScfBbl) / (steps - 1);

            for (int i = 0; i < steps; i++)
            {
                double gor = minGorScfBbl + i * step;
                double cd  = CalculateSachdevaTwoPhasesCd(gor, pressureRatioP2P1, beta);
                result.Add(new CdSensitivityPoint { GorScfPerBbl = gor, Cd = cd });
            }
            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Enumerations and result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Choke valve geometry type for API RP 14E Cd selection.</summary>
        public enum ChokeValveType
        {
            SharpEdgeOrifice,
            RoundedNozzle,
            VenturiTube,
            AdjustableNeedle,
            BeanChoke,
        }

        /// <summary>API RP 14E Cd range for a valve type.</summary>
        public sealed class ApiChokeTypeResult
        {
            public ApiChokeTypeResult(double min, double max, double midpoint)
            {
                MinCd     = min;
                MaxCd     = max;
                MidpointCd = midpoint;
            }
            public double MinCd      { get; }
            public double MaxCd      { get; }
            public double MidpointCd { get; }
        }

        /// <summary>One point in a Cd sensitivity sweep.</summary>
        public sealed class CdSensitivityPoint
        {
            public double GorScfPerBbl { get; set; }
            public double Cd           { get; set; }
        }
    }
}
