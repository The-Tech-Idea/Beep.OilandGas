using System;
using System.Threading.Tasks;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        #region POOL Table - Known PPDM 3.9 Fields

        public async Task<decimal> GetPoolInitialPressureAsync(string poolId, decimal defaultValue = 3000m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "INITIAL_RESERVOIR_PRESSURE", "ORIG_RESERVOIR_PRES", "INITIAL_PRESSURE") ?? defaultValue;
        }

        public async Task<decimal> GetPoolPorosityAsync(string poolId, decimal defaultValue = 0.2m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "AVG_POROSITY", "POROSITY") ?? defaultValue;
        }

        public async Task<decimal> GetPoolPermeabilityAsync(string poolId, decimal defaultValue = 100m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "AVG_PERMEABILITY", "PERMEABILITY") ?? defaultValue;
        }

        public async Task<decimal> GetPoolThicknessAsync(string poolId, decimal defaultValue = 50m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "AVG_THICKNESS", "NET_PAY_THICKNESS", "THICKNESS", "NET_PAY") ?? defaultValue;
        }

        public async Task<decimal> GetPoolTemperatureAsync(string poolId, decimal defaultValue = 560m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "RESERVOIR_TEMPERATURE", "AVG_RESERVOIR_TEMP", "TEMPERATURE") ?? defaultValue;
        }

        public async Task<decimal> GetPoolCompressibilityAsync(string poolId, decimal defaultValue = 0.00001m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "TOTAL_COMPRESSIBILITY", "COMPRESSIBILITY") ?? defaultValue;
        }

        public async Task<decimal?> GetPoolBubblePointPressureAsync(string poolId)
        {
            if (string.IsNullOrEmpty(poolId)) return null;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "BUBBLE_POINT_PRESSURE", "BUBBLE_POINT");
        }

        public async Task<decimal> GetPoolOilViscosityAsync(string poolId, decimal defaultValue = 1.0m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "OIL_VISVISCOSITY", "VISCOSITY") ?? defaultValue;
        }

        public async Task<decimal> GetPoolGasViscosityAsync(string poolId, decimal defaultValue = 0.02m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "GAS_VISCOSITY") ?? defaultValue;
        }

        public async Task<decimal> GetPoolFormationVolumeFactorAsync(string poolId, decimal defaultValue = 1.2m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "FORMATION_VOLUME_FACTOR", "OIL_FVF", "FVF") ?? defaultValue;
        }

        public async Task<decimal> GetPoolGasGravityAsync(string poolId, decimal defaultValue = 0.65m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "GAS_GRAVITY", "GAS_SPECIFIC_GRAVITY") ?? defaultValue;
        }

        public async Task<decimal> GetPoolDrainageAreaAsync(string poolId, decimal defaultValue = 640m)
        {
            if (string.IsNullOrEmpty(poolId)) return defaultValue;
            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "DRAINAGE_AREA", "AREA") ?? defaultValue;
        }

        public async Task<decimal> GetPoolDrainageRadiusAsync(string poolId, decimal defaultValue = 1000m)
        {
            var area = await GetPoolDrainageAreaAsync(poolId, 0m);
            if (area <= 0) return defaultValue;
            var areaFt2 = area * 43560m;
            return (decimal)Math.Sqrt((double)(areaFt2 / (decimal)Math.PI));
        }

        #endregion
    }
}
