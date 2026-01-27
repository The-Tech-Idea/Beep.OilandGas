using System;
using System.Threading.Tasks;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region WELL_TEST_ANALYSIS Table - Known PPDM 3.9 Fields

        public async Task<decimal?> GetWellTestPermeabilityAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            object? analysis = !string.IsNullOrEmpty(testId) 
                ? await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM")
                : await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(analysis, "PERMEABILITY", "CALCULATED_PERMEABILITY");
        }

        public async Task<decimal?> GetWellTestSkinAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            object? analysis = !string.IsNullOrEmpty(testId) 
                ? await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM")
                : await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(analysis, "SKIN", "SKIN_FACTOR");
        }

        public async Task<decimal?> GetWellTestProductivityIndexAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            object? analysis = !string.IsNullOrEmpty(testId) 
                ? await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM")
                : await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(analysis, "PI", "PRODUCTIVITY_INDEX");
        }

        #endregion
    }
}
