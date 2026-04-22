using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services
{
    public interface IDecommissioningServiceClient
    {
        Task<List<WellAbandonmentResponse>> GetAbandonedWellsAsync(Dictionary<string, string>? filters = null);
        Task<WellAbandonmentResponse?> GetWellAbandonmentAsync(string abandonmentId);
        Task<WellAbandonmentResponse?> AbandonWellAsync(string wellId, WellAbandonmentRequest request, string userId);
        Task<bool> ApprovePAAsync(string abandonmentId);
        Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesAsync(Dictionary<string, string>? filters = null);
        Task<FacilityDecommissioningResponse?> GetFacilityDecommissioningAsync(string decommissioningId);
        Task<DecommissioningCostEstimateResponse?> EstimateCostsAsync();
    }
}