using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Repositories
{
    /// <summary>
    /// Service for managing Well Status facets and types
    /// Handles decomposition of complex status values into atomic facets
    /// </summary>
    public interface IWellStatusService
    {
        /// <summary>
        /// Gets well status facets for a given STATUS_ID
        /// Uses R_WELL_STATUS_XREF to decompose complex status values into atomic facets
        /// </summary>
        Task<Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId);

        /// <summary>
        /// Gets well status facets for a given STATUS_TYPE and STATUS
        /// </summary>
        Task<Dictionary<string, object>> GetWellStatusFacetsByTypeAndStatusAsync(string statusType, string status);

        /// <summary>
        /// Gets all well status facets grouped by STATUS_ID
        /// </summary>
        Task<Dictionary<string, Dictionary<string, object>>> GetAllWellStatusFacetsAsync();

        /// <summary>
        /// Gets the well status XREF table name
        /// </summary>
        string GetWellStatusXrefTableName();

        /// <summary>
        /// Gets all STATUS_TYPE values from R_WELL_STATUS table
        /// These are the default status types available for wells (e.g., Role, Business Life Cycle Phase, etc.)
        /// </summary>
        Task<List<string>> GetAllWellStatusTypesAsync();

        /// <summary>
        /// Gets all STATUS_TYPE values grouped by their purpose/category
        /// </summary>
        Task<Dictionary<string, List<string>>> GetWellStatusTypesGroupedAsync();

        /// <summary>
        /// Gets the list of default STATUS_TYPEs that should be initialized for every new well or wellbore
        /// These are the standard well facets from PPDM 3.9
        /// </summary>
        List<string> GetDefaultWellStatusTypes();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wells (not wellbores)
        /// </summary>
        List<string> GetDefaultWellStatusTypesForWell();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellbores
        /// </summary>
        List<string> GetDefaultWellStatusTypesForWellbore();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellhead streams
        /// </summary>
        List<string> GetDefaultWellStatusTypesForWellheadStream();
    }
}
