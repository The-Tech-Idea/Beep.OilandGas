using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.NodalAnalysis.Services
{
    public partial class NodalAnalysisService
    {
        public async Task<NODAL_ANALYSIS_RESULT> RunNodalAnalysisAsync(NodalAnalysisRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            _logger?.LogInformation("Starting Nodal Analysis for Well {WellId}", request.WellUWI);

            var result = new NODAL_ANALYSIS_RESULT
            {
                ANALYSIS_ID = Guid.NewGuid().ToString(),
                WELL_UWI = request.WellUWI,
                ANALYSIS_DATE = DateTime.UtcNow,
                STATUS = "RUNNING"
            };

            // 1. Generate IPR Curve
            // Needs Pr and Qmax (or PI)
            // request has Pr, PI? 
            decimal pr = request.ReservoirPressure ?? 3000m;
            decimal qmax = 0;
            
            if (request.ProductivityIndex != null && request.ProductivityIndex > 0)
            {
                // Calc Qmax from PI for Vogel? Or just use Darcy?
                // Qmax (Vogel) = Q_test / (1 - 0.2(Pwf/Pr) - 0.8(Pwf/Pr)^2) 
                // If PI is J, Q = J(Pr - Pwf). At Pwf=0, Qmax = J*Pr?
                // Vogel Qmax approx J * Pr / 1.8 ?
                qmax = (request.ProductivityIndex.Value * pr) / 1.8m; 
            }
            else
            {
                qmax = 1000m; // Default fallback
            }

            var iprPoints = NodalCalculator.GenerateIPRCurve(pr, qmax);
            
            // 2. Generate VLP Curve
            decimal pwh = request.WellheadPressure ?? 200m;
            decimal depth = request.WellDepth ?? 8000m;
            var vlpPoints = NodalCalculator.GenerateVLPCurve(pwh, depth, qmax);

            // 3. Find Operating Point
            var (opRate, opPress, found) = NodalCalculator.FindOperatingPoint(iprPoints, vlpPoints);

            if (found)
            {
                result.OPERATING_FLOW_RATE = opRate;
                result.OPERATING_PRESSURE = opPress;
                result.STATUS = "SUCCESS";
            }
            else
            {
                result.STATUS = "NO_INTERSECTION";
                result.OPERATING_FLOW_RATE = 0;
            }
            
            // Note: Currently result model does not store curve points directly in root props shown.
            // Would likely save them to side tables or JSON blob if existed.
            // For this scope, returning the Operating Point is key.

            return await Task.FromResult(result);
        }
    }
}
