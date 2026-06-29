using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.PlungerLift.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PlungerLift.Services
{
    public partial class PlungerLiftService
    {
        public async Task<PLUNGER_LIFT_CYCLE_RESULT> AnalyzePlungerCycleAsync(PlungerLiftAnalysisRequest request)
        {
             if (request == null) throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Analyzing Plunger Lift Cycle for Well {WellId}", request.WellId);

            var result = new PLUNGER_LIFT_CYCLE_RESULT
            {
                 PLUNGER_LIFT_CYCLE_RESULT_ID = Guid.NewGuid().ToString(),
                 PLUNGER_LIFT_WELL_PROPERTIES_ID = request.EquipmentId,
                 // CYCLE_DATE = DateTime.UtcNow // If property exists
            };

            // 1. Critical Velocity
            // Need densities. calculate or assume?
            decimal rho_g = 2.5m; // approx assumption if not calc'd
            decimal rho_L = 62.4m; // Water
            if (request.OilGravity != null) rho_L = 50.0m; // Oil approx

            decimal vc = PlungerLiftCalculator.CalculateCriticalVelocity_Turner(rho_g, rho_L, 60m); // sigma water=60
            
            // 2. Velocities — using actual differential pressure for rise velocity
            decimal avgDifferentialPressure = request.CasingPressure > 0 && request.TubingPressure > 0
                ? request.CasingPressure - request.TubingPressure
                : 50m; // default if pressures not provided
            result.FALL_VELOCITY = PlungerLiftCalculator.EstimateFallVelocity("BAR", false);
            result.RISE_VELOCITY = PlungerLiftCalculator.EstimateRiseVelocity(avgDifferentialPressure);

            // 3. Cycle Times
            decimal depth = request.WellDepth ?? 5000m;
            result.FALL_TIME = (depth / result.FALL_VELOCITY) / 60m; // Minutes
            result.RISE_TIME = (depth / result.RISE_VELOCITY) / 60m; // Minutes

            // Shut-in time: pressure build-up for next cycle
            // Lower reservoir pressure → longer build-up needed
            result.SHUT_IN_TIME = request.ReservoirPressure > 0
                ? Math.Max(10m, Math.Min(45m, 500m / request.ReservoirPressure * 20m))
                : 30m;

            result.CYCLE_TIME = result.RISE_TIME + result.FALL_TIME + result.SHUT_IN_TIME;
            
            // 4. Production
            // Assume slug size based on production rate per cycle
            decimal dailyRate = request.LiquidProductionRate ?? 10m;
            decimal cyclesPerDay = 1440m / result.CYCLE_TIME;
            
            result.CYCLES_PER_DAY = cyclesPerDay;
            result.PRODUCTION_PER_CYCLE = dailyRate / cyclesPerDay;
            result.LIQUID_SLUG_SIZE = result.PRODUCTION_PER_CYCLE;
            result.DAILY_PRODUCTION_RATE = dailyRate;

            return await Task.FromResult(result);
        }
    }
}
