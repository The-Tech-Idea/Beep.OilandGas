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
        private decimal RequiredGasInjectionRateValue;

        public decimal RequiredGasInjectionRate

        {

            get { return this.RequiredGasInjectionRateValue; }

            set { SetProperty(ref RequiredGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Available gas from well in Mscf/day
        /// </summary>
        private decimal AvailableGasValue;

        public decimal AvailableGas

        {

            get { return this.AvailableGasValue; }

            set { SetProperty(ref AvailableGasValue, value); }

        }

        /// <summary>
        /// Additional gas required in Mscf/day
        /// </summary>
        private decimal AdditionalGasRequiredValue;

        public decimal AdditionalGasRequired

        {

            get { return this.AdditionalGasRequiredValue; }

            set { SetProperty(ref AdditionalGasRequiredValue, value); }

        }

        /// <summary>
        /// Gas-liquid ratio required
        /// </summary>
        private decimal RequiredGasLiquidRatioValue;

        public decimal RequiredGasLiquidRatio

        {

            get { return this.RequiredGasLiquidRatioValue; }

            set { SetProperty(ref RequiredGasLiquidRatioValue, value); }

        }

        /// <summary>
        /// Minimum casing pressure in psia
        /// </summary>
        private decimal MinimumCasingPressureValue;

        public decimal MinimumCasingPressure

        {

            get { return this.MinimumCasingPressureValue; }

            set { SetProperty(ref MinimumCasingPressureValue, value); }

        }

        /// <summary>
        /// Maximum casing pressure in psia
        /// </summary>
        private decimal MaximumCasingPressureValue;

        public decimal MaximumCasingPressure

        {

            get { return this.MaximumCasingPressureValue; }

            set { SetProperty(ref MaximumCasingPressureValue, value); }

        }
    }
}




