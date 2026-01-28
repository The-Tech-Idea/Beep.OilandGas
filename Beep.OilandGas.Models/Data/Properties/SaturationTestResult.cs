using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SaturationTestResult : ModelEntityBase
    {
        private string TestIdValue = string.Empty;

        public string TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private decimal SaturationPressureValue;

        public decimal SaturationPressure

        {

            get { return this.SaturationPressureValue; }

            set { SetProperty(ref SaturationPressureValue, value); }

        }
        private decimal SaturationTemperatureValue;

        public decimal SaturationTemperature

        {

            get { return this.SaturationTemperatureValue; }

            set { SetProperty(ref SaturationTemperatureValue, value); }

        }
        private string SaturationTypeValue = string.Empty;

        public string SaturationType

        {

            get { return this.SaturationTypeValue; }

            set { SetProperty(ref SaturationTypeValue, value); }

        } // Bubble or Dew
        private List<SaturationPoint> TestPointsValue = new();

        public List<SaturationPoint> TestPoints

        {

            get { return this.TestPointsValue; }

            set { SetProperty(ref TestPointsValue, value); }

        }
        private decimal CompressibilityAboveSaturationValue;

        public decimal CompressibilityAboveSaturation

        {

            get { return this.CompressibilityAboveSaturationValue; }

            set { SetProperty(ref CompressibilityAboveSaturationValue, value); }

        }
        private decimal CompressibilityBelowSaturationValue;

        public decimal CompressibilityBelowSaturation

        {

            get { return this.CompressibilityBelowSaturationValue; }

            set { SetProperty(ref CompressibilityBelowSaturationValue, value); }

        }
        private DateTime TestDateValue;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
    }
}
