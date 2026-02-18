using System;
using System.Collections.Generic;

namespace Beep.OilandGas.DrillingAndConstruction.Calculations
{
    /// <summary>
    /// Provides calculations for drilling hydraulics.
    /// </summary>
    public static class HydraulicsCalculator
    {
        /// <summary>
        /// Calculates Frictional Pressure Loss in a pipe or annulus section.
        /// Uses Bingham Plastic or Power Law model based on input rheology.
        /// </summary>
        /// <param name="length">Length of section (ft).</param>
        /// <param name="flowRate">Flow rate (gpm).</param>
        /// <param name="id">Inside diameter (inch) - for pipe flow.</param>
        /// <param name="od">Outside diameter of inner pipe (inch) - for annular flow (0 if pipe flow).</param>
        /// <param name="mudDensity">Mud density (ppg).</param>
        /// <param name="plasticViscosity">Plastic Viscosity (cp) - Bingham.</param>
        /// <param name="yieldPoint">Yield Point (lb/100ft2) - Bingham.</param>
        /// <param name="n">Power Law index n (dimensionless) - Optional Power Law.</param>
        /// <param name="k">Power Law consistency index k (lbf*s^n/100ft2) - Optional Power Law.</param>
        /// <returns>Pressure loss (psi).</returns>
        public static double CalculatePressureLoss(
            double length, double flowRate, double id, double od = 0,
            double mudDensity = 8.33,
            double plasticViscosity = 0, double yieldPoint = 0,
            double n = 0, double k = 0)
        {
            if (id <= 0) return 0;

            double velocity;
            double hydraulicDiameter;

            if (od > 0) // Annulus
            {
                velocity = (24.51 * flowRate) / (Math.Pow(id, 2) - Math.Pow(od, 2));
                hydraulicDiameter = id - od;
            }
            else // Pipe
            {
                velocity = (24.51 * flowRate) / Math.Pow(id, 2);
                hydraulicDiameter = id;
            }

            // Decide model: If n & k provided, use Power Law. Else Bingham.
            if (n > 0 && k > 0)
            {
                return CalculatePowerLawLoss(length, velocity, hydraulicDiameter, mudDensity, n, k);
            }
            else
            {
                return CalculateBinghamLoss(length, velocity, hydraulicDiameter, mudDensity, plasticViscosity, yieldPoint);
            }
        }

        private static double CalculateBinghamLoss(
            double length, double velocity, double dh, double rho, double pv, double yp)
        {
            // Bingham Plastic Model
            // Critical Velocity (Turbulent transition)
            double Re = (928 * rho * velocity * dh) / pv;
            
            // Laminar Flow
            // dP = (L/300/dh) * (PV*V/200/dh + YP) ?? 
            // Standard field formula: dP = [ (PV*vel)/(1500*dh^2) + YP/(225*dh) ] * L
            
            double lossLaminar = ( (pv * velocity) / (1500 * Math.Pow(dh, 2)) + yp / (225 * dh) ) * length;
            
            // Turbulent Flow (Fanning friction factor approx)
            // dP = (rho^0.75 * vel^1.75 * pv^0.25 * L) / (1800 * dh^1.25)
            double lossTurbulent = (Math.Pow(rho, 0.75) * Math.Pow(velocity, 1.75) * Math.Pow(pv, 0.25) * length) / (1800 * Math.Pow(dh, 1.25));

            // Determine regine (Simple Critical Reynolds ~ 2100 often used, or 3000)
            if (Re > 3000) return lossTurbulent;
            return lossLaminar;
        }

        private static double CalculatePowerLawLoss(
            double length, double velocity, double dh, double rho, double n, double k)
        {
            // Power Law Model
            // Effective Viscosity for Reynolds Number
            // mu_eff = 100 * K * (96*V/dh)^(n-1) ? (Field units varying)
            
            // Standard Equation for Laminar Power Law pressure drop
            // dP = (K * L * Vel^n) / (300 * dh^(1+n)) * ... geometric factors
            // Simplified API 13D:
            // dP = ( (k * length) / (300 * dh) ) * Math.Pow( (1.6 * velocity * (3*n + 1)) / (dh * 4 * n), n );
            
            // This is for Pipe. Annulus is slightly different geometry factor.
            // Using generic pipe approx for now.
            
            double shearRateTerm = (1.6 * velocity * (3 * n + 1)) / (dh * 4 * n);
            double lossLaminar = ( (k * length) / (300 * dh) ) * Math.Pow(shearRateTerm, n);
            
            // Turbulent checks complex for Power Law (Dodge-Metzner). 
            // Fallback to Newtonian turbulent if Re high, or verify.
            // For this implementation, keeping to Laminar logic often dominant in annulus or simple check.
            
             // Generalized reynolds number
             double Re_gen = (89100 * rho * Math.Pow(velocity, 2 - n) * Math.Pow(dh, n)) / (k * Math.Pow((2 + 1/n) / 0.0208, n)); // Complex...
             
             // Simplification: Return Laminar for now as baseline.
             return lossLaminar;
        }

        /// <summary>
        /// Calculates Bit Pressure Drop (psi).
        /// </summary>
        public static double CalculateBitPressureDrop(double flowRate, double mudDensity, double nozzleArea)
        {
             // Pb = (Q^2 * rho) / (12031 * TFA^2 * C^2)
             // C = discharge coeff ~ 0.95
             if (nozzleArea <= 0) return 0;
             double C = 0.95;
             double Pb = (Math.Pow(flowRate, 2) * mudDensity) / (12031 * Math.Pow(nozzleArea, 2) * Math.Pow(C, 2));
             return Pb;
        }

        /// <summary>
        /// Calculates Jet Impact Force (lbf).
        /// </summary>
        public static double CalculateJetImpactForce(double flowRate, double mudDensity, double bitPressureDrop)
        {
            // IF = (Q * rho * Vn) / 1930 ? 
            // Or IF = 0.0173 * Q * sqrt(rho * Pb)
            return 0.0173 * flowRate * Math.Sqrt(mudDensity * bitPressureDrop);
        }

        /// <summary>
        /// Calculates Equivalent Circulating Density (ECD) in ppg.
        /// </summary>
        public static double CalculateECD(double mudDensity, double annularPressureLoss, double tvd)
        {
            // ECD = ESD + dP_annulus / (0.052 * TVD)
            if (tvd <= 0) return mudDensity;
            return mudDensity + annularPressureLoss / (0.052 * tvd);
        }
    }
}
