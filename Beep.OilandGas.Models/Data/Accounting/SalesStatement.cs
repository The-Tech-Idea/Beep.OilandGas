using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class SalesStatement : ModelEntityBase
    {
        private System.String StatementIdValue;
        public System.String StatementId
        {
            get { return this.StatementIdValue; }
            set { SetProperty(ref StatementIdValue, value); }
        }

        private System.DateTime StatementPeriodStartValue;
        public System.DateTime StatementPeriodStart
        {
            get { return this.StatementPeriodStartValue; }
            set { SetProperty(ref StatementPeriodStartValue, value); }
        }

        private System.DateTime StatementPeriodEndValue;
        public System.DateTime StatementPeriodEnd
        {
            get { return this.StatementPeriodEndValue; }
            set { SetProperty(ref StatementPeriodEndValue, value); }
        }

        private System.String? PropertyOrLeaseIdValue;
        public System.String? PropertyOrLeaseId
        {
            get { return this.PropertyOrLeaseIdValue; }
            set { SetProperty(ref PropertyOrLeaseIdValue, value); }
        }

        private SalesSummary SummaryValue = new SalesSummary();
        public SalesSummary Summary
        {
            get { return this.SummaryValue; }
            set { SetProperty(ref SummaryValue, value); }
        }

        private List<VolumeDetail> VolumeDetailsValue = new List<VolumeDetail>();
        public List<VolumeDetail> VolumeDetails
        {
            get { return this.VolumeDetailsValue; }
            set { SetProperty(ref VolumeDetailsValue, value); }
        }

        private List<PricingDetail> PricingDetailsValue = new List<PricingDetail>();
        public List<PricingDetail> PricingDetails
        {
            get { return this.PricingDetailsValue; }
            set { SetProperty(ref PricingDetailsValue, value); }
        }

        private List<SalesTransaction> TransactionsValue = new List<SalesTransaction>();
        public List<SalesTransaction> Transactions
        {
            get { return this.TransactionsValue; }
            set { SetProperty(ref TransactionsValue, value); }
        }
    }
}
