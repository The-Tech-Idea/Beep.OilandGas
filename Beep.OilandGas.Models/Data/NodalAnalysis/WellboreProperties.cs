namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Wellbore properties for nodal analysis
    /// DTO for calculations - Entity class: NODAL_WELLBORE_PROPERTIES
    /// </summary>
    public class WellboreProperties : ModelEntityBase
    {
        /// <summary>
        /// Tubing diameter (inches or mm)
        /// </summary>
        private double TubingDiameterValue;

        public double TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }

        /// <summary>
        /// Tubing length (feet or meters)
        /// </summary>
        private double TubingLengthValue;

        public double TubingLength

        {

            get { return this.TubingLengthValue; }

            set { SetProperty(ref TubingLengthValue, value); }

        }

        /// <summary>
        /// Wellhead pressure (psia or kPa)
        /// </summary>
        private double WellheadPressureValue;

        public double WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }

        /// <summary>
        /// Water cut (fraction, 0 to 1)
        /// </summary>
        private double WaterCutValue;

        public double WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }

        /// <summary>
        /// Gas-oil ratio (scf/STB or m³/m³)
        /// </summary>
        private double GasOilRatioValue;

        public double GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }

        /// <summary>
        /// Oil gravity (API degrees)
        /// </summary>
        private double OilGravityValue;

        public double OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        private double GasSpecificGravityValue;

        public double GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }

        /// <summary>
        /// Wellhead temperature (°F or °C)
        /// </summary>
        private double WellheadTemperatureValue;

        public double WellheadTemperature

        {

            get { return this.WellheadTemperatureValue; }

            set { SetProperty(ref WellheadTemperatureValue, value); }

        }

        /// <summary>
        /// Bottomhole temperature (°F or °C)
        /// </summary>
        private double BottomholeTemperatureValue;

        public double BottomholeTemperature

        {

            get { return this.BottomholeTemperatureValue; }

            set { SetProperty(ref BottomholeTemperatureValue, value); }

        }

        /// <summary>
        /// Tubing roughness (inches or mm)
        /// </summary>
        private double TubingRoughnessValue;

        public double TubingRoughness

        {

            get { return this.TubingRoughnessValue; }

            set { SetProperty(ref TubingRoughnessValue, value); }

        }
    }
}




