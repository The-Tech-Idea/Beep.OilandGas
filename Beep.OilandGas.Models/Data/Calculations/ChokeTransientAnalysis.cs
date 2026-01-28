using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeTransientAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal InitialUpstreamPressureValue;

        public decimal InitialUpstreamPressure

        {

            get { return this.InitialUpstreamPressureValue; }

            set { SetProperty(ref InitialUpstreamPressureValue, value); }

        }
        private decimal InitialTemperatureValue;

        public decimal InitialTemperature

        {

            get { return this.InitialTemperatureValue; }

            set { SetProperty(ref InitialTemperatureValue, value); }

        }
        private decimal FinalUpstreamPressureValue;

        public decimal FinalUpstreamPressure

        {

            get { return this.FinalUpstreamPressureValue; }

            set { SetProperty(ref FinalUpstreamPressureValue, value); }

        }
        private decimal FinalTemperatureValue;

        public decimal FinalTemperature

        {

            get { return this.FinalTemperatureValue; }

            set { SetProperty(ref FinalTemperatureValue, value); }

        }
        private decimal ChangeRateValue;

        public decimal ChangeRate

        {

            get { return this.ChangeRateValue; }

            set { SetProperty(ref ChangeRateValue, value); }

        } // psi/hour or °R/hour
        private decimal TransientDurationValue;

        public decimal TransientDuration

        {

            get { return this.TransientDurationValue; }

            set { SetProperty(ref TransientDurationValue, value); }

        } // hours
        private decimal AverageFlowRateValue;

        public decimal AverageFlowRate

        {

            get { return this.AverageFlowRateValue; }

            set { SetProperty(ref AverageFlowRateValue, value); }

        }
        private decimal PeakFlowRateValue;

        public decimal PeakFlowRate

        {

            get { return this.PeakFlowRateValue; }

            set { SetProperty(ref PeakFlowRateValue, value); }

        }
        private decimal MinimumFlowRateValue;

        public decimal MinimumFlowRate

        {

            get { return this.MinimumFlowRateValue; }

            set { SetProperty(ref MinimumFlowRateValue, value); }

        }
        private decimal TemperatureEffectValue;

        public decimal TemperatureEffect

        {

            get { return this.TemperatureEffectValue; }

            set { SetProperty(ref TemperatureEffectValue, value); }

        } // psi equivalent
        private decimal PressureEffectValue;

        public decimal PressureEffect

        {

            get { return this.PressureEffectValue; }

            set { SetProperty(ref PressureEffectValue, value); }

        } // psi
        private List<TransientPoint> TransientCurveValue = new();

        public List<TransientPoint> TransientCurve

        {

            get { return this.TransientCurveValue; }

            set { SetProperty(ref TransientCurveValue, value); }

        }
        private string TransientTypeValue = string.Empty;

        public string TransientType

        {

            get { return this.TransientTypeValue; }

            set { SetProperty(ref TransientTypeValue, value); }

        } // Pressure, Temperature, Both
    }
}
