using System;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Provides rigorous Pipeline Flow calculations.
    /// Methods: Panhandle B (Gas), Darcy-Weisbach (Liquid), Friction Factor (Colebrook-White).
    /// </summary>
    public static class PipelineCalculator
    {
        // 1. Panhandle B (Gas Transmission - High Pressure)
        // Q = 737 * E * ( (P1^2 - P2^2) / L )^0.51 * D^2.53
        // Q in scfd, P in psia, L in miles, D in inches.
        // E is efficiency (0.92 usually)
        public static decimal CalculateGasFlow_PanhandleB(decimal p1, decimal p2, decimal lengthMiles, decimal diameterInches, decimal efficiency = 0.92m)
        {
            if (lengthMiles <= 0 || diameterInches <= 0) return 0;
            if (p1 <= p2) return 0;

            double P1 = (double)p1;
            double P2 = (double)p2;
            double L = (double)lengthMiles;
            double D = (double)diameterInches;
            double E = (double)efficiency;
            
            double termPressure = (Math.Pow(P1, 2) - Math.Pow(P2, 2)) / L;
            double flow = 737.0 * E * Math.Pow(termPressure, 0.51) * Math.Pow(D, 2.53);
            
            return (decimal)flow;
        }

        // 2. Swamee-Jain Friction Factor (Explicit approx of Colebrook-White)
        // f = 0.25 / [ log10( (e/3.7D) + (5.74 / Re^0.9) ) ]^2
        public static decimal CalculateFrictionFactor(decimal reynolds, decimal roughness, decimal diameter)
        {
             if (reynolds < 2000) return 64.0m / reynolds; // Laminar
             if (reynolds <= 0) return 0.02m;
             
             double Re = (double)reynolds;
             double e = (double)roughness;
             double D = (double)diameter;
             
             if (D <= 0) return 0.02m;

             double term = (e / (3.7 * D)) + (5.74 / Math.Pow(Re, 0.9));
             double f = 0.25 / Math.Pow(Math.Log10(term), 2);
             
             return (decimal)f;
        }

        // 3. Darcy-Weisbach Pressure Drop (Liquid)
        // dP = f * (L/D) * (rho * v^2 / 2)
        // Practical Oilfield Units: dP (psi) = 0.0000115 * f * (L_ft / D_in) * rho_ppg * Q_bpd^2 / D_in^4 ??
        // Let's use standard: dP = f * (L/D) * (rho V^2 / 2g) then convert.
        // Or simpler: dP (psi) = (f * L_ft * density_lb_ft3 * V_ft_s^2) / (144 * 2 * 32.2 * D_ft)
        
        // Let's calculate Velocity first:
        // V (ft/s) = (0.0119 * Q_bpd) / D_in^2
        public static decimal CalculateLiquidVelocity(decimal flowBpd, decimal diameterInches)
        {
            if (diameterInches <= 0) return 0;
            return (0.0119m * flowBpd) / (diameterInches * diameterInches);
        }

        public static decimal CalculateReynoldsNumber(decimal density_lb_ft3, decimal velocity_ft_s, decimal diameter_inches, decimal viscosity_cp)
        {
             // Re = 928 * (rho_lb_ft3 * V_ft_s * D_in) / mu_cp
             if (viscosity_cp <= 0) return 0;
             return (928m * density_lb_ft3 * velocity_ft_s * diameter_inches) / viscosity_cp;
        }

        public static decimal CalculatePressureDrop_Darcy(decimal f, decimal length_ft, decimal density_lb_ft3, decimal velocity_ft_s, decimal diameter_inches)
        {
            // dP (psi) = 0.001294 * f * L * rho * V^2 / D
            // L in ft, rho in lb/ft3, V in ft/s, D in inches
            if (diameter_inches <= 0) return 0;
            double term = (double)(f * length_ft * density_lb_ft3 * velocity_ft_s * velocity_ft_s) / (double)diameter_inches;
            return (decimal)(0.001294 * term);
        }
    }
}
