using System;

namespace Beep.OilandGas.Properties.Calculations
{
    /// <summary>
    /// Provides Oil PVT property calculations.
    /// Correlations: Standing, Vasquez-Beggs, Beggs-Robinson.
    /// </summary>
    public static class OilPropertyCalculator
    {
        #region Standing Correlation (Bubble Point, GOR, FVF)

        public static decimal CalculateBubblePoint_Standing(decimal rs, decimal gasGravity, decimal api, decimal tempF)
        {
            // Pb = 18.2 * [ (Rs/Gg)^0.83 * 10^(0.00091*T - 0.0125*API) - 1.4 ]
            double Rs = (double)rs;
            double Gg = (double)gasGravity;
            double API = (double)api;
            double T = (double)tempF;

            double A = Math.Pow(Rs / Gg, 0.83);
            double B = Math.Pow(10, 0.00091 * T - 0.0125 * API);
            double Pb = 18.2 * (A * B - 1.4);

            return Pb < 0 ? 0 : (decimal)Pb;
        }

        public static decimal CalculateSolutionGOR_Standing(decimal p, decimal gasGravity, decimal api, decimal tempF)
        {
            // Rs = Gg * [ (P/18.2 + 1.4) * 10^(0.0125*API - 0.00091*T) ]^(1/0.83)
            double P = (double)p;
            double Gg = (double)gasGravity;
            double API = (double)api;
            double T = (double)tempF;

            double term1 = P / 18.2 + 1.4;
            double term2 = Math.Pow(10, 0.0125 * API - 0.00091 * T);
            double Rs = Gg * Math.Pow(term1 * term2, 1.2048); // 1/0.83 = 1.2048

            return Rs < 0 ? 0 : (decimal)Rs;
        }

        public static decimal CalculateOilFVF_Standing(decimal rs, decimal gasGravity, decimal api, decimal tempF)
        {
            // Bo = 0.9759 + 0.000120 * [ Rs * (Gg/Go)^0.5 + 1.25*T ]^1.2
            // Note: Standing usually uses (Gg/Go)^0.5 term
            // Specific Gravity of Oil (Go) = 141.5 / (131.5 + API)
            
            double Rs = (double)rs;
            double Gg = (double)gasGravity;
            double API = (double)api;
            double T = (double)tempF;
            double Go = 141.5 / (131.5 + API);

            double term = Rs * Math.Sqrt(Gg / Go) + 1.25 * T;
            double Bo = 0.9759 + 0.00012 * Math.Pow(term, 1.2);

            return Bo < 1 ? 1 : (decimal)Bo;
        }

        #endregion

        #region Beggs-Robinson (Viscosity)

        public static decimal CalculateDeadOilViscosity_BeggsRobinson(decimal api, decimal tempF)
        {
            // mu_od = 10^(x) - 1
            // x = y * T^-1.163
            // y = 10^(3.0324 - 0.02023*API)
            
            double API = (double)api;
            double T = (double)tempF;

            double y = Math.Pow(10, 3.0324 - 0.02023 * API);
            double x = y * Math.Pow(T, -1.163);
            double visc = Math.Pow(10, x) - 1.0;

            return (decimal)visc;
        }

        public static decimal CalculateSaturatedViscosity_BeggsRobinson(decimal deadVisc, decimal rs)
        {
            // mu_o = A * mu_od^B
            // A = 10.715 * (Rs + 100)^-0.515
            // B = 5.44 * (Rs + 150)^-0.338
            
            double mu_od = (double)deadVisc;
            double Rs = (double)rs;

            double A = 10.715 * Math.Pow(Rs + 100, -0.515);
            double B = 5.44 * Math.Pow(Rs + 150, -0.338);
            double mu_o = A * Math.Pow(mu_od, B);

            return (decimal)mu_o;
        }
        
        #endregion

        #region Vasquez-Beggs (Compressibility)
        
        public static decimal CalculateCompressibility_VasquezBeggs(decimal p, decimal rs, decimal api, decimal tempF, decimal gasGravity)
        {
            // co = ( -1433 + 5 * Rs + 17.2 * T - 1180 * Gg + 12.61 * API ) / ( P * 10^5 )
            // Valid for P > Pb
            
            double P = (double)p;
            if (P <= 0) return 0;

            double Rs = (double)rs;
            double T = (double)tempF;
            double Gg = (double)gasGravity;
            double API = (double)api;

            double num = -1433 + 5 * Rs + 17.2 * T - 1180 * Gg + 12.61 * API;
            double co = num / (P * 100000.0);

            return co < 0 ? 0 : (decimal)co;
        }

        #endregion
        
        #region Density
        public static decimal CalculateDensity(decimal pressure, decimal oilFVF, decimal rs, decimal api, decimal gasGravity)
        {
             // rho_o = (62.4 * Go + 0.0136 * Rs * Gg) / Bo
             double Go = 141.5 / (131.5 + (double)api);
             double Rs = (double)rs;
             double Gg = (double)gasGravity;
             double Bo = (double)oilFVF;
             
             if (Bo == 0) return 0;

             double rho = (62.4 * Go + 0.0136 * Rs * Gg) / Bo;
             return (decimal)rho;
        }
        #endregion
    }
}
