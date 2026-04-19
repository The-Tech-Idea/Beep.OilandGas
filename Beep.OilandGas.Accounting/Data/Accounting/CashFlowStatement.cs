using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class CashFlowStatement : ModelEntityBase
    {
        private System.DateTime PeriodStartValue;
        /// <summary>
        /// Gets or sets the period start.
        /// </summary>
        public System.DateTime PeriodStart
        {
            get { return this.PeriodStartValue; }
            set { SetProperty(ref PeriodStartValue, value); }
        }

        private System.DateTime PeriodEndValue;
        /// <summary>
        /// Gets or sets the period end.
        /// </summary>
        public System.DateTime PeriodEnd
        {
            get { return this.PeriodEndValue; }
            set { SetProperty(ref PeriodEndValue, value); }
        }

        private System.String PeriodNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the period name.
        /// </summary>
        public System.String PeriodName
        {
            get { return this.PeriodNameValue; }
            set { SetProperty(ref PeriodNameValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets the generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private System.Decimal OperatingValue;
        /// <summary>
        /// Gets or sets operating cash flow.
        /// </summary>
        public System.Decimal Operating
        {
            get { return this.OperatingValue; }
            set { SetProperty(ref OperatingValue, value); }
        }

        private List<CashFlowLine> OperatingActivitiesValue = new List<CashFlowLine>();
        /// <summary>
        /// Gets or sets operating activities.
        /// </summary>
        public List<CashFlowLine> OperatingActivities
        {
            get { return this.OperatingActivitiesValue; }
            set { SetProperty(ref OperatingActivitiesValue, value); }
        }

        private System.Decimal NetCashFromOperatingValue;
        /// <summary>
        /// Gets or sets net cash from operating.
        /// </summary>
        public System.Decimal NetCashFromOperating
        {
            get { return this.NetCashFromOperatingValue; }
            set { SetProperty(ref NetCashFromOperatingValue, value); }
        }

        private System.Decimal InvestingValue;
        /// <summary>
        /// Gets or sets investing cash flow.
        /// </summary>
        public System.Decimal Investing
        {
            get { return this.InvestingValue; }
            set { SetProperty(ref InvestingValue, value); }
        }

        private List<CashFlowLine> InvestingActivitiesValue = new List<CashFlowLine>();
        /// <summary>
        /// Gets or sets investing activities.
        /// </summary>
        public List<CashFlowLine> InvestingActivities
        {
            get { return this.InvestingActivitiesValue; }
            set { SetProperty(ref InvestingActivitiesValue, value); }
        }

        private System.Decimal NetCashFromInvestingValue;
        /// <summary>
        /// Gets or sets net cash from investing.
        /// </summary>
        public System.Decimal NetCashFromInvesting
        {
            get { return this.NetCashFromInvestingValue; }
            set { SetProperty(ref NetCashFromInvestingValue, value); }
        }

        private System.Decimal FinancingValue;
        /// <summary>
        /// Gets or sets financing cash flow.
        /// </summary>
        public System.Decimal Financing
        {
            get { return this.FinancingValue; }
            set { SetProperty(ref FinancingValue, value); }
        }

        private List<CashFlowLine> FinancingActivitiesValue = new List<CashFlowLine>();
        /// <summary>
        /// Gets or sets financing activities.
        /// </summary>
        public List<CashFlowLine> FinancingActivities
        {
            get { return this.FinancingActivitiesValue; }
            set { SetProperty(ref FinancingActivitiesValue, value); }
        }

        private System.Decimal NetCashFromFinancingValue;
        /// <summary>
        /// Gets or sets net cash from financing.
        /// </summary>
        public System.Decimal NetCashFromFinancing
        {
            get { return this.NetCashFromFinancingValue; }
            set { SetProperty(ref NetCashFromFinancingValue, value); }
        }

        private System.Decimal NetChangeValue;
        /// <summary>
        /// Gets or sets net change.
        /// </summary>
        public System.Decimal NetChange
        {
            get { return this.NetChangeValue; }
            set { SetProperty(ref NetChangeValue, value); }
        }

        private System.Decimal NetChangeInCashValue;
        /// <summary>
        /// Gets or sets net change in cash.
        /// </summary>
        public System.Decimal NetChangeInCash
        {
            get { return this.NetChangeInCashValue; }
            set { SetProperty(ref NetChangeInCashValue, value); }
        }

        private System.Decimal BeginningCashValue;
        /// <summary>
        /// Gets or sets beginning cash.
        /// </summary>
        public System.Decimal BeginningCash
        {
            get { return this.BeginningCashValue; }
            set { SetProperty(ref BeginningCashValue, value); }
        }

        private System.Decimal EndingCashValue;
        /// <summary>
        /// Gets or sets ending cash.
        /// </summary>
        public System.Decimal EndingCash
        {
            get { return this.EndingCashValue; }
            set { SetProperty(ref EndingCashValue, value); }
        }

        private System.Decimal CashReconciliationValue;
        /// <summary>
        /// Gets or sets cash reconciliation.
        /// </summary>
        public System.Decimal CashReconciliation
        {
            get { return this.CashReconciliationValue; }
            set { SetProperty(ref CashReconciliationValue, value); }
        }

        private List<CashFlowLine> LinesValue = new List<CashFlowLine>();
        /// <summary>
        /// Gets or sets cash flow lines.
        /// </summary>
        public List<CashFlowLine> Lines
        {
            get { return this.LinesValue; }
            set { SetProperty(ref LinesValue, value); }
        }
    }
}
