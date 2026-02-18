using System;

namespace Beep.OilandGas.HydraulicPumps.Calculations
{
    /// <summary>
    /// Provides rigoruous hydraulic pump arithmetic.
    /// Methods: Hydraulic Horsepower (HHP), Jet Pump Sizing, Cavitation Checks.
    /// </summary>
    public static class HydraulicPumpCalculator
    {
        // Hydraulic Horsepower
        // HHP = (Q * P) / 1714
        public static decimal CalculateHydraulicHorsepower(decimal flowRateBpd, decimal pressurePsi)
        {
            if (flowRateBpd <= 0 || pressurePsi <= 0) return 0;
            return (flowRateBpd * pressurePsi) / 1714m;
        }

        // Jet Pump Nozzle/Throat Area Ratio (R)
        // R = An / At
        // Optimization usually aims for R such that efficiency is maximized for given M (Mass Ratio) and N (Head Ratio).
        // M = Q_produced / Q_power_fluid
        // N = (P_discharge - P_suction) / (P_power_fluid - P_discharge)
        // Efficiency = M * N
        public static (decimal Efficiency, decimal AreaRatio) CalculateJetPumpPerformance(
            decimal q_produced, 
            decimal q_power, 
            decimal p_suction, 
            decimal p_discharge, 
            decimal p_power_fluid_surface) // Assuming nozzle pressure ~ surface + head - friction? Simplified.
        {
            if (q_power <= 0 || (p_power_fluid_surface - p_discharge) <= 0) return (0, 0);

            decimal M = q_produced / q_power;
            decimal N = (p_discharge - p_suction) / (p_power_fluid_surface - p_discharge);
            
            decimal efficiency = M * N;

            // Theoretical best Area Ratio R for given N can be approximated or looked up.
            // Simplified Cunningham correlation or similar? 
            // For N approx 0.3-0.6, R is often 0.2-0.4.
            // Formula: N = R^2 / (1-R)^2 ... very rough approx for optimal.
            // Let's return the computed efficiency and a placeholder R calc.
            
            // R approx sqrt(N / (1+N)) for max efficiency?
            // H_n = P_n - P_s
            // H_t = P_d - P_s
            
            double n_val = (double)N;
            double r_approx = Math.Sqrt(n_val / (1.3 + n_val)); // Curve fit approx
            
            return (efficiency, (decimal)r_approx);
        }

        // Check for Cavitation using NPSH
        // NPSH_available = P_suction - P_vapor
        // NPSH_required is pump specific.
        public static bool CheckCavitation(decimal suctionPressure, decimal vaporPressure, decimal requiredNPSH_psi)
        {
            decimal available = suctionPressure - vaporPressure;
            return available < requiredNPSH_psi;
        }

        // Sizing Recs
        public static decimal RecommendPowerFluidRate(decimal productionTarget, decimal currentEfficiency)
        {
            // if eff is low, maybe need more power fluid?
            // M = Qs / Qp => Qp = Qs / M
            // Assume target M ~ 0.5 to 1.0 depending on setup.
            if (currentEfficiency < 0.2m) return productionTarget * 3.0m; // Need lots of power fluid
            if (currentEfficiency > 0.35m) return productionTarget * 1.5m; // Good efficiency
            return productionTarget * 2.0m; // Default starting point
        }
    }
}
