using System;

namespace Beep.OilandGas.OilProperties.Calculations
{
    /// <summary>
    /// Provides rigorous Black Oil property correlations.
    /// Methods: Bubble Point (Standing), Solution GOR (Standing), FVF (Standing), Viscosity (Vasquez-Beggs).
    /// </summary>
    public static class OilPropertyCalculator
    {
        // 1. Bubble Point Pressure (Standing 1947)
        // Pb = 18.2 * [ (Rs/Gg)^0.83 * 10^(0.00091*T - 0.0125*API) - 1.4 ]
        // T in Fahrenheit, Rs in scf/stb
        public static decimal CalculateBubblePointPressure_Standing(decimal rs, decimal gasGravity, decimal api, decimal tempF)
        {
            if (rs <= 0 || gasGravity <= 0) return 0;
            
            double Rs = (double)rs;
            double Gg = (double)gasGravity;
            double API = (double)api;
            double T = (double)tempF;
            
            double term1 = Math.Pow((Rs / Gg), 0.83);
            double term2 = Math.Pow(10, (0.00091 * T - 0.0125 * API));
            
            double pb = 18.2 * ((term1 * term2) - 1.4);
            
            return pb < 0 ? 0 : (decimal)pb;
        }

        // 2. Solution GOR (Standing Inverted)
        // Rs = Gg * [ ((P/18.2) + 1.4) * 10^(0.0125*API - 0.00091*T) ]^1.2048
        // P in psia
        public static decimal CalculateSolutionGOR_Standing(decimal p, decimal gasGravity, decimal api, decimal tempF)
        {
            if (p <= 0 || gasGravity <= 0) return 0;

            double P = (double)p;
            double Gg = (double)gasGravity;
            double API = (double)api;
            double T = (double)tempF;
            
            double term1 = (P / 18.2) + 1.4;
            double term2 = Math.Pow(10, (0.0125 * API - 0.00091 * T));
            
            double rs = Gg * Math.Pow((term1 * term2), 1.2048);
            
            return rs < 0 ? 0 : (decimal)rs;
        }

        // 3. Oil FVF (Standing)
        // Bo = 0.9759 + 0.00012 * [ Rs * (Gg/Go)^0.5 + 1.25*T ]^1.2
        // Go = 141.5 / (API + 131.5)
        public static decimal CalculateOilFVF_Standing(decimal rs, decimal gasGravity, decimal api, decimal tempF)
        {
             double Rs = (double)rs;
             double Gg = (double)gasGravity;
             double API = (double)api;
             double T = (double)tempF;
             
             double Go = 141.5 / (API + 131.5);
             
             double termInside = Rs * Math.Sqrt(Gg / Go) + 1.25 * T;
             double bo = 0.9759 + 0.00012 * Math.Pow(termInside, 1.2);
             
             return (decimal)bo;
        }

        // 4. Dead Oil Viscosity (Beggs-Robinson)
        // mu_od = 10^(x) - 1
        // x = 10^(3.0324 - 0.02023*API) * T^(-1.163)
        // T in Fahrenheit
        public static decimal CalculateDeadOilViscosity_BeggsRobinson(decimal api, decimal tempF)
        {
            double API = (double)api;
            double T = (double)tempF;
            
            double x = Math.Pow(10, (3.0324 - 0.02023 * API)) * Math.Pow(T, -1.163);
            double mu_od = Math.Pow(10, x) - 1.0;
            
            return (decimal)mu_od;
        }
        
        // 5. Saturated Oil Viscosity (Beggs-Robinson)
        // mu_o = A * mu_od^B
        // A = 10.715*(Rs+100)^-0.515
        // B = 5.44*(Rs+150)^-0.338
        public static decimal CalculateSaturatedViscosity_BeggsRobinson(decimal deadVisc, decimal rs)
        {
            double mu_od = (double)deadVisc;
            double Rs = (double)rs;
            
            double A = 10.715 * Math.Pow(Rs + 100, -0.515);
            double B = 5.44 * Math.Pow(Rs + 150, -0.338);
            
            double mu_o = A * Math.Pow(mu_od, B);
            
            return (decimal)mu_o;
        }
    }
}
