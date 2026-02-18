using System;

namespace Beep.OilandGas.GasLift.Calculations
{
    /// <summary>
    /// Provides rigorous gas lift arithmetic.
    /// Methods: Thornhill-Craver (Valve Performance).
    /// </summary>
    public static class GasLiftCalculator
    {
        // Thornhill-Craver Equation constants
        private const double Gravity = 32.174;
        private const double C_d = 0.865; // Discharge coefficient standard

        /// <summary>
        /// Calculates Gas Passage flow rate (Q) through an orifice using Thornhill-Craver.
        /// Returns rate in Mscf/day.
        /// </summary>
        public static decimal CalculateThornhillCraverThroughput(
            decimal upstreamPressurePsia,
            decimal downstreamPressurePsia,
            decimal portSizeInches,
            decimal temperatureRankine,
            decimal gasSpecificGravity,
            decimal specificHeatRatio) // k
        {
            if (upstreamPressurePsia <= 0 || portSizeInches <= 0 || temperatureRankine <= 0) return 0;

            double P1 = (double)upstreamPressurePsia;
            double P2 = (double)downstreamPressurePsia;
            double d = (double)portSizeInches;
            double T = (double)temperatureRankine;
            double G = (double)gasSpecificGravity;
            double k = (double)specificHeatRatio;

            // Area in sq. inches
            double A = (Math.PI / 4.0) * Math.Pow(d, 2);

            // Critical Pressure Ratio
            double rc = Math.Pow(2.0 / (k + 1.0), k / (k - 1.0));
            double ratio = P2 / P1;

            double flowFactor;
            
            if (ratio < rc) // Critical Flow (Choked)
            {
                // Use ratio = rc
                 double term = Math.Pow(rc, 2.0/k) - Math.Pow(rc, (k+1.0)/k);
                 flowFactor = Math.Sqrt((2.0 * Gravity * k) / (k - 1.0) * term);
            }
            else // Subcritical Flow
            {
                 double term = Math.Pow(ratio, 2.0/k) - Math.Pow(ratio, (k+1.0)/k);
                 flowFactor = Math.Sqrt((2.0 * Gravity * k) / (k - 1.0) * term);
            }

            // Standard formulation:
            // Q (Mscf/d) = 155.5 * Cd * A * P1 * Func(R) / Sqrt(G*T) ??
            // Let's use the explicit textbook form:
            // Q = 155.5 * Cd * A * P1 * FlowFactor / Sqrt(G * T) ... factor 155.5 combines units
            
            // F_g = sqrt( 1/GT )
            double Fg = Math.Sqrt(1.0 / (G * T));
            
            double Q = 155.5 * C_d * A * P1 * flowFactor * Fg;
            
            return (decimal)Q; 
        }

        // Temperature at depth assuming linear gradient
        public static decimal CaclulateTempAtDepth(decimal surfaceTemp, decimal bottomHoleTemp, decimal totalDepth, decimal targetDepth)
        {
            if (totalDepth <= 0) return surfaceTemp;
            decimal gradient = (bottomHoleTemp - surfaceTemp) / totalDepth;
            return surfaceTemp + (gradient * targetDepth);
        }
        
        // Surface Injection Pressure required to open valve at depth
        // P_surf = P_open_depth * e^(-S) ? No, pure hydrostatic gas column
        // P_d = P_s * e^(GMH / ZRT)
        // So P_s = P_d / e^X
        public static decimal CalculateRequiredSurfacePressure(decimal valvePressure, decimal depth, decimal gasGravity, decimal avgTemp, decimal avgZ)
        {
             // P_surf = P_valve / exp(0.01875 * G * H / (Z * T_avg))
             if (avgTemp <= 0 || avgZ <= 0) return 0;
             
             double exponent = (0.01875 * (double)gasGravity * (double)depth) / ((double)avgZ * (double)avgTemp);
             return valvePressure / (decimal)Math.Exp(exponent);
        }
    }
}
