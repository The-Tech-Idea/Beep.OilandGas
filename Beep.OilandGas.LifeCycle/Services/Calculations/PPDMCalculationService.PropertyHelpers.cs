using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region RESERVES Table - Reservoir Data

        public async Task<decimal?> GetProvedOilReservesAsync(string entityId, string entityType = "FIELD")
        {
            if (string.IsNullOrEmpty(entityId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = $"{entityType}_ID", Operator = "=", FilterValue = entityId },
                new AppFilter { FieldName = "RESERVE_CLASS", Operator = "=", FilterValue = "PROVED" }
            };

            var entities = await GetEntitiesAsync("RESENT", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var reserve = entities.FirstOrDefault();

            return GetPropertyValueMultiple(reserve, "OIL_VOLUME", "REMAINING_OIL", "OIL_RESERVES");
        }

        public async Task<decimal?> GetProvedGasReservesAsync(string entityId, string entityType = "FIELD")
        {
            if (string.IsNullOrEmpty(entityId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = $"{entityType}_ID", Operator = "=", FilterValue = entityId },
                new AppFilter { FieldName = "RESERVE_CLASS", Operator = "=", FilterValue = "PROVED" }
            };

            var entities = await GetEntitiesAsync("RESENT", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var reserve = entities.FirstOrDefault();

            return GetPropertyValueMultiple(reserve, "GAS_VOLUME", "REMAINING_GAS", "GAS_RESERVES");
        }

        #endregion

        #region WELL_FLUID_SAMPLE - PVT Data

        public async Task<decimal?> GetFluidAPIGravityAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "API_GRAVITY", "OIL_GRAVITY", "API");
        }

        public async Task<decimal?> GetFluidGORAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "GOR", "GAS_OIL_RATIO", "SOLUTION_GOR");
        }

        #endregion

        #region WELL_TREATMENT - Stimulation Data

        public async Task<string?> GetTreatmentTypeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetStringValue(treatment, "TREATMENT_TYPE");
        }

        #endregion

        #region WELL_STRAT_UNIT_INTPR - Stratigraphy Data

        public async Task<decimal?> GetFormationTopDepthAsync(string wellId, string stratUnitId)
        {
            if (string.IsNullOrEmpty(wellId) || string.IsNullOrEmpty(stratUnitId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                new AppFilter { FieldName = "STRAT_UNIT_ID", Operator = "=", FilterValue = stratUnitId }
            };

            var entities = await GetEntitiesAsync("WELL_STRAT_UNIT_INTPR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var interp = entities.FirstOrDefault();

            return GetPropertyValueMultiple(interp, "TOP_DEPTH", "FORMATION_TOP", "TOP_MD");
        }

        #endregion

        #region WELL_STATUS - Status Data

        public async Task<string?> GetWellStatusAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var status = await GetLatestEntityForWellAsync("WELL_STATUS", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetStringValue(status, "STATUS_TYPE");
        }

        #endregion
    }
}
