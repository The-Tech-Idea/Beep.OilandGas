using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class RoyaltyStatement : ModelEntityBase
    {
        private string StatementIdValue = string.Empty;

        public string StatementId

        {

            get { return this.StatementIdValue; }

            set { SetProperty(ref StatementIdValue, value); }

        }
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private DateTime StatementPeriodStartValue;

        public DateTime StatementPeriodStart

        {

            get { return this.StatementPeriodStartValue; }

            set { SetProperty(ref StatementPeriodStartValue, value); }

        }
        private DateTime StatementPeriodEndValue;

        public DateTime StatementPeriodEnd

        {

            get { return this.StatementPeriodEndValue; }

            set { SetProperty(ref StatementPeriodEndValue, value); }

        }
        private ProductionSummary? ProductionSummaryValue;

        public ProductionSummary? ProductionSummary

        {

            get { return this.ProductionSummaryValue; }

            set { SetProperty(ref ProductionSummaryValue, value); }

        }
        private RevenueSummary? RevenueSummaryValue;

        public RevenueSummary? RevenueSummary

        {

            get { return this.RevenueSummaryValue; }

            set { SetProperty(ref RevenueSummaryValue, value); }

        }
        private DeductionsSummary? DeductionsSummaryValue;

        public DeductionsSummary? DeductionsSummary

        {

            get { return this.DeductionsSummaryValue; }

            set { SetProperty(ref DeductionsSummaryValue, value); }

        }
        private decimal TotalRoyaltyAmountValue;

        public decimal TotalRoyaltyAmount

        {

            get { return this.TotalRoyaltyAmountValue; }

            set { SetProperty(ref TotalRoyaltyAmountValue, value); }

        }
    }
}
