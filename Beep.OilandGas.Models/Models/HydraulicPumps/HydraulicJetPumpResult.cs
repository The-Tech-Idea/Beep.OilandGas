namespace Beep.OilandGas.Models.HydraulicPumps
{
    /// <summary>
    /// Represents the result of a hydraulic jet pump performance calculation.
    /// </summary>
    public class HydraulicJetPumpResult
    {
        /// <summary>
        /// Production rate in bbl/day.
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Total flow rate (production + power fluid) in bbl/day.
        /// </summary>
        public decimal TotalFlowRate { get; set; }

        /// <summary>
        /// Production ratio (production rate / power fluid rate).
        /// </summary>
        public decimal ProductionRatio { get; set; }

        /// <summary>
        /// Pump efficiency as a fraction (0-1).
        /// </summary>
        public decimal PumpEfficiency { get; set; }

        /// <summary>
        /// Pump intake pressure in psia.
        /// </summary>
        public decimal PumpIntakePressure { get; set; }

        /// <summary>
        /// Pump discharge pressure in psia.
        /// </summary>
        public decimal PumpDischargePressure { get; set; }

        /// <summary>
        /// Pressure differential (discharge - intake) in psi.
        /// </summary>
        public decimal PressureDifferential => PumpDischargePressure - PumpIntakePressure;

        /// <summary>
        /// Power fluid horsepower.
        /// </summary>
        public decimal PowerFluidHorsepower { get; set; }

        /// <summary>
        /// Hydraulic horsepower (produced).
        /// </summary>
        public decimal HydraulicHorsepower { get; set; }

        /// <summary>
        /// System efficiency as a fraction (0-1).
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Pump intake temperature in degrees Rankine.
        /// </summary>
        public decimal? PumpIntakeTemperature { get; set; }

        /// <summary>
        /// Pump discharge temperature in degrees Rankine.
        /// </summary>
        public decimal? PumpDischargeTemperature { get; set; }

        /// <summary>
        /// Calculation timestamp.
        /// </summary>
        public DateTime CalculationTime { get; set; } = DateTime.UtcNow;
    }
}
