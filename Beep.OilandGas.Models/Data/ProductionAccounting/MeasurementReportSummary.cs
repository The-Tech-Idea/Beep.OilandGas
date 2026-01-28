using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class MeasurementReportSummary : ModelEntityBase
    {
        private int MeasurementCountValue;

        public int MeasurementCount

        {

            get { return this.MeasurementCountValue; }

            set { SetProperty(ref MeasurementCountValue, value); }

        }
        private decimal AverageAccuracyValue;

        public decimal AverageAccuracy

        {

            get { return this.AverageAccuracyValue; }

            set { SetProperty(ref AverageAccuracyValue, value); }

        }
        private int CalibrationsDueValue;

        public int CalibrationsDue

        {

            get { return this.CalibrationsDueValue; }

            set { SetProperty(ref CalibrationsDueValue, value); }

        }
    }
}
