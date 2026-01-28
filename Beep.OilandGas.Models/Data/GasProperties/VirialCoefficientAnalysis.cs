using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    public class VirialCoefficientAnalysis : ModelEntityBase
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
        private decimal SecondVirialCoefficientValue;

        public decimal SecondVirialCoefficient

        {

            get { return this.SecondVirialCoefficientValue; }

            set { SetProperty(ref SecondVirialCoefficientValue, value); }

        } // B
        private decimal ThirdVirialCoefficientValue;

        public decimal ThirdVirialCoefficient

        {

            get { return this.ThirdVirialCoefficientValue; }

            set { SetProperty(ref ThirdVirialCoefficientValue, value); }

        } // C
        private decimal ReducedTemperatureValue;

        public decimal ReducedTemperature

        {

            get { return this.ReducedTemperatureValue; }

            set { SetProperty(ref ReducedTemperatureValue, value); }

        }
        private decimal ReducedPressureValue;

        public decimal ReducedPressure

        {

            get { return this.ReducedPressureValue; }

            set { SetProperty(ref ReducedPressureValue, value); }

        }
    }
}
