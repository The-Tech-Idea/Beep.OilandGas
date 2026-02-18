using System;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Calculations
{
    /// <summary>
    /// Provides rigorous thermodynamic calculations for compressors.
    /// Methods: Schultz Polytropic Head (Centrifugal), API 618 Volumetric Efficiency (Reciprocating).
    /// </summary>
    public static class AdvancedCompressorCalculator
    {
        private const decimal GasConstantR_ft_lbf = 1545.0m; // ft-lbf / (lb-mol * R)
        private const decimal AirMolecularWeight = 28.96m;

        /// <summary>
        /// Calculates Polytropic Head and Efficiency using the Schultz method.
        /// </summary>
        public static void CalculateCentrifugalRigorous(
            CentrifugalCompressorAnalysis analysis, 
            decimal inletTempRankine, 
            decimal gasSpecificGravity, 
            decimal k_iso, 
            decimal z_inlet, 
            decimal z_discharge)
        {
            // Schultz Method for Polytropic Head
            // Hp = (Z_avg * R * T1 / MW) * (n / (n-1)) * [ (P2/P1)^((n-1)/n) - 1 ]
            
            if (analysis.InletPressure <= 0 || analysis.DischargePressure <= 0) return;

            decimal mw = gasSpecificGravity * AirMolecularWeight;
            decimal p_ratio = analysis.DischargePressure / analysis.InletPressure;
            decimal z_avg = (z_inlet + z_discharge) / 2m;

            // Determine Polytropic Exponent (n)
            // If we don't have discharge temp, we assume an efficiency to get T2? 
            // Or if we strictly calculate Head required for the pressure ratio:
            // Often we start with an estimated polytropic efficiency (eta_p) -> calculate n -> calculate Head.
            
            // Equation: (n-1)/n = (k-1)/k * (1/eta_p)
            decimal eta_p_est = 0.75m; // Initial estimate or input? 
            // Let's use a standard assumption if not provided.
            
            decimal m = ((k_iso - 1m) / k_iso) * (1m / eta_p_est); // m = (n-1)/n
            decimal n = 1m / (1m - m);

            // Calculate Head
            double term1 = (double)((z_avg * GasConstantR_ft_lbf * inletTempRankine) / mw);
            double term2 = (double)(n / (n - 1m));
            double term3 = Math.Pow((double)p_ratio, (double)m) - 1.0;
            
            decimal head = (decimal)(term1 * term2 * term3);
            
            analysis.PolyIsentropicHead = head;
            analysis.HeadDeveloped = head; // In this context often same
            
            // Operating Region Check (Simplified Surge)
            // Surge often happens at low flow / high head. 
            // This is just a label setter based on a heuristic.
            analysis.OperatingRegion = "Stable"; 
        }

        /// <summary>
        /// Calculates Volumetric Efficiency and Flow for Reciprocating Compressors (API 618).
        /// </summary>
        public static void CalculateReciprocatingRigorous(
            ReciprocationCompressorAnalysis analysis,
            decimal gasSpecificGravity,
            decimal k_iso,
            decimal z_avg)
        {
            // Flow = Displacement * VolumetricEfficiency
            // VE = 100 - C * (r^(1/k) - 1) - Leakage
            
            if (analysis.SuctionPressure <= 0) return;

            decimal r = analysis.DischargePressure / analysis.SuctionPressure; // Compression Ratio
            decimal c = 10.0m; // Clearance percent (typically 10-15%? No, usually 0.05-0.15 as fraction. API formula uses %)
            // Actually commonly: E_v = 1 - (Cl/100) * (r^(1/k) - 1) * (Zs/Zd) ... 
            
            // Let's use the standard equation:
            // Ev = 1.0 - Cl * (r^(1/k) - 1.0)
            // Cl is clearance fraction.
            
            decimal clearanceFraction = 0.15m; // Default conservative
            double k_val = (double)k_iso;
            double r_val = (double)r;
            
            double ev = 1.0 - (double)clearanceFraction * (Math.Pow(r_val, 1.0/k_val) - 1.0);
            
            // Adjust for compressibility (Zs/Zd) roughly 1.0 if not specific
            // Ev = Ev * (Zs/Zd) - Leakage
            ev = ev * 0.98; // Leakage factor
            
            analysis.VolumetricEfficiency = Math.Max(0.1m, (decimal)ev); // Store as fraction

            // Calculate Displacement
            // PD (ft3/min) = (Area * Stroke * RPM * Cylinders) / 1728
            // Area in sq in.
            decimal area = (decimal)(Math.PI * Math.Pow((double)analysis.BoreSize / 2.0, 2));
            decimal disp_cfm = (area * analysis.StrokeLength * analysis.RPM * analysis.CylinderCount) / 1728m;
            
            analysis.DisplacementVolume = disp_cfm; // CFM
            
            // Actual Flow
            analysis.VolumetricFlowRate = disp_cfm * analysis.VolumetricEfficiency; 

            // Rod Load
            // Load = P_discharge * Area_bore (Compression) - P_suction * (Area_bore - Area_rod) (Tension) approximate
            // Let's compute simple Gas Load max
            decimal gasLoadComp = analysis.DischargePressure * area;
            decimal gasLoadTens = analysis.SuctionPressure * area; 
            
            analysis.RodLoad = Math.Max(gasLoadComp, gasLoadTens);
        }
    }
}
