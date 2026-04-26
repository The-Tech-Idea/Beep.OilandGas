using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Compressor station modeling for gas transmission pipelines.
    ///
    /// This module focuses on the thermodynamic sizing and placement of individual
    /// compressor stations.  Economic comparison of compression vs. looping is handled
    /// by <see cref="LoopingAndCompressionEconomics"/>.
    ///
    /// Compressor power (isentropic basis, Huntington 1950):
    ///
    ///   HP = (Q_MMSCFD × 1,000,000 × MW) / (24 × 3,600)   (lb/s)  ← mass flow
    ///   W_isen = (k/(k-1)) × (Z₁+Z₂)/2 × R × T₁ × [(P₂/P₁)^((k-1)/k) − 1]
    ///   HP_shaft = W_isen × mass_flow / (550 × η_isen)
    ///
    /// Simplified form widely used in field calculations (Menon, 2005):
    ///   HP = 0.0857 × Q_MMSCFD × T₁ × (k/(k-1)) × [(r)^((k-1)/k) − 1] / η
    ///   where r = P_out / P_in (compression ratio), T₁ in °R.
    ///
    /// Multi-stage compression: split total ratio across stages so each stage ratio
    ///   r_stage = r_total^(1/n_stages)
    ///
    /// Station pressure constraints (AGA pipeline operating guidelines):
    ///   • Maximum allowable discharge pressure (MAOP or MOP)
    ///   • Minimum suction pressure (instrument constraint + surge margin)
    ///   • Maximum compression ratio per stage ≤ 1.5–2.5 (centrifugal) or ≤ 5 (reciprocating)
    ///
    /// Fuel gas consumption:
    ///   Q_fuel = HP × heat_rate / LHV_gas
    ///   Typical engine heat rate ≈ 7,000–9,000 BTU/HP-hr; LHV_gas ≈ 23,800 BTU/lb.
    ///
    /// References:
    ///   - Menon, E.S. (2005) "Gas Pipeline Hydraulics", CRC Press, Ch. 5.
    ///   - Huntington, R.L. (1950) "Natural Gas and Gas Condensate Properties"
    ///   - AGA Report No. 9 (2007) for flow metering requirements.
    /// </summary>
    public static class CompressorStations
    {
        private const double R_GAS_FT_LBF  = 1545.35;   // universal gas constant ft·lbf/(lbmol·°R)
        private const double BTU_PER_HP_HR  = 2545.0;   // 1 HP = 2545 BTU/hr
        private const double MMBTU_LHV_GAS  = 1.0;      // placeholder; caller supplies LHV

        // ─────────────────────────────────────────────────────────────────
        //  1. Single-stage isentropic compression power
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates required shaft horsepower for a single-stage centrifugal or
        /// reciprocating compressor (isentropic basis, Menon simplified form).
        ///
        ///   HP = 0.0857 × Q × T₁ × (k/(k−1)) × [(r)^((k−1)/k) − 1] / η
        /// </summary>
        /// <param name="flowRateMmscfd">Gas flow rate (MMSCFD).</param>
        /// <param name="suctionPressurePsia">Suction pressure (psia).</param>
        /// <param name="dischargePressurePsia">Discharge pressure (psia).</param>
        /// <param name="suctionTempF">Suction temperature (°F).</param>
        /// <param name="specificHeatRatioK">Cp/Cv ratio for the gas (typically 1.2–1.4 for natural gas).</param>
        /// <param name="isentropicEfficiency">Isentropic efficiency η (0–1; typical 0.75–0.85).</param>
        /// <returns><see cref="CompressionPowerResult"/>.</returns>
        public static CompressionPowerResult CalculateCompressionPower(
            double flowRateMmscfd,
            double suctionPressurePsia,
            double dischargePressurePsia,
            double suctionTempF,
            double specificHeatRatioK = 1.3,
            double isentropicEfficiency = 0.80)
        {
            if (suctionPressurePsia <= 0)    throw new ArgumentOutOfRangeException(nameof(suctionPressurePsia));
            if (dischargePressurePsia <= 0)  throw new ArgumentOutOfRangeException(nameof(dischargePressurePsia));
            if (isentropicEfficiency <= 0)   throw new ArgumentOutOfRangeException(nameof(isentropicEfficiency));

            double T1  = suctionTempF + 459.67;  // °R
            double r   = dischargePressurePsia / suctionPressurePsia;
            double k   = specificHeatRatioK;
            double exp = (k - 1.0) / k;

            double hp = 0.0857 * flowRateMmscfd * T1
                        * (k / (k - 1.0))
                        * (Math.Pow(r, exp) - 1.0)
                        / isentropicEfficiency;

            double dischargeTemp = T1 * Math.Pow(r, exp) - 459.67;  // °F

            return new CompressionPowerResult
            {
                RequiredHorsepowerHP    = hp,
                CompressionRatio        = r,
                DischargeTempF          = dischargeTemp,
                SuctionPressurePsia     = suctionPressurePsia,
                DischargePressurePsia   = dischargePressurePsia,
                SuctionTempF            = suctionTempF,
                FlowRateMmscfd          = flowRateMmscfd,
                IsentropicEfficiency    = isentropicEfficiency,
                SpecificHeatRatioK      = k,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  2. Multi-stage compression
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Splits a high overall compression ratio across <paramref name="numberOfStages"/> equal
        /// stages with inter-cooling (T₁ same for each stage after perfect inter-cooling).
        ///
        ///   r_stage = r_total^(1/n)
        ///   Total HP = n × HP_per_stage
        /// </summary>
        public static MultiStageCompressionResult CalculateMultiStageCompression(
            double flowRateMmscfd,
            double overallSuctionPressurePsia,
            double overallDischargePressurePsia,
            double suctionTempF,
            int numberOfStages,
            double specificHeatRatioK = 1.3,
            double isentropicEfficiency = 0.80)
        {
            if (numberOfStages < 1) throw new ArgumentOutOfRangeException(nameof(numberOfStages));

            double r_total  = overallDischargePressurePsia / overallSuctionPressurePsia;
            double r_stage  = Math.Pow(r_total, 1.0 / numberOfStages);

            var stages = new List<CompressionPowerResult>();
            double currentSuction = overallSuctionPressurePsia;

            for (int i = 0; i < numberOfStages; i++)
            {
                double discharge = currentSuction * r_stage;
                var stageResult = CalculateCompressionPower(
                    flowRateMmscfd, currentSuction, discharge,
                    suctionTempF, specificHeatRatioK, isentropicEfficiency);
                stages.Add(stageResult);
                currentSuction = discharge;
                // Assume perfect inter-cooling: reset suction temp for next stage
            }

            return new MultiStageCompressionResult
            {
                NumberOfStages             = numberOfStages,
                StageCompressionRatio      = r_stage,
                OverallCompressionRatio    = r_total,
                Stages                     = stages,
                TotalHorsepowerHP          = stages.Sum(s => s.RequiredHorsepowerHP),
                FinalDischargePressurePsia = overallDischargePressurePsia,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  3. Fuel gas consumption
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates fuel gas consumption for a compressor station.
        ///
        ///   Q_fuel_MMSCFD = (HP × heat_rate_BTU_per_HP_hr × 24) / (LHV_BTU_per_SCF × 1,000,000)
        /// </summary>
        /// <param name="stationHorsepowerHP">Total shaft horsepower required.</param>
        /// <param name="engineHeatRateBtuPerHpHr">Prime mover heat rate (BTU/HP-hr). Gas engine ≈ 8,500.</param>
        /// <param name="lhvBtuPerScf">Lower heating value of fuel gas (BTU/scf). Typical ≈ 900–1,050.</param>
        /// <returns>Fuel gas consumption in MMSCFD.</returns>
        public static double CalculateFuelGasConsumption(
            double stationHorsepowerHP,
            double engineHeatRateBtuPerHpHr = 8_500.0,
            double lhvBtuPerScf = 1_000.0)
        {
            if (lhvBtuPerScf <= 0) throw new ArgumentOutOfRangeException(nameof(lhvBtuPerScf));
            return stationHorsepowerHP * engineHeatRateBtuPerHpHr * 24.0
                   / (lhvBtuPerScf * 1_000_000.0);
        }

        // ─────────────────────────────────────────────────────────────────
        //  4. Station placement optimizer
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Determines the optimal number and spacing of compressor stations to maintain
        /// a target delivery pressure over a long pipeline (Weymouth assumption).
        ///
        /// Logic: Incrementally places stations wherever pipeline pressure would drop below
        /// the minimum acceptable suction pressure.
        /// </summary>
        /// <param name="inletPressurePsia">Pipeline inlet (first station suction) pressure (psia).</param>
        /// <param name="minimumSuctionPsia">Minimum allowable suction pressure before a new station (psia).</param>
        /// <param name="targetDischargePsia">Each station's discharge pressure (psia) after compression.</param>
        /// <param name="deliveryPressurePsia">Required delivery pressure at pipeline terminus (psia).</param>
        /// <param name="pipelineLengthMiles">Total pipeline length (miles).</param>
        /// <param name="pressureDropPerMilePsi">Approximate linear pressure drop (psi/mile) at design flow.</param>
        /// <param name="flowRateMmscfd">Flow rate for HP calculation at each station.</param>
        /// <param name="suctionTempF">Suction temperature (°F).</param>
        public static StationPlacementResult OptimizeStationPlacement(
            double inletPressurePsia,
            double minimumSuctionPsia,
            double targetDischargePsia,
            double deliveryPressurePsia,
            double pipelineLengthMiles,
            double pressureDropPerMilePsi,
            double flowRateMmscfd,
            double suctionTempF = 60.0)
        {
            var stations = new List<CompressorStationLocation>();
            double currentPressure = inletPressurePsia;
            double milesTraveled   = 0;

            while (milesTraveled < pipelineLengthMiles)
            {
                // Advance mile by mile until we hit minimum suction
                double milesThisStation = (currentPressure - minimumSuctionPsia) / pressureDropPerMilePsi;
                milesThisStation = Math.Min(milesThisStation, pipelineLengthMiles - milesTraveled);
                milesTraveled   += milesThisStation;

                if (milesTraveled >= pipelineLengthMiles)
                    break; // No more station needed — we reached the end

                double suctionAtStation = currentPressure - milesThisStation * pressureDropPerMilePsi;

                var powerResult = CalculateCompressionPower(
                    flowRateMmscfd, suctionAtStation, targetDischargePsia, suctionTempF);

                stations.Add(new CompressorStationLocation
                {
                    StationNumber        = stations.Count + 1,
                    MilePostLocation     = milesTraveled,
                    SuctionPressurePsia  = suctionAtStation,
                    DischargePressurePsia = targetDischargePsia,
                    RequiredHorsepowerHP = powerResult.RequiredHorsepowerHP,
                    DischargeTempF       = powerResult.DischargeTempF,
                });

                currentPressure = targetDischargePsia;
            }

            double finalDeliveryPressure = currentPressure
                - (pipelineLengthMiles - (stations.Count > 0 ? stations.Last().MilePostLocation : 0))
                * pressureDropPerMilePsi;

            return new StationPlacementResult
            {
                Stations                    = stations,
                NumberOfStations            = stations.Count,
                TotalSystemHorsepowerHP     = stations.Sum(s => s.RequiredHorsepowerHP),
                EstimatedDeliveryPressure   = finalDeliveryPressure,
                DeliveryPressureMet         = finalDeliveryPressure >= deliveryPressurePsia,
            };
        }

        // ─────────────────────────────────────────────────────────────────
        //  Result types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Single-stage compression power result.</summary>
        public sealed class CompressionPowerResult
        {
            public double RequiredHorsepowerHP    { get; set; }
            public double CompressionRatio        { get; set; }
            public double DischargeTempF          { get; set; }
            public double SuctionPressurePsia     { get; set; }
            public double DischargePressurePsia   { get; set; }
            public double SuctionTempF            { get; set; }
            public double FlowRateMmscfd          { get; set; }
            public double IsentropicEfficiency    { get; set; }
            public double SpecificHeatRatioK      { get; set; }
        }

        /// <summary>Multi-stage compression result.</summary>
        public sealed class MultiStageCompressionResult
        {
            public int    NumberOfStages             { get; set; }
            public double StageCompressionRatio      { get; set; }
            public double OverallCompressionRatio    { get; set; }
            public List<CompressionPowerResult> Stages { get; set; } = new();
            public double TotalHorsepowerHP          { get; set; }
            public double FinalDischargePressurePsia { get; set; }
        }

        /// <summary>Location and sizing of one compressor station on the pipeline route.</summary>
        public sealed class CompressorStationLocation
        {
            public int    StationNumber         { get; set; }
            public double MilePostLocation      { get; set; }
            public double SuctionPressurePsia   { get; set; }
            public double DischargePressurePsia { get; set; }
            public double RequiredHorsepowerHP  { get; set; }
            public double DischargeTempF        { get; set; }
        }

        /// <summary>Result of the station-placement optimization algorithm.</summary>
        public sealed class StationPlacementResult
        {
            public List<CompressorStationLocation> Stations  { get; set; } = new();
            public int    NumberOfStations              { get; set; }
            public double TotalSystemHorsepowerHP       { get; set; }
            public double EstimatedDeliveryPressure     { get; set; }
            public bool   DeliveryPressureMet           { get; set; }
        }
    }
}
