using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class BankReconciliation : ModelEntityBase
    {
        private System.String ReportNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the report name.
        /// </summary>
        public System.String ReportName
        {
            get { return this.ReportNameValue; }
            set { SetProperty(ref ReportNameValue, value); }
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

        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.DateTime StatementDateValue;
        /// <summary>
        /// Gets or sets the statement date.
        /// </summary>
        public System.DateTime StatementDate
        {
            get { return this.StatementDateValue; }
            set { SetProperty(ref StatementDateValue, value); }
        }

        private System.Decimal BankStatementBalanceValue;
        /// <summary>
        /// Gets or sets the bank statement balance.
        /// </summary>
        public System.Decimal BankStatementBalance
        {
            get { return this.BankStatementBalanceValue; }
            set { SetProperty(ref BankStatementBalanceValue, value); }
        }

        private System.Decimal GLBalanceValue;
        /// <summary>
        /// Gets or sets the GL balance.
        /// </summary>
        public System.Decimal GLBalance
        {
            get { return this.GLBalanceValue; }
            set { SetProperty(ref GLBalanceValue, value); }
        }

        private System.Decimal TotalOutstandingChecksValue;
        /// <summary>
        /// Gets or sets total outstanding checks.
        /// </summary>
        public System.Decimal TotalOutstandingChecks
        {
            get { return this.TotalOutstandingChecksValue; }
            set { SetProperty(ref TotalOutstandingChecksValue, value); }
        }

        private System.Decimal TotalDepositsInTransitValue;
        /// <summary>
        /// Gets or sets total deposits in transit.
        /// </summary>
        public System.Decimal TotalDepositsInTransit
        {
            get { return this.TotalDepositsInTransitValue; }
            set { SetProperty(ref TotalDepositsInTransitValue, value); }
        }

        private System.Decimal ReconciledGLBalanceValue;
        /// <summary>
        /// Gets or sets the reconciled GL balance.
        /// </summary>
        public System.Decimal ReconciledGLBalance
        {
            get { return this.ReconciledGLBalanceValue; }
            set { SetProperty(ref ReconciledGLBalanceValue, value); }
        }

        private System.Decimal DifferenceValue;
        /// <summary>
        /// Gets or sets the difference between GL and reconciled balance.
        /// </summary>
        public System.Decimal Difference
        {
            get { return this.DifferenceValue; }
            set { SetProperty(ref DifferenceValue, value); }
        }

        private System.Boolean IsReconciledValue;
        /// <summary>
        /// Gets or sets whether reconciliation is balanced.
        /// </summary>
        public System.Boolean IsReconciled
        {
            get { return this.IsReconciledValue; }
            set { SetProperty(ref IsReconciledValue, value); }
        }

        private List<OutstandingCheck> OutstandingChecksValue = new List<OutstandingCheck>();
        /// <summary>
        /// Gets or sets outstanding checks.
        /// </summary>
        public List<OutstandingCheck> OutstandingChecks
        {
            get { return this.OutstandingChecksValue; }
            set { SetProperty(ref OutstandingChecksValue, value); }
        }

        private List<DepositInTransit> DepositsInTransitValue = new List<DepositInTransit>();
        /// <summary>
        /// Gets or sets deposits in transit.
        /// </summary>
        public List<DepositInTransit> DepositsInTransit
        {
            get { return this.DepositsInTransitValue; }
            set { SetProperty(ref DepositsInTransitValue, value); }
        }
    }
}
