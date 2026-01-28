using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WaxAnalysis : ModelEntityBase
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
        private decimal WaxAppearanceTemperatureValue;

        public decimal WaxAppearanceTemperature

        {

            get { return this.WaxAppearanceTemperatureValue; }

            set { SetProperty(ref WaxAppearanceTemperatureValue, value); }

        }
        private decimal PourPointValue;

        public decimal PourPoint

        {

            get { return this.PourPointValue; }

            set { SetProperty(ref PourPointValue, value); }

        }
        private decimal CloudPointValue;

        public decimal CloudPoint

        {

            get { return this.CloudPointValue; }

            set { SetProperty(ref CloudPointValue, value); }

        }
        private decimal WaxContentValue;

        public decimal WaxContent

        {

            get { return this.WaxContentValue; }

            set { SetProperty(ref WaxContentValue, value); }

        }
        private List<WaxFraction> WaxFractionsValue = new();

        public List<WaxFraction> WaxFractions

        {

            get { return this.WaxFractionsValue; }

            set { SetProperty(ref WaxFractionsValue, value); }

        }
        private string AnalysisMethodValue = string.Empty;

        public string AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
