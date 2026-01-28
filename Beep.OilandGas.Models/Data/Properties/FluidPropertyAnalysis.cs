using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FluidPropertyAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string FluidTypeValue = string.Empty;

        public string FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        } // Oil, Gas, Condensate
        private decimal ReservoirTemperatureValue;

        public decimal ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private decimal DewPointPressureValue;

        public decimal DewPointPressure

        {

            get { return this.DewPointPressureValue; }

            set { SetProperty(ref DewPointPressureValue, value); }

        }
        private decimal GORValue;

        public decimal GOR

        {

            get { return this.GORValue; }

            set { SetProperty(ref GORValue, value); }

        }
        private decimal CGRValue;

        public decimal CGR

        {

            get { return this.CGRValue; }

            set { SetProperty(ref CGRValue, value); }

        }
        private decimal WaterCutValue;

        public decimal WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }
        private string FluidClassificationValue = string.Empty;

        public string FluidClassification

        {

            get { return this.FluidClassificationValue; }

            set { SetProperty(ref FluidClassificationValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private List<PropertyResult> CalculatedPropertiesValue = new();

        public List<PropertyResult> CalculatedProperties

        {

            get { return this.CalculatedPropertiesValue; }

            set { SetProperty(ref CalculatedPropertiesValue, value); }

        }
    }
}
