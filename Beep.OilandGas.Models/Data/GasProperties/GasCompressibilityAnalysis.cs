using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class GasCompressibilityAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal IsothermalCompressibilityValue;

        public decimal IsothermalCompressibility

        {

            get { return this.IsothermalCompressibilityValue; }

            set { SetProperty(ref IsothermalCompressibilityValue, value); }

        }
        private decimal AdiabaticCompressibilityValue;

        public decimal AdiabaticCompressibility

        {

            get { return this.AdiabaticCompressibilityValue; }

            set { SetProperty(ref AdiabaticCompressibilityValue, value); }

        }
        private decimal ZFactorValue;

        public decimal ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
        private decimal CompressibilityFactorValue;

        public decimal CompressibilityFactor

        {

            get { return this.CompressibilityFactorValue; }

            set { SetProperty(ref CompressibilityFactorValue, value); }

        }
    }
}
