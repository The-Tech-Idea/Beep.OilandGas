using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class BalanceSheet : ModelEntityBase
    {
        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.String ReportNameValue = string.Empty;
        /// <summary>
        /// Gets or sets report name.
        /// </summary>
        public System.String ReportName
        {
            get { return this.ReportNameValue; }
            set { SetProperty(ref ReportNameValue, value); }
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

        private List<BalanceSheetLine> CurrentAssetsValue = new List<BalanceSheetLine>();
        /// <summary>
        /// Gets or sets current assets.
        /// </summary>
        public List<BalanceSheetLine> CurrentAssets
        {
            get { return this.CurrentAssetsValue; }
            set { SetProperty(ref CurrentAssetsValue, value); }
        }

        private System.Decimal TotalCurrentAssetsValue;
        /// <summary>
        /// Gets or sets total current assets.
        /// </summary>
        public System.Decimal TotalCurrentAssets
        {
            get { return this.TotalCurrentAssetsValue; }
            set { SetProperty(ref TotalCurrentAssetsValue, value); }
        }

        private List<BalanceSheetLine> FixedAssetsValue = new List<BalanceSheetLine>();
        /// <summary>
        /// Gets or sets fixed assets.
        /// </summary>
        public List<BalanceSheetLine> FixedAssets
        {
            get { return this.FixedAssetsValue; }
            set { SetProperty(ref FixedAssetsValue, value); }
        }

        private System.Decimal TotalFixedAssetsValue;
        /// <summary>
        /// Gets or sets total fixed assets.
        /// </summary>
        public System.Decimal TotalFixedAssets
        {
            get { return this.TotalFixedAssetsValue; }
            set { SetProperty(ref TotalFixedAssetsValue, value); }
        }

        private List<BalanceSheetLine> OtherAssetsValue = new List<BalanceSheetLine>();
        /// <summary>
        /// Gets or sets other assets.
        /// </summary>
        public List<BalanceSheetLine> OtherAssets
        {
            get { return this.OtherAssetsValue; }
            set { SetProperty(ref OtherAssetsValue, value); }
        }

        private System.Decimal TotalOtherAssetsValue;
        /// <summary>
        /// Gets or sets total other assets.
        /// </summary>
        public System.Decimal TotalOtherAssets
        {
            get { return this.TotalOtherAssetsValue; }
            set { SetProperty(ref TotalOtherAssetsValue, value); }
        }

        private System.Decimal TotalAssetsValue;
        /// <summary>
        /// Gets or sets total assets.
        /// </summary>
        public System.Decimal TotalAssets
        {
            get { return this.TotalAssetsValue; }
            set { SetProperty(ref TotalAssetsValue, value); }
        }

        private List<BalanceSheetLine> CurrentLiabilitiesValue = new List<BalanceSheetLine>();
        /// <summary>
        /// Gets or sets current liabilities.
        /// </summary>
        public List<BalanceSheetLine> CurrentLiabilities
        {
            get { return this.CurrentLiabilitiesValue; }
            set { SetProperty(ref CurrentLiabilitiesValue, value); }
        }

        private System.Decimal TotalCurrentLiabilitiesValue;
        /// <summary>
        /// Gets or sets total current liabilities.
        /// </summary>
        public System.Decimal TotalCurrentLiabilities
        {
            get { return this.TotalCurrentLiabilitiesValue; }
            set { SetProperty(ref TotalCurrentLiabilitiesValue, value); }
        }

        private List<BalanceSheetLine> LongTermLiabilitiesValue = new List<BalanceSheetLine>();
        /// <summary>
        /// Gets or sets long term liabilities.
        /// </summary>
        public List<BalanceSheetLine> LongTermLiabilities
        {
            get { return this.LongTermLiabilitiesValue; }
            set { SetProperty(ref LongTermLiabilitiesValue, value); }
        }

        private System.Decimal TotalLongTermLiabilitiesValue;
        /// <summary>
        /// Gets or sets total long term liabilities.
        /// </summary>
        public System.Decimal TotalLongTermLiabilities
        {
            get { return this.TotalLongTermLiabilitiesValue; }
            set { SetProperty(ref TotalLongTermLiabilitiesValue, value); }
        }

        private System.Decimal TotalLiabilitiesValue;
        /// <summary>
        /// Gets or sets total liabilities.
        /// </summary>
        public System.Decimal TotalLiabilities
        {
            get { return this.TotalLiabilitiesValue; }
            set { SetProperty(ref TotalLiabilitiesValue, value); }
        }

        private List<BalanceSheetLine> EquityAccountsValue = new List<BalanceSheetLine>();
        /// <summary>
        /// Gets or sets equity accounts.
        /// </summary>
        public List<BalanceSheetLine> EquityAccounts
        {
            get { return this.EquityAccountsValue; }
            set { SetProperty(ref EquityAccountsValue, value); }
        }

        private System.Decimal TotalEquityValue;
        /// <summary>
        /// Gets or sets total equity.
        /// </summary>
        public System.Decimal TotalEquity
        {
            get { return this.TotalEquityValue; }
            set { SetProperty(ref TotalEquityValue, value); }
        }

        private System.Decimal TotalLiabilitiesAndEquityValue;
        /// <summary>
        /// Gets or sets total liabilities and equity.
        /// </summary>
        public System.Decimal TotalLiabilitiesAndEquity
        {
            get { return this.TotalLiabilitiesAndEquityValue; }
            set { SetProperty(ref TotalLiabilitiesAndEquityValue, value); }
        }

        private System.Decimal BalanceDifferenceValue;
        /// <summary>
        /// Gets or sets balance difference.
        /// </summary>
        public System.Decimal BalanceDifference
        {
            get { return this.BalanceDifferenceValue; }
            set { SetProperty(ref BalanceDifferenceValue, value); }
        }

        private System.Boolean IsBalancedValue;
        /// <summary>
        /// Gets or sets whether the balance sheet is balanced.
        /// </summary>
        public System.Boolean IsBalanced
        {
            get { return this.IsBalancedValue; }
            set { SetProperty(ref IsBalancedValue, value); }
        }
    }
}
