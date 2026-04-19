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
        /// Looks up the R_WELL_STATUS row that matches a given STATUS_ID.
        /// STATUS_ID format: <c>"STATUS_TYPE,STATUS"</c> (composite key stored on WELL_STATUS).
        /// Falls back to an STATUS-only match for legacy records.
        /// </summary>
        private async Task<R_WELL_STATUS> GetWellStatusDescriptionByStatusIdAsync(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
                return null;

            var allStatusRefs = await GetAllWellStatusDescriptionsAsync();

            // Primary: composite key "STATUS_TYPE,STATUS"
            var match = allStatusRefs.FirstOrDefault(s =>
                string.Equals(s.STATUS_TYPE + "," + s.STATUS, statusId, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return match;

            // Fallback: STATUS-only match (legacy records)
            return allStatusRefs.FirstOrDefault(s =>
                string.Equals(s.STATUS, statusId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Fetches the first active R_WELL_STATUS entry for the given STATUS_TYPE.
        /// Used by <see cref="InitializeDefaultWellStatusesAsync"/> to seed initial facet values.
        /// </summary>
        private async Task<R_WELL_STATUS> GetDefaultStatusForTypeAsync(string statusType)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", FilterValue = statusType, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            var statuses = await _wellStatusReferenceRepository.GetEntitiesWithFiltersAsync(
                typeof(R_WELL_STATUS), "R_WELL_STATUS", filters);

            return statuses?.OfType<R_WELL_STATUS>().FirstOrDefault();
        }

        /// <summary>
        /// Builds the canonical STATUS_ID string used by WELL_STATUS.
        /// Format: <c>"STATUS_TYPE,STATUS"</c> (e.g., <c>"Business Life Cycle Phase,Production"</c>).
        /// </summary>
        internal static string FormatStatusId(string statusType, string status)
            => $"{statusType},{status}";

        /// <summary>
        /// Parses a STATUS_ID in canonical <c>"STATUS_TYPE,STATUS"</c> format into its components.
        /// Returns <c>(null, statusId)</c> if the string contains no comma (legacy format).
        /// </summary>
        internal static (string StatusType, string Status) ParseStatusId(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
                return (null, null);

            var idx = statusId.IndexOf(',');
            if (idx < 0)
                return (null, statusId);

            return (statusId[..idx], statusId[(idx + 1)..]);
        }

        /// <summary>
        /// Safe reflection helper: reads a string property from a dynamically-typed entity.
        /// Use only when the concrete type is not known at compile time.
        /// For known types (WELL, WELL_STATUS, etc.) use typed properties directly.
        /// </summary>
        internal static string GetStringValue(object entity, string fieldName)
        {
            if (entity == null || string.IsNullOrWhiteSpace(fieldName))
                return null;

            var prop = entity.GetType().GetProperty(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            return prop?.GetValue(entity)?.ToString();
        }

        #endregion
    }
}

