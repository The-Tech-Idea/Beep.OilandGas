using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL
{
    /// <summary>
    /// Partial class for helper methods and utilities
    /// Contains private helper methods used across the WellServices
    /// </summary>
    public partial class WellServices
    {
        #region Private Helper Methods

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

        #endregion
    }
}
