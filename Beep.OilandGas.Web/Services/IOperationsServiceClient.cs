using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for operations (ProspectIdentification, EnhancedRecovery, LeaseAcquisition, DrillingOperation).
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
        Task<List<DrillingOperation>> GetDrillingOperationsAsync(string? wellUWI = null);
        Task<DrillingOperation?> GetDrillingOperationAsync(string operationId);
        Task<DrillingOperation> CreateDrillingOperationAsync(CreateDrillingOperation createDto);
        Task<DrillingOperation> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperation updateDto);
        Task<List<DrillingReport>> GetDrillingReportsAsync(string operationId);
        Task<DrillingReport> CreateDrillingReportAsync(string operationId, CreateDrillingReport createDto);
    }
}

