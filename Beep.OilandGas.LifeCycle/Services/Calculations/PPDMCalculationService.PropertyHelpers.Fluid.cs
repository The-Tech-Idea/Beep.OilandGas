using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region WELL_FLUID_SAMPLE / ANL_REPORT Table - PVT Data

        public async Task<decimal?> GetFluidWaterCutAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "WATER_CUT", "BSW", "BS_AND_W");
        }

        public async Task<decimal?> GetFluidOilViscosityAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "OIL_VISCOSITY", "VISCOSITY", "DEAD_OIL_VISCOSITY");
        }

        public async Task<decimal?> GetFluidBubblePointAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "BUBBLE_POINT", "BUBBLE_POINT_PRESSURE", "SATURATION_PRESSURE");
        }

        public async Task<decimal?> GetFluidFVFAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "FVF", "FORMATION_VOLUME_FACTOR", "OIL_FVF", "BO");
        }

        #endregion

        #region WELL_TREATMENT Table - Stimulation Data

        public async Task<decimal?> GetTreatmentProppantVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetPropertyValueMultiple(treatment, "PROPPANT_VOLUME", "SAND_VOLUME", "PROPPANT_MASS");
        }

        public async Task<decimal?> GetTreatmentFluidVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetPropertyValueMultiple(treatment, "FLUID_VOLUME", "TREATMENT_VOLUME", "TOTAL_FLUID");
        }

        public async Task<decimal?> GetTreatmentMaxPressureAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return null;
            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetPropertyValueMultiple(treatment, "MAX_PRESSURE", "TREATING_PRESSURE", "MAX_TREATING_PRESSURE");
        }

        public async Task<List<(DateTime Date, string Type, decimal? Volume)>> GetTreatmentHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId)) return new List<(DateTime, string, decimal?)>();
            var entities = await GetHistoryForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", startDate, endDate);
            var result = new List<(DateTime Date, string Type, decimal? Volume)>();
            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "TREATMENT_DATE");
                if (!date.HasValue) continue;
                var type = GetStringValue(entity, "TREATMENT_TYPE") ?? "UNKNOWN";
                var volume = GetPropertyValueMultiple(entity, "FLUID_VOLUME", "TREATMENT_VOLUME");
                result.Add((date.Value, type, volume));
            }
            return result.OrderBy(x => x.Date).ToList();
        }

        private string? GetStringValue(object entity, string v)
        {
            
            if (entity == null) return null;
            var dict = entity as IDictionary<string, object>;
            if (dict != null && dict.ContainsKey(v) && dict[v] != null)
            {
                return dict[v].ToString();
            }
            var prop = entity.GetType().GetProperty(v);
            if (prop != null)
            {
                var value = prop.GetValue(entity);
                return value?.ToString();
            }
            return null;
        }

        #endregion
    }
}
