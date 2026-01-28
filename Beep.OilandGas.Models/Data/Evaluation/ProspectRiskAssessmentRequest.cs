using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectRiskAssessmentRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private List<string> RiskCategoriesValue = new();

        public List<string> RiskCategories

        {

            get { return this.RiskCategoriesValue; }

            set { SetProperty(ref RiskCategoriesValue, value); }

        }
        private string AssessmentMethodologyValue = "Subjective";

        public string AssessmentMethodology

        {

            get { return this.AssessmentMethodologyValue; }

            set { SetProperty(ref AssessmentMethodologyValue, value); }

        }
        private bool IncludeMitigationAnalysisValue = true;

        public bool IncludeMitigationAnalysis

        {

            get { return this.IncludeMitigationAnalysisValue; }

            set { SetProperty(ref IncludeMitigationAnalysisValue, value); }

        }
    }
}
