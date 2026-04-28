using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    public partial class ProductionAccountingService
    {
        /// <summary>
        /// Lists <c>AFE</c> rows with active-only filter (same as <see cref="AfeService"/> queries).
        /// When <paramref name="status"/> is set, use <see cref="AfeStatusCodes"/> values (seed <c>AFE_STATUS</c>).
        /// </summary>
        public async Task<List<AFE>> GetAfesAsync(
            string? afeId = null,
            string? propertyId = null,
            string? fieldId = null,
            string? status = null,
            string? connectionName = null)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            if (!string.IsNullOrWhiteSpace(afeId))
            {
                filters.Add(new AppFilter { FieldName = "AFE_ID", Operator = "=", FilterValue = afeId });
            }

            if (!string.IsNullOrWhiteSpace(propertyId))
            {
                filters.Add(new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId });
            }

            if (!string.IsNullOrWhiteSpace(fieldId))
            {
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filters.Add(new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = status });
            }

            var results = await GetRepository(typeof(AFE), connectionName ?? ConnectionName, "AFE").GetAsync(filters);
            return results?.OfType<AFE>().ToList() ?? new List<AFE>();
        }

        /// <summary>Line items for an AFE; active rows only (aligned with <see cref="AfeService"/> line-item queries).</summary>
        public async Task<List<AFE_LINE_ITEM>> GetAfeLineItemsAsync(string afeId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(afeId))
            {
                throw new ArgumentException("AFE ID is required.", nameof(afeId));
            }

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AFE_ID", Operator = "=", FilterValue = afeId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };

            var results = await GetRepository(typeof(AFE_LINE_ITEM), connectionName ?? ConnectionName, "AFE_LINE_ITEM").GetAsync(filters);
            return results?.OfType<AFE_LINE_ITEM>().ToList() ?? new List<AFE_LINE_ITEM>();
        }
    }
}