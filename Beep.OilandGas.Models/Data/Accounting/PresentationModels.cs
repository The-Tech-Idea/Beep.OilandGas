using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Presentation package for financial statements.
    /// </summary>
    public partial class PresentationPackage : ModelEntityBase
    {
        private System.String EntityNameValue = string.Empty;
        /// <summary>
        /// Gets or sets entity name.
        /// </summary>
        public System.String EntityName
        {
            get { return this.EntityNameValue; }
            set { SetProperty(ref EntityNameValue, value); }
        }

        private System.String ReportingCurrencyValue = string.Empty;
        /// <summary>
        /// Gets or sets reporting currency.
        /// </summary>
        public System.String ReportingCurrency
        {
            get { return this.ReportingCurrencyValue; }
            set { SetProperty(ref ReportingCurrencyValue, value); }
        }

        private System.DateTime PeriodStartValue;
        /// <summary>
        /// Gets or sets period start.
        /// </summary>
        public System.DateTime PeriodStart
        {
            get { return this.PeriodStartValue; }
            set { SetProperty(ref PeriodStartValue, value); }
        }

        private System.DateTime PeriodEndValue;
        /// <summary>
        /// Gets or sets period end.
        /// </summary>
        public System.DateTime PeriodEnd
        {
            get { return this.PeriodEndValue; }
            set { SetProperty(ref PeriodEndValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private IncomeStatement IncomeStatementValue = new IncomeStatement();
        /// <summary>
        /// Gets or sets income statement.
        /// </summary>
        public IncomeStatement IncomeStatement
        {
            get { return this.IncomeStatementValue; }
            set { SetProperty(ref IncomeStatementValue, value); }
        }

        private BalanceSheet BalanceSheetValue = new BalanceSheet();
        /// <summary>
        /// Gets or sets balance sheet.
        /// </summary>
        public BalanceSheet BalanceSheet
        {
            get { return this.BalanceSheetValue; }
            set { SetProperty(ref BalanceSheetValue, value); }
        }

        private CashFlowStatement CashFlowStatementValue = new CashFlowStatement();
        /// <summary>
        /// Gets or sets cash flow statement.
        /// </summary>
        public CashFlowStatement CashFlowStatement
        {
            get { return this.CashFlowStatementValue; }
            set { SetProperty(ref CashFlowStatementValue, value); }
        }

        private List<System.String> RequiredDisclosuresValue = new List<System.String>();
        /// <summary>
        /// Gets or sets required disclosures.
        /// </summary>
        public List<System.String> RequiredDisclosures
        {
            get { return this.RequiredDisclosuresValue; }
            set { SetProperty(ref RequiredDisclosuresValue, value); }
        }
    }
}


