namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    /// <summary>
    /// Reciprocating compressor properties
    /// DTO for calculations - Entity class: RECIPROCATING_COMPRESSOR_PROPERTIES
    /// </summary>
    public class ReciprocatingCompressorProperties : ModelEntityBase
    {
        /// <summary>
        /// Operating conditions
        /// </summary>
        private CompressorOperatingConditions OperatingConditionsValue = new();

        public CompressorOperatingConditions OperatingConditions

        {

            get { return this.OperatingConditionsValue; }

            set { SetProperty(ref OperatingConditionsValue, value); }

        }

        /// <summary>
        /// Cylinder diameter in inches
        /// </summary>
        private decimal CylinderDiameterValue;

        public decimal CylinderDiameter

        {

            get { return this.CylinderDiameterValue; }

            set { SetProperty(ref CylinderDiameterValue, value); }

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
        /// Rotational speed in RPM
        /// </summary>
        private decimal RotationalSpeedValue;

        public decimal RotationalSpeed

        {

            get { return this.RotationalSpeedValue; }

            set { SetProperty(ref RotationalSpeedValue, value); }

        }

        /// <summary>
        /// Number of cylinders
        /// </summary>
        private int NumberOfCylindersValue = 1;

        public int NumberOfCylinders

        {

            get { return this.NumberOfCylindersValue; }

            set { SetProperty(ref NumberOfCylindersValue, value); }

        }

        /// <summary>
        /// Volumetric efficiency (0-1)
        /// </summary>
        private decimal VolumetricEfficiencyValue = 0.85m;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }
        private int ClearanceFactorValue;

        public int ClearanceFactor

        {

            get { return this.ClearanceFactorValue; }

            set { SetProperty(ref ClearanceFactorValue, value); }

        }
    }
}





