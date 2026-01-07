namespace Beep.OilandGas.Models.HydraulicPumps
{
    /// <summary>
    /// Represents the result of a hydraulic piston pump performance calculation.
    /// </summary>
    public class HydraulicPistonPumpResult
    {
        /// <summary>
        /// Pump displacement in cubic inches per stroke or bbl/day equivalent.
        /// </summary>
        public decimal PumpDisplacement { get; set; }

        /// <summary>
        /// Production rate in barrels per day (bbl/day).
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Power fluid consumption in bbl/day.
        /// </summary>
        public decimal PowerFluidConsumption { get; set; }

        /// <summary>
        /// Pump intake pressure in psia.
        /// </summary>
        public decimal PumpIntakePressure { get; set; }

        /// <summary>
        /// Pump discharge pressure in psia.
        /// </summary>
        public decimal PumpDischargePressure { get; set; }

        /// <summary>
        /// Power fluid horsepower required.
        /// </summary>
        public decimal PowerFluidHorsepower { get; set; }

        /// <summary>
        /// Hydraulic horsepower output.
        /// </summary>
        public decimal HydraulicHorsepower { get; set; }

        /// <summary>
        /// System efficiency as a fraction (0-1).
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Inlet pressure in psia.
        /// </summary>
        public decimal InletPressure { get; set; }

        /// <summary>
        /// Outlet pressure in psia.
        /// </summary>
        public decimal OutletPressure { get; set; }

        /// <summary>
        /// Pressure differential (outlet - inlet) in psi.
        /// </summary>
        public decimal PressureDifferential => OutletPressure - InletPressure;

        /// <summary>
        /// Pump speed in revolutions per minute (RPM).
        /// </summary>
        public decimal Speed { get; set; }

        /// <summary>
        /// Volumetric efficiency as a fraction (0-1).
        /// </summary>
        public decimal VolumetricEfficiency { get; set; }

        /// <summary>
        /// Mechanical efficiency as a fraction (0-1).
        /// </summary>
        public decimal MechanicalEfficiency { get; set; }

        /// <summary>
        /// Overall efficiency as a fraction (0-1).
        /// </summary>
        public decimal OverallEfficiency { get; set; }

        /// <summary>
        /// Input horsepower required.
        /// </summary>
        public decimal InputHorsepower { get; set; }

        /// <summary>
        /// Slippage in barrels per day.
        /// </summary>
        public decimal Slippage { get; set; }

        /// <summary>
        /// Pump outlet temperature in degrees Rankine.
        /// </summary>
        public decimal? OutletTemperature { get; set; }

        /// <summary>
        /// Calculation timestamp.
        /// </summary>
        public DateTime CalculationTime { get; set; } = DateTime.UtcNow;
    }
}
