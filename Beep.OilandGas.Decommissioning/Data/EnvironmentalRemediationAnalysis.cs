using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EnvironmentalRemediationAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private string LocationValue = string.Empty;
        public string Location
        {
            get { return this.LocationValue; }
            set { SetProperty(ref LocationValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<string> PotentialContaminantsValue = new();
        public List<string> PotentialContaminants
        {
            get { return this.PotentialContaminantsValue; }
            set { SetProperty(ref PotentialContaminantsValue, value); }
        }

        private string EnvironmentalRiskLevelValue = string.Empty;
        public string EnvironmentalRiskLevel
        {
            get { return this.EnvironmentalRiskLevelValue; }
            set { SetProperty(ref EnvironmentalRiskLevelValue, value); }
        }

        private List<string> RemediationActivitiesValue = new();
        public List<string> RemediationActivities
        {
            get { return this.RemediationActivitiesValue; }
            set { SetProperty(ref RemediationActivitiesValue, value); }
        }

        private int EstimatedRemediationMonthsValue;
        public int EstimatedRemediationMonths
        {
            get { return this.EstimatedRemediationMonthsValue; }
            set { SetProperty(ref EstimatedRemediationMonthsValue, value); }
        }

        private int MonitoringPeriodYearsValue;
        public int MonitoringPeriodYears
        {
            get { return this.MonitoringPeriodYearsValue; }
            set { SetProperty(ref MonitoringPeriodYearsValue, value); }
        }

        private double LongTermLiabilityCostValue;
        public double LongTermLiabilityCost
        {
            get { return this.LongTermLiabilityCostValue; }
            set { SetProperty(ref LongTermLiabilityCostValue, value); }
        }

        private List<string> RegulatoryRequirementsValue = new();
        public List<string> RegulatoryRequirements
        {
            get { return this.RegulatoryRequirementsValue; }
            set { SetProperty(ref RegulatoryRequirementsValue, value); }
        }
    }
}
