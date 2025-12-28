using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using WELL = Beep.OilandGas.PPDM39.Models.WELL;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Repository for Well entities and related business logic
    /// Handles well structures (using WELL_XREF), well status, and child entities
    /// Uses PPDMGenericRepository directly for all table operations
    /// Uses AppFilter for all queries (no SQL)
    /// </summary>
    public class WellRepository
    {
        protected readonly PPDMGenericRepository _wellRepository;
        protected readonly PPDMGenericRepository _wellXrefRepository;
        protected readonly PPDMGenericRepository _wellStatusRepository;
        protected readonly PPDMGenericRepository _wellStatusReferenceRepository;
        protected readonly IPPDM39DefaultsRepository _defaults;
        protected readonly IPPDMMetadataRepository _metadata;

        public WellRepository(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

            // Create generic repositories for each table
            _wellRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(Beep.OilandGas.PPDM39.Models.WELL), connectionName, "WELL");

            _wellXrefRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(WELL_XREF), connectionName, "WELL_XREF");

            _wellStatusRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(WELL_STATUS), connectionName, "WELL_STATUS");

            _wellStatusReferenceRepository = new PPDMGenericRepository(
                editor, commonColumnHandler, defaults, metadata,
                typeof(R_WELL_STATUS), connectionName, "R_WELL_STATUS");
        }

        /// <summary>
        /// Gets well by UWI (Unique Well Identifier)
        /// </summary>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> GetByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellRepository.GetAsync(filters);
            return result.Cast<Beep.OilandGas.PPDM39.Models.WELL>().FirstOrDefault();
        }

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
        /// Gets all well status records for a given UWI (all status types, all dates)
        /// </summary>
        public async Task<List<WELL_STATUS>> GetWellStatusByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellStatusRepository.GetAsync(filters);
            return result.Cast<WELL_STATUS>().OrderByDescending(s => s.EFFECTIVE_DATE).ToList();
        }

        /// <summary>
        /// Gets current well status for a given UWI (most recent status for each STATUS_TYPE)
        /// Answers: "What are my wells doing today?"
        /// Returns one status per STATUS_TYPE, using the most recent EFFECTIVE_DATE
        /// </summary>
        public async Task<Dictionary<string, WELL_STATUS>> GetCurrentWellStatusByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            // Get all status records for this well
            var allStatuses = await GetWellStatusByUwiAsync(uwi);

            // Group by STATUS_TYPE and get the most recent one for each type
            var currentStatuses = new Dictionary<string, WELL_STATUS>(StringComparer.OrdinalIgnoreCase);

            // Get STATUS_TYPE from STATUS_ID by looking up in R_WELL_STATUS
            // For now, we'll need to get the status description to find STATUS_TYPE
            // This is a simplified approach - in practice, you might want to denormalize STATUS_TYPE into WELL_STATUS
            
            // Group by STATUS_ID first, then get most recent
            var groupedByStatusId = allStatuses
                .GroupBy(s => s.STATUS_ID)
                .ToList();

            foreach (var group in groupedByStatusId)
            {
                // Get the most recent status for this STATUS_ID
                var mostRecent = group.OrderByDescending(s => s.EFFECTIVE_DATE).First();

                // Get STATUS_TYPE from R_WELL_STATUS
                // Note: This requires a lookup - you might want to cache this or denormalize STATUS_TYPE
                var statusDesc = await GetWellStatusDescriptionByStatusIdAsync(mostRecent.STATUS_ID);
                if (statusDesc != null && !string.IsNullOrWhiteSpace(statusDesc.STATUS_TYPE))
                {
                    var statusType = statusDesc.STATUS_TYPE;
                    
                    // Only keep if this is the most recent for this STATUS_TYPE
                    if (!currentStatuses.ContainsKey(statusType) || 
                        currentStatuses[statusType].EFFECTIVE_DATE < mostRecent.EFFECTIVE_DATE)
                    {
                        currentStatuses[statusType] = mostRecent;
                    }
                }
            }

            return currentStatuses;
        }

        /// <summary>
        /// Gets current well status for a specific STATUS_TYPE (e.g., "ROLE")
        /// Returns the most recent status record for the given STATUS_TYPE
        /// </summary>
        public async Task<WELL_STATUS> GetCurrentWellStatusByTypeAsync(string uwi, string statusType)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));

            // Get all status descriptions for this STATUS_TYPE
            var statusDescriptions = await GetWellStatusDescriptionsByTypeAsync(statusType);
            
            // STATUS_ID in WELL_STATUS might be "STATUS_TYPE,STATUS" or just "STATUS"
            // Create a set of possible STATUS_ID values to match
            var possibleStatusIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var desc in statusDescriptions)
            {
                // Try composite key format
                possibleStatusIds.Add($"{desc.STATUS_TYPE},{desc.STATUS}");
                // Try just STATUS
                possibleStatusIds.Add(desc.STATUS);
            }

            // Get all well status records for this UWI
            var allStatuses = await GetWellStatusByUwiAsync(uwi);
            
            // Filter by STATUS_IDs that match this STATUS_TYPE
            var matchingStatuses = allStatuses
                .Where(s => !string.IsNullOrWhiteSpace(s.STATUS_ID) && 
                           possibleStatusIds.Contains(s.STATUS_ID))
                .OrderByDescending(s => s.EFFECTIVE_DATE)
                .ToList();

            return matchingStatuses.FirstOrDefault();
        }

        /// <summary>
        /// Gets well status history for a specific STATUS_TYPE
        /// Returns all status records for the given STATUS_TYPE, ordered by date
        /// Example: Get all "ROLE" status changes over time for a well
        /// </summary>
        public async Task<List<WELL_STATUS>> GetWellStatusHistoryByTypeAsync(string uwi, string statusType)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));

            // Get all status descriptions for this STATUS_TYPE
            var statusDescriptions = await GetWellStatusDescriptionsByTypeAsync(statusType);
            
            // STATUS_ID in WELL_STATUS might be "STATUS_TYPE,STATUS" or just "STATUS"
            // Create a set of possible STATUS_ID values to match
            var possibleStatusIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var desc in statusDescriptions)
            {
                // Try composite key format
                possibleStatusIds.Add($"{desc.STATUS_TYPE},{desc.STATUS}");
                // Try just STATUS
                possibleStatusIds.Add(desc.STATUS);
            }

            // Get all well status records for this UWI
            var allStatuses = await GetWellStatusByUwiAsync(uwi);
            
            // Filter by STATUS_IDs that match this STATUS_TYPE and order by date
            return allStatuses
                .Where(s => !string.IsNullOrWhiteSpace(s.STATUS_ID) && 
                           possibleStatusIds.Contains(s.STATUS_ID))
                .OrderByDescending(s => s.EFFECTIVE_DATE)
                .ToList();
        }

        /// <summary>
        /// Gets well status at a specific date
        /// Returns the status that was effective on the given date for each STATUS_TYPE
        /// </summary>
        public async Task<Dictionary<string, WELL_STATUS>> GetWellStatusAtDateAsync(string uwi, DateTime asOfDate)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            // Get all status records for this well
            var allStatuses = await GetWellStatusByUwiAsync(uwi);

            // Filter to statuses that were effective on or before the asOfDate
            // and not expired before the asOfDate
            var effectiveStatuses = allStatuses
                .Where(s => s.EFFECTIVE_DATE <= asOfDate && 
                           (s.EXPIRY_DATE == DateTime.MinValue || s.EXPIRY_DATE >= asOfDate))
                .OrderByDescending(s => s.EFFECTIVE_DATE)
                .ToList();

            // Group by STATUS_TYPE and get the most recent one for each type at that date
            var statusesAtDate = new Dictionary<string, WELL_STATUS>(StringComparer.OrdinalIgnoreCase);

            // Similar to GetCurrentWellStatusByUwiAsync, but filter by date
            var groupedByStatusId = effectiveStatuses
                .GroupBy(s => s.STATUS_ID)
                .ToList();

            foreach (var group in groupedByStatusId)
            {
                var mostRecent = group.First(); // Already ordered by EFFECTIVE_DATE desc

                var statusDesc = await GetWellStatusDescriptionByStatusIdAsync(mostRecent.STATUS_ID);
                if (statusDesc != null && !string.IsNullOrWhiteSpace(statusDesc.STATUS_TYPE))
                {
                    var statusType = statusDesc.STATUS_TYPE;
                    
                    if (!statusesAtDate.ContainsKey(statusType) || 
                        statusesAtDate[statusType].EFFECTIVE_DATE < mostRecent.EFFECTIVE_DATE)
                    {
                        statusesAtDate[statusType] = mostRecent;
                    }
                }
            }

            return statusesAtDate;
        }

        /// <summary>
        /// Gets well status description by STATUS_ID
        /// Helper method to lookup STATUS_TYPE from STATUS_ID
        /// STATUS_ID format might be "STATUS_TYPE,STATUS" (composite key) or just "STATUS"
        /// </summary>
        private async Task<R_WELL_STATUS> GetWellStatusDescriptionByStatusIdAsync(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
                return null;

            // Get all status references
            var allStatusRefs = await GetAllWellStatusDescriptionsAsync();
            
            // Try to find by STATUS_ID pattern
            // First try exact match with composite key format
            var match = allStatusRefs.FirstOrDefault(s => 
                (s.STATUS_TYPE + "," + s.STATUS).Equals(statusId, StringComparison.OrdinalIgnoreCase));
            
            if (match != null)
                return match;
            
            // If not found, try matching just STATUS
            return allStatusRefs.FirstOrDefault(s => 
                s.STATUS.Equals(statusId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets well status description for a given STATUS_TYPE and STATUS
        /// </summary>
        public async Task<R_WELL_STATUS> GetWellStatusDescriptionAsync(string statusType, string status)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "STATUS", FilterValue = status, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellStatusReferenceRepository.GetAsync(filters);
            return result.Cast<R_WELL_STATUS>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all well status descriptions for a given STATUS_TYPE
        /// </summary>
        public async Task<List<R_WELL_STATUS>> GetWellStatusDescriptionsByTypeAsync(string statusType)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellStatusReferenceRepository.GetAsync(filters);
            return result.Cast<R_WELL_STATUS>().ToList();
        }

        /// <summary>
        /// Gets all well status descriptions
        /// </summary>
        public async Task<List<R_WELL_STATUS>> GetAllWellStatusDescriptionsAsync()
        {
            var result = await _wellStatusReferenceRepository.GetActiveAsync();
            return result.Cast<R_WELL_STATUS>().ToList();
        }

        /// <summary>
        /// Gets well status descriptions grouped by STATUS_TYPE
        /// </summary>
        public async Task<Dictionary<string, List<R_WELL_STATUS>>> GetWellStatusDescriptionsGroupedByTypeAsync()
        {
            var allStatuses = await GetAllWellStatusDescriptionsAsync();
            return allStatuses
                .GroupBy(s => s.STATUS_TYPE ?? "UNKNOWN")
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets well status descriptions by STATUS_GROUP
        /// </summary>
        public async Task<List<R_WELL_STATUS>> GetWellStatusDescriptionsByGroupAsync(string statusGroup)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_GROUP", FilterValue = statusGroup, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellStatusReferenceRepository.GetAsync(filters);
            return result.Cast<R_WELL_STATUS>().ToList();
        }

        /// <summary>
        /// Gets distinct STATUS_TYPE values from R_WELL_STATUS (default status types)
        /// These are the available status types like: Role, Business Life Cycle Phase, etc.
        /// </summary>
        public async Task<List<string>> GetWellStatusTypesAsync()
        {
            return await _defaults.GetAllWellStatusTypesAsync();
        }

        /// <summary>
        /// Gets STATUS_TYPE values grouped by category
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetWellStatusTypesGroupedAsync()
        {
            return await _defaults.GetWellStatusTypesGroupedAsync();
        }

        /// <summary>
        /// Gets distinct STATUS_GROUP values
        /// </summary>
        public async Task<List<string>> GetWellStatusGroupsAsync()
        {
            var allStatuses = await GetAllWellStatusDescriptionsAsync();
            return allStatuses
                .Where(s => !string.IsNullOrWhiteSpace(s.STATUS_GROUP))
                .Select(s => s.STATUS_GROUP)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        /// <summary>
        /// Gets well status facets for a given STATUS_ID
        /// Uses defaults repository to retrieve facets from R_WELL_STATUS_XREF
        /// </summary>
        public async Task<Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId)
        {
            return await _defaults.GetWellStatusFacetsAsync(statusId);
        }

        /// <summary>
        /// Gets well status facets for a given STATUS_TYPE and STATUS
        /// Uses defaults repository to retrieve facets
        /// </summary>
        public async Task<Dictionary<string, object>> GetWellStatusFacetsByTypeAndStatusAsync(string statusType, string status)
        {
            return await _defaults.GetWellStatusFacetsByTypeAndStatusAsync(statusType, status);
        }

        /// <summary>
        /// Gets all well status facets grouped by STATUS_ID
        /// Uses defaults repository to retrieve all facets
        /// </summary>
        public async Task<Dictionary<string, Dictionary<string, object>>> GetAllWellStatusFacetsAsync()
        {
            return await _defaults.GetAllWellStatusFacetsAsync();
        }

        /// <summary>
        /// Gets well status with facets for a given WELL_STATUS entry
        /// </summary>
        public async Task<(WELL_STATUS Status, Dictionary<string, object> Facets)> GetWellStatusWithFacetsAsync(WELL_STATUS wellStatus)
        {
            if (wellStatus == null)
                throw new ArgumentNullException(nameof(wellStatus));

            var facets = string.IsNullOrWhiteSpace(wellStatus.STATUS_ID)
                ? new Dictionary<string, object>()
                : await GetWellStatusFacetsAsync(wellStatus.STATUS_ID);

            return (wellStatus, facets);
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

        // Well CRUD Operations

        /// <summary>
        /// Creates a new well
        /// Optionally initializes default status types for the well
        /// </summary>
        /// <param name="well">Well entity to create</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="initializeDefaultStatuses">If true, creates default status records for all standard STATUS_TYPEs</param>
        /// <returns>Created well entity</returns>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> CreateWellAsync(
            Beep.OilandGas.PPDM39.Models.WELL well, 
            string userId,
            bool initializeDefaultStatuses = true)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellRepository.InsertAsync(well, userId);
            var createdWell = result as Beep.OilandGas.PPDM39.Models.WELL;

            // Initialize default status types for the well
            if (initializeDefaultStatuses && createdWell != null && !string.IsNullOrWhiteSpace(createdWell.UWI))
            {
                await InitializeDefaultWellStatusesAsync(createdWell.UWI, userId);
            }

            return createdWell;
        }

        /// <summary>
        /// Updates an existing well
        /// </summary>
        public async Task<Beep.OilandGas.PPDM39.Models.WELL> UpdateWellAsync(
            Beep.OilandGas.PPDM39.Models.WELL well, 
            string userId)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellRepository.UpdateAsync(well, userId);
            return result as Beep.OilandGas.PPDM39.Models.WELL;
        }

        /// <summary>
        /// Soft deletes a well (sets ACTIVE_IND = 'N')
        /// </summary>
        public async Task<bool> SoftDeleteWellAsync(string uwi, string userId)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            return await _wellRepository.SoftDeleteAsync(uwi, userId);
        }

        /// <summary>
        /// Hard deletes a well from the database
        /// </summary>
        public async Task<bool> DeleteWellAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            return await _wellRepository.DeleteAsync(uwi);
        }

        // Well Status CRUD Operations

        /// <summary>
        /// Creates a new well status entry for a specific STATUS_TYPE
        /// Well status consists of multiple records - one per STATUS_TYPE, each can change over time
        /// This method creates a new status record and automatically expires any previous active status of the same STATUS_TYPE
        /// STATUS_ID format: "STATUS_TYPE,STATUS" (e.g., "ROLE,Producer")
        /// </summary>
        /// <param name="uwi">Unique Well Identifier</param>
        /// <param name="statusType">STATUS_TYPE (e.g., "ROLE", "Business Life Cycle Phase", "Operatorship")</param>
        /// <param name="status">STATUS value (must exist in R_WELL_STATUS for this STATUS_TYPE)</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="effectiveDate">When this status becomes effective (defaults to current date)</param>
        /// <param name="expiryDate">When this status expires (null = no expiry, remains current)</param>
        /// <param name="source">Source of the status (defaults to system default)</param>
        /// <param name="percentCapability">Percent capability (defaults to 100)</param>
        /// <returns>Created WELL_STATUS record</returns>
        public async Task<WELL_STATUS> CreateWellStatusAsync(
            string uwi,
            string statusType,
            string status,
            string userId,
            DateTime? effectiveDate = null,
            DateTime? expiryDate = null,
            string source = null,
            decimal? percentCapability = null)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Verify the status exists in R_WELL_STATUS for this STATUS_TYPE
            var statusDesc = await GetWellStatusDescriptionAsync(statusType, status);
            if (statusDesc == null)
            {
                throw new InvalidOperationException($"Status '{status}' for type '{statusType}' not found in R_WELL_STATUS. Please ensure the status exists in the reference table.");
            }

            // Create STATUS_ID (format: STATUS_TYPE,STATUS)
            var statusId = $"{statusType},{status}";
            var newEffectiveDate = effectiveDate ?? DateTime.Now;
            var defaultSource = source ?? _defaults.GetDefaultSource();

            // Get all current active statuses for this STATUS_TYPE
            // A well can have multiple status records for the same STATUS_TYPE over time
            var currentStatuses = await GetWellStatusHistoryByTypeAsync(uwi, statusType);
            
            // Find any active status (no expiry or expiry in future) that overlaps with the new effective date
            var overlappingStatus = currentStatuses
                .Where(s => s.EXPIRY_DATE == DateTime.MinValue || s.EXPIRY_DATE >= newEffectiveDate)
                .OrderByDescending(s => s.EFFECTIVE_DATE)
                .FirstOrDefault();

            if (overlappingStatus != null)
            {
                // Check if it's the same status value
                if (overlappingStatus.STATUS_ID.Equals(statusId, StringComparison.OrdinalIgnoreCase))
                {
                    // Same status - just update the existing record if dates changed
                    if (effectiveDate.HasValue && overlappingStatus.EFFECTIVE_DATE != effectiveDate.Value)
                    {
                        overlappingStatus.EFFECTIVE_DATE = effectiveDate.Value;
                    }
                    if (expiryDate.HasValue)
                    {
                        overlappingStatus.EXPIRY_DATE = expiryDate.Value;
                        overlappingStatus.END_TIME = expiryDate.Value;
                    }
                    if (percentCapability.HasValue)
                        overlappingStatus.PERCENT_CAPABILITY = percentCapability.Value;
                    if (!string.IsNullOrWhiteSpace(source))
                        overlappingStatus.SOURCE = source;

                    return await UpdateWellStatusAsync(overlappingStatus, userId);
                }
                else
                {
                    // Different status value - expire the old one
                    // Set expiry to day before new effective date (or current date if new is today)
                    var expiryDateForOld = newEffectiveDate.Date > DateTime.Now.Date 
                        ? newEffectiveDate.AddDays(-1) 
                        : DateTime.Now.Date.AddDays(-1);
                    
                    overlappingStatus.EXPIRY_DATE = expiryDateForOld;
                    overlappingStatus.END_TIME = expiryDateForOld;
                    await UpdateWellStatusAsync(overlappingStatus, userId);
                }
            }

            // Create new well status record
            var wellStatus = new WELL_STATUS
            {
                UWI = uwi,
                SOURCE = defaultSource,
                STATUS_ID = statusId, // Format: "STATUS_TYPE,STATUS"
                EFFECTIVE_DATE = newEffectiveDate,
                EXPIRY_DATE = expiryDate ?? DateTime.MinValue, // DateTime.MinValue = no expiry (current status)
                END_TIME = expiryDate ?? DateTime.MinValue,
                PERCENT_CAPABILITY = percentCapability ?? 100,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.Now,
                ROW_CHANGED_BY = userId,
                ROW_CHANGED_DATE = DateTime.Now,
                ROW_QUALITY = _defaults.GetDefaultRowQuality()
            };

            var result = await _wellStatusRepository.InsertAsync(wellStatus, userId);
            return result as WELL_STATUS;
        }

        /// <summary>
        /// Updates an existing well status entry
        /// </summary>
        public async Task<WELL_STATUS> UpdateWellStatusAsync(
            WELL_STATUS wellStatus, 
            string userId)
        {
            if (wellStatus == null)
                throw new ArgumentNullException(nameof(wellStatus));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var result = await _wellStatusRepository.UpdateAsync(wellStatus, userId);
            return result as WELL_STATUS;
        }

        /// <summary>
        /// Updates the current status for a specific STATUS_TYPE
        /// This creates a new status record and expires the old one (temporal tracking)
        /// Well status is multiple records - one per STATUS_TYPE, each can change over time
        /// </summary>
        /// <param name="uwi">Unique Well Identifier</param>
        /// <param name="statusType">STATUS_TYPE to update (e.g., "ROLE")</param>
        /// <param name="newStatus">New STATUS value</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="effectiveDate">When the new status becomes effective (defaults to current date)</param>
        /// <param name="expiryDate">When the new status expires (null = no expiry)</param>
        /// <param name="source">Source of the status</param>
        /// <param name="percentCapability">Percent capability</param>
        /// <returns>New WELL_STATUS record created</returns>
        public async Task<WELL_STATUS> UpdateCurrentWellStatusAsync(
            string uwi,
            string statusType,
            string newStatus,
            string userId,
            DateTime? effectiveDate = null,
            DateTime? expiryDate = null,
            string source = null,
            decimal? percentCapability = null)
        {
            // This is essentially the same as CreateWellStatusAsync
            // It will automatically expire the old status and create a new one
            return await CreateWellStatusAsync(
                uwi, statusType, newStatus, userId, 
                effectiveDate, expiryDate, source, percentCapability);
        }

        /// <summary>
        /// Expires a well status (sets EXPIRY_DATE and END_TIME)
        /// Used when a status is no longer valid
        /// </summary>
        public async Task<WELL_STATUS> ExpireWellStatusAsync(
            string uwi,
            string statusType,
            DateTime expiryDate,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var currentStatus = await GetCurrentWellStatusByTypeAsync(uwi, statusType);
            if (currentStatus == null)
            {
                throw new InvalidOperationException($"No current status found for type '{statusType}' and UWI '{uwi}'");
            }

            currentStatus.EXPIRY_DATE = expiryDate;
            currentStatus.END_TIME = expiryDate;

            return await UpdateWellStatusAsync(currentStatus, userId);
        }

        /// <summary>
        /// Soft deletes a well status (sets ACTIVE_IND = 'N')
        /// </summary>
        public async Task<bool> SoftDeleteWellStatusAsync(
            string uwi, 
            string source, 
            string statusId, 
            string userId)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("SOURCE cannot be null or empty", nameof(source));
            if (string.IsNullOrWhiteSpace(statusId))
                throw new ArgumentException("Status ID cannot be null or empty", nameof(statusId));

            // Get the status by composite key
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" },
                new AppFilter { FieldName = "SOURCE", FilterValue = source, Operator = "=" },
                new AppFilter { FieldName = "STATUS_ID", FilterValue = statusId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var result = await _wellStatusRepository.GetAsync(filters);
            var status = result.Cast<WELL_STATUS>().FirstOrDefault();
            
            if (status == null)
                return false;

            return await _wellStatusRepository.SoftDeleteAsync($"{uwi},{source},{statusId}", userId);
        }

        /// <summary>
        /// Creates multiple well statuses in a batch (one per STATUS_TYPE)
        /// Useful for setting up initial statuses for a new well
        /// Each STATUS_TYPE gets its own record - well status is multiple records, not one
        /// </summary>
        /// <param name="uwi">Unique Well Identifier</param>
        /// <param name="statusesByType">Dictionary of STATUS_TYPE -> STATUS values to create</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="effectiveDate">Effective date for all statuses (defaults to current date)</param>
        /// <param name="source">Source for all statuses</param>
        /// <returns>List of created WELL_STATUS records (one per STATUS_TYPE)</returns>
        public async Task<List<WELL_STATUS>> CreateWellStatusesBatchAsync(
            string uwi,
            Dictionary<string, string> statusesByType, // STATUS_TYPE -> STATUS
            string userId,
            DateTime? effectiveDate = null,
            string source = null)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (statusesByType == null || statusesByType.Count == 0)
                throw new ArgumentException("Statuses cannot be null or empty", nameof(statusesByType));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var createdStatuses = new List<WELL_STATUS>();
            var commonEffectiveDate = effectiveDate ?? DateTime.Now;

            // Create one status record for each STATUS_TYPE
            // Each STATUS_TYPE is independent and can have its own temporal history
            foreach (var kvp in statusesByType)
            {
                var statusType = kvp.Key;
                var status = kvp.Value;

                // CreateWellStatusAsync will handle expiring any existing status of the same type
                var wellStatus = await CreateWellStatusAsync(
                    uwi, statusType, status, userId,
                    commonEffectiveDate, null, source);

                createdStatuses.Add(wellStatus);
            }

            return createdStatuses;
        }

        /// <summary>
        /// Gets well status with all related information (description, facets, etc.)
        /// </summary>
        public async Task<WellStatusInfo> GetWellStatusInfoAsync(WELL_STATUS wellStatus)
        {
            if (wellStatus == null)
                throw new ArgumentNullException(nameof(wellStatus));

            var statusDesc = await GetWellStatusDescriptionByStatusIdAsync(wellStatus.STATUS_ID);
            var facets = await GetWellStatusFacetsAsync(wellStatus.STATUS_ID);

            return new WellStatusInfo
            {
                WellStatus = wellStatus,
                StatusDescription = statusDesc,
                Facets = facets,
                StatusType = statusDesc?.STATUS_TYPE,
                StatusName = statusDesc?.LONG_NAME ?? statusDesc?.STATUS
            };
        }

        /// <summary>
        /// Initializes default status types for a well
        /// Creates status records for all standard STATUS_TYPEs from PPDM 3.9 Well Facets
        /// Each STATUS_TYPE will be initialized with the first available status value from R_WELL_STATUS
        /// Based on PPDM 3.9 documentation, these facets should be initialized for every new well
        /// </summary>
        /// <param name="uwi">Unique Well Identifier</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="effectiveDate">Effective date for all statuses (defaults to current date)</param>
        /// <returns>List of created WELL_STATUS records</returns>
        public async Task<List<WELL_STATUS>> InitializeDefaultWellStatusesAsync(
            string uwi,
            string userId,
            DateTime? effectiveDate = null)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var defaultStatusTypes = _defaults.GetDefaultWellStatusTypesForWell();
            var createdStatuses = new List<WELL_STATUS>();
            var commonEffectiveDate = effectiveDate ?? DateTime.Now;

            foreach (var statusType in defaultStatusTypes)
            {
                // Get the first/default status value for this STATUS_TYPE from R_WELL_STATUS
                var defaultStatus = await GetDefaultStatusForTypeAsync(statusType);
                
                if (defaultStatus != null)
                {
                    // Create status record with the default value
                    var wellStatus = await CreateWellStatusAsync(
                        uwi, 
                        statusType, 
                        defaultStatus.STATUS, 
                        userId,
                        commonEffectiveDate,
                        null, // No expiry - remains current
                        null, // Use default source
                        null); // Use default percent capability

                    createdStatuses.Add(wellStatus);
                }
            }

            return createdStatuses;
        }

        /// <summary>
        /// Gets the default status value for a given STATUS_TYPE from R_WELL_STATUS
        /// Returns the first active status found for the type, or null if none exists
        /// </summary>
        /// <param name="statusType">STATUS_TYPE to get default status for</param>
        /// <returns>R_WELL_STATUS entry with the default status, or null if not found</returns>
        private async Task<R_WELL_STATUS> GetDefaultStatusForTypeAsync(string statusType)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                return null;

            // Query R_WELL_STATUS for the first active status of this type
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var statuses = await _wellStatusReferenceRepository.GetEntitiesWithFiltersAsync(
                typeof(R_WELL_STATUS), "R_WELL_STATUS", filters);

            if (statuses != null && statuses.Any())
            {
                // Return the first one (could be ordered by preference if needed)
                var firstStatus = statuses.FirstOrDefault();
                if (firstStatus != null)
                {
                    return firstStatus as R_WELL_STATUS;
                }
            }

            return null;
        }

        /// <summary>
        /// Comprehensive information about a well status
        /// </summary>
        public class WellStatusInfo
        {
            public WELL_STATUS WellStatus { get; set; }
            public R_WELL_STATUS StatusDescription { get; set; }
            public Dictionary<string, object> Facets { get; set; }
            public string StatusType { get; set; }
            public string StatusName { get; set; }
        }
    }
}
