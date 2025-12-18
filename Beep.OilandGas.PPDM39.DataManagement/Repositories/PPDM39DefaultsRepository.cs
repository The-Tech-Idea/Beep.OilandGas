using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private IUnitOfWorkWrapper _wellStatusXrefUnitOfWork;
        private readonly object _unitOfWorkLock = new object();
        private const string WELL_STATUS_XREF_TABLE = "R_WELL_STATUS_XREF";

        public PPDM39DefaultsRepository(IDMEEditor editor = null, string connectionName = "PPDM39", PPDM39.Core.Metadata.IPPDMMetadataRepository metadata = null)
        {
            _editor = editor;
            _connectionName = connectionName;
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

        // Default Well Status Types (STATUS_TYPE values from PPDM 3.9 Well Facets)
        // These are the standard facets that should be initialized for every well/wellbore
        // Based on PPDM 3.9 Well Facets documentation
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES = new List<string>
        {
            "Business Interest",           // Well/Wellbore - Only 1 value (mutually exclusive, ranked)
            "Business Life Cycle Phase",   // Well - Changes predictably, may reoccur
            "Business Intention",          // Well - Set at Drilling start, doesn't change unless reverts to Planning
            "Operatorship",                // Well - May change if accountability transfers
            "Outcome",                     // Well - Doesn't change unless Business Life Cycle reverts to Planning
            "Lahee Class",                 // Well - Doesn't change over life cycle
            "Role",                        // Wellbore - May change over life cycle
            "Play Type",                   // Well - May change if Role changes or different formation completed
            "Well Structure",               // Well - May change as new wellbores added
            "Trajectory Type",             // Wellbore - Doesn't change over life cycle
            "Fluid Direction",             // Wellhead Stream - Can change
            "Well Reporting Class",        // Well - Can change over life cycle
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion - Can change
            "Wellbore Status",              // Wellbore - Slowly changing, measures milestones
            "Well Status"                   // Well - Summary state, may change infrequently
        };

        // STATUS_TYPEs specific to Well level (not Wellbore)
        // Based on PPDM 3.9 documentation: Well Component = Well
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELL = new List<string>
        {
            "Business Interest",           // Well or Wellbore (but typically Well)
            "Business Life Cycle Phase",   // Well
            "Business Intention",          // Well
            "Operatorship",                // Well
            "Outcome",                     // Well
            "Lahee Class",                 // Well
            "Play Type",                   // Well
            "Well Structure",               // Well
            "Well Reporting Class",        // Well
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion
            "Well Status"                   // Well
        };

        // STATUS_TYPEs specific to Wellbore level
        // Based on PPDM 3.9 documentation: Well Component = Wellbore
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLBORE = new List<string>
        {
            "Business Interest",           // Well or Wellbore (can be at Wellbore level)
            "Role",                        // Wellbore
            "Trajectory Type",             // Wellbore
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion
            "Wellbore Status"              // Wellbore
        };

        // STATUS_TYPEs specific to Wellhead Stream level
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLHEAD_STREAM = new List<string>
        {
            "Fluid Direction"              // Wellhead Stream
        };

        public string GetActiveIndicatorYes() => ACTIVE_IND_YES;
        public string GetActiveIndicatorNo() => ACTIVE_IND_NO;
        public string GetDefaultRowQuality() => DEFAULT_ROW_QUALITY;
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

        // Well Status Facets Methods

        /// <summary>
        /// Gets the UnitOfWork for R_WELL_STATUS_XREF table
        /// </summary>
        private IUnitOfWorkWrapper GetWellStatusXrefUnitOfWork()
        {
            if (_editor == null)
                throw new InvalidOperationException("IDMEEditor is required for well status facet queries. Please inject IDMEEditor in constructor.");

            if (_wellStatusXrefUnitOfWork == null)
            {
                lock (_unitOfWorkLock)
                {
                    if (_wellStatusXrefUnitOfWork == null)
                    {
                        _wellStatusXrefUnitOfWork = UnitOfWorkFactory.CreateUnitOfWork(
                            typeof(object), // Dynamic type for cross-reference table
                            _editor,
                            _connectionName,
                            WELL_STATUS_XREF_TABLE,
                            "STATUS_XREF_ID");
                    }
                }
            }

            return _wellStatusXrefUnitOfWork;
        }

        public string GetWellStatusXrefTableName() => WELL_STATUS_XREF_TABLE;

        public async Task<Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
                throw new ArgumentException("Status ID cannot be null or empty", nameof(statusId));

            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = WELL_STATUS_XREF_TABLE;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_XREF_ID", FilterValue = statusId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
            };

            var queryResult = await uow.Get(filters);
            var statusXrefs = ConvertToDynamicList(queryResult);

            // Extract facet information from the cross-reference
            var facets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var xref in statusXrefs)
            {
                // Extract facet values from the cross-reference record
                if (xref is System.Dynamic.ExpandoObject expando)
                {
                    var dict = (IDictionary<string, object>)expando;
                    foreach (var kvp in dict)
                    {
                        if (!facets.ContainsKey(kvp.Key) && 
                            !string.Equals(kvp.Key, "STATUS_XREF_ID", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(kvp.Key, "ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                        {
                            facets[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return facets;
        }

        public async Task<Dictionary<string, object>> GetWellStatusFacetsByTypeAndStatusAsync(string statusType, string status)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));

            // First, we need to find the STATUS_XREF_ID for this STATUS_TYPE and STATUS
            // This would typically come from R_WELL_STATUS table or similar
            // For now, we'll use a pattern: STATUS_TYPE + "_" + STATUS
            var statusId = $"{statusType}_{status}";
            return await GetWellStatusFacetsAsync(statusId);
        }

        public async Task<Dictionary<string, Dictionary<string, object>>> GetAllWellStatusFacetsAsync()
        {
            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = WELL_STATUS_XREF_TABLE;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
            };

            var queryResult = await uow.Get(filters);
            var statusXrefs = ConvertToDynamicList(queryResult);

            // Group facets by STATUS_XREF_ID
            var allFacets = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            foreach (var xref in statusXrefs)
            {
                if (xref is System.Dynamic.ExpandoObject expando)
                {
                    var dict = (IDictionary<string, object>)expando;
                    var statusId = dict.ContainsKey("STATUS_XREF_ID") ? dict["STATUS_XREF_ID"]?.ToString() : null;

                    if (string.IsNullOrWhiteSpace(statusId))
                        continue;

                    if (!allFacets.ContainsKey(statusId))
                    {
                        allFacets[statusId] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    }

                    foreach (var kvp in dict)
                    {
                        if (!string.Equals(kvp.Key, "STATUS_XREF_ID", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(kvp.Key, "ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!allFacets[statusId].ContainsKey(kvp.Key))
                            {
                                allFacets[statusId][kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
            }

            return allFacets;
        }

        private List<dynamic> ConvertToDynamicList(dynamic result)
        {
            var list = new List<dynamic>();
            if (result == null) return list;

            if (result is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    list.Add(item);
                }
            }
            else
            {
                list.Add(result);
            }

            return list;
        }

        // Well Status TYPE Methods (default values from R_WELL_STATUS)

        public async Task<List<string>> GetAllWellStatusTypesAsync()
        {
            if (_editor == null)
                throw new InvalidOperationException("IDMEEditor is required for well status type queries. Please inject IDMEEditor in constructor.");

            // Get all distinct STATUS_TYPE values from R_WELL_STATUS
            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = "R_WELL_STATUS";

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
            };

            var queryResult = await uow.Get(filters);
            var statusRefs = ConvertToDynamicList(queryResult);

            var statusTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var statusRef in statusRefs)
            {
                if (statusRef is System.Dynamic.ExpandoObject expando)
                {
                    var dict = (IDictionary<string, object>)expando;
                    if (dict.ContainsKey("STATUS_TYPE") && dict["STATUS_TYPE"] != null)
                    {
                        var statusType = dict["STATUS_TYPE"].ToString();
                        if (!string.IsNullOrWhiteSpace(statusType))
                        {
                            statusTypes.Add(statusType);
                        }
                    }
                }
            }

            return statusTypes.OrderBy(t => t).ToList();
        }

        public async Task<Dictionary<string, List<string>>> GetWellStatusTypesGroupedAsync()
        {
            // Group status types by common categories
            // This is a logical grouping - can be customized based on business needs
            var allTypes = await GetAllWellStatusTypesAsync();
            var grouped = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Business/Operational Status Types
            var businessTypes = new List<string>();
            var operationalTypes = new List<string>();
            var technicalTypes = new List<string>();

            foreach (var statusType in allTypes)
            {
                var upperType = statusType.ToUpper();
                if (upperType.Contains("BUSINESS") || upperType.Contains("LIFE") || upperType.Contains("INTENTION") || 
                    upperType.Contains("OUTCOME") || upperType.Contains("INTEREST") || upperType.Contains("OPERATORSHIP"))
                {
                    businessTypes.Add(statusType);
                }
                else if (upperType.Contains("STATUS") || upperType.Contains("ROLE") || upperType.Contains("TRAJECTORY") ||
                         upperType.Contains("FLUID") || upperType.Contains("WELLBORE"))
                {
                    operationalTypes.Add(statusType);
                }
                else if (upperType.Contains("CLASS") || upperType.Contains("TYPE") || upperType.Contains("STRUCTURE"))
                {
                    technicalTypes.Add(statusType);
                }
                else
                {
                    // Default to operational
                    operationalTypes.Add(statusType);
                }
            }

            if (businessTypes.Any())
                grouped["Business"] = businessTypes.OrderBy(t => t).ToList();
            if (operationalTypes.Any())
                grouped["Operational"] = operationalTypes.OrderBy(t => t).ToList();
            if (technicalTypes.Any())
                grouped["Technical"] = technicalTypes.OrderBy(t => t).ToList();

            return grouped;
        }

        /// <summary>
        /// Gets the list of default STATUS_TYPEs that should be initialized for every new well or wellbore
        /// These are the standard well facets from PPDM 3.9
        /// </summary>
        public List<string> GetDefaultWellStatusTypes()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES);
        }

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wells (not wellbores)
        /// </summary>
        public List<string> GetDefaultWellStatusTypesForWell()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES_FOR_WELL);
        }

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellbores
        /// </summary>
        public List<string> GetDefaultWellStatusTypesForWellbore()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES_FOR_WELLBORE);
        }

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellhead streams
        /// </summary>
        public List<string> GetDefaultWellStatusTypesForWellheadStream()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES_FOR_WELLHEAD_STREAM);
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
    }
}

