using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DecommissioningCostAnalysis : ModelEntityBase
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

        private string WellTypeValue = string.Empty;
        public string WellType
        {
            get { return this.WellTypeValue; }
            set { SetProperty(ref WellTypeValue, value); }
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

        private double WellPluggingCostValue;
        public double WellPluggingCost
        {
            get { return this.WellPluggingCostValue; }
            set { SetProperty(ref WellPluggingCostValue, value); }
        }

        private double WellheadRemovalCostValue;
        public double WellheadRemovalCost
        {
            get { return this.WellheadRemovalCostValue; }
            set { SetProperty(ref WellheadRemovalCostValue, value); }
        }

        private double SiteRestorationCostValue;
        public double SiteRestorationCost
        {
            get { return this.SiteRestorationCostValue; }
            set { SetProperty(ref SiteRestorationCostValue, value); }
        }

        private double EnvironmentalRemediationCostValue;
        public double EnvironmentalRemediationCost
        {
            get { return this.EnvironmentalRemediationCostValue; }
            set { SetProperty(ref EnvironmentalRemediationCostValue, value); }
        }

        private double AbandonmentBondCostValue;
        public double AbandonmentBondCost
        {
            get { return this.AbandonmentBondCostValue; }
            set { SetProperty(ref AbandonmentBondCostValue, value); }
        }

        private double TotalEstimatedCostValue;
        public double TotalEstimatedCost
        {
            get { return this.TotalEstimatedCostValue; }
            set { SetProperty(ref TotalEstimatedCostValue, value); }
        }

        private double PluggingCostPercentageValue;
        public double PluggingCostPercentage
        {
            get { return this.PluggingCostPercentageValue; }
            set { SetProperty(ref PluggingCostPercentageValue, value); }
        }

        private double WellheadRemovalPercentageValue;
        public double WellheadRemovalPercentage
        {
            get { return this.WellheadRemovalPercentageValue; }
            set { SetProperty(ref WellheadRemovalPercentageValue, value); }
        }

        private double SiteRestorationPercentageValue;
        public double SiteRestorationPercentage
        {
            get { return this.SiteRestorationPercentageValue; }
            set { SetProperty(ref SiteRestorationPercentageValue, value); }
        }

        private double ContingencyAmountValue;
        public double ContingencyAmount
        {
            get { return this.ContingencyAmountValue; }
            set { SetProperty(ref ContingencyAmountValue, value); }
        }

        private double TotalWithContingencyValue;
        public double TotalWithContingency
        {
            get { return this.TotalWithContingencyValue; }
            set { SetProperty(ref TotalWithContingencyValue, value); }
        }
    }
}
