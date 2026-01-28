using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ImbalanceReconciliation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the reconciliation identifier.
        /// </summary>
        private string ReconciliationIdValue = string.Empty;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the reconciliation date.
        /// </summary>
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the total imbalance before reconciliation.
        /// </summary>
        private decimal TotalImbalanceBeforeValue;

        public decimal TotalImbalanceBefore

        {

            get { return this.TotalImbalanceBeforeValue; }

            set { SetProperty(ref TotalImbalanceBeforeValue, value); }

        }

        /// <summary>
        /// Gets or sets the adjustments made.
        /// </summary>
        private List<ImbalanceAdjustment> AdjustmentsValue = new();

        public List<ImbalanceAdjustment> Adjustments

        {

            get { return this.AdjustmentsValue; }

            set { SetProperty(ref AdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets the total adjustments.
        /// </summary>
        public decimal TotalAdjustments => Adjustments.Sum(a => a.AdjustmentAmount);

        /// <summary>
        /// Gets the total imbalance after reconciliation.
        /// </summary>
        public decimal TotalImbalanceAfter => TotalImbalanceBefore + TotalAdjustments;

        /// <summary>
        /// Gets or sets the reconciled by.
        /// </summary>
        private string ReconciledByValue = string.Empty;

        public string ReconciledBy

        {

            get { return this.ReconciledByValue; }

            set { SetProperty(ref ReconciledByValue, value); }

        }

        /// <summary>
        /// Gets or sets any notes.
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
