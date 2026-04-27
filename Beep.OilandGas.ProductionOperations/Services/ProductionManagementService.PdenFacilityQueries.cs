using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionOperations.Services
{
    /// <summary>
    /// PDEN queries scoped to facility production reporting (PPDM <c>PDEN_SUBTYPE = FACILITY</c>).
    /// </summary>
    public partial class ProductionManagementService
    {
        public async Task<IReadOnlyList<PDEN>> ListFacilityPdenDeclarationsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pdenUow = GetPDENUnitOfWork();
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" },
                new AppFilter { FieldName = "PDEN_SUBTYPE", FilterValue = PdenSubtypeFacility, Operator = "=" }
            };

            object units = await pdenUow.Get(filters).ConfigureAwait(false);
            List<PDEN> list = ConvertToList<PDEN>(units);

            if (startDate.HasValue)
                list = list.Where(p => p.CURRENT_STATUS_DATE >= startDate.Value).ToList();
            if (endDate.HasValue)
                list = list.Where(p => p.CURRENT_STATUS_DATE <= endDate.Value).ToList();

            return list;
        }
    }
}
