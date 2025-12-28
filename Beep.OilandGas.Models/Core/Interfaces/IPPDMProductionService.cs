using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for Production & Reserves data management service
    /// </summary>
    public interface IPPDMProductionService
    {
        /// <summary>
        /// Gets fields
        /// </summary>
        Task<List<FIELD>> GetFieldsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets pools
        /// </summary>
        Task<List<POOL>> GetPoolsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets production data (maps to PDEN_VOL_SUMMARY table)
        /// </summary>
        Task<List<PDEN_VOL_SUMMARY>> GetProductionAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets reserves data (maps to RESERVE_ENTITY table)
        /// </summary>
        Task<List<RESERVE_ENTITY>> GetReservesAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets production reporting data (maps to PDEN_VOL_SUMMARY table)
        /// </summary>
        Task<List<PDEN_VOL_SUMMARY>> GetProductionReportingAsync(List<AppFilter> filters = null);
    }
}

