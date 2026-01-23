namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    /// <summary>
    /// Centrifugal compressor properties
    /// DTO for calculations - Entity class: CENTRIFUGAL_COMPRESSOR_PROPERTIES
    /// </summary>
    public class CentrifugalCompressorProperties : ModelEntityBase
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
        /// Specific heat ratio (k = cp/cv)
        /// </summary>
        private decimal SpecificHeatRatioValue = 1.3m;

        public decimal SpecificHeatRatio

        {

            get { return this.SpecificHeatRatioValue; }

            set { SetProperty(ref SpecificHeatRatioValue, value); }

        }

        /// <summary>
        /// Polytropic efficiency (0-1)
        /// </summary>
        private decimal PolytropicEfficiencyValue = 0.75m;

        public decimal PolytropicEfficiency

        {

            get { return this.PolytropicEfficiencyValue; }

            set { SetProperty(ref PolytropicEfficiencyValue, value); }

        }
        private int NumberOfStagesValue;

        public int NumberOfStages

        {

            get { return this.NumberOfStagesValue; }

            set { SetProperty(ref NumberOfStagesValue, value); }

        }
        private int SpeedValue;

        public int Speed

        {

            get { return this.SpeedValue; }

            set { SetProperty(ref SpeedValue, value); }

        }
    }
}





