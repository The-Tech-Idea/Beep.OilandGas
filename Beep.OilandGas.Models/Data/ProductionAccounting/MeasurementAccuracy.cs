using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class MeasurementAccuracy : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the minimum required accuracy percentage.
        /// </summary>
        private decimal MinimumAccuracyValue = 99.5m;

        public decimal MinimumAccuracy

        {

            get { return this.MinimumAccuracyValue; }

            set { SetProperty(ref MinimumAccuracyValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum allowed error percentage.
        /// </summary>
        private decimal MaximumErrorValue = 0.5m;

        public decimal MaximumError

        {

            get { return this.MaximumErrorValue; }

            set { SetProperty(ref MaximumErrorValue, value); }

        }

        /// <summary>
        /// Gets or sets whether calibration is required.
        /// </summary>
        private bool CalibrationRequiredValue = true;

        public bool CalibrationRequired

        {

            get { return this.CalibrationRequiredValue; }

            set { SetProperty(ref CalibrationRequiredValue, value); }

        }

        /// <summary>
        /// Gets or sets the calibration frequency in days.
        /// </summary>
        private int CalibrationFrequencyDaysValue = 90;

        public int CalibrationFrequencyDays

        {

            get { return this.CalibrationFrequencyDaysValue; }

            set { SetProperty(ref CalibrationFrequencyDaysValue, value); }

        }
    }
}
