namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod flow rate and power results
    /// DTO for calculations - Entity class: SUCKER_ROD_FLOW_RATE_POWER_RESULT
    /// </summary>
    public class SuckerRodFlowRatePowerResult : ModelEntityBase
    {
        /// <summary>
        /// Production rate in bbl/day
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Pump displacement in bbl/day
        /// </summary>
        public decimal PumpDisplacement { get; set; }

        /// <summary>
        /// Volumetric efficiency (0-1)
        /// </summary>
        public decimal VolumetricEfficiency { get; set; }

        /// <summary>
        /// Polished rod horsepower
        /// </summary>
        public decimal PolishedRodHorsepower { get; set; }

        /// <summary>
        /// Hydraulic horsepower
        /// </summary>
        public decimal HydraulicHorsepower { get; set; }

        /// <summary>
        /// Friction horsepower
        /// </summary>
        public decimal FrictionHorsepower { get; set; }

        /// <summary>
        /// Total horsepower required
        /// </summary>
        public decimal TotalHorsepower { get; set; }

        /// <summary>
        /// Motor horsepower
        /// </summary>
        public decimal MotorHorsepower { get; set; }

        /// <summary>
        /// System efficiency (0-1)
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Energy consumption in kWh/day
        /// </summary>
        public decimal EnergyConsumption { get; set; }
    }
}



