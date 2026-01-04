using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Centralized service for managing PPDM default values
    /// Provides caching and batch operations for default value management
    /// </summary>
    public class PPDMDefaultValueService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private PPDMGenericRepository _repository;
        
        // Cache for default values (key: databaseId_userId_key, value: default value)
        private readonly Dictionary<string, string> _valueCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _cacheLock = new object();

        public PPDMDefaultValueService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PPDM_DEFAULT_VALUE), _connectionName, "PPDM_DEFAULT_VALUE");
        }

        /// <summary>
        /// Gets a default value (with caching)
        /// </summary>
        public async Task<string?> GetDefaultValueAsync(string key, string databaseId, string? userId = null)
        {
            var cacheKey = $"{databaseId}_{userId ?? "SYSTEM"}_{key}";
            
            lock (_cacheLock)
            {
                if (_valueCache.TryGetValue(cacheKey, out var cachedValue))
                    return cachedValue;
            }

            var value = await _defaults.GetDefaultValueAsync(key, databaseId, userId);
            
            if (value != null)
            {
                lock (_cacheLock)
                {
                    _valueCache[cacheKey] = value;
                }
            }

            return value;
        }

        /// <summary>
        /// Sets or updates a default value (clears cache)
        /// </summary>
        public async Task SetDefaultValueAsync(string key, string value, string databaseId, string? userId = null, string category = "System", string valueType = "String", string description = null)
        {
            await _defaults.SetDefaultValueAsync(key, value, databaseId, userId, category, valueType, description);
            
            // Clear cache for this key
            var cacheKey = $"{databaseId}_{userId ?? "SYSTEM"}_{key}";
            lock (_cacheLock)
            {
                _valueCache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Gets all default values for a database (optionally filtered by user)
        /// </summary>
        public async Task<Dictionary<string, string>> GetDefaultsForDatabaseAsync(string databaseId, string? userId = null)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DATABASE_ID", Operator = "=", FilterValue = databaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(userId))
            {
                filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId });
            }
            else
            {
                filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "IS", FilterValue = "NULL" });
            }

            var results = await _repository.GetAsync(filters);
            var defaults = results.Cast<PPDM_DEFAULT_VALUE>()
                .ToDictionary(dv => dv.DEFAULT_KEY, dv => dv.DEFAULT_VALUE ?? string.Empty);

            // Update cache
            lock (_cacheLock)
            {
                foreach (var kvp in defaults)
                {
                    var cacheKey = $"{databaseId}_{userId ?? "SYSTEM"}_{kvp.Key}";
                    _valueCache[cacheKey] = kvp.Value;
                }
            }

            return defaults;
        }

        /// <summary>
        /// Initializes system defaults for a database
        /// </summary>
        public async Task InitializeDefaultsForDatabaseAsync(string databaseId, string userId = "SYSTEM")
        {
            await _defaults.InitializeSystemDefaultsAsync(databaseId, userId);
            
            // Clear cache for this database
            lock (_cacheLock)
            {
                var keysToRemove = _valueCache.Keys.Where(k => k.StartsWith($"{databaseId}_", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var key in keysToRemove)
                {
                    _valueCache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Resets user overrides to system defaults
        /// </summary>
        public async Task ResetToSystemDefaultsAsync(string databaseId, string userId)
        {
            // Get all user-specific defaults
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DATABASE_ID", Operator = "=", FilterValue = databaseId },
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var userDefaults = await _repository.GetAsync(filters);
            var entities = userDefaults.Cast<PPDM_DEFAULT_VALUE>().ToList();

            // Soft delete user overrides
            foreach (var entity in entities)
            {
                entity.ACTIVE_IND = "N";
                if (entity is IPPDMEntity ppdmEntity)
                    _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
                await _repository.UpdateAsync(entity, userId);
            }

            // Clear cache for this user
            lock (_cacheLock)
            {
                var keysToRemove = _valueCache.Keys.Where(k => k.StartsWith($"{databaseId}_{userId}_", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var key in keysToRemove)
                {
                    _valueCache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _valueCache.Clear();
            }
        }

        /// <summary>
        /// Clears cache for a specific database
        /// </summary>
        public void ClearCacheForDatabase(string databaseId)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _valueCache.Keys.Where(k => k.StartsWith($"{databaseId}_", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var key in keysToRemove)
                {
                    _valueCache.Remove(key);
                }
            }
        }
    }
}
