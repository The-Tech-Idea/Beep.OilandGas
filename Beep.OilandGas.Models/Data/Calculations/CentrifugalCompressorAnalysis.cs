using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CentrifugalCompressorAnalysis : ModelEntityBase
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
        private decimal InletFlowRateValue;

        public decimal InletFlowRate

        {

            get { return this.InletFlowRateValue; }

            set { SetProperty(ref InletFlowRateValue, value); }

        }
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal ImpellerDiameterValue;

        public decimal ImpellerDiameter

        {

            get { return this.ImpellerDiameterValue; }

            set { SetProperty(ref ImpellerDiameterValue, value); }

        }
        private decimal ImpellerSpeedValue;

        public decimal ImpellerSpeed

        {

            get { return this.ImpellerSpeedValue; }

            set { SetProperty(ref ImpellerSpeedValue, value); }

        }
        private decimal HeadDevelopedValue;

        public decimal HeadDeveloped

        {

            get { return this.HeadDevelopedValue; }

            set { SetProperty(ref HeadDevelopedValue, value); }

        }
        private decimal SurgeMarginValue;

        public decimal SurgeMargin

        {

            get { return this.SurgeMarginValue; }

            set { SetProperty(ref SurgeMarginValue, value); }

        }
        private decimal PolyIsentropicHeadValue;

        public decimal PolyIsentropicHead

        {

            get { return this.PolyIsentropicHeadValue; }

            set { SetProperty(ref PolyIsentropicHeadValue, value); }

        }
        private decimal ActualHeadValue;

        public decimal ActualHead

        {

            get { return this.ActualHeadValue; }

            set { SetProperty(ref ActualHeadValue, value); }

        }
        private decimal StallMarginValue;

        public decimal StallMargin

        {

            get { return this.StallMarginValue; }

            set { SetProperty(ref StallMarginValue, value); }

        }
        private string OperatingRegionValue = string.Empty;

        public string OperatingRegion

        {

            get { return this.OperatingRegionValue; }

            set { SetProperty(ref OperatingRegionValue, value); }

        } // Normal, Stall, Surge
    }
}
