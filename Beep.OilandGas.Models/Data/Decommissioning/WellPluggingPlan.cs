using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellPluggingPlan : ModelEntityBase
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

        private int ZonesIdentifiedValue;
        public int ZonesIdentified
        {
            get { return this.ZonesIdentifiedValue; }
            set { SetProperty(ref ZonesIdentifiedValue, value); }
        }

        private double FreshwaterAquiferDepthValue;
        public double FreshwaterAquiferDepth
        {
            get { return this.FreshwaterAquiferDepthValue; }
            set { SetProperty(ref FreshwaterAquiferDepthValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<string> CriticalZonesValue = new();
        public List<string> CriticalZones
        {
            get { return this.CriticalZonesValue; }
            set { SetProperty(ref CriticalZonesValue, value); }
        }

        private string PluggingStrategyValue = string.Empty;
        public string PluggingStrategy
        {
            get { return this.PluggingStrategyValue; }
            set { SetProperty(ref PluggingStrategyValue, value); }
        }

        private double CementRequirementsValue;
        public double CementRequirements
        {
            get { return this.CementRequirementsValue; }
            set { SetProperty(ref CementRequirementsValue, value); }
        }

        private List<string> PlugSpecificationsValue = new();
        public List<string> PlugSpecifications
        {
            get { return this.PlugSpecificationsValue; }
            set { SetProperty(ref PlugSpecificationsValue, value); }
        }

        private int EstimatedDaysRequiredValue;
        public int EstimatedDaysRequired
        {
            get { return this.EstimatedDaysRequiredValue; }
            set { SetProperty(ref EstimatedDaysRequiredValue, value); }
        }

        private List<string> PotentialIssuesValue = new();
        public List<string> PotentialIssues
        {
            get { return this.PotentialIssuesValue; }
            set { SetProperty(ref PotentialIssuesValue, value); }
        }
    }
}
