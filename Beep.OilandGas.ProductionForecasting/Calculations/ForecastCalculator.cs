using System;

namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Provides Arps Decline Curve Analysis calculations.
    /// Methods: Rate and Cumulative equations for Exponential, Hyperbolic, and Harmonic declines.
    /// </summary>
    public static class ForecastCalculator
    {
        // 1. Arps Rate Equation
        // q(t) = qi / (1 + b * Di * t)^(1/b)
        // b = 0 -> Exponential: q(t) = qi * exp(-Di * t)
        // b = 1 -> Harmonic: q(t) = qi / (1 + Di * t)
        public static decimal CalculateFlowRate_Arps(decimal qi, decimal di, decimal b, decimal t)
        {
            if (qi <= 0) return 0;
            
            double Qi = (double)qi;
            double Di = (double)di;
            double B = (double)b;
            double Time = (double)t;
            
            double q_t = 0;

            if (Math.Abs(B) < 0.001) // Exponential (b ~ 0)
            {
                q_t = Qi * Math.Exp(-Di * Time);
            }
            else if (Math.Abs(B - 1.0) < 0.001) // Harmonic (b ~ 1)
            {
                q_t = Qi / (1.0 + Di * Time);
            }
            else // Hyperbolic
            {
                q_t = Qi / Math.Pow((1.0 + B * Di * Time), (1.0 / B));
            }

            return (decimal)q_t;
        }

        // 2. Arps Cumulative Equation
        // Exponential: Np = (qi - qt) / Di
        // Harmonic: Np = (qi / Di) * ln(qi / qt)
        // Hyperbolic: Np = (qi^b / ((1-b)*Di)) * (qi^(1-b) - qt^(1-b))
        public static decimal CalculateCumulative_Arps(decimal qi, decimal di, decimal b, decimal qt)
        {
             if (qi <= 0 || di <= 0) return 0;
             
             double Qi = (double)qi;
             double Di = (double)di;
             double B = (double)b;
             double Qt = (double)qt;
             
             double Np = 0;

             if (Math.Abs(B) < 0.001) // Exponential
             {
                 Np = (Qi - Qt) / Di;
             }
             else if (Math.Abs(B - 1.0) < 0.001) // Harmonic
             {
                 if (Qt <= 0) Qt = 0.0001; // Avoid divide by zero/log(0)
                 Np = (Qi / Di) * Math.Log(Qi / Qt);
             }
             else // Hyperbolic
             {
                 if (Qt < 0) Qt = 0;
                 double term1 = Math.Pow(Qi, B) / ((1.0 - B) * Di);
                 double term2 = Math.Pow(Qi, 1.0 - B) - Math.Pow(Qt, 1.0 - B);
                 Np = term1 * term2;
             }
             
             return (decimal)Np;
        }

        // Helper: Effective to Nominal Decline
        // Di = -ln(1 - De)  for Exponential
        public static decimal ConvertEffectiveToNominal(decimal effectiveDecline, string period = "Yearly")
        {
            // De is usually %/year or %/month
            // Nominal Di is instantaneous rate 1/time
            
            double De = (double)effectiveDecline;
            if (De >= 1.0) De = 0.9999;
            
            double Di = -Math.Log(1.0 - De);
            return (decimal)Di;
        }
    }
}
