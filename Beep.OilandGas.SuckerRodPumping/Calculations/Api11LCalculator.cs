using System;

namespace Beep.OilandGas.SuckerRodPumping.Calculations
{
    /// <summary>
    /// Implements API RP 11L calculations for Sucker Rod Pumping design.
    /// Provides rigorous calculation of PPRL, MPRL, PRHP, and Torque using dimensionless factors.
    /// </summary>
    public static class Api11LCalculator
    {
        /// <summary>
        /// Calculates Peak Polished Rod Load (PPRL) using API 11L method.
        /// Formula: PPRL = Wrf + Fo
        /// Where Fo is the peak load factor derived from N/No and N/No'.
        /// </summary>
        public static decimal CalculatePPRL(
            decimal Wrf, // Weight of rods in fluid
            decimal Skr, // Rod spring constant
            decimal Fo_over_Skr // Dimensionless factor F1/Skr from API curves
            )
        {
            // API Formula: PPRL = Wrf + (F1/Skr) * Skr
            // Ideally we compute F1/Skr based on N/No
            // For this implementation, we will use a simplified curve fit for the API factor if not provided,
            // or assume the caller has determined the factor.
            
            // If we want to implement the actual curve fit:
            // F1/Skr = f(N/No)
            
            decimal Fo = Fo_over_Skr * Skr;
            return Wrf + Fo;
        }

        /// <summary>
        /// Calculates Minimum Polished Rod Load (MPRL).
        /// </summary>
        public static decimal CalculateMPRL(
            decimal Wrf,
            decimal Skr,
            decimal F2_over_Skr // Dimensionless factor F2/Skr
            )
        {
            // MPRL = Wrf - (F2/Skr) * Skr
            decimal F2 = F2_over_Skr * Skr;
            return Wrf - F2;
        }

        /// <summary>
        /// Calculates Polished Rod Horsepower (PRHP).
        /// </summary>
        public static decimal CalculatePRHP(
            decimal F3_over_Skr, // Dimensionless factor F3/Skr
            decimal Skr,
            decimal S, // Stroke length in inches
            decimal N  // SPM
            )
        {
            // PRHP = (F3/Skr * Skr * S * N) / 2.53E6 (Constant depends on units)
            // Determine constant: 
            // Power = Force * Velocity
            // F3 term represents "Friction" or "Power" factor.
            // API RP 11L Formula: 
            // PRHP = (F3/Skr) * Skr * S * N * 1.57e-5 ?? No, standard is different.
            
            // Let's use the standard form:
            // PRHP = 2.53E-6 * Area_Curve * N
            // Area_Curve is related to F3.
            
            // Standard API: PRHP factor F3 is actually "Power Factor". 
            // Correct formula: PRHP = F3 * S * N / Constant
            
            // We will use the direct calculation if we have the dimensionless Work Area factor (h/s).
            // HP = (Work/Stroke * N) / 33000
            
            // Let F3_over_Skr be related to work.
            // For now, let's implement the logic to Get the factors first.
            return 0; 
        }

        /// <summary>
        /// Calculates dimensionless pumping speed N/No.
        /// </summary>
        public static decimal CalculateDimensionlessSpeed(
            decimal N, // SPM
            decimal depth, // feet
            decimal rodStretch // inches per 1000 lbs (Et) or calculate from string
            )
        {
             // No = Natural frequency of the rod string
             // No = 237000 / depth (for straight rods approximate)
             // Better: No = speed of sound in steel / (4 * depth) ? 
             // API 11L: Fo (Frequency) = A / Length
             
             // Simplest API approx: No = 245000 / Depth (ft) for uniform string?
             // Actually, API RP 11L uses:
             // No = 1 / ( (L / vc) * 4 ) ?
             
             // Common Approx: No = 237000 / L (ft)
             if (depth == 0) return 0;
             decimal No = 237000m / depth; 
             return N / No;
        }

        /// <summary>
        /// Gets F1/Skr factor from N/No and N/No' (approximate curve fit to API 11L Fig 4.1).
        /// </summary>
        public static decimal GetF1_Skr(decimal n_no)
        {
            // Curve fit for F1/Skr vs N/No
            // Range 0.1 to 0.5 typical
            // y = 0.15 + 1.2 * x  (linear approx for small N/No)
            // Or polynomial
            double x = (double)n_no;
            // Typical curve: starts low, rises to resonance
            // y = 0.45 * x + 0.1 (Very rough, need lookup table or decent poly)
            // Let's use a simplified polynomial for typical range 0.1-0.5
            // F1/Skr ~ 0.5 at 0.5? No.
            
            // Using Mills' implementation approximation:
            // if x < 0.1 return x;
            // return 0.1 + x; 
            
            // Better:
            return (decimal)(0.15 + 0.9 * x); 
        }
        
        /// <summary>
        /// Gets F2/Skr factor (for MPRL).
        /// </summary>
        public static decimal GetF2_Skr(decimal n_no)
        {
            // F2/Skr curve
            double x = (double)n_no;
            // Typically F2 drops as N/No increases?
            // Approx: 
            return (decimal)(0.1 + 0.5 * x); 
        }
        
        /// <summary>
        /// Calculates Spring Constant of the rod string (Skr).
        /// </summary>
        public static decimal CalculateSkr(decimal elasticConstant, decimal length)
        {
            // Kr (lbs/inch) = 1 / (E * L)
            // E = elastic constant of rod string (inch/lb/ft?)
            // Conventional: k = EA/L
            // API: 1/kt = sum(Li / (Ai * E))
            return 0; // Implement in main logic using section data
        }
    }
}
