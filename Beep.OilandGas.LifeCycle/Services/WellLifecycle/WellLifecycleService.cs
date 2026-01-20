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
using TheTechIdea.Beep.Editor;

using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Services.WellLifecycle
{
    /// <summary>
    /// Service for managing Well lifecycle state transitions
    /// States: PLANNED → DRILLING → COMPLETED → PRODUCING → WORKOVER → SUSPENDED → ABANDONED
    /// </summary>
    public class WellLifecycleService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<WellLifecycleService>? _logger;

        // Well state machine
        private readonly Dictionary<string, List<string>> _allowedTransitions = new Dictionary<string, List<string>>
        {
            { "PLANNED", new List<string> { "DRILLING", "REJECTED" } },
            { "DRILLING", new List<string> { "COMPLETED", "REJECTED" } },
            { "COMPLETED", new List<string> { "PRODUCING", "REJECTED" } },
            { "PRODUCING", new List<string> { "WORKOVER", "SUSPENDED", "ABANDONED" } },
            { "WORKOVER", new List<string> { "PRODUCING", "ABANDONED" } },
            { "SUSPENDED", new List<string> { "PRODUCING", "ABANDONED" } },
            { "ABANDONED", new List<string>() }, // Terminal state
            { "REJECTED", new List<string>() } // Terminal state
        };

        public WellLifecycleService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<WellLifecycleService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        /// <summary>
        /// Transition well to a new state
        /// </summary>
        public async Task<bool> TransitionWellStateAsync(string wellId, string targetState, string userId)
        {
            try
            {
                var currentState = await GetCurrentWellStateAsync(wellId);
                if (string.IsNullOrEmpty(currentState))
                {
                    currentState = "PLANNED"; // Default initial state
                }

                if (!CanTransition(currentState, targetState))
                {
                    _logger?.LogWarning($"Invalid transition from {currentState} to {targetState} for well {wellId}");
                    return false;
                }

                // Update well status in WELL_STATUS table
                await UpdateWellStatusAsync(wellId, targetState, userId);

                _logger?.LogInformation($"Well {wellId} transitioned from {currentState} to {targetState}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error transitioning well {wellId} to state {targetState}");
                throw;
            }
        }

        /// <summary>
        /// Get available transitions for a well
        /// </summary>
        public async Task<List<string>> GetAvailableTransitionsAsync(string wellId)
        {
            try
            {
                var currentState = await GetCurrentWellStateAsync(wellId);
                if (string.IsNullOrEmpty(currentState))
                {
                    currentState = "PLANNED";
                }

                return _allowedTransitions.ContainsKey(currentState)
                    ? _allowedTransitions[currentState]
                    : new List<string>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting available transitions for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Check if a transition is allowed
        /// </summary>
        public async Task<bool> CanTransitionAsync(string wellId, string targetState)
        {
            try
            {
                var currentState = await GetCurrentWellStateAsync(wellId);
                if (string.IsNullOrEmpty(currentState))
                {
                    currentState = "PLANNED";
                }

                return CanTransition(currentState, targetState);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error checking transition for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Get current well state
        /// </summary>
        public async Task<string> GetCurrentWellStateAsync(string wellId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_STATUS");
                if (metadata == null)
                {
                    return string.Empty;
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    return string.Empty;
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL_STATUS");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        FilterValue = _defaults.FormatIdForTable("WELL_STATUS", wellId),
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
                    .OrderByDescending(r => GetDateTimeValue(r, "EFFECTIVE_DATE"))
                    .FirstOrDefault();

                if (latest != null)
                {
                    return GetStringValue(latest, "STATUS_TYPE") ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting current well state for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Get well state history
        /// </summary>
        public async Task<List<(DateTime Date, string Status)>> GetWellStateHistoryAsync(string wellId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_STATUS");
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
                    entityType, _connectionName, "WELL_STATUS");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        FilterValue = _defaults.FormatIdForTable("WELL_STATUS", wellId),
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                var history = new List<(DateTime, string)>();

                foreach (var result in results.OrderBy(r => GetDateTimeValue(r, "EFFECTIVE_DATE")))
                {
                    var date = GetDateTimeValue(result, "EFFECTIVE_DATE");
                    var status = GetStringValue(result, "STATUS_TYPE");
                    if (date.HasValue && !string.IsNullOrEmpty(status))
                    {
                        history.Add((date.Value, status));
                    }
                }

                return history;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting well state history for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Validate state transition
        /// </summary>
        public async Task<ValidationResult> ValidateStateTransitionAsync(string wellId, string fromState, string toState)
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

            // Additional business rule validations can be added here

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Validate well is in required state
        /// </summary>
        public async Task<bool> ValidateWellStateAsync(string wellId, string requiredState)
        {
            var currentState = await GetCurrentWellStateAsync(wellId);
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

        private async Task UpdateWellStatusAsync(string wellId, string newState, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_STATUS");
                if (metadata == null)
                {
                    throw new InvalidOperationException("WELL_STATUS table metadata not found");
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    throw new InvalidOperationException($"Entity type not found for WELL_STATUS: {metadata.EntityTypeName}");
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL_STATUS");

                // Deactivate current status
                var currentStatus = await GetCurrentWellStateAsync(wellId);
                if (!string.IsNullOrEmpty(currentStatus))
                {
                    var filters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "UWI", FilterValue = _defaults.FormatIdForTable("WELL_STATUS", wellId), Operator = "=" },
                        new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                    };

                    var existing = await repo.GetAsync(filters);
                    foreach (var item in existing)
                    {
                        var entity = item as Dictionary<string, object> ?? new Dictionary<string, object>();
                        entity["ACTIVE_IND"] = "N";
                        entity["END_TIME"] = DateTime.UtcNow;
                        entity["ROW_CHANGED_BY"] = userId;
                        entity["ROW_CHANGED_DATE"] = DateTime.UtcNow;
                        await repo.UpdateAsync(entity, userId);
                    }
                }

                // Create new status entry
                var newStatus = new Dictionary<string, object>
                {
                    ["STATUS_ID"] = GenerateStatusId(),
                    ["UWI"] = _defaults.FormatIdForTable("WELL_STATUS", wellId),
                    ["STATUS_TYPE"] = newState,
                    ["ACTIVE_IND"] = "Y",
                    ["EFFECTIVE_DATE"] = DateTime.UtcNow,
                    ["ROW_CREATED_BY"] = userId,
                    ["ROW_CREATED_DATE"] = DateTime.UtcNow
                };

                await repo.InsertAsync(newStatus, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating well status for well {wellId}");
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

        private string GenerateStatusId() => $"WS_{Guid.NewGuid():N}";

        #endregion
    }

    /// <summary>
    /// Validation result for state transitions
    /// </summary>
  
}

