using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.HydraulicPumps.Services
{
    public partial class HydraulicPumpService
    {
        public async Task<HydraulicPumpAnalysisResult> AnalyzeJetPumpPerformanceAsync(
            HydraulicPumpAnalysisResult input, 
            decimal powerFluidSurfacePressure, 
            decimal vaporPressure)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _logger?.LogInformation("Analyzing Jet Pump Performance for CalculationId {Id}", input.CalculationId);

            // 1. Calc Efficiency & Area Ratio
            var (eff, r) = HydraulicPumpCalculator.CalculateJetPumpPerformance(
                input.ProductionRate, 
                input.PowerFluidRate, 
                input.SuctionPressure, 
                input.DischargePressure, 
                powerFluidSurfacePressure);

            input.OverallEfficiency = eff;
            input.RecommendedNozzleSize = 0; // Map R to nozzle OD in a future sizing pass; 0 = not sized here.

            // 2. Hydraulic HP on produced lift (Q x delta-P), screening formula consistent with calculators.
            decimal liftPsi = Math.Max(0m, input.DischargePressure - input.SuctionPressure);
            input.PowerRequired = HydraulicPumpCalculator.CalculateHydraulicHorsepower(input.ProductionRate, liftPsi);

            // 3. Cavitation Check
            // Assume some standard NPSH required if not known, e.g., 50 psi
            bool cavitationRisk = HydraulicPumpCalculator.CheckCavitation(input.SuctionPressure, vaporPressure, 50m);
            if (cavitationRisk)
            {
               input.Status = "WARNING_CAVITATION";
               input.ErrorMessage = "Low suction pressure indicates potential cavitation.";
            }
            else
            {
                input.Status = "SUCCESS";
            }
            
            return await Task.FromResult(input);
        }

        public async Task<HydraulicPumpDesign> DesignHydraulicSystemAsync(string wellId, decimal targetRate, decimal pumpDepth, decimal operatingPressure)
        {
             var design = new HydraulicPumpDesign
             {
                 DesignId = Guid.NewGuid().ToString(),
                 WellUWI = wellId,
                 DesignDate = DateTime.UtcNow,
                 PumpType = "JET", // Defaulting to Jet for this logic
                 PumpDepth = pumpDepth,
                 OperatingPressure = operatingPressure,
                 FlowRate = targetRate // Target production (bbl/day); RecommendPowerFluidRate available for sizing UI.
             };

             // 2. Rec Efficiency
             design.Efficiency = 0.30m; // Target for Jet Pump

             // 3. Size Pump (Throat/Nozzle)
             // ... Simple sizing logic could go here or in Calculator
             // design.PumpSize = ...

             design.Status = "DRAFT";
             
             return await Task.FromResult(design);
        }
    }
}
