using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for operations (ProspectIdentification, EnhancedRecovery, LeaseAcquisition, DRILLING_OPERATION).
    /// </summary>
    public interface IOperationsServiceClient
    {
        // Prospect Identification Operations
        Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId);
        Task<List<Prospect>> GetProspectsAsync(Dictionary<string, string>? filters = null);
        Task<string> CreateProspectAsync(Prospect prospect, string? userId = null);
        Task<List<ProspectRanking>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria);

        // Enhanced Recovery Operations
        Task<EnhancedRecoveryOperation> AnalyzeEORPotentialAsync(string fieldId, string eorMethod);
        Task<EnhancedRecoveryOperation> CalculateRecoveryFactorAsync(string projectId);
        Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate);

        // Lease Acquisition Operations
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId);
        Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null);
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest, string? userId = null);
        Task<bool> UpdateLeaseStatusAsync(string leaseId, string status, string? userId = null);

        // Drilling Operation Operations
        Task<List<DRILLING_OPERATION>> GetDrillingOperationsAsync(string? wellUWI = null);
        Task<DRILLING_OPERATION?> GetDrillingOperationAsync(string operationId);
        Task<DRILLING_OPERATION> CreateDrillingOperationAsync(CREATE_DRILLING_OPERATION createDto);
        Task<DRILLING_OPERATION> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperation updateDto);
        Task<List<DRILLING_REPORT>> GetDrillingReportsAsync(string operationId);
        Task<DRILLING_REPORT> CreateDrillingReportAsync(string operationId, CreateDrillingReport createDto);
    }
}

