using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class DeferralOpportunity : ModelEntityBase
    {
        private System.String OpportunityTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets opportunity type.
        /// </summary>
        public System.String OpportunityType
        {
            get { return this.OpportunityTypeValue; }
            set { SetProperty(ref OpportunityTypeValue, value); }
        }

        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private System.Decimal DeferralAmountValue;
        /// <summary>
        /// Gets or sets deferral amount.
        /// </summary>
        public System.Decimal DeferralAmount
        {
            get { return this.DeferralAmountValue; }
            set { SetProperty(ref DeferralAmountValue, value); }
        }

        private System.Decimal TaxSavingsValue;
        /// <summary>
        /// Gets or sets tax savings.
        /// </summary>
        public System.Decimal TaxSavings
        {
            get { return this.TaxSavingsValue; }
            set { SetProperty(ref TaxSavingsValue, value); }
        }

        private System.Decimal CashFlowImpactValue;
        /// <summary>
        /// Gets or sets cash flow impact.
        /// </summary>
        public System.Decimal CashFlowImpact
        {
            get { return this.CashFlowImpactValue; }
            set { SetProperty(ref CashFlowImpactValue, value); }
        }

        private System.DateTime DeadlineValue;
        /// <summary>
        /// Gets or sets deadline.
        /// </summary>
        public System.DateTime Deadline
        {
            get { return this.DeadlineValue; }
            set { SetProperty(ref DeadlineValue, value); }
        }
    }
}
