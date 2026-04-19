using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class TaxDeferralAnalysis : ModelEntityBase
    {
        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private System.Decimal CurrentYearIncomeValue;
        /// <summary>
        /// Gets or sets current year income.
        /// </summary>
        public System.Decimal CurrentYearIncome
        {
            get { return this.CurrentYearIncomeValue; }
            set { SetProperty(ref CurrentYearIncomeValue, value); }
        }

        private List<DeferralOpportunity> OpportunitiesValue = new List<DeferralOpportunity>();
        /// <summary>
        /// Gets or sets opportunities.
        /// </summary>
        public List<DeferralOpportunity> Opportunities
        {
            get { return this.OpportunitiesValue; }
            set { SetProperty(ref OpportunitiesValue, value); }
        }

        private System.Decimal TotalDeferralAmountValue;
        /// <summary>
        /// Gets or sets total deferral amount.
        /// </summary>
        public System.Decimal TotalDeferralAmount
        {
            get { return this.TotalDeferralAmountValue; }
            set { SetProperty(ref TotalDeferralAmountValue, value); }
        }

        private System.Decimal TotalTaxSavingsValue;
        /// <summary>
        /// Gets or sets total tax savings.
        /// </summary>
        public System.Decimal TotalTaxSavings
        {
            get { return this.TotalTaxSavingsValue; }
            set { SetProperty(ref TotalTaxSavingsValue, value); }
        }

        private System.Decimal TotalCashFlowImpactValue;
        /// <summary>
        /// Gets or sets total cash flow impact.
        /// </summary>
        public System.Decimal TotalCashFlowImpact
        {
            get { return this.TotalCashFlowImpactValue; }
            set { SetProperty(ref TotalCashFlowImpactValue, value); }
        }
    }
}
