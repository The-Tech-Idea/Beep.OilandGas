using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class AnalysisComparisonResult : ModelEntityBase
    {
        private string ComparisonIdValue = string.Empty;

        public string ComparisonId

        {

            get { return this.ComparisonIdValue; }

            set { SetProperty(ref ComparisonIdValue, value); }

        }
        private List<ComparisonEntry> ComparisonsValue = new();

        public List<ComparisonEntry> Comparisons

        {

            get { return this.ComparisonsValue; }

            set { SetProperty(ref ComparisonsValue, value); }

        }
        private string ConclusionValue = string.Empty;

        public string Conclusion

        {

            get { return this.ConclusionValue; }

            set { SetProperty(ref ConclusionValue, value); }

        }
    }
}
