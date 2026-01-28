
namespace Beep.OilandGas.Models.Data.PipelineAnalysis
{
    public class PipelineCapacityResult : ModelEntityBase
    {
        /// <summary>
        /// Maximum flow rate in Mscf/day (gas) or bbl/day (liquid)
        /// </summary>
        private decimal MaximumFlowRateValue;

        public decimal MaximumFlowRate

        {

            get { return this.MaximumFlowRateValue; }

            set { SetProperty(ref MaximumFlowRateValue, value); }

        }

        /// <summary>
        /// Pressure drop in psi
        /// </summary>
        private decimal PressureDropValue;

        public decimal PressureDrop

        {

            get { return this.PressureDropValue; }

            set { SetProperty(ref PressureDropValue, value); }

        }

        /// <summary>
        /// Flow velocity in ft/s
        /// </summary>
        private decimal FlowVelocityValue;

        public decimal FlowVelocity

        {

            get { return this.FlowVelocityValue; }

            set { SetProperty(ref FlowVelocityValue, value); }

        }

        /// <summary>
        /// Reynolds number
        /// </summary>
        private decimal ReynoldsNumberValue;

        public decimal ReynoldsNumber

        {

            get { return this.ReynoldsNumberValue; }

            set { SetProperty(ref ReynoldsNumberValue, value); }

        }

        /// <summary>
        /// Friction factor
        /// </summary>
        private decimal FrictionFactorValue;

        public decimal FrictionFactor

        {

            get { return this.FrictionFactorValue; }

            set { SetProperty(ref FrictionFactorValue, value); }

        }

        /// <summary>
        /// Pressure gradient in psia/ft
        /// </summary>
        private decimal PressureGradientValue;

        public decimal PressureGradient

        {

            get { return this.PressureGradientValue; }

            set { SetProperty(ref PressureGradientValue, value); }

        }

        /// <summary>
        /// Outlet pressure in psia
        /// </summary>
        private decimal OutletPressureValue;

        public decimal OutletPressure

        {

            get { return this.OutletPressureValue; }

            set { SetProperty(ref OutletPressureValue, value); }

        }

        // PPDM aliases for compatibility with PPDMCalculationService
        public decimal MAXIMUM_FLOW_RATE { get => MaximumFlowRate; set => MaximumFlowRate = value; }
        public decimal PRESSURE_DROP { get => PressureDrop; set => PressureDrop = value; }
        public decimal REYNOLDS_NUMBER { get => ReynoldsNumber; set => ReynoldsNumber = value; }
        public decimal FRICTION_FACTOR { get => FrictionFactor; set => FrictionFactor = value; }
        public decimal OUTLET_PRESSURE { get => OutletPressure; set => OutletPressure = value; }
    }
}
