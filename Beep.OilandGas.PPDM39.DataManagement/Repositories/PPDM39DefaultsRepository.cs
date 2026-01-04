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
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories
{
    /// <summary>
    /// Implementation of PPDM39 defaults repository
    /// Provides system-wide default values for PPDM39 entities
    /// Includes Well Structure definitions based on PPDM 3.9 Well Structure Model
    /// Includes Well Status Facet retrieval for decomposing complex status values
    /// </summary>
    public class PPDM39DefaultsRepository : IPPDM39DefaultsRepository
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDMMetadataRepository _metadata;
        private PPDMGenericRepository _defaultValueRepository;

        public PPDM39DefaultsRepository(
            IDMEEditor editor = null, 
            string connectionName = "PPDM39", 
            IPPDMMetadataRepository metadata = null,
            ICommonColumnHandler commonColumnHandler = null)
        {
            _editor = editor;
            _connectionName = connectionName;
            _metadata = metadata;
            _commonColumnHandler = commonColumnHandler;
            
            // Initialize repository for PPDM_DEFAULT_VALUE if editor is available
            if (_editor != null && _commonColumnHandler != null && _metadata != null)
            {
                _defaultValueRepository = new PPDMGenericRepository(
                    _editor, 
                    _commonColumnHandler, 
                    this, 
                    _metadata,
                    typeof(PPDM_DEFAULT_VALUE), 
                    _connectionName, 
                    "PPDM_DEFAULT_VALUE");
            }
        }
        // Standard PPDM39 default values
        private const string ACTIVE_IND_YES = "Y";
        private const string ACTIVE_IND_NO = "N";
        private const string DEFAULT_ROW_QUALITY = "GOOD";
        private const string DEFAULT_PREFERRED_IND = "N";
        private const string DEFAULT_CERTIFIED_IND = "N";
        private const string DEFAULT_HIERARCHY_TYPE = "PARENT_CHILD";
        private const string DEFAULT_STRAT_COLUMN_TYPE = "STANDARD";
        private const string DEFAULT_STRAT_TYPE = "LITHOSTRATIGRAPHIC";
        private const string DEFAULT_STRAT_UNIT_TYPE = "FORMATION";
        private const string DEFAULT_STRAT_STATUS = "VALID";
        private const string DEFAULT_AREA_TYPE = "FIELD";
        private const string DEFAULT_DEPTH_OUOM = "M";
        private const string DEFAULT_PICK_DEPTH_OUOM = "M";
        private const string DEFAULT_AZIMUTH_NORTH_TYPE = "TRUE_NORTH";
        private const string DEFAULT_CONFORMITY_RELATIONSHIP = "CONFORMABLE";
        private const string DEFAULT_STRAT_INTERPRET_METHOD = "LOG_INTERPRETATION";
        private const string DEFAULT_PICK_QUALITY = "GOOD";
        private const string DEFAULT_PICK_VERSION_TYPE = "CURRENT";
        private const string DEFAULT_TVD_METHOD = "STANDARD";
        private const string DEFAULT_SOURCE = "SYSTEM";
        private const string DEFAULT_REMARK = "";

        // Well Structure XREF_TYPE values (used in WELL_XREF table)
        private const string WELL_ORIGIN_XREF_TYPE = "WELL_ORIGIN";
        private const string WELLBORE_XREF_TYPE = "WELLBORE";
        private const string WELLBORE_SEGMENT_XREF_TYPE = "WELLBORE_SEGMENT";
        private const string WELLBORE_CONTACT_INTERVAL_XREF_TYPE = "WELLBORE_CONTACT_INTERVAL";
        private const string WELLBORE_COMPLETION_XREF_TYPE = "WELLBORE_COMPLETION";
        private const string WELLHEAD_STREAM_XREF_TYPE = "WELLHEAD_STREAM";


        public string GetActiveIndicatorYes()
        {
            // Try to get from database if available, otherwise use hardcoded constant
            if (_defaultValueRepository != null && !string.IsNullOrEmpty(_connectionName))
            {
                try
                {
                    var dbValue = GetDefaultValueAsync("ACTIVE_IND_YES", _connectionName).GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(dbValue))
                        return dbValue;
                }
                catch
                {
                    // Fall through to hardcoded value
                }
            }
            return ACTIVE_IND_YES;
        }

        public string GetActiveIndicatorNo()
        {
            if (_defaultValueRepository != null && !string.IsNullOrEmpty(_connectionName))
            {
                try
                {
                    var dbValue = GetDefaultValueAsync("ACTIVE_IND_NO", _connectionName).GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(dbValue))
                        return dbValue;
                }
                catch
                {
                    // Fall through to hardcoded value
                }
            }
            return ACTIVE_IND_NO;
        }

        public string GetDefaultRowQuality()
        {
            if (_defaultValueRepository != null && !string.IsNullOrEmpty(_connectionName))
            {
                try
                {
                    var dbValue = GetDefaultValueAsync("DEFAULT_ROW_QUALITY", _connectionName).GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(dbValue))
                        return dbValue;
                }
                catch
                {
                    // Fall through to hardcoded value
                }
            }
            return DEFAULT_ROW_QUALITY;
        }
        public string GetDefaultPreferredIndicator() => DEFAULT_PREFERRED_IND;
        public string GetDefaultCertifiedIndicator() => DEFAULT_CERTIFIED_IND;
        public string GetDefaultHierarchyType() => DEFAULT_HIERARCHY_TYPE;
        public string GetDefaultStratColumnType() => DEFAULT_STRAT_COLUMN_TYPE;
        public string GetDefaultStratType() => DEFAULT_STRAT_TYPE;
        public string GetDefaultStratUnitType() => DEFAULT_STRAT_UNIT_TYPE;
        public string GetDefaultStratStatus() => DEFAULT_STRAT_STATUS;
        public string GetDefaultAreaType() => DEFAULT_AREA_TYPE;
        public string GetDefaultDepthOuom() => DEFAULT_DEPTH_OUOM;
        public string GetDefaultPickDepthOuom() => DEFAULT_PICK_DEPTH_OUOM;
        public string GetDefaultAzimuthNorthType() => DEFAULT_AZIMUTH_NORTH_TYPE;
        public string GetDefaultConformityRelationship() => DEFAULT_CONFORMITY_RELATIONSHIP;
        public string GetDefaultStratInterpretMethod() => DEFAULT_STRAT_INTERPRET_METHOD;
        public string GetDefaultPickQuality() => DEFAULT_PICK_QUALITY;
        public string GetDefaultPickVersionType() => DEFAULT_PICK_VERSION_TYPE;
        public string GetDefaultTvdMethod() => DEFAULT_TVD_METHOD;
        public string GetDefaultSource() => DEFAULT_SOURCE;
        public string GetDefaultRemark() => DEFAULT_REMARK;

        // Well Structure XREF_TYPE Methods (default values only)

        public string GetWellOriginXrefType() => WELL_ORIGIN_XREF_TYPE;
        public string GetWellboreXrefType() => WELLBORE_XREF_TYPE;
        public string GetWellboreSegmentXrefType() => WELLBORE_SEGMENT_XREF_TYPE;
        public string GetWellboreContactIntervalXrefType() => WELLBORE_CONTACT_INTERVAL_XREF_TYPE;
        public string GetWellboreCompletionXrefType() => WELLBORE_COMPLETION_XREF_TYPE;
        public string GetWellheadStreamXrefType() => WELLHEAD_STREAM_XREF_TYPE;

        public Dictionary<string, string> GetAllWellStructureXrefTypes()
        {
            return new Dictionary<string, string>
            {
                { "Well Origin", WELL_ORIGIN_XREF_TYPE },
                { "Wellbore", WELLBORE_XREF_TYPE },
                { "Wellbore Segment", WELLBORE_SEGMENT_XREF_TYPE },
                { "Wellbore Contact Interval", WELLBORE_CONTACT_INTERVAL_XREF_TYPE },
                { "Wellbore Completion", WELLBORE_COMPLETION_XREF_TYPE },
                { "Wellhead Stream", WELLHEAD_STREAM_XREF_TYPE }
            };
        }


        // ID Type Configuration for PPDM Tables

        /// <summary>
        /// Gets whether PPDM tables use string IDs
        /// All PPDM tables use string IDs by default
        /// </summary>
        public bool UseStringIds()
        {
            return true; // All PPDM tables use string IDs
        }

        /// <summary>
        /// Gets the ID type for a specific table
        /// Defaults to string for all PPDM tables
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>ID type name (always "String" for PPDM tables)</returns>
        public string GetIdTypeForTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            // All PPDM tables use string IDs
            return "String";
        }

        /// <summary>
        /// Formats an ID value according to the table's ID type configuration
        /// For PPDM tables, this always converts to string
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="id">ID value</param>
        /// <returns>Formatted ID value as string</returns>
        public string FormatIdForTable(string tableName, object id)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            // All PPDM tables use string IDs, so convert to string
            return id?.ToString() ?? string.Empty;
        }

        #region Database-Backed Default Values

        /// <summary>
        /// Gets a default value from database (user-specific if userId provided, otherwise system default)
        /// Falls back to hardcoded constant if not found in database
        /// </summary>
        public async Task<string?> GetDefaultValueAsync(string key, string databaseId, string? userId = null)
        {
            if (_defaultValueRepository == null)
                return null; // Database not available, will use hardcoded fallback

            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "DEFAULT_KEY", Operator = "=", FilterValue = key },
                    new AppFilter { FieldName = "DATABASE_ID", Operator = "=", FilterValue = databaseId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = GetActiveIndicatorYes() }
                };

                // Try user-specific first if userId provided
                if (!string.IsNullOrEmpty(userId))
                {
                    filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId });
                    var userResults = await _defaultValueRepository.GetAsync(filters);
                    var userDefault = userResults.Cast<PPDM_DEFAULT_VALUE>().FirstOrDefault();
                    if (userDefault != null)
                        return userDefault.DEFAULT_VALUE;
                }

                // Try system default (USER_ID IS NULL)
                filters.RemoveAll(f => f.FieldName == "USER_ID");
                filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "IS", FilterValue = "NULL" });
                var systemResults = await _defaultValueRepository.GetAsync(filters);
                var systemDefault = systemResults.Cast<PPDM_DEFAULT_VALUE>().FirstOrDefault();
                return systemDefault?.DEFAULT_VALUE;
            }
            catch
            {
                // If database query fails, return null to use hardcoded fallback
                return null;
            }
        }

        /// <summary>
        /// Sets or updates a default value in the database
        /// </summary>
        public async Task SetDefaultValueAsync(string key, string value, string databaseId, string? userId = null, string category = "System", string valueType = "String", string description = null)
        {
            if (_defaultValueRepository == null)
                throw new InvalidOperationException("Database repository not initialized. IDMEEditor, ICommonColumnHandler, and IPPDMMetadataRepository are required.");

            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "DEFAULT_KEY", Operator = "=", FilterValue = key },
                    new AppFilter { FieldName = "DATABASE_ID", Operator = "=", FilterValue = databaseId }
                };

                if (!string.IsNullOrEmpty(userId))
                {
                    filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId });
                }
                else
                {
                    filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "IS", FilterValue = "NULL" });
                }

                var existing = await _defaultValueRepository.GetAsync(filters);
                var existingEntity = existing.Cast<PPDM_DEFAULT_VALUE>().FirstOrDefault();

                if (existingEntity != null)
                {
                    // Update existing
                    existingEntity.DEFAULT_VALUE = value;
                    existingEntity.DEFAULT_CATEGORY = category;
                    existingEntity.VALUE_TYPE = valueType;
                    if (!string.IsNullOrEmpty(description))
                        existingEntity.DESCRIPTION = description;

                    if (existingEntity is IPPDMEntity entity)
                        _commonColumnHandler.PrepareForUpdate(entity, userId ?? "SYSTEM");

                    await _defaultValueRepository.UpdateAsync(existingEntity, userId ?? "SYSTEM");
                }
                else
                {
                    // Insert new
                    var newEntity = new PPDM_DEFAULT_VALUE
                    {
                        DEFAULT_VALUE_ID = Guid.NewGuid().ToString(),
                        DEFAULT_KEY = key,
                        DEFAULT_VALUE = value,
                        DEFAULT_CATEGORY = category,
                        VALUE_TYPE = valueType,
                        USER_ID = userId,
                        DATABASE_ID = databaseId,
                        DESCRIPTION = description,
                        ACTIVE_IND = GetActiveIndicatorYes()
                    };

                    if (newEntity is IPPDMEntity entity)
                        _commonColumnHandler.PrepareForInsert(entity, userId ?? "SYSTEM");

                    await _defaultValueRepository.InsertAsync(newEntity, userId ?? "SYSTEM");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set default value for key '{key}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all default values for a category
        /// </summary>
        public async Task<Dictionary<string, string>> GetDefaultsByCategoryAsync(string category, string databaseId, string? userId = null)
        {
            if (_defaultValueRepository == null)
                return new Dictionary<string, string>();

            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "DEFAULT_CATEGORY", Operator = "=", FilterValue = category },
                    new AppFilter { FieldName = "DATABASE_ID", Operator = "=", FilterValue = databaseId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = GetActiveIndicatorYes() }
                };

                if (!string.IsNullOrEmpty(userId))
                {
                    filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId });
                }
                else
                {
                    filters.Add(new AppFilter { FieldName = "USER_ID", Operator = "IS", FilterValue = "NULL" });
                }

                var results = await _defaultValueRepository.GetAsync(filters);
                return results.Cast<PPDM_DEFAULT_VALUE>()
                    .ToDictionary(dv => dv.DEFAULT_KEY, dv => dv.DEFAULT_VALUE ?? string.Empty);
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Initializes system defaults for a database
        /// Seeds all hardcoded constants into the database
        /// </summary>
        public async Task InitializeSystemDefaultsAsync(string databaseId, string userId = "SYSTEM")
        {
            if (_defaultValueRepository == null)
                throw new InvalidOperationException("Database repository not initialized. IDMEEditor, ICommonColumnHandler, and IPPDMMetadataRepository are required.");

            try
            {
                // System defaults
                await SetDefaultValueAsync("ACTIVE_IND_YES", ACTIVE_IND_YES, databaseId, null, "System", "String", "Default active indicator value");
                await SetDefaultValueAsync("ACTIVE_IND_NO", ACTIVE_IND_NO, databaseId, null, "System", "String", "Default inactive indicator value");
                await SetDefaultValueAsync("DEFAULT_ROW_QUALITY", DEFAULT_ROW_QUALITY, databaseId, null, "System", "String", "Default row quality indicator");
                await SetDefaultValueAsync("DEFAULT_PREFERRED_IND", DEFAULT_PREFERRED_IND, databaseId, null, "System", "String", "Default preferred indicator");
                await SetDefaultValueAsync("DEFAULT_CERTIFIED_IND", DEFAULT_CERTIFIED_IND, databaseId, null, "System", "String", "Default certified indicator");
                await SetDefaultValueAsync("DEFAULT_HIERARCHY_TYPE", DEFAULT_HIERARCHY_TYPE, databaseId, null, "System", "String", "Default hierarchy type");
                await SetDefaultValueAsync("DEFAULT_STRAT_COLUMN_TYPE", DEFAULT_STRAT_COLUMN_TYPE, databaseId, null, "System", "String", "Default stratigraphic column type");
                await SetDefaultValueAsync("DEFAULT_STRAT_TYPE", DEFAULT_STRAT_TYPE, databaseId, null, "System", "String", "Default stratigraphic type");
                await SetDefaultValueAsync("DEFAULT_STRAT_UNIT_TYPE", DEFAULT_STRAT_UNIT_TYPE, databaseId, null, "System", "String", "Default stratigraphic unit type");
                await SetDefaultValueAsync("DEFAULT_STRAT_STATUS", DEFAULT_STRAT_STATUS, databaseId, null, "System", "String", "Default stratigraphic status");
                await SetDefaultValueAsync("DEFAULT_AREA_TYPE", DEFAULT_AREA_TYPE, databaseId, null, "System", "String", "Default area type");
                await SetDefaultValueAsync("DEFAULT_DEPTH_OUOM", DEFAULT_DEPTH_OUOM, databaseId, null, "System", "String", "Default depth unit of measure");
                await SetDefaultValueAsync("DEFAULT_PICK_DEPTH_OUOM", DEFAULT_PICK_DEPTH_OUOM, databaseId, null, "System", "String", "Default pick depth unit of measure");
                await SetDefaultValueAsync("DEFAULT_AZIMUTH_NORTH_TYPE", DEFAULT_AZIMUTH_NORTH_TYPE, databaseId, null, "System", "String", "Default azimuth north type");
                await SetDefaultValueAsync("DEFAULT_CONFORMITY_RELATIONSHIP", DEFAULT_CONFORMITY_RELATIONSHIP, databaseId, null, "System", "String", "Default conformity relationship");
                await SetDefaultValueAsync("DEFAULT_STRAT_INTERPRET_METHOD", DEFAULT_STRAT_INTERPRET_METHOD, databaseId, null, "System", "String", "Default stratigraphic interpretation method");
                await SetDefaultValueAsync("DEFAULT_PICK_QUALITY", DEFAULT_PICK_QUALITY, databaseId, null, "System", "String", "Default pick quality");
                await SetDefaultValueAsync("DEFAULT_PICK_VERSION_TYPE", DEFAULT_PICK_VERSION_TYPE, databaseId, null, "System", "String", "Default pick version type");
                await SetDefaultValueAsync("DEFAULT_TVD_METHOD", DEFAULT_TVD_METHOD, databaseId, null, "System", "String", "Default TVD method");
                await SetDefaultValueAsync("DEFAULT_SOURCE", DEFAULT_SOURCE, databaseId, null, "System", "String", "Default source");
                await SetDefaultValueAsync("DEFAULT_REMARK", DEFAULT_REMARK, databaseId, null, "System", "String", "Default remark");

                // Well Structure XREF types
                await SetDefaultValueAsync("WELL_ORIGIN_XREF_TYPE", WELL_ORIGIN_XREF_TYPE, databaseId, null, "WellStructure", "String", "Well origin cross-reference type");
                await SetDefaultValueAsync("WELLBORE_XREF_TYPE", WELLBORE_XREF_TYPE, databaseId, null, "WellStructure", "String", "Wellbore cross-reference type");
                await SetDefaultValueAsync("WELLBORE_SEGMENT_XREF_TYPE", WELLBORE_SEGMENT_XREF_TYPE, databaseId, null, "WellStructure", "String", "Wellbore segment cross-reference type");
                await SetDefaultValueAsync("WELLBORE_CONTACT_INTERVAL_XREF_TYPE", WELLBORE_CONTACT_INTERVAL_XREF_TYPE, databaseId, null, "WellStructure", "String", "Wellbore contact interval cross-reference type");
                await SetDefaultValueAsync("WELLBORE_COMPLETION_XREF_TYPE", WELLBORE_COMPLETION_XREF_TYPE, databaseId, null, "WellStructure", "String", "Wellbore completion cross-reference type");
                await SetDefaultValueAsync("WELLHEAD_STREAM_XREF_TYPE", WELLHEAD_STREAM_XREF_TYPE, databaseId, null, "WellStructure", "String", "Wellhead stream cross-reference type");

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize system defaults for database '{databaseId}': {ex.Message}", ex);
            }
        }


        #endregion
    }
}

