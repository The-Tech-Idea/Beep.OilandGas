namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Comprehensive well properties for gas lift analysis and design
    /// DTO for calculations - Entity class: GAS_LIFT_WELL_PROPERTIES
    /// Includes all parameters needed for industry-standard gas lift engineering calculations
    /// </summary>
    public class GasLiftWellProperties : ModelEntityBase
    {
        /// <summary>
        /// Well identifier (UWI - Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Well depth (in feet or meters depending on units)
        /// </summary>
        private decimal WellDepthValue;

        public decimal WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        }

        /// <summary>
        /// Well type (Vertical, Deviated, Horizontal)
        /// </summary>
        private string WellTypeValue = "Vertical";

        public string WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }

        /// <summary>
        /// Wellhead pressure (in psia or kPa)
        /// </summary>
        private decimal WellheadPressureValue;

        public decimal WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }

        /// <summary>
        /// Bottom hole pressure (in psia or kPa)
        /// </summary>
        private decimal BottomHolePressureValue;

        public decimal BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }

        /// <summary>
        /// Static reservoir pressure (in psia or kPa)
        /// </summary>
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }

        /// <summary>
        /// Gas-oil ratio (scf/STB or m³/m³)
        /// </summary>
        private decimal GasOilRatioValue;

        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }

        /// <summary>
        /// Desired production rate (STB/day or m³/day)
        /// </summary>
        private decimal DesiredProductionRateValue;

        public decimal DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        }

        /// <summary>
        /// Current production rate (STB/day or m³/day)
        /// </summary>
        private decimal CurrentProductionRateValue;

        public decimal CurrentProductionRate

        {

            get { return this.CurrentProductionRateValue; }

            set { SetProperty(ref CurrentProductionRateValue, value); }

        }

        /// <summary>
        /// Oil gravity (API degrees)
        /// </summary>
        private decimal OilGravityValue;

        public decimal OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        }

        /// <summary>
        /// Water cut (fraction, 0 to 1)
        /// </summary>
        private decimal WaterCutValue;

        public decimal WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }

        /// <summary>
        /// Wellhead temperature (in °F or °C)
        /// </summary>
        private decimal WellheadTemperatureValue;

        public decimal WellheadTemperature

        {

            get { return this.WellheadTemperatureValue; }

            set { SetProperty(ref WellheadTemperatureValue, value); }

        }

        /// <summary>
        /// Bottom hole temperature (in °F or °C)
        /// </summary>
        private decimal BottomHoleTemperatureValue;

        public decimal BottomHoleTemperature

        {

            get { return this.BottomHoleTemperatureValue; }

            set { SetProperty(ref BottomHoleTemperatureValue, value); }

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
        /// Tubing diameter (in inches or mm)
        /// </summary>
        private int TubingDiameterValue;

        public int TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }

        /// <summary>
        /// Tubing pressure rating (in psia or kPa)
        /// </summary>
        private decimal TubingPressureRatingValue = 5000m;

        public decimal TubingPressureRating

        {

            get { return this.TubingPressureRatingValue; }

            set { SetProperty(ref TubingPressureRatingValue, value); }

        } // Default 5000 psia

        /// <summary>
        /// Casing pressure rating (in psia or kPa)
        /// </summary>
        private decimal CasingPressureRatingValue = 3000m;

        public decimal CasingPressureRating

        {

            get { return this.CasingPressureRatingValue; }

            set { SetProperty(ref CasingPressureRatingValue, value); }

        } // Default 3000 psia

        /// <summary>
        /// Tubing ID (inner diameter in inches)
        /// </summary>
        private decimal TubingIDValue;

        public decimal TubingID

        {

            get { return this.TubingIDValue; }

            set { SetProperty(ref TubingIDValue, value); }

        }

        /// <summary>
        /// Tubing wall thickness (in inches)
        /// </summary>
        private decimal TubingThicknessValue;

        public decimal TubingThickness

        {

            get { return this.TubingThicknessValue; }

            set { SetProperty(ref TubingThicknessValue, value); }

        }

        /// <summary>
        /// CO2 content in producing gas (mole fraction, 0-1)
        /// </summary>
        private decimal CO2ContentValue;

        public decimal CO2Content

        {

            get { return this.CO2ContentValue; }

            set { SetProperty(ref CO2ContentValue, value); }

        }

        /// <summary>
        /// H2S content in producing gas (mole fraction, 0-1)
        /// </summary>
        private decimal H2SContentValue;

        public decimal H2SContent

        {

            get { return this.H2SContentValue; }

            set { SetProperty(ref H2SContentValue, value); }

        }

        /// <summary>
        /// Permeability of producing interval (in md)
        /// </summary>
        private decimal PermeabilityValue;

        public decimal Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }

        /// <summary>
        /// Porosity of producing interval (fraction, 0-1)
        /// </summary>
        private decimal PorosityValue;

        public decimal Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }

        /// <summary>
        /// Net pay thickness (in feet or meters)
        /// </summary>
        private decimal NetPayThicknessValue;

        public decimal NetPayThickness

        {

            get { return this.NetPayThicknessValue; }

            set { SetProperty(ref NetPayThicknessValue, value); }

        }

        /// <summary>
        /// Casing diameter (in inches or mm)
        /// </summary>
        private decimal CasingDiameterValue;

        public decimal CasingDiameter

        {

            get { return this.CasingDiameterValue; }

            set { SetProperty(ref CasingDiameterValue, value); }

        }
    }
}





