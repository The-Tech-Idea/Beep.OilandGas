using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Bank Reconciliation Result
    /// </summary>
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

    /// <summary>
    /// Outstanding Check
    /// </summary>
    public partial class OutstandingCheck : ModelEntityBase
    {
        private System.String CheckNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the check number.
        /// </summary>
        public System.String CheckNumber
        {
            get { return this.CheckNumberValue; }
            set { SetProperty(ref CheckNumberValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.DateTime CheckDateValue;
        /// <summary>
        /// Gets or sets the check date.
        /// </summary>
        public System.DateTime CheckDate
        {
            get { return this.CheckDateValue; }
            set { SetProperty(ref CheckDateValue, value); }
        }
    }

    /// <summary>
    /// Deposit In Transit
    /// </summary>
    public partial class DepositInTransit : ModelEntityBase
    {
        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.DateTime DepositDateValue;
        /// <summary>
        /// Gets or sets the deposit date.
        /// </summary>
        public System.DateTime DepositDate
        {
            get { return this.DepositDateValue; }
            set { SetProperty(ref DepositDateValue, value); }
        }
    }

    /// <summary>
    /// Check Clearing Analysis
    /// </summary>
    public partial class CheckClearingAnalysis : ModelEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

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

        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets the analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private System.Int32 TotalPaymentsValue;
        /// <summary>
        /// Gets or sets total payment count.
        /// </summary>
        public System.Int32 TotalPayments
        {
            get { return this.TotalPaymentsValue; }
            set { SetProperty(ref TotalPaymentsValue, value); }
        }

        private System.Decimal TotalAmountValue;
        /// <summary>
        /// Gets or sets total amount.
        /// </summary>
        public System.Decimal TotalAmount
        {
            get { return this.TotalAmountValue; }
            set { SetProperty(ref TotalAmountValue, value); }
        }

        private System.Int32 AverageProcessingTimeValue;
        /// <summary>
        /// Gets or sets average processing time.
        /// </summary>
        public System.Int32 AverageProcessingTime
        {
            get { return this.AverageProcessingTimeValue; }
            set { SetProperty(ref AverageProcessingTimeValue, value); }
        }

        private List<PaymentTypeAnalysis> PaymentsByTypeValue = new List<PaymentTypeAnalysis>();
        /// <summary>
        /// Gets or sets payment type analysis.
        /// </summary>
        public List<PaymentTypeAnalysis> PaymentsByType
        {
            get { return this.PaymentsByTypeValue; }
            set { SetProperty(ref PaymentsByTypeValue, value); }
        }
    }

    /// <summary>
    /// Payment Type Analysis
    /// </summary>
    public partial class PaymentTypeAnalysis : ModelEntityBase
    {
        private System.String PaymentMethodValue = string.Empty;
        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public System.String PaymentMethod
        {
            get { return this.PaymentMethodValue; }
            set { SetProperty(ref PaymentMethodValue, value); }
        }

        private System.Int32 CountValue;
        /// <summary>
        /// Gets or sets count.
        /// </summary>
        public System.Int32 Count
        {
            get { return this.CountValue; }
            set { SetProperty(ref CountValue, value); }
        }

        private System.Decimal TotalAmountValue;
        /// <summary>
        /// Gets or sets total amount.
        /// </summary>
        public System.Decimal TotalAmount
        {
            get { return this.TotalAmountValue; }
            set { SetProperty(ref TotalAmountValue, value); }
        }

        private System.Decimal AverageAmountValue;
        /// <summary>
        /// Gets or sets average amount.
        /// </summary>
        public System.Decimal AverageAmount
        {
            get { return this.AverageAmountValue; }
            set { SetProperty(ref AverageAmountValue, value); }
        }
    }

    /// <summary>
    /// Aged Outstanding Items Report
    /// </summary>
    public partial class AgedItemsReport : ModelEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets the as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets the analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private List<AgedItem> CurrentValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets current aged items.
        /// </summary>
        public List<AgedItem> Current
        {
            get { return this.CurrentValue; }
            set { SetProperty(ref CurrentValue, value); }
        }

        private List<AgedItem> _30to60DaysValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets 30-60 day aged items.
        /// </summary>
        public List<AgedItem> _30to60Days
        {
            get { return this._30to60DaysValue; }
            set { SetProperty(ref _30to60DaysValue, value); }
        }

        private List<AgedItem> _60to90DaysValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets 60-90 day aged items.
        /// </summary>
        public List<AgedItem> _60to90Days
        {
            get { return this._60to90DaysValue; }
            set { SetProperty(ref _60to90DaysValue, value); }
        }

        private List<AgedItem> Over90DaysValue = new List<AgedItem>();
        /// <summary>
        /// Gets or sets over 90 day aged items.
        /// </summary>
        public List<AgedItem> Over90Days
        {
            get { return this.Over90DaysValue; }
            set { SetProperty(ref Over90DaysValue, value); }
        }

        private System.Decimal CurrentTotalValue;
        /// <summary>
        /// Gets or sets current total.
        /// </summary>
        public System.Decimal CurrentTotal
        {
            get { return this.CurrentTotalValue; }
            set { SetProperty(ref CurrentTotalValue, value); }
        }

        private System.Decimal _30to60TotalValue;
        /// <summary>
        /// Gets or sets 30-60 day total.
        /// </summary>
        public System.Decimal _30to60Total
        {
            get { return this._30to60TotalValue; }
            set { SetProperty(ref _30to60TotalValue, value); }
        }

        private System.Decimal _60to90TotalValue;
        /// <summary>
        /// Gets or sets 60-90 day total.
        /// </summary>
        public System.Decimal _60to90Total
        {
            get { return this._60to90TotalValue; }
            set { SetProperty(ref _60to90TotalValue, value); }
        }

        private System.Decimal Over90TotalValue;
        /// <summary>
        /// Gets or sets over 90 day total.
        /// </summary>
        public System.Decimal Over90Total
        {
            get { return this.Over90TotalValue; }
            set { SetProperty(ref Over90TotalValue, value); }
        }

        private System.Decimal GrandTotalValue;
        /// <summary>
        /// Gets or sets grand total.
        /// </summary>
        public System.Decimal GrandTotal
        {
            get { return this.GrandTotalValue; }
            set { SetProperty(ref GrandTotalValue, value); }
        }
    }

    /// <summary>
    /// Aged Item
    /// </summary>
    public partial class AgedItem : ModelEntityBase
    {
        private System.DateTime EntryDateValue;
        /// <summary>
        /// Gets or sets the entry date.
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

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.Int32 AgeInDaysValue;
        /// <summary>
        /// Gets or sets age in days.
        /// </summary>
        public System.Int32 AgeInDays
        {
            get { return this.AgeInDaysValue; }
            set { SetProperty(ref AgeInDaysValue, value); }
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
    }
}


