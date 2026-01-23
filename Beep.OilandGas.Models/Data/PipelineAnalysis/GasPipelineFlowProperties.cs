namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    /// <summary>
    /// Represents gas pipeline flow properties
    /// DTO for calculations - Entity class: GAS_PIPELINE_FLOW_PROPERTIES
    /// </summary>
    public class GasPipelineFlowProperties : ModelEntityBase
    {
        /// <summary>
        /// Pipeline properties
        /// </summary>
        private PipelineProperties PipelineValue = new();

        public PipelineProperties Pipeline

        {

            get { return this.PipelineValue; }

            set { SetProperty(ref PipelineValue, value); }

        }

        /// <summary>
        /// Gas flow rate in Mscf/day
        /// </summary>
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        private decimal GasSpecificGravityValue;

        public decimal GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }

        /// <summary>
        /// Gas molecular weight
        /// </summary>
        private decimal GasMolecularWeightValue;

        public decimal GasMolecularWeight

        {

            get { return this.GasMolecularWeightValue; }

            set { SetProperty(ref GasMolecularWeightValue, value); }

        }

        /// <summary>
        /// Base pressure in psia
        /// </summary>
        private decimal BasePressureValue = 14.7m;

        public decimal BasePressure

        {

            get { return this.BasePressureValue; }

            set { SetProperty(ref BasePressureValue, value); }

        }

        /// <summary>
        /// Base temperature in Rankine
        /// </summary>
        private decimal BaseTemperatureValue = 520m;

        public decimal BaseTemperature

        {

            get { return this.BaseTemperatureValue; }

            set { SetProperty(ref BaseTemperatureValue, value); }

        }

        /// <summary>
        /// Z-factor
        /// </summary>
        private decimal ZFactorValue;

        public decimal ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
    }
}





