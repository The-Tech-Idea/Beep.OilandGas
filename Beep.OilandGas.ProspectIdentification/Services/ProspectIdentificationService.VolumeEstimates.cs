using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    public partial class ProspectIdentificationService
    {
        /// <summary>
        /// The volume estimates recorded for a prospect — its active PROSPECT_VOLUME_ESTIMATE rows — newest first (by
        /// estimate date; an undated estimate last).
        /// </summary>
        public async Task<List<PROSPECT_VOLUME_ESTIMATE>> GetVolumeEstimatesAsync(string prospectId)
        {
            if (string.IsNullOrWhiteSpace(prospectId))
                throw new ArgumentException("Prospect ID is required.", nameof(prospectId));

            var connection = _resolveConnection is null ? _connectionName : await _resolveConnection();
            if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("An EXPLORATION database binding is required.");

            var repo = new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                typeof(PROSPECT_VOLUME_ESTIMATE),
                connection,
                "PROSPECT_VOLUME_ESTIMATE",
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROSPECT_ID", Operator = "=", FilterValue = prospectId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            return (await repo.GetAsync(filters))
                .OfType<PROSPECT_VOLUME_ESTIMATE>()
                .OrderByDescending(estimate => estimate.ESTIMATE_DATE ?? DateTime.MinValue)
                .ToList();
        }
    }
}
