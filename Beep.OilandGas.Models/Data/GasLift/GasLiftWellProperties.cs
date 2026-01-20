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
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Well depth (in feet or meters depending on units)
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Well type (Vertical, Deviated, Horizontal)
        /// </summary>
        public string WellType { get; set; } = "Vertical";

        /// <summary>
        /// Wellhead pressure (in psia or kPa)
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure (in psia or kPa)
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Static reservoir pressure (in psia or kPa)
        /// </summary>
        public decimal ReservoirPressure { get; set; }

        /// <summary>
        /// Gas-oil ratio (scf/STB or m³/m³)
        /// </summary>
        public decimal GasOilRatio { get; set; }

        /// <summary>
        /// Desired production rate (STB/day or m³/day)
        /// </summary>
        public decimal DesiredProductionRate { get; set; }

        /// <summary>
        /// Current production rate (STB/day or m³/day)
        /// </summary>
        public decimal CurrentProductionRate { get; set; }

        /// <summary>
        /// Oil gravity (API degrees)
        /// </summary>
        public decimal OilGravity { get; set; }

        /// <summary>
        /// Water cut (fraction, 0 to 1)
        /// </summary>
        public decimal WaterCut { get; set; }

        /// <summary>
        /// Wellhead temperature (in °F or °C)
        /// </summary>
        public decimal WellheadTemperature { get; set; }

        /// <summary>
        /// Bottom hole temperature (in °F or °C)
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Tubing diameter (in inches or mm)
        /// </summary>
        public int TubingDiameter { get; set; }

        /// <summary>
        /// Tubing pressure rating (in psia or kPa)
        /// </summary>
        public decimal TubingPressureRating { get; set; } = 5000m; // Default 5000 psia

        /// <summary>
        /// Casing pressure rating (in psia or kPa)
        /// </summary>
        public decimal CasingPressureRating { get; set; } = 3000m; // Default 3000 psia

        /// <summary>
        /// Tubing ID (inner diameter in inches)
        /// </summary>
        public decimal TubingID { get; set; }

        /// <summary>
        /// Tubing wall thickness (in inches)
        /// </summary>
        public decimal TubingThickness { get; set; }

        /// <summary>
        /// CO2 content in producing gas (mole fraction, 0-1)
        /// </summary>
        public decimal CO2Content { get; set; }

        /// <summary>
        /// H2S content in producing gas (mole fraction, 0-1)
        /// </summary>
        public decimal H2SContent { get; set; }

        /// <summary>
        /// Permeability of producing interval (in md)
        /// </summary>
        public decimal Permeability { get; set; }

        /// <summary>
        /// Porosity of producing interval (fraction, 0-1)
        /// </summary>
        public decimal Porosity { get; set; }

        /// <summary>
        /// Net pay thickness (in feet or meters)
        /// </summary>
        public decimal NetPayThickness { get; set; }
    }
}



