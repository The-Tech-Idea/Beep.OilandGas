using TheTechIdea.Beep.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Core.DTOs
{
    /// <summary>
    /// Interface for Seismic data management service
    /// </summary>
    public interface IPPDMSeismicService
    {
        /// <summary>
        /// Gets seismic surveys
        /// </summary>
        Task<List<object>> GetSeismicSurveysAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets seismic acquisition data
        /// </summary>
        Task<List<object>> GetSeismicAcquisitionAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets seismic processing data
        /// </summary>
        Task<List<object>> GetSeismicProcessingAsync(List<AppFilter> filters = null);
    }
}


