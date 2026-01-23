namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    /// <summary>
    /// Represents jet pump properties for hydraulic pump calculations.
    /// </summary>
    public class HydraulicJetPumpProperties : ModelEntityBase
    {
        /// <summary>
        /// Nozzle diameter in inches.
        /// </summary>
        private decimal NozzleDiameterValue;

        public decimal NozzleDiameter

        {

            get { return this.NozzleDiameterValue; }

            set { SetProperty(ref NozzleDiameterValue, value); }

        }

        /// <summary>
        /// Throat diameter in inches.
        /// </summary>
        private decimal ThroatDiameterValue;

        public decimal ThroatDiameter

        {

            get { return this.ThroatDiameterValue; }

            set { SetProperty(ref ThroatDiameterValue, value); }

        }

        /// <summary>
        /// Diffuser diameter in inches.
        /// </summary>
        private decimal? DiffuserDiameterValue;

        public decimal? DiffuserDiameter

        {

            get { return this.DiffuserDiameterValue; }

            set { SetProperty(ref DiffuserDiameterValue, value); }

        }

        /// <summary>
        /// Power fluid flow rate in bbl/day.
        /// </summary>
        private decimal PowerFluidRateValue;

        public decimal PowerFluidRate

        {

            get { return this.PowerFluidRateValue; }

            set { SetProperty(ref PowerFluidRateValue, value); }

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
        /// Power fluid specific gravity (relative to water).
        /// </summary>
        private decimal PowerFluidSpecificGravityValue;

        public decimal PowerFluidSpecificGravity

        {

            get { return this.PowerFluidSpecificGravityValue; }

            set { SetProperty(ref PowerFluidSpecificGravityValue, value); }

        }

        /// <summary>
        /// Production fluid type (Oil, Water, Gas).
        /// </summary>
        private string? ProductionFluidTypeValue;

        public string? ProductionFluidType

        {

            get { return this.ProductionFluidTypeValue; }

            set { SetProperty(ref ProductionFluidTypeValue, value); }

        }

        /// <summary>
        /// Pump depth in feet.
        /// </summary>
        private decimal? PumpDepthValue;

        public decimal? PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }

        /// <summary>
        /// Pump ID/serial number.
        /// </summary>
        private string? PumpIDValue;

        public string? PumpID

        {

            get { return this.PumpIDValue; }

            set { SetProperty(ref PumpIDValue, value); }

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
    }
}

