namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    /// <summary>
    /// Represents ESP motor properties
    /// DTO for calculations - Entity class: ESP_MOTOR_PROPERTIES
    /// </summary>
    public class ESPMotorProperties : ModelEntityBase
    {
        /// <summary>
        /// Motor horsepower
        /// </summary>
        private decimal HorsepowerValue;

        public decimal Horsepower

        {

            get { return this.HorsepowerValue; }

            set { SetProperty(ref HorsepowerValue, value); }

        }

        /// <summary>
        /// Motor voltage
        /// </summary>
        private decimal VoltageValue;

        public decimal Voltage

        {

            get { return this.VoltageValue; }

            set { SetProperty(ref VoltageValue, value); }

        }

        /// <summary>
        /// Motor efficiency (0-1)
        /// </summary>
        private decimal EfficiencyValue = 0.9m;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }

        /// <summary>
        /// Power factor
        /// </summary>
        private decimal PowerFactorValue = 0.85m;

        public decimal PowerFactor

        {

            get { return this.PowerFactorValue; }

            set { SetProperty(ref PowerFactorValue, value); }

        }
    }
}





