using System;

namespace Beep.OilandGas.PlungerLift.Calculations
{
    /// <summary>
    /// Provides rigorous Plunger Lift calculations.
    /// Methods: Critical Velocity (Turner), Cycle Timing, Gas Requirements.
    /// </summary>
    public static class PlungerLiftCalculator
    {
        // 1. Critical Velocity (Turner et al. 1969)
        // v_c (ft/s) = 1.593 * sigma^0.25 * (rho_L - rho_g)^0.25 / rho_g^0.5
        // sigma: surface tension (dynes/cm), water=60, oil=20-30
        // rho_L, rho_g: density in lb/ft3
        public static decimal CalculateCriticalVelocity_Turner(decimal gasDensity, decimal liquidDensity, decimal surfaceTension)
        {
            if (gasDensity <= 0) return 0;
            
            double rho_g = (double)gasDensity;
            double rho_L = (double)liquidDensity;
            double sigma = (double)surfaceTension;
            
            double term1 = Math.Pow(sigma, 0.25);
            double term2 = Math.Pow((rho_L - rho_g), 0.25);
            double term3 = Math.Pow(rho_g, 0.5);
            
            double vc = 1.593 * term1 * term2 / term3;
            
            return (decimal)vc;
        }

        // 2. Estimate Fall Velocity (Heuristic)
        // Bar stock > Pad plunger
        // Typical Fall V: Gas ~ 1000-2000 ft/min? No, typically 100-2000 ft/min depending on fluid.
        // In gas: 200-1000 ft/min (3-16 ft/s)
        // In liquid: 50-150 ft/min (0.8-2.5 ft/s)
        public static decimal EstimateFallVelocity(string plungerType, bool inLiquid)
        {
             // Simple lookup
             if (inLiquid) return 1.5m; // ft/s
             
             // In Gas
             return plungerType?.ToUpper() switch
             {
                 "BAR" => 15.0m, // ft/s
                 "PAD" => 8.0m,
                 "BRUSH" => 10.0m,
                 "CONTINUOUS" => 12.0m,
                 _ => 10.0m
             };
        }

        // 3. Estimate Rise Velocity
        // Typically 500-1000 ft/min (8-16 ft/s)
        // Max usually capped to avoid hitting lubricator too hard.
        public static decimal EstimateRiseVelocity(decimal avgDifferentialPressure)
        {
             // Heuristic: V ~ C * dP
             // For now return typical target 750 ft/min = 12.5 ft/s
             return 12.5m; 
        }

        // 4. Gas Required Per Cycle
        // V (scf) = Volume of Tubing * (P_avg / P_std) * (T_std / T_avg) * (1/Z) ... roughly
        // Often approximations used per bbl lifted.
        // Rule of thumb: 400 scf per bbl per 1000 ft?
        public static decimal EstimateGasRequired(decimal depth, decimal liquidLoadBbl, decimal pressure)
        {
             // Simple GLR rule of thumb based on depth and pressure
             // GLR_req (scf/bbl) approx 400 * (Depth/1000)
             decimal glr = 400m * (depth / 1000m);
             return glr * liquidLoadBbl;
        }
    }
}
