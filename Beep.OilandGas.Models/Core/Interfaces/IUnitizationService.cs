using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

using Beep.OilandGas.Models.DTOs.Unitization;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for unitization operations.
    /// </summary>
    public interface IUnitizationService
    {
        /// <summary>
        /// Creates a unit agreement.
        /// </summary>
        Task<UNIT_AGREEMENT> CreateUnitAgreementAsync(CreateUnitAgreementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a unit agreement by ID.
        /// </summary>
        Task<UNIT_AGREEMENT?> GetUnitAgreementAsync(string agreementId, string? connectionName = null);
        
        /// <summary>
        /// Gets all unit agreements.
        /// </summary>
        Task<List<UNIT_AGREEMENT>> GetUnitAgreementsAsync(string? connectionName = null);
        
        /// <summary>
        /// Creates a participating area.
        /// </summary>
        Task<PARTICIPATING_AREA> CreateParticipatingAreaAsync(CreateParticipatingAreaRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets participating areas by unit.
        /// </summary>
        Task<List<PARTICIPATING_AREA>> GetParticipatingAreasByUnitAsync(string unitId, string? connectionName = null);
        
        /// <summary>
        /// Registers tract participation.
        /// </summary>
        Task<TRACT_PARTICIPATION> RegisterTractParticipationAsync(CreateTractParticipationRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets tract participations by area.
        /// </summary>
        Task<List<TRACT_PARTICIPATION>> GetTractParticipationsByAreaAsync(string areaId, string? connectionName = null);
        
        /// <summary>
        /// Approves a unit agreement.
        /// </summary>
        Task<UnitApprovalResult> ApproveUnitAgreementAsync(string agreementId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets unit operations summary.
        /// </summary>
        Task<UnitOperationsSummary> GetUnitOperationsSummaryAsync(string unitId, string? connectionName = null);
        
        /// <summary>
        /// Gets agreements requiring approval.
        /// </summary>
        Task<List<UNIT_AGREEMENT>> GetAgreementsRequiringApprovalAsync(string? connectionName = null);
    }
}




