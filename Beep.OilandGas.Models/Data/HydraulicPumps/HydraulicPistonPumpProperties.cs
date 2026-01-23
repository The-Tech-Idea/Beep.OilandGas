namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    /// <summary>
    /// Represents piston pump properties for hydraulic pump calculations.
    /// </summary>
    public class HydraulicPistonPumpProperties : ModelEntityBase
    {
        /// <summary>
        /// Piston diameter in inches.
        /// </summary>
        private decimal PistonDiameterValue;

        public decimal PistonDiameter

        {

            get { return this.PistonDiameterValue; }

            set { SetProperty(ref PistonDiameterValue, value); }

        }

        /// <summary>
        /// Rod diameter in inches.
        /// </summary>
        private decimal RodDiameterValue;

        public decimal RodDiameter

        {

            get { return this.RodDiameterValue; }

            set { SetProperty(ref RodDiameterValue, value); }

        }

        /// <summary>
        /// Stroke length in inches.
        /// </summary>
        private decimal StrokeLengthValue;

        public decimal StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        }

        /// <summary>
        /// Strokes per minute (pump speed).
        /// </summary>
        private decimal StrokesPerMinuteValue;

        public decimal StrokesPerMinute

        {

            get { return this.StrokesPerMinuteValue; }

            set { SetProperty(ref StrokesPerMinuteValue, value); }

        }

        /// <summary>
        /// Power fluid pressure in psia.
        /// </summary>
        private decimal PowerFluidPressureValue;

        public decimal PowerFluidPressure

        {

            get { return this.PowerFluidPressureValue; }

            set { SetProperty(ref PowerFluidPressureValue, value); }

        }

        /// <summary>
        /// Power fluid rate in bbl/day.
        /// </summary>
        private decimal PowerFluidRateValue;

        public decimal PowerFluidRate

        {

            get { return this.PowerFluidRateValue; }

            set { SetProperty(ref PowerFluidRateValue, value); }

        }

        /// <summary>
        /// Power fluid specific gravity (relative to water).
        /// </summary>
        private decimal PowerFluidSpecificGravityValue;

        public decimal PowerFluidSpecificGravity

        {

            get { return this.PowerFluidSpecificGravityValue; }

            set { SetProperty(ref PowerFluidSpecificGravityValue, value); }

        }

        /// <summary>
        /// Pump displacement in cubic inches per stroke.
        /// </summary>
        private decimal? DisplacementValue;

        public decimal? Displacement

        {

            get { return this.DisplacementValue; }

            set { SetProperty(ref DisplacementValue, value); }

        }

        /// <summary>
        /// Pump volumetric efficiency (fraction 0-1).
        /// </summary>
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }

        /// <summary>
        /// Pump mechanical efficiency (fraction 0-1).
        /// </summary>
        private decimal MechanicalEfficiencyValue;

        public decimal MechanicalEfficiency

        {

            get { return this.MechanicalEfficiencyValue; }

            set { SetProperty(ref MechanicalEfficiencyValue, value); }

        }

        /// <summary>
        /// Pump manufacturer.
        /// </summary>
        private string? ManufacturerValue;

        public string? Manufacturer

        {

            get { return this.ManufacturerValue; }

            set { SetProperty(ref ManufacturerValue, value); }

        }

        /// <summary>
        /// Pump model/ID.
        /// </summary>
        private string? PumpIDValue;

        public string? PumpID

        {

            get { return this.PumpIDValue; }

            set { SetProperty(ref PumpIDValue, value); }

        }
    }
}

