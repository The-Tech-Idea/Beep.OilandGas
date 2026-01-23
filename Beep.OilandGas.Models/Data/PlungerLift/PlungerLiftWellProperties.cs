namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents plunger lift well properties
    /// DTO for calculations - Entity class: PLUNGER_LIFT_WELL_PROPERTIES
    /// </summary>
    public class PlungerLiftWellProperties : ModelEntityBase
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
        /// Plunger diameter in inches
        /// </summary>
        private decimal PlungerDiameterValue;

        public decimal PlungerDiameter

        {

            get { return this.PlungerDiameterValue; }

            set { SetProperty(ref PlungerDiameterValue, value); }

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
        /// Casing pressure in psia
        /// </summary>
        private decimal CasingPressureValue;

        public decimal CasingPressure

        {

            get { return this.CasingPressureValue; }

            set { SetProperty(ref CasingPressureValue, value); }

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
        /// Wellhead temperature in Rankine
        /// </summary>
        private decimal WellheadTemperatureValue;

        public decimal WellheadTemperature

        {

            get { return this.WellheadTemperatureValue; }

            set { SetProperty(ref WellheadTemperatureValue, value); }

        }

        /// <summary>
        /// Bottom hole temperature in Rankine
        /// </summary>
        private decimal BottomHoleTemperatureValue;

        public decimal BottomHoleTemperature

        {

            get { return this.BottomHoleTemperatureValue; }

            set { SetProperty(ref BottomHoleTemperatureValue, value); }

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
        /// Liquid production rate in bbl/day
        /// </summary>
        private decimal LiquidProductionRateValue;

        public decimal LiquidProductionRate

        {

            get { return this.LiquidProductionRateValue; }

            set { SetProperty(ref LiquidProductionRateValue, value); }

        }
    }
}




