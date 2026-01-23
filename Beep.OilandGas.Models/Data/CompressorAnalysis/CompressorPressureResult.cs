namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    /// <summary>
    /// Compressor pressure calculation results
    /// DTO for calculations - Entity class: COMPRESSOR_PRESSURE_RESULT
    /// </summary>
    public class CompressorPressureResult : ModelEntityBase
    {
        /// <summary>
        /// Required discharge pressure in psia
        /// </summary>
        private decimal RequiredDischargePressureValue;

        public decimal RequiredDischargePressure

        {

            get { return this.RequiredDischargePressureValue; }

            set { SetProperty(ref RequiredDischargePressureValue, value); }

        }

        /// <summary>
        /// Compression ratio
        /// </summary>
        private decimal CompressionRatioValue;

        public decimal CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }

        /// <summary>
        /// Required power in horsepower
        /// </summary>
        private decimal RequiredPowerValue;

        public decimal RequiredPower

        {

            get { return this.RequiredPowerValue; }

            set { SetProperty(ref RequiredPowerValue, value); }

        }

        /// <summary>
        /// Discharge temperature in Rankine
        /// </summary>
        private decimal DischargeTemperatureValue;

        public decimal DischargeTemperature

        {

            get { return this.DischargeTemperatureValue; }

            set { SetProperty(ref DischargeTemperatureValue, value); }

        }

        /// <summary>
        /// Whether the operation is feasible
        /// </summary>
        private bool IsFeasibleValue;

        public bool IsFeasible

        {

            get { return this.IsFeasibleValue; }

            set { SetProperty(ref IsFeasibleValue, value); }

        }
    }
}




