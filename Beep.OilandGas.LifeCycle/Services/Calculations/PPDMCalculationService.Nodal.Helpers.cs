using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        /// <summary>
        /// Gets well ID from UWI
        /// </summary>
        private async Task<string?> GetWellIdByUwiAsync(string uwi)
        {
            if (string.IsNullOrEmpty(uwi)) return null;

             var metadata = await _metadata.GetTableMetadataAsync("WELL");
             var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(WELL);
             var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL");

             // Assuming UWI is stored in UWI column. 
             // If WellId is different from UWI, we need to query.
             var filter = new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = uwi };
             var res = await repo.GetAsync(new List<AppFilter> { filter });
             var well = res?.FirstOrDefault() as WELL;
             return well?.UWI; // In PPDM39 UWI is valid ID.
        }

        private async Task<decimal?> GetWellTotalDepthAsync(string wellId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(WELL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL");
                var filter = new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId };
                var result = await repo.GetAsync(new List<AppFilter> { filter });
                var well = result?.FirstOrDefault() as WELL;
                if (well == null) return null;
                // Prefer FINAL_TD (total depth at end of drilling), fall back to DRILL_TD
                if (well.FINAL_TD > 0) return well.FINAL_TD;
                if (well.DRILL_TD > 0) return well.DRILL_TD;
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<decimal?> GetWellTestProductivityIndexAsync(string wellId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_TEST_FLOW");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(WELL_TEST_FLOW);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL_TEST_FLOW");
                var filter = new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId };
                var result = await repo.GetAsync(new List<AppFilter> { filter });
                var testFlow = result?.OfType<WELL_TEST_FLOW>().OrderByDescending(f => f.ROW_CHANGED_DATE).FirstOrDefault();
                if (testFlow == null) return null;
                // PI = flow rate / pressure drawdown; use MAX_SURFACE_PRESSURE as proxy drawdown
                if (testFlow.MAX_FLUID_RATE > 0 && testFlow.MAX_SURFACE_PRESSURE > 0)
                    return testFlow.MAX_FLUID_RATE / testFlow.MAX_SURFACE_PRESSURE;
                // Minimal proxy: return null so caller falls back to default
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<decimal?> GetWellTestStaticPressureAsync(string wellId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_TEST_PRESSURE");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(WELL_TEST_PRESSURE);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL_TEST_PRESSURE");
                var filter = new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId };
                var result = await repo.GetAsync(new List<AppFilter> { filter });
                var testPressure = result?.OfType<WELL_TEST_PRESSURE>().OrderByDescending(p => p.ROW_CHANGED_DATE).FirstOrDefault();
                // START_PRESSURE is the static reservoir pressure at test start
                return testPressure?.START_PRESSURE > 0 ? testPressure.START_PRESSURE : null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<decimal?> GetTubularOuterDiameterAsync(string wellId, string type)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_TUBULAR");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(WELL_TUBULAR);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL_TUBULAR");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };
                if (!string.IsNullOrEmpty(type))
                    filters.Add(new AppFilter { FieldName = "TUBING_TYPE", Operator = "=", FilterValue = type });

                var results = await repo.GetAsync(filters);
                var tubular = results?.OfType<WELL_TUBULAR>()
                    .OrderByDescending(t => t.OUTSIDE_DIAMETER)
                    .FirstOrDefault();
                return tubular?.OUTSIDE_DIAMETER > 0 ? tubular.OUTSIDE_DIAMETER : 2.875m;
            }
            catch
            {
                return 2.875m; // Default tubing OD fallback
            }
        }

        private async Task<decimal?> GetTubularDepthAsync(string wellId, string type)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_TUBULAR");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ?? typeof(WELL_TUBULAR);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "WELL_TUBULAR");
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };
                if (!string.IsNullOrEmpty(type))
                    filters.Add(new AppFilter { FieldName = "TUBING_TYPE", Operator = "=", FilterValue = type });

                var results = await repo.GetAsync(filters);
                var tubular = results?.OfType<WELL_TUBULAR>()
                    .OrderByDescending(t => t.BASE_DEPTH)
                    .FirstOrDefault();
                return tubular?.BASE_DEPTH > 0 ? tubular.BASE_DEPTH : 8000m;
            }
            catch
            {
                return 8000m; // Default tubing depth fallback
            }
        }
    }
}
