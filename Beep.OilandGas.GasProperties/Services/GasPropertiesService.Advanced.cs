using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.GasProperties;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.GasProperties.Services
{
    public partial class GasPropertiesService
    {
        public async Task<List<GAS_PROPERTIES>> CalculateRealGasPropertiesAsync(
            decimal specificGravity,
            decimal variablePressureStart,
            decimal variablePressureEnd,
            decimal step,
            decimal constantTemperature)
        {
            if (specificGravity <= 0) throw new ArgumentException(nameof(specificGravity));
            
            var results = new List<GAS_PROPERTIES>();
            
            // 1. Calc Pseudocriticals
            var (tpc, ppc) = GasPropertyCalculator.CalculatePseudocriticalProperties(specificGravity);
            
            for (decimal p = variablePressureStart; p <= variablePressureEnd; p += step)
            {
                // 2. Calc Z
                var z = GasPropertyCalculator.CalculateZFactor(p, constantTemperature, tpc, ppc);
                
                // 3. Calc Density
                var rho = GasPropertyCalculator.CalculateDensity(p, constantTemperature, z, specificGravity);
                
                // 4. Calc Viscosity
                // Need rho in g/cc for Lee-Gonzalez?
                // rho lb/ft3 * 0.0160185 = g/cc
                decimal rho_g_cc = rho * 0.0160185m;
                var visc = GasPropertyCalculator.CalculateGasViscosity(
                    constantTemperature, 
                    z, 
                    rho_g_cc, 
                    specificGravity * 28.96m);
                
                results.Add(new GAS_PROPERTIES
                {
                     GAS_PROPERTIES_ID = Guid.NewGuid().ToString(),
                     PRESSURE = p,
                     TEMPERATURE = constantTemperature,
                     Z_FACTOR = z,
                     DENSITY = rho,
                     VISCOSITY = visc,
                     SPECIFIC_GRAVITY = specificGravity,
                     CRITICAL_PRESSURE = ppc,
                     CRITICAL_TEMPERATURE = tpc,
                     GAS_FVF = (0.02827m * z * constantTemperature) / p // Bg (rb/scf) approx
                });
            }
            
            return await Task.FromResult(results);
        }
    }
}
