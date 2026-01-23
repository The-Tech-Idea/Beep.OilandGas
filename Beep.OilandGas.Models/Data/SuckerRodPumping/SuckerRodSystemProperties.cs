namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod pumping system properties
    /// DTO for calculations - Entity class: SUCKER_ROD_SYSTEM_PROPERTIES
    /// </summary>
    public class SuckerRodSystemProperties : ModelEntityBase
    {
        /// <summary>
        /// Well depth in feet
        /// </summary>
        private decimal WellDepthValue;

        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Tubing diameter in inches
        /// </summary>
        private decimal TubingDiameterValue;

        public decimal TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }

        /// <summary>
        /// Rod diameter in inches
        /// </summary>
        private decimal RodDiameterValue;

        public decimal RodDiameter

        {

            get { return this.RodDiameterValue; }

            set { SetProperty(ref RodDiameterValue, value); }

        }

        /// <summary>
        /// Pump diameter in inches
        /// </summary>
        private decimal PumpDiameterValue;

        public decimal PumpDiameter

        {

            get { return this.PumpDiameterValue; }

            set { SetProperty(ref PumpDiameterValue, value); }

        }

        /// <summary>
        /// Stroke length in inches
        /// </summary>
        private decimal StrokeLengthValue;

        public decimal StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        }

        /// <summary>
        /// Strokes per minute (SPM)
        /// </summary>
        private decimal StrokesPerMinuteValue;

        public decimal StrokesPerMinute

        {

            get { return this.StrokesPerMinuteValue; }

            set { SetProperty(ref StrokesPerMinuteValue, value); }

        }

        /// <summary>
        /// Wellhead pressure in psia
        /// </summary>
        private decimal WellheadPressureValue;

        public decimal WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }

        /// <summary>
        /// Bottom hole pressure in psia
        /// </summary>
        private decimal BottomHolePressureValue;

        public decimal BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }

        /// <summary>
        /// Oil gravity in API
        /// </summary>
        private decimal OilGravityValue;

        public decimal OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        }

        /// <summary>
        /// Water cut (fraction, 0-1)
        /// </summary>
        private decimal WaterCutValue;

        public decimal WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }

        /// <summary>
        /// Gas-oil ratio in scf/bbl
        /// </summary>
        private decimal GasOilRatioValue;

        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

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
        /// Rod material density in lb/ft³
        /// </summary>
        private decimal RodDensityValue = 490m;

        public decimal RodDensity

        {

            get { return this.RodDensityValue; }

            set { SetProperty(ref RodDensityValue, value); }

        } // Steel

        /// <summary>
        /// Pump efficiency (0-1)
        /// </summary>
        private decimal PumpEfficiencyValue = 0.85m;

        public decimal PumpEfficiency

        {

            get { return this.PumpEfficiencyValue; }

            set { SetProperty(ref PumpEfficiencyValue, value); }

        }

        /// <summary>
        /// Fluid level
        /// </summary>
        private decimal FluidLevelValue;

        public decimal FluidLevel

        {

            get { return this.FluidLevelValue; }

            set { SetProperty(ref FluidLevelValue, value); }

        }

        /// <summary>
        /// Fluid density
        /// </summary>
        private decimal FluidDensityValue;

        public decimal FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        }
    }
}





