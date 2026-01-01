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
using Beep.OilandGas.Models.Data.Common;

namespace Beep.OilandGas.LifeCycle.Services.ReservoirLifecycle
{
    /// <summary>
    /// Service for managing Reservoir lifecycle state transitions
    /// States: DISCOVERED → APPRAISED → DEVELOPED → PRODUCING → DEPLETED → ABANDONED
    /// </summary>
    public class ReservoirLifecycleService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<ReservoirLifecycleService>? _logger;

        // Reservoir state transitions
        private readonly Dictionary<string, List<string>> _allowedTransitions = new Dictionary<string, List<string>>
        {
            { "DISCOVERED", new List<string> { "APPRAISED", "REJECTED" } },
            { "APPRAISED", new List<string> { "DEVELOPED", "REJECTED" } },
            { "DEVELOPED", new List<string> { "PRODUCING", "REJECTED" } },
            { "PRODUCING", new List<string> { "DEPLETED", "ABANDONED" } },
            { "DEPLETED", new List<string> { "ABANDONED" } },
            { "ABANDONED", new List<string>() }, // Terminal state
            { "REJECTED", new List<string>() } // Terminal state
        };

        public ReservoirLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<ReservoirLifecycleService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        /// <summary>
        /// Transition reservoir to a new state
        /// </summary>
        public async Task<bool> TransitionReservoirStateAsync(string reservoirId, string targetState, string userId)
        {
            try
            {
                var currentState = await GetCurrentReservoirStateAsync(reservoirId);
                if (string.IsNullOrEmpty(currentState))
                {
                    currentState = "DISCOVERED"; // Default initial state
                }

                if (!CanTransition(currentState, targetState))
                {
                    _logger?.LogWarning($"Invalid transition from {currentState} to {targetState} for reservoir {reservoirId}");
                    return false;
                }

                // Update reservoir status
                await UpdateReservoirStatusAsync(reservoirId, targetState, userId);

                _logger?.LogInformation($"Reservoir {reservoirId} transitioned from {currentState} to {targetState}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error transitioning reservoir {reservoirId} to state {targetState}");
                throw;
            }
        }

        /// <summary>
        /// Get available transitions for a reservoir
        /// </summary>
        public async Task<List<string>> GetAvailableTransitionsAsync(string reservoirId)
        {
            try
            {
                var currentState = await GetCurrentReservoirStateAsync(reservoirId);
                if (string.IsNullOrEmpty(currentState))
                {
                    currentState = "DISCOVERED";
                }

                return _allowedTransitions.ContainsKey(currentState)
                    ? _allowedTransitions[currentState]
                    : new List<string>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting available transitions for reservoir {reservoirId}");
                throw;
            }
        }

        /// <summary>
        /// Check if a transition is allowed
        /// </summary>
        public async Task<bool> CanTransitionAsync(string reservoirId, string targetState)
        {
            try
            {
                var currentState = await GetCurrentReservoirStateAsync(reservoirId);
                if (string.IsNullOrEmpty(currentState))
                {
                    currentState = "DISCOVERED";
                }

                return CanTransition(currentState, targetState);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error checking transition for reservoir {reservoirId}");
                throw;
            }
        }

        /// <summary>
        /// Get current reservoir state
        /// </summary>
        public async Task<string> GetCurrentReservoirStateAsync(string reservoirId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("RESERVOIR_STATUS");
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
                    entityType, _connectionName, "RESERVOIR_STATUS");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "RESERVE_ENTITY_ID",
                        FilterValue = _defaults.FormatIdForTable("RESERVOIR_STATUS", reservoirId),
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "ACTIVE_IND",
                        FilterValue = "Y",
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                var latest = results
                    .OrderByDescending(r => GetDateTimeValue(r, "STATUS_DATE"))
                    .FirstOrDefault();

                if (latest != null)
                {
                    return GetStringValue(latest, "STATUS") ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting current reservoir state for reservoir {reservoirId}");
                throw;
            }
        }

        /// <summary>
        /// Get reservoir state history
        /// </summary>
        public async Task<List<(DateTime Date, string Status)>> GetReservoirStateHistoryAsync(string reservoirId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("RESERVOIR_STATUS");
                if (metadata == null)
                {
                    return new List<(DateTime, string)>();
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    return new List<(DateTime, string)>();
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "RESERVOIR_STATUS");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "RESERVE_ENTITY_ID",
                        FilterValue = _defaults.FormatIdForTable("RESERVOIR_STATUS", reservoirId),
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                var history = new List<(DateTime, string)>();

                foreach (var result in results.OrderBy(r => GetDateTimeValue(r, "STATUS_DATE")))
                {
                    var date = GetDateTimeValue(result, "STATUS_DATE");
                    var status = GetStringValue(result, "STATUS");
                    if (date.HasValue && !string.IsNullOrEmpty(status))
                    {
                        history.Add((date.Value, status));
                    }
                }

                return history;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting reservoir state history for reservoir {reservoirId}");
                throw;
            }
        }

        /// <summary>
        /// Validate state transition
        /// </summary>
        public async Task<ValidationResult> ValidateStateTransitionAsync(string reservoirId, string fromState, string toState)
        {
            var result = new ValidationResult
            {
                ValidationId = Guid.NewGuid().ToString(),
                IsValid = true,
                ValidatedDate = DateTime.UtcNow
            };

            if (!CanTransition(fromState, toState))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Invalid transition from {fromState} to {toState}";
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Validate reservoir is in required state
        /// </summary>
        public async Task<bool> ValidateReservoirStateAsync(string reservoirId, string requiredState)
        {
            var currentState = await GetCurrentReservoirStateAsync(reservoirId);
            return currentState == requiredState;
        }

        #region Private Methods

        private bool CanTransition(string fromState, string toState)
        {
            if (!_allowedTransitions.ContainsKey(fromState))
            {
                return false;
            }

            return _allowedTransitions[fromState].Contains(toState);
        }

        private async Task UpdateReservoirStatusAsync(string reservoirId, string newState, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("RESERVOIR_STATUS");
                if (metadata == null)
                {
                    throw new InvalidOperationException("RESERVOIR_STATUS table metadata not found");
                }

                // Try PPDM39.Models first, then Models.Data for moved classes
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    throw new InvalidOperationException($"Entity type not found for RESERVOIR_STATUS: {metadata.EntityTypeName}");
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "RESERVOIR_STATUS");

                // Deactivate current status
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "RESERVE_ENTITY_ID", FilterValue = _defaults.FormatIdForTable("RESERVOIR_STATUS", reservoirId), Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };

                var existing = await repo.GetAsync(filters);
                foreach (var item in existing)
                {
                    var entity = item as Dictionary<string, object> ?? new Dictionary<string, object>();
                    entity["ACTIVE_IND"] = "N";
                    entity["ROW_CHANGED_BY"] = userId;
                    entity["ROW_CHANGED_DATE"] = DateTime.UtcNow;
                    await repo.UpdateAsync(entity, userId);
                }

                // Create new status entry
                var newStatus = new Dictionary<string, object>
                {
                    ["RESERVOIR_STATUS_ID"] = GenerateStatusId(),
                    ["RESERVE_ENTITY_ID"] = _defaults.FormatIdForTable("RESERVOIR_STATUS", reservoirId),
                    ["STATUS"] = newState,
                    ["STATUS_DATE"] = DateTime.UtcNow,
                    ["STATUS_CHANGED_BY"] = userId,
                    ["ACTIVE_IND"] = "Y",
                    ["ROW_CREATED_BY"] = userId,
                    ["ROW_CREATED_DATE"] = DateTime.UtcNow
                };

                await repo.InsertAsync(newStatus, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating reservoir status for reservoir {reservoirId}");
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

        private string GenerateStatusId() => $"RS_{Guid.NewGuid():N}";

        #endregion
    }

    /// <summary>
    /// Validation result for state transitions
    /// </summary>
  
}

