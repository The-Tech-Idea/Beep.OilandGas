using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for drilling and construction workflow operations.
    /// </summary>
    public interface IDrillingServiceClient
    {
        Task<List<DRILLING_OPERATION>> GetDrillingOperationsAsync(string? wellUWI = null);
        Task<DRILLING_OPERATION?> GetDrillingOperationAsync(string operationId);
        Task<DRILLING_OPERATION> CreateDrillingOperationAsync(CREATE_DRILLING_OPERATION createDto);
        Task<DRILLING_OPERATION> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperation updateDto);
        Task<List<DRILLING_REPORT>> GetDrillingReportsAsync(string operationId);
        Task<DRILLING_REPORT> CreateDrillingReportAsync(string operationId, CreateDrillingReport createDto);
    }
}