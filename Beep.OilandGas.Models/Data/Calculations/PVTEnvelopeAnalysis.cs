using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PVTEnvelopeAnalysis : ModelEntityBase
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
        private decimal MinPressureValue;

        public decimal MinPressure

        {

            get { return this.MinPressureValue; }

            set { SetProperty(ref MinPressureValue, value); }

        }
        private decimal MaxPressureValue;

        public decimal MaxPressure

        {

            get { return this.MaxPressureValue; }

            set { SetProperty(ref MaxPressureValue, value); }

        }
        private decimal MinTemperatureValue;

        public decimal MinTemperature

        {

            get { return this.MinTemperatureValue; }

            set { SetProperty(ref MinTemperatureValue, value); }

        }
        private decimal MaxTemperatureValue;

        public decimal MaxTemperature

        {

            get { return this.MaxTemperatureValue; }

            set { SetProperty(ref MaxTemperatureValue, value); }

        }
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private decimal BubblePointTemperatureValue;

        public decimal BubblePointTemperature

        {

            get { return this.BubblePointTemperatureValue; }

            set { SetProperty(ref BubblePointTemperatureValue, value); }

        }
        private decimal DewPointPressureValue;

        public decimal DewPointPressure

        {

            get { return this.DewPointPressureValue; }

            set { SetProperty(ref DewPointPressureValue, value); }

        }
        private decimal DewPointTemperatureValue;

        public decimal DewPointTemperature

        {

            get { return this.DewPointTemperatureValue; }

            set { SetProperty(ref DewPointTemperatureValue, value); }

        }
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private List<EnvelopePoint> EnvelopePointsValue = new();

        public List<EnvelopePoint> EnvelopePoints

        {

            get { return this.EnvelopePointsValue; }

            set { SetProperty(ref EnvelopePointsValue, value); }

        }
        private string EnvelopeTypeValue = string.Empty;

        public string EnvelopeType

        {

            get { return this.EnvelopeTypeValue; }

            set { SetProperty(ref EnvelopeTypeValue, value); }

        } // Type I, Type II, Type III, Type IV
    }
}
