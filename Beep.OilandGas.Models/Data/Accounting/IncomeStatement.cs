using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class IncomeStatement : ModelEntityBase
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
}
