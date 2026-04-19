using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.LifeCycle;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Operations
{
    /// <summary>
    /// Service for Operations Management including daily operations, shift handovers, incidents, safety, and compliance
    /// </summary>
    public class OperationsManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<OperationsManagementService>? _logger;

        public OperationsManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<OperationsManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        public async Task<OperationsResponse> RecordDailyOperationsAsync(DailyOperationsRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Daily operations recorded for {EntityType}: {EntityId}, Date: {OperationDate}", 
                    request.EntityType, request.EntityId, request.OperationDate);
                
                return new OperationsResponse
                {
                    OperationId = Guid.NewGuid().ToString(),
                    EntityId = request.EntityId,
                    EntityType = request.EntityType,
                    OperationDate = request.OperationDate,
                    Status = "RECORDED",
                    OperationData = request.OperationData
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording daily operations: {EntityId}", request.EntityId);
                throw;
            }
        }

        public async Task<bool> RecordShiftHandoverAsync(ShiftHandoverRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Shift handover recorded for {EntityType}: {EntityId}, From: {FromShift}, To: {ToShift}", 
                    request.EntityType, request.EntityId, request.FromShift, request.ToShift);
                await PersistActivityAsync(request.EntityType, request.EntityId,
                    "SHIFT_HANDOVER", request.HandoverDate,
                    $"{request.FromShift} → {request.ToShift}: {request.HandoverNotes}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording shift handover: {EntityId}", request.EntityId);
                throw;
            }
        }

        public async Task<bool> ReportIncidentAsync(IncidentRequest request, string userId)
        {
            try
            {
                _logger?.LogWarning("Incident reported for {EntityType}: {EntityId}, Type: {IncidentType}, Severity: {Severity}", 
                    request.EntityType, request.EntityId, request.IncidentType, request.Severity);
                await PersistActivityAsync(request.EntityType, request.EntityId,
                    "INCIDENT_" + request.IncidentType, request.IncidentDate,
                    $"Severity={request.Severity}, ReportedBy={request.ReportedBy}: {request.Description}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reporting incident: {EntityId}", request.EntityId);
                throw;
            }
        }

        public async Task<bool> ConductSafetyAssessmentAsync(SafetyAssessmentRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Safety assessment conducted for {EntityType}: {EntityId}, Date: {AssessmentDate}", 
                    request.EntityType, request.EntityId, request.AssessmentDate);
                await PersistActivityAsync(request.EntityType, request.EntityId,
                    "SAFETY_ASSESSMENT", request.AssessmentDate,
                    $"Assessor={request.Assessor}: {request.Findings}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error conducting safety assessment: {EntityId}", request.EntityId);
                throw;
            }
        }

        public async Task<bool> RecordComplianceAsync(ComplianceRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Compliance recorded for {EntityType}: {EntityId}, Type: {ComplianceType}, Status: {Status}", 
                    request.EntityType, request.EntityId, request.ComplianceType, request.Status);
                await PersistActivityAsync(request.EntityType, request.EntityId,
                    "COMPLIANCE_" + request.ComplianceType, request.ComplianceDate,
                    $"Status={request.Status}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording compliance: {EntityId}", request.EntityId);
                throw;
            }
        }

        private async Task PersistActivityAsync(string entityType, string entityId, string activityType, DateTime? eventDate, string remark, string userId)
        {
            var isWell = string.Equals(entityType, "WELL", StringComparison.OrdinalIgnoreCase);
            if (isWell)
            {
                var meta = await _metadata.GetTableMetadataAsync("WELL_ACTIVITY");
                if (meta != null)
                {
                    var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY", null);
                    var rec = new WELL_ACTIVITY
                    {
                        UWI = _defaults.FormatIdForTable("WELL_ACTIVITY", entityId),
                        SOURCE = "LIFECYCLE",
                        ACTIVITY_OBS_NO = Math.Abs((decimal)Guid.NewGuid().GetHashCode()),
                        ACTIVITY_TYPE_ID = activityType,
                        EVENT_DATE = eventDate,
                        START_DATE = eventDate,
                        REMARK = remark,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (rec is IPPDMEntity e) _commonColumnHandler.PrepareForInsert(e, userId);
                    await repo.InsertAsync(rec, userId);
                }
            }
            else
            {
                var meta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (meta != null)
                {
                    var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var rec = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", entityId),
                        FACILITY_TYPE = entityType?.ToUpperInvariant() ?? "FACILITY",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS = activityType,
                        STATUS_TYPE = activityType,
                        START_TIME = eventDate,
                        REMARK = remark,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (rec is IPPDMEntity e) _commonColumnHandler.PrepareForInsert(e, userId);
                    await repo.InsertAsync(rec, userId);
                }
            }
        }
    }
}

