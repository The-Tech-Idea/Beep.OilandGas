namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    /// <summary>
    /// Represents well properties for hydraulic pump calculations.
    /// </summary>
    public class HydraulicPumpWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well depth in feet.
        /// </summary>
        private decimal WellDepthValue;

        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Tubing diameter in inches.
        /// </summary>
        private decimal TubingDiameterValue;

        public decimal TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }

        /// <summary>
        /// API gravity of the oil (degrees API).
        /// </summary>
        private decimal OilGravityValue;

        public decimal OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        }

        /// <summary>
        /// Water cut as a fraction (0-1).
        /// </summary>
        private decimal WaterCutValue;

        public decimal WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        private decimal BottomHolePressureValue;

        public decimal BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        private decimal WellheadPressureValue;

        public decimal WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }

        /// <summary>
        /// Average reservoir temperature in degrees Rankine.
        /// </summary>
        private decimal? ReservoirTemperatureValue;

        public decimal? ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }

        /// <summary>
        /// Well UWI (Unique Well Identifier).
        /// </summary>
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private int GasOilRatioValue;

        public int GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }
        private int GasSpecificGravityValue;

        public int GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private int DesiredProductionRateValue;

        public int DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        }

        /// <summary>
        /// Pump setting depth in feet.
        /// </summary>
        private decimal PumpDepthValue;

        public decimal PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }

        public decimal CasingDiameter { get; set; }
        public decimal WellheadTemperature { get; set; }
        public decimal BottomHoleTemperature { get; set; }
    }
}

