using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region PDEN_VOL_SUMMARY Table - Production Volumes Summary

        public async Task<decimal?> GetPDENCumulativeOilAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId)) return null;
            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "CUM_OIL", "CUMULATIVE_OIL", "CUM_OIL_PROD");
        }

        public async Task<decimal?> GetPDENCumulativeGasAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId)) return null;
            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "CUM_GAS", "CUMULATIVE_GAS", "CUM_GAS_PROD");
        }

        public async Task<decimal?> GetPDENCumulativeWaterAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId)) return null; // wait, pdenId is used here
            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "CUM_WATER", "CUMULATIVE_WATER", "CUM_WATER_PROD");
        }

        public async Task<decimal?> GetPDENOnProdDaysAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId)) return null;
            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "ON_PROD_DAYS", "PRODUCING_DAYS", "DAYS_ON");
        }

        #endregion
    }
}
