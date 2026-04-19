using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Partial class for Well Status operations
    /// Handles all well status queries, CRUD operations, facets, and reference data
    /// </summary>
    public partial class WellServices
    {
        #region Well Status Queries

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
            return result.Cast<WELL_STATUS>().OrderByDescending(s => s.EFFECTIVE_DATE ?? DateTime.MinValue).ToList();
        }

        /// <summary>
        /// Gets current well status for a given UWI (most recent status for each STATUS_TYPE).
        /// Answers: "What are my wells doing today?"
        /// Returns one <see cref="WELL_STATUS"/> per STATUS_TYPE, using the most recent EFFECTIVE_DATE.
        /// <para>
        /// PPDM 3.9: WELL_STATUS carries STATUS_TYPE directly, so no R_WELL_STATUS join is needed.
        /// The 15 standard facets are populated by <see cref="InitializeDefaultWellStatusesAsync"/>.
        /// </para>
        /// </summary>
        public async Task<Dictionary<string, WELL_STATUS>> GetCurrentWellStatusByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var allStatuses = await GetWellStatusByUwiAsync(uwi);

            // WELL_STATUS.STATUS_TYPE is a direct column — group by it and pick most recent per type.
            // No R_WELL_STATUS round-trips required.
            return allStatuses
                .Where(s => !string.IsNullOrWhiteSpace(s.STATUS_TYPE))
                .GroupBy(s => s.STATUS_TYPE, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(s => s.EFFECTIVE_DATE ?? DateTime.MinValue).First(),
                    StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets current well status for a specific STATUS_TYPE (e.g., <c>"Business Life Cycle Phase"</c>).
        /// Returns the most recent <see cref="WELL_STATUS"/> record for the given STATUS_TYPE.
        /// </summary>
        public async Task<WELL_STATUS> GetCurrentWellStatusByTypeAsync(string uwi, string statusType)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));

            // WELL_STATUS.STATUS_TYPE is a direct column — filter by it, no extra lookups needed.
            var allStatuses = await GetWellStatusByUwiAsync(uwi);
            return allStatuses
                .Where(s => string.Equals(s.STATUS_TYPE, statusType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.EFFECTIVE_DATE ?? DateTime.MinValue)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the full history for a specific STATUS_TYPE, most-recent-first.
        /// Example: Get all "Business Life Cycle Phase" changes over time for a well.
        /// </summary>
        public async Task<List<WELL_STATUS>> GetWellStatusHistoryByTypeAsync(string uwi, string statusType)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));

            // WELL_STATUS.STATUS_TYPE is a direct column — filter and sort in one pass.
            var allStatuses = await GetWellStatusByUwiAsync(uwi);
            return allStatuses
                .Where(s => string.Equals(s.STATUS_TYPE, statusType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.EFFECTIVE_DATE ?? DateTime.MinValue)
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

            // Filter to statuses effective on/before asOfDate and not yet expired.
            // EXPIRY_DATE == null means "currently active with no set end date".
            return allStatuses
                .Where(s => !string.IsNullOrWhiteSpace(s.STATUS_TYPE) &&
                            (s.EFFECTIVE_DATE == null || s.EFFECTIVE_DATE <= asOfDate) &&
                            (s.EXPIRY_DATE == null || s.EXPIRY_DATE >= asOfDate))
                .GroupBy(s => s.STATUS_TYPE, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(s => s.EFFECTIVE_DATE ?? DateTime.MinValue).First(),
                    StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Well Status Reference Data

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
            return await GetAllWellStatusTypesAsync();
        }

        /// <summary>
        /// Gets STATUS_TYPE values grouped by category
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetWellStatusTypesGroupedAsync()
        {
            return await GetWellStatusTypesGroupedInternalAsync();
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
        /// Gets all STATUS_TYPE values from R_WELL_STATUS table
        /// These are the default status types available for wells (e.g., Role, Business Life Cycle Phase, etc.)
        /// </summary>
        public async Task<List<string>> GetAllWellStatusTypesAsync()
        {
            // Get all distinct STATUS_TYPE values from R_WELL_STATUS
            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = "R_WELL_STATUS";

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
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

        /// <summary>
        /// Gets all STATUS_TYPE values grouped by their purpose/category
        /// </summary>
        private async Task<Dictionary<string, List<string>>> GetWellStatusTypesGroupedInternalAsync()
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

        #endregion

        #region Well Status Facets

        /// <summary>
        /// Gets well status facets for a given STATUS_ID
        /// Uses R_WELL_STATUS_XREF to decompose complex status values into atomic facets
        /// </summary>
        public async Task<Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
                throw new ArgumentException("Status ID cannot be null or empty", nameof(statusId));

            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = WELL_STATUS_XREF_TABLE;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_XREF_ID", FilterValue = statusId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
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

        /// <summary>
        /// Gets well status facets for a given STATUS_TYPE and STATUS
        /// </summary>
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

        /// <summary>
        /// Gets all well status facets grouped by STATUS_ID
        /// </summary>
        public async Task<Dictionary<string, Dictionary<string, object>>> GetAllWellStatusFacetsAsync()
        {
            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = WELL_STATUS_XREF_TABLE;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
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
        /// Gets the well status XREF table name
        /// </summary>
        public string GetWellStatusXrefTableName() => WELL_STATUS_XREF_TABLE;

        #endregion

        #region Well Status CRUD Operations

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
            
            // Find any active status (no expiry or expiry in future) that overlaps with the new effective date.
            // EXPIRY_DATE == null means "currently active with no set end date".
            var overlappingStatus = currentStatuses
                .Where(s => s.EXPIRY_DATE == null || s.EXPIRY_DATE >= newEffectiveDate)
                .OrderByDescending(s => s.EFFECTIVE_DATE ?? DateTime.MinValue)
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
                STATUS_ID = statusId,          // Format: "STATUS_TYPE,STATUS"
                STATUS_TYPE = statusType,       // Stored directly on WELL_STATUS for fast queries
                STATUS = status,               // Stored directly on WELL_STATUS for fast queries
                EFFECTIVE_DATE = newEffectiveDate,
                EXPIRY_DATE = expiryDate,      // null = currently active with no set end date
                END_TIME = expiryDate,
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

            var defaultStatusTypes = GetDefaultWellStatusTypesForWell();
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

        #endregion

        #region Convenience Facet Accessors

        // ── PPDM Well Status & Classification v3 convenience accessors ───────────────────────────────
        // Facet names match the R_WELL_STATUS.STATUS_TYPE values in the database.
        // Reference: PPDM Well Status & Classification v3 (R-3, June 2020).

        /// <summary>
        /// Returns the current <c>Life Cycle</c> phase (WSC v3).
        /// Values: Planning | Constructing | Operating | Closing | Closed.
        /// The Life Cycle of the well is derived from the current Life Cycle of each component.
        /// </summary>
        public Task<WELL_STATUS> GetWellLifecyclePhaseAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Life Cycle");

        /// <summary>
        /// Returns the current <c>Condition</c> facet (WSC v3).
        /// Values: Active | Inactive (Shut In, Idle, Abandoned).
        /// Condition is the operational state relative to the Role.
        /// </summary>
        public Task<WELL_STATUS> GetWellConditionAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Condition");

        /// <summary>
        /// Returns the current <c>Role</c> facet (WSC v3).
        /// Values: Produce | Inject | Produce/Inject | Service | Research | No Role.
        /// Role may change over the Life Cycle; if multiple Roles exist, use the highest significance.
        /// </summary>
        public Task<WELL_STATUS> GetWellRoleAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Role");

        /// <summary>
        /// Returns the current <c>Business Intention</c> facet (WSC v3).
        /// Values: Explore | Appraise | Extend | Develop.
        /// Set at drilling start; generally does not change unless Life Cycle reverts to Planning.
        /// </summary>
        public Task<WELL_STATUS> GetWellBusinessIntentionAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Business Intention");

        /// <summary>
        /// Returns the current <c>Business Interest</c> facet (WSC v3).
        /// Values: Yes (Financial-Operated / Financial-Non-operated / Obligatory / Technical) | No.
        /// Most important qualifier in the hierarchy should be stored. Can change over time.
        /// </summary>
        public Task<WELL_STATUS> GetWellBusinessInterestAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Business Interest");

        /// <summary>
        /// Returns the current <c>Product Type</c> facet (WSC v3, replaces v2 "Fluid Type").
        /// Values: Oil | Gas | Geothermal | Mineral | Non-hydrocarbon Gas | Steam | Water.
        /// Always interpret alongside Product Significance (Primary/Secondary/Tertiary/Show).
        /// </summary>
        public Task<WELL_STATUS> GetWellProductTypeAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Product Type");

        /// <summary>
        /// Returns the current <c>Product Significance</c> facet (WSC v3, new in v3).
        /// Values: Primary | Secondary | Tertiary | Show.
        /// Always paired with Product Type; significance may vary over the life of the well.
        /// </summary>
        public Task<WELL_STATUS> GetWellProductSignificanceAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Product Significance");

        /// <summary>
        /// Returns the current <c>Well Structure</c> facet (WSC v3).
        /// Values: Simple | Simplex | Compound | Complex | Network.
        /// May change as new wellbores are added to the well.
        /// </summary>
        public Task<WELL_STATUS> GetWellStructureFacetAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Well Structure");

        /// <summary>
        /// Returns the current <c>Profile Type</c> facet (WSC v3, replaces v2 "Trajectory Type").
        /// Values: Vertical | Inclined (Slant Hole, S-type, Deep Inclined) | Horizontal (Toe-up, Toe-down, Level, Undulating).
        /// Confirmed when Construction is finished; may change if drilling deviates from plan.
        /// </summary>
        public Task<WELL_STATUS> GetWellProfileTypeAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Profile Type");

        /// <summary>
        /// Returns the current <c>Fluid Direction</c> facet (WSC v3 — Wellhead Stream scope).
        /// Values: Inflow | Outflow | Static | Dual Flow.
        /// </summary>
        public Task<WELL_STATUS> GetWellFluidDirectionAsync(string uwi)
            => GetCurrentWellStatusByTypeAsync(uwi, "Fluid Direction");

        /// <summary>
        /// Returns true if the well currently has a status record for the given STATUS_TYPE.
        /// </summary>
        public async Task<bool> HasStatusTypeAsync(string uwi, string statusType)
        {
            var status = await GetCurrentWellStatusByTypeAsync(uwi, statusType);
            return status != null;
        }

        /// <summary>
        /// Builds a <see cref="WellSummaryDto"/> for the given UWI, including all current status facets.
        /// </summary>
        public async Task<WellSummaryDto> GetWellStatusSummaryAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI cannot be null or empty", nameof(uwi));

            var currentStatuses = await GetCurrentWellStatusByUwiAsync(uwi);
            var allStatusDescs = await GetAllWellStatusDescriptionsAsync();
            var descLookup = allStatusDescs.ToDictionary(
                d => $"{d.STATUS_TYPE}|{d.STATUS}",
                d => d,
                StringComparer.OrdinalIgnoreCase);

            var statusInfos = new Dictionary<string, WellStatusInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in currentStatuses)
            {
                var ws = kvp.Value;
                descLookup.TryGetValue($"{ws.STATUS_TYPE}|{ws.STATUS}", out var desc);

                statusInfos[kvp.Key] = new WellStatusInfo
                {
                    WellStatus = ws,
                    StatusDescription = desc,
                    StatusType = ws.STATUS_TYPE,
                    StatusName = desc?.LONG_NAME ?? desc?.STATUS ?? ws.STATUS,
                    StatusAbbreviation = desc?.ABBREVIATION,
                    StatusGroup = desc?.STATUS_GROUP,
                    EffectiveDate = ws.EFFECTIVE_DATE,
                    ExpiryDate = ws.EXPIRY_DATE
                };
            }

            return new WellSummaryDto
            {
                UWI = uwi,
                CurrentStatuses = statusInfos
            };
        }

        #endregion

        #region Facet Reference Data (R_WELL_STATUS / R_WELL_STATUS_QUAL / R_WELL_STATUS_QUAL_VALUE)

        // ── Methods that load validated reference data from the database and fall back to  ──────────
        // ── the embedded FACET_CATALOG when the R_* tables have not been seeded.           ──────────

        /// <summary>
        /// Returns all <see cref="FacetValueDto"/> records for the given STATUS_TYPE from
        /// <c>R_WELL_STATUS</c>.  Falls back to <see cref="FACET_CATALOG"/> when the table is empty.
        /// </summary>
        public async Task<List<FacetValueDto>> GetFacetValuesAsync(string statusType)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("STATUS_TYPE is required", nameof(statusType));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND",  FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var rows = await _wellStatusReferenceRepository.GetAsync(filters);
            var typed = rows.OfType<R_WELL_STATUS>().ToList();

            // If the database has data use it; otherwise fall back to the embedded catalog.
            if (typed.Any())
            {
                return typed.Select(r => new FacetValueDto
                {
                    StatusType   = r.STATUS_TYPE,
                    Status       = r.STATUS,
                    Abbreviation = r.ABBREVIATION,
                    LongName     = r.LONG_NAME
                }).ToList();
            }

            if (FACET_CATALOG.TryGetValue(statusType, out var def))
                return def.Values.Select(v => new FacetValueDto
                {
                    StatusType   = statusType,
                    Status       = v.Status,
                    Abbreviation = v.Abbreviation,
                    LongName     = v.LongName
                }).ToList();

            return new List<FacetValueDto>();
        }

        /// <summary>
        /// Returns all <see cref="FacetQualifierDto"/> records for the given STATUS_TYPE from
        /// <c>R_WELL_STATUS_QUAL</c>.  Falls back to <see cref="FACET_CATALOG"/> when the table is empty.
        /// Optionally filter to only those qualifiers applicable to a specific STATUS value.
        /// </summary>
        public async Task<List<FacetQualifierDto>> GetFacetQualifiersAsync(string statusType, string status = null)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("STATUS_TYPE is required", nameof(statusType));

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND",  FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var rows = await _wellStatusQualRepository.GetAsync(filters);
            var typed = rows.OfType<R_WELL_STATUS_QUAL>().ToList();

            if (typed.Any())
            {
                return typed.Select(r => new FacetQualifierDto
                {
                    StatusType = r.STATUS_TYPE,
                    Status     = null,   // R_WELL_STATUS_QUAL doesn't store a per-status scope in this model
                    Qualifier  = r.STATUS_QUALIFIER,
                    LongName   = r.LONG_NAME
                }).ToList();
            }

            if (FACET_CATALOG.TryGetValue(statusType, out var def))
            {
                // Collect qualifiers from the catalog: apply "*" (all) plus the specific status key
                var result = new List<FacetQualifierDto>();
                foreach (var key in new[] { "*", status })
                {
                    if (key != null && def.Qualifiers.TryGetValue(key, out var list))
                        result.AddRange(list.Select(q => new FacetQualifierDto
                        {
                            StatusType = statusType,
                            Status     = key == "*" ? null : key,
                            Qualifier  = q.Qualifier,
                            LongName   = q.LongName
                        }));
                }
                return result;
            }

            return new List<FacetQualifierDto>();
        }

        /// <summary>
        /// Returns all <see cref="FacetQualifierValueDto"/> records for a given
        /// STATUS_TYPE + STATUS + QUALIFIER combination from <c>R_WELL_STATUS_QUAL_VALUE</c>.
        /// Falls back to <see cref="FACET_CATALOG"/>.
        /// </summary>
        public async Task<List<FacetQualifierValueDto>> GetFacetQualifierValuesAsync(
            string statusType, string status, string qualifier)
        {
            if (string.IsNullOrWhiteSpace(statusType) || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(qualifier))
                throw new ArgumentException("statusType, status, and qualifier are all required");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE",     FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "STATUS",          FilterValue = status,     Operator = "=" },
                new AppFilter { FieldName = "STATUS_QUALIFIER",FilterValue = qualifier,  Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND",      FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var rows = await _wellStatusQualValueRepository.GetAsync(filters);
            var typed = rows.OfType<R_WELL_STATUS_QUAL_VALUE>().ToList();

            if (typed.Any())
            {
                return typed.Select(r => new FacetQualifierValueDto
                {
                    StatusType     = r.STATUS_TYPE,
                    Status         = r.STATUS,
                    Qualifier      = r.STATUS_QUALIFIER,
                    QualifierValue = r.STATUS_QUALIFIER_VALUE,
                    LongName       = r.LONG_NAME
                }).ToList();
            }

            // Catalog fallback
            if (FACET_CATALOG.TryGetValue(statusType, out var def))
            {
                var key = (status, qualifier);
                if (def.QualifierValues.TryGetValue(key, out var list))
                    return list.Select(v => new FacetQualifierValueDto
                    {
                        StatusType     = statusType,
                        Status         = status,
                        Qualifier      = qualifier,
                        QualifierValue = v.QualifierValue,
                        LongName       = v.LongName
                    }).ToList();
            }

            return new List<FacetQualifierValueDto>();
        }

        /// <summary>
        /// Returns a fully-populated <see cref="FacetTypeDto"/> for each of the 13 WSC v3 facets,
        /// enriched with the current WELL_STATUS value for the specified <paramref name="uwi"/>.
        /// This is the single call made by the classification page on load.
        /// </summary>
        public async Task<List<FacetTypeDto>> GetWellFacetPageDataAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi))
                throw new ArgumentException("UWI is required", nameof(uwi));

            // Load current status facets for the well (one per STATUS_TYPE, most recent wins).
            Dictionary<string, WELL_STATUS> currentStatus;
            try { currentStatus = await GetCurrentWellStatusByUwiAsync(uwi); }
            catch { currentStatus = new Dictionary<string, WELL_STATUS>(System.StringComparer.OrdinalIgnoreCase); }

            var result = new List<FacetTypeDto>();

            foreach (var facetType in DEFAULT_WELL_STATUS_TYPES)
            {
                if (!FACET_CATALOG.TryGetValue(facetType, out var catalogDef))
                    continue;

                // Reference values
                var values     = await GetFacetValuesAsync(facetType);
                var qualifiers = await GetFacetQualifiersAsync(facetType);

                // Build qualifier groups keyed by STATUS
                var qualifiersByStatus = new Dictionary<string, List<FacetQualifierDto>>(System.StringComparer.OrdinalIgnoreCase);
                foreach (var q in qualifiers)
                {
                    var key = q.Status ?? "*";
                    if (!qualifiersByStatus.ContainsKey(key))
                        qualifiersByStatus[key] = new List<FacetQualifierDto>();
                    qualifiersByStatus[key].Add(q);
                }

                // Current value for this well
                WellStatusInfo currentInfo = null;
                if (currentStatus.TryGetValue(facetType, out var ws))
                {
                    R_WELL_STATUS refRow = null;
                    if (!string.IsNullOrWhiteSpace(ws.STATUS))
                    {
                        var match = values.FirstOrDefault(v =>
                            string.Equals(v.Status, ws.STATUS, System.StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            refRow = new R_WELL_STATUS
                            {
                                STATUS_TYPE  = match.StatusType,
                                STATUS       = match.Status,
                                ABBREVIATION = match.Abbreviation,
                                LONG_NAME    = match.LongName
                            };
                        }
                    }
                    currentInfo = new WellStatusInfo
                    {
                        WellStatus         = ws,
                        StatusDescription  = refRow,
                        StatusType         = ws.STATUS_TYPE,
                        StatusName         = refRow?.LONG_NAME ?? ws.STATUS,
                        StatusAbbreviation = refRow?.ABBREVIATION,
                        EffectiveDate      = ws.EFFECTIVE_DATE,
                        ExpiryDate         = ws.EXPIRY_DATE
                    };
                }

                result.Add(new FacetTypeDto
                {
                    StatusType   = catalogDef.StatusType,
                    LongName     = catalogDef.LongName,
                    Scope        = catalogDef.Scope,
                    Values       = values,
                    Qualifiers   = qualifiersByStatus,
                    CurrentValue = currentInfo
                });
            }

            return result;
        }

        /// <summary>
        /// Inserts or updates the WELL_STATUS record for a single facet on the given well.
        /// <list type="bullet">
        ///   <item>If an active record already exists for (UWI, STATUS_TYPE) it is expired first.</item>
        ///   <item>A new WELL_STATUS row is then inserted with the supplied values.</item>
        /// </list>
        /// </summary>
        /// <param name="request">Facet assignment details. <see cref="SetFacetRequest"/> must supply UWI, StatusType, and Status.</param>
        /// <param name="userId">Audit user ID (ROW_CREATED_BY / ROW_CHANGED_BY).</param>
        /// <returns>The newly created WELL_STATUS row.</returns>
        public async Task<WELL_STATUS> SetFacetAsync(SetFacetRequest request, string userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.UWI))
                throw new ArgumentException("UWI is required", nameof(request));
            if (string.IsNullOrWhiteSpace(request.StatusType))
                throw new ArgumentException("StatusType is required", nameof(request));
            if (string.IsNullOrWhiteSpace(request.Status))
                throw new ArgumentException("Status is required", nameof(request));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var effectiveDate = request.EffectiveDate ?? System.DateTime.UtcNow.Date;

            // Expire any currently-active record for this STATUS_TYPE before inserting the new one.
            try
            {
                await ExpireWellStatusAsync(request.UWI, request.StatusType, effectiveDate.AddDays(-1), userId);
            }
            catch (InvalidOperationException)
            {
                // No current status to expire — this is the first assignment; continue.
            }

            // Build the new WELL_STATUS row with full column control (includes QUALIFIER and REMARK).
            var statusId = $"{request.StatusType},{request.Status}";
            var newRecord = new WELL_STATUS
            {
                UWI                    = request.UWI,
                SOURCE                 = request.Source ?? "USER",
                STATUS_ID              = statusId,
                STATUS_TYPE            = request.StatusType,
                STATUS                 = request.Status,
                STATUS_QUALIFIER       = request.StatusQualifier,
                STATUS_QUALIFIER_VALUE = request.StatusQualifierValue,
                EFFECTIVE_DATE         = effectiveDate,
                EXPIRY_DATE            = request.ExpiryDate,
                REMARK                 = request.Remark,
                PERCENT_CAPABILITY     = request.PercentCapability ?? 0,
                ACTIVE_IND             = _defaults.GetActiveIndicatorYes()
            };

            var inserted = await _wellStatusRepository.InsertAsync(newRecord, userId);
            return inserted as WELL_STATUS ?? newRecord;
        }

        #endregion

        #region Default Status Types

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

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Gets the UnitOfWork for R_WELL_STATUS_XREF table
        /// </summary>
        private IUnitOfWorkWrapper GetWellStatusXrefUnitOfWork()
        {
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

        /// <summary>
        /// Converts dynamic query result to list of dynamic objects
        /// </summary>
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

        #endregion
    }
}
