using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TestResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the test result identifier.
        /// </summary>
        private string TestResultIdValue = string.Empty;

        public string TestResultId

        {

            get { return this.TestResultIdValue; }

            set { SetProperty(ref TestResultIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the test date.
        /// </summary>
        private DateTime TestDateValue;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil volume in barrels.
        /// </summary>
        private decimal OilVolumeValue;

        public decimal OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas volume in MCF.
        /// </summary>
        private decimal GasVolumeValue;

        public decimal GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the water volume in barrels.
        /// </summary>
        private decimal WaterVolumeValue;

        public decimal WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the test duration in hours.
        /// </summary>
        private decimal TestDurationHoursValue;

        public decimal TestDurationHours

        {

            get { return this.TestDurationHoursValue; }

            set { SetProperty(ref TestDurationHoursValue, value); }

        }
    }
}
