using System;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Provides centrifugal compressor power calculations.
    /// </summary>
    public static class CentrifugalCompressorCalculator
    {
        /// <summary>
        /// Calculates centrifugal compressor power requirements.
        /// </summary>
        /// <param name="compressorProperties">Centrifugal compressor properties.</param>
        /// <param name="useSIUnits">Whether to use SI units (false = US field units).</param>
        /// <returns>Compressor power calculation results.</returns>
        public static COMPRESSOR_POWER_RESULT CalculatePower(
            CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
            bool useSIUnits = false)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));

            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));

            var result = new COMPRESSOR_POWER_RESULT();

            var conditions = compressorProperties.OPERATING_CONDITIONS;

            // Calculate compression ratio
            result.COMPRESSION_RATIO = conditions.DISCHARGE_PRESSURE / conditions.SUCTION_PRESSURE;

            // Calculate average pressure and temperature
            decimal averagePressure = (conditions.SUCTION_PRESSURE + conditions.DISCHARGE_PRESSURE) / 2m;
            decimal averageTemperature = (conditions.SUCTION_TEMPERATURE + conditions.DISCHARGE_TEMPERATURE) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, averageTemperature, conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate polytropic head
            result.POLYTROPIC_HEAD = CalculatePolytropicHead(
                conditions.SUCTION_PRESSURE,
                conditions.DISCHARGE_PRESSURE,
                conditions.SUCTION_TEMPERATURE,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                compressorProperties.POLYTROPIC_EFFICIENCY,
                zFactor,
                conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate adiabatic head
            result.ADIABATIC_HEAD = CalculateAdiabaticHead(
                conditions.SUCTION_PRESSURE,
                conditions.DISCHARGE_PRESSURE,
                conditions.SUCTION_TEMPERATURE,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                zFactor,
                conditions.GAS_SPECIFIC_GRAVITY);

            // Calculate theoretical power
            result.THEORETICAL_POWER = CalculateTheoreticalPower(
                conditions.GAS_FLOW_RATE,
                result.POLYTROPIC_HEAD,
                conditions.GAS_SPECIFIC_GRAVITY,
                averageTemperature,
                zFactor,
                averagePressure);

            // Calculate brake horsepower
            result.BRAKE_HORSEPOWER = result.THEORETICAL_POWER / compressorProperties.POLYTROPIC_EFFICIENCY;

            // Calculate motor horsepower
            result.MOTOR_HORSEPOWER = result.BRAKE_HORSEPOWER / conditions.MECHANICAL_EFFICIENCY;

            // Calculate power consumption
            if (useSIUnits)
            {
                result.POWER_CONSUMPTION_KW = result.MOTOR_HORSEPOWER * 0.746m;
            }
            else
            {
                result.POWER_CONSUMPTION_KW = result.MOTOR_HORSEPOWER * 0.746m; // Still in kW for consistency
            }

            // Calculate discharge temperature
            result.DISCHARGE_TEMPERATURE = CalculateDischargeTemperature(
                conditions.SUCTION_TEMPERATURE,
                result.COMPRESSION_RATIO,
                compressorProperties.SPECIFIC_HEAT_RATIO,
                compressorProperties.POLYTROPIC_EFFICIENCY);

            // Overall efficiency
            result.OVERALL_EFFICIENCY = compressorProperties.POLYTROPIC_EFFICIENCY * conditions.MECHANICAL_EFFICIENCY;

            return result;
        }

        /// <summary>
        /// Calculates polytropic head.
        /// </summary>
        private static decimal CalculatePolytropicHead(
            decimal SUCTION_PRESSURE,
            decimal DISCHARGE_PRESSURE,
            decimal SUCTION_TEMPERATURE,
            decimal SPECIFIC_HEAT_RATIO,
            decimal POLYTROPIC_EFFICIENCY,
            decimal zFactor,
            decimal GAS_SPECIFIC_GRAVITY)
        {
            // Polytropic head: Hp = (Z_avg * R * T1 / MW) * (n / (n-1)) * [(P2/P1)^((n-1)/n) - 1]
            // Where n = polytropic exponent = (k * Î·p) / (k - Î·p * (k - 1))

            decimal k = SPECIFIC_HEAT_RATIO;
            decimal etaP = POLYTROPIC_EFFICIENCY;

            // Polytropic exponent
            decimal n = (k * etaP) / (k - etaP * (k - 1m));

            // Compression ratio
            decimal compressionRatio = DISCHARGE_PRESSURE / SUCTION_PRESSURE;

            // Average Z-factor (simplified - using suction Z)
            decimal zAvg = zFactor;

            // Gas constant
            decimal R = 1545.0m; // ft-lbf/(lbmol-R)

            // Molecular weight
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;

            // Polytropic head
            decimal head = (zAvg * R * SUCTION_TEMPERATURE / molecularWeight) *
                          (n / (n - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((n - 1m) / n)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates adiabatic head.
        /// </summary>
        private static decimal CalculateAdiabaticHead(
            decimal SUCTION_PRESSURE,
            decimal DISCHARGE_PRESSURE,
            decimal SUCTION_TEMPERATURE,
            decimal SPECIFIC_HEAT_RATIO,
            decimal zFactor,
            decimal GAS_SPECIFIC_GRAVITY)
        {
            // Adiabatic head: Ha = (Z_avg * R * T1 / MW) * (k / (k-1)) * [(P2/P1)^((k-1)/k) - 1]

            decimal k = SPECIFIC_HEAT_RATIO;
            decimal compressionRatio = DISCHARGE_PRESSURE / SUCTION_PRESSURE;
            decimal zAvg = zFactor;
            decimal R = 1545.0m;
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;

            decimal head = (zAvg * R * SUCTION_TEMPERATURE / molecularWeight) *
                          (k / (k - 1m)) *
                          ((decimal)Math.Pow((double)compressionRatio, (double)((k - 1m) / k)) - 1m);

            return Math.Max(0m, head);
        }

        /// <summary>
        /// Calculates theoretical power.
        /// </summary>
        private static decimal CalculateTheoreticalPower(
            decimal GAS_FLOW_RATE,
            decimal polytropicHead,
            decimal GAS_SPECIFIC_GRAVITY,
            decimal averageTemperature,
            decimal zFactor,
            decimal averagePressure)
        {
            // Theoretical power: P = (W * Hp) / (33000 * Î·)
            // Where W = weight flow rate

            // Convert gas flow rate from Mscf/day to scf/min
            decimal flowRateScfMin = GAS_FLOW_RATE * 1000m / 1440m; // scf/min

            // Calculate weight flow rate
            decimal molecularWeight = GAS_SPECIFIC_GRAVITY * 28.9645m;
            decimal weightFlowRate = flowRateScfMin * molecularWeight / 379.0m; // lbm/min

            // Theoretical power
            decimal theoreticalPower = (weightFlowRate * polytropicHead) / 33000m;

            return Math.Max(0m, theoreticalPower);
        }

        /// <summary>
        /// Calculates discharge temperature.
        /// </summary>
        private static decimal CalculateDischargeTemperature(
            decimal SUCTION_TEMPERATURE,
            decimal compressionRatio,
            decimal SPECIFIC_HEAT_RATIO,
            decimal POLYTROPIC_EFFICIENCY)
        {
            // Discharge temperature: T2 = T1 * (P2/P1)^((n-1)/n)
            // Where n = polytropic exponent

            decimal k = SPECIFIC_HEAT_RATIO;
            decimal etaP = POLYTROPIC_EFFICIENCY;
            decimal n = (k * etaP) / (k - etaP * (k - 1m));

            decimal dischargeTemperature = SUCTION_TEMPERATURE *
                                         (decimal)Math.Pow((double)compressionRatio, (double)((n - 1m) / n));

            return Math.Max(SUCTION_TEMPERATURE, dischargeTemperature);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Performance mapping and surge margin analysis
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the surge margin for a centrifugal compressor.
        ///
        /// Surge is the unstable operating condition that occurs when flow drops below
        /// the minimum stable flow (surge point). The surge margin is the ratio of
        /// actual flow to surge flow — if it falls below 1.10 (10 %), intervention is needed.
        ///
        /// Surge flow is approximated as a fraction of design flow using the inlet
        /// Mach number and the shape of the impeller:
        ///   Q_surge ≈ Q_design · (1 - 0.15 · (1 - η_polytropic))
        ///
        /// Reference: Ludtke, K.H. (2004) "Process Centrifugal Compressors", Springer; Ch. 6.
        /// </summary>
        /// <param name="compressorProperties">Centrifugal compressor properties.</param>
        /// <returns>
        /// Surge margin (dimensionless ratio; values &gt; 1.10 are safe).
        /// Also includes the estimated surge flow rate (Mscf/day) and operating status.
        /// </returns>
        public static (decimal SurgeMargin, decimal SurgeFlowRate, string OperatingStatus)
            CalculateSurgeMargin(CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));
            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));

            var conditions = compressorProperties.OPERATING_CONDITIONS;
            decimal etaP = compressorProperties.POLYTROPIC_EFFICIENCY;
            decimal actualFlow = conditions.GAS_FLOW_RATE;

            // Surge flow estimation: lower efficiency → higher relative surge point
            decimal surgeFlow = actualFlow * (1m - 0.15m * (1m - etaP));
            surgeFlow = Math.Max(surgeFlow, actualFlow * 0.55m);  // floor at 55 % of design

            decimal surgeMargin = actualFlow > 0 ? actualFlow / surgeFlow : 0m;

            string status = surgeMargin >= 1.20m ? "Safe (>20% margin)"
                : surgeMargin >= 1.10m ? "Acceptable (10-20% margin)"
                : surgeMargin >= 1.05m ? "Warning (<10% margin)"
                : "Surge Risk — increase flow or recycle";

            return (Math.Round(surgeMargin, 3), Math.Round(surgeFlow, 1), status);
        }

        /// <summary>
        /// Generates a head-flow performance map (H-Q curve) for the centrifugal compressor
        /// across a range of flow rates from 60 % to 120 % of design flow.
        ///
        /// The polytropic head at off-design conditions is approximated by the fan-law
        /// parabola modified for compressible flow:
        ///   H(Q) = H_design · [1 - α·(Q/Q_design - 1)² + β·(Q/Q_design - 1)]
        ///
        /// Constants α = 0.5 and β = 0 (symmetric drop-off) are typical for industrial
        /// centrifugal gas compressors.
        /// Reference: API 617 (8th ed.), Annex F; Ludtke (2004), Ch. 7.
        /// </summary>
        /// <param name="compressorProperties">Centrifugal compressor properties.</param>
        /// <param name="points">Number of points on the curve (default 10).</param>
        /// <returns>
        /// List of (FlowRate [Mscf/day], PolytropicHead [ft-lbf/lbm], Efficiency [fraction],
        /// CompressionRatio) tuples from 60 % to 120 % of design flow.
        /// </returns>
        public static System.Collections.Generic.List<(decimal FlowRate, decimal Head, decimal Efficiency, decimal CompressionRatio)>
            GeneratePerformanceMap(CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties, int points = 10)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));
            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));

            var result = new System.Collections.Generic.List<(decimal, decimal, decimal, decimal)>();
            var conditions = compressorProperties.OPERATING_CONDITIONS;

            // Design-point result
            var designResult = CalculatePower(compressorProperties);
            decimal qDesign = conditions.GAS_FLOW_RATE;
            decimal hDesign = designResult.POLYTROPIC_HEAD;
            decimal etaDesign = compressorProperties.POLYTROPIC_EFFICIENCY;

            decimal qMin = qDesign * 0.60m;
            decimal qMax = qDesign * 1.20m;
            decimal step = points > 1 ? (qMax - qMin) / (points - 1) : 0;

            for (int i = 0; i < points; i++)
            {
                decimal q = qMin + step * i;
                decimal flowRatio = qDesign > 0 ? q / qDesign : 1m;

                // Head parabola: H = H_design * (1 - 0.5*(fr - 1)^2)
                decimal fr = flowRatio - 1m;
                decimal head = hDesign * (1m - 0.5m * fr * fr);
                head = Math.Max(0m, head);

                // Efficiency parabola: eta = eta_design * (1 - (fr)^2)
                decimal eta = etaDesign * (1m - fr * fr);
                eta = Math.Max(0.05m, Math.Min(etaDesign, eta));

                // Estimate compression ratio from polytropic head:
                //   H = (Z*R*T1/MW) * (n/(n-1)) * [(P2/P1)^((n-1)/n) - 1]
                //   Invert: (P2/P1) = (1 + H*MW*(n-1)/(Z*R*T1*n))^(n/(n-1))
                decimal k = compressorProperties.SPECIFIC_HEAT_RATIO;
                decimal n = (k * eta) / (k - eta * (k - 1m));
                decimal mw = conditions.GAS_SPECIFIC_GRAVITY * 28.9645m;
                decimal z = ZFactorCalculator.CalculateBrillBeggs(
                    conditions.SUCTION_PRESSURE, conditions.SUCTION_TEMPERATURE,
                    conditions.GAS_SPECIFIC_GRAVITY);
                decimal R = 1545m;
                decimal hFactor = head * mw * (n - 1m) / (z * R * conditions.SUCTION_TEMPERATURE * n);
                decimal cr = n > 1m && hFactor > -1m
                    ? (decimal)Math.Pow((double)(1m + hFactor), (double)(n / (n - 1m)))
                    : 1m;
                cr = Math.Max(1m, cr);

                result.Add((Math.Round(q, 1), Math.Round(head, 0), Math.Round(eta, 4), Math.Round(cr, 3)));
            }

            return result;
        }

        /// <summary>
        /// Checks whether the operating point is within the allowable operating region (AOR)
        /// defined by the surge line and the choke (stonewall) limit.
        ///
        /// The choke limit occurs when the impeller tip Mach number approaches 1.0 and
        /// head drops to zero — approximated here as Q &gt; 1.10 · Q_design.
        ///
        /// Reference: API 617 (8th ed.), Sections 2.3 and 6.3.
        /// </summary>
        /// <param name="compressorProperties">Compressor design properties.</param>
        /// <returns>
        /// Tuple: (IsInAOR, DistanceToSurge [Mscf/day], DistanceToChoke [Mscf/day], Notes).
        /// </returns>
        public static (bool IsInAOR, decimal DistanceToSurge, decimal DistanceToChoke, string Notes)
            CheckOperatingRegion(CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties)
        {
            if (compressorProperties == null)
                throw new ArgumentNullException(nameof(compressorProperties));
            if (compressorProperties.OPERATING_CONDITIONS == null)
                throw new ArgumentNullException(nameof(compressorProperties.OPERATING_CONDITIONS));

            var (surgeMargin, surgeFlow, surgeStatus) = CalculateSurgeMargin(compressorProperties);
            decimal q = compressorProperties.OPERATING_CONDITIONS.GAS_FLOW_RATE;
            decimal qDesign = q;  // operating point is the design point for this check
            decimal chokeFlow = qDesign * 1.10m;

            bool inAOR = q > surgeFlow && q < chokeFlow;
            string notes = inAOR
                ? $"Operating point in AOR. Surge margin = {surgeMargin:F2}. {surgeStatus}"
                : (q <= surgeFlow
                    ? $"Below surge line! {surgeStatus}"
                    : "Operating near choke limit — flow may be excessive.");

            return (inAOR, Math.Round(q - surgeFlow, 1), Math.Round(chokeFlow - q, 1), notes);
        }
    }
}

