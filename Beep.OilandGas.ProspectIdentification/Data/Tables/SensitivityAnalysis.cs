using System;

namespace Beep.OilandGas.Models.Data
{
    public class SensitivityAnalysis : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime AnalysisDateValue = DateTime.UtcNow;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
