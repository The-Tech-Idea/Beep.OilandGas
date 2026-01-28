using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class MeasurementCorrections : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the temperature correction factor.
        /// </summary>
        private decimal TemperatureCorrectionFactorValue = 1.0m;

        public decimal TemperatureCorrectionFactor

        {

            get { return this.TemperatureCorrectionFactorValue; }

            set { SetProperty(ref TemperatureCorrectionFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the pressure correction factor.
        /// </summary>
        private decimal PressureCorrectionFactorValue = 1.0m;

        public decimal PressureCorrectionFactor

        {

            get { return this.PressureCorrectionFactorValue; }

            set { SetProperty(ref PressureCorrectionFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter factor (calibration correction).
        /// </summary>
        private decimal MeterFactorValue = 1.0m;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the shrinkage factor.
        /// </summary>
        private decimal ShrinkageFactorValue = 1.0m;

        public decimal ShrinkageFactor

        {

            get { return this.ShrinkageFactorValue; }

            set { SetProperty(ref ShrinkageFactorValue, value); }

        }

        /// <summary>
        /// Applies all corrections to a volume.
        /// </summary>
        public decimal ApplyCorrections(decimal volume)
        {
            return volume * TemperatureCorrectionFactor
                         * PressureCorrectionFactor
                         * MeterFactor
                         * ShrinkageFactor;
        }
    }
}
