using System;
using Beep.OilandGas.PlungerLift.Constants;

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
             if (inLiquid) return PlungerLiftConstants.FallVelocityLiquid;
             
             return PlungerLiftConstants.GetFallVelocityGas(plungerType);
        }

        // 3. Estimate Rise Velocity
        // Plunger rise velocity depends on differential pressure across the plunger.
        // V_rise ∝ sqrt(dP / rho_g) — from force balance: dP × A = drag + slug weight.
        // Typical range: 500–1000 ft/min (8–16 ft/s) at surface.
        // Formula: V_rise = sqrt(2 × dP × 144 / (rho_g × Cd)) with Cd ≈ 1.0
        // Simplified: V_rise (ft/min) = 60 × sqrt(2 × 144) × sqrt(dP/rho_g) ≈ 100 × sqrt(dP/rho_g)
        // where dP is in psi and rho_g in lb/ft³.
        public static decimal EstimateRiseVelocity(decimal avgDifferentialPressure)
        {
            if (avgDifferentialPressure <= 0)
                return PlungerLiftConstants.RiseVelocityDefault;

            // Assume average gas density ~ 2 lb/ft³ (typical wellbore gas)
            const decimal rhoGas = 2.0m;
            double dP = (double)avgDifferentialPressure;
            double vFtPerMin = 100.0 * Math.Sqrt(dP / 2.0);
            double vFtPerSec = vFtPerMin / 60.0;

            // Clamp to realistic range: 5–20 ft/s (300–1200 ft/min)
            return (decimal)Math.Max(5.0, Math.Min(20.0, vFtPerSec));
        }

        // 4. Gas Required Per Cycle
        // V_gas (scf) = tubing_volume × (P_avg / P_std) × (T_std / T_avg)
        // where P_avg ≈ casing_pressure, T_avg ≈ 520 °R, P_std = 14.7 psia
        // Tubing volume = π/4 × D² × depth / 144 (ft³)
        // Plus 10% for gas slippage past plunger.
        // Falls back to rule-of-thumb 400 scf/bbl/1000 ft when pressure unknown.
        public static decimal EstimateGasRequired(decimal depth, decimal liquidLoadBbl, decimal pressure)
        {
            // Physical calculation: gas needed = tubing volume at average pressure
            // Tubing ID typically 2.441 inches (2-3/8" tubing), cross-section = π/4 × D²
            const decimal tubingIdInches = 2.441m;
            const decimal pStd = 14.7m;   // psia
            const decimal tStd = 520m;     // °R (60 °F)
            const decimal tAvg = 560m;     // °R (100 °F — typical wellbore average)
            const decimal slipFactor = 1.10m; // +10% for gas slippage past plunger

            decimal tubingAreaFt2 = (decimal)(Math.PI / 4.0) * tubingIdInches * tubingIdInches / 144m;
            decimal tubingVolumeFt3 = tubingAreaFt2 * depth;
            decimal pAvg = pressure > 0 ? (pressure + 14.7m) / 2m : 100m; // average casing + atmosphere
            decimal gasAtPressure = tubingVolumeFt3 * (pAvg / pStd) * (tStd / tAvg);
            decimal gasRequired = gasAtPressure * slipFactor;

            // Fallback to rule-of-thumb if pressure is unknown or result seems unreasonable
            if (pressure <= 0 || gasRequired < 100)
            {
                decimal glr = 400m * (depth / 1000m);
                return glr * liquidLoadBbl;
            }

            return gasRequired;
        }
    }
}
