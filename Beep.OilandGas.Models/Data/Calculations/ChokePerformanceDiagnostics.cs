using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokePerformanceDiagnostics : ModelEntityBase
    {
        private string DiagnosticsIdValue = string.Empty;

        public string DiagnosticsId

        {

            get { return this.DiagnosticsIdValue; }

            set { SetProperty(ref DiagnosticsIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal MeasuredFlowRateValue;

        public decimal MeasuredFlowRate

        {

            get { return this.MeasuredFlowRateValue; }

            set { SetProperty(ref MeasuredFlowRateValue, value); }

        }
        private decimal ExpectedFlowRateValue;

        public decimal ExpectedFlowRate

        {

            get { return this.ExpectedFlowRateValue; }

            set { SetProperty(ref ExpectedFlowRateValue, value); }

        }
        private decimal FlowDeviationValue;

        public decimal FlowDeviation

        {

            get { return this.FlowDeviationValue; }

            set { SetProperty(ref FlowDeviationValue, value); }

        } // percent
        private decimal MeasuredDownstreamPressureValue;

        public decimal MeasuredDownstreamPressure

        {

            get { return this.MeasuredDownstreamPressureValue; }

            set { SetProperty(ref MeasuredDownstreamPressureValue, value); }

        }
        private decimal ExpectedDownstreamPressureValue;

        public decimal ExpectedDownstreamPressure

        {

            get { return this.ExpectedDownstreamPressureValue; }

            set { SetProperty(ref ExpectedDownstreamPressureValue, value); }

        }
        private decimal PressureDeviationValue;

        public decimal PressureDeviation

        {

            get { return this.PressureDeviationValue; }

            set { SetProperty(ref PressureDeviationValue, value); }

        } // psi
        private string StatusCodeValue = string.Empty;

        public string StatusCode

        {

            get { return this.StatusCodeValue; }

            set { SetProperty(ref StatusCodeValue, value); }

        } // Normal, Warning, Critical
        private List<string> IdentifiedIssuesValue = new();

        public List<string> IdentifiedIssues

        {

            get { return this.IdentifiedIssuesValue; }

            set { SetProperty(ref IdentifiedIssuesValue, value); }

        }
        private List<string> DiagnosticsDetailsValue = new();

        public List<string> DiagnosticsDetails

        {

            get { return this.DiagnosticsDetailsValue; }

            set { SetProperty(ref DiagnosticsDetailsValue, value); }

        }
        private decimal DischargeCoefficientDegradationValue;

        public decimal DischargeCoefficientDegradation

        {

            get { return this.DischargeCoefficientDegradationValue; }

            set { SetProperty(ref DischargeCoefficientDegradationValue, value); }

        } // percent
        private string ProbableCauseValue = string.Empty;

        public string ProbableCause

        {

            get { return this.ProbableCauseValue; }

            set { SetProperty(ref ProbableCauseValue, value); }

        }
        private string RecommendedActionValue = string.Empty;

        public string RecommendedAction

        {

            get { return this.RecommendedActionValue; }

            set { SetProperty(ref RecommendedActionValue, value); }

        }
    }
}
