
namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public class LiquidPipelineFlowProperties : ModelEntityBase
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
        /// Liquid flow rate in bbl/day
        /// </summary>
        private decimal LiquidFlowRateValue;

        public decimal LiquidFlowRate

        {

            get { return this.LiquidFlowRateValue; }

            set { SetProperty(ref LiquidFlowRateValue, value); }

        }

        /// <summary>
        /// Liquid specific gravity
        /// </summary>
        private decimal LiquidSpecificGravityValue;

        public decimal LiquidSpecificGravity

        {

            get { return this.LiquidSpecificGravityValue; }

            set { SetProperty(ref LiquidSpecificGravityValue, value); }

        }

        /// <summary>
        /// Liquid viscosity in cp
        /// </summary>
        private decimal LiquidViscosityValue = 1.0m;

        public decimal LiquidViscosity

        {

            get { return this.LiquidViscosityValue; }

            set { SetProperty(ref LiquidViscosityValue, value); }

        }

        /// <summary>
        /// Liquid density
        /// </summary>
        private decimal LiquidDensityValue;

        public decimal LiquidDensity

        {

            get { return this.LiquidDensityValue; }

            set { SetProperty(ref LiquidDensityValue, value); }

        }
    }
}
