using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PhaseEnvelope : ModelEntityBase
    {
        private string EnvelopeIdValue = string.Empty;

        public string EnvelopeId

        {

            get { return this.EnvelopeIdValue; }

            set { SetProperty(ref EnvelopeIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private List<PhaseEnvelopePoint> BubblePointCurveValue = new();

        public List<PhaseEnvelopePoint> BubblePointCurve

        {

            get { return this.BubblePointCurveValue; }

            set { SetProperty(ref BubblePointCurveValue, value); }

        }
        private List<PhaseEnvelopePoint> DewPointCurveValue = new();

        public List<PhaseEnvelopePoint> DewPointCurve

        {

            get { return this.DewPointCurveValue; }

            set { SetProperty(ref DewPointCurveValue, value); }

        }
        private PhaseEnvelopePoint CriticalPointValue = new();

        public PhaseEnvelopePoint CriticalPoint

        {

            get { return this.CriticalPointValue; }

            set { SetProperty(ref CriticalPointValue, value); }

        }
        private decimal CricondenthermValue;

        public decimal Cricondentherm

        {

            get { return this.CricondenthermValue; }

            set { SetProperty(ref CricondenthermValue, value); }

        }
        private decimal CricondenbarValue;

        public decimal Cricondenbar

        {

            get { return this.CricondenbarValue; }

            set { SetProperty(ref CricondenbarValue, value); }

        }
        private string QualityAssessmentValue = string.Empty;

        public string QualityAssessment

        {

            get { return this.QualityAssessmentValue; }

            set { SetProperty(ref QualityAssessmentValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
    }
}
