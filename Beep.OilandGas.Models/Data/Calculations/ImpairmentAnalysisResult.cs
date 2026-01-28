using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ImpairmentAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Property group identifier
        /// </summary>
        private string PropertyGroupIdValue = string.Empty;

        public string PropertyGroupId

        {

            get { return this.PropertyGroupIdValue; }

            set { SetProperty(ref PropertyGroupIdValue, value); }

        }

        /// <summary>
        /// Carrying amount (book value)
        /// </summary>
        private decimal CarryingAmountValue;

        public decimal CarryingAmount

        {

            get { return this.CarryingAmountValue; }

            set { SetProperty(ref CarryingAmountValue, value); }

        }

        /// <summary>
        /// Expected undiscounted future cash flows
        /// </summary>
        private decimal UndiscountedCashFlowsValue;

        public decimal UndiscountedCashFlows

        {

            get { return this.UndiscountedCashFlowsValue; }

            set { SetProperty(ref UndiscountedCashFlowsValue, value); }

        }

        /// <summary>
        /// Is impairment indicated?
        /// </summary>
        private bool IsImpairmentIndicatorValue;

        public bool IsImpairmentIndicator

        {

            get { return this.IsImpairmentIndicatorValue; }

            set { SetProperty(ref IsImpairmentIndicatorValue, value); }

        }

        /// <summary>
        /// Fair value of asset (if impaired)
        /// </summary>
        private decimal? FairValueValue;

        public decimal? FairValue

        {

            get { return this.FairValueValue; }

            set { SetProperty(ref FairValueValue, value); }

        }

        /// <summary>
        /// Impairment charge (if applicable)
        /// </summary>
        private decimal? ImpairmentChargeValue;

        public decimal? ImpairmentCharge

        {

            get { return this.ImpairmentChargeValue; }

            set { SetProperty(ref ImpairmentChargeValue, value); }

        }

        /// <summary>
        /// Analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
