using System;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>Payload for service-backed inventory reconciliation reporting.</summary>
    public class GenerateInventoryReconciliationReportRequest
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
