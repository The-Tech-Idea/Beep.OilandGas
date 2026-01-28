using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class MeterConfiguration : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the meter identifier.
        /// </summary>
        private string MeterIdValue = string.Empty;

        public string MeterId

        {

            get { return this.MeterIdValue; }

            set { SetProperty(ref MeterIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter type.
        /// </summary>
        private string MeterTypeValue = string.Empty;

        public string MeterType

        {

            get { return this.MeterTypeValue; }

            set { SetProperty(ref MeterTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter factor.
        /// </summary>
        private decimal MeterFactorValue = 1.0m;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the last calibration date.
        /// </summary>
        private DateTime? LastCalibrationDateValue;

        public DateTime? LastCalibrationDate

        {

            get { return this.LastCalibrationDateValue; }

            set { SetProperty(ref LastCalibrationDateValue, value); }

        }
    }
}
