using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Core.DTOs
{
    /// <summary>
    /// Interface for Stratigraphy data management service
    /// </summary>
    public interface IPPDMStratigraphyService
    {
        /// <summary>
        /// Gets stratigraphic columns
        /// </summary>
        Task<List<STRAT_COLUMN>> GetStratigraphicColumnsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets stratigraphic column by ID
        /// </summary>
        Task<STRAT_COLUMN> GetStratigraphicColumnByIdAsync(string columnId);

        /// <summary>
        /// Gets stratigraphic units
        /// </summary>
        Task<List<STRAT_UNIT>> GetStratigraphicUnitsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets stratigraphic unit by ID
        /// </summary>
        Task<STRAT_UNIT> GetStratigraphicUnitByIdAsync(string unitId);

        /// <summary>
        /// Gets stratigraphic hierarchy
        /// </summary>
        Task<List<STRAT_HIERARCHY>> GetStratigraphicHierarchyAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets well sections
        /// </summary>
        Task<List<STRAT_WELL_SECTION>> GetWellSectionsAsync(List<AppFilter> filters = null);

        /// <summary>
        /// Gets well sections for a specific well
        /// </summary>
        Task<List<STRAT_WELL_SECTION>> GetWellSectionsByWellAsync(string uwi);

        /// <summary>
        /// Creates a new stratigraphic column
        /// </summary>
        Task<STRAT_COLUMN> CreateStratigraphicColumnAsync(STRAT_COLUMN column, string userId);

        /// <summary>
        /// Updates a stratigraphic column
        /// </summary>
        Task<STRAT_COLUMN> UpdateStratigraphicColumnAsync(STRAT_COLUMN column, string userId);

        /// <summary>
        /// Creates a new stratigraphic unit
        /// </summary>
        Task<STRAT_UNIT> CreateStratigraphicUnitAsync(STRAT_UNIT unit, string userId);

        /// <summary>
        /// Updates a stratigraphic unit
        /// </summary>
        Task<STRAT_UNIT> UpdateStratigraphicUnitAsync(STRAT_UNIT unit, string userId);
    }
}



