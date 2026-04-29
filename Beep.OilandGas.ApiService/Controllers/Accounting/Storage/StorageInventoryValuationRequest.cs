using System;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Storage
{
    /// <summary>Payload for service-backed storage valuation requests.</summary>
    public class StorageInventoryValuationRequest
    {
        public DateTime ValuationDate { get; set; }
        public string Method { get; set; } = string.Empty;
    }
}
