using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Accounting;

namespace Beep.OilandGas.Accounting.Models
{
    public class ReconciliationDashboardDTO
    {
        public DateTime AsOfDate { get; set; }
        public List<ReconciliationSummary> Summaries { get; set; } = new List<ReconciliationSummary>();
        public bool OverallStatus { get; set; }
        public decimal TotalDiscrepancy { get; set; }
        public int ModulesInSync { get; set; }
        public int ModulesOutOfSync { get; set; }
    }

    /// <summary>
    /// Represents a detailed discrepancy item for drill-down
    /// </summary>
    public class ReconciliationDiscrepancyDetail
    {
        public string SourceModule { get; set; } // AP, AR, INV
        public string TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal SubledgerAmount { get; set; }
        public decimal GLAmount { get; set; }
        public decimal Variance { get; set; }
        public string Remarks { get; set; }
    }
}
