using TheTechIdea.Beep.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Core.DTOs
{
    /// <summary>
    /// Interface for Production & Reserves data management service
    /// </summary>
    public interface IPPDMProductionService
    {
        /// <summary>
        /// Gets fields
        /// </summary>
        Task<List<object>> GetFieldsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets pools
        /// </summary>
        Task<List<object>> GetPoolsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets production data
        /// </summary>
        Task<List<object>> GetProductionAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets reserves data
        /// </summary>
        Task<List<object>> GetReservesAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets production reporting data
        /// </summary>
        Task<List<object>> GetProductionReportingAsync(List<AppFilter> filters = null);
    }
}



