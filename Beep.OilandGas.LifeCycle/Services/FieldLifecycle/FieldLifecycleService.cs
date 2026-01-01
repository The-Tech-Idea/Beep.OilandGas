using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Data.LifeCycle;

namespace Beep.OilandGas.LifeCycle.Services.FieldLifecycle
{
    /// <summary>
    /// Service for managing Field lifecycle phase transitions
    /// Phases: EXPLORATION → DEVELOPMENT → PRODUCTION → DECLINE → DECOMMISSIONING → DECOMMISSIONED
    /// </summary>
    public class FieldLifecycleService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<FieldLifecycleService>? _logger;

        // Field phase transitions
        private readonly Dictionary<string, List<string>> _allowedTransitions = new Dictionary<string, List<string>>
        {
            { "EXPLORATION", new List<string> { "DEVELOPMENT", "REJECTED" } },
            { "DEVELOPMENT", new List<string> { "PRODUCTION", "REJECTED" } },
            { "PRODUCTION", new List<string> { "DECLINE", "DECOMMISSIONING" } },
            { "DECLINE", new List<string> { "PRODUCTION", "DECOMMISSIONING" } },
            { "DECOMMISSIONING", new List<string> { "DECOMMISSIONED" } },
            { "DECOMMISSIONED", new List<string>() }, // Terminal state
            { "REJECTED", new List<string>() } // Terminal state
        };

        public FieldLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<FieldLifecycleService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        /// <summary>
        /// Transition field to a new phase
        /// </summary>
        public async Task<bool> TransitionFieldPhaseAsync(string fieldId, string targetPhase, string userId)
        {
            try
            {
                var currentPhase = await GetCurrentFieldPhaseAsync(fieldId);
                if (string.IsNullOrEmpty(currentPhase))
                {
                    currentPhase = "EXPLORATION"; // Default initial phase
                }

                if (!CanTransition(currentPhase, targetPhase))
                {
                    _logger?.LogWarning($"Invalid transition from {currentPhase} to {targetPhase} for field {fieldId}");
                    return false;
                }

                // End current phase
                await EndFieldPhaseAsync(fieldId, currentPhase, userId);

                // Start new phase
                await StartFieldPhaseAsync(fieldId, targetPhase, userId);

                _logger?.LogInformation($"Field {fieldId} transitioned from {currentPhase} to {targetPhase}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error transitioning field {fieldId} to phase {targetPhase}");
                throw;
            }
        }

        /// <summary>
        /// Get available phase transitions for a field
        /// </summary>
        public async Task<List<string>> GetAvailablePhaseTransitionsAsync(string fieldId)
        {
            try
            {
                var currentPhase = await GetCurrentFieldPhaseAsync(fieldId);
                if (string.IsNullOrEmpty(currentPhase))
                {
                    currentPhase = "EXPLORATION";
                }

                return _allowedTransitions.ContainsKey(currentPhase)
                    ? _allowedTransitions[currentPhase]
                    : new List<string>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting available phase transitions for field {fieldId}");
                throw;
            }
        }

        /// <summary>
        /// Check if a phase transition is allowed
        /// </summary>
        public async Task<bool> CanTransitionPhaseAsync(string fieldId, string targetPhase)
        {
            try
            {
                var currentPhase = await GetCurrentFieldPhaseAsync(fieldId);
                if (string.IsNullOrEmpty(currentPhase))
                {
                    currentPhase = "EXPLORATION";
                }

                return CanTransition(currentPhase, targetPhase);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error checking phase transition for field {fieldId}");
                throw;
            }
        }

        /// <summary>
        /// Get current field phase
        /// </summary>
        public async Task<string> GetCurrentFieldPhaseAsync(string fieldId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD_PHASE");
                if (metadata == null)
                {
                    return string.Empty;
                }

                // Try PPDM39.Models first, then Models.Data for moved classes
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    return string.Empty;
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD_PHASE");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FIELD_PHASE", fieldId),
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "PHASE_STATUS",
                        FilterValue = "ACTIVE",
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                var latest = results
                    .OrderByDescending(r => GetDateTimeValue(r, "PHASE_START_DATE"))
                    .FirstOrDefault();

                if (latest != null)
                {
                    return GetStringValue(latest, "PHASE") ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting current field phase for field {fieldId}");
                throw;
            }
        }

        /// <summary>
        /// Get field phase status
        /// </summary>
        public async Task<FieldPhaseStatus> GetFieldPhaseStatusAsync(string fieldId)
        {
            try
            {
                var currentPhase = await GetCurrentFieldPhaseAsync(fieldId);
                var availableTransitions = await GetAvailablePhaseTransitionsAsync(fieldId);

                return new FieldPhaseStatus
                {
                    FieldId = fieldId,
                    CurrentPhase = currentPhase,
                    AvailableTransitions = availableTransitions,
                    PhaseCompletionStatus = new Dictionary<string, bool>()
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting field phase status for field {fieldId}");
                throw;
            }
        }

        /// <summary>
        /// Validate phase transition
        /// </summary>
        public async Task<ValidationResult> ValidatePhaseTransitionAsync(string fieldId, string fromPhase, string toPhase)
        {
            var result = new ValidationResult
            {
                ValidationId = Guid.NewGuid().ToString(),
                IsValid = true,
                ValidatedDate = DateTime.UtcNow
            };

            if (!CanTransition(fromPhase, toPhase))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Invalid transition from {fromPhase} to {toPhase}";
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Validate phase completion
        /// </summary>
        public async Task<bool> ValidatePhaseCompletionAsync(string fieldId, string phase)
        {
            // Business logic to validate phase completion
            // For example, check if all required activities are completed
            return await Task.FromResult(true);
        }

        #region Private Methods

        private bool CanTransition(string fromPhase, string toPhase)
        {
            if (!_allowedTransitions.ContainsKey(fromPhase))
            {
                return false;
            }

            return _allowedTransitions[fromPhase].Contains(toPhase);
        }

        private async Task StartFieldPhaseAsync(string fieldId, string phase, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD_PHASE");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FIELD_PHASE table metadata not found");
                }

                // Try PPDM39.Models first, then Models.Data for moved classes
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    throw new InvalidOperationException($"Entity type not found for FIELD_PHASE: {metadata.EntityTypeName}");
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD_PHASE");

                var phaseEntity = new Dictionary<string, object>
                {
                    ["FIELD_PHASE_ID"] = GeneratePhaseId(),
                    ["FIELD_ID"] = _defaults.FormatIdForTable("FIELD_PHASE", fieldId),
                    ["PHASE"] = phase,
                    ["PHASE_START_DATE"] = DateTime.UtcNow,
                    ["PHASE_STATUS"] = "ACTIVE",
                    ["ACTIVE_IND"] = "Y",
                    ["ROW_CREATED_BY"] = userId,
                    ["ROW_CREATED_DATE"] = DateTime.UtcNow
                };

                await repo.InsertAsync(phaseEntity, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting field phase for field {fieldId}");
                throw;
            }
        }

        private async Task EndFieldPhaseAsync(string fieldId, string phase, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD_PHASE");
                if (metadata == null)
                {
                    return;
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    return;
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD_PHASE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", FilterValue = _defaults.FormatIdForTable("FIELD_PHASE", fieldId), Operator = "=" },
                    new AppFilter { FieldName = "PHASE", FilterValue = phase, Operator = "=" },
                    new AppFilter { FieldName = "PHASE_STATUS", FilterValue = "ACTIVE", Operator = "=" }
                };

                var results = await repo.GetAsync(filters);
                foreach (var result in results)
                {
                    var entity = result as Dictionary<string, object> ?? new Dictionary<string, object>();
                    entity["PHASE_END_DATE"] = DateTime.UtcNow;
                    entity["PHASE_STATUS"] = "COMPLETED";
                    entity["ROW_CHANGED_BY"] = userId;
                    entity["ROW_CHANGED_DATE"] = DateTime.UtcNow;
                    await repo.UpdateAsync(entity, userId);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error ending field phase for field {fieldId}");
                throw;
            }
        }

        private string GetStringValue(object entity, string fieldName)
        {
            if (entity is Dictionary<string, object> dict)
            {
                return dict.ContainsKey(fieldName) ? dict[fieldName]?.ToString() ?? string.Empty : string.Empty;
            }

            var prop = entity.GetType().GetProperty(fieldName);
            return prop?.GetValue(entity)?.ToString() ?? string.Empty;
        }

        private DateTime? GetDateTimeValue(object entity, string fieldName)
        {
            if (entity is Dictionary<string, object> dict)
            {
                if (!dict.ContainsKey(fieldName))
                    return null;

                var value = dict[fieldName];
                if (value is DateTime dt)
                    return dt;
                if (value is string str && DateTime.TryParse(str, out var parsed))
                    return parsed;
                return null;
            }

            var prop = entity.GetType().GetProperty(fieldName);
            var propValue = prop?.GetValue(entity);
            if (propValue is DateTime dt2)
                return dt2;
            if (propValue is string str2 && DateTime.TryParse(str2, out var parsed2))
                return parsed2;
            return null;
        }

        private string GeneratePhaseId() => $"FP_{Guid.NewGuid():N}";

        #endregion
    }

    /// <summary>
    /// Field phase status information
    /// </summary>
    public class FieldPhaseStatus
    {
        public string FieldId { get; set; } = string.Empty;
        public string CurrentPhase { get; set; } = string.Empty;
        public List<string> AvailableTransitions { get; set; } = new List<string>();
        public Dictionary<string, bool> PhaseCompletionStatus { get; set; } = new Dictionary<string, bool>();
    }
}
   

