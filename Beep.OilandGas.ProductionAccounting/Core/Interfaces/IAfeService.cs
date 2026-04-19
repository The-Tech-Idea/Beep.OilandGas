using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for AFE (Authorization for Expenditure) management.
    /// </summary>
    public interface IAfeService
    {
        Task<AFE> CreateAfeAsync(AFE afe, string userId, string cn = "PPDM39");
        Task<AFE_LINE_ITEM> AddLineItemAsync(AFE_LINE_ITEM lineItem, string userId, string cn = "PPDM39");
        Task<AFE> ApproveAfeAsync(string afeId, DateTime approvalDate, string userId, string cn = "PPDM39");
        Task<ACCOUNTING_COST> RecordCostAsync(string afeId, ACCOUNTING_COST cost, string userId, string cn = "PPDM39");
        Task<List<AFE_LINE_ITEM>> GetLineItemsAsync(string afeId, string cn = "PPDM39");
        Task<COST_VARIANCE_REPORT> GenerateBudgetVarianceReportAsync(string afeId, decimal varianceThreshold, string userId, string cn = "PPDM39");
        Task<List<COST_VARIANCE_REPORT>> GetVarianceReportsAsync(string? afeId, string? costCenterId, DateTime? startDate, DateTime? endDate, string cn = "PPDM39");
    }
}
