using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Partial class for Well Structure operations
    /// Handles WELL_XREF operations and child entity relationships
    /// </summary>
    public partial class WellServices
    {
        #region Well Structure Queries

        /// <summary>
        /// Gets well structures for a given UWI using WELL_XREF
        /// </summary>
        public async Task<Dictionary<string, List<WELL_XREF>>> GetWellStructuresByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var xrefList = await _wellXrefRepository.GetAsync(filters);
            var allXrefs = xrefList.Cast<WELL_XREF>().ToList();

            // Group by XREF_TYPE
            var result = new Dictionary<string, List<WELL_XREF>>(StringComparer.OrdinalIgnoreCase);
            
            // Get all XREF_TYPE values from defaults
            var wellOriginType = _defaults.GetWellOriginXrefType();
            var wellboreType = _defaults.GetWellboreXrefType();
            var wellboreSegmentType = _defaults.GetWellboreSegmentXrefType();
            var wellboreContactIntervalType = _defaults.GetWellboreContactIntervalXrefType();
            var wellboreCompletionType = _defaults.GetWellboreCompletionXrefType();
            var wellheadStreamType = _defaults.GetWellheadStreamXrefType();

            var allXrefTypes = new[] { wellOriginType, wellboreType, wellboreSegmentType, wellboreContactIntervalType, wellboreCompletionType, wellheadStreamType };

            foreach (var xrefType in allXrefTypes)
            {
                var structures = allXrefs.Where(x => x.XREF_TYPE == xrefType).ToList();
                if (structures.Any())
                {
                    result[xrefType] = structures;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets child entities linked to a well structure
        /// </summary>
        public async Task<List<T>> GetChildEntitiesByWellStructureAsync<T>(
            string uwi, 
            string xrefType, 
            string childTableName) where T : class
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(xrefType))
                throw new ArgumentException("XREF_TYPE cannot be null or empty", nameof(xrefType));
            if (string.IsNullOrWhiteSpace(childTableName))
                throw new ArgumentException("Child table name cannot be null or empty", nameof(childTableName));

            // Get the XREF_ID for this well structure
            var xrefFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "XREF_TYPE", FilterValue = xrefType, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var xrefs = await _wellXrefRepository.GetAsync(xrefFilters);
            var xrefIds = xrefs.Cast<WELL_XREF>().Select(x => x.XREF_ID).ToList();

            if (!xrefIds.Any())
                return new List<T>();

            // Get child entities - need to determine the foreign key column from metadata
            var childMetadata = await _metadata.GetTableMetadataAsync(childTableName);
            if (childMetadata == null)
                throw new InvalidOperationException($"Metadata not found for table: {childTableName}");

            // Find foreign key to WELL_XREF or UWI
            var fkToWell = childMetadata.ForeignKeys
                .FirstOrDefault(fk => fk.ReferencedTable.Equals("WELL_XREF", StringComparison.OrdinalIgnoreCase) ||
                                     fk.ReferencedTable.Equals("WELL", StringComparison.OrdinalIgnoreCase));

            if (fkToWell == null)
                throw new InvalidOperationException($"No foreign key relationship found from {childTableName} to WELL or WELL_XREF");

            // Build filters for child entities
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            // If referencing WELL_XREF, filter by XREF_ID; if referencing WELL, filter by UWI
            if (fkToWell.ReferencedTable.Equals("WELL_XREF", StringComparison.OrdinalIgnoreCase))
            {
                filters.Add(new AppFilter 
                { 
                    FieldName = fkToWell.ForeignKeyColumn, 
                    FilterValue = string.Join(",", xrefIds), 
                    Operator = "IN" 
                });
            }
            else
            {
                filters.Add(new AppFilter 
                { 
                    FieldName = fkToWell.ForeignKeyColumn, 
                    FilterValue = uwi, 
                    Operator = "=" 
                });
            }

            // Use well repository's UnitOfWork but set EntityName to child table
            var childUow = _wellRepository.UnitOfWork;
            childUow.EntityName = childTableName;
            var childResult = await childUow.Get(filters);
            
            var list = new List<T>();
            if (childResult is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is T typedItem)
                    {
                        list.Add(typedItem);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Gets all child tables/modules that can be linked to a well structure
        /// Uses metadata to determine relationships
        /// </summary>
        public async Task<List<string>> GetLinkedChildTablesAsync(string xrefType)
        {
            // Get metadata for tables that might reference WELL or WELL_XREF
            var allMetadata = PPDM39Metadata.GetMetadata();
            var linkedTables = new List<string>();

            foreach (var kvp in allMetadata)
            {
                var tableMeta = kvp.Value;
                var hasWellFk = tableMeta.ForeignKeys.Any(fk => 
                    fk.ReferencedTable.Equals("WELL", StringComparison.OrdinalIgnoreCase) ||
                    fk.ReferencedTable.Equals("WELL_XREF", StringComparison.OrdinalIgnoreCase));

                if (hasWellFk && !linkedTables.Contains(tableMeta.TableName, StringComparer.OrdinalIgnoreCase))
                {
                    linkedTables.Add(tableMeta.TableName);
                }
            }

            return linkedTables.OrderBy(t => t).ToList();
        }

        #endregion

        #region Well Structure CRUD Operations

        /// <summary>
        /// Creates a WELL_XREF entry for a well structure
        /// </summary>
        public async Task<WELL_XREF> CreateWellStructureAsync(
            string uwi, 
            string xrefType, 
            string xrefId, 
            string userId,
            string uwi2 = null,
            string remark = null)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(xrefType))
                throw new ArgumentException("XREF_TYPE cannot be null or empty", nameof(xrefType));
            if (string.IsNullOrWhiteSpace(xrefId))
                throw new ArgumentException("XREF_ID cannot be null or empty", nameof(xrefId));

            var wellXref = new WELL_XREF
            {
                UWI = uwi,
                XREF_TYPE = xrefType,
                XREF_ID = xrefId,
                UWI2 = uwi2,
                REMARK = remark ?? _defaults.GetDefaultRemark(),
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                SOURCE = _defaults.GetDefaultSource(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.Now,
                ROW_CHANGED_BY = userId,
                ROW_CHANGED_DATE = DateTime.Now,
                ROW_QUALITY = _defaults.GetDefaultRowQuality()
            };

            // Use well XREF repository to insert
            var result = await _wellXrefRepository.InsertAsync(wellXref, userId);
            return result as WELL_XREF;
        }

        #endregion
    }
}
