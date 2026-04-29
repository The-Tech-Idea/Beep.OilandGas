using System;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>Payload for service-backed inventory valuation.</summary>
    public class CalculateInventoryValuationRequest
    {
        public DateTime ValuationDate { get; set; }
        public string Method { get; set; } = string.Empty;
    }
}
