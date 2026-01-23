namespace Beep.OilandGas.Models.Data.CompressorAnalysis
{
    /// <summary>
    /// Compressor power calculation results
    /// DTO for calculations - Entity class: COMPRESSOR_POWER_RESULT
    /// </summary>
    public class CompressorPowerResult : ModelEntityBase
    {
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
        /// Adiabatic head in feet
        /// </summary>
        private decimal AdiabaticHeadValue;

        public decimal AdiabaticHead

        {

            get { return this.AdiabaticHeadValue; }

            set { SetProperty(ref AdiabaticHeadValue, value); }

        }

        /// <summary>
        /// Polytropic head in feet
        /// </summary>
        private decimal PolytropicHeadValue;

        public decimal PolytropicHead

        {

            get { return this.PolytropicHeadValue; }

            set { SetProperty(ref PolytropicHeadValue, value); }

        }

        /// <summary>
        /// Theoretical power in horsepower
        /// </summary>
        private decimal TheoreticalPowerValue;

        public decimal TheoreticalPower

        {

            get { return this.TheoreticalPowerValue; }

            set { SetProperty(ref TheoreticalPowerValue, value); }

        }

        /// <summary>
        /// Brake horsepower
        /// </summary>
        private decimal BrakeHorsepowerValue;

        public decimal BrakeHorsepower

        {

            get { return this.BrakeHorsepowerValue; }

            set { SetProperty(ref BrakeHorsepowerValue, value); }

        }

        /// <summary>
        /// Motor horsepower
        /// </summary>
        private decimal MotorHorsepowerValue;

        public decimal MotorHorsepower

        {

            get { return this.MotorHorsepowerValue; }

            set { SetProperty(ref MotorHorsepowerValue, value); }

        }

        /// <summary>
        /// Power consumption in kW
        /// </summary>
        private decimal PowerConsumptionKWValue;

        public decimal PowerConsumptionKW

        {

            get { return this.PowerConsumptionKWValue; }

            set { SetProperty(ref PowerConsumptionKWValue, value); }

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
        /// Overall efficiency (0-1)
        /// </summary>
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        }
    }
}




