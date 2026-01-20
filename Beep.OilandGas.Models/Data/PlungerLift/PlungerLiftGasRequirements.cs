namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents plunger lift gas requirements
    /// DTO for calculations - Entity class: PLUNGER_LIFT_GAS_REQUIREMENTS
    /// </summary>
    public class PlungerLiftGasRequirements : ModelEntityBase
    {
        /// <summary>
        /// Required gas injection rate in Mscf/day
        /// </summary>
        public decimal RequiredGasInjectionRate { get; set; }

        /// <summary>
        /// Available gas from well in Mscf/day
        /// </summary>
        public decimal AvailableGas { get; set; }

        /// <summary>
        /// Additional gas required in Mscf/day
        /// </summary>
        public decimal AdditionalGasRequired { get; set; }

        /// <summary>
        /// Gas-liquid ratio required
        /// </summary>
        public decimal RequiredGasLiquidRatio { get; set; }

        /// <summary>
        /// Minimum casing pressure in psia
        /// </summary>
        public decimal MinimumCasingPressure { get; set; }

        /// <summary>
        /// Maximum casing pressure in psia
        /// </summary>
        public decimal MaximumCasingPressure { get; set; }
    }
}



