using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.GasLift.Services
{
    public partial class GasLiftService
    {
        public async Task<GasLiftValveData> CalculateValvePerformanceAsync(
            GasLiftValveData valve, 
            decimal upstreamPressure, 
            decimal downstreamPressure, 
            decimal gasSpecificGravity)
        {
            if (valve == null) throw new ArgumentNullException(nameof(valve));
            
            _logger?.LogInformation("Calculating Valve Performance for Depth {Depth}", valve.Depth);

            decimal ratio = 1.28m; // k
            // Update Temp check
            if (valve.Temperature <= 0) valve.Temperature = 520; // Default 60F
            
            var rate = GasLiftCalculator.CalculateThornhillCraverThroughput(
                upstreamPressure, 
                downstreamPressure, 
                valve.PortSize, 
                valve.Temperature, 
                gasSpecificGravity, 
                ratio);
                
            // Store result? GasLiftValveData usually holds "Design" rates or "Test" rates.
            // GasInjectionRate is a good candidate.
            valve.GasInjectionRate = rate;
            
            return await Task.FromResult(valve);
        }

        public async Task<List<GasLiftValveData>> VerifyValveStringAsync(
            List<GasLiftValveData> valves, 
            decimal surfaceInjPressure, 
            decimal surfaceTemp, 
            decimal bottomHoleTemp, 
            decimal totalDepth, 
            decimal gasGravity)
        {
             // Verify throughput of all valves for a given surface injection pressure
             // Need to calc pressure at depth for each valve
             
             foreach(var valve in valves)
             {
                 // 1. Temp at depth
                 valve.Temperature = GasLiftCalculator.CaclulateTempAtDepth(surfaceTemp, bottomHoleTemp, totalDepth, valve.Depth);
                 
                 // 2. Pressure at depth (Gas column)
                 // P_d = P_s * exp( ... )
                 double exponent = (0.01875 * (double)gasGravity * (double)valve.Depth) / (0.95 * (double)valve.Temperature); // Z=0.95 approx
                 decimal p_inj_depth = surfaceInjPressure * (decimal)Math.Exp(exponent);
                 
                 // 3. Throughput assuming it's passing gas (if open)
                 // Check if open: P_inj_depth > P_close ? Or P_tubing considerations.
                 // For now, calculating max throughput capacity if fully open.
                 
                 decimal p_tubing = 500; // Placeholder or passed in? 
                 // We will skip p_tubing and assume critical flow vs 0 or provide P_tubing via other method.
                 // Assuming critical for capacity check.
                 
                 valve.GasInjectionRate = GasLiftCalculator.CalculateThornhillCraverThroughput(
                     p_inj_depth, 
                     p_tubing, 
                     valve.PortSize, 
                     valve.Temperature, 
                     gasGravity, 
                     1.28m);
             }
             
             return await Task.FromResult(valves);
        }
    }
}
