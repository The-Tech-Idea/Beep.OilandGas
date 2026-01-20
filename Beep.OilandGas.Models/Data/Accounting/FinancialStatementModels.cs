using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Income Statement (P&L) Data Model
    /// </summary>
    public partial class IncomeStatement : AccountingEntityBase
    {
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

        private System.String PeriodNameValue = string.Empty;
        /// <summary>
        /// Gets or sets period name.
        /// </summary>
        public System.String PeriodName
        {
            get { return this.PeriodNameValue; }
            set { SetProperty(ref PeriodNameValue, value); }
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

        private List<IncomeStatementLine> RevenuesValue = new List<IncomeStatementLine>();
        /// <summary>
        /// Gets or sets revenues.
        /// </summary>
        public List<IncomeStatementLine> Revenues
        {
            get { return this.RevenuesValue; }
            set { SetProperty(ref RevenuesValue, value); }
        }

        private System.Decimal TotalRevenuesValue;
        /// <summary>
        /// Gets or sets total revenues.
        /// </summary>
        public System.Decimal TotalRevenues
        {
            get { return this.TotalRevenuesValue; }
            set { SetProperty(ref TotalRevenuesValue, value); }
        }

        private List<IncomeStatementLine> CostOfGoodsValue = new List<IncomeStatementLine>();
        /// <summary>
        /// Gets or sets cost of goods.
        /// </summary>
        public List<IncomeStatementLine> CostOfGoods
        {
            get { return this.CostOfGoodsValue; }
            set { SetProperty(ref CostOfGoodsValue, value); }
        }

        private System.Decimal TotalCostOfGoodsValue;
        /// <summary>
        /// Gets or sets total cost of goods.
        /// </summary>
        public System.Decimal TotalCostOfGoods
        {
            get { return this.TotalCostOfGoodsValue; }
            set { SetProperty(ref TotalCostOfGoodsValue, value); }
        }

        private System.Decimal GrossProfitValue;
        /// <summary>
        /// Gets or sets gross profit.
        /// </summary>
        public System.Decimal GrossProfit
        {
            get { return this.GrossProfitValue; }
            set { SetProperty(ref GrossProfitValue, value); }
        }

        private List<IncomeStatementLine> OperatingExpensesValue = new List<IncomeStatementLine>();
        /// <summary>
        /// Gets or sets operating expenses.
        /// </summary>
        public List<IncomeStatementLine> OperatingExpenses
        {
            get { return this.OperatingExpensesValue; }
            set { SetProperty(ref OperatingExpensesValue, value); }
        }

        private System.Decimal TotalOperatingExpensesValue;
        /// <summary>
        /// Gets or sets total operating expenses.
        /// </summary>
        public System.Decimal TotalOperatingExpenses
        {
            get { return this.TotalOperatingExpensesValue; }
            set { SetProperty(ref TotalOperatingExpensesValue, value); }
        }

        private System.Decimal OperatingIncomeValue;
        /// <summary>
        /// Gets or sets operating income.
        /// </summary>
        public System.Decimal OperatingIncome
        {
            get { return this.OperatingIncomeValue; }
            set { SetProperty(ref OperatingIncomeValue, value); }
        }

        private List<IncomeStatementLine> OtherIncomeExpenseValue = new List<IncomeStatementLine>();
        /// <summary>
        /// Gets or sets other income/expense.
        /// </summary>
        public List<IncomeStatementLine> OtherIncomeExpense
        {
            get { return this.OtherIncomeExpenseValue; }
            set { SetProperty(ref OtherIncomeExpenseValue, value); }
        }

        private System.Decimal TotalOtherIncomeExpenseValue;
        /// <summary>
        /// Gets or sets total other income/expense.
        /// </summary>
        public System.Decimal TotalOtherIncomeExpense
        {
            get { return this.TotalOtherIncomeExpenseValue; }
            set { SetProperty(ref TotalOtherIncomeExpenseValue, value); }
        }

        private System.Decimal NetIncomeValue;
        /// <summary>
        /// Gets or sets net income.
        /// </summary>
        public System.Decimal NetIncome
        {
            get { return this.NetIncomeValue; }
            set { SetProperty(ref NetIncomeValue, value); }
        }
    }

    /// <summary>
    /// Income Statement Line Item
    /// </summary>
    public partial class IncomeStatementLine : AccountingEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        /// <summary>
        /// Gets or sets account name.
        /// </summary>
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }
    }

    /// <summary>
    /// Balance Sheet Data Model
    /// </summary>
    public partial class BalanceSheet : AccountingEntityBase
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

    /// <summary>
    /// Balance Sheet Line Item
    /// </summary>
    public partial class BalanceSheetLine : AccountingEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        /// <summary>
        /// Gets or sets account name.
        /// </summary>
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }
    }
}

