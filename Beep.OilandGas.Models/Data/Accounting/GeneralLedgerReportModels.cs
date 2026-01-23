using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// GL Detail Report - All transactions for an account
    /// </summary>
    public partial class GLDetailReport : ModelEntityBase
    {
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

        private System.String AccountTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets account type.
        /// </summary>
        public System.String AccountType
        {
            get { return this.AccountTypeValue; }
            set { SetProperty(ref AccountTypeValue, value); }
        }

        private System.String NormalBalanceValue = string.Empty;
        /// <summary>
        /// Gets or sets normal balance.
        /// </summary>
        public System.String NormalBalance
        {
            get { return this.NormalBalanceValue; }
            set { SetProperty(ref NormalBalanceValue, value); }
        }

        private System.Nullable<System.DateTime> StartDateValue;
        /// <summary>
        /// Gets or sets start date.
        /// </summary>
        public System.Nullable<System.DateTime> StartDate
        {
            get { return this.StartDateValue; }
            set { SetProperty(ref StartDateValue, value); }
        }

        private System.Nullable<System.DateTime> EndDateValue;
        /// <summary>
        /// Gets or sets end date.
        /// </summary>
        public System.Nullable<System.DateTime> EndDate
        {
            get { return this.EndDateValue; }
            set { SetProperty(ref EndDateValue, value); }
        }

        private List<GLEntryLine> GLEntriesValue = new List<GLEntryLine>();
        /// <summary>
        /// Gets or sets GL entries.
        /// </summary>
        public List<GLEntryLine> GLEntries
        {
            get { return this.GLEntriesValue; }
            set { SetProperty(ref GLEntriesValue, value); }
        }

        private System.Decimal TotalDebitsValue;
        /// <summary>
        /// Gets or sets total debits.
        /// </summary>
        public System.Decimal TotalDebits
        {
            get { return this.TotalDebitsValue; }
            set { SetProperty(ref TotalDebitsValue, value); }
        }

        private System.Decimal TotalCreditsValue;
        /// <summary>
        /// Gets or sets total credits.
        /// </summary>
        public System.Decimal TotalCredits
        {
            get { return this.TotalCreditsValue; }
            set { SetProperty(ref TotalCreditsValue, value); }
        }

        private System.Decimal EndingBalanceValue;
        /// <summary>
        /// Gets or sets ending balance.
        /// </summary>
        public System.Decimal EndingBalance
        {
            get { return this.EndingBalanceValue; }
            set { SetProperty(ref EndingBalanceValue, value); }
        }
    }

    /// <summary>
    /// GL Entry Line - Single transaction
    /// </summary>
    public partial class GLEntryLine : ModelEntityBase
    {
        private System.DateTime EntryDateValue;
        /// <summary>
        /// Gets or sets entry date.
        /// </summary>
        public System.DateTime EntryDate
        {
            get { return this.EntryDateValue; }
            set { SetProperty(ref EntryDateValue, value); }
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

        private System.String ReferenceValue = string.Empty;
        /// <summary>
        /// Gets or sets reference.
        /// </summary>
        public System.String Reference
        {
            get { return this.ReferenceValue; }
            set { SetProperty(ref ReferenceValue, value); }
        }

        private System.String EntryTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets entry type.
        /// </summary>
        public System.String EntryType
        {
            get { return this.EntryTypeValue; }
            set { SetProperty(ref EntryTypeValue, value); }
        }

        private System.Decimal DebitAmountValue;
        /// <summary>
        /// Gets or sets debit amount.
        /// </summary>
        public System.Decimal DebitAmount
        {
            get { return this.DebitAmountValue; }
            set { SetProperty(ref DebitAmountValue, value); }
        }

        private System.Decimal CreditAmountValue;
        /// <summary>
        /// Gets or sets credit amount.
        /// </summary>
        public System.Decimal CreditAmount
        {
            get { return this.CreditAmountValue; }
            set { SetProperty(ref CreditAmountValue, value); }
        }

        private System.Decimal RunningBalanceValue;
        /// <summary>
        /// Gets or sets running balance.
        /// </summary>
        public System.Decimal RunningBalance
        {
            get { return this.RunningBalanceValue; }
            set { SetProperty(ref RunningBalanceValue, value); }
        }
    }

    /// <summary>
    /// GL Summary Report - Account balances
    /// </summary>
    public partial class GLSummaryReport : ModelEntityBase
    {
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

        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private List<GLSummaryLine> AccountsValue = new List<GLSummaryLine>();
        /// <summary>
        /// Gets or sets accounts.
        /// </summary>
        public List<GLSummaryLine> Accounts
        {
            get { return this.AccountsValue; }
            set { SetProperty(ref AccountsValue, value); }
        }

        private System.Decimal TotalDebitsValue;
        /// <summary>
        /// Gets or sets total debits.
        /// </summary>
        public System.Decimal TotalDebits
        {
            get { return this.TotalDebitsValue; }
            set { SetProperty(ref TotalDebitsValue, value); }
        }

        private System.Decimal TotalCreditsValue;
        /// <summary>
        /// Gets or sets total credits.
        /// </summary>
        public System.Decimal TotalCredits
        {
            get { return this.TotalCreditsValue; }
            set { SetProperty(ref TotalCreditsValue, value); }
        }

        private System.Boolean IsBalancedValue;
        /// <summary>
        /// Gets or sets balanced flag.
        /// </summary>
        public System.Boolean IsBalanced
        {
            get { return this.IsBalancedValue; }
            set { SetProperty(ref IsBalancedValue, value); }
        }
    }

    /// <summary>
    /// GL Summary Line - Account balance
    /// </summary>
    public partial class GLSummaryLine : ModelEntityBase
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

        private System.String AccountTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets account type.
        /// </summary>
        public System.String AccountType
        {
            get { return this.AccountTypeValue; }
            set { SetProperty(ref AccountTypeValue, value); }
        }

        private System.String NormalBalanceValue = string.Empty;
        /// <summary>
        /// Gets or sets normal balance.
        /// </summary>
        public System.String NormalBalance
        {
            get { return this.NormalBalanceValue; }
            set { SetProperty(ref NormalBalanceValue, value); }
        }

        private System.Decimal BalanceValue;
        /// <summary>
        /// Gets or sets balance.
        /// </summary>
        public System.Decimal Balance
        {
            get { return this.BalanceValue; }
            set { SetProperty(ref BalanceValue, value); }
        }
    }
}


