namespace Beep.OilandGas.PPDM39.Repositories
{
    /// <summary>
    /// Repository for PPDM39 system-wide default values and reference data
    /// Provides consistent default values across all PPDM39 repositories
    /// </summary>
    public interface IPPDM39DefaultsRepository
    {
        /// <summary>
        /// Gets the default value for ACTIVE_IND when creating new records
        /// </summary>
        string GetActiveIndicatorYes();

        /// <summary>
        /// Gets the value for ACTIVE_IND when soft deleting records
        /// </summary>
        string GetActiveIndicatorNo();

        /// <summary>
        /// Gets the default value for ROW_QUALITY
        /// </summary>
        string GetDefaultRowQuality();

        /// <summary>
        /// Gets the default value for PREFERRED_IND
        /// </summary>
        string GetDefaultPreferredIndicator();

        /// <summary>
        /// Gets the default value for CERTIFIED_IND
        /// </summary>
        string GetDefaultCertifiedIndicator();

        /// <summary>
        /// Gets the default value for HIERARCHY_TYPE
        /// </summary>
        string GetDefaultHierarchyType();

        /// <summary>
        /// Gets the default value for STRAT_COLUMN_TYPE
        /// </summary>
        string GetDefaultStratColumnType();

        /// <summary>
        /// Gets the default value for STRAT_TYPE
        /// </summary>
        string GetDefaultStratType();

        /// <summary>
        /// Gets the default value for STRAT_UNIT_TYPE
        /// </summary>
        string GetDefaultStratUnitType();

        /// <summary>
        /// Gets the default value for STRAT_STATUS
        /// </summary>
        string GetDefaultStratStatus();

        /// <summary>
        /// Gets the default value for AREA_TYPE
        /// </summary>
        string GetDefaultAreaType();

        /// <summary>
        /// Gets the default value for DEPTH_OUOM
        /// </summary>
        string GetDefaultDepthOuom();

        /// <summary>
        /// Gets the default value for PICK_DEPTH_OUOM
        /// </summary>
        string GetDefaultPickDepthOuom();

        /// <summary>
        /// Gets the default value for AZIMUTH_NORTH_TYPE
        /// </summary>
        string GetDefaultAzimuthNorthType();

        /// <summary>
        /// Gets the default value for CONFORMITY_RELATIONSHIP
        /// </summary>
        string GetDefaultConformityRelationship();

        /// <summary>
        /// Gets the default value for STRAT_INTERPRET_METHOD
        /// </summary>
        string GetDefaultStratInterpretMethod();

        /// <summary>
        /// Gets the default value for PICK_QUALITY
        /// </summary>
        string GetDefaultPickQuality();

        /// <summary>
        /// Gets the default value for PICK_VERSION_TYPE
        /// </summary>
        string GetDefaultPickVersionType();

        /// <summary>
        /// Gets the default value for TVD_METHOD
        /// </summary>
        string GetDefaultTvdMethod();

        /// <summary>
        /// Gets the default value for SOURCE
        /// </summary>
        string GetDefaultSource();

        /// <summary>
        /// Gets the default value for REMARK
        /// </summary>
        string GetDefaultRemark();

        // Well Structure XREF_TYPE values (default values for WELL_XREF table)

        /// <summary>
        /// Gets the XREF_TYPE value for Well Origin
        /// </summary>
        string GetWellOriginXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore
        /// </summary>
        string GetWellboreXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore Segment
        /// </summary>
        string GetWellboreSegmentXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore Contact Interval
        /// </summary>
        string GetWellboreContactIntervalXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore Completion
        /// </summary>
        string GetWellboreCompletionXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellhead Stream
        /// </summary>
        string GetWellheadStreamXrefType();

        /// <summary>
        /// Gets all well structure XREF_TYPE values
        /// </summary>
        System.Collections.Generic.Dictionary<string, string> GetAllWellStructureXrefTypes();

        // Well Status Facets (default values for well status decomposition)

        /// <summary>
        /// Gets well status facets for a given STATUS_ID
        /// Uses R_WELL_STATUS_XREF to decompose complex status values into atomic facets
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId);

        /// <summary>
        /// Gets well status facets for a given STATUS_TYPE and STATUS
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, object>> GetWellStatusFacetsByTypeAndStatusAsync(string statusType, string status);

        /// <summary>
        /// Gets all well status facets grouped by STATUS_ID
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>> GetAllWellStatusFacetsAsync();

        /// <summary>
        /// Gets the well status XREF table name
        /// </summary>
        string GetWellStatusXrefTableName();

        // Well Status TYPE defaults (from R_WELL_STATUS table)

        /// <summary>
        /// Gets all STATUS_TYPE values from R_WELL_STATUS table
        /// These are the default status types available for wells (e.g., Role, Business Life Cycle Phase, etc.)
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetAllWellStatusTypesAsync();

        /// <summary>
        /// Gets all STATUS_TYPE values grouped by their purpose/category
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>> GetWellStatusTypesGroupedAsync();

        /// <summary>
        /// Gets the list of default STATUS_TYPEs that should be initialized for every new well or wellbore
        /// These are the standard well facets from PPDM 3.9
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypes();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wells (not wellbores)
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypesForWell();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellbores
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypesForWellbore();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellhead streams
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypesForWellheadStream();

        // ID Type Configuration for PPDM Tables

        /// <summary>
        /// Gets whether PPDM tables use string IDs (all PPDM tables use string IDs by default)
        /// </summary>
        bool UseStringIds();

        /// <summary>
        /// Gets the ID type for a specific table (defaults to string for all PPDM tables)
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>ID type name (e.g., "String", "Int32", "Guid")</returns>
        string GetIdTypeForTable(string tableName);

        /// <summary>
        /// Formats an ID value according to the table's ID type configuration
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="id">ID value</param>
        /// <returns>Formatted ID value (as string for PPDM tables)</returns>
        string FormatIdForTable(string tableName, object id);
    }
}

