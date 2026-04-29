using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Cost
{
    /// <summary>Service-backed allocation request payload.</summary>
    public class AllocateProductionRequest
    {
        public RUN_TICKET? RunTicket { get; set; }
        public string? Method { get; set; }
    }
}
