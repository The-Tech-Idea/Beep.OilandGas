using System;

namespace Beep.OilandGas.GasProperties.Calculations
{
    /// <summary>
    /// Provides standard PVT correlations for Natural Gas.
    /// Methods: Hall-Yarborough (Z), Lee-Gonzalez (Visc), Sutton (Ppc/Tpc).
    /// </summary>
    public static class GasPropertyCalculator
    {
        // Sutton's Correlations for Pseudocritical Properties
        public static (decimal Tpc, decimal Ppc) CalculatePseudocriticalProperties(decimal specificGravity)
        {
            if (specificGravity <= 0) return (0, 0);

            double sg = (double)specificGravity;

            // Sutton (1985)
            double Tpc = 169.2 + 349.5 * sg - 74.0 * sg * sg;
            double Ppc = 756.8 - 131.0 * sg - 3.6 * sg * sg;

            return ((decimal)Tpc, (decimal)Ppc);
        }

        // Hall-Yarborough (1973) for Z-Factor
        // Accurate for Ppr < 20-30
        public static decimal CalculateZFactor(decimal pressure, decimal temperature, decimal Tpc, decimal Ppc)
        {
            if (Tpc <= 0 || Ppc <= 0 || temperature <= 0) return 1.0m;

            double Tr = (double)(temperature / Tpc);
            double Pr = (double)(pressure / Ppc);
            double t = 1.0 / Tr;

            // Constants
            double A = 0.06125 * t * Math.Exp(-1.2 * Math.Pow(1.0 - t, 2));
            double B = t * (14.76 - 9.76 * t + 4.58 * Math.Pow(t, 2));
            double C = t * (90.7 - 242.2 * t + 42.4 * Math.Pow(t, 2));
            double D = 2.18 + 2.82 * t;

            // Solve for Reduced Density (Y) using Newton-Raphson
            // f(Y) = ... = 0
            double Y = 0.001; // Initial guess
            for (int i = 0; i < 50; i++)
            {
                double f = -A * Pr
                           + (Y + Math.Pow(Y, 2) + Math.Pow(Y, 3) - Math.Pow(Y, 4)) / Math.Pow(1.0 - Y, 3)
                           - B * Math.Pow(Y, 2)
                           + C * Math.Pow(Y, D);

                double df = (1.0 + 4.0 * Y + 4.0 * Math.Pow(Y, 2) - 4.0 * Math.Pow(Y, 3) + Math.Pow(Y, 4)) / Math.Pow(1.0 - Y, 4)
                            - 2.0 * B * Y
                            + C * D * Math.Pow(Y, D - 1.0);

                if (Math.Abs(df) < 1e-10) break;
                double newY = Y - f / df;
                if (Math.Abs(newY - Y) < 1e-6)
                {
                    Y = newY;
                    break;
                }
                Y = newY;
            }

            // Z = A * Pr / Y
            if (Y <= 0) return 1.0m; 
            double z = A * Pr / Y;
            return (decimal)z;
        }

        // Lee-Gonzalez-Eakin (1966) for Gas Viscosity
        public static decimal CalculateGasViscosity(decimal temperature, decimal zFactor, decimal density_g_cc, decimal molecularWeight)
        {
            if (temperature <= 0 || density_g_cc <= 0) return 0;
            
            double T = (double)temperature; // Rankine
            double M = (double)molecularWeight;
            double rho = (double)density_g_cc;

            // Constants conversion if needed? Standard uses T in Rankine and Rho in g/cc
            
            double X = 3.5 + 986.0 / T + 0.01 * M;
            double Y = 2.4 - 0.2 * X;
            double K = (9.4 + 0.02 * M) * Math.Pow(T, 1.5) / (209.0 + 19.0 * M + T);

            double visc = K * 1e-4 * Math.Exp(X * Math.Pow(rho, Y));
            return (decimal)visc;
        }

        public static decimal CalculateDensity(decimal pressure, decimal temperature, decimal zFactor, decimal specificGravity)
        {
             // rho = P * MW / (Z * R * T)
             // R = 10.732 psia ft3 / (lb-mol R)
             if (zFactor <= 0 || temperature <= 0) return 0;
             
             double P = (double)pressure;
             double T = (double)temperature;
             double Z = (double)zFactor;
             double MW = (double)specificGravity * 28.96;
             
             double rho = (P * MW) / (Z * 10.732 * T); // lb/ft3
             return (decimal)rho;
        }
    }
}
