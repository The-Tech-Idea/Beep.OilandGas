using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides time-dependent Vertical Lift Performance (VLP) calculations.
    /// Models how VLP curves shift over time due to reservoir pressure depletion,
    /// tubing erosion from sand/high-velocity flow, and scale build-up inside
    /// the tubing string.
    /// </summary>
    public static class DynamicVLP
    {
        // ─────────────────────────────────────────────────────────────────
        //  Erosion velocity limit  (API RP 14E)
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the erosional velocity limit for a mixture flowing in tubing
        /// according to API RP 14E.
        ///
        ///   Ve = C / sqrt(ρm)
        ///
        /// where ρm is the mixture density in lb/ft³ and C is an empirical constant
        /// (100 for continuous service, 125 for intermittent, 150–200 for non-corrosive clean).
        /// </summary>
        /// <param name="mixtureDensity_lbft3">In-situ mixture density (lb/ft³).</param>
        /// <param name="apiRp14eConstant">API RP 14E erosional constant C (default 100).</param>
        /// <returns>Erosional velocity limit in ft/s.</returns>
        public static double ErosionalVelocity(double mixtureDensity_lbft3, double apiRp14eConstant = 100.0)
        {
            if (mixtureDensity_lbft3 <= 0)
                throw new ArgumentException("Mixture density must be positive.", nameof(mixtureDensity_lbft3));
            if (apiRp14eConstant <= 0)
                throw new ArgumentException("API RP 14E constant must be positive.", nameof(apiRp14eConstant));

            return apiRp14eConstant / Math.Sqrt(mixtureDensity_lbft3);
        }

        /// <summary>
        /// Calculates the maximum allowable flow rate (STB/day) before the in-situ mixture
        /// velocity exceeds the API RP 14E erosional limit inside a given tubing size.
        /// </summary>
        /// <param name="tubingID_inches">Tubing internal diameter (inches).</param>
        /// <param name="mixtureDensity_lbft3">In-situ mixture density (lb/ft³).</param>
        /// <param name="formationVolumeFactor">Liquid FVF (res bbl / STB) for unit conversion.</param>
        /// <param name="apiRp14eConstant">API RP 14E erosional constant C (default 100).</param>
        /// <returns>Maximum allowable liquid flow rate (STB/day) before erosion onset.</returns>
        public static double MaxErosionFreeFlowRate(
            double tubingID_inches,
            double mixtureDensity_lbft3,
            double formationVolumeFactor = 1.0,
            double apiRp14eConstant = 100.0)
        {
            if (tubingID_inches <= 0)
                throw new ArgumentException("Tubing ID must be positive.", nameof(tubingID_inches));

            double ve = ErosionalVelocity(mixtureDensity_lbft3, apiRp14eConstant);  // ft/s

            // Cross-sectional area of tubing: A = π/4 * d²  (d in feet)
            double d_ft = tubingID_inches / 12.0;
            double area_ft2 = Math.PI / 4.0 * d_ft * d_ft;

            // Volumetric flow rate at reservoir conditions: Q_res = Ve * A (ft³/s)
            double qRes_ft3s = ve * area_ft2;

            // Convert ft³/s → bbl/s → STB/day
            // 1 bbl = 5.615 ft³; 1 res bbl = FVF * STB
            double qRes_bblDay = qRes_ft3s * 86400.0 / 5.615;  // res bbl/day
            double qStbDay = qRes_bblDay / Math.Max(formationVolumeFactor, 0.001);

            return qStbDay;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Tubing wall-thickness reduction from erosion
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates cumulative tubing wall-thickness reduction (inches) after a given
        /// production period, based on a simplified erosion-rate model.
        ///
        /// The model uses the Finnie erosion equation (mass loss proportional to V² × sand mass):
        ///   ER (in/year) = k_er × (V / Ve)² × SandConcentration_ppm / 1000
        ///
        /// where k_er is a material-dependent erosion rate constant.
        /// </summary>
        /// <param name="flowVelocity_ftps">In-situ mixture velocity (ft/s).</param>
        /// <param name="mixtureDensity_lbft3">Mixture density (lb/ft³).</param>
        /// <param name="sandConcentration_ppm">Sand concentration in produced fluid (ppm by mass).</param>
        /// <param name="years">Production period (years).</param>
        /// <param name="erosionRateConstant">
        ///   Material erosion rate constant k_er (in/year per unit normalised velocity² per 1000 ppm sand).
        ///   Typical values: carbon steel ≈ 0.01, chrome steel ≈ 0.005.
        /// </param>
        /// <param name="apiRp14eConstant">API RP 14E C constant used to compute erosional velocity.</param>
        /// <returns>Estimated cumulative wall-thickness loss (inches).</returns>
        public static double TubingWallThicknessLoss(
            double flowVelocity_ftps,
            double mixtureDensity_lbft3,
            double sandConcentration_ppm,
            double years,
            double erosionRateConstant = 0.01,
            double apiRp14eConstant = 100.0)
        {
            if (flowVelocity_ftps <= 0) return 0;
            if (years <= 0) return 0;

            double ve = ErosionalVelocity(mixtureDensity_lbft3, apiRp14eConstant);
            double velocityRatio = flowVelocity_ftps / ve;

            // Erosion rate: proportional to velocity ratio² and sand concentration
            double erosionRate_inPerYear = erosionRateConstant
                * velocityRatio * velocityRatio
                * (sandConcentration_ppm / 1000.0);

            return erosionRate_inPerYear * years;
        }

        /// <summary>
        /// Determines whether a given tubing wall-thickness loss represents a critical
        /// failure risk (remaining wall below 50% of original is considered critical).
        /// </summary>
        /// <param name="originalWallThickness_inches">Original nominal wall thickness (inches).</param>
        /// <param name="cumulativeLoss_inches">Cumulative wall-thickness loss (inches).</param>
        /// <returns>Fraction of original wall remaining (1.0 = intact, 0.0 = fully eroded).</returns>
        public static double RemainingWallFraction(
            double originalWallThickness_inches,
            double cumulativeLoss_inches)
        {
            if (originalWallThickness_inches <= 0) return 0;
            double remaining = originalWallThickness_inches - cumulativeLoss_inches;
            return Math.Max(0, remaining / originalWallThickness_inches);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Time-dependent VLP forecast
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generates a time-stepped VLP forecast that accounts for:
        /// <list type="bullet">
        ///   <item>Reservoir pressure depletion (shifting the IPR curve downward year-on-year).</item>
        ///   <item>Tubing effective-diameter reduction from scale build-up.</item>
        ///   <item>Increased friction factor from sand-induced roughening of tubing wall.</item>
        /// </list>
        /// For each time step the effective BHP required for three canonical flow rates
        /// (low = 25%, mid = 50%, high = 75% of year-0 operating rate) is computed.
        /// </summary>
        /// <param name="baseReservoir">Year-0 reservoir properties.</param>
        /// <param name="baseWellbore">Year-0 wellbore properties.</param>
        /// <param name="annualPressureDeclineRate">
        ///   Fractional decline in reservoir pressure per year (e.g. 0.05 = 5% per year).
        /// </param>
        /// <param name="annualSandProduction_tonsPerYear">
        ///   Annual sand production (metric tonnes/year) used to compute roughening and
        ///   wall erosion.
        /// </param>
        /// <param name="annualScaleRate">
        ///   Fractional reduction in tubing internal diameter per year due to scale
        ///   (e.g. 0.005 = 0.5% ID reduction per year).
        /// </param>
        /// <param name="years">Number of years to forecast.</param>
        /// <returns>A <see cref="TimeStepVLPForecast"/> containing year-by-year VLP data.</returns>
        public static TimeStepVLPForecast GenerateTimeStepForecast(
            ReservoirProperties baseReservoir,
            WellboreProperties baseWellbore,
            double annualPressureDeclineRate,
            double annualSandProduction_tonsPerYear = 0.0,
            double annualScaleRate = 0.0,
            int years = 10)
        {
            if (baseReservoir == null) throw new ArgumentNullException(nameof(baseReservoir));
            if (baseWellbore == null) throw new ArgumentNullException(nameof(baseWellbore));
            if (years < 1) years = 1;
            if (annualPressureDeclineRate < 0) annualPressureDeclineRate = 0;

            // Year-0 base operating rate to set reference flow bands
            double baseRate = ComputeSimpleOperatingFlowRate(baseReservoir, baseWellbore);
            double lowRate = baseRate * 0.25;
            double midRate = baseRate * 0.50;
            double highRate = baseRate * 0.75;

            var forecast = new TimeStepVLPForecast
            {
                YearsForecasted = years,
                AnnualSandProduction = annualSandProduction_tonsPerYear,
                AnnualScaleRate = annualScaleRate
            };

            double cumulativeSandConcentration = 0;
            double year0Bhp = ComputeBhpAtFlowRate(baseReservoir, baseWellbore, midRate);

            for (int yr = 0; yr <= years; yr++)
            {
                // Reservoir pressure for this year
                double prYear = baseReservoir.ReservoirPressure
                    * Math.Pow(1.0 - annualPressureDeclineRate, yr);

                // Cumulative scale build-up: reduces effective ID fraction
                double cumulativeScale = annualScaleRate * yr;  // fraction of ID reduction

                // Effective tubing ID (scale reduces the bore)
                double effectiveID = baseWellbore.TubingDiameter * (1.0 - cumulativeScale);
                if (effectiveID < baseWellbore.TubingDiameter * 0.1)
                    effectiveID = baseWellbore.TubingDiameter * 0.1;  // floor at 10% ID

                // Cumulative sand concentration proxy (rough roughening factor)
                cumulativeSandConcentration += annualSandProduction_tonsPerYear;  // cumulative tonnes

                // Create modified wellbore for this year
                var wellYear = CloneWellbore(baseWellbore);
                wellYear.TubingDiameter = effectiveID;

                // Roughness effect on friction: Moody roughness factor increases with sand
                // Friction penalty expressed as a BHP uplift fraction
                double roughnessPenalty = ComputeRoughnessPenalty(cumulativeSandConcentration);

                // BHP required at each flow-rate band using linear VLP approximation
                var modReservoir = CloneReservoir(baseReservoir);
                modReservoir.ReservoirPressure = prYear;

                double bhpLow = ApplyRoughnessPenalty(
                    ComputeBhpAtFlowRate(modReservoir, wellYear, lowRate), roughnessPenalty);
                double bhpMid = ApplyRoughnessPenalty(
                    ComputeBhpAtFlowRate(modReservoir, wellYear, midRate), roughnessPenalty);
                double bhpHigh = ApplyRoughnessPenalty(
                    ComputeBhpAtFlowRate(modReservoir, wellYear, highRate), roughnessPenalty);

                double degradationPct = yr > 0
                    ? (bhpMid - year0Bhp) / Math.Max(year0Bhp, 1.0) * 100.0
                    : 0.0;

                forecast.ForecastTimeSteps.Add(new VLPForecastTimeStep
                {
                    Year = yr,
                    SandConcentration = cumulativeSandConcentration,
                    ScaleBuildupFraction = cumulativeScale,
                    LowFlowRequiredBHP = bhpLow,
                    MidFlowRequiredBHP = bhpMid,
                    HighFlowRequiredBHP = bhpHigh
                });
            }

            // Total degradation: compare final-year mid-BHP vs year-0 mid-BHP
            if (forecast.ForecastTimeSteps.Count > 0)
            {
                double finalBhp = forecast.ForecastTimeSteps[^1].MidFlowRequiredBHP ?? year0Bhp;
                forecast.TotalDegradationPercentage = (finalBhp - year0Bhp) / Math.Max(year0Bhp, 1.0) * 100.0;
            }

            return forecast;
        }

        /// <summary>
        /// Computes a single VLP curve (list of (flowRate, BHP) points) for a given year,
        /// accounting for reservoir pressure depletion and tubing scale build-up.
        /// Useful for overlaying multiple-year VLP envelopes on a nodal-analysis plot.
        /// </summary>
        /// <param name="baseReservoir">Year-0 reservoir properties.</param>
        /// <param name="baseWellbore">Year-0 wellbore properties.</param>
        /// <param name="year">Target year (0 = base case).</param>
        /// <param name="annualPressureDeclineRate">Fractional annual reservoir pressure decline.</param>
        /// <param name="annualScaleRate">Fractional annual ID reduction from scale.</param>
        /// <param name="cumulativeSandTonnes">Total sand produced by target year (tonnes).</param>
        /// <param name="maxFlowRate">Maximum flow rate for curve (STB/day).</param>
        /// <param name="points">Number of points on the VLP curve.</param>
        /// <returns>List of (flowRate STB/day, BHP psia) tuples.</returns>
        public static List<VLPPoint> GenerateYearlyVLPCurve(
            ReservoirProperties baseReservoir,
            WellboreProperties baseWellbore,
            int year,
            double annualPressureDeclineRate = 0.05,
            double annualScaleRate = 0.005,
            double cumulativeSandTonnes = 0.0,
            double maxFlowRate = 5000,
            int points = 50)
        {
            if (baseReservoir == null) throw new ArgumentNullException(nameof(baseReservoir));
            if (baseWellbore == null) throw new ArgumentNullException(nameof(baseWellbore));
            if (year < 0) year = 0;
            if (points < 2) points = 2;

            double prYear = baseReservoir.ReservoirPressure * Math.Pow(1.0 - annualPressureDeclineRate, year);
            double cumulativeScale = annualScaleRate * year;
            double effectiveID = baseWellbore.TubingDiameter * (1.0 - cumulativeScale);
            if (effectiveID < baseWellbore.TubingDiameter * 0.1)
                effectiveID = baseWellbore.TubingDiameter * 0.1;

            double roughnessPenalty = ComputeRoughnessPenalty(cumulativeSandTonnes);

            var modReservoir = CloneReservoir(baseReservoir);
            modReservoir.ReservoirPressure = prYear;

            var wellYear = CloneWellbore(baseWellbore);
            wellYear.TubingDiameter = effectiveID;

            var curve = new List<VLPPoint>();

            for (int i = 0; i <= points; i++)
            {
                double q = maxFlowRate * i / points;
                double bhp = ComputeBhpAtFlowRate(modReservoir, wellYear, q);
                bhp = ApplyRoughnessPenalty(bhp, roughnessPenalty);

                curve.Add(new VLPPoint { FlowRate = q, RequiredBottomholePressure = bhp });
            }

            return curve;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Simple linear VLP: BHP = WHP + hydrostatic gradient × depth + friction term.
        /// Used internally for dynamic VLP calculations where a full multiphase correlation
        /// is not required.
        /// </summary>
        private static double ComputeBhpAtFlowRate(
            ReservoirProperties reservoir, WellboreProperties wellbore, double flowRate)
        {
            double depth = wellbore.TubingLength;
            double whp = wellbore.WellheadPressure;

            // Hydrostatic component: 0.433 psi/ft × depth for water, corrected for fluid mixture
            double mixtureGradient = 0.433 * (1.0 - wellbore.WaterCut) + 0.433 * wellbore.WaterCut;
            double hydrostaticBhp = whp + mixtureGradient * depth;

            // Friction term: simplified Darcy-Weisbach friction proportional to q²
            // f_friction ≈ 0.001 × q² / d^5  (highly simplified; d in inches)
            double d = wellbore.TubingDiameter;
            double frictionBhp = (d > 0)
                ? 0.001 * flowRate * flowRate / Math.Pow(d, 5)
                : 0;

            return hydrostaticBhp + frictionBhp;
        }

        private static double ComputeSimpleOperatingFlowRate(
            ReservoirProperties reservoir, WellboreProperties wellbore)
        {
            const int pts = 40;
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;
            if (qMax <= 0) return 0;

            double bestQ = 0;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= pts; i++)
            {
                double q = qMax * i / pts;
                double ratio = q / qMax;
                double pwfIPR = reservoir.ReservoirPressure * (1.0 - 0.2 * ratio - 0.8 * ratio * ratio);
                double pwfVLP = ComputeBhpAtFlowRate(reservoir, wellbore, q);

                double diff = Math.Abs(pwfIPR - pwfVLP);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestQ = q;
                }
                if (pwfIPR < pwfVLP) break;
            }

            return bestQ;
        }

        /// <summary>
        /// Computes a friction-penalty multiplier based on cumulative sand production.
        /// Sand erosion roughens the tubing wall, increasing the Moody friction factor.
        /// Penalty is capped at 15% BHP uplift.
        /// </summary>
        private static double ComputeRoughnessPenalty(double cumulativeSandTonnes)
        {
            // Empirical: 0.5% BHP increase per 10 tonnes cumulative sand, capped at 15%
            double penalty = (cumulativeSandTonnes / 10.0) * 0.005;
            return Math.Min(penalty, 0.15);
        }

        private static double ApplyRoughnessPenalty(double bhp, double penaltyFraction)
        {
            return bhp * (1.0 + penaltyFraction);
        }

        private static ReservoirProperties CloneReservoir(ReservoirProperties src)
        {
            return new ReservoirProperties
            {
                ReservoirPressure = src.ReservoirPressure,
                BubblePointPressure = src.BubblePointPressure,
                ProductivityIndex = src.ProductivityIndex,
                WaterCut = src.WaterCut,
                GasOilRatio = src.GasOilRatio,
                OilGravity = src.OilGravity,
                FormationVolumeFactor = src.FormationVolumeFactor,
                OilViscosity = src.OilViscosity
            };
        }

        private static WellboreProperties CloneWellbore(WellboreProperties src)
        {
            return new WellboreProperties
            {
                TubingDiameter = src.TubingDiameter,
                TubingLength = src.TubingLength,
                WellheadPressure = src.WellheadPressure,
                WaterCut = src.WaterCut,
                GasOilRatio = src.GasOilRatio,
                OilGravity = src.OilGravity,
                GasSpecificGravity = src.GasSpecificGravity,
                WellheadTemperature = src.WellheadTemperature,
                BottomholeTemperature = src.BottomholeTemperature,
                TubingRoughness = src.TubingRoughness
            };
        }
    }
}
