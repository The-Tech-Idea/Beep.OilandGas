using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for tracking entity versions and changes over time
    /// Provides versioning capabilities for audit and rollback
    /// </summary>
    public class PPDMDataVersioningService : IPPDMDataVersioningService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        // In-memory version storage (in production, use database table)
        private readonly Dictionary<string, List<VersionSnapshot>> _versionStore = new Dictionary<string, List<VersionSnapshot>>();
        private readonly object _versionLock = new object();

        public PPDMDataVersioningService(
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
        }

        /// <summary>
        /// Creates a version snapshot of an entity
        /// </summary>
        public async Task<VersionSnapshot> CreateVersionAsync(string tableName, object entity, string userId, string versionLabel = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Get entity ID
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                throw new InvalidOperationException($"Metadata not found for table: {tableName}");

            var entityType = entity.GetType();
            var pkColumn = metadata.PrimaryKeyColumn.Split(',').FirstOrDefault()?.Trim();
            var pkProperty = entityType.GetProperty(pkColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pkProperty == null)
                throw new InvalidOperationException($"Primary key property '{pkColumn}' not found");

            var entityId = pkProperty.GetValue(entity);

            // Create deep copy of entity
            var entityCopy = CreateDeepCopy(entity);

            // Get existing versions to determine next version number
            var versions = await GetVersionsAsync(tableName, entityId);
            var nextVersion = versions.Count > 0 ? versions.Max(v => v.VersionNumber) + 1 : 1;

            var snapshot = new VersionSnapshot
            {
                VersionNumber = nextVersion,
                TableName = tableName,
                EntityId = entityId,
                EntityData = entityCopy,
                VersionLabel = versionLabel ?? $"Version {nextVersion}",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId,
                ChangeDescription = $"Version {nextVersion} created"
            };

            // Store version
            var key = $"{tableName}_{entityId}";
            lock (_versionLock)
            {
                if (!_versionStore.ContainsKey(key))
                {
                    _versionStore[key] = new List<VersionSnapshot>();
                }
                _versionStore[key].Add(snapshot);
            }

            return snapshot;
        }

        /// <summary>
        /// Gets all versions of an entity
        /// </summary>
        public async Task<List<VersionSnapshot>> GetVersionsAsync(string tableName, object entityId)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            if (entityId == null)
                throw new ArgumentNullException(nameof(entityId));

            var key = $"{tableName}_{entityId}";
            lock (_versionLock)
            {
                if (_versionStore.ContainsKey(key))
                {
                    return Task.FromResult(_versionStore[key].OrderByDescending(v => v.VersionNumber).ToList()).Result;
                }
                return Task.FromResult(new List<VersionSnapshot>()).Result;
            }
        }

        /// <summary>
        /// Gets a specific version of an entity
        /// </summary>
        public async Task<VersionSnapshot> GetVersionAsync(string tableName, object entityId, int versionNumber)
        {
            var versions = await GetVersionsAsync(tableName, entityId);
            return versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        }

        /// <summary>
        /// Compares two versions of an entity
        /// </summary>
        public async Task<VersionComparison> CompareVersionsAsync(string tableName, object entityId, int version1, int version2)
        {
            var v1 = await GetVersionAsync(tableName, entityId, version1);
            var v2 = await GetVersionAsync(tableName, entityId, version2);

            if (v1 == null || v2 == null)
                throw new InvalidOperationException("One or both versions not found");

            var differences = new List<FieldDifference>();
            var entityType = v1.EntityData.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var value1 = property.GetValue(v1.EntityData);
                var value2 = property.GetValue(v2.EntityData);

                if (!Equals(value1, value2))
                {
                    differences.Add(new FieldDifference
                    {
                        FieldName = property.Name,
                        Source1Value = value1,
                        Source2Value = value2,
                        DifferenceType = "Different"
                    });
                }
            }

            return new VersionComparison
            {
                TableName = tableName,
                EntityId = entityId,
                Version1 = version1,
                Version2 = version2,
                Differences = differences,
                HasDifferences = differences.Count > 0
            };
        }

        /// <summary>
        /// Restores an entity to a specific version
        /// </summary>
        public async Task<RestoreVersionResult> RestoreToVersionAsync(string tableName, object entityId, int versionNumber, string userId)
        {
            var version = await GetVersionAsync(tableName, entityId, versionNumber);
            if (version == null)
            {
                return new RestoreVersionResult
                {
                    Success = false,
                    Message = $"Version {versionNumber} not found"
                };
            }

            // Create repository for the table
            var entityType = version.EntityData.GetType();
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName);

            // Restore entity
            var restoredEntity = CreateDeepCopy(version.EntityData);
            await repo.UpdateAsync(restoredEntity, userId);

            // Create new version after restore
            await CreateVersionAsync(tableName, restoredEntity, userId, $"Restored to version {versionNumber}");

            return new RestoreVersionResult
            {
                Success = true,
                RestoredEntity = restoredEntity,
                RestoredVersionNumber = versionNumber,
                Message = $"Successfully restored to version {versionNumber}"
            };
        }

        /// <summary>
        /// Creates a deep copy of an entity using reflection
        /// </summary>
        private object CreateDeepCopy(object source)
        {
            if (source == null)
                return null;

            var type = source.GetType();
            var copy = Activator.CreateInstance(type);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanWrite && property.CanRead)
                {
                    var value = property.GetValue(source);
                    
                    // For simple types, just copy the value
                    if (value == null || IsSimpleType(property.PropertyType))
                    {
                        property.SetValue(copy, value);
                    }
                    else
                    {
                        // For complex types, create a shallow copy (can be enhanced for deep copy)
                        property.SetValue(copy, value);
                    }
                }
            }

            return copy;
        }

        /// <summary>
        /// Checks if a type is a simple type
        /// </summary>
        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(DateTime) ||
                   type == typeof(decimal) ||
                   type == typeof(Guid) ||
                   (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}

