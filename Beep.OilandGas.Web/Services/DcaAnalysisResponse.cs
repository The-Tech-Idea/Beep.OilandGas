using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.Web.Services
{
    public sealed class DcaAnalysisResponse
    {
        public string? OperationId { get; set; }
        public DCAResult? Result { get; set; }
    }
}