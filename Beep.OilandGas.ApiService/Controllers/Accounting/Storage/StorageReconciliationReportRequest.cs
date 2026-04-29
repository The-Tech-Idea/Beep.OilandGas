using System;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Storage
{
    /// <summary>Payload for service-backed storage reconciliation report requests.</summary>
    public class StorageReconciliationReportRequest
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
