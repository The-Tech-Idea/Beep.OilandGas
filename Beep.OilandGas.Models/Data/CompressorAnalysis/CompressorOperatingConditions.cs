namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    /// <summary>
    /// Operating conditions for compressors
    /// DTO for calculations - Entity class: COMPRESSOR_OPERATING_CONDITIONS
    /// </summary>
    public class CompressorOperatingConditions : ModelEntityBase
    {
        /// <summary>
        /// Suction pressure in psia
        /// </summary>
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        }

        /// <summary>
        /// Discharge pressure in psia
        /// </summary>
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }

        /// <summary>
        /// Suction temperature in Rankine
        /// </summary>
        private decimal SuctionTemperatureValue;

        public decimal SuctionTemperature

        {

            get { return this.SuctionTemperatureValue; }

            set { SetProperty(ref SuctionTemperatureValue, value); }

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
        /// Gas flow rate in Mscf/day
        /// </summary>
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

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
        /// Compressor efficiency (0-1)
        /// </summary>
        private decimal CompressorEfficiencyValue = 0.75m;

        public decimal CompressorEfficiency

        {

            get { return this.CompressorEfficiencyValue; }

            set { SetProperty(ref CompressorEfficiencyValue, value); }

        }

        /// <summary>
        /// Mechanical efficiency (0-1)
        /// </summary>
        private decimal MechanicalEfficiencyValue = 0.95m;

        public decimal MechanicalEfficiency

        {

            get { return this.MechanicalEfficiencyValue; }

            set { SetProperty(ref MechanicalEfficiencyValue, value); }

        }
    }
}





