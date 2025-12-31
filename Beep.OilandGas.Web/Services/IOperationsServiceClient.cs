using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for operations (ProspectIdentification, EnhancedRecovery, LeaseAcquisition, DrillingOperation).
    /// </summary>
    public interface IOperationsServiceClient
    {
        // Prospect Identification Operations
        Task<ProspectEvaluationDto> EvaluateProspectAsync(string prospectId);
        Task<List<ProspectDto>> GetProspectsAsync(Dictionary<string, string>? filters = null);
        Task<string> CreateProspectAsync(ProspectDto prospect, string? userId = null);
        Task<List<ProspectRankingDto>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria);

        // Enhanced Recovery Operations
        Task<EnhancedRecoveryOperationDto> AnalyzeEORPotentialAsync(string fieldId, string eorMethod);
        Task<EnhancedRecoveryOperationDto> CalculateRecoveryFactorAsync(string projectId);
        Task<InjectionOperationDto> ManageInjectionAsync(string injectionWellId, decimal injectionRate);

        // Lease Acquisition Operations
        Task<LeaseSummary> EvaluateLeaseAsync(string leaseId);
        Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null);
        Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisitionDto leaseRequest, string? userId = null);
        Task<bool> UpdateLeaseStatusAsync(string leaseId, string status, string? userId = null);

        // Drilling Operation Operations
        Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null);
        Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId);
        Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto);
        Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto);
        Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId);
        Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto);
    }
}

