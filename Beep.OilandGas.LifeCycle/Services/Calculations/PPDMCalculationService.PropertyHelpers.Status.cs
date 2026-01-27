using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region WELL_STATUS Table - Well Status Data

        public async Task<List<(DateTime Date, string Status)>> GetWellStatusHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return new List<(DateTime, string)>();
            var entities = await GetHistoryForWellAsync("WELL_STATUS", wellId, "EFFECTIVE_DATE", startDate, endDate);
            var result = new List<(DateTime Date, string Status)>();
            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "EFFECTIVE_DATE");
                var status = GetStringValue(entity, "STATUS_TYPE");
                if (date.HasValue && !string.IsNullOrEmpty(status)) result.Add((date.Value, status));
            }
            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region Custom Field Mapping Support

      

        #endregion
    }
}
