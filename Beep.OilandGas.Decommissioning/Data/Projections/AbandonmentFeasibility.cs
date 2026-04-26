using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AbandonmentFeasibility : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private string WellStatusValue = string.Empty;
        public string WellStatus
        {
            get { return this.WellStatusValue; }
            set { SetProperty(ref WellStatusValue, value); }
        }

        private DateTime LastProductionDateValue;
        public DateTime LastProductionDate
        {
            get { return this.LastProductionDateValue; }
            set { SetProperty(ref LastProductionDateValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private string WellConditionStatusValue = string.Empty;
        public string WellConditionStatus
        {
            get { return this.WellConditionStatusValue; }
            set { SetProperty(ref WellConditionStatusValue, value); }
        }

        private bool AbandonmentFeasibleValue;
        public bool AbandonmentFeasible
        {
            get { return this.AbandonmentFeasibleValue; }
            set { SetProperty(ref AbandonmentFeasibleValue, value); }
        }

        private List<string> AbandonmentChallengesValue = new();
        public List<string> AbandonmentChallenges
        {
            get { return this.AbandonmentChallengesValue; }
            set { SetProperty(ref AbandonmentChallengesValue, value); }
        }

        private string RecommendedApproachValue = string.Empty;
        public string RecommendedApproach
        {
            get { return this.RecommendedApproachValue; }
            set { SetProperty(ref RecommendedApproachValue, value); }
        }

        private bool CanAbandonWithin12MonthsValue;
        public bool CanAbandonWithin12Months
        {
            get { return this.CanAbandonWithin12MonthsValue; }
            set { SetProperty(ref CanAbandonWithin12MonthsValue, value); }
        }

        private double AbandonmentBenefitValue;
        public double AbandonmentBenefit
        {
            get { return this.AbandonmentBenefitValue; }
            set { SetProperty(ref AbandonmentBenefitValue, value); }
        }

        private double AbandonmentCostValue;
        public double AbandonmentCost
        {
            get { return this.AbandonmentCostValue; }
            set { SetProperty(ref AbandonmentCostValue, value); }
        }

        private double NetBenefitValue;
        public double NetBenefit
        {
            get { return this.NetBenefitValue; }
            set { SetProperty(ref NetBenefitValue, value); }
        }

        private string AbandonmentRiskLevelValue = string.Empty;
        public string AbandonmentRiskLevel
        {
            get { return this.AbandonmentRiskLevelValue; }
            set { SetProperty(ref AbandonmentRiskLevelValue, value); }
        }
    }
}
