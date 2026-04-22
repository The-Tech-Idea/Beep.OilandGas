using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for shared operations concerns that still span prospect identification, enhanced recovery, and lease acquisition.
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
        Task<EnhancedRecoveryOperation> CalculateRecoveryFactorAsync(string operationId);
        Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate);

        // Lease Acquisition Operations
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId);
        Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null);
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest, string? userId = null);
        Task<bool> UpdateLeaseStatusAsync(string leaseId, string status, string? userId = null);
    }
}

